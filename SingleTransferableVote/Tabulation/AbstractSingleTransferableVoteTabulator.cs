using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Utility;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public abstract class AbstractSingleTransferableVoteTabulator : RunoffTabulator
    {
        public AbstractSingleTransferableVoteTabulator(TabulationMediator mediator,
            AbstractTiebreakerFactory tiebreakerFactory,
            IEnumerable<ITabulatorSetting> tabulatorSettings)
            : base(mediator, tiebreakerFactory, tabulatorSettings)
        {
        }
    }
}
