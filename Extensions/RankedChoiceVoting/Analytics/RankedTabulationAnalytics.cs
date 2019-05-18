using System;
using System.Collections.Generic;
using System.Text;
using MoonsetTechnologies.Voting.Ballots;

namespace MoonsetTechnologies.Voting.Analytics
{
    public class RankedTabulationAnalytics : AbstractTabulationAnalytics
    {
        protected TopCycle topCycle;
        protected PairwiseGraph pairwiseGraph;
        // FIXME:  Add a PairwiseGraph
        public RankedTabulationAnalytics(IEnumerable<Ballot> ballots, int seats = 1) : base(ballots, seats)
        {
            topCycle = new TopCycle(ballots);
            pairwiseGraph = null;
        }

        public IEnumerable<Candidate> GetTopCycle(IEnumerable<Candidate> candidates, TopCycle.TopCycleSets set)
            => topCycle.GetTopCycle(candidates, set);

        public IEnumerable<Candidate> GetSmithSet(IEnumerable<Candidate> candidates)
            => GetTopCycle(candidates, TopCycle.TopCycleSets.smith);
        public IEnumerable<Candidate> GetSchwartzSet(IEnumerable<Candidate> candidates)
            => GetTopCycle(candidates, TopCycle.TopCycleSets.schwartz);

        public PairwiseGraph GetPairwiseGraph()
            => new PairwiseGraph(pairwiseGraph);
    }
}
