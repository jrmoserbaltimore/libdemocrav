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

        public TidemansAlternativeBatchEliminator(RankedTabulationAnalytics analytics,
            int seats = 1,
            TopCycle.TopCycleSets condorcetSet = TopCycle.TopCycleSets.schwartz,
            TopCycle.TopCycleSets retentionSet = TopCycle.TopCycleSets.smith)
            : base(analytics, seats)
        {
            this.condorcetSet = condorcetSet;
            this.retentionSet = retentionSet;
        }

        /// <inheritdoc/>
        public override IEnumerable<Candidate> GetBatchElimination
            (Dictionary<Candidate, CandidateState> candidateStates, decimal surplus = 0.0m)
        {
            List<Candidate> cCheck, rSet;

            HashSet<Candidate> withdrawnSet = (from x in candidateStates
                                              where x.Value.State == CandidateState.States.withdrawn
                                              || x.Value.State == CandidateState.States.defeated
                                              select x.Key).ToHashSet();

            cCheck = (analytics as RankedTabulationAnalytics).GetTopCycle(withdrawnSet, condorcetSet).ToList();
            // Reduce these to the appropriate checks
            rSet = (analytics as RankedTabulationAnalytics).GetTopCycle(withdrawnSet, retentionSet).ToList();

            // Condorcet winner!
            if (cCheck.Count == 1)
            {
                return (from x in candidateStates
                        where x.Value.State == CandidateState.States.hopeful
                        select x.Key).Except(cCheck).ToArray();
            }
            else if (rSet.Count < withdrawnSet.Count)
            {
                return candidateStates.Keys.Except(rSet).ToArray();
            }
            else
            {
                // Top cycle is all candidates, so use a runoff batch eliminator
                return base.GetBatchElimination(candidateStates);
            }
        }
    }
}
