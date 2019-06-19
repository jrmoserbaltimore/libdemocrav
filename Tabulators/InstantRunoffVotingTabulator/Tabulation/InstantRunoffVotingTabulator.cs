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
            IEnumerable<ITabulatorSetting> tabulatorSettings)
            : base(mediator, tiebreakerFactory, tabulatorSettings)
        {

        }

        /// <inheritdoc/>
        protected override void InitializeTabulation(BallotSet ballots, IEnumerable<Candidate> withdrawn, int seats)
        {
            base.InitializeTabulation(ballots, withdrawn, seats);

            RankedTabulationAnalytics analytics;
            analytics = new RankedTabulationAnalytics(ballots, seats);
            batchEliminator = new RunoffBatchEliminator(analytics, seats);
        }
    }
}
