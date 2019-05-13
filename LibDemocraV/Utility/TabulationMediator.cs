using MoonsetTechnologies.Voting.Tabulation;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public class TabulationMediator : TabulationMonitor
    {
        /// <summary>
        /// Inform listeners of a completed tabulation round.
        /// </summary>
        /// <param name="candidateStates">The current candidate states at the time of update.</param>
        /// <param name="note">A note to attach to the message.</param>
        public new void CompleteRound(Dictionary<Candidate, CandidateState> candidateStates,
            string note = null)
          => base.CompleteRound(candidateStates, note);

        /// <summary>
        /// Call for an update to the tiebreaker.
        /// </summary>
        /// <param name="candidateStates">The current candidate states at the time of update.</param>
        /// <param name="note">A note to attach to the message.</param>
        public new void UpdateTiebreaker(Dictionary<Candidate, CandidateState> candidateStates,
            string note = null)
        {
            base.UpdateTiebreaker(candidateStates, note);
        }

        /// <summary>
        /// Inform listeners of a tiebreaker state change.
        /// </summary>
        /// <param name="winPairs">The new pairs of candidates who will win a tiebreaker.</param>
        /// <param name="note">A note to attach to the message.</param>
        public new void ChangeTiebreakerState(Dictionary<Candidate, Dictionary<Candidate, bool>> winPairs,
            string note = null)
        {
            base.ChangeTiebreakerState(winPairs, note);
        }

        /// <summary>
        /// Inform listeners of a completed tabulation.
        /// </summary>
        /// <param name="candidateStates">The current candidate states at the time of update.</param>
        /// <param name="note">A note to attach to the message.</param>
        public new void CompleteTabulation(Dictionary<Candidate, CandidateState> candidateStates,
            string note = null)
          => base.CompleteTabulation(candidateStates, note);
        /// <summary>
        /// Inform listeners of a completed tabulation round.
        /// </summary>
        /// <param name="tabulationState">The current tabulation state at the time of update.</param>
        public new void CompleteRound(TabulationStateEventArgs tabulationState)
          => base.CompleteRound(tabulationState);
    }
}
