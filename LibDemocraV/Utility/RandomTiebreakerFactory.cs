using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public class RandomTiebreakerFactory : TiebreakerFactory<RandomTiebreaker>
    {
        /// <inheritdoc/>
        public RandomTiebreakerFactory() : base()
        {
        }

        protected override void CreateDefaultConfiguration()
          => FallbackTiebreakerFactory = null;
    }
}
