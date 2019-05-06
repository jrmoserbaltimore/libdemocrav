using System;
using System.Collections.Generic;
using System.Text;
using MoonsetTechnologies.Voting.Tabulation;

namespace MoonsetTechnologies.Voting.Tiebreaking
{
    public interface ITiebreaker : IUpdateTiebreaker
    {
        /// <summary>
        /// True if batch elimination will not affect tiebreakers occurring after batch elimination.
        /// </summary>
        bool FullyInformed { get; }
        /// <summary>
        /// Get all candidates who win the tie by this method.
        /// </summary>
        /// <param name="candidates">The tied candidates.</param>
        /// <returns></returns>
        IEnumerable<Candidate> GetTieWinners(IEnumerable<Candidate> candidates);
    }
}
