using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoonsetTechnologies.Voting.Tiebreaking;

namespace MoonsetTechnologies.Voting.Analytics
{
    public interface IVoteCount : IBatchEliminator, IUpdateTiebreaker
    {
        /// <summary>
        /// Gets the count of votes for a candidate.
        /// </summary>
        /// <param name="c">The candidate whose votes to count</param>
        /// <returns>The number of votes received.</returns>
        decimal GetVoteCount(Candidate c);
        /// <summary>
        /// Get the count of votes for all candidates.
        /// </summary>
        /// <returns>The number of votes each candidate receives.</returns>
        Dictionary<Candidate, decimal> GetVoteCounts();
    }
  
}
