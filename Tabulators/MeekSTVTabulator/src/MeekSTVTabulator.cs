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
using MoonsetTechnologies.Voting.Analytics;

namespace MoonsetTechnologies.Voting.Tabulators
{
    public class MeekSTVTabulator : ISTVTabulator
    {
        // number to elect
        private int seats;
        // Number of decimal places. The decimal type avoids
        // rounding error for up to 27 digits. For 9 billion
        // votes, precision up to 17 decimal places is possible.
        // Beyond ten is not generally necessary:
        // OpaVote uses 6, and the reference algorithm uses 9.
        private int precision = 9;
        // Round up to precision
        private decimal RoundUp(decimal d)
        {
            // The reference rule says to round UP, so we need
            // to Round(weight + 10^(-precision) / 2).
            decimal r = 0.5m * Convert.ToDecimal(Math.Pow(10.0D,
                Convert.ToDouble(0 - precision)));
            return decimal.Round(d + r, precision);
        }
        private decimal omega = 0.000001m;
        private decimal quota = 0.0m;
        private List<IRankedBallot> Ballots;

        private Dictionary<Candidate, CandidateState> candidates;

        private class CandidateState
        {
            public enum States
            {
                defeated = 0,
                withdrawn = 1,
                hopeful = 2,
                elected = 3
            };
            public decimal KeepFactor;
            public decimal VoteCount;
            public States State;
        }

        public MeekSTVTabulator()
        {
        }

        protected bool IsComplete()
        {
            // Reference rule B.1
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
        protected void DistributeVotes()
        {
            // Zero the vote counts
            foreach (CandidateState c in candidates.Values)
                c.VoteCount = 0.0m;

            // Distribute the ballots among the votes.
            // Meek also considered an alternative formulation in which
            // voters would be allowed to indicate equal preference for
            // some candidates instead of a strict ordering; we have not
            // implemented this alternative.
            foreach (IRankedBallot b in Ballots)
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
        protected decimal ComputeQuota()
        {
            decimal p = Convert.ToDecimal(Math.Pow(10.0D, Convert.ToDouble(0 - precision)));
            decimal q = 0;
            foreach (CandidateState c in candidates.Values)
                q += c.VoteCount;
            q /= seats + 1;
            // truncate and add the precision digit
            q = decimal.Round(q - 0.5m * p) + p;
            return q;
        }

        // Reference rule B.2.c, returns true if hopefuls get elected.
        protected bool ElectWinners()
        {
            bool elected = false;
            foreach (CandidateState c in candidates.Values)
            {
                // Elect hopefuls who made it
                if (c.State == CandidateState.States.hopeful && c.VoteCount >= quota)
                {
                    c.State = CandidateState.States.elected;
                    elected = true;
                }
            }
            return elected;
        }

        protected decimal ComputeSurplus()
        {
            decimal s = 0.0m;
            // XXX:  does the reference rule mean total surplus not less
            // than zero, or only count candidates whose surplus is greater
            // than zero?
            foreach (CandidateState c in candidates.Values)
            {
                if (c.State == CandidateState.States.elected)
                    s += c.VoteCount - quota;
            }
            if (s < 0.0m)
                s = 0.0m;
            return s;
        }

        protected bool UpdateKeepFactors()
        {
            // If no KeepFactors change or if any Keepfactor increases,
            // we have a stable state and are in stasis.
            bool noChange = true;
            bool increase = false;
            foreach (CandidateState c in candidates.Values)
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

        protected void EliminateCandidates()
        {
            throw new NotImplementedException();
        }

        protected bool DetectStasis()
        {
            throw new NotImplementedException();
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
        public bool Complete => IsComplete();

        public void TabulateRound()
        {
            // The s>=surplus check is skipped the first iteration.
            // We implement this by having surplus greater than the
            // number of whole ballots.
            decimal surplus = Convert.ToDecimal(Ballots.Count) + 1;

            // B.1 Test Count complete
            if (IsComplete())
                return;

            do
            {
                decimal s = surplus;
                bool elected = false;
                bool kfStasis = true;
                // B.2.a
                DistributeVotes();
                // B.2.b
                quota = ComputeQuota();
                // B.2.c
                elected = ElectWinners();
                // B.2.d
                surplus = ComputeSurplus();
                // B.2.e Counting continues at B1, next tabulation round.
                if (elected)
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
            // B.3 Defeat low candidates
            EliminateCandidates();
            // B.4:  Next call enters at B.1
        }
    }
}