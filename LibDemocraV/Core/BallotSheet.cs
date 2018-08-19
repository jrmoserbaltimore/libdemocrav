//
// Copyright (c) Secure Democratic Election Services, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//

using System;
using System.Collections;
using System.Collections.Generic;

namespace DemocraticElections.Voting
{
    
    public class BallotSheet : ICollection<Ballot>
    {
        protected List<Ballot> Ballots { get; } = new List<Ballot>();

        // Ballot collection
        int ICollection<Ballot>.Count => Ballots.Count;
        bool ICollection<Ballot>.IsReadOnly => true;
        void ICollection<Ballot>.Add(Ballot item) => throw new NotImplementedException();
        bool ICollection<Ballot>.Remove(Ballot item) => throw new NotImplementedException();
        void ICollection<Ballot>.Clear() => throw new NotImplementedException();
        bool ICollection<Ballot>.Contains(Ballot item) => throw new NotImplementedException();
        void ICollection<Ballot>.CopyTo(Ballot[] array, int arrayIndex) => throw new NotImplementedException();
        // Shallow copy
        IEnumerator<Ballot> IEnumerable<Ballot>.GetEnumerator() => new List<Ballot>(Ballots).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((ICollection<Ballot>)this).GetEnumerator();
        
        public BallotSheet(IEnumerable<Race> races)
        {
            foreach (Race r in races)
                Ballots.Add(new Ballot(r));
        }
    }
}