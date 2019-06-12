using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    // For testing ballot formats
    public class ByNamePeopleFactory : AbstractPeopleFactory
    {
        public ByNamePeopleFactory()
        {
        }

        protected ConcurrentDictionary<string, WeakReference<Person>> Names { get; } = new ConcurrentDictionary<string, WeakReference<Person>>();

        /// <inheritdoc/>
        protected override Person FetchPerson(Person reference)
        {
            Person p;
            if (reference is null)
                throw new ArgumentNullException("reference", "reference cannot be null!");
            if (!(Names.TryGetValue(reference.Name, out WeakReference<Person> wp)
                && wp.TryGetTarget(out p)))
            {
                Names[reference.Name] = new WeakReference<Person>(reference);
                p = reference;
            }
            return p;
        }
    }
}
