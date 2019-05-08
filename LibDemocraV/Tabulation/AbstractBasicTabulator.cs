using System;
using System.Collections.Generic;
using System.Linq;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public abstract class AbstractBasicTabulator<T, TType> : AbstractTabulator<T, TType>
        where T : IBallot
        where TType : AbstractTabulator<T, TType>
    {
        /// <inheritdoc/>
        public override void TabulateRound()
        {
            Dictionary<Candidate, CandidateState.States> tabulation = GetTabulation();

            if (tabulation.Count() == 0)
                return;

            // B.2.c Elect candidates, or B.3 defeat low candidates
            // We won't have defeats if there were elections in B.2.c,
            // but rule C may provide both winners and losers
            SetStates(tabulation);

            batchEliminator.UpdateTiebreaker(candidateStates);

            // Update tabulation for next round
            ComputeTabulation();
        }

        /// <inheritdoc/>
        public override Dictionary<Candidate, CandidateState> GetFullTabulation()
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
        /// Compute the next round of tabulation.  Does not move to the next tabulation round.
        /// </summary>
        protected virtual void ComputeTabulation()
        {
            Dictionary<Candidate, CandidateState.States> tabulation;

            // B.1 Test Count complete
            if (Complete)
                return;

            // Perform iteration B.2
            CountBallots();
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

        /// <summary>
        /// Initialize candidate states.
        /// </summary>
        /// <param name="candidates">The candidates in this election.</param>
        protected virtual void InitializeCandidateStates(IEnumerable<Candidate> candidates)
        {
            foreach (Candidate c in candidates)
                candidateStates[c] = new CandidateState();
        }

        public class Builder : Builder<Builder>
        {
            public override void BuildTiebreaker()
            {
                throw new NotImplementedException();
            }

            public override AbstractBatchEliminator BuildBatchEliminator()
            {
                throw new NotImplementedException();
            }

            public override TType Build()
            {
                throw new NotImplementedException();
            }

            public override Builder WithSeats(int seats)
            {
                this.seats = seats;
                return this;
            }

            public override Builder WithCandidates(IEnumerable<Candidate> candidates)
            {
                this.candidates = candidates;
                return this;
            }

            public override Builder WithBallots(IEnumerable<T> ballots)
            {
                this.ballots = ballots;
                return this;
            }
        }
    }
}