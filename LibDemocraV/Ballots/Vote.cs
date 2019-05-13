using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Ballots
{
    /// <summary>
    /// A ranked vote.  Immutable.
    /// </summary>
    public class Vote : IComparable<Vote>
    {
        /// <summary>
        /// The candidate for whom this vote is cast
        /// </summary>
        public Candidate Candidate { get; protected set; }
        /// <summary>
        /// The ordinal value, with lower indicating more preferred.
        /// </summary>
        public decimal Value { get; protected set;  }

        public Vote(Candidate candidate, decimal value)
        {
            Candidate = candidate;
            Value = value;
        }

        /// <summary>
        /// Check if this vote is ranked higher in preference to (vote).
        /// </summary>
        /// <param name="vote">The vote to compare.</param>
        /// <returns>true if this vote is ranked higher in preference.  false on tie or loss.</returns>
        public virtual bool Beats(Vote vote) => Value < vote.Value;

        /// <inheritdoc/>
        public virtual bool Equals(Vote v)
        {
            if (v is null)
                return false;
            else if (ReferenceEquals(this, v))
                return true;
            return Candidate.Equals(v.Candidate) && Value.Equals(v.Value);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as Vote);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Candidate, Value);

        /// <inheritdoc/>
        public virtual int CompareTo(Vote other)
        {
            if (other is null) return 1;
            return 0 - Value.CompareTo(other.Value);
        }
    }
}
