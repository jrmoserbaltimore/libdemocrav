//
// Copyright (c) Moonset Technologies, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//


using System;

namespace MoonsetTechnologies.Voting
{
    /// <summary>
    /// Base Person class which holds information about Voters, Candidates,
    /// etc.
    /// </summary>
    public abstract class ReadOnlyPerson : IEquatable<ReadOnlyPerson>
    {
        /// <summary>
        /// Unique identifier for this person.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Determines whether the current Person object refers to the
        /// same person as the other Person object.
        /// </summary>
        /// <param name="other">The object to compare with the current
        /// object.</param>
        /// <returns></returns>
        public virtual bool Equals(ReadOnlyPerson other) => Id.Equals(other.Id);

        /// <summary>
        /// Determines whether the current object is equal to the other object.
        /// </summary>
        /// <param name="obj">The object to compare with the current
        /// object.</param>
        /// <returns></returns>
        public override bool Equals(Object obj)
        {
            if (obj is ReadOnlyPerson p)
                return Equals(p);
            throw new ArgumentException("obj is not a Person object.");
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => Id.GetHashCode();

        protected ReadOnlyPerson(Guid id)
        {
            Id = id;
        }

        /// <summary>
        /// Copy from another Person.
        /// </summary>
        /// <param name="person"></param>
        protected ReadOnlyPerson(ReadOnlyPerson person)
            : this(person.Id)
        {

        }
    }

    public class Person : ReadOnlyPerson
    {
        public Person()
            : base(Guid.NewGuid())
        {
            
        }
    }

    /// <summary>
    /// A Voter.
    /// </summary>
    public class Voter : ReadOnlyPerson
    {
        public Voter(ReadOnlyPerson person)
            : base(person)
        {

        }
    }

    // FIXME:  Refactor as Candidacy, not as a Candidate.
    /// <summary>
    /// A Candidate, which is a Person in a particular Race.
    /// </summary>
    public class Candidate : ReadOnlyPerson
    {
        public Race Race { get; }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash consistent for the Person and Race representing the Candidate.</returns>
        public override int GetHashCode() => HashCode.Combine(Id, Race.Id);

        public Candidate(ReadOnlyPerson person, Race race)
            : base(person)
        {
            Race = race;
        }
    }
}
