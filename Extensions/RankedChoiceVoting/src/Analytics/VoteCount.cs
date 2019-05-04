using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MoonsetTechnologies.Voting.Analytics
{
    public interface IVoteCount
    {
        /// <summary>
        /// Gets the count of votes for a candidate.
        /// </summary>
        /// <param name="c">The candidate whose votes to count</param>
        /// <returns>The number of votes received.</returns>
        decimal GetVoteCount(Candidate c);
        /// <summary>
        /// Get the count of votes for all candidates.
        /// </summary>
        /// <returns>The number of votes each candidate receives.</returns>
        Dictionary<Candidate, decimal> GetVoteCounts();

        /// <summary>
        /// Get batch elimination candidates for a run-off round.
        /// </summary>
        /// <returns>A list of candidates to eliminate.</returns>
        IEnumerable<Candidate> GetBatchEliminationCandidates();
    }

    public class CandidateState
    {
        public enum States
        {
            defeated = 0,
            withdrawn = 1,
            hopeful = 2,
            elected = 3
        };
        public decimal VoteCount { get; set; }
        public States State { get; set; }

        public CandidateState()
        {
        }
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
        public decimal GetVoteCount(Candidate c)
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
        public Dictionary<Candidate, decimal> GetVoteCounts()
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
        /// <inheritdoc/>
        public IEnumerable<Candidate> GetBatchEliminationCandidates()
        {
            throw new NotImplementedException();
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

    public class CachedVoteCount : IVoteCount
    {
        private readonly IVoteCount VoteCount;
        private readonly Dictionary<Candidate, decimal> VoteCounts = new Dictionary<Candidate, decimal>();
        private readonly List<Candidate> Candidates;

        public CachedVoteCount(IEnumerable<Candidate> candidates, IVoteCount voteCount)
        {
            Candidates = new List<Candidate>(candidates);
            VoteCount = voteCount;
        }

        /// <inheritdoc/>
        public decimal GetVoteCount(Candidate c)
        {
            if (VoteCounts.ContainsKey(c))
                return VoteCounts[c];
            return VoteCounts[c] = VoteCount.GetVoteCount(c);
        }

        /// <inheritdoc/>
        public Dictionary<Candidate, decimal> GetVoteCounts()
        {
            // Fully populate VoteCounts, then return a copy
            foreach (Candidate c in Candidates)
                GetVoteCount(c);
            return new Dictionary<Candidate, decimal>(VoteCounts);
        }

        public Candidate GetLeastVotedCandidate()
        {
            Dictionary<Candidate, decimal> vc = GetVoteCounts();
            Candidate output = null;
            foreach (Candidate c in vc.Keys)
            {
                if (output is null || vc[c] < vc[output])
                    output = c;
            }
            return output;
        }

        /// <inheritdoc/>
        public IEnumerable<Candidate> GetBatchEliminationCandidates()
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
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
                    // so remove (c) from the top candidates.
                    // This will result in more than (count) candidates
                    // if there is a tie for last place.
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
