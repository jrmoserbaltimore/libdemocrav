using System;
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

        protected Dictionary<string, Person> Names { get; } = new Dictionary<string, Person>();

        /// <inheritdoc/>
        protected override Person FetchPerson(Person reference)
        {
            Person p = reference;

            if (!Names.TryGetValue(p.Name, out p))
                p = Names[reference.Name] = reference;

            return p;
        }
    }
}
