using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Tabulators;
using MoonsetTechnologies.Voting.Tiebreakers;
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

        private ITiebreaker NewTiebreaker()
        {
            ITiebreaker firstDifference = new FirstDifference();
            ITiebreaker tiebreaker = new SeriesTiebreaker(
                new ITiebreaker[] {
                new SequentialTiebreaker(
                    new ITiebreaker[] {
                        new LastDifference(),
                        firstDifference,
                    }.ToList()
                ),
                new LastDifference(),
                firstDifference,
                }.ToList()
            );
            return tiebreaker;
        }
        private IBatchEliminator NewBatchEliminator(ITiebreaker tiebreaker = null)
        {
            if (tiebreaker is null)
                tiebreaker = NewTiebreaker();
            return new RunoffBatchEliminator(tiebreaker);
        }
        public TidemansAlternativeTabulatorFactory()
        {
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