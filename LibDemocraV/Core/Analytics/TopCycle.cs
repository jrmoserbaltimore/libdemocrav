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

        /// <summary>
        /// Compute Smith and Schwartz sets with Tarjan's Algorithm.
        /// </summary>
        /// <param name="graph">The pairwise graph.</param>
        private void ComputeSets(PairwiseGraph graph)
        {
            Dictionary<Candidate, int> linkId = new Dictionary<Candidate, int>();
            Dictionary<Candidate, int> nodeId = new Dictionary<Candidate, int>();
            Stack<Candidate> s = new Stack<Candidate>();
            int i = 0;

            void dfs(Candidate c, bool isSmith)
            {
                // Skip this node when isSmith is false and the node is not in the
                // Smith Set, of which the Schwartz Set is a subset.
                if (!isSmith && !smithSet.Contains(c))
                // Only search if not yet visited.
                if (nodeId.ContainsKey(c))
                    return;
                // put onto the stack
                s.Push(c);
                // Set the node's linkId and nodeId, then increment i
                nodeId[c] = i;
                linkId[c] = i;
                i++;

                // Visit each neighbor
                List<Candidate> neighbors = new List<Candidate>(graph.Wins(c));
                if (isSmith)
                    neighbors.AddRange(graph.Ties(c));
                foreach (Candidate d in graph.Wins(c))
                {
                    // Visit first so it will be on the stack when we do the next check,
                    // unless it's already visited and thus won't be on the stack.
                    dfs(d, isSmith);
                    // It's on the stack, so set linkId to the low-link value
                    if (s.Contains(d))
                        linkId[c] = Math.Min(linkId[c], linkId[d]);
                }
                // We've visited all neighbors, did we find a SCC?
                if (linkId[c] == nodeId[c])
                {
                    // Remove all associated members of the SCC
                    while (linkId[s.Peek()] == linkId[c])
                        s.Pop();
                }
            }

            List<Candidate> getSet(bool isSmith)
            {
                // Visit each node in the graph as a starting point.
                foreach (Candidate c in graph.Candidates)
                    dfs(c, false);

                // Find every SCC that cannot be reached by any other SCC.
                // In the Smith Set, this is one SCC; in the Schwartz Set,
                // we may have several.

                throw new NotImplementedException();
            }

            // Must compute SmithSet first
            smithSet = getSet(true);
            schwartzSet = getSet(false);
        }
    }
}
