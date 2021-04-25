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
    [ExportMetadata("Algorithm", "alternative-vote")]
    [ExportMetadata("Factory", typeof(AlternativeVoteTabulatorFactory))]
    [ExportMetadata("Title", "Alternative Vote")]
    [ExportMetadata("Description", "Uses the Instant Runoff Voting, Smith/IRV, or " +
        " Alternative  Smithalgorithm, which elect by eliminating the candidate " +
        " with the fewest first-preference votes until one has a simple majority.  " +
        " Smith/IRV eliminates all non-Smith candidates first; Alternative Smith " +
        " eliminates all non-Smith candidates before each runoff elimination.")]
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
    public class AlternativeVoteTabulator : AbstractPairwiseTabulator
    {
        bool SmithConstrain = false;
        bool AlternativeSmith = false;

        public AlternativeVoteTabulator(TabulationMediator mediator,
            AbstractTiebreakerFactory tiebreakerFactory,
            IEnumerable<ITabulatorSetting> tabulatorSettings)
            : base(mediator, tiebreakerFactory, tabulatorSettings)
        {

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
            HashSet<Candidate> ec = new HashSet<Candidate>();

            HashSet<Candidate> tc = new HashSet<Candidate>(topCycle.GetTopCycle(candidateStates
                .Where(x => x.Value.State != CandidateState.States.hopeful)
                .Select(x => x.Key), TopCycle.TopCycleSets.smith));

            long totalCount = ballots.TotalCount();

            // First, eliminate all non-Smith candidates
            if (AlternativeSmith && !SmithConstrain)
                throw new ApplicationException("AlternativeSmith and not Smith Constrained!");
            if (SmithConstrain)
            {
                ec.UnionWith(GetNonSmithCandidates());
            }
            if (!AlternativeSmith)
                SmithConstrain = false;

            // If all were in the Smith set, do eliminations
            if (ec.Count == 0)
            {
                Candidate winner = candidateStates
                    .Where(x => x.Value.VoteCount > new decimal((totalCount / 2) + 0.5))
                    .Select(x => x.Key)
                    .First();
                // if we have a simple majority winner, eliminate everyone else
                if (!(winner is null))
                {
                    ec.UnionWith(candidateStates
                        .Where(x => x.Value.State == CandidateState.States.hopeful)
                        .Select(x => x.Key)
                        .Where(x => x != winner));
                }
                else
                {
                    decimal minVotes = candidateStates
                        .Where(x => x.Value.State == CandidateState.States.hopeful)
                        .Where(x => !ec.Contains(x.Key))
                        .Select(x => x.Value.VoteCount).Min();
                    ec.UnionWith(candidateStates
                        .Where(x => x.Value.State == CandidateState.States.hopeful)
                        .Where(x => !ec.Contains(x.Key))
                        .Where(x => x.Value.VoteCount == minVotes)
                        .Select(x => x.Key));
                }
            }
            return ec;
        }
    }
}
