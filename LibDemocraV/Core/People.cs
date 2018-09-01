//
// Copyright (c) Moonset Technology Holdings, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//


using System;

namespace MoonsetTechnologies.Voting
{
    /// <summary>
    /// Base Person class which holds information about Voters, Candidates,
    /// etc.
    /// </summary>
    public abstract class Person : IEquatable<Person>
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
        public virtual bool Equals(Person other) => Id.Equals(other.Id);

        /// <summary>
        /// Determines whether the current object is equal to the other object.
        /// </summary>
        /// <param name="obj">The object to compare with the current
        /// object.</param>
        /// <returns></returns>
        public override bool Equals(Object obj)
        {
            if (obj is Person p)
                return Equals(p);
            throw new ArgumentException("obj is not a Person object.");
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => Id.GetHashCode();
    }

    /// <summary>
    /// A Voter.
    /// </summary>
    public class Voter : Person
    {

    }

    /// <summary>
    /// A Candidate, which is a Person in a particular Race.
    /// </summary>
    public class Candidate : Person
    {
        public Race Race { get; }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash consistent for the Person and Race representing the Candidate.</returns>
        public override int GetHashCode() => HashCode.Combine(Id, Race.Id);

    }
}
