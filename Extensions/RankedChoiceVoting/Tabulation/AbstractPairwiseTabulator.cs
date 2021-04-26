// This uses pairwise tabulation.
// If using summable methods, produce summable pairwise ballot sets from the indivdual
// counts, and then load them all with the combining utility.
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Utility;
using System;
using System.Collections.Generic;
using System.Linq;


namespace MoonsetTechnologies.Voting.Tabulation
{
    public abstract class AbstractPairwiseTabulator : AbstractTabulator
    {
        protected PairwiseGraph pairwiseGraph;
        protected TopCycle topCycle;
        public AbstractPairwiseTabulator(TabulationMediator mediator,
            AbstractTiebreakerFactory tiebreakerFactory,
            IEnumerable<ITabulatorSetting> tabulatorSettings) : base(mediator, tiebreakerFactory, tabulatorSettings)
        {

        }

        protected override void InitializeTabulation(BallotSet ballots, IEnumerable<Candidate> withdrawn, int seats)
        {
            base.InitializeTabulation(ballots, withdrawn, seats);
            pairwiseGraph = new PairwiseGraph(ballots);
            topCycle = new TopCycle(pairwiseGraph);
        }

        protected IEnumerable<Candidate> GetNonSmithCandidates()
        {
            HashSet<Candidate> tc = new HashSet<Candidate>(topCycle.GetTopCycle(candidateStates
                .Where(x => x.Value.State != CandidateState.States.hopeful)
                .Select(x => x.Key), TopCycle.TopCycleSets.smith));
            return candidateStates
                    .Where(x => x.Value.State == CandidateState.States.hopeful)
                    .Select(x => x.Key)
                    .Except(tc);
        }

        // TODO:  Implement a TabulateRound() that can do the below:
        //  return new RankedTabulationStateEventArgs
        //  {
        //      CandidateStates = CandidateStatesCopy,
        //      SchwartzSet = (analytics as RankedTabulationAnalytics).GetSchwartzSet(candidateStates.Keys.Except(startSet)),
        //      SmithSet = (analytics as RankedTabulationAnalytics).GetSmithSet(candidateStates.Keys.Except(startSet)),
        //      PairwiseGraph = pairwiseGraph
        // };

    }
}
