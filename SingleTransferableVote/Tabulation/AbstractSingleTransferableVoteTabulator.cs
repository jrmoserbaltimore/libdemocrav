using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoonsetTechnologies.Voting.Ballots;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public abstract class AbstractSingleTransferableVoteTabulator : RankedTabulator
    {

        /// <inheritdoc/>
        public AbstractSingleTransferableVoteTabulator(IEnumerable<Candidate> candidates, IEnumerable<Ballot> ballots,
            IBatchEliminator batchEliminator, int seats = 1)
            : base(candidates, ballots, batchEliminator, seats)
        {

        }

        // A simple count of who has the most votes.
        /// <inheritdoc/>
        protected abstract override void CountBallots();
    }
}
