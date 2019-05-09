using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoonsetTechnologies.Voting.Ballots;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public class RankedTabulator : AbstractBasicTabulator
    {

        /// <inheritdoc/>
        public RankedTabulator(IEnumerable<Candidate> candidates, IEnumerable<Ballot> ballots,
            IBatchEliminator batchEliminator, int seats = 1)
            : base(candidates, ballots, batchEliminator, seats)
        {

        }

        // A simple count of who has the most votes.
        /// <inheritdoc/>
        protected override void CountBallots()
        {
            if (seats > 1)
                throw new ArgumentOutOfRangeException("seats", "This algorithm can't count multiple seats.");
            Dictionary<Candidate, CandidateState> candidates
                = candidateStates
                   .Where(x => x.Value.State == CandidateState.States.hopeful
                               || x.Value.State == CandidateState.States.elected)
                   .ToDictionary(x => x.Key, x => x.Value);

            foreach (Candidate c in candidateStates.Keys)
                candidateStates[c].VoteCount = 0.0m;

            foreach (Ballot b in ballots)
            {
                Vote vote = null;
                foreach (Vote v in b.Votes)
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
    }
}
