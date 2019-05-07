using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoonsetTechnologies.Voting.Tiebreaking;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public class RunoffBatchEliminator : AbstractBatchEliminator
    {

        protected bool enableBatchElimination = true;

        public RunoffBatchEliminator(ITiebreaker tiebreakers, int seats = 1)
            : base(tiebreakers, seats)
        {

        }

        /// <inheritdoc/>
        public override IEnumerable<Candidate> GetEliminationCandidates
            (Dictionary<Candidate, CandidateState> candidateStates, decimal surplus = 0.0m)
        {
            Dictionary<Candidate, decimal> hopefuls = candidateStates
                .Where(x => x.Value.State == CandidateState.States.hopeful)
                .ToDictionary(x => x.Key, x => x.Value.VoteCount);
            int elected = candidateStates.Where(x => x.Value.State == CandidateState.States.elected).Count();
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

            bool enableBatchElimination = tiebreaker.FullyInformed && this.enableBatchElimination;

            // Batch elimination disabled, so eliminate all in the batch except the
            // candidate with the fewest votes, or the set of tied candidates with
            // the fewest.
            //
            // Ties within the batch can be eliminated without a tiebreaker, so we
            // eliminate the ties if they're tied for last place in a batch even if
            // we're trying to eliminate one at a time.
            if (!enableBatchElimination && batchLosers.Count > 1)
            {
                Candidate min = batchLosers.OrderBy(x => x.Value).First().Key;
                foreach (Candidate c in batchLosers.Keys)
                {
                    if (batchLosers[c] > batchLosers[min])
                        batchLosers.Remove(c);
                }
            }
            // True tie check
            else if (batchLosers.Count == 1)
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

                // Delete from the losers all but the tiewinners
                foreach (Candidate c in ties)
                {
                    retain[c] = batchLosers[c];
                    batchLosers.Remove(c);
                }

                // Unbreakable tie.
                if (batchLosers.Count > 1)
                {
                    throw new NotImplementedException();
                }
            }
            return batchLosers.Keys;
        }
    }
}
