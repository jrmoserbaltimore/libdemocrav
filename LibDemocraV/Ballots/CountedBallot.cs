using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Ballots
{
    public class CountedBallot : Ballot, IEquatable<CountedBallot>
    {
        public long Count { get; protected set; }
        public CountedBallot(Ballot ballot, long count)
            : base(ballot.Votes)
        {
            Count = count;
        }

        // Combine the Count into the HashCode
        /// <inheritdoc/>
        public override int GetHashCode()
          => HashCode.Combine(base.GetHashCode(), Count);

        public bool Equals(CountedBallot other)
        {
            if (other.Count != Count)
                return false;
            return base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (obj is CountedBallot)
                return Equals(obj as CountedBallot);
            else if (obj is Ballot)
                return false;
            return base.Equals(obj);
        }
    }
}
