using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Tabulation;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Tiebreaking
{
    public class LastDifferenceTiebreaker : AbstractDifferenceTiebreaker
    {
        public LastDifferenceTiebreaker()
        {
        }

        public override void UpdateTiebreaker(Dictionary<Candidate, CandidateState> candidateStates)
        {
            bool allLastDifferences = true;
            foreach (Candidate c in candidateStates.Keys)
            {
                foreach (Candidate d in candidateStates.Keys)
                {
                    // Update winPairs whenever not a pairwise tie
                    if (candidateStates[c].VoteCount != candidateStates[d].VoteCount)
                        winPairs[c][d] =
                          (candidateStates[c].VoteCount > candidateStates[d].VoteCount);
                    // Can't break this tie
                    if (!winPairs[c].ContainsKey(d))
                        allLastDifferences = false;
                }
            }
            allTiesBreakable = allLastDifferences;
        }
    }
}
