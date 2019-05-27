using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public class LastDifferenceTiebreakerFactory : TiebreakerFactory<LastDifferenceTiebreaker>
    {
        public LastDifferenceTiebreakerFactory() : base()
        {
        }

        protected override void CreateDefaultConfiguration()
          => TiebreakerFactory = new RandomTiebreakerFactory();
    }
}
