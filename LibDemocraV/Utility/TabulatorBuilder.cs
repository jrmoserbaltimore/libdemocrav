using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public abstract class TabulatorBuilder<T, U>
        where T : IBallot
        where U : AbstractTabulator<AbstractVoteCount<T>>
    {
        protected ITiebreaker tiebreaker;
        protected AbstractBatchEliminator batchEliminator;
        protected IEnumerable<T> ballots;
        protected AbstractVoteCount<T> voteCount;
        protected U tabulator;

        public abstract void BuildTiebreaker();

        public abstract void BuildBatchEliminator(IEnumerable<T> ballots);

        public abstract void BuildVoteCounter(IEnumerable<Candidate> candidates);
        public abstract void BuildTabulator(int seats = 1);

        public virtual U GetTabulator()
        {
            U tabulator = this.tabulator;
            this.tabulator = default;
            this.tiebreaker = null;
            this.batchEliminator = null;
            this.ballots = null;
            this.voteCount = null;
            return tabulator;
        }
    }
}
