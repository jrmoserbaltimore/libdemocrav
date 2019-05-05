using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public class GenericTiebreakerFactory : AbstractTiebreakerFactory
    {
        private readonly Type tiebreakerType;
        private readonly List<AbstractTiebreakerFactory> factories = null;

        /// <summary>
        /// A generic tiebreaker factory.
        /// </summary>
        /// <param name="t">Tiebreaker type.</param>
        /// <param name="factories">Factories to create a list of tiebreakers for this tiebraeker.</param>
        public GenericTiebreakerFactory(Type t, IEnumerable<AbstractTiebreakerFactory> factories = null)
        {
            if (!(t.GetInterfaces().Contains(typeof(ITiebreaker))))
                throw new ArgumentException();
            tiebreakerType = t;
            if (!(factories is null))
                this.factories = factories.ToList();
        }

        public override ITiebreaker CreateTiebreaker()
        {
            // Call the empty constructor
            if (factories is null)
                return Activator.CreateInstance(tiebreakerType) as ITiebreaker;
            // Call the Constructor(IEnumerable<ITiebreaker> tiebreakers) constructor
            // using tiebreakers created from the factories
            else
            {
                List<ITiebreaker> t = new List<ITiebreaker>();
                foreach (AbstractTiebreakerFactory f in factories)
                    t.Add(f.CreateTiebreaker());
                return Activator.CreateInstance(tiebreakerType, new object[] { t }) as ITiebreaker;
            }
        }
    }
}
