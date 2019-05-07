using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting
{
    public interface IRankedVote : IVote, IComparable<IRankedVote>
    {
        /// <summary>
        /// The ordinal value, with lower indicating more preferred.
        /// </summary>
        int Value { get; }
        /// <summary>
        /// Check if this vote is ranked higher in preference to (vote).
        /// </summary>
        /// <param name="vote">The vote to compare.</param>
        /// <returns>true if this vote is ranked higher in preference.  false on tie or loss.</returns>
        bool Beats(IRankedVote vote);
    }
}
