using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonsetTechnologies.Voting;

namespace MoonsetTechnologies.Voting.Utility
{
    public class TidemansAlternativeTabulatorFactory 
        : AbstractTabulatorFactory<IRankedBallot, IRankedTabulator>
    {
        readonly GenericTiebreakerFactory tiebreakerFactory;

        private ITiebreaker NewTiebreaker() => tiebreakerFactory.CreateTiebreaker();

        private IBatchEliminator NewBatchEliminator(ITiebreaker tiebreaker = null)
        {
            if (tiebreaker is null)
                tiebreaker = NewTiebreaker();
            return new RunoffBatchEliminator(tiebreaker);
        }
        public TidemansAlternativeTabulatorFactory()
        {
            // Generic tiebreaker factory is:
            //   SeriesTiebreaker
            //   {
            //     SequentialTiebreaker
            //     {
            //       LastDifferenceTiebreaker,
            //       FirstDifferenceTiebreaker
            //     },
            //     firstDifferenceTiebreaker
            //   }

            AbstractTiebreakerFactory f;
            AbstractTiebreakerFactory firstDifference = new GenericTiebreakerFactory(typeof(FirstDifferenceTiebreaker));
            List<AbstractTiebreakerFactory> factories = new List<AbstractTiebreakerFactory>();
            factories.Add(new GenericTiebreakerFactory(typeof(LastDifferenceTiebreaker)));
            factories.Add(firstDifference);
            f = new GenericTiebreakerFactory(typeof(SequentialTiebreaker), factories);

            factories = new List<AbstractTiebreakerFactory>();
            factories.Add(f);
            factories.Add(firstDifference);
            f = new GenericTiebreakerFactory(typeof(SeriesTiebreaker), factories);
            tiebreakerFactory = f as GenericTiebreakerFactory;
        }

        public override IRankedTabulator CreateTabulator(IEnumerable<Candidate> candidates,
            IEnumerable<IRankedBallot> ballots)
          => CreateTabulator(candidates, ballots, null, null);
      
        public IRankedTabulator CreateTabulator(IEnumerable<Candidate> candidates,
            IEnumerable<IRankedBallot> ballots,
            ITiebreaker tiebreaker = null,
            IBatchEliminator batchEliminator = null)
        {
            if (tiebreaker is null)
                tiebreaker = NewTiebreaker();
            if (batchEliminator is null)
                batchEliminator = NewBatchEliminator(tiebreaker);

            return new TidemansAlternativeTabulator(candidates, ballots, tiebreaker, batchEliminator);
        }
    }
}