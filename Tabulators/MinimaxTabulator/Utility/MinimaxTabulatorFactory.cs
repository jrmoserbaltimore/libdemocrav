﻿using MoonsetTechnologies.Voting.Analytics;
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

    [Export(typeof(ITabulatorSetting))]
    [ExportMetadata("Title", "Smith-constrained")]
    [ExportMetadata("Description", "Enable to use Smith/Minimax.")]
    public class MinimaxTabulatorSmithSetting : TopCycleTabulatorSetting
    {

    }
}

