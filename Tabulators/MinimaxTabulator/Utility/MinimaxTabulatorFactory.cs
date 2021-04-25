using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonsetTechnologies.Voting;
using MoonsetTechnologies.Voting.Ballots;
using System.Composition;

namespace MoonsetTechnologies.Voting.Utility
{
    // General algorithm:
    //   if CondorcetSet is One Candidate
    //     Winner is Candidate in CondorcetSet
    //   else
    //     Eliminate Candidates not in RetainSet
    //     Eliminate Candidate with Fewest Votes

    // Variants as (CondorcetSet, RetainSet):
    //   Tideman's Alternative:  (schwartz, smith)
    //   Tideman's Alternative Schwartz:  (schwartz, schwartz)
    //   Tideman's Alternative Smith:  (smith, smith)
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

