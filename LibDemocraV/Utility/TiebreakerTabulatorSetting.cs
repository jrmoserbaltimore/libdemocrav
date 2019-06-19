using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    [Export(typeof(ITabulatorSetting))]
    [ExportMetadata("Title", "Tiebreaker")]
    [ExportMetadata("Description", "The tiebreaker algorithm to use.")]
    public class TiebreakerTabulatorSetting : ITabulatorDiscreteSetting<Type>
    {
        ITabulatorDiscreteSettingValue[] ITabulatorDiscreteSetting<Type>.GetValues() => GetValues();
        public Type Value { get; set; }

        [ImportMany]
        private IEnumerable<Lazy<AbstractTiebreaker, IAbstractTiebreakerMetadata>> Tiebreakers { get; }

        public TiebreakerTabulatorSettingValue[] GetValues()
            => (from x in Tiebreakers
                select new TiebreakerTabulatorSettingValue(x.Metadata, x.Metadata.Factory)).ToArray();
    }

    public class TiebreakerTabulatorSettingValue : ITabulatorDiscreteSettingValue
    {
        object ITabulatorDiscreteSettingValue.Metadata => Metadata as object;
        object ITabulatorDiscreteSettingValue.Value => Value as object;
        public IAbstractTiebreakerMetadata Metadata { get; }
        public Type Value { get; }
        public TiebreakerTabulatorSettingValue(IAbstractTiebreakerMetadata metadata, Type value)
        {
            Metadata = metadata;
            Value = value;
        }
    }
}
