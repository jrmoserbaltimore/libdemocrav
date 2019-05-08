using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoonsetTechnologies.Voting.Tiebreaking;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public interface IVoteCount
    {
        /// <summary>
        /// Perform a ballot count and updates the internal state.
        /// </summary>
        void CountBallots();

        /// <summary>
        /// Get the winners and losers for the next round of tabulation.
        /// Winners should go to ElectCandidates(); losers should go to DefeatCandidates().
        /// </summary>
        /// <returns>The new state of all Candidates which require a state change in this
        /// tabulation round. Returns an empty dictionary when tabulation is complete.</returns>
        Dictionary<Candidate, CandidateState.States> GetTabulation();

        /// <summary>
        /// Get winners and losers at the current round, with vote counts.
        /// </summary>
        /// <returns>A full set of candidate states and votes.</returns>
        Dictionary<Candidate, CandidateState> GetFullTabulation();

        /// <summary>
        /// Apply tabulation results for this round.
        /// </summary>
        /// <returns>true if candidates have been elected or defeated.
        /// false if tabulation is complete.</returns>
        bool ApplyTabulation();
    }
  
}
