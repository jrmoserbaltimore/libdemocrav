using System;
using System.Collections.Generic;
using System.Text;
using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Tiebreaking;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public abstract class AbstractRankedBatchEliminator : AbstractBatchEliminator
    {
        public AbstractRankedBatchEliminator(AbstractTiebreaker tiebreaker,
            AbstractTabulationAnalytics analytics,
            int seats = 1)
            : base(tiebreaker, analytics as RankedTabulationAnalytics, seats)
        {
            // Not a RankedTabulationAnalytics
            if (this.analytics is null)
                throw new ArgumentException();
        }
    }
}
