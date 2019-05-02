using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public abstract class AbstractBallotFactory
    {
        public abstract IBallot CreateBallot(IEnumerable<IVote> votes);
        public abstract IBallot DecodeBallot(string encodedBallot);
    }
}
