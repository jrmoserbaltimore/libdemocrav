using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public abstract class TiebreakerFactory<T> : AbstractTiebreakerFactory
    where T : AbstractTiebreaker, new()
    {

        protected virtual AbstractTiebreaker InstantiateTiebreaker(AbstractTiebreaker fallbackTiebreaker)
          => Activator.CreateInstance(typeof(T), new object[] { fallbackTiebreaker }) as T;

        public override AbstractTiebreaker CreateTiebreaker(TabulationMediator mediator)
        {
            AbstractTiebreaker fallbackTiebreaker = TiebreakerFactory?.CreateTiebreaker(mediator);
            AbstractTiebreaker tiebreaker = InstantiateTiebreaker(fallbackTiebreaker);

            tiebreaker.Mediator = mediator;
            return tiebreaker;
        }
    }
}
