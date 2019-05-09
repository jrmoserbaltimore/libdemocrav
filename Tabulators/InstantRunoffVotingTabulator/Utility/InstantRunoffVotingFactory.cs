using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    /*
    public class InstantRunoffVotingFactory
        : AbstractTabulatorFactory<IRankedBallot, RankedTabulator>
    {
        readonly GenericTiebreakerFactory tiebreakerFactory;

        private ITiebreaker NewTiebreaker() => tiebreakerFactory.CreateTiebreaker();

        private IBatchEliminator NewBatchEliminator(ITiebreaker tiebreaker)
           => new RunoffBatchEliminator(tiebreaker);
        public InstantRunoffVotingFactory()
            : base()
        {

        }

        public override RankedTabulator CreateTabulator(IEnumerable<Candidate> candidates,
            IEnumerable<IRankedBallot> ballots)
          => CreateTabulator(candidates, ballots, null, null);

        public RankedTabulator CreateTabulator(IEnumerable<Candidate> candidates,
            IEnumerable<IRankedBallot> ballots,
            ITiebreaker tiebreaker = null,
            IBatchEliminator batchEliminator = null)
        {
            RankedVoteCount voteCount;
            // Standard tiebreaker
            if (tiebreaker is null)
                tiebreaker = NewTiebreaker();

            // The default batch eliminator is a standard runoff batch eliminator
            if (batchEliminator is null)
                batchEliminator = NewBatchEliminator(tiebreaker);

            voteCount = new RankedVoteCount(candidates, ballots, batchEliminator);

            return new RankedTabulator(voteCount);
        }
    }
    */
}
