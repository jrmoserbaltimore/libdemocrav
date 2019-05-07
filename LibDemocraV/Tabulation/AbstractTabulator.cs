using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public abstract class AbstractTabulator<T, U> : ITabulator
        where T : IBallot
        where U : IVoteCount
    {
        protected U voteCount;
        /// <inheritdoc/>
        public bool Complete => voteCount.GetTabulation().Count() == 0;

        /// <inheritdoc/>
        public virtual void TabulateRound()
        {
            Dictionary<Candidate, CandidateState.States> tabulation;

            // B.1 Test Count complete
            if (Complete)
                return;

            // Perform iteration B.2
            voteCount.CountBallots();

            // B.2.c Elect candidates, or B.3 defeat low candidates
            // We won't have defeats if there were elections in B.2.c,
            // but rule C may provide both winners and losers
            voteCount.ApplyTabulation();
        }
    }
}
