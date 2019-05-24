using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Tiebreaking
{
    interface IAbstractTiebreakerMetadata
    {
        String Algorithm { get; }
        Type Factory { get; }
        String Description { get; }
    }
}
