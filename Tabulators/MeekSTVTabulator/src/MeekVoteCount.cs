using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoonsetTechnologies.Voting.Tiebreakers;

namespace MoonsetTechnologies.Voting.Analytics
{
    public class MeekCandidateState : CandidateState
    {
        public decimal KeepFactor { get; set; }

        public MeekCandidateState()
            : base()
        {

        }
    }
    public class MeekVoteCount : ITiebreaker
    {
        private readonly List<IRankedBallot> ballots;
        private readonly List<ITiebreaker> tiebreakers =
            new ITiebreaker[] {
                new LastDifference(),
                new FirstDifference(),
            }.ToList();

        // number to elect
        private int seats;
        // Number of decimal places. The decimal type avoids
        // rounding error for up to 27 digits. For 9 billion
        // votes, precision up to 17 decimal places is possible.
        // Beyond ten is not generally necessary:
        // OpaVote uses 6, and the reference algorithm uses 9.
        private int precision = 9;

        public MeekVoteCount(IEnumerable<Candidate> candidates,
            IEnumerable<IRankedBallot> ballots,
            int seats, int precision)
        {
            this.ballots = ballots.ToList();
            this.seats = seats;
            this.precision = precision;

        }

        // Reference rule B.2.c, returns null if no hopefuls get elected.
        public IEnumerable<Candidate> GetWinners(decimal quota,
            Dictionary<Candidate, decimal> hopefuls)
        {
            List<Candidate> elected = new List<Candidate>();
            foreach (Candidate c in hopefuls.Keys)
            {
                // Elect hopefuls who made it
                if (hopefuls[c] >= quota)
                    elected.Add(c);
            }
            if (elected.Count == 0)
                return null;
            return elected;
        }

        /// <inheritdoc/>
        public IEnumerable<Candidate> GetEliminationCandidates(decimal surplus,
            int elected,
            Dictionary<Candidate, decimal> hopefuls)
        {
            Dictionary<Candidate, decimal> retain = new Dictionary<Candidate, decimal>();
            Dictionary<Candidate, decimal> batchLosers = new Dictionary<Candidate, decimal>(hopefuls);

            // If we elect candidates in B.2.c, the rule checks for a finished
            // election before running defeats.  It is logically-impossible to
            // attempt to defeat the last hopeful when correctly implementing
            // the rule.
            if (hopefuls.Count == 1)
                throw new ArgumentOutOfRangeException("hopefuls",
                    "Elimination of sole remaining candidate is impossible.");
            do
            {
                // XXX:  is this deterministic to get the key for the maximum value?
                Candidate max = batchLosers.OrderBy(x => x.Value).Last().Key;

                // Move this new candidate from cv to cd
                retain[max] = batchLosers[max];
                batchLosers.Remove(max);
                // Loop on two conditions:
                //
                //   - We've eliminated so many hopefuls as to not fill seats
                //   - batchLosers combined have more votes than the least-voted
                //     non-loser candidate
                //
                // In any case, stop looping if we have only one loser.
            } while (batchLosers.Count > 1
                && (elected + retain.Count() > seats 
                || batchLosers.Sum(x => x.Value) + surplus >= retain.Min(x => x.Value)));

            // Tie check
            if (batchLosers.Count == 1)
            { 
                // Load all the ties into batchLosers
                while (batchLosers.Max(x => x.Value) == retain.Min(x => x.Value))
                {
                    Candidate min = retain.OrderBy(x => x.Value).First().Key;
                    batchLosers[min] = retain[min];
                    retain.Remove(min);
                }

                
                List<Candidate> ties = new List<Candidate>();
                ITiebreaker t;
                // Try each tiebreaker
                foreach (ITiebreaker u in tiebreakers)
                {
                    t = u;
                    ties = u.GetTieWinners(batchLosers.Keys).ToList();
                    if (ties.Count == 1)
                        break;
                }

                // Delete all but the single loser
                foreach (Candidate c in ties)
                    batchLosers.Remove(c);
            }


            return batchLosers.Keys;
        }

        /// <inheritdoc/>
        public void UpdateTiebreaker<T>(Dictionary<Candidate, T> CandidateStates)
            where T : CandidateState
        {
            foreach (ITiebreaker t in tiebreakers)
                t.UpdateTiebreaker(CandidateStates);
        }

        /// <inheritdoc/>
        public bool AllTiesBreakable
        {
            get
            {
                foreach (ITiebreaker t in tiebreakers)
                {
                    if (!t.AllTiesBreakable)
                        return false;
                }
                return true;
            }
        }
        public IEnumerable<Candidate> GetTieWinners(IEnumerable<Candidate> candidates)
        {
            throw new NotImplementedException();
        }
    }
}
