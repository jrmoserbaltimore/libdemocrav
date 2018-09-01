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
    public class BallotSheet : IReadOnlyCollection<Ballot>, ICloneable
    {
        protected List<Ballot> Ballots { get; } = new List<Ballot>();

        // Ballot collection
        int IReadOnlyCollection<Ballot>.Count => Ballots.Count;
        // Shallow copy
        IEnumerator<Ballot> IEnumerable<Ballot>.GetEnumerator()
        {
            List<Ballot> ballots = new List<Ballot>();
            foreach (Ballot b in Ballots)
                ballots.Add((Ballot)b.Clone());
            return ballots.GetEnumerator();
        } 

        IEnumerator IEnumerable.GetEnumerator() => ((IReadOnlyCollection<Ballot>)this).GetEnumerator();

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