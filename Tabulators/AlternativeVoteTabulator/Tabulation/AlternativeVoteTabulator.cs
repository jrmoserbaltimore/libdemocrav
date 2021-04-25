using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Tiebreaking;
using MoonsetTechnologies.Voting.Utility;

namespace MoonsetTechnologies.Voting.Tabulation
{
    /// <inheritdoc/>
    [Export(typeof(AbstractTabulator))]
    [ExportMetadata("Algorithm", "tideman-alternative")]
    [ExportMetadata("Factory", typeof(AlternativeVoteTabulatorFactory))]
    [ExportMetadata("Title", "Tideman's Alternative")]
    [ExportMetadata("Description", "Uses the Tideman's Alternative algorithm, which " +
        "elects a Condorcet candidate or, if no such candidate exists, eliminates all " +
        "candidates not in the top cycle, performs one round of runoff to eliminate " +
        "the candidate with the fewest first-preference votes, and repeats the whole " +
        "tabulation.")]
    [ExportMetadata("Settings", new[]
    {
        typeof(TiebreakerTabulatorSetting)
    })]
    //[ExportMetadata("Constraints", new[] { "condorcet", "majority", "condorcet-loser",
    // "majority-loser", "mutual-majority", "smith", "isda", "clone-independence",
    // "polynomial-time", "resolvability" })]

    // TODO:  Base on an abstract pairwise tabulator
    // TODO:  Include setting for Alternative-Smith (Tideman's Alternative)
    // TODO:  Include setting for Smith/IRV
    public class AlternativeVoteTabulator : AbstractTabulator
    {
        TopCycle.TopCycleSets condorcetSet = TopCycle.TopCycleSets.schwartz;
        TopCycle.TopCycleSets retainSet = TopCycle.TopCycleSets.smith;

        bool SmithConstrain = false;
        bool AlternativeSmith = false;

        public AlternativeVoteTabulator(TabulationMediator mediator,
            AbstractTiebreakerFactory tiebreakerFactory,
            IEnumerable<ITabulatorSetting> tabulatorSettings)
            : base(mediator, tiebreakerFactory, tabulatorSettings)
        {

        }

        protected override void InitializeTabulation(BallotSet ballots, IEnumerable<Candidate> withdrawn, int seats)
        {
            base.InitializeTabulation(ballots, withdrawn, seats);

            RankedTabulationAnalytics analytics;
            analytics = new RankedTabulationAnalytics(ballots, seats);

        }

        // Performs IRV count
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

        // Eliminates all non-Smith candidates, plus one more if there is no Condorcet winner
        protected override IEnumerable<Candidate> GetEliminationCandidates()
        {
            // TODO:  identify the Smith set
            if (AlternativeSmith && !SmithConstrain)
                throw new ApplicationException("AlternativeSmith and not Smith Constrained!");
            if (SmithConstrain)
            {
                // TODO:  Eliminate all non-Smith candidates
            }
            if (!AlternativeSmith)
                SmithConstrain = false;
            
            // TODO:  Check if the Smith set is one candidate; if not, append the lowest-vote
            // candidate to the elimination set
            decimal minVotes = candidateStates
                .Where(x => x.Value.State == CandidateState.States.hopeful)
                .Select(x => x.Value.VoteCount).Min();
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
