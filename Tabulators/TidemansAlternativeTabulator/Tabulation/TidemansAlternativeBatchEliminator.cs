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
        TopCycle topcycle;
        public TidemansAlternativeBatchEliminator(ITiebreaker tiebreakers, TopCycle topcycle, int seats = 1)
            : base(tiebreakers, seats)
        {
            this.topcycle = topcycle;
        }
        public override IEnumerable<Candidate> GetEliminationCandidates
            (Dictionary<Candidate, CandidateState> candidateStates, decimal surplus = 0.0m)
        {
            List<Candidate> output;
            TopCycle t;
            List<Candidate> cCheck, rSet;

            List<Candidate> inputSet = candidateStates.Where(x => x.Value.State == CandidateState.States.elected
                  || x.Value.State == CandidateState.States.hopeful)
                  .ToDictionary(x => x.Key, null).Keys.ToList();

            cCheck = topcycle.GetSchwartzSet(inputSet).ToList();
            // Reduce these to the appropriate checks
            rSet = topcycle.GetSmithSet(inputSet).ToList();

            // Update tiebreakers before we send results back
            tiebreaker.UpdateTiebreaker(candidateStates);
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
                // Top cycle is all candidates, so use tiebreaker.
                // The tiebreaker should be the BatchEliminatorTiebreaker using a
                // RunoffBatchEliminator with actual tiebreakers
                output = new List<Candidate>(inputSet);
                foreach (Candidate c in tiebreaker.GetTieWinners(rSet))
                    output.Remove(c);
                return output;
            }
        }
    }
}
