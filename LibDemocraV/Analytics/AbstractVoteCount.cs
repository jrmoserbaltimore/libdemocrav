using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Analytics
{
    public abstract class AbstractVoteCount : IVoteCount
    {
        protected readonly IBatchEliminator batchEliminator;
        protected readonly ITiebreaker tiebreaker;

        protected AbstractVoteCount(ITiebreaker tiebreaker, IBatchEliminator batchEliminator)
        {
            this.batchEliminator = batchEliminator;
            this.tiebreaker = tiebreaker;
        }
        public abstract decimal GetVoteCount(Candidate candidate);
        public abstract Dictionary<Candidate, decimal> GetVoteCounts();
        /// <inheritdoc/>
        public virtual IEnumerable<Candidate> GetEliminationCandidates
            (Dictionary<Candidate, decimal> hopefuls, int elected, decimal surplus = 0.0m)
          => batchEliminator.GetEliminationCandidates(hopefuls, elected, surplus);
        public virtual void UpdateTiebreaker<T>(Dictionary<Candidate, T> CandidateStates) where T : CandidateState
        {
            tiebreaker.UpdateTiebreaker(CandidateStates);
        }
    }
}
