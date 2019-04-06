using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting
{
    public interface IBallot
    {
        IEnumerable<Vote> Votes { get; }
    }

    /// <summary>
    /// A Vote object.  Allows placing a value on a vote.
    /// Immutable.
    /// </summary>
    public class Vote : IComparable<Vote>, IEquatable<Vote>
    {
        public Candidate Candidate { get; }
        public int Value { get; }

        public Vote(Candidate candidate, int value)
        {
            Candidate = candidate;
            Value = value;
        }

        /// <summary>
        /// Returns the higher-ranked Vote, with a lower Vote.Value indicating
        /// a higher rank, or null if both are of equal rank.
        /// </summary>
        /// <param name="first">A Vote to compare.  Must not be null.</param>
        /// <param name="second">A Vote to compare.  Must not be null.</param>
        /// <returns></returns>
        public static Vote GetHigherRanked(Vote first, Vote second)
        {
            if (first is null)
                throw new ArgumentNullException("first", "Attempt to compare ranks of a null Vote object.");
            if (second is null)
                throw new ArgumentNullException("second", "Attempt to compare ranks of a null Vote object.");
            if (first.CompareTo(second) == 0)
                return null;
            if (first > second)
                return second;
            if (first < second)
                return first;
            throw new NotImplementedException("Somehow reached unreachable code path.");
        }

        public virtual bool Equals(Vote v)
        {
            if (v is null)
                return false;
            else if (ReferenceEquals(this, v))
                return true;
            return Candidate.Equals(v.Candidate) && Value.Equals(v.Value);
        }

        public override bool Equals(object obj) => Equals(obj as Vote);

        public override int GetHashCode() => HashCode.Combine(Candidate, Value);

        /// <summary>
        /// Compares two votes to determine which has a higher Value,
        /// but not a stronger ordinal rank.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Vote other)
        {
            if (other is null)
                return 1;
            return Value.CompareTo(other.Value);
        }

        public int CompareTo(object obj)
        {
            Vote v = obj as Vote;
            if (!(obj is null) && (obj as Vote is null))
                throw new ArgumentException("Object is not a Vote");
            return CompareTo(obj as Vote);
        }

        public static bool operator >(Vote lhs, Vote rhs) => lhs.CompareTo(rhs) > 0;
        public static bool operator <(Vote lhs, Vote rhs) => lhs.CompareTo(rhs) < 0;
        public static bool operator >=(Vote lhs, Vote rhs) => lhs.CompareTo(rhs) >= 0;
        public static bool operator <=(Vote lhs, Vote rhs) => lhs.CompareTo(rhs) <= 0;

        public static bool operator ==(Vote lhs, Vote rhs)
        {
            if (lhs is null && rhs is null)
                return true;
            else if (lhs is null)
                return false;
            else
                return lhs.Equals(rhs);
        }

        public static bool operator !=(Vote lhs, Vote rhs) => !(lhs == rhs);
    }

    /// <summary>
    /// The basic ReadOnlyBallot for iterating through votes.  Read-only.
    /// </summary>
    public abstract class Ballot : IBallot
    {
        protected List<Vote> votes = new List<Vote>();

        public IEnumerable<Vote> Votes => votes;

        /// <summary>
        /// Create a ballot with a Race and no votes.
        /// </summary>
        protected Ballot()
        {

        }
    }

}
