using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Utility;
using MoonsetTechnologies.Voting.Analytics;

namespace MoonsetTechnologies.Voting.Tabulation
{
    // Borda is terrible.  Never use it.
    public class BordaTabulator : AbstractTabulator
    {
        /// <inheritdoc/>
        public BordaTabulator(TabulationMediator mediator,
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
            int totalCandidates = candidates.Count;

            foreach (Vote v in ballot.Votes)
            {
                // Skip candidates not included in this count.
                if (!candidates.Keys.Contains(v.Candidate))
                    continue;
                // Add borda score, which for rank r and c candidates is c-r
                if (!(v is null))
                    candidateStates[v.Candidate].VoteCount += ballot.Count * (totalCandidates - v.Value);
            }
        }

        protected override TabulationStateEventArgs TabulateRound()
        {
            return new TabulationStateEventArgs
            {
                CandidateStates = CandidateStatesCopy,
                Note = ""
            };
        }
    }
}