using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public abstract class AbstractBatchEliminator
    {
        protected readonly AbstractTabulationAnalytics analytics;
        protected readonly int seats;

        public AbstractBatchEliminator(AbstractTabulationAnalytics analytics, int seats = 1)
        {
            this.analytics = analytics;
            this.seats = seats;
        }

        /// <summary>
        /// Get the set of elimination candidates.
        /// </summary>
        /// <param name="candidateStates">CandidateState at the current tabulation and vote count.</param>
        /// <param name="surplus">Surplus votes for systems that use this.</param>
        /// <returns>An IEnumerable of Candidates to eliminate, or null if a tie.</returns>
        public abstract IEnumerable<Candidate> GetBatchElimination(Dictionary<Candidate, CandidateState> candidateStates,
            decimal surplus = 0.0m);

        /// <summary>
        /// Get a single elimination candidate, or all tied for elimination.
        /// </summary>
        /// <param name="candidateStates">CandidateState at the current tabulation and vote count.</param>
        /// <param name="surplus">Surplus votes for systems that use this.</param>
        /// <returns>An IEnumerable of Candidates to eliminate.</returns>
        public abstract IEnumerable<Candidate> GetSingleElimination(Dictionary<Candidate, CandidateState> candidateStates,
            decimal surplus = 0.0m);
    }
}
