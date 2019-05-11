using System;
using System.Collections.Generic;
using System.Text;
using MoonsetTechnologies.Voting.Analytics;
using System.Linq;
using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Utility;

namespace MoonsetTechnologies.Voting.Tiebreaking
{
    public abstract class AbstractDifferenceTiebreaker : AbstractTiebreaker
    {
        // If winPairs[a][b] = true, a wins a tie against b
        protected Dictionary<Candidate, Dictionary<Candidate, bool>> winPairs
            = new Dictionary<Candidate, Dictionary<Candidate, bool>>();

        protected bool allTiesBreakable = false;

        public AbstractDifferenceTiebreaker() : base()
        {
        }

        public override bool FullyInformed => allTiesBreakable;
        protected void SetWinPair(Candidate winner, Candidate loser)
        {
            if (!winPairs.ContainsKey(winner))
                winPairs[winner] = new Dictionary<Candidate, bool>();
            winPairs[winner][loser] = true;
            if (!winPairs.ContainsKey(loser))
                winPairs[loser] = new Dictionary<Candidate, bool>();
            winPairs[loser][winner] = false;
        }

        /// <inheritdoc/>
        protected override Dictionary<Candidate, Dictionary<Candidate, bool>> GetWinPairs()
        {
            // Make a copy
            return winPairs.ToDictionary(x => x.Key, x => x.Value.ToDictionary(y => y.Key, y => y.Value));
        }

        /// <inheritdoc/>
        public override IEnumerable<Candidate> GetTieWinners(IEnumerable<Candidate> candidates)
        {
            List<Candidate> winners = candidates.ToList();
            // Check each [c][d] pair—in both orders—and eliminate the loser
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
                        }
                        else
                            // We've seen c but nobody has yet voted for d at all
                            winners.Remove(d);
                    }
                }
            }
            return winners;
        }

       
    }
}