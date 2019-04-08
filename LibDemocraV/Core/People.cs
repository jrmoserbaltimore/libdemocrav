using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting
{
    /// <summary>
    /// Base Person class which holds information about Voters, Candidates,
    /// etc.
    /// </summary>
    public class Person : IEquatable<Person>
    {
        /// <summary>
        /// Unique identifier for this person.
        /// </summary>
        public Guid Id { get; }
        public String Name { get; }

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


        private Person(Guid id)
        {
            Id = id;
        }

        public Person(string name)
            : this()
        {
            Name = name;
        }

        /// <summary>
        /// Copy from another Person.
        /// </summary>
        /// <param name="person"></param>
        protected Person(Person person)
            : this(person.Id)
        {

        }

        protected Person()
            : this(Guid.NewGuid())
        {

        }
    }

    /// <summary>
    /// A Voter.
    /// </summary>
    public class Voter : Person
    {
        public Voter(Person person)
            : base(person)
        {

        }

        public Voter()
            : base()
        {

        }
    }

    /// <summary>
    /// A Candidate, which is a Person in a particular Race.
    /// </summary>
    public class Candidate : Person
    {
        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash consistent for the Person and Race representing the Candidate.</returns>
        //public override int GetHashCode() => HashCode.Combine(Id, Race.Id);

        public Candidate(Person person)
            : base(person)
        {

        }
    }

}
