using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public interface IAbstractTabulatorMetadata
    {
        String Algorithm { get; }
        Type Factory { get; }
        String Description { get; }
        // FIXME:  Provide a method of indicating tunables and configurations.
    }
}
