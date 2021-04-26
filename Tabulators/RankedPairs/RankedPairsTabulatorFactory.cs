using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Tabulation;
using System.Composition;

namespace MoonsetTechnologies.Voting.Utility
{
    public class RankedPairsTabulatorFactory : AbstractTabulatorFactory<RankedPairsTabulator>
    {
        public RankedPairsTabulatorFactory()
            : base()
        {
        }

        /// <inheritdoc/>
        protected override void ConfigureTabulator(ITabulatorSetting tabulatorSetting)
        {
            base.ConfigureTabulator(tabulatorSetting);
        }
    }
}

