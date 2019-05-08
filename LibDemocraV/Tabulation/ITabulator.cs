using System;
using System.Collections;
using System.Collections.Generic;

namespace MoonsetTechnologies.Voting.Tabulation
{
    /// <summary>
    /// A voting rule to tabulate votes.
    /// </summary>
    public interface ITabulator
    {
        /// <summary>
        /// True if tabulation is complete; else false.
        /// </summary>
        bool Complete { get; }

        /// <summary>
        /// Perform the next round of tabulation.
        /// </summary>
        void TabulateRound();

        /// <summary>
        /// Get the winners and losers for this round next round of tabulation.
        /// </summary>
        /// <returns>The state of those Candidates who have become winners and losers
        /// in this round of tabulation.  Returns an empty dictionary when tabulation is complete.</returns>
        Dictionary<Candidate, CandidateState.States> GetTabulation();

        /// <summary>
        /// Get winners and losers at the current round, with vote counts.
        /// </summary>
        /// <returns>A full set of candidate states and votes.</returns>
        Dictionary<Candidate, CandidateState> GetFullTabulation();


    }
}