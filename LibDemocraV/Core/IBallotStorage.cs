using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Storage
{
    public interface IBallotStorage
    {
        IEnumerable<IBallot> Ballots { get; }
    }

}
