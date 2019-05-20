using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public abstract class AbstractPeopleFactory
    {
        /// <summary>
        /// Look up a Candidate based on a template Person.
        /// </summary>
        /// <param name="candidate">A Person to match as a Candidate.</param>
        /// <returns>A matching Candidate with correct details.</returns>
        public abstract Candidate GetCandidate(Person candidate);

        public abstract Person GetPerson(Person person);
    }
}
