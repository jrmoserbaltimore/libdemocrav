using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoonsetTechnologies.Voting.Analytics;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public class RankedVoteCount : AbstractRankedVoteCount
    {

        public RankedVoteCount(IEnumerable<Candidate> candidates, IEnumerable<IRankedBallot> ballots,
            IBatchEliminator batchEliminator, int seats = 1)
            : base(candidates, ballots, batchEliminator, seats)
        {

        }

        /// <inheritdoc/>
        public override void CountBallots()
        {
            Dictionary<Candidate, CandidateState> candidates
                = candidateStates
                   .Where(x => x.Value.State == CandidateState.States.hopeful
                               || x.Value.State == CandidateState.States.elected)
                   .ToDictionary(x => x.Key, x => x.Value);

            foreach (Candidate c in candidateStates.Keys)
                candidateStates[c].VoteCount = 0.0m;

            foreach (IRankedBallot b in ballots)
            {
                IRankedVote vote = null;
                foreach (IRankedVote v in b.Votes)
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
            }
        }

        /// <inheritdoc/>
        public override Dictionary<Candidate, CandidateState.States> GetTabulation()
        {
            Dictionary<Candidate, CandidateState> hopefuls =
                candidateStates.Where(x => x.Value.State == CandidateState.States.hopeful)
                .ToDictionary(x => x.Key, x => x.Value);

            Dictionary<Candidate, CandidateState> elected =
                candidateStates.Where(x => x.Value.State == CandidateState.States.hopeful)
                .ToDictionary(x => x.Key, x => x.Value);

            Dictionary <Candidate, CandidateState.States> result;

            // Fill remaining seats
            if (hopefuls.Count() + elected.Count() <= seats)
                result = hopefuls.ToDictionary(x => x.Key, x => x.Value.State);
            else
            {
                // Check for elimination
                result = batchEliminator.GetEliminationCandidates(
                      hopefuls.ToDictionary(x => x.Key, x => x.Value.VoteCount),
                      elected.Count)
                    .ToDictionary(x => x, x => CandidateState.States.defeated);
            }
            // No elimination, despite more candidats than seats?  It's broken.
            if (result.Count() == 0 && hopefuls.Count() + elected.Count() > seats)
                throw new InvalidOperationException();
            return result;
        }
    }
}
