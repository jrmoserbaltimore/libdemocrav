using MoonsetTechnologies.Voting.Analytics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Tiebreakers
{
    public class LastDifference : AbstractDifference
    {
        public LastDifference()
        {
        }

        public override void UpdateTiebreaker<T>(Dictionary<Candidate, T> CandidateStates)
        {
            bool allLastDifferences = true;
            foreach (Candidate c in CandidateStates.Keys)
            {
                foreach (Candidate d in CandidateStates.Keys)
                {
                    // Update winPairs whenever not a pairwise tie
                    if (CandidateStates[c].VoteCount != CandidateStates[d].VoteCount)
                        winPairs[c][d] =
                          (CandidateStates[c].VoteCount > CandidateStates[d].VoteCount);
                    // Can't break this tie
                    if (!winPairs[c].ContainsKey(d))
                        allLastDifferences = false;
                }
            }
            allTiesBreakable = allLastDifferences;
        }
    }
}
