using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Analytics
{
    public interface IVoteCount
    {
        /// <summary>
        /// Gets the count of votes for a candidate.
        /// </summary>
        /// <param name="c">The candidate whose votes to count</param>
        /// <returns>The number of votes received.</returns>
        int GetVoteCount(Candidate c);
        /// <summary>
        /// Get the count of votes for all candidates.
        /// </summary>
        /// <returns>The number of votes each candidate receives.</returns>
        Dictionary<Candidate, int> GetVoteCounts();
    }
    public class RankedVoteCount : IVoteCount
    {
        private readonly List<Candidate> Candidates;
        private readonly List<IRankedBallot> Ballots;

        public RankedVoteCount(IEnumerable<Candidate> candidates, IEnumerable<IRankedBallot> ballots)
        {
            Candidates = new List<Candidate>(candidates);
            Ballots = new List<IRankedBallot>(ballots);
        }

        /// <inheritdoc/>
        public int GetVoteCount(Candidate c)
        {
            int count = 0;
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
        public Dictionary<Candidate, int> GetVoteCounts()
        {
            Dictionary<Candidate, int> vc = new Dictionary<Candidate, int>();
            foreach (Candidate c in Candidates)
                vc[c] = GetVoteCount(c);
            return vc;
        }
    }

    public class CachedVoteCount : IVoteCount
    {
        private readonly IVoteCount VoteCount;
        private readonly Dictionary<Candidate, int> VoteCounts = new Dictionary<Candidate, int>();
        private readonly List<Candidate> Candidates;

        public CachedVoteCount(IEnumerable<Candidate> candidates, IVoteCount voteCount)
        {
            Candidates = new List<Candidate>(candidates);
            VoteCount = voteCount;
        }

        /// <inheritdoc/>
        public int GetVoteCount(Candidate c)
        {
            if (VoteCounts.ContainsKey(c))
                return VoteCounts[c];
            return VoteCounts[c] = VoteCount.GetVoteCount(c);
        }

        /// <inheritdoc/>
        public Dictionary<Candidate, int> GetVoteCounts()
        {
            // Fully populate VoteCounts, then return a copy
            foreach (Candidate c in Candidates)
                GetVoteCount(c);
            return new Dictionary<Candidate, int>(VoteCounts);
        }
    }
}
