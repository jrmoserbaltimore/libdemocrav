using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public interface IBatchEliminator : IUpdateTiebreaker
    {
        /// <summary>
        /// Get the set of elimination candidates.
        /// </summary>
        /// <param name="hopefuls">Vote counts for Candidates who have not been elected or eliminated.</param>
        /// <param name="elected">Number of elected candidates</param>
        /// <param name="surplus">Surplus votes.</param>
        /// <returns></returns>
        IEnumerable<Candidate> GetEliminationCandidates
           (Dictionary<Candidate, decimal> hopefuls, int elected, decimal surplus = 0.0m);
    }
}
