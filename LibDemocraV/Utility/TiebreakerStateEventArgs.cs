using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    /// <summary>
    /// Information about a tiebreaker state after update.
    /// </summary>
    public class TiebreakerStateEventArgs : EventArgs
    {
        /// <summary>
        /// Type of tiebreaker reporting.
        /// </summary>
        public Type TiebreakerType;
        /// <summary>
        /// Pairs of winners, if applicable to this tiebreaking method.
        /// </summary>
        public Dictionary<Candidate, Dictionary<Candidate, bool>> WinPairs { get; set; }
        /// <summary>
        /// Additional information.
        /// </summary>
        public string Note { get; set; }
    }
}
