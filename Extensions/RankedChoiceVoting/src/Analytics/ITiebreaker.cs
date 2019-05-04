using System;
using System.Collections.Generic;
using System.Text;
using MoonsetTechnologies.Voting.Analytics;

namespace MoonsetTechnologies.Voting.Tiebreakers
{
    public interface ITiebreaker
    {
        bool AllTiesBreakable { get; }
        /// <summary>
        /// Update tiebreaker based on vote counts at this round
        /// </summary>
        /// <param name="CandidateStates">Candidate state information.</param>
        void UpdateTiebreaker<T>(Dictionary<Candidate, T> CandidateStates) where T : CandidateState;
        /// <summary>
        /// Get all candidates who win the tie by this method.
        /// </summary>
        /// <param name="candidates">The tied candidates.</param>
        /// <returns></returns>
        IEnumerable<Candidate> GetTieWinners(IEnumerable<Candidate> candidates);
    }
}
