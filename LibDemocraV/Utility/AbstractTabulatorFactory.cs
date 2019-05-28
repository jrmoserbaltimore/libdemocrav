using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public abstract class AbstractTabulatorFactory
    {
        protected AbstractTiebreakerFactory tiebreakerFactory;
        protected BallotFactory ballotFactory = new BallotFactory();
        public abstract AbstractTabulator CreateTabulator();

        public Ballot CreateBallot(IEnumerable<Vote> votes)
            => ballotFactory.CreateBallot(votes);

        public Vote CreateVote(Candidate candidate, decimal value)
            => ballotFactory.CreateVote(candidate, value);

        public void SetTiebreaker(AbstractTiebreakerFactory tiebreakerFactory)
        {
            this.tiebreakerFactory = tiebreakerFactory;
        }
    }

    public abstract class AbstractTabulatorFactory<T> : AbstractTabulatorFactory
        where T : AbstractTabulator
    {
        /// <inheritdoc/>
        public override AbstractTabulator CreateTabulator()
        {
            TabulationMediator mediator = new TabulationMediator
            {
                BallotFactory = ballotFactory
            };

            T t = Activator.CreateInstance(typeof(T), new object[] { mediator, tiebreakerFactory }) as T;
            return t;
        }
    }
}