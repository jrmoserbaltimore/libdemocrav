//
// Copyright (c) Secure Democratic Election Services, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//


using System;
using System.Collections;
using System.Collections.Generic;

namespace DemocraticElections.Voting
{
    public abstract class Election : IEquatable<Election>
    {
        
	public IReadOnlyCollection<Voter> Voters => VoterList.AsReadOnly();
	protected ICollection<Voter> VoterList { get; } = new List<Voter>();

	public IReadOnlyCollection<Race> Races => RaceList.AsReadOnly();
        protected ICollection<Race> RaceList { get; } = new List<Race>();

        protected Guid Id { get; private set; }

        // Comparators
        public virtual bool Equals(Election e) => e.Id.Equals(Id);
        public virtual bool Equals(Object o) {
            if (o is Election e)
                return e.Id.Equals(Id);
            throw new ArgumentException("O is not an Election object.");
        }
        public virtual int GetHashCode(Election e) => e.Id.GetHashCode();

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
