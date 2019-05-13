using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public class TiebreakerFactory<T> : AbstractTiebreakerFactory
    where T : AbstractTiebreaker, new()
    {
        public override AbstractTiebreaker CreateTiebreaker(TabulationMediator mediator)
        {
            return new T
            {
                Mediator = mediator
            };
        }
    }
}
