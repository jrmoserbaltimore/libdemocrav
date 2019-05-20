using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Ballots
{
    public class CountedBallot : Ballot
    {
        public int Count { get; protected set; }
        public CountedBallot(Ballot ballot, int count)
            : base(ballot.Votes)
        {
            Count = count;
        }

        // Combine the Count into the HashCode
        /// <inheritdoc/>
        public override int GetHashCode()
          => HashCode.Combine(base.GetHashCode(), Count);
    }
}
