using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoonsetTechnologies.Voting.Tiebreakers;

namespace MoonsetTechnologies.Voting.Analytics
{
    public class RunoffBatchEliminator : IBatchEliminator
    {
        private readonly ITiebreaker tiebreaker;
        private readonly int seats;

        public RunoffBatchEliminator(ITiebreaker tiebreakers, int seats = 1)
        {
            this.tiebreaker = tiebreakers;
            this.seats = seats;
        }

        /// <inheritdoc/>
        public IEnumerable<Candidate> GetEliminationCandidates
            (Dictionary<Candidate, decimal> hopefuls, int elected, decimal surplus = 0.0m)
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
                ties = tiebreaker.GetTieWinners(batchLosers.Keys).ToList();

                // Delete all but the single loser
                foreach (Candidate c in ties)
                    batchLosers.Remove(c);
            }
            return batchLosers.Keys;
        }
    }
}
