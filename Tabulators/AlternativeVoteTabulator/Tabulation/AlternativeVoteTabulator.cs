using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Utility;

namespace MoonsetTechnologies.Voting.Tabulation
{
    /// <inheritdoc/>
    [Export(typeof(AbstractTabulator))]
    [ExportMetadata("Algorithm", "alternative-vote")]
    [ExportMetadata("Factory", typeof(AlternativeVoteTabulatorFactory))]
    [ExportMetadata("Title", "Alternative Vote")]
    [ExportMetadata("Description", "Uses the Instant Runoff Voting, Smith/IRV, or " +
        " Alternative Smith algorithm, which elect by eliminating the candidate " +
        " with the fewest first-preference votes until one has a simple majority.  " +
        " Smith/IRV eliminates all non-Smith candidates first; Alternative Smith " +
        " eliminates all non-Smith candidates before each runoff elimination.")]
    [ExportMetadata("Settings", new[]
    {
        typeof(SmithConstrainedTabulatorSetting),
        typeof(AlternativeVoteAlternativeSmithSetting),
        typeof(AlternativeVoteGellerSetting)
    })]
    //[ExportMetadata("Constraints", new[] { "condorcet", "majority", "condorcet-loser",
    // "majority-loser", "mutual-majority", "smith", "isda", "clone-independence",
    // "polynomial-time", "resolvability" })]
    public class AlternativeVoteTabulator : AbstractPairwiseTabulator
    {
        bool SmithConstrain = false;
        bool AlternativeSmith = false;
        bool GellerElimination = false;

        public AlternativeVoteTabulator(TabulationMediator mediator,
            AbstractTiebreakerFactory tiebreakerFactory,
            IEnumerable<ITabulatorSetting> tabulatorSettings)
            : base(mediator, tabulatorSettings)
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
            decimal totalCandidates = (from x in candidateStates
                                       where x.Value.State != CandidateState.States.withdrawn
                                       select x).Count();
            foreach (Vote v in ballot.Votes)
            {
                // Compute the Borda score
                (candidateStates[vote.Candidate] as GellerCandidateState).BordaScore +=
                     totalCandidates - v.Value;
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

            HashSet<Candidate> tc = new HashSet<Candidate>(
                topCycle.GetTopCycle(from x in candidateStates
                                     where x.Value.State != CandidateState.States.hopeful
                                     select x.Key, TopCycle.TopCycleSets.smith));

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
                Candidate winner = (from x in candidateStates
                                    where x.Value.VoteCount > new decimal((totalCount / 2) + 0.5)
                                    select x.Key).First();
                // if we have a simple majority winner, eliminate everyone else
                if (!(winner is null))
                {
                    ec.UnionWith(from x in candidateStates
                                 where x.Value.State == CandidateState.States.hopeful
                                 where x.Key != winner
                                 select x.Key);
                }
                else
                {
                    if (GellerElimination)
                    {
                        decimal minBorda = (from x in candidateStates
                                            where x.Value.State == CandidateState.States.hopeful
                                            select (x.Value as GellerCandidateState).BordaScore)
                                           .Min();
                        ec.UnionWith(from x in candidateStates
                                     where (x.Value as GellerCandidateState).BordaScore == minBorda
                                     where x.Value.State == CandidateState.States.hopeful
                                     select x.Key);
                    }
                    else
                    {
                        decimal minVotes = (from x in candidateStates
                                            where x.Value.State == CandidateState.States.hopeful
                                            select x.Value.VoteCount).Min();
                        ec.UnionWith(from x in candidateStates
                                     where x.Value.State == CandidateState.States.hopeful
                                     where x.Value.VoteCount == minVotes
                                     select x.Key);
                    }
                }
            }
            return ec;
        }
    }
}
