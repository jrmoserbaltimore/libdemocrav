using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoonsetTechnologies.Voting.Tiebreaking;
using MoonsetTechnologies.Voting.Analytics;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public class TopCycleBatchEliminator : AbstractBatchEliminator
    {
        public TopCycleBatchEliminator(RankedTabulationAnalytics analytics,
            int seats = 1)
            : base(analytics, seats)
        {

        }

        /// <inheritdoc/>
        protected IEnumerable<Candidate> GetEliminationCandidates(Dictionary<Candidate, CandidateState> candidateStates, decimal surplus, bool batchElimination = true)
        {
            List<Candidate> rSet;

            HashSet<Candidate> withdrawnSet = (from x in candidateStates
                                               where x.Value.State == CandidateState.States.withdrawn
                                               || x.Value.State == CandidateState.States.defeated
                                               select x.Key).ToHashSet();

            // FIXME:  Make configurable
            TopCycle.TopCycleSets retentionSet = TopCycle.TopCycleSets.schwartz;
            // Reduce these to the appropriate checks
            rSet = (analytics as RankedTabulationAnalytics).GetTopCycle(withdrawnSet, retentionSet).ToList();

            // Return hopefuls outside the appropriate set.  May return zero.
            return (from x in candidateStates
                    where x.Value.State == CandidateState.States.hopeful
                    select x.Key).Except(rSet).ToArray();
        }

        /// <inheritdoc/>
        public override IEnumerable<Candidate> GetSingleElimination(Dictionary<Candidate, CandidateState> candidateStates, decimal surplus = 0.0m)
            => GetEliminationCandidates(candidateStates, surplus, false);

        /// <inheritdoc/>
        public override IEnumerable<Candidate> GetBatchElimination(Dictionary<Candidate, CandidateState> candidateStates, decimal surplus = 0.0m)
            => GetEliminationCandidates(candidateStates, surplus, true);
    }
}
