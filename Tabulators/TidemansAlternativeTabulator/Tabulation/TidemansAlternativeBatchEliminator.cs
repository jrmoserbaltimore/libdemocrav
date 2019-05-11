using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Tiebreaking;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public class TidemansAlternativeBatchEliminator : RunoffBatchEliminator
    {
        protected readonly TopCycle.TopCycleSets condorcetSet;
        protected readonly TopCycle.TopCycleSets retentionSet;

        public TidemansAlternativeBatchEliminator(AbstractTiebreaker tiebreaker,
            RankedTabulationAnalytics analytics,
            int seats = 1,
            TopCycle.TopCycleSets condorcetSet = TopCycle.TopCycleSets.schwartz,
            TopCycle.TopCycleSets retentionSet = TopCycle.TopCycleSets.smith)
            : base(tiebreaker, analytics, seats)
        {
            this.condorcetSet = condorcetSet;
            this.retentionSet = retentionSet;

        }

        /// <inheritdoc/>
        public override IEnumerable<Candidate> GetEliminationCandidates
            (Dictionary<Candidate, CandidateState> candidateStates, decimal surplus = 0.0m)
        {
            List<Candidate> output;
            List<Candidate> cCheck, rSet;

            List<Candidate> inputSet = candidateStates.Where(x => x.Value.State == CandidateState.States.elected
                  || x.Value.State == CandidateState.States.hopeful)
                  .ToDictionary(x => x.Key, null).Keys.ToList();

            cCheck = (analytics as RankedTabulationAnalytics).GetTopCycle(inputSet, condorcetSet).ToList();
            // Reduce these to the appropriate checks
            rSet = (analytics as RankedTabulationAnalytics).GetTopCycle(inputSet, retentionSet).ToList();

            // Condorcet winner!
            if (cCheck.Count == 1)
            {
                return candidateStates
                    .Where(x => x.Value.State == CandidateState.States.hopeful && x.Key != cCheck.First())
                    .Select(x => x.Key).ToList();

            }
            else if (rSet.Count() < inputSet.Count())
            {
                output = new List<Candidate>(inputSet);
                foreach (Candidate c in rSet)
                    output.Remove(c);
                return output;
            }
            else
            {
                // Top cycle is all candidates, so use a runoff batch eliminator
                return base.GetEliminationCandidates(candidateStates);
            }
        }
    }
}
