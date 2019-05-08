using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public abstract class AbstractTabulator<T, TType> : ITabulator
        where T : IBallot
        where TType: AbstractTabulator<T, TType>
    {
        protected int seats;
        protected IBatchEliminator batchEliminator;
        protected List<T> ballots;
        protected readonly Dictionary<Candidate, CandidateState> candidateStates
            = new Dictionary<Candidate, CandidateState>();

        /// <inheritdoc/>
        public bool Complete => GetTabulation().Count() == 0;

        /// <inheritdoc/>
        public abstract void TabulateRound();

        /// <inheritdoc/>
        public abstract Dictionary<Candidate, CandidateState.States> GetTabulation();

        /// <inheritdoc/>
        public abstract Dictionary<Candidate, CandidateState> GetFullTabulation();

        /// <summary>
        /// Perform a ballot count and updates the internal state.
        /// </summary>
        protected abstract void CountBallots();

        protected AbstractTabulator()
        {

        }

        public abstract class Builder<BType>
            where BType : Builder<BType>
        {
            protected IEnumerable<T> ballots;
            protected IEnumerable<Candidate> candidates;
            protected int seats;

            public abstract void BuildTiebreaker();

            public abstract AbstractBatchEliminator BuildBatchEliminator();

            public abstract BType WithSeats(int seats);

            public abstract BType WithCandidates(IEnumerable<Candidate> candidates);

            public abstract BType WithBallots(IEnumerable<T> ballots);

            public abstract TType Build();
        }
    }
}
