using System;
using System.Collections.Generic;
using System.Text;
using MoonsetTechnologies.Voting.Analytics;

namespace MoonsetTechnologies.Voting.Tabulators
{
    /// <summary>
    /// Computes the Smith and Schwartz sets.
    /// </summary>
    class TopCycle
    {
        protected List<Candidate> smithSet;
        protected List<Candidate> schwartzSet;
        public IEnumerator<Candidate> SmithSet => smithSet.GetEnumerator();
        public IEnumerator<Candidate> SchwartzSet => schwartzSet.GetEnumerator();

        public TopCycle(IEnumerable<Candidate> candidates, IEnumerable<IRankedBallot> ballots)
            : this(new PairwiseGraph(candidates, ballots))
        {
            
        }

        public TopCycle(PairwiseGraph graph)
        {
            ComputeSets(graph);
        }

        private void ComputeSets(PairwiseGraph graph)
        {

        }

    }
}
