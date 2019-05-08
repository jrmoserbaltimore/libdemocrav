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

        private IBatchEliminator NewBatchEliminator(IBatchEliminator batchEliminator)
        {

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
            IBatchEliminator bE;
            // Standard tiebreaker
            if (tiebreaker is null)
                tiebreaker = NewTiebreaker();

            // The default batch eliminator is a standard runoff batch eliminator
            if (batchEliminator is null)
                batchEliminator = new RunoffBatchEliminator(tiebreaker);

            
            bE = NewBatchEliminator(batchEliminator, condorcetSet, retentionSet);
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

        public override IBatchEliminator CreateBatchEliminator()
        {
            TopCycle condorcetSet = new TopCycle(ballots, TopCycle.TopCycleSets.schwartz);
            TopCycle retentionSet = new TopCycle(ballots, TopCycle.TopCycleSets.smith);
            return new TidemansAlternativeBatchEliminator(batchEliminator,
                condorcetSet, retentionSet);

        }

        public override ITiebreaker CreateTiebreaker()
        {
            throw new NotImplementedException();
        }
    }
}