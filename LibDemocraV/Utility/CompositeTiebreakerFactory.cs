using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MoonsetTechnologies.Voting.Tiebreaking;

namespace MoonsetTechnologies.Voting.Utility
{
    public class CompositeTiebreakerFactory : AbstractTiebreakerFactory
    {
        Type compositeType = typeof(SequentialTiebreaker);

        List<AbstractTiebreakerFactory> tiebreakerFactories;

        public CompositeTiebreakerFactory()
        {
        }

        public void WithTiebreakers(List<Guid> tiebreakerIds)
        {

            tiebreakerFactories.Clear();

            foreach (Guid g in tiebreakerIds)
            {
                foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type t = a.GetTypes()
                        .Where(x => x.IsClass && x.IsSubclassOf(typeof(AbstractTiebreakerFactory))
                          && x.GetCustomAttributes(typeof(TiebreakerTypeId)).Single() != null).Single();
                    // FIXME:  Null check
                    tiebreakerFactories.Add(Activator.CreateInstance(t) as AbstractTiebreakerFactory);
                }
            }
        }

        public void WithTiebreakerFactories(IEnumerable<AbstractTiebreakerFactory> factories)
        {
            tiebreakerFactories = factories.ToList();
        }

        public override ITiebreaker CreateTiebreaker()
        {
            List<ITiebreaker> tiebreakers = new List<ITiebreaker>();
            foreach (AbstractTiebreakerFactory f in tiebreakerFactories)
                tiebreakers.Add(f.CreateTiebreaker());
            return Activator.CreateInstance(compositeType, new object[] { tiebreakers } ) as ITiebreaker;
        }
    }
}
