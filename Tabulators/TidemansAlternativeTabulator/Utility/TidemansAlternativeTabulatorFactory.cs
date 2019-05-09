using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonsetTechnologies.Voting;
using MoonsetTechnologies.Voting.Ballots;

namespace MoonsetTechnologies.Voting.Utility
{ 
    // General algorithm:
    //   if CondorcetSet is One Candidate
    //     Winner is Candidate in CondorcetSet
    //   else
    //     Eliminate Candidates not in RetainSet
    //     Eliminate Candidate with Fewest Votes

    // Variants as (CondorcetSet, RetainSet):
    //   Tideman's Alternative:  (schwartz, smith)
    //   Tideman's Alternative Schwartz:  (schwartz, schwartz)
    //   Tideman's Alternative Smith:  (smith, smith)
    public class TidemansAlternativeTabulatorFactory : AbstractTabulatorFactory
    {
        TopCycle.TopCycleSets condorcetSet = TopCycle.TopCycleSets.schwartz;
        TopCycle.TopCycleSets retainSet = TopCycle.TopCycleSets.smith;

        public TidemansAlternativeTabulatorFactory WithCondorcetSet(TopCycle.TopCycleSets set)
        {
            TidemansAlternativeTabulatorFactory f = new TidemansAlternativeTabulatorFactory
            {
                condorcetSet = set,
                retainSet = set
            };
            return f;
        }

        public TidemansAlternativeTabulatorFactory WithRetainSet(TopCycle.TopCycleSets set)
        {
            TidemansAlternativeTabulatorFactory f = new TidemansAlternativeTabulatorFactory
            {
                condorcetSet = set,
                retainSet = set
            };
            return f;
        }

        private IBatchEliminator NewBatchEliminator(IEnumerable<Ballot> ballots)
        {
            return new TidemansAlternativeBatchEliminator(
                new RunoffBatchEliminator(new DifferenceTiebreakerFactory().CreateTiebreaker()),
                new TopCycle(ballots, condorcetSet),
                new TopCycle(ballots, retainSet));
        }

        public override AbstractTabulator CreateTabulator(IEnumerable<Candidate> candidates,
            IEnumerable<Ballot> ballots)
            => new RankedTabulator(candidates, ballots, NewBatchEliminator(ballots));

        public override Ballot CreateBallot(IEnumerable<Vote> votes)
        {
            throw new NotImplementedException();
        }

        public override Vote CreateVote(Candidate candidate, decimal value)
        {
            throw new NotImplementedException();
        }
    }
}