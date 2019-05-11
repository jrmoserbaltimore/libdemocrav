using System;
using System.Collections.Generic;
using System.Text;
using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Tiebreaking;
using MoonsetTechnologies.Voting.Utility;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public class InstantRunoffVotingTabulator : RunoffTabulator
    {
        public InstantRunoffVotingTabulator(TabulationMediator mediator,
            AbstractTiebreakerFactory tiebreakerFactory,
            int seats = 1)
            : base(mediator, tiebreakerFactory, seats)
        {

        }

        protected override void InitializeTabulation(IEnumerable<Ballot> ballots, IEnumerable<Candidate> withdrawn, int seats)
        {
            base.InitializeTabulation(ballots, withdrawn, seats);

            RankedTabulationAnalytics analytics;
            analytics = new RankedTabulationAnalytics(ballots, seats);


            batchEliminator = new RunoffBatchEliminator(
                tiebreakerFactory.CreateTiebreaker(mediator), analytics, seats);
        }
    }
}
