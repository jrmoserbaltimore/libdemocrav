using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonsetTechnologies.Voting.Analytics;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public class TidemansAlternativeVoteCount : RankedVoteCount
    {
        public TidemansAlternativeVoteCount(IEnumerable<Candidate> candidates,
            IEnumerable<IRankedBallot> ballots,
            IBatchEliminator batchEliminator)
            : base(candidates, ballots, batchEliminator)
        {

        }

        // XXX:  This is moving up into the BatchEliminator.
        public override Dictionary<Candidate, CandidateState.States> GetTabulation()
        {
            throw new NotImplementedException();
        }
    }
}
