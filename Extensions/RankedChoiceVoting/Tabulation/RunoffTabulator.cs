using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Utility;
using MoonsetTechnologies.Voting.Analytics;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public class RunoffTabulator : AbstractTabulator
    {

        /// <inheritdoc/>
        public RunoffTabulator(TabulationMediator mediator,
            AbstractTiebreakerFactory tiebreakerFactory)
            : base(mediator, tiebreakerFactory)
        {

        }

        protected override void InitializeTabulation(IEnumerable<Ballot> ballots, IEnumerable<Candidate> withdrawn, int seats)
        {
            RankedTabulationAnalytics a;
            a = new RankedTabulationAnalytics(ballots, seats);
            analytics = a;
            batchEliminator = new RunoffBatchEliminator(tiebreakerFactory.CreateTiebreaker(mediator), a, seats);
            this.seats = seats;
        }

        // A simple count of who has the most votes.
        /// <inheritdoc/>
        protected override void CountBallot(Ballot ballot)
        {
            // Only counts ballots for hopeful and elected candidates
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
                candidateStates[vote.Candidate].VoteCount += 1.0m;
            else
            {
                // FIXME:  Send an exhausted ballot event for counting purposes
            }
        }

        /// <inheritdoc/>
        protected override TabulationStateEventArgs TabulateRound()
        {
            IEnumerable<Candidate> startSet =
                candidateStates.
                Where(x => new[] { CandidateState.States.hopeful, CandidateState.States.elected }
                     .Contains(x.Value.State)).Select(x => x.Key).ToList();

            IEnumerable<Candidate> eliminationCandidates;

            if (IsComplete())
                throw new InvalidOperationException("Called TabulateRound() after completion.");
            // If we're done, there will be only enough hopefuls to fill remaining seats
            if (IsFinalRound())
                SetFinalWinners();
            else
            {
                mediator.UpdateTiebreaker(CandidateStatesCopy);
                eliminationCandidates = batchEliminator.GetEliminationCandidates(CandidateStatesCopy);
                if (!(eliminationCandidates?.Count() > 0))
                    throw new InvalidOperationException("Called TabulateRound() after completion.");
                foreach (Candidate c in eliminationCandidates)
                    SetState(c, CandidateState.States.defeated);

            }
            PairwiseGraph pairwiseGraph = new PairwiseGraph(startSet, ballots);
            return new RankedTabulationStateEventArgs
            {
                CandidateStates = CandidateStatesCopy,
                SchwartzSet = (analytics as RankedTabulationAnalytics).GetSchwartzSet(startSet),
                SmithSet = (analytics as RankedTabulationAnalytics).GetSmithSet(startSet),
                PairwiseGraph = pairwiseGraph
            };
        }
    }
}
