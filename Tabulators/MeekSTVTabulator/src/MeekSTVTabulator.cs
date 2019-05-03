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
            // First Difference Tiebreaker method
            // If FirstDifference[Candidate] = true, this candidate wins
            // a tie with that candidate.
            public Dictionary<Candidate,bool> FirstDifference;
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

        protected void EliminateCandidates(decimal surplus)
        {
            Dictionary<Candidate, decimal> cv = new Dictionary<Candidate, decimal>();
            Dictionary<Candidate, decimal> cd = new Dictionary<Candidate, decimal>();
            bool disableBatch = false;
            // Count only hopefuls
            foreach (Candidate c in candidates.Keys)
            {
                if (candidates[c].State == CandidateState.States.hopeful)
                    cv[c] = candidates[c].VoteCount;

                // Update FirstDifference
                foreach (Candidate d in candidates.Keys)
                {
                    // Update FirstDifference only where all prior rounds have
                    // been ties.
                    // If any candidate continues with no FirstDifference,
                    // disable batch elimination.
                    if (!candidates[c].FirstDifference.ContainsKey(d))
                    {
                        if (candidates[c].VoteCount > candidates[d].VoteCount)
                            candidates[c].FirstDifference[d] = true;
                        else if (candidates[c].VoteCount < candidates[d].VoteCount)
                            candidates[c].FirstDifference[d] = false;
                        else
                            disableBatch = true;
                    }
                }
            }
            
            do
            {
                // XXX:  is this deterministic to get the key for the minimum value?
                Candidate min = cv.OrderBy(x => x.Value).First().Key;

                // Move this new candidate from cv to cd
                cd[min] = cv[min];
                cv.Remove(min);

                // If sum of lowest votes plus surplus is lower, check
                // for adequate hopefuls and continue
                if (cd.Sum(x => x.Value) + surplus < cv.Min(x => x.Value))
                {
                    // This should prevent removing all candidates.  Consider:
                    //   - Electing 5
                    //   - Have elected 3
                    //   - 7 hopefuls remain
                    //   - Each hopeful has more votes than all prior hopefuls
                    //     combined
                    //
                    // We would eliminate ALL hopefuls in one batch.
                    //
                    // After we eliminate 5, 2 remain.  2 + 3 = 5, so we'll
                    // eliminate those 5.
                    //
                    // It is impossible to tie here:  if the last 3 are ties,
                    // then the tie case will remain, e.g.:
                    //   [x x x x] H H H [e e e]
                    // electing 5, three tied hopefuls, four eliminated.  This
                    // produces zerocandidates to eliminate in the next round,
                    // requiring a tiebreaker.
                    if (
                        candidates.Select(
                        x => x.Value.State == CandidateState.States.elected).Count()
                        + cv.Count() > seats
                    )
                        continue;
                }
                else
                {
                    // Overshot, so move it back.
                    cv[min] = cd[min];
                    cd.Remove(min);
                }
            } while(false);

            // No complete tie, batching disabled
            if (disableBatch && cd.Count() > 0)
            {
                Candidate min = cd.OrderBy(x => x.Value).First().Key;
                List<Candidate> fd = new List<Candidate>();

                // Remove from cv each above the minimum vote count
                while (cd.OrderBy(x => x.Value).Last().Value > cd[min])
                    cd.Remove(cd.OrderBy(x => x.Value).Last().Key);

                // First-Difference tiebreaker
                fd.AddRange(FirstDifference(cd.Keys));
                
                // Remove candidates from FirstDifference results.
                // Batch up any remaining ties to avoid a tiebreaker.
                foreach (Candidate c in cd.Keys)
                {
                    if (!fd.Contains(c))
                    cd.Remove(c);
                }
            }
            // If the two lowest-voted candidates tie, invoke tiebreaker.
            // This is a standard First-Difference tiebreaker.  It looks
            // at all 
            else if (cd.Count() == 0)
            {
                Candidate min = cv.OrderBy(x => x.Value).First().Key;
                List<Candidate> cs = new List<Candidate>();
                foreach (Candidate c in cv.Keys)
                {
                    // Grab all candidates tied for last
                    if (cv[c] == cv[min])
                        cs.Add(c);
                }
                // Move each candidate qualified by FirstDifference to ce
                foreach (Candidate c in FirstDifference(cs))
                {
                    // Move this new candidate from cv to cd
                    cd[min] = cv[min];
                    cv.Remove(min);
                }

                cs.Clear();
                cs.AddRange(cd.Keys);

                // There are still multiple tied candidates, so we
                // need to carry out further tiebreakers.
                if (cs.Count > 1)
                {

                }
            }

            // Eliminate the whole batch
            foreach(Candidate c in cd.Keys)
            {
                candidates[c].KeepFactor = 0.0m;
                candidates[c].State = CandidateState.States.defeated;
            }
        }

        protected IEnumerable<Candidate> FirstDifference(IEnumerable<Candidate> candidates)
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
            EliminateCandidates(surplus);
            // B.4:  Next call enters at B.1
        }
    }
}