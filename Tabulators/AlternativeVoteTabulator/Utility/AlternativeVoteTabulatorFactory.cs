using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Tabulation;
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
    public class AlternativeVoteTabulatorFactory : AbstractTabulatorFactory<AlternativeVoteTabulator>
    {
        public AlternativeVoteTabulatorFactory()
            : base()
        {
        }

        /// <inheritdoc/>
        protected override void ConfigureTabulator(ITabulatorSetting tabulatorSetting)
        {
            // Condorcet and retention checks
            if (tabulatorSetting is AlternativeVoteAlternativeSmithSetting
                || tabulatorSetting is AlternativeVoteSmithIRVSetting)
                settings.Add(tabulatorSetting);
            // Don't recognize the setting
            else
                base.ConfigureTabulator(tabulatorSetting);
        }
    }

    // FIXME:  Allow dependencies
    [Export(typeof(ITabulatorSetting))]
    [ExportMetadata("Title", "Alternative Smith")]
    [ExportMetadata("Description", "Before each elimination, eliminate all non-Smith candidates and elect if only one remains.")]
    public class AlternativeVoteAlternativeSmithSetting : TopCycleTabulatorSetting
    {

    }
    [Export(typeof(ITabulatorSetting))]
    [ExportMetadata("Title", "Smith/IRV")]
    [ExportMetadata("Description", "Before beginning, eliminate all non-smith candidates.")]
    public class AlternativeVoteSmithIRVSetting : TopCycleTabulatorSetting
    {

    }
}