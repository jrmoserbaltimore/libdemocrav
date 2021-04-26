using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Tabulation;
using System.Composition;

namespace MoonsetTechnologies.Voting.Utility
{
    public class MinimaxTabulatorFactory : AbstractTabulatorFactory<MinimaxTabulator>
    {
        public MinimaxTabulatorFactory()
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

