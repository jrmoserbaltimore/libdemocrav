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
                || tabulatorSetting is SmithConstrainedTabulatorSetting)
                settings.Add(tabulatorSetting);
            // Don't recognize the setting
            else
                base.ConfigureTabulator(tabulatorSetting);
        }
    }

    [Export(typeof(ITabulatorSetting))]
    [ExportMetadata("Title", "Alternative Smith")]
    [ExportMetadata("Description", "Before each elimination, eliminate all non-Smith candidates and elect if only one remains.")]
    [ExportMetadata("ParentOption", new System.Type[] { typeof(SmithConstrainedTabulatorSetting) })]
    public class AlternativeVoteAlternativeSmithSetting : BooleanTabulatorSetting
    {

    }

    [Export(typeof(ITabulatorSetting))]
    [ExportMetadata("Title", "Geller Elimination")]
    [ExportMetadata("Description", "Eliminate candidates by lowes Borda score instead of least votes.")]
    public class AlternativeVoteGellerSetting : BooleanTabulatorSetting
    {

    }

    // 26 A>B>C
    // 25 B>A>C
    // 49 C>B>A
    //
    // A: 77
    // B: 125
    // C: 98
}