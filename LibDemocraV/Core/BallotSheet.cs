//
// Copyright (c) Moonset Technology Holdings, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//

using System;
using System.Collections;
using System.Collections.Generic;
using MoonsetTechnologies.Voting.Factories;

namespace MoonsetTechnologies.Voting
{
    public class BallotSheet : IReadOnlyCollection<ReadOnlyBallot>, ICloneable
    {
        protected List<ReadOnlyBallot> Ballots { get; } = new List<ReadOnlyBallot>();

        // Ballot collection
        int IReadOnlyCollection<ReadOnlyBallot>.Count => Ballots.Count;
        // Shallow copy
        IEnumerator<ReadOnlyBallot> IEnumerable<ReadOnlyBallot>.GetEnumerator()
        {
            List<ReadOnlyBallot> ballots = new List<ReadOnlyBallot>();
            foreach (ReadOnlyBallot b in Ballots)
                ballots.Add((ReadOnlyBallot)b.Clone());
            return ballots.GetEnumerator();
        } 

        IEnumerator IEnumerable.GetEnumerator() => ((IReadOnlyCollection<ReadOnlyBallot>)this).GetEnumerator();

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public BallotSheet(IEnumerable<Race> races)
        {
            foreach (Race r in races)
            {
                AbstractBallotFactory bf = AbstractBallotFactory.GetFactory(r);
                Ballots.Add(bf.CreateBallot());
            }
        }
    }
}