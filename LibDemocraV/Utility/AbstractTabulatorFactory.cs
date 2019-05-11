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
        public abstract AbstractTabulator CreateTabulator();

        public abstract Ballot CreateBallot(IEnumerable<Vote> votes);

        public abstract Vote CreateVote(Candidate candidate, decimal value);

        public void SetTiebreaker(AbstractTiebreakerFactory tiebreakerFactory)
        {
            this.tiebreakerFactory = tiebreakerFactory;
        }
    }
}