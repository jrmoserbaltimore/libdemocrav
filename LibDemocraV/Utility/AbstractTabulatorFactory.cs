using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public abstract class AbstractTabulatorFactory
    {
        protected AbstractTiebreakerFactory tiebreakerFactory;
        protected BallotFactory ballotFactory = new BallotFactory();
        public abstract AbstractTabulator CreateTabulator();

        public Ballot CreateBallot(IEnumerable<Vote> votes)
            => ballotFactory.CreateBallot(votes);

        public Vote CreateVote(Candidate candidate, decimal value)
            => ballotFactory.CreateVote(candidate, value);

        public void SetTiebreaker(AbstractTiebreakerFactory tiebreakerFactory)
        {
            this.tiebreakerFactory = tiebreakerFactory;
        }

        /// <summary>
        /// Configures the Tabulators created based on a settings object.
        /// </summary>
        /// <param name="tabulatorSetting">The setting to adjust</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the setting is ultimately not recognized for this Tabulator.</exception>
        protected virtual void ConfigureTabulator(ITabulatorSetting tabulatorSetting)
        {
            if (tabulatorSetting is TiebreakerTabulatorSetting)
                tiebreakerFactory = Activator.CreateInstance((tabulatorSetting as TiebreakerTabulatorSetting).Value) as AbstractTiebreakerFactory;
            else
                throw new ArgumentOutOfRangeException("tabulatorSetting", "ITabulatorSetting not applicable to tabulator.");
        }

        /// <summary>
        /// Configures the Tabulators created based on an array of settings objects.
        /// </summary>
        /// <param name="tabulatorSettings">The settings to adjust</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if any setting is ultimately not recognized for this Tabulator.</exception>
        /// <inheritdoc/>
        public void ConfigureTabulator(IEnumerable<ITabulatorSetting> tabulatorSettings)
        {
            foreach (var x in tabulatorSettings)
            {
                ConfigureTabulator(x);
            }
        }

        protected virtual IEnumerable<ITabulatorSetting> CreateSettings()
            => new List<ITabulatorSetting>();
    }

    public abstract class AbstractTabulatorFactory<T> : AbstractTabulatorFactory
        where T : AbstractTabulator
    {
        /// <inheritdoc/>
        public override AbstractTabulator CreateTabulator()
        {
            TabulationMediator mediator = new TabulationMediator
            {
                BallotFactory = ballotFactory
            };

            T t = Activator.CreateInstance(typeof(T), new object[] { mediator, tiebreakerFactory, CreateSettings() }) as T;
            return t;
        }
    }
}