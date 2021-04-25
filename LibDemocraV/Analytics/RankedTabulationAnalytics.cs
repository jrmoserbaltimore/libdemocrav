using System;
using System.Collections.Generic;
using System.Text;
using MoonsetTechnologies.Voting.Ballots;

namespace MoonsetTechnologies.Voting.Analytics
{
    public class RankedTabulationAnalytics
    {
        protected TopCycle topCycle;
        public PairwiseGraph pairwiseGraph { get; protected set; }

        public RankedTabulationAnalytics(BallotSet ballots, int seats = 1)
        {
            pairwiseGraph = new PairwiseGraph(ballots);
            topCycle = new TopCycle(pairwiseGraph);
        }

        public IEnumerable<Candidate> GetTopCycle(IEnumerable<Candidate> withdrawn, TopCycle.TopCycleSets set)
            => topCycle.GetTopCycle(withdrawn, set);

        public IEnumerable<Candidate> GetSmithSet(IEnumerable<Candidate> withdrawn)
            => GetTopCycle(withdrawn, TopCycle.TopCycleSets.smith);
        public IEnumerable<Candidate> GetSchwartzSet(IEnumerable<Candidate> withdrawn)
            => GetTopCycle(withdrawn, TopCycle.TopCycleSets.schwartz);
    }
}
