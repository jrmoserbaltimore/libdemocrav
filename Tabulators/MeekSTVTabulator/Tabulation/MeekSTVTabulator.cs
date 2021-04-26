// Meek STV proportional election implementation.
//
// Special thanks goes to the work of Brian Wichmann and David Hill:
//
//   Tie Breaking in STV, Voting Matters Issue 19
//     http://www.votingmatters.org.uk/ISSUE19/I19P1.PDF
//   Implementing STV by Meek's Method, Voting Matters Issue 22
//     http://www.votingmatters.org.uk/ISSUE22/I22P2.pdf
//   Validation of Implementation of the Meek Algorithm for STV
//     http://www.votingmatters.org.uk/RES/MKVAL.pdf
//   Single Transferable Vote by Meek's Method
//     http://www.dia.govt.nz/diawebsite.NSF/Files/meekm/%24file/meekm.pdf
//   The Meek STV reference rule
//     https://prfound.org/resources/reference/reference-meek-rule/
//
// The decimal type avoids rounding error for up to 27 digits.
// For 9 billion votes, precision up to 17 decimal places is
// possible. Beyond ten is not generally necessary: OpaVote
// uses 6, and the reference algorithm uses 9.

using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Utility;

namespace MoonsetTechnologies.Voting.Tabulation
{
    /// <inheritdoc/>
    [Export(typeof(AbstractTabulator))]
    [ExportMetadata("Algorithm", "alternative-vote")]
    [ExportMetadata("Factory", typeof(MeekSTVTabulatorFactory))]
    [ExportMetadata("Title", "Minimax")]
    [ExportMetadata("Description", "Meek's method of Single Transferable Vote.  " +
                    "Configurable to use Borda eliminations (Meek-Geller).")]
    [ExportMetadata("Settings", new[]
    {
        typeof(TiebreakerTabulatorSetting)
    })]
    public class MeekSTVTabulator : AbstractSingleTransferableVoteTabulator
    {
        private decimal quota = 0.0m;
        private decimal surplus = 0.0m;
        private readonly int precision = 9;
        private bool kfStasis = false;
        private bool surplusStasis = false;
        // Round up to precision
        private decimal RoundUp(decimal d)
        {
            // The reference rule says to round UP, so we need
            // to Round(weight + 10^(-precision) / 2).
            decimal r = 0.5m * Convert.ToDecimal(Math.Pow(10.0D,
                Convert.ToDouble(0 - precision)));
            return decimal.Round(d + r, precision);
        }

        public MeekSTVTabulator(TabulationMediator mediator,
            AbstractTiebreakerFactory tiebreakerFactory,
            IEnumerable<ITabulatorSetting> tabulatorSettings)
            : base(mediator, tiebreakerFactory, tabulatorSettings)
        {

        }

        // Reference rule A:  Initialize candidate states
        protected override void InitializeTabulation(BallotSet ballots, IEnumerable<Candidate> withdrawn, int seats)
        {
            base.InitializeTabulation(ballots, withdrawn, seats);
        }

        // Finds the candidates with the lowest vote count
        protected override IEnumerable<Candidate> GetEliminationCandidates()
        {
            decimal minVotes = (from x in candidateStates select x.Value.VoteCount).Min();
            return from x in candidateStates
                   where x.Value.VoteCount == minVotes
                   select x.Key;
        }

        /// <inheritdoc/>
        protected override TabulationStateEventArgs TabulateRound()
        {
            IEnumerable<Candidate> startSet = 
                from x in candidateStates
                where new[] { CandidateState.States.hopeful, CandidateState.States.elected }
                     .Contains(x.Value.State)
                select x.Key;

            IEnumerable<Candidate> winners = null;
            IEnumerable<Candidate> eliminationCandidates;
            // Mutually exclusive, and exclusive with electing winners
            string note = kfStasis ? "KeepFactor Stasis, so eliminating candidates." : null;
            note = surplusStasis ? "Surplus Stasis, so eliminating candidates." : note;

            // B.1:  if we have fewer hopefuls than open seats, elect everyone
            if (IsFinalRound())
            {
                int count = (from x in candidateStates
                    where x.Value.State == CandidateState.States.elected
                    select x).Count();
                if (count < seats)
                {
                    winners = from x in candidateStates
                              where x.Value.State == CandidateState.States.hopeful
                              select x.Key;
                    note = "Fewer hopeful candidates than open seats, so elected all.";
                }
            }
            else
            {
                // Meek's Method iterates until it elects winners or hits a no-winner state.
                // On a winner state, it repeats the iteration
                winners = GetNewWinners(quota);
            }

            // Elect our winners either way
            foreach (Candidate c in winners)
                SetState(c, CandidateState.States.elected);

            // A round ends after an election or an elimination, which updates the tiebreaker
            if (winners.Count() == 0)
            {
                if (IsFinalRound())
                {
                    eliminationCandidates = from x in candidateStates
                        where x.Value.State == CandidateState.States.hopeful
                        select x.Key;
                    note = "Filled seats, so eliminated all remainig hopefuls.";
                }
                else
                {
                    eliminationCandidates = GetEliminationCandidates();
                }
                if (!(eliminationCandidates?.Count() > 0))
                    throw new InvalidOperationException("Called TabulateRound() but no winners or losers.");
                foreach (Candidate c in eliminationCandidates)
                    SetState(c, CandidateState.States.defeated);
            }

            return new MeekSTVTabulationStateEventArgs
            {
                CandidateStates = CandidateStatesCopy,
               // SchwartzSet = (analytics as RankedTabulationAnalytics).GetSchwartzSet(startSet),
               // SmithSet = (analytics as RankedTabulationAnalytics).GetSchwartzSet(startSet),
                Quota = quota,
                Surplus = surplus
            };
        }

        /// <inheritdoc/>
        protected override void SetState(Candidate candidate, CandidateState.States state)
        {
            if (!candidateStates.ContainsKey(candidate))
                candidateStates[candidate] = new MeekCandidateState();
            base.SetState(candidate, state);

            // Set KeepFactor to 0 for losers as per B.3
            if (new[] { CandidateState.States.defeated, CandidateState.States.withdrawn }
                      .Contains(candidateStates[candidate].State))
                (candidateStates[candidate] as MeekCandidateState).KeepFactor = 0.0m;
        }

        /// <inheritdoc/>
        protected override void CountBallot(CountedBallot ballot)
        {
            decimal weight = 1.0m;
            List<Vote> votes = ballot.Votes.ToList();
            // Ensure any newly-seen candidates are counted
            InitializeCandidateStates(from x in ballot.Votes
                                      where !candidateStates.Keys.Contains(x.Candidate)
                                      select x.Candidate);

            // Used for Borda score.  Do not count withdrawn candidates.
            int totalCandidates
                = (from x in candidateStates
                   where new[] { CandidateState.States.hopeful, CandidateState.States.elected, CandidateState.States.defeated }
                      .Contains(x.Value.State)
                   select x).Count();

            votes.Sort();
            foreach (Vote v in votes)
            {

                MeekCandidateState cs = candidateStates[v.Candidate] as MeekCandidateState;
                if (cs is null)
                    throw new InvalidOperationException("candidateState contains objects that aren't of type MeekCandidateState");
                // Get value to transfer to this candidate, restricted to
                // the specified precision
                decimal value = weight * cs.KeepFactor;
                weight = RoundUp(weight);

                // Add this to the candidate's vote and remove from ballot's
                // weight.  CountedBallot shows multiple identical ballots, so
                // we add that many ballots to the vote and decrease the weight
                // of all identical ballots by the value kept.
                cs.VoteCount += value * ballot.Count;
                cs.BordaScore += value * ballot.Count * (totalCandidates - v.Value);
                weight -= value;

                // Do this until weight hits zero, or we run out of rankings.
                // Note:  Already-elected candidates have keep factors between
                // 0 and 1; hopefuls all have 1; defeated will have 0.  The
                // remaining voting power is either all transfered to the first
                // hopeful candidate or exhausted as the ballot runs out of
                // non-defeated candidates.
                //
                // We only hit 0 if a non-elected hopeful is on the ballot.
                if (weight <= 0.0m)
                    break;
            }
        }

        /// <inheritdoc/>
        protected override void CountBallots()
        {
            // The s>=surplus check is skipped the first iteration.
            // We implement this by having surplus greater than the
            // number of whole ballots.
            surplus = Convert.ToDecimal(ballots.TotalCount()) + 1;
            const decimal omega = 0.000001m;
            IEnumerable<Candidate> elected;

            // B.1 skip all this if we're finished.
            if (candidateStates.Count() > 0 && IsComplete())
                return;

            // Reference rule B.2.a Distribute Votes
            void DistributeVotes()
            {
                // Zero the vote counts
                foreach (CandidateState c in candidateStates.Values)
                    c.VoteCount = 0.0m;

                // Distribute the ballots among the votes.
                // Meek also considered an alternative formulation in which
                // voters would be allowed to indicate equal preference for
                // some candidates instead of a strict ordering; we have not
                // implemented this alternative.
                foreach (CountedBallot b in ballots)
                    CountBallot(b);
            }

            // Reference rule B.2.b
            decimal ComputeQuota()
            {
                decimal p = Convert.ToDecimal(Math.Pow(10.0D, Convert.ToDouble(0 - precision)));
                decimal q = candidateStates.Sum(x => x.Value.VoteCount);
                q /= seats + 1;
                // truncate and add the precision digit
                q = decimal.Round(q - 0.5m * p) + p;
                return q;
            }

            // B.2.d compute surplus of elected candidates
            decimal ComputeSurplus(decimal quota)
            {
                decimal s = 0.0m;
                // XXX:  does the reference rule mean total surplus not less
                // than zero, or only count candidates whose surplus is greater
                // than zero?
                foreach (Candidate c in candidateStates.Keys)
                {
                    if (candidateStates[c].State == CandidateState.States.elected)
                        s += candidateStates[c].VoteCount - quota;
                }
                if (s < 0.0m)
                    s = 0.0m;
                return s;
            }

            // B.2.f Update keep factors for elected candidates.  Returns true if stable state detected.
            bool UpdateKeepFactors(decimal quota)
            {
                // If no KeepFactors change or if any Keepfactor increases,
                // we have a stable state and are in stasis.
                bool noChange = true;
                bool increase = false;
                foreach (MeekCandidateState c in candidateStates.Values)
                {
                    if (!(new[] { CandidateState.States.elected }.Contains(c.State)))
                        continue;
                    decimal kf = c.KeepFactor;
                    // XXX: does the reference rule intend we round up
                    // between each operation, or multiply by (q/v) and
                    // then round up?
                    c.KeepFactor *= quota / c.VoteCount;
                    c.KeepFactor = RoundUp(c.KeepFactor);
                    if (kf != c.KeepFactor)
                        noChange = false;
                    if (c.KeepFactor > kf)
                        increase = true;
                }

                // Stasis detect
                return (noChange || increase);
            }

            // Reset this in case we immediately elect or hit surplus
            kfStasis = surplusStasis = false;

            do
            {
                decimal s = surplus;
                // B.2.a
                DistributeVotes();
                // B.2.b
                quota = ComputeQuota();
                // B.2.c
                elected = GetNewWinners(quota);
                // B.2.d
                surplus = ComputeSurplus(quota);
                if (surplus >= s)
                    surplusStasis = true;
                // B.2.e Counting continues at B1, next tabulation round.
                if (elected.Count() > 0)
                    break;
                // B.2.e Iteration complete, no election, go to B.3
                else if (surplus >= s || omega > surplus)
                    break;
                // B.2.f Update keep factors.  Also detect stasis.
                kfStasis = UpdateKeepFactors(quota);
                // Upon Stasis, we must eliminate candidates (B.3)
                // We know:
                //   - Elected < Seats
                //   - Elected + Hopeful > Seats
                // Re-entering this body is guaranteed to return to this state.
                // Elimination is required.
                if (kfStasis)
                    break;
            } while (true);
        }

        // Reference rule B.2.c, returns null if no hopefuls get elected.
        private IEnumerable<Candidate> GetNewWinners(decimal quota)
        {
            List<Candidate> winners = new List<Candidate>();
            Dictionary<Candidate, decimal> hopefuls = candidateStates
                    .Where(x => x.Value.State == CandidateState.States.hopeful)
                    .ToDictionary(x => x.Key, x => x.Value.VoteCount);
            foreach (Candidate c in hopefuls.Keys)
            {
                // Elect hopefuls who made it
                if (hopefuls[c] >= quota)
                    winners.Add(c);
            }
            return winners;
        }
    }
}