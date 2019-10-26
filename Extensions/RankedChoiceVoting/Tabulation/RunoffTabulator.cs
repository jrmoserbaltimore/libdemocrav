using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Utility;
using MoonsetTechnologies.Voting.Analytics;


// XXX
// This tabulator is implemented simply by API that doesn't even exist.
// Refactor everything else to match.
// XXX

namespace MoonsetTechnologies.Voting.Tabulation
{

    //
    // 1. If Smith/IRV or Tideman's, eliminate all non-Smith candidates;
    // 2. If only one candidate remains, go to 6;
    // 3. Eliminate the remaining candidate with the fewest votes;
    // 4. If Tideman’s Alternative, go to 1;
    // 5. If Instant Runoff Voting, go to 2;
    // 6. Elect the single remaining candidate.
    //
    public class RunoffTabulator : AbstractTabulator
    {
        private CondorcetCheck condorcetCheck = CondorcetCheck.none;
        private AbstractBatchEliminator condorcetBatcheliminator = null;

        TopCycle.TopCycleSets condorcetSet = TopCycle.TopCycleSets.schwartz;
        TopCycle.TopCycleSets retainSet = TopCycle.TopCycleSets.smith;

        public enum CondorcetCheck
        {
            // no Condorcet checks
            none = 0,
            // Check on first round, restrict to retainSet
            start = 1,
            // Check after each single-candidate runoff
            runoff = 2
        }

        /// <inheritdoc/>
        public RunoffTabulator(TabulationMediator mediator,
            AbstractTiebreakerFactory tiebreakerFactory,
            IEnumerable<ITabulatorSetting> tabulatorSettings)
            : base(mediator, tiebreakerFactory, tabulatorSettings)
        {

        }

        /// <inheritdoc/>
        protected override void InitializeTabulation(BallotSet ballots, IEnumerable<Candidate> withdrawn, int seats)
        {
            base.InitializeTabulation(ballots, withdrawn, seats);
            analytics = new RankedTabulationAnalytics(ballots, seats);
            batchEliminator = new RunoffBatchEliminator(analytics as RankedTabulationAnalytics, seats);
            if (condorcetCheck == CondorcetCheck.start)
                condorcetBatcheliminator = new TopCycleBatchEliminator(analytics as RankedTabulationAnalytics, seats);
        }

        // A simple count of who has the most votes.
        /// <inheritdoc/>
        protected override void CountBallot(CountedBallot ballot)
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
                candidateStates[vote.Candidate].VoteCount += ballot.Count;
            else
            {
                // FIXME:  Send an exhausted ballot event for counting purposes
            }
        }

        protected override IEnumerable<Candidate> GetEliminationCandidates()
        {
            HashSet<Candidate> retainedCandidates = null;
            HashSet<Candidate> eliminatedCandidates;

            // Get the Condorcet candidate
            if (!(condorcetSet is null))
            {
                retainedCandidates = analytics.GetCandidates(condorcetSet);
            }

            if (!(retainSet is null))
            {
                // There is no Condorcet candidate
                if (retainedCandidates is null || retainedCandidates.Count > 1)
                {
                    retainedCandidates = analytics.GetCandidates(retainSet);
                }
                eliminatedCandidates = analytics.GetCandidates(electableCandidates).Except(retainedCandidates);
            }
            else
                eliminatedCandidates = new HashSet<Candidate>();

            // All candidates are in the retained set, so use one round of runoff
            if (eliminatedCandidates.Count == 0)
            {
                eliminatedCandidates = analytics.GetCandidates(runoffLoser);

                // Break ties
                if (eliminatedCandidates.Count > 1)
                {

                }
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

                eliminationCandidates = GetEliminationCandidates();
                if (!(eliminationCandidates?.Count() > 0))
                    throw new InvalidOperationException("Called TabulateRound() after completion.");
                foreach (Candidate c in eliminationCandidates)
                    SetState(c, CandidateState.States.defeated);
            }
            PairwiseGraph pairwiseGraph = new PairwiseGraph(ballots);
            return new RankedTabulationStateEventArgs
            {
                CandidateStates = CandidateStatesCopy,
                SchwartzSet = (analytics as RankedTabulationAnalytics).GetSchwartzSet(candidateStates.Keys.Except(startSet)),
                SmithSet = (analytics as RankedTabulationAnalytics).GetSmithSet(candidateStates.Keys.Except(startSet)),
                PairwiseGraph = pairwiseGraph
            };
        }
    }
}
