using System;
using System.Collections.Generic;
using System.Composition;
using MoonsetTechnologies.Voting.Analytics;

namespace MoonsetTechnologies.Voting.Utility
{

    public class TopCycleTabulatorSetting : ITabulatorDiscreteSetting<TopCycle.TopCycleSets>
    {
        ITabulatorDiscreteSettingValue[] ITabulatorDiscreteSetting<TopCycle.TopCycleSets>.GetValues() => GetValues();
        public TopCycle.TopCycleSets Value { get; }

        public TopCycleDiscreteSettingValue[] GetValues()
        {
            TopCycleDiscreteSettingValue[] a = new[]
            {
                new TopCycleDiscreteSettingValue("The Smith (GETCHA) set", TopCycle.TopCycleSets.smith),
                new TopCycleDiscreteSettingValue("The Schwartz (GOCHA) set", TopCycle.TopCycleSets.schwartz)
            };
            return a;
        }
    }

    public class TopCycleDiscreteSettingValue : ITabulatorDiscreteSettingValue
    {
        object ITabulatorDiscreteSettingValue.Metadata => Metadata as object;
        object ITabulatorDiscreteSettingValue.Value => Value as object;
        public string Metadata { get; }
        public TopCycle.TopCycleSets Value { get; }
        public TopCycleDiscreteSettingValue(string metadata, TopCycle.TopCycleSets value)
        {
            Metadata = metadata;
            Value = value;
        }
    }

    [Export(typeof(ITabulatorSetting))]
    [ExportMetadata("Title", "Smith-constrained")]
    [ExportMetadata("Description", "Before beginning tabulation, eliminate all non-Smith candidates.")]
    public class SmithConstrainedTabulatorSetting : BooleanTabulatorSetting
    {

    }
}
