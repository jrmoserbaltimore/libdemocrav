using MoonsetTechnologies.Voting.Tabulation;
using System.Composition;

namespace MoonsetTechnologies.Voting.Utility
{
    public class MeekSTVTabulatorFactory : AbstractTabulatorFactory<MeekSTVTabulator>
    {
    }

    [Export(typeof(ITabulatorSetting))]
    [ExportMetadata("Title", "Meek-Geller")]
    [ExportMetadata("Description", "Eliminate by the lowest Borda count.")]
    public class MeekSTVGellerSetting : BooleanTabulatorSetting
    {

    }
}
