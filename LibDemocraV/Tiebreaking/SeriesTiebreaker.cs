using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoonsetTechnologies.Voting.Tabulation;

namespace MoonsetTechnologies.Voting.Tiebreaking
{
    /// <summary>
    /// Tests a series of tiebreakers, using the first that returns a sigle result.
    /// </summary>
    public class SeriesTiebreaker : ITiebreaker
    {
        private readonly IList<ITiebreaker> tiebreakers;
        // Not fully informed until the first tiebreaker is fully informed.
        /// <inheritdoc/>
        public bool FullyInformed => tiebreakers.First().FullyInformed;

        public SeriesTiebreaker(IEnumerable<ITiebreaker> tiebreakers)
        {
            this.tiebreakers = tiebreakers.ToList();
        }

        /// <inheritdoc/>
        public IEnumerable<Candidate> GetTieWinners(IEnumerable<Candidate> candidates)
        {
            IList<Candidate> winners = null;
            foreach (ITiebreaker t in tiebreakers)
            {
                winners = t.GetTieWinners(candidates).ToList();
                // A single winner isn't a tie, so we're done
                if (winners.Count == 1)
                    break;
            }
            return winners;
        }

        /// <inheritdoc/>
        public void UpdateTiebreaker(Dictionary<Candidate, CandidateState> CandidateStates)
        {
            foreach (ITiebreaker t in tiebreakers)
                t.UpdateTiebreaker(CandidateStates);
        }
    }
}
