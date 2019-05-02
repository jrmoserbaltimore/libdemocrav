using System;
using System.Collections.Generic;
using MoonsetTechnologies.Voting;

namespace MoonsetTechnologies.Voting.Utility
{
    [BallotTypeId(typeof(RankedBallot))]
    public class RankedBallotFactory : AbstractBallotFactory
    {
        public IBallot CreateBallot(IEnumerable<IRankedVote> votes) => new RankedBallot(votes);
        public override IBallot DecodeBallot(string encodedBallot)
        {
            throw new NotImplementedException();
        }
        public override IBallot CreateBallot(IEnumerable<IVote> votes)
        {
            if (votes is IEnumerable<IRankedVote>)
                return CreateBallot((IEnumerable<IRankedVote>)votes);
            throw new ArgumentException("votes is not IEnumerable<IRankedVote>");
        }
    }
}