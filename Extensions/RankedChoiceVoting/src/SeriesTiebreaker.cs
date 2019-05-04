﻿using System;
using System.Collections.Generic;
using System.Text;
using MoonsetTechnologies.Voting.Analytics;
using System.Linq;

namespace MoonsetTechnologies.Voting.Tiebreakers
{
    /// <summary>
    /// Tests a series of tiebreakers, using the first that returns a sigle result.
    /// </summary>
    public class SeriesTiebreaker : ITiebreaker
    {
        private readonly IList<ITiebreaker> tiebreakers;
        public bool AllTiesBreakable
        {
            get
            {
                // If any one tiebreaker can break all ties, all ties are breakable
                foreach (ITiebreaker t in tiebreakers)
                {
                    if (t.AllTiesBreakable)
                        return true;
                }
                // All ties might be breakable, but we can't test that
                return false;
            }
        }

        public SeriesTiebreaker(IEnumerable<ITiebreaker> tiebreakers)
        {
            this.tiebreakers = tiebreakers.ToList();
        }

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

        public void UpdateTiebreaker<T>(Dictionary<Candidate, T> CandidateStates) where T : CandidateState
        {
            foreach (ITiebreaker t in tiebreakers)
                t.UpdateTiebreaker(CandidateStates);
        }
    }
}
