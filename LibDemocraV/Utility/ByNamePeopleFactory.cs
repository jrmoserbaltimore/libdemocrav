using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public class ByNamePeopleFactory : AbstractPeopleFactory
    {
        public ByNamePeopleFactory()
        {
        }

        protected Dictionary<string, WeakReference<Person>> Names { get; } = new Dictionary<string, WeakReference<Person>>();

        /// <inheritdoc/>
        protected override Person FetchPerson(Person reference)
        {
            Person p = reference;

            // Fetch the person based on name
            if (!(Names.ContainsKey(reference.Name)
                && Names[reference.Name].TryGetTarget(out p)))
            {
                p = reference;
                Names[p.Name] = new WeakReference<Person>(p);
            }

            return p;
        }
    }
}
