using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public abstract class AbstractTabulator
    {
        protected int seats;
        protected List<Ballot> ballots;
        protected AbstractBatchEliminator batchEliminator;
        protected AbstractTiebreakerFactory tiebreakerFactory;
        protected AbstractTabulationAnalytics analytics;

        protected readonly Dictionary<Candidate, CandidateState> candidateStates
            = new Dictionary<Candidate, CandidateState>();

        protected TabulationMediator mediator;
        public TabulationMonitor Monitor => mediator as TabulationMonitor;
        protected Dictionary<Candidate, CandidateState> CandidateStatesCopy =>
            candidateStates.ToDictionary(x => x.Key, x => x.Value.Clone() as CandidateState);

        protected abstract void InitializeTabulation(IEnumerable<Ballot> ballots, IEnumerable<Candidate> withdrawn, int seats);

        /// <summary>
        /// Performs a complete tabulation of given ballots.
        /// </summary>
        /// <param name="ballots">The ballots to tabulate.</param>
        /// <param name="withdrawn">Candidates withdrawn from the election.  Any votes for these candidates are ignored.</param>
        /// <param name="seats">The number of winners to elect.</param>
        public void Tabulate(IEnumerable<Ballot> ballots,
            IEnumerable<Candidate> withdrawn = null,
            int seats = 1)
        {
            InitializeTabulation(ballots, withdrawn, seats);
            if (seats < 1)
                throw new ArgumentOutOfRangeException("seats", "seats must be at least one.");
            // Count() throws ArgumentNullException when ballots is null
            if (ballots.Count() < 1)
                throw new ArgumentOutOfRangeException("ballots", "Require at least one ballot");

            this.seats = seats;
            this.ballots = ballots.ToList();

            candidateStates.Clear();
            if (!(withdrawn is null))
                InitializeCandidateStates(withdrawn);

            do
            {
                CountBallots();
                TabulationStateEventArgs state = TabulateRound();

                // Make copies of candidateStates to prevent errors if written to by client code
                mediator.CompleteRound(state);
            // The check is at the end so we can fill our candidate roster
            } while (!IsComplete());

            // Perform a final count
            CountBallots();
            mediator.CompleteTabulation(CandidateStatesCopy);
        }

        /// <summary>
        /// Determine if tabulation is complete.
        /// </summary>
        /// <returns>True if complete, false otherwise.</returns>
        protected virtual bool IsComplete()
        {
            int possibleWinnerCount;

            possibleWinnerCount = candidateStates.Where(x =>
                new[] { CandidateState.States.elected, CandidateState.States.hopeful }
                .Contains(x.Value.State)).Count();
            // We're done counting if we have fewer hopeful+elected than seats
            return (possibleWinnerCount <= seats);
        }

        /// <summary>
        /// Perform a round of tabulation, electing and eliminating candidates.
        /// Returns when it's time to check for completion or recount ballots.
        /// </summary>
        protected abstract TabulationStateEventArgs TabulateRound();

        /// <summary>
        /// Count ballot and update candidateState.
        /// </summary>
        /// <param name="ballot">The ballot to count.</param>
        protected abstract void CountBallot(Ballot ballot);

        /// <summary>
        /// Perform a ballot count and updates the internal state.
        /// </summary>
        protected virtual void CountBallots()
        {
            // Zero all the vote counts
            foreach (CandidateState c in candidateStates.Values)
                c.VoteCount = 0.0m;
            foreach(Ballot b in ballots)
            {
                // Add any candidates not yet seen to candidateStates
                List<Candidate> c = b.Votes.Where(x => !candidateStates.Keys.Contains(x.Candidate))
                    .Select(x => x.Candidate).ToList();
                InitializeCandidateStates(c);
                CountBallot(b);
            }
        }

        /// <summary>
        /// Initialize candidate states.
        /// </summary>
        /// <param name="candidates">The candidates in this election.</param>
        /// <param name="state">The state in which to initialize the candidate.</param>
        protected void InitializeCandidateStates(IEnumerable<Candidate> candidates,
            CandidateState.States state = CandidateState.States.hopeful)
        {
            foreach (Candidate c in candidates)
            {
                SetState(c, state);
                candidateStates[c].VoteCount = 0;
            }
        }

        /// <summary>
        /// Set the State of a candidate.
        /// </summary>
        /// <param name="candidate">The candidate for which to set state.</param>
        /// <param name="state">The state in which to initialize the candidate.</param>
        protected virtual void SetState(Candidate candidate, CandidateState.States state)
        {
            if (!candidateStates.ContainsKey(candidate))
                candidateStates[candidate] = new CandidateState();
            candidateStates[candidate].State = state;
        }

        protected AbstractTabulator(TabulationMediator mediator,
            AbstractTiebreakerFactory tiebreakerFactory,
            int seats = 1)
        {
            this.mediator = mediator;
            this.tiebreakerFactory = tiebreakerFactory;
            this.seats = seats;
        }
    }
}