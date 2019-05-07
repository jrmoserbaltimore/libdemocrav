using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting
{
    public interface IRankedBallot : IBallot
    {
        new IEnumerable<IRankedVote> Votes { get; }
    }
}
