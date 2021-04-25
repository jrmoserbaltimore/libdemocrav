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
    public class MinimaxTabulator : AbstractTabulator
    {
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

            // TODO:  Create a graph
        }

        protected override TabulationStateEventArgs TabulateRound()
        {
            // TODO:  If Smith setting is enabled, eliminate all non-Smith candidates first
            // TODO:  Generate a list of largest pairwise defeats per candidate
            // TODO:  Elect candidate who has the smallest pairwise defeat
            return new TabulationStateEventArgs
            {
                CandidateStates = CandidateStatesCopy,
                Note = ""
            };
        }
    }
}