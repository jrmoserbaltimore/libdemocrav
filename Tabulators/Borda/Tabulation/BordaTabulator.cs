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
            IEnumerable<ITabulatorSetting> tabulatorSettings)
            : base(mediator, tabulatorSettings)
        {

        }
        protected override void CountBallot(CountedBallot ballot)
        {
            // Only counts hopeful and elected candidates
            HashSet<Candidate> candidates
                = new HashSet<Candidate>(
                    from x in candidateStates
                    where new[] { CandidateState.States.hopeful, CandidateState.States.elected, CandidateState.States.defeated }
                      .Contains(x.Value.State)
                    select x.Key);

            foreach (Vote v in ballot.Votes)
            {
                // Skip candidates not included in this count.
                if (!candidates.Contains(v.Candidate))
                    continue;
                // Add borda score, which for rank r and c candidates is c-r
                if (!(v is null))
                    candidateStates[v.Candidate].VoteCount += ballot.Count * (candidates.Count - v.Value);
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