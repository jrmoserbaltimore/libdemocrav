using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Utility;
using MoonsetTechnologies.Voting.Analytics;

namespace MoonsetTechnologies.Voting.Tabulation
{
    // Only use Minimax as Smith/Minimax
    // TODO:  base on an abstract pairwise tabulator that initializes the graph
    public class MinimaxTabulator : AbstractPairwiseTabulator
    {
        protected bool SmithConstrain = false;
        /// <inheritdoc/>
        public MinimaxTabulator(TabulationMediator mediator,
            AbstractTiebreakerFactory tiebreakerFactory,
            IEnumerable<ITabulatorSetting> tabulatorSettings)
            : base(mediator, tiebreakerFactory, tabulatorSettings)
        {

        }

        protected override void CountBallot(CountedBallot ballot)
        {
            // Only counts hopeful and elected candidates
            Dictionary<Candidate, CandidateState> candidates
                = candidateStates
                   .Where(x => new[] { CandidateState.States.hopeful, CandidateState.States.elected, CandidateState.States.defeated }
                     .Contains(x.Value.State))
                   .ToDictionary(x => x.Key, x => x.Value);
        }

        protected override IEnumerable<Candidate> GetEliminationCandidates()
        {
            HashSet<Candidate> ec = new HashSet<Candidate>();

            HashSet <Candidate> tc = new HashSet<Candidate>(topCycle.GetTopCycle(candidateStates
                .Where(x => x.Value.State != CandidateState.States.hopeful)
                .Select(x => x.Key), TopCycle.TopCycleSets.smith));

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

                foreach (Candidate c in candidateStates
                    .Where(x => x.Value.State == CandidateState.States.hopeful)
                    .Select(x => x.Key)
                    .Except(ec))
                {
                    biggestLosses[c] = pairwiseGraph.Losses(c)
                        .Select(x => pairwiseGraph.GetVoteCount(c, x))
                        .Max(x => x.v2 - x.v1);
                }
                // Select everyone whose biggest loss is bigger than the smallest biggest loss
                ec.UnionWith(biggestLosses
                    .Where(x => x.Value > biggestLosses.Values.Min())
                    .Select(x => x.Key));
            }

            return ec;
        }
    }
}