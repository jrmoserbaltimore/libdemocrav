using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public abstract class AbstractBatchEliminator : IBatchEliminator
    {
        protected readonly ITiebreaker tiebreaker;
        protected readonly int seats;

        public AbstractBatchEliminator(ITiebreaker tiebreakers, int seats = 1)
        {
            this.tiebreaker = tiebreakers;
            this.seats = seats;
        }

        /// <inheritdoc/>
        public abstract IEnumerable<Candidate> GetEliminationCandidates(Dictionary<Candidate, CandidateState> candidateStates, decimal surplus = 0.0M);
        /// <inheritdoc/>
        public virtual void UpdateTiebreaker(Dictionary<Candidate, CandidateState> candidateStates)
            => tiebreaker.UpdateTiebreaker(candidateStates);
    }
}
