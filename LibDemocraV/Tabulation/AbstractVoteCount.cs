using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public abstract class AbstractVoteCount<T> : IVoteCount
        where T : IBallot
    {
        protected readonly int seats;
        protected readonly IBatchEliminator batchEliminator;
        protected readonly List<T> ballots;
        protected readonly Dictionary<Candidate, CandidateState> candidateStates
            = new Dictionary<Candidate, CandidateState>();

        /// <summary>
        /// Initialize candidate states.
        /// </summary>
        /// <param name="candidates">The candidates in this election.</param>
        protected virtual void InitializeCandidateStates(IEnumerable<Candidate> candidates)
        {
            foreach (Candidate c in candidates)
                candidateStates[c] = new CandidateState();
        }
        protected AbstractVoteCount(IEnumerable<Candidate> candidates, IEnumerable<T> ballots,
            IBatchEliminator batchEliminator, int seats = 1)
        {
            InitializeCandidateStates(candidates);

            this.ballots = ballots.ToList();

            this.batchEliminator = batchEliminator;

            this.seats = seats;

            // State is not valid until ballots have been counted once.
            CountBallots();
        }

        /// <inheritdoc/>
        public abstract void CountBallots();

        /// <inheritdoc/>
        public virtual decimal GetVoteCount(Candidate candidate)
            => candidateStates[candidate].VoteCount;

        /// <inheritdoc/>
        public virtual Dictionary<Candidate, decimal> GetVoteCounts()
            => candidateStates.Where(x => x.Value.State == CandidateState.States.elected
                || x.Value.State == CandidateState.States.hopeful).ToDictionary(x => x.Key, x => x.Value.VoteCount);

        /// <inheritdoc/>
        public abstract Dictionary<Candidate, CandidateState.States> GetTabulation();

        /// <inheritdoc/>
        public virtual Dictionary<Candidate, CandidateState> GetFullTabulation()
        {

            Dictionary<Candidate, CandidateState> fullTabulation =
              candidateStates.Where(x => x.Value.State == CandidateState.States.elected
                || x.Value.State == CandidateState.States.hopeful)
                .ToDictionary(x => x.Key, x => new CandidateState { VoteCount = x.Value.VoteCount, State = x.Value.State });
            Dictionary<Candidate, CandidateState.States> tabulation = GetTabulation();

            // Merge the tabulation into the full tabulation
            foreach (Candidate c in tabulation.Keys)
            {
                // FIXME:  Improve exception
                if (!fullTabulation.ContainsKey(c))
                    throw new InvalidOperationException();
                fullTabulation[c].State = tabulation[c];
            }

            return fullTabulation;
        }

        /// <summary>
        /// Set the States of candidates.  Includes a validation check.
        /// </summary>
        /// <param name="candidates">The candidates for which to set state.</param>
        /// <param name="state">The state.</param>
        protected virtual void SetStates(Dictionary<Candidate, CandidateState.States> candidates)
        {
            foreach (Candidate c in candidates.Keys)
            {
                // FIXME:  Improve exception
                if (!candidateStates.ContainsKey(c))
                    throw new ArgumentOutOfRangeException();
                candidateStates[c].State = candidates[c];
            }
        }

        /// <inheritdoc/>
        public virtual bool ApplyTabulation()
        {
            Dictionary<Candidate, CandidateState.States> tabulation = GetTabulation();

            if (tabulation.Count() == 0)
                return false;

            SetStates(tabulation);

            batchEliminator.UpdateTiebreaker(candidateStates);
            return true;
        }


    }
}
