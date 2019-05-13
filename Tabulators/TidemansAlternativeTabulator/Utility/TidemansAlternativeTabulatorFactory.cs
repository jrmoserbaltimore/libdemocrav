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

        public TidemansAlternativeTabulatorFactory()
            : base()
        {
            //SetTiebreaker(new TiebreakerFactory<LastDifferenceTiebreaker>());
        }

        public TidemansAlternativeTabulatorFactory WithCondorcetSet(TopCycle.TopCycleSets set)
        {
            TidemansAlternativeTabulatorFactory f = new TidemansAlternativeTabulatorFactory
            {
                condorcetSet = set,
                retainSet = retainSet
            };
            return f;
        }

        public TidemansAlternativeTabulatorFactory WithRetainSet(TopCycle.TopCycleSets set)
        {
            TidemansAlternativeTabulatorFactory f = new TidemansAlternativeTabulatorFactory
            {
                condorcetSet = condorcetSet,
                retainSet = set
            };
            return f;
        }

        public override AbstractTabulator CreateTabulator()
        {
            TabulationMediator mediator = new TabulationMediator();

            TidemansAlternativeTabulator t = new TidemansAlternativeTabulator(mediator, tiebreakerFactory);
            return t;
        }

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