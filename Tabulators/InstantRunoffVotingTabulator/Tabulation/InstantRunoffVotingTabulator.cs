using System;
using System.Collections.Generic;
using System.Linq;
using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Tiebreaking;
using MoonsetTechnologies.Voting.Utility;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public class InstantRunoffVotingTabulator : AbstractTabulator
    {
        /// <inheritdoc/>
        public InstantRunoffVotingTabulator(TabulationMediator mediator,
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
                   .Where(x => new[] { CandidateState.States.hopeful, CandidateState.States.elected }
                     .Contains(x.Value.State))
                   .ToDictionary(x => x.Key, x => x.Value);

            Vote vote = null;
            foreach (Vote v in ballot.Votes)
            {
                // Skip candidates not included in this count.
                if (!candidates.Keys.Contains(v.Candidate))
                    continue;
                // First vote examined or it beats current
                if (vote is null || v.Beats(vote))
                    vote = v;
            }
            if (!(vote is null))
                candidateStates[vote.Candidate].VoteCount += ballot.Count;
            else
            {
                // TODO:  Send an exhausted ballot event for counting purposes
            }
        }
        // Finds the candidates with the lowest vote count
        protected override IEnumerable<Candidate> GetEliminationCandidates()
        {
            decimal minVotes = candidateStates.Select(x => x.Value.VoteCount).Min();
            return candidateStates.Where(x => x.Value.VoteCount == minVotes).Select(x => x.Key);
        }

        protected override TabulationStateEventArgs TabulateRound()
        {
            long tc = ballots.TotalCount();

            Candidate winner = candidateStates.Where(x => x.Value.VoteCount > new decimal((tc / 2) + 0.5)).Select(x => x.Key).First();

            if (!(winner is null))
            {
                SetState(winner, CandidateState.States.elected);
            }
            else
            {
                foreach (Candidate c in GetEliminationCandidates())
                {
                    SetState(c, CandidateState.States.defeated);
                }
            }
            return new TabulationStateEventArgs
            {
                CandidateStates = CandidateStatesCopy,
                Note = ""
            };
        }
    }
}
