//
// Copyright (c) Moonset Technologies, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//

using System;
using System.Collections;
using System.Collections.Generic;

namespace MoonsetTechnologies.Voting
{
    public class ReadOnlyBallotSheet : IReadOnlyCollection<ReadOnlyBallot>
    {
        protected List<Ballot> Ballots { get; } = new List<Ballot>();

        public int Count => Ballots.Count;

        // Shallow copy
        IEnumerator<ReadOnlyBallot> IEnumerable<ReadOnlyBallot>.GetEnumerator() =>
            Ballots.GetEnumerator();

        public virtual IEnumerator GetEnumerator() =>
            ((IReadOnlyCollection<ReadOnlyBallot>)this).GetEnumerator();

        protected ReadOnlyBallotSheet(IEnumerable<Race> races)
        {

        }

        public ReadOnlyBallotSheet(IEnumerable<ReadOnlyBallot> ballots)
        {
            foreach (ReadOnlyBallot r in ballots)
            {
                Ballot b = r as Ballot;
                if (b is null)
                    Ballots.Add(r.Race.GetNewBallot(r));
                else
                    Ballots.Add(b);
            }
        }
    }

    public class BallotSheet : ReadOnlyBallotSheet, IReadOnlyCollection<Ballot>
    {
        public BallotSheet(IEnumerable<Race> races)
            : base(races)
        {
            foreach (Race r in races)
            {
                Ballots.Add(r.GetNewBallot());
            }
        }

        IEnumerator<Ballot> IEnumerable<Ballot>.GetEnumerator() =>
            Ballots.GetEnumerator();

        public override IEnumerator GetEnumerator() =>
            ((IReadOnlyCollection<Ballot>)this).GetEnumerator();
    }
}