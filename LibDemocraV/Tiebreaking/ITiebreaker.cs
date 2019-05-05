using System;
using System.Collections.Generic;
using System.Text;
using MoonsetTechnologies.Voting.Analytics;

namespace MoonsetTechnologies.Voting.Tiebreaking
{
    public interface ITiebreaker : IUpdateTiebreaker
    {
        bool AllTiesBreakable { get; }
        /// <summary>
        /// Get all candidates who win the tie by this method.
        /// </summary>
        /// <param name="candidates">The tied candidates.</param>
        /// <returns></returns>
        IEnumerable<Candidate> GetTieWinners(IEnumerable<Candidate> candidates);
    }
}
