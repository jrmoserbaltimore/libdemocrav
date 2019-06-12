using System;
using System.Collections.Generic;
using System.Text;
using MoonsetTechnologies.Voting.Analytics;
using System.Linq;
using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Utility;

namespace MoonsetTechnologies.Voting.Tiebreaking
{

    public class FirstDifferenceTiebreaker : AbstractDifferenceTiebreaker
    {
        public FirstDifferenceTiebreaker(AbstractTiebreaker tiebreaker = null)
            : base(tiebreaker)
        {
        }
        public FirstDifferenceTiebreaker() : this(null)
        {
        }

        /// <inheritdoc/>
        protected override void UpdateTiebreaker(Dictionary<Candidate, CandidateState> candidateStates)
        {
            bool allFirstDifferences = true;

            foreach (Candidate c in candidateStates.Keys)
            {
                foreach (Candidate d in candidateStates.Keys)
                {
                    // Update winPairs only where all prior rounds have been ties.
                    if (!winPairs.ContainsKey(c))
                        winPairs[c] = new Dictionary<Candidate, bool>();
                    if (!winPairs[c].ContainsKey(d))
                    {
                        // If it's a tie, we can't break this tie yet
                        if (candidateStates[c].VoteCount == candidateStates[d].VoteCount)
                            allFirstDifferences = false;
                        else
                            winPairs[c][d] =
                                (candidateStates[c].VoteCount > candidateStates[d].VoteCount);
                    }
                }
            }
            allTiesBreakable = allFirstDifferences;
        }
    }
}
