using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Analytics
{
    public interface IAbstractAnalyticsMetadata
    {
        String Algorithm { get; }
        Type Factory { get; }
        String Description { get; }
    }
}
