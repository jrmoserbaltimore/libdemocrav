using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public abstract class AbstractPeopleFactory
    {
        private DeduplicatorHashSet<Person> People { get; }

        public AbstractPeopleFactory()
        {
            People = new DeduplicatorHashSet<Person>(FetchPersonProxy);
        }
        /// <summary>
        /// Look up a Candidate based on a template Person.
        /// </summary>
        /// <param name="candidate">A Person to match as a Candidate.</param>
        /// <returns>A matching Candidate with correct details.</returns>
        public Candidate GetCandidate(Person candidate)
        {
            Candidate c;
            c = GetPerson(candidate) as Candidate;

            // Just make a new one
            // FIXME:  this has to produce actual correct state change instead of just handing out new candidates
            // if e.g. the person exists as a voter
            if (c is null)
            {
                c = new Candidate(candidate);
            }

            return c as Candidate;
        }

        /// <summary>
        /// Look up a Person based on a template Person.
        /// </summary>
        /// <param name="person">A Person to match as a Candidate.</param>
        /// <returns>A matching Person with correct details.</returns>
        public Person GetPerson(Person person) => People[person];

        /// <summary>
        /// Used to look up Person objects not found otherwise.
        /// </summary>
        /// <param name="reference">A reference Person object.</param>
        /// <returns>A retrieved object, or the same object if none is retrieved.</returns>
        protected abstract Person FetchPerson(Person reference);

        // This cannot be called until something calls GetPerson.
        // Calling GetPerson at any time during the constructor body
        // of any derived class thus may call a derived method before
        // the derived type state has been created.  Don't do it.
        private Person FetchPersonProxy(Person reference)
            => FetchPerson(reference);
    }
}
