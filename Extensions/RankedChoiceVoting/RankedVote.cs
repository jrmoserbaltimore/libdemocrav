using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting
{

    /// <summary>
    /// A Vote object.  Allows placing a value on a vote.
    /// Immutable.
    /// </summary>
    public class RankedVote : IRankedVote, IEquatable<RankedVote>
    {
        public Candidate Candidate { get; }
        /// <inheritdoc/>
        public int Value { get; }

        public RankedVote(Candidate candidate, int value)
        {
            Candidate = candidate;
            Value = value;
        }

        /// <inheritdoc/>
        public virtual bool Beats(IRankedVote vote) => Value < vote.Value;

        /// <inheritdoc/>
        public virtual bool Equals(RankedVote v)
        {
            if (v is null)
                return false;
            else if (ReferenceEquals(this, v))
                return true;
            return Candidate.Equals(v.Candidate) && Value.Equals(v.Value);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as RankedVote);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Candidate, Value);

        /// <inheritdoc/>
        public int CompareTo(IRankedVote other)
        {
            if (other is null) return 1;
            return Value - other.Value;
        }
    }
}
