using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoonsetTechnologies.Voting.Tiebreaking;

namespace MoonsetTechnologies.Voting.Analytics
{
    public class RankedVoteCount : AbstractVoteCount
    {
        private readonly List<Candidate> Candidates;
        private readonly List<IRankedBallot> Ballots;

        public RankedVoteCount(IEnumerable<Candidate> candidates, IEnumerable<IRankedBallot> ballots,
            ITiebreaker tiebreaker, IBatchEliminator batchEliminator)
            : base(tiebreaker, batchEliminator)
        {
            Candidates = new List<Candidate>(candidates);
            Ballots = new List<IRankedBallot>(ballots);
        }

        /// <inheritdoc/>
        public override decimal GetVoteCount(Candidate c)
        {
            decimal count = 0.0m;
            foreach (IRankedBallot b in Ballots)
            {
                IRankedVote vote = null;
                foreach (IRankedVote v in b.Votes)
                {
                    // Skip candidates not included in this count.
                    if (!Candidates.Contains(v.Candidate))
                        continue;
                    // First vote examined or it beats current
                    if (vote is null || v.Beats(vote))
                        vote = v;
                }
                if (!(vote is null) && vote.Candidate == c)
                    count++;
            }
            return count;
        }

        /// <inheritdoc/>
        public override Dictionary<Candidate, decimal> GetVoteCounts()
        {
            Dictionary<Candidate, decimal> vc = new Dictionary<Candidate, decimal>();
            foreach (Candidate c in Candidates)
                vc[c] = GetVoteCount(c);
            return vc;
        }

        public Candidate GetLeastVotedCandidate()
        {
            Dictionary<Candidate, decimal> vc = GetVoteCounts();
            Candidate output = null;
            foreach(Candidate c in vc.Keys)
            {
                if (output is null || vc[c] < vc[output])
                    output = c;
            }
            return output;
        }


        /// <summary>
        /// Gets the top (count) candidates, such as for Top-2, by plurality vote.
        /// </summary>
        /// <param name="count">Number of candidates to retrieve.</param>
        /// <returns>A list of the top (count) candidates. If there are ties for
        /// last place, all such ties will be included, which may return more than
        /// (count).</returns>
        public IEnumerable<Candidate> GetTopCandidates(int count)
        {
            Dictionary<Candidate, decimal> vc = GetVoteCounts();
            foreach (Candidate c in vc.Keys)
            {
                int j = 0;
                foreach (Candidate d in vc.Keys)
                {
                    // Found someone with more votes than (c)
                    if (vc[d] > vc[c])
                        j++;
                    // Found (count) candidates with more votes than (c)
                    // so remove (c) from the top candidates
                    if (j > count)
                    {
                        vc.Remove(c);
                        break;
                    }
                }
            }
            return vc.Keys;
        }
    }
}
