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
        : AbstractTabulatorFactory<IRankedBallot, RankedTabulator>
    {
        readonly GenericTiebreakerFactory tiebreakerFactory;

        private ITiebreaker NewTiebreaker() => tiebreakerFactory.CreateTiebreaker();

        private IBatchEliminator NewBatchEliminator(ITiebreaker tiebreaker, TopCycle condorcetSet, TopCycle retentionSet)
        {
            return new TidemansAlternativeBatchEliminator(new RunoffBatchEliminator(tiebreaker),
                condorcetSet, retentionSet);
        }
        public TidemansAlternativeTabulatorFactory()
        {
            // FIXME:  Implement this in the tiebreaker factory
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

            // Alternate implementation:
            /*
            ITiebreaker firstDifference = new FirstDifferenceTiebreaker();
            ITiebreaker tiebreaker = new SeriesTiebreaker(
                new ITiebreaker[] {
                    new SequentialTiebreaker(
                        new ITiebreaker[] {
                          new LastDifferenceTiebreaker(),
                          firstDifference,
                        }.ToList()
                    ),
                    new LastDifferenceTiebreaker(),
                    firstDifference,
                }.ToList()
            );
            */
        }

        public override RankedTabulator CreateTabulator(IEnumerable<Candidate> candidates,
            IEnumerable<IRankedBallot> ballots)
          => CreateTabulator(candidates, ballots, null, null);

        public RankedTabulator CreateTabulator(IEnumerable<Candidate> candidates,
            IEnumerable<IRankedBallot> ballots,
            ITiebreaker tiebreaker = null,
            IBatchEliminator batchEliminator = null)
            => CreateTidemansAlternativeTabulator(candidates, ballots, tiebreaker, batchEliminator);

        public RankedTabulator CreateTabulator(IEnumerable<Candidate> candidates,
            IEnumerable<IRankedBallot> ballots,
            TopCycle condorcetSet,
            TopCycle retentionSet,
            ITiebreaker tiebreaker = null,
            IBatchEliminator batchEliminator = null)
        {
            RankedVoteCount voteCount;
            TidemansAlternativeBatchEliminator bE;
            // Standard tiebreaker
            if (tiebreaker is null)
                tiebreaker = NewTiebreaker();

            // The default batch eliminator is a standard runoff batch eliminator
            if (batchEliminator is null)
                batchEliminator = new RunoffBatchEliminator(tiebreaker);

            
            bE = new TidemansAlternativeBatchEliminator(batchEliminator, condorcetSet, retentionSet);
            voteCount = new RankedVoteCount(candidates, ballots, bE);

            return new RankedTabulator(voteCount);
        }

        // Predefined standard tabulators

        // General algorithm:
        //   if SchwartzSet is One Candidate
        //     Winner is Candidate in SchwartzSet
        //   else
        //     Eliminate Candidates not in SmithSet
        //     Eliminate Candidate with Fewest Votes

        public RankedTabulator CreateTidemansAlternativeTabulator(IEnumerable<Candidate> candidates,
            IEnumerable<IRankedBallot> ballots,
            ITiebreaker tiebreaker = null,
            IBatchEliminator batchEliminator = null)
        {
            TopCycle condorcetSet = new TopCycle(ballots, TopCycle.TopCycleSets.schwartz);
            TopCycle retentionSet = new TopCycle(ballots, TopCycle.TopCycleSets.smith);

            return CreateTabulator(candidates, ballots, condorcetSet, retentionSet, tiebreaker, batchEliminator);
        }

        // General algorithm:
        //   if SmithSet is One Candidate
        //     Winner is Candidate in SmithSet
        //   else
        //     Eliminate Candidates not in SmithSet
        //     Eliminate Candidate with Fewest Votes

        public RankedTabulator CreateTidemansAlternativeSmithTabulator(IEnumerable<Candidate> candidates,
            IEnumerable<IRankedBallot> ballots,
            ITiebreaker tiebreaker = null,
            IBatchEliminator batchEliminator = null)
        {
            TopCycle condorcetSet = new TopCycle(ballots, TopCycle.TopCycleSets.smith);
            TopCycle retentionSet = condorcetSet;

            return CreateTabulator(candidates, ballots, condorcetSet, retentionSet, tiebreaker, batchEliminator);
        }

        // General algorithm:
        //   if SchwartzSet is One Candidate
        //     Winner is Candidate in SchwartzSet
        //   else
        //     Eliminate Candidates not in SchwartzSet
        //     Eliminate Candidate with Fewest Votes

        public RankedTabulator CreateTidemansAlternativeSchwartzTabulator(IEnumerable<Candidate> candidates,
            IEnumerable<IRankedBallot> ballots,
            ITiebreaker tiebreaker = null,
            IBatchEliminator batchEliminator = null)
        {
            TopCycle condorcetSet = new TopCycle(ballots, TopCycle.TopCycleSets.schwartz);
            TopCycle retentionSet = condorcetSet;

            return CreateTabulator(candidates, ballots, condorcetSet, retentionSet, tiebreaker, batchEliminator);
        }
    }
}