using MoonsetTechnologies.Voting.Ballots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public abstract class AbstractTabulator
    {
        protected int seats;
        protected IBatchEliminator batchEliminator;
        protected List<Ballot> ballots;
        protected readonly Dictionary<Candidate, CandidateState> candidateStates
            = new Dictionary<Candidate, CandidateState>();

        /// <inheritdoc/>
        public bool Complete => GetTabulation().Count() == 0;

        /// <inheritdoc/>
        public abstract void TabulateRound();

        /// <inheritdoc/>
        public abstract Dictionary<Candidate, CandidateState.States> GetTabulation();

        /// <inheritdoc/>
        public abstract Dictionary<Candidate, CandidateState> GetFullTabulation();

        /// <summary>
        /// Perform a ballot count and updates the internal state.
        /// </summary>
        protected abstract void CountBallots();

        /// <summary>
        /// Initialize candidate states.
        /// </summary>
        /// <param name="candidates">The candidates in this election.</param>
        protected virtual void InitializeCandidateStates(IEnumerable<Candidate> candidates)
        {
            foreach (Candidate c in candidates)
                candidateStates[c] = new CandidateState();
        }

        /// <summary>
        /// Set the States of candidates.  Includes a validation check.
        /// </summary>
        /// <param name="candidates">The candidates for which to set state.</param>
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

        protected AbstractTabulator(IEnumerable<Candidate> candidates, IEnumerable<Ballot> ballots,
            IBatchEliminator batchEliminator, int seats = 1)
        {
            if (seats < 1)
                throw new ArgumentOutOfRangeException("seats", "seats must be at least one.");
            // Count() throws ArgumentNullException when candidates is null
            if (candidates.Count() < 2)
                throw new ArgumentOutOfRangeException("candidates", "must be at least two candidates");
            // Count() throws ArgumentNullException when ballots is null
            if (ballots.Count() < 1)
                throw new ArgumentOutOfRangeException("ballots", "Require at least one ballot");

            seats = this.seats;
            ballots = this.ballots.ToList();
            batchEliminator = this.batchEliminator;
            InitializeCandidateStates(candidates);
        }
    }
}
