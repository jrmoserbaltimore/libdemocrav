using System;
using System.Collections.Generic;
using System.Text;
using MoonsetTechnologies.Voting.Analytics;

namespace MoonsetTechnologies.Voting.Tabulation
{
    class TidemansAlternativeVoteCount : AbstractRankedVoteCount
    {
        public TidemansAlternativeVoteCount(IEnumerable<Candidate> candidates,
            IEnumerable<IRankedBallot> ballots,
            IBatchEliminator batchEliminator)
            : base(candidates, ballots, batchEliminator)
        {

        }

        public override void CountBallots()
        {
            throw new NotImplementedException();
        }

        public override Dictionary<Candidate, CandidateState.States> GetTabulation()
        {
            throw new NotImplementedException();
        }
    }
}
