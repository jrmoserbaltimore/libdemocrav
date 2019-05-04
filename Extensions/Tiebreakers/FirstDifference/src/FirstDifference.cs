using System;
using System.Collections.Generic;
using System.Text;
using MoonsetTechnologies.Voting.Analytics;
using System.Linq;

namespace MoonsetTechnologies.Voting.Tiebreakers
{
    public class FirstDifference : AbstractDifference
    {

        public FirstDifference()
        {
        }

        public override void UpdateTiebreaker<T>(Dictionary<Candidate, T> CandidateStates)
        {
            bool allFirstDifferences = true;

            foreach (Candidate c in CandidateStates.Keys)
            {
                foreach (Candidate d in CandidateStates.Keys)
                {
                    // Update winPairs only where all prior rounds have been ties.
                    if (!winPairs[c].ContainsKey(d))
                    {
                        // If it's a tie, we can't break this tie yet
                        if (CandidateStates[c].VoteCount == CandidateStates[d].VoteCount)
                            allFirstDifferences = false;
                        else
                            winPairs[c][d] =
                                (CandidateStates[c].VoteCount > CandidateStates[d].VoteCount);
                    }
                }
            }
            allTiesBreakable = allFirstDifferences;
        }
    }
}
