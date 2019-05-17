using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Tiebreaking
{
    [TiebreakerTypeId("e7b8e618-1fee-4552-8b5b-b0616a90f03c")]
    public class LastDifferenceTiebreaker : AbstractDifferenceTiebreaker
    {
        public LastDifferenceTiebreaker() : base()
        {
        }

        /// <inheritdoc/>
        protected override void UpdateTiebreaker(Dictionary<Candidate, CandidateState> candidateStates)
        {
            bool allLastDifferences = true;
            foreach (Candidate c in candidateStates.Keys)
            {
                foreach (Candidate d in candidateStates.Keys)
                {
                    if (c == d)
                        continue;
                    // Update winPairs whenever not a pairwise tie
                    if (candidateStates[c].VoteCount != candidateStates[d].VoteCount)
                    {
                        if (!winPairs.ContainsKey(c))
                            winPairs[c] = new Dictionary<Candidate, bool>();
                        winPairs[c][d] =
                          (candidateStates[c].VoteCount > candidateStates[d].VoteCount);
                    }
                    // Can't break this tie
                    else if (!(winPairs.ContainsKey(c) && winPairs[c].ContainsKey(d)))
                        allLastDifferences = false;
                }
            }
            allTiesBreakable = allLastDifferences;
        }
    }
}
