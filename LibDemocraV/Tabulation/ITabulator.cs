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
        /// Iterate through the next round of tabulation
        /// </summary>
        void TabulateRound();

        /// <summary>
        /// Retrieve the results.
        /// </summary>
        Dictionary<Candidate, CandidateState> GetResults();
    }
}