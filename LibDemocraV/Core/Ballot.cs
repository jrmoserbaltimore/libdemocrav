using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting
{
    public interface IBallot
    {
        IEnumerable<IVote> Votes { get; }
    }
    /// <summary>
    /// A Vote object.  Immutable.
    /// </summary>
     public interface IVote
    {
        Candidate Candidate { get; }
    }


}
