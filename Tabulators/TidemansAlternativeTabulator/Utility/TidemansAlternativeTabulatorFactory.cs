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
    public class TidemansAlternativeTabulatorFactory : AbstractTabulatorFactory<TidemansAlternativeTabulator>
    {
        protected TopCycle.TopCycleSets condorcetSet = TopCycle.TopCycleSets.schwartz;
        protected TopCycle.TopCycleSets retainSet = TopCycle.TopCycleSets.smith;

        public TidemansAlternativeTabulatorFactory()
            : base()
        {
        }

        /// <inheritdoc/>
        protected override void ConfigureTabulator(ITabulatorSetting tabulatorSetting)
        {
            // Condorcet and retention checks
            if (tabulatorSetting is TidemansAlternativeCondorcetSetting)
                condorcetSet = (tabulatorSetting as TopCycleTabulatorSetting).Value;
            else if (tabulatorSetting is TidemansAlternativeRetentionSetting)
                retainSet = (tabulatorSetting as TopCycleTabulatorSetting).Value;
            // Don't recognize the setting
            else
                base.ConfigureTabulator(tabulatorSetting);
        }
    }

    [Export(typeof(ITabulatorSetting))]
    [ExportMetadata("Title", "Condorcet set")]
    [ExportMetadata("Description","The top cycle set used to test for a single Condorcet winner.")]
    public class TidemansAlternativeCondorcetSetting : TopCycleTabulatorSetting
    {

    }

    [Export(typeof(ITabulatorSetting))]
    [ExportMetadata("Title", "Top cycle set")]
    [ExportMetadata("Description", "The top cycle set from which candidates are retained each round.")]
    public class TidemansAlternativeRetentionSetting : TopCycleTabulatorSetting
    {

    }
}