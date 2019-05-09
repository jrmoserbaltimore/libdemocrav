using MoonsetTechnologies.Voting.Ballots;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public abstract class AbstractBallotFactory
    {
        public abstract Ballot CreateBallot(IEnumerable<Vote> votes);
        /// <summary>
        /// Decode an encoded ballot 
        /// </summary>
        /// <param name="encodedBallot"></param>
        /// <returns></returns>
        public abstract Ballot DecodeBallot(string encodedBallot);
    }
}
