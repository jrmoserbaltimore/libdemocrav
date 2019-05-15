using System;
using System.Collections.Generic;
using System.Text;
using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Tiebreaking;
using MoonsetTechnologies.Voting.Utility;
using MoonsetTechnologies.Voting.Utility.Attributes;

namespace MoonsetTechnologies.Voting.Tabulation
{
    [SmithEfficient]
    public class TidemansAlternativeTabulator : RunoffTabulator
    {
        TopCycle.TopCycleSets condorcetSet = TopCycle.TopCycleSets.schwartz;
        TopCycle.TopCycleSets retainSet = TopCycle.TopCycleSets.smith;

        public TidemansAlternativeTabulator(TabulationMediator mediator,
            AbstractTiebreakerFactory tiebreakerFactory)
            : base(mediator, tiebreakerFactory)
        {

        }

        protected override void InitializeTabulation(IEnumerable<Ballot> ballots, IEnumerable<Candidate> withdrawn, int seats)
        {
            base.InitializeTabulation(ballots, withdrawn, seats);

            RankedTabulationAnalytics analytics;
            analytics = new RankedTabulationAnalytics(ballots, seats);

            batchEliminator = new TidemansAlternativeBatchEliminator(
                tiebreakerFactory.CreateTiebreaker(mediator), analytics, seats,
                condorcetSet, retainSet);
        }
    }
}
