// Minimax and Smith/Minimax tabulator
// If you want to use this to elect multiple seats, elect one at a time
// and rerun with each candidate withdrawn.

using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Utility;
using MoonsetTechnologies.Voting.Analytics;

namespace MoonsetTechnologies.Voting.Tabulation
{
    // Only use Minimax as Smith/Minimax
    /// <inheritdoc/>
    [Export(typeof(AbstractTabulator))]
    [ExportMetadata("Algorithm", "alternative-vote")]
    [ExportMetadata("Factory", typeof(MinimaxTabulatorFactory))]
    [ExportMetadata("Title", "Minimax")]
    [ExportMetadata("Description", "Elects the candidate whose largest pairwise defeat " +
                    "is the smallest.  The Smith constraint is highly recommended, as " +
                    "Minimax can, in some cases, elect the Condorcet loser otherwise.")]
    [ExportMetadata("Settings", new[]
    {
        typeof(TiebreakerTabulatorSetting)
    })]
    // TODO:  support this, and support placing constraints on configuration options
    //[ExportMetadata("Constraints", new[] { "condorcet", "majority", "condorcet-loser",
    // "majority-loser", "mutual-majority", "smith", "isda", "polynomial-time", "resolvability",
    // "summability"})]

    public class MinimaxTabulator : AbstractPairwiseTabulator
    {
        protected bool SmithConstrain = false;
        /// <inheritdoc/>
        public MinimaxTabulator(TabulationMediator mediator,
            AbstractTiebreakerFactory tiebreakerFactory,
            IEnumerable<ITabulatorSetting> tabulatorSettings)
            : base(mediator, tabulatorSettings)
        {

        }

        protected override void CountBallot(CountedBallot ballot)
        {
            // Only counts hopeful and elected candidates
            Dictionary<Candidate, CandidateState> candidates
                = new Dictionary<Candidate, CandidateState>
                  (from x in candidateStates
                   where new[] { CandidateState.States.hopeful, CandidateState.States.elected, CandidateState.States.defeated }
                       .Contains(x.Value.State)
                   select x);
        }

        protected override IEnumerable<Candidate> GetEliminationCandidates()
        {
            HashSet<Candidate> ec = new HashSet<Candidate>();

            HashSet<Candidate> tc = new HashSet<Candidate>(
                topCycle.GetTopCycle(from x in candidateStates
                                     where x.Value.State != CandidateState.States.hopeful
                                     select x.Key, TopCycle.TopCycleSets.smith));

            // When Smith/Minimax OR there is a Condorcet winner,
            // eliminate all non-Smith candidates first
            if (SmithConstrain || (tc.Count() == 1))
            {
                ec.UnionWith(GetNonSmithCandidates());
            }

            // We're done if we found the Condorcet winner
            if (tc.Count() > 1)
            {
                Dictionary<Candidate, decimal> biggestLosses = new Dictionary<Candidate, decimal>();

                foreach (Candidate c in (from x in candidateStates
                                         where x.Value.State == CandidateState.States.hopeful
                                         select x.Key).Except(ec))
                {
                    biggestLosses[c] = (from x in pairwiseGraph.Losses(c)
                                        select pairwiseGraph.GetVoteCount(c, x))
                                       .Max(x => x.v2 - x.v1);
                }
                // Select everyone whose biggest loss is bigger than the smallest biggest loss
                ec.UnionWith(from x in biggestLosses
                             where x.Value > biggestLosses.Values.Min()
                             select x.Key);
            }

            return ec;
        }
    }
}