using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public interface ITabulatorSettingMetadata
    {
        string Description { get; }
        string Title { get; }
    }

    public interface ITabulatorSetting
    {
    }

    public interface ITabulatorSetting<T> : ITabulatorSetting
    {
        T Value { get; }
    }
    
    public interface ITabulatorDiscreteSetting<T> : ITabulatorSetting<T>
    {
        ITabulatorDiscreteSettingValue[] GetValues();
    }

    public interface ITabulatorDiscreteSettingValue
    {
        object Metadata { get; }
        object Value { get; }
    }

    public class BooleanTabulatorSetting : ITabulatorDiscreteSetting<bool>
    {
        ITabulatorDiscreteSettingValue[] ITabulatorDiscreteSetting<bool>.GetValues() => GetValues();
        public bool Value { get; }

        public BooleanDiscreteSettingValue[] GetValues()
        {
            BooleanDiscreteSettingValue[] a = new[]
            {
                new BooleanDiscreteSettingValue("True", true),
                new BooleanDiscreteSettingValue("False", false)
            };
            return a;
        }
    }

    public class BooleanDiscreteSettingValue : ITabulatorDiscreteSettingValue
    {
        object ITabulatorDiscreteSettingValue.Metadata => Metadata as object;
        object ITabulatorDiscreteSettingValue.Value => Value as object;
        public string Metadata { get; }
        public bool Value { get; }
        public BooleanDiscreteSettingValue(string metadata, bool value)
        {
            Metadata = metadata;
            Value = value;
        }
    }
}
