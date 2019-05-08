using System;
using System.Collections.Generic;
using System.Text;
using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Tiebreaking;

namespace MoonsetTechnologies.Voting.Utility
{
    class TidemansAlternativeTabulatorBuilder : RankedTabulatorBuilder
    {
        protected AbstractTiebreakerFactory tiebreakerFactory = new DifferenceTiebreakerFactory();
        protected TopCycle condorcetSet;
        protected TopCycle retentionSet;
        public override void BuildBatchEliminator(IEnumerable<IRankedBallot> ballots)
        {
            IBatchEliminator bE = new RunoffBatchEliminator(tiebreaker);
            condorcetSet = new TopCycle(ballots);
            retentionSet = new TopCycle(ballots);
            this.ballots = ballots;
            batchEliminator = new TidemansAlternativeBatchEliminator(bE, condorcetSet, retentionSet);
        }

        public override void BuildVoteCounter(IEnumerable<Candidate> candidates)
        {
            voteCount = new RankedVoteCount(candidates, ballots, batchEliminator);
        }
        public override void BuildTabulator()
        {
            tabulator = new RankedTabulator(voteCount as RankedVoteCount);
        }

        public override void BuildTiebreaker()
        {
            tiebreaker = tiebreakerFactory.CreateTiebreaker();
        }

        public override void GetTabulator()
        {

        }
    }
}
