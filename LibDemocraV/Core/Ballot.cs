//
// Copyright (c) Moonset Technologies, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MoonsetTechnologies.Voting
{
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

        public virtual bool Equals(Vote v)
        {
            if (v is null)
                return false;
            else if (ReferenceEquals(this, v))
                return true;
            return Candidate.Equals(v.Candidate) && Value.Equals(v.Value);
        }

        public override bool Equals(object obj) => Equals(obj as Vote);

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
    }

    /// <summary>
    /// The basic ReadOnlyBallot for iterating through votes.  Read-only.
    /// </summary>
    public abstract class ReadOnlyBallot : IReadOnlyCollection<Vote>
    {
        public Race Race { get; }
        protected List<Vote> Votes { get; } = new List<Vote>();

        // Vote collection
        int IReadOnlyCollection<Vote>.Count => Votes.Count;
        IEnumerator<Vote> IEnumerable<Vote>.GetEnumerator() => Votes.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<Vote>)this).GetEnumerator();

        /// <summary>
        /// Create a ballot with a Race and no votes.
        /// </summary>
        /// <param name="race">The Race this ballot represents.</param>
        protected ReadOnlyBallot(Race race)
        {
            Race = race;
        }
    }

    /// <summary>
    /// Ballot which allows the casting of votes.
    /// </summary>
    public abstract class Ballot : ReadOnlyBallot
    {
        /// <summary>
        /// Cast a vote.
        /// </summary>
        /// <param name="vote">The vote to cast.</param>
        public virtual void Cast(Vote vote)
        {
            if (!Race.Candidates.Contains(vote.Candidate))
                throw new ArgumentException("Vote is for a candidate not in the race.");
            Votes.RemoveAll(v => v.Candidate == vote.Candidate);
            if (vote.Value > 0)
                Votes.Add(vote);
        }

        /// <summary>
        /// Remove the votes cast for a candidate, if any.
        /// </summary>
        /// <param name="candidate">Candidate whose votes to remove.</param>
        protected virtual void Remove(Candidate candidate) =>
            Votes.RemoveAll(v => v.Candidate == candidate);

        /// <summary>
        /// Create a ballot with a Race and no votes.
        /// </summary>
        /// <param name="race">The Race this ballot represents.</param>
        protected Ballot(Race race)
            : base(race)
        {

        }
    }
}