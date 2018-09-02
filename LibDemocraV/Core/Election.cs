//
// Copyright (c) Moonset Technologies, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//


using System;
using System.Collections;
using System.Collections.Generic;

namespace MoonsetTechnologies.Voting
{
    public abstract class Election : IEquatable<Election>
    {
        
        public IReadOnlyCollection<Voter> Voters => VoterList.AsReadOnly();
        protected List<Voter> VoterList { get; } = new List<Voter>();

        public IReadOnlyCollection<Race> Races => RaceList.AsReadOnly();
        protected List<Race> RaceList { get; } = new List<Race>();

        protected Guid Id { get; private set; }

        // Comparators
        public virtual bool Equals(Election e) => e.Id.Equals(Id);

        public override bool Equals(Object o) {
            if (o is Election e)
                return e.Id.Equals(Id);
            throw new ArgumentException("o is not an Election object.");
        }

        public override int GetHashCode() => Id.GetHashCode();
        
        // No empty elections
        private Election() => throw new NotImplementedException();

        public Election(IEnumerable<Voter> voters, IEnumerable<Race> races, Guid id)
        {
            if (voters != null)
                VoterList.AddRange(voters);
            RaceList.AddRange(races);
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
        public Election(Election e) : this(e.Voters, e.Races, e.Id)
        {

        }

        // Throw an exception if the voter has already voted.
        public void Cast(ReadOnlyBallotSheet ballots, Voter voter)
        {
            if (VoterList.Contains(voter))
                throw new ArgumentException("Voter has already voted", "voter");
            VoterList.Add(voter);
            throw new NotImplementedException();
        }
    }
}
