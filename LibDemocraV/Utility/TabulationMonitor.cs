using MoonsetTechnologies.Voting.Tabulation;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public class TabulationMonitor
    {
        /// <summary>
        /// Provides analytics of a vote count.
        /// </summary>
        public event EventHandler<TabulationStateEventArgs> CountComplete;
        /// <summary>
        /// Provides a full tabulation, including final vote counts, after each round of tabulation.
        /// </summary>
        public event EventHandler<TabulationDetailsEventArgs> TabulationBegin;
        /// <summary>
        /// Provides a full tabulation, including final vote counts, after each round of tabulation.
        /// </summary>
        public event EventHandler<TabulationStateEventArgs> RoundComplete;
        /// <summary>
        /// Provides a full tabulation, including final vote counts, at the end of tabulation.
        /// </summary>
        public event EventHandler<TabulationStateEventArgs> TabulationComplete;
        /// <summary>
        /// Raises informational messages about detailed tabulation events.
        /// </summary>
        public event EventHandler<TabulationStateEventArgs> TabulationInfo;
        /// <summary>
        /// Provides updates to the tiebreaker's state after tiebreaking state has changed
        /// </summary>
        public event EventHandler<TiebreakerStateEventArgs> TiebreakerStateChange;
        /// <summary>
        /// Provides winners and losers of a tiebreaking computation, with tie winners having State = elected and losers State = defeated.
        /// </summary>
        public event EventHandler<TabulationStateEventArgs> Tiebreaking;

        private void SendEvent(EventHandler<TabulationStateEventArgs> handler,
            Dictionary<Candidate, CandidateState> candidateStates,
            string note = null)
        {
            handler?.Invoke(this, new TabulationStateEventArgs
            {
                CandidateStates = candidateStates,
                Note = note
            });
        }

        private void SendEvent(EventHandler<TiebreakerStateEventArgs> handler,
            Dictionary<Candidate, Dictionary<Candidate, bool>> winPairs,
            string note = null)
        {
            handler?.Invoke(this, new TiebreakerStateEventArgs
            {
                WinPairs = winPairs,
                Note = note
            });
        }

        private void SendEvent(EventHandler<TabulationStateEventArgs> handler,
            TabulationStateEventArgs tabulationState)
        {
            handler?.Invoke(this, tabulationState);
        }

        private void SendEvent(EventHandler<TabulationDetailsEventArgs> handler,
            TabulationDetailsEventArgs tabulationState)
        {
            handler?.Invoke(this, tabulationState);
        }

        /// <summary>
        /// Inform listeners of a tiebreaker state change.
        /// </summary>
        /// <param name="winPairs">The new pairs of candidates who will win a tiebreaker.</param>
        /// <param name="note">A note to attach to the message.</param>
        protected void ChangeTiebreakerState(Dictionary<Candidate, Dictionary<Candidate, bool>> winPairs,
            string note = null)
        {
            EventHandler<TiebreakerStateEventArgs> handler = TiebreakerStateChange;
            handler?.Invoke(this, new TiebreakerStateEventArgs
            {
                WinPairs = winPairs,
                Note = note
            });
        }

        /// <summary>
        /// Inform listeners of a tabulation beginning.
        /// </summary>
        /// <param name="tabulationDetails">Details of the tabulation.</param>
        protected void BeginTabulation(TabulationDetailsEventArgs tabulationDetails)
          => SendEvent(TabulationBegin, tabulationDetails);

        /// <summary>
        /// Inform listeners of a completed count.
        /// </summary>
        /// <param name="tabulationState">The current tabulation state at the time of update.</param>
        protected void CompleteCount(TabulationStateEventArgs tabulationState)
          => SendEvent(CountComplete, tabulationState);

        /// <summary>
        /// Inform listeners of a completed tabulation round.
        /// </summary>
        /// <param name="candidateStates">The current candidate states at the time of update.</param>
        /// <param name="note">A note to attach to the message.</param>
        protected void CompleteRound(Dictionary<Candidate, CandidateState> candidateStates,
            string note = null)
          => SendEvent(RoundComplete, candidateStates, note);

        /// <summary>
        /// Inform listeners of a completed tabulation round.
        /// </summary>
        /// <param name="tabulationState">The current tabulation state at the time of update.</param>
        protected void CompleteRound(TabulationStateEventArgs tabulationState)
          => SendEvent(RoundComplete, tabulationState);

        /// <summary>
        /// Inform listeners of a completed tabulation.
        /// </summary>
        /// <param name="candidateStates">The current candidate states at the time of update.</param>
        /// <param name="note">A note to attach to the message.</param>
        protected void CompleteTabulation(Dictionary<Candidate, CandidateState> candidateStates,
            string note = null)
          => SendEvent(TabulationComplete, candidateStates, note);
    }
}
