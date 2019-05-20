using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public class ByNamePeopleFactory : DeduplicatingPeopleFactory
    {
        protected Dictionary<string, int> Names { get; } = new Dictionary<string, int>();
        /// <inheritdoc/>
        protected override Person FetchPerson(Person person)
        {
            Person p = person;

            // Fetch the person based on name
            if (!(Names.ContainsKey(person.Name)
                && People[Names[person.Name]].TryGetTarget(out p)))
            {
                p = person;
            }
            Names[p.Name] = p.GetHashCode();

            return p;
        }
    }
}
