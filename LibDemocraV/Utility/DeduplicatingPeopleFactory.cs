using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public abstract class DeduplicatingPeopleFactory : AbstractPeopleFactory
    {
        protected Dictionary<int, WeakReference<Person>> People { get; set; } = new Dictionary<int, WeakReference<Person>>();

        /// <summary>
        /// Use the information in a Person object to find a matching stored Person.
        /// </summary>
        /// <param name="person">The template Person.</param>
        /// <returns>An appropriate Person match.</returns>
        protected abstract Person FetchPerson(Person person);

        /// <inheritdoc/>
        public override Person GetPerson(Person person)
        {
            Person p = null;
            // Go through the trouble of deduplicating the candidate
            if (!(People.ContainsKey(person.GetHashCode())
                  && People[person.GetHashCode()].TryGetTarget(out p)))
            {
                // No such entry, so fetch a new one and add it to the table
                p = FetchPerson(person);
                People[p.GetHashCode()] = new WeakReference<Person>(p);
            }
            return p;

        }
        public override Candidate GetCandidate(Person candidate)
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

    }
}
