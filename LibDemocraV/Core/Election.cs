//
// Copyright (c) Secure Democratic Election Services, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//


using System;
using System.Collections;
using System.Collections.Generic;

namespace DemocraticElections.Voting
{
    public abstract class Election : ICollection<Race>, ICollection<Voter>
    {
        protected List<Voter> Voters { get; } = new List<Voter>();
        protected List<Race> Races { get; } = new List<Race>();

        protected Guid Id { get; private set; }

        // Voter collection
        int ICollection<Voter>.Count => Voters.Count;
        bool ICollection<Voter>.IsReadOnly => true;
        void ICollection<Voter>.Add(Voter item) => throw new NotImplementedException();
        bool ICollection<Voter>.Remove(Voter item) => throw new NotImplementedException();
        void ICollection<Voter>.Clear() => throw new NotImplementedException();
        bool ICollection<Voter>.Contains(Voter item) => throw new NotImplementedException();
        void ICollection<Voter>.CopyTo(Voter[] array, int arrayIndex) => throw new NotImplementedException();
        IEnumerator<Voter> IEnumerable<Voter>.GetEnumerator() => Voters.GetEnumerator();

        // Race collection
        int ICollection<Race>.Count => Races.Count;
        bool ICollection<Race>.IsReadOnly => true;
        void ICollection<Race>.Add(Race item) => throw new NotImplementedException();
        bool ICollection<Race>.Remove(Race item) => throw new NotImplementedException();
        void ICollection<Race>.Clear() => throw new NotImplementedException();
        bool ICollection<Race>.Contains(Race item) => throw new NotImplementedException();
        void ICollection<Race>.CopyTo(Race[] array, int arrayIndex) => throw new NotImplementedException();
        IEnumerator<Race> IEnumerable<Race>.GetEnumerator() => Races.GetEnumerator();

        // Comparators
        public virtual bool Equals(Election e) => e.Id.Equals(Id);
        public virtual int GetHashCode(Election e) => e.Id.GetHashCode();

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        // No empty elections
        private Election() => throw new NotImplementedException();

        public Election(IEnumerable<Voter> voters, IEnumerable<Race> races, Guid id)
        {
            if (voters != null)
                Voters.AddRange(voters);
            Races.AddRange(races);
            Id = new Guid(id.ToByteArray());
        }

        public Election(IEnumerable<Voter> voters, IEnumerable<Race> races)
            : this(voters, races, Guid.NewGuid())
        {

        }

        public Election(IEnumerable<Race> races)
            : this(null, races)
        {

        }
        // Copy constructor
        public Election(Election e) : this(e, e, e.Id)
        {

        }

        // Throw an exception if the voter has already voted.
        public void Cast(IBallotSheet ballots, Voter voter)
        {
            if (Voters.Contains(voter))
                throw new ArgumentException("Voter has already voted", "voter");
            Voters.Add(voter);
            throw new NotImplementedException();
        }
    }
}
