using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public abstract class AbstractTiebreakerFactory
    {
        public AbstractTiebreakerFactory()
        {
        }

        public abstract ITiebreaker CreateTiebreaker();
    }
}
