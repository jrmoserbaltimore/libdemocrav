using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public abstract class AbstractTiebreakerFactory
    {
        /// <summary>
        /// Fallack tiebreaker when the tie isn't broken.
        /// </summary>
        public AbstractTiebreakerFactory FallbackTiebreakerFactory { protected get; set; }
        public AbstractTiebreakerFactory()
        {
            CreateDefaultConfiguration();
        }

        /// <summary>
        /// Set up the default configuration
        /// </summary>
        protected abstract void CreateDefaultConfiguration();
        public abstract AbstractTiebreaker CreateTiebreaker(TabulationMediator mediator);

    }
}