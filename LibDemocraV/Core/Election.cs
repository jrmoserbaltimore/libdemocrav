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

        public virtual bool Equals(Election e)
        {
            if (e is null)
                return false;
            else if (ReferenceEquals(this, e))
                return true;
            return Id.Equals(e.Id);
        }

        public override bool Equals(Object obj) => Equals(obj as Election);

        public override int GetHashCode() => Id.GetHashCode();
        
        public static bool operator ==(Election lhs, Election rhs)
        {
            if (lhs is null && rhs is null)
                return true;
            else if (lhs is null)
                return false;
            else
                return lhs.Equals(rhs);
        }

        public static bool operator !=(Election lhs, Election rhs) => !(lhs == rhs);

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
