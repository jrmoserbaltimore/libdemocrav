using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MoonsetTechnologies.Voting;


namespace MoonsetTechnologies.Voting.Utility
{
    [BallotTypeId(typeof(RankedBallot))]
    public class RankedBallotFactory : AbstractBallotFactory
    {
        protected List<Candidate> candidates = new List<Candidate>();
        protected IEnumerable<Candidate> Candidates => candidates;
        public IBallot CreateBallot(IEnumerable<IRankedVote> votes) => new RankedBallot(votes);
        public override IBallot DecodeBallot(string encodedBallot)
        {
            Regex r = new Regex(@"^(([\da-f]{8}(-[\da-f]{4}){3}-[\da-f]{12})([>=]|$))+");

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