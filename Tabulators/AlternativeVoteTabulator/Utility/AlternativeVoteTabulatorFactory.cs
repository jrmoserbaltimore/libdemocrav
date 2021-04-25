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
        protected TopCycle.TopCycleSets condorcetSet = TopCycle.TopCycleSets.schwartz;
        protected TopCycle.TopCycleSets retainSet = TopCycle.TopCycleSets.smith;

        public AlternativeVoteTabulatorFactory()
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