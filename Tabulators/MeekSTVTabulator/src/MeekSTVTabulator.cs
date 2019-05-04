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

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoonsetTechnologies.Voting.Analytics;

namespace MoonsetTechnologies.Voting.Tabulators
{
    public class MeekSTVTabulator : ISTVTabulator
    {
        private readonly MeekVoteCount voteCount;
        // number to elect
        private readonly int seats;
        // Number of decimal places. The decimal type avoids
        // rounding error for up to 27 digits. For 9 billion
        // votes, precision up to 17 decimal places is possible.
        // Beyond ten is not generally necessary:
        // OpaVote uses 6, and the reference algorithm uses 9.
        private readonly int precision = 9;
        private const decimal omega = 0.000001m;
        private decimal quota = 0.0m;
        private readonly List<IRankedBallot> ballots;

        // Round up to precision
        private decimal RoundUp(decimal d)
        {
            // The reference rule says to round UP, so we need
            // to Round(weight + 10^(-precision) / 2).
            decimal r = 0.5m * Convert.ToDecimal(Math.Pow(10.0D,
                Convert.ToDouble(0 - precision)));
            return decimal.Round(d + r, precision);
        }

        private readonly Dictionary<Candidate, MeekCandidateState> candidates
            = new Dictionary<Candidate, MeekCandidateState>();

        public MeekSTVTabulator(IEnumerable<Candidate> candidates,
            IEnumerable<IRankedBallot> ballots, int seats)
        {
            voteCount = new MeekVoteCount(candidates, ballots, seats, precision);
            this.ballots = new List<IRankedBallot>(ballots);
            this.seats = seats;

            // Reference rule A
            foreach (Candidate c in candidates)
            {
                MeekCandidateState cs = new MeekCandidateState
                {
                    KeepFactor = 1,
                    State = CandidateState.States.hopeful
                };
                this.candidates[c] = cs;
            }
        }

        public IEnumerable<Candidate> SchwartzSet => throw new NotImplementedException();

        public IEnumerable<Candidate> SmithSet => throw new NotImplementedException();

        public IEnumerable<Candidate> Candidates => candidates.Keys;

        // It's okay to use IsComplete() here because it will only
        // change state when there are no more rounds to tabulate.
        // This only occurs in a special case where the election has
        // no more candidates in total than seats and IsComplete() is
        // called before TabulateRound();
        /// <inheritdoc/>
        public bool Complete => CheckComplete();

        public void TabulateRound()
        {
            Dictionary<Candidate, decimal> vc;
            List<Candidate> cs = new List<Candidate>();
            // The s>=surplus check is skipped the first iteration.
            // We implement this by having surplus greater than the
            // number of whole ballots.
            decimal surplus = Convert.ToDecimal(ballots.Count) + 1;

            // B.1 Test Count complete
            if (Complete)
                return;

            do
            {
                decimal s = surplus;
                IEnumerable<Candidate> elected;
                bool kfStasis;
                // B.2.a
                //vc = voteCount.GetVoteCounts();
                DistributeVotes();
                // B.2.b
                quota = ComputeQuota();
                // B.2.c
                vc = candidates
                    .Where(x => x.Value.State == CandidateState.States.hopeful)
                    .ToDictionary(x => x.Key, x => x.Value.VoteCount);
                elected = voteCount.GetWinners(quota, vc);
                // B.2.d
                surplus = ComputeSurplus();
                // B.2.e Counting continues at B1, next tabulation round.
                // XXX:  Should we call UpdateFirstDifference() if we've elected?
                if (elected != null)
                    return;
                // B.2.e Iteration complete, no election, go to B.3
                else if (surplus >= s || omega > surplus)
                    break;
                // B.2.f Update keep factors.  Also detect stasis.
                kfStasis = UpdateKeepFactors();
                // Upon Stasis, we must eliminate candidates (B.3)
                // We know:
                //   - Elected < Seats
                //   - Elected + Hopeful > Seats
                // Therefor IsComplete() is false and will not change state.
                // Re-entering this body is guaranteed to return to this state.
                // Elimination is required.
                if (kfStasis)
                    break;
            } while (true);

            // B.3 Defeat low candidates.
            // We won't reach here if we elected someone in B.2.c
            DefeatLosers(surplus);
            // B.4:  Next call enters at B.1
        }

        // Reference rule components

        // Reference rule B.1
        private bool CheckComplete()
        {
            int e = 0, h = 0;
            foreach (CandidateState c in candidates.Values)
            {
                if (c.State == CandidateState.States.elected)
                    e++;
                else if (c.State == CandidateState.States.hopeful)
                    h++;
            }
            // We're done counting.  Go to reference rule step C.
            if (e + h <= seats)
            {
                foreach (CandidateState c in candidates.Values)
                {
                    // Elect or defeat hopefuls.
                    if (c.State == CandidateState.States.hopeful)
                    {
                        if (e == seats)
                            c.State = CandidateState.States.defeated;
                        else
                            c.State = CandidateState.States.elected;
                    }
                }
                return true;
            }
            // Still not done.
            return false;
        }

        // Reference rule B.2.a
        private void DistributeVotes()
        {
            // Zero the vote counts
            foreach (CandidateState c in candidates.Values)
                c.VoteCount = 0.0m;

            // Distribute the ballots among the votes.
            // Meek also considered an alternative formulation in which
            // voters would be allowed to indicate equal preference for
            // some candidates instead of a strict ordering; we have not
            // implemented this alternative.
            foreach (IRankedBallot b in ballots)
            {
                decimal weight = 1.0m;
                List<IRankedVote> votes = new List<IRankedVote>(b.Votes);
                votes.Sort();
                foreach (IRankedVote v in votes)
                {
                    // Get value to transfer to this candidate, restricted to
                    // the specified precision
                    decimal value = weight * candidates[v.Candidate].KeepFactor;
                    weight = RoundUp(weight);

                    // Add this to the candidate's vote and remove from ballot's
                    //weight
                    candidates[v.Candidate].VoteCount += value;
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
        }

        // Reference rule B.2.b
        private decimal ComputeQuota()
        {
            decimal p = Convert.ToDecimal(Math.Pow(10.0D, Convert.ToDouble(0 - precision)));
            decimal q = candidates.Sum(x => x.Value.VoteCount);
            q /= seats + 1;
            // truncate and add the precision digit
            q = decimal.Round(q - 0.5m * p) + p;
            return q;
        }

        // B.2.d compute surplus of elected candidates
        private decimal ComputeSurplus()
        {
            decimal s = 0.0m;
            // XXX:  does the reference rule mean total surplus not less
            // than zero, or only count candidates whose surplus is greater
            // than zero?
            foreach (Candidate c in candidates.Keys)
            {
                if (candidates[c].State == CandidateState.States.elected)
                    s += candidates[c].VoteCount - quota;
            }
            if (s < 0.0m)
                s = 0.0m;
            return s;
        }

        // B.2.f Update keep factors.  Returns true if stable state detected.
        private bool UpdateKeepFactors()
        {
            // If no KeepFactors change or if any Keepfactor increases,
            // we have a stable state and are in stasis.
            bool noChange = true;
            bool increase = false;
            foreach (MeekCandidateState c in candidates.Values)
            {
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

        // B.3 defeat candidates
        private void DefeatLosers(decimal surplus)
        {
            Dictionary<Candidate, decimal> hopefuls;
            List<Candidate> losers = new List<Candidate>();
            int numWinners;

            // Disable batch elimination until a full set of first
            // differences has been seen.  Typically there are no
            // ties in round one and batch elimination is enabled
            // immediately.  If not, one-by-one elimination avoids
            // missing a tie-breaking first difference as such:
            //
            //   - Y and Z are both batched losers
            //   - W and X are tied
            //   - Z transfers n and m votes to W and X
            //   - Y transfers n and m votes to X and W
            //
            // After eliminating Z, W and X are no longer tied.
            // After eliminating Z and Y, W and X are again tied.
            voteCount.UpdateTiebreaker(candidates);
            bool enableBatchElimination = voteCount.AllTiesBreakable;
            hopefuls = candidates
                .Where(x => x.Value.State == CandidateState.States.hopeful)
                .ToDictionary(x => x.Key, x => x.Value.VoteCount);
            numWinners = candidates
                .Where(x => x.Value.State == CandidateState.States.elected).Count();
            losers = voteCount.GetEliminationCandidates(surplus, numWinners,
                hopefuls).ToList();

            // Batch elimination disabled, select first
            if (!enableBatchElimination && losers.Count > 1)
            {
                Candidate min = losers.First();
                foreach (Candidate c in losers)
                {
                    if (candidates[c].VoteCount < candidates[min].VoteCount)
                        min = c;
                }
                losers.Clear();
                losers.Add(min);
            }

            // Eliminate the whole batch
            foreach (Candidate c in losers)
            {
                candidates[c].KeepFactor = 0.0m;
                candidates[c].State = CandidateState.States.defeated;
            }
        }
    }
}