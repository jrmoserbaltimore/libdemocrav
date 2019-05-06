using System;
using System.Collections.Generic;
using System.Text;
using MoonsetTechnologies.Voting.Analytics;
using System.Linq;
using MoonsetTechnologies.Voting.Tabulation;

namespace MoonsetTechnologies.Voting.Tiebreaking
{
    public abstract class AbstractDifferenceTiebreaker : ITiebreaker
    {
        // If winPairs[a][b] = true, a wins a tie against b
        protected Dictionary<Candidate, Dictionary<Candidate, bool>> winPairs
            = new Dictionary<Candidate, Dictionary<Candidate, bool>>();

        protected bool allTiesBreakable = false;

        public bool FullyInformed => allTiesBreakable;

        public virtual IEnumerable<Candidate> GetTieWinners(IEnumerable<Candidate> candidates)
        {
            List<Candidate> winners = candidates.ToList();
            foreach (Candidate c in candidates)
            {
                foreach (Candidate d in candidates)
                {
                    if (winPairs.ContainsKey(c))
                    {
                        if (winPairs[c].ContainsKey(d))
                        {
                            if (winPairs[c][d])
                                winners.Remove(d);
                            else
                                winners.Remove(c);
                        }
                    }
                }
            }
            return winners;
        }

        public abstract void UpdateTiebreaker(Dictionary<Candidate, CandidateState> candidateStates);
    }
}