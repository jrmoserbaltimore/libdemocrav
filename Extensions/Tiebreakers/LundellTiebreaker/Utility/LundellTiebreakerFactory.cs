using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    class LundellTiebreakerFactory : TiebreakerFactory<LundellTiebreaker>
    {
        public LundellTiebreakerFactory() : base()
        {
        }

        protected override void CreateDefaultConfiguration()
          => FallbackTiebreakerFactory = new LastDifferenceTiebreakerFactory();
    }
}
