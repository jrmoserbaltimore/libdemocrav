using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoonsetTechnologies.Voting.Tiebreaking;
using MoonsetTechnologies.Voting.Analytics;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public abstract class AbstractRankedVoteCount : AbstractVoteCount
    {
        protected int seats;
        public AbstractRankedVoteCount(IEnumerable<Candidate> candidates, IEnumerable<IRankedBallot> ballots,
            IBatchEliminator batchEliminator, int seats = 1)
            : base(candidates, ballots, batchEliminator)
        {
            this.seats = seats;
        }
    }
}
