using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Tiebreaking;
using MoonsetTechnologies.Voting.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonsetTechnologies.Voting.Tabulation
{
    /// <summary>
    /// A tabulator to compute the result of an election from ballots.
    /// </summary>
    public abstract class AbstractTabulator
    {
        protected int seats;
        /// <summary>
        /// The ballots to tabulate.
        /// </summary>
        protected BallotSet ballots;

        /// <summary>
        /// Internal state for all candidates.
        /// </summary>
        protected readonly Dictionary<Candidate, CandidateState> candidateStates
            = new Dictionary<Candidate, CandidateState>();

        /// <summary>
        /// The mediator for communication of tabulation events.
        /// </summary>
        protected TabulationMediator mediator;
        /// <summary>
        /// Monitoring interface for reporting tabulation process and results.
        /// </summary>
        public TabulationMonitor Monitor => mediator as TabulationMonitor;
        /// <summary>
        /// A safe copy of candidateStates.
        /// </summary>
        protected Dictionary<Candidate, CandidateState> CandidateStatesCopy =>
            candidateStates.ToDictionary(x => x.Key, x => x.Value.Clone() as CandidateState);

        /// <summary>
        /// Initialize the tabulation state from a set of candidates, withdrawn candidates, and a number of seats.
        /// </summary>
        /// <param name="ballots">The ballots to tabulate.</param>
        /// <param name="withdrawn">Candidates excluded from tabulation.</param>
        /// <param name="seats">The number of seats to elect.</param>
        protected virtual void InitializeTabulation(BallotSet ballots, IEnumerable<Candidate> withdrawn, int seats)
        {
            this.seats = seats;
            this.ballots = ballots;

            candidateStates.Clear();
            if (!(withdrawn is null))
                InitializeCandidateStates(withdrawn, CandidateState.States.withdrawn);

            // Initialize hopefuls
            var q = (from b in ballots
                     from v in b.Votes
                     select v.Candidate).Distinct().Except(candidateStates.Keys);
            InitializeCandidateStates(q.ToArray());
        }

        /// <summary>
        /// Performs a complete tabulation of given ballots.
        /// </summary>
        /// <param name="ballots">The ballots to tabulate.</param>
        /// <param name="withdrawn">Candidates withdrawn from the election.  Any votes for these candidates are ignored.</param>
        /// <param name="seats">The number of winners to elect.</param>
        public void Tabulate(BallotSet ballots,
            IEnumerable<Candidate> withdrawn = null,
            int seats = 1)
        {
            TabulationStateEventArgs state;
            TabulationDetailsEventArgs tabulationDetails;
            if (seats < 1)
                throw new ArgumentOutOfRangeException("seats", "seats must be at least one.");
            // Count() throws ArgumentNullException when ballots is null
            if (ballots.TotalCount() < 1)
                throw new ArgumentOutOfRangeException("ballots", "Require at least one ballot");

            InitializeTabulation(ballots, withdrawn, seats);

            tabulationDetails = new TabulationDetailsEventArgs
            {
                Ballots = ballots,
                CandidateStates = CandidateStatesCopy,
                Note = null,
                Seats = seats
            };

            mediator.BeginTabulation(tabulationDetails);

            do
            {
                CountBallots();
                state = TabulateRound();

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
        protected bool IsComplete()
          => !(from x in candidateStates
               where x.Value.State == CandidateState.States.hopeful
               select x).Any();

        /// <summary>
        /// Determine if no further rounds are possible.
        /// </summary>
        /// <returns>True if all seats are elected or there are only as many hopefuls as open seats; false otherwise.</returns>
        protected bool IsFinalRound()
        {
            int possibleWinnerCount;
            CandidateState.States s = CandidateState.States.elected;
            var q = from x in candidateStates
                    where x.Value.State == s
                    select x;

            possibleWinnerCount = q.Count();
            // So far, elected fewer candidates than seats, so include hopefuls
            if (possibleWinnerCount < seats)
            {
                s = CandidateState.States.hopeful;
                possibleWinnerCount += q.Count();
            }

            // We're done counting if we have fewer hopeful+elected than seats
            return (possibleWinnerCount <= seats);
        }
        
        /// <summary>
        /// Sets final winners when IsFinalRound() is true.
        /// </summary>
        protected void SetFinalWinners()
        {
            var defeated = from x in new Candidate[] { } select x; ;

            // If we're done, there will be only enough hopefuls to fill remaining seats
            if (IsFinalRound())
            {
                var elected = from x in candidateStates
                              where x.Value.State == CandidateState.States.elected
                              select x.Key;
                var hopeful = from x in candidateStates
                              where x.Value.State == CandidateState.States.hopeful
                              select x.Key;
                
                if (elected.Count() + hopeful.Count() <= seats)
                    elected = hopeful;
                else if (elected.Count() == seats)
                {
                    elected = from x in new Candidate[] { } select x;
                    defeated = hopeful;
                }
                else if (elected.Count() > seats)
                    throw new InvalidOperationException("Somehow elected more candidates than seats.");

                foreach (Candidate c in elected.ToArray())
                    SetState(c, CandidateState.States.elected);
                foreach (Candidate c in defeated.ToArray())
                    SetState(c, CandidateState.States.defeated);
            }
        }

        /// <summary>
        /// Perform a round of tabulation, electing and eliminating candidates.
        /// Returns when it's time to check for completion or recount ballots.
        /// </summary>
        protected virtual TabulationStateEventArgs TabulateRound()
        {
            foreach (Candidate c in GetEliminationCandidates())
            {
                SetState(c, CandidateState.States.defeated);
            }

            return new TabulationStateEventArgs
            {
                CandidateStates = CandidateStatesCopy,
                Note = ""
            };
        }

        /// <summary>
        /// Count ballot and update candidateState.
        /// </summary>
        /// <param name="ballot">The ballot to count.</param>
        protected abstract void CountBallot(CountedBallot ballot);

        /// <summary>
        /// Perform a ballot count and updates the internal state.
        /// </summary>
        protected virtual void CountBallots()
        {
            // Zero all the vote counts
            foreach (CandidateState c in candidateStates.Values)
                c.VoteCount = 0.0m;
            foreach(CountedBallot b in ballots)
            {
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
        /// <summary>
        /// Get elimination candidates, including checking for and breaking ties.
        /// </summary>
        /// <returns>An enumerable of elimination candidates.</returns>
        protected abstract IEnumerable<Candidate> GetEliminationCandidates();

        /// <summary>
        /// Configures the Tabulators created based on a settings object.
        /// </summary>
        /// <param name="tabulatorSetting">The setting to adjust</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the setting is ultimately not recognized for this Tabulator.</exception>
        protected virtual void ConfigureTabulator(ITabulatorSetting tabulatorSetting)
        {
            throw new ArgumentOutOfRangeException("tabulatorSetting", "Tabulator factory given ITabulatorSetting not applicable to tabulator.");
        }

        /// <summary>
        /// Create a Tabulator with the given mediator and tiebreaker factory.
        /// </summary>
        /// <param name="mediator">The mediator to use.</param>
        /// <param name="tabulatorSettings">The settings to use.</param>
        protected AbstractTabulator(TabulationMediator mediator,
            IEnumerable<ITabulatorSetting> tabulatorSettings)
        {
            this.mediator = mediator;
            foreach (var x in tabulatorSettings)
            {
                ConfigureTabulator(x);
            }
        }
    }
}
