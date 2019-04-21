using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Core.Analytics
{
    interface IVoteCount
    {
        int GetVoteCount(Candidate c);
        Dictionary<Candidate, int> GetVoteCounts();
    }
    class VoteCount : IVoteCount
    {
        private readonly List<Candidate> Candidates;
        private readonly List<IRankedBallot> Ballots;
        public VoteCount(IEnumerable<Candidate> candidates, IEnumerable<IRankedBallot> ballots)
        {
            Candidates = new List<Candidate>(candidates);
            Ballots = new List<IRankedBallot>(ballots);
        }

        public int GetVoteCount(Candidate c)
        {
            int count = 0;
            foreach (IRankedBallot b in Ballots)
            {
                IRankedVote vote = null;
                foreach (IRankedVote v in b.Votes)
                {
                    // First vote examined or it beats current
                    if (vote is null || v.Beats(vote))
                        vote = v;
                }
                if (!(vote is null) && vote.Candidate == c)
                    count++;
            }
            return count;
        }

        public Dictionary<Candidate, int> GetVoteCounts()
        {
            Dictionary<Candidate, int> vc = new Dictionary<Candidate, int>();
            foreach (Candidate c in Candidates)
                vc[c] = GetVoteCount(c);
            return vc;
        }
    }

    class CachedVoteCount : IVoteCount
    {
        private readonly IVoteCount VoteCount;
        private readonly Dictionary<Candidate, int> VoteCounts = new Dictionary<Candidate, int>();
        private readonly List<Candidate> Candidates;

        public CachedVoteCount(IEnumerable<Candidate> candidates, IVoteCount voteCount)
        {
            Candidates = new List<Candidate>(candidates);
            VoteCount = voteCount;
        }
        public int GetVoteCount(Candidate c)
        {
            if (VoteCounts.ContainsKey(c))
                return VoteCounts[c];
            return VoteCounts[c] = VoteCount.GetVoteCount(c);
        }

        public Dictionary<Candidate, int> GetVoteCounts()
        {
            // Fully populate VoteCounts, then return a copy
            foreach (Candidate c in Candidates)
                GetVoteCount(c);
            return new Dictionary<Candidate, int>(VoteCounts);
        }
    }
}
