using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Composition;

namespace MoonsetTechnologies.Voting.Tiebreaking
{
    /// <inheritdoc/>
    [Export(typeof(AbstractTiebreaker))]
    [ExportMetadata("Algorithm","last-difference")]
    [ExportMetadata("Factory", typeof(TiebreakerFactory<LastDifferenceTiebreaker>))]
    [ExportMetadata("Title","Last Difference")]
    [ExportMetadata("Description", "Last Difference selects a winner between any "
        + "two tied candidates based on which candidate had more votes at the end of "
        + "the last round in which the candidates were not tied.")]
    public class LastDifferenceTiebreaker : AbstractDifferenceTiebreaker
    {
        public LastDifferenceTiebreaker(AbstractTiebreaker tiebreaker = null)
            : base(tiebreaker)
        {
        }

        public LastDifferenceTiebreaker() : this(null)
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
