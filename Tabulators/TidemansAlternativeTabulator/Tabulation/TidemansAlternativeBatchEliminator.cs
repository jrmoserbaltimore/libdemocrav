using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Tiebreaking;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public class TidemansAlternativeBatchEliminator : AbstractBatchEliminator
    {
        protected readonly TopCycle condorcetSet;
        protected readonly TopCycle retentionSet;
        protected readonly IBatchEliminator batchEliminator;

        public TidemansAlternativeBatchEliminator(IBatchEliminator batchEliminator, TopCycle condorcetSet, TopCycle retentionSet, int seats = 1)
            : base(null, seats)
        {
            this.condorcetSet = condorcetSet;
            this.retentionSet = retentionSet;
            this.batchEliminator = batchEliminator;
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

            cCheck = condorcetSet.GetTopCycle(inputSet).ToList();
            // Reduce these to the appropriate checks
            rSet = retentionSet.GetTopCycle(inputSet).ToList();

            // Update tiebreakers before we send results back
            batchEliminator.UpdateTiebreaker(candidateStates);
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
                return batchEliminator.GetEliminationCandidates(candidateStates);
            }
        }

        // We use a batch eliminator which has its own tiebreaker
        /// <inheritdoc/>
        public override void UpdateTiebreaker(Dictionary<Candidate, CandidateState> candidateStates)
            => batchEliminator.UpdateTiebreaker(candidateStates);
    }
}
