using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoonsetTechnologies.Voting.Tiebreakers;

namespace MoonsetTechnologies.Voting.Analytics
{
    public interface IVoteCount : IBatchEliminator, IUpdateTiebreaker
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

    public abstract class AbstractVoteCount : IVoteCount
    {
        protected readonly IBatchEliminator batchEliminator;
        protected readonly ITiebreaker tiebreaker;

        protected AbstractVoteCount(ITiebreaker tiebreaker, IBatchEliminator batchEliminator)
        {
            this.batchEliminator = batchEliminator;
            this.tiebreaker = tiebreaker;
        }
        public abstract decimal GetVoteCount(Candidate candidate);
        public abstract Dictionary<Candidate, decimal> GetVoteCounts();
        /// <inheritdoc/>
        public virtual IEnumerable<Candidate> GetEliminationCandidates
            (Dictionary<Candidate, decimal> hopefuls, int elected, decimal surplus = 0.0m)
          => batchEliminator.GetEliminationCandidates(hopefuls, elected, surplus);
        public virtual void UpdateTiebreaker<T>(Dictionary<Candidate, T> CandidateStates) where T : CandidateState
        {
            tiebreaker.UpdateTiebreaker(CandidateStates);
        }
    }
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
