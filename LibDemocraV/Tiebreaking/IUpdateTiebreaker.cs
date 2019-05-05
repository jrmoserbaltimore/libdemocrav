using System;
using System.Collections.Generic;
using System.Text;
using MoonsetTechnologies.Voting.Analytics;

namespace MoonsetTechnologies.Voting.Tiebreaking
{
    public interface IUpdateTiebreaker
    {
        /// <summary>
        /// Update tiebreaker based on vote counts at this round
        /// </summary>
        /// <param name="CandidateStates">Candidate state information.</param>
        void UpdateTiebreaker<T>(Dictionary<Candidate, T> CandidateStates) where T : CandidateState;
    }
}
