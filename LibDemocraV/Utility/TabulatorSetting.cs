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

}
