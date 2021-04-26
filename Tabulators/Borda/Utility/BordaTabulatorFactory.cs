using System.Composition;
using MoonsetTechnologies.Voting.Tabulation;

namespace MoonsetTechnologies.Voting.Utility
{
    public class BordaTabulatorFactory : AbstractTabulatorFactory<BordaTabulator>
    {
    }

    [Export(typeof(ITabulatorSetting))]
    [ExportMetadata("Title", "Black's Method")]
    [ExportMetadata("Description", "Elect the Condorcet candidate if one exists.")]
    [ExportMetadata("MutuallyExclusive", new System.Type[] { typeof(SmithConstrainedTabulatorSetting) })]
    public class BordaBlackSetting : BooleanTabulatorSetting
    {

    }
}
