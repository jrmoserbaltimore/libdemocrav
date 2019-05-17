using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Ballots;

namespace MoonsetTechnologies.Voting.Analytics
{
    // API for this:
    //  TopCycle t = TopCycle(ballots)
    //  IEnumerable<Candidate> SmithSet = t.SmithSet(candidateStates);
    //  IEnumerable<Candidate> SchwartzSet = t.SchwartzSet(candidateStates);
    /// <summary>
    /// Computes the Smith and Schwartz sets.
    /// </summary>
    public class TopCycle
    {
        protected List<Ballot> ballots;
        public enum TopCycleSets
        {
            smith = 1, // Generalize Top-Choice Assumption, GETCHA
            schwartz = 2 // Generalized Optimal-Choice Axiom, GOCHA
        }

        private readonly TopCycleSets defaultSet;
        public TopCycle(IEnumerable<Ballot> ballots, TopCycleSets set = TopCycleSets.smith)
        {
            this.ballots = ballots.ToList();
            defaultSet = set;
        }

        public IEnumerable<Candidate> GetTopCycle(IEnumerable<Candidate> candidates, TopCycleSets set)
            => ComputeSets(new PairwiseGraph(candidates, ballots), set);

        public IEnumerable<Candidate> GetTopCycle(IEnumerable<Candidate> candidates)
            => GetTopCycle(candidates, defaultSet);

        /// <summary>
        /// Compute Smith and Schwartz sets with Tarjan's Algorithm.
        /// </summary>
        /// <param name="graph">The pairwise graph.</param>
        private IEnumerable<Candidate> ComputeSets(PairwiseGraph graph, TopCycleSets set)
        {
            List<Candidate> smithSet = null;
            List<Candidate> schwartzSet;
            Dictionary<Candidate, int> linkId;
            Dictionary<Candidate, int> nodeId;
            List<List<Candidate>> stronglyConnectedComponents;
            Stack<Candidate> s;
            int i = 0;

            void dfs(Candidate c, bool isSmith)
            {
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
                HashSet<Candidate> neighbors = graph.Wins(c).ToHashSet();
                if (isSmith)
                    neighbors.UnionWith(graph.Ties(c));
                foreach (Candidate d in neighbors)
                {
                    // Visit first so it will be on the stack when we do the next check,
                    // unless it's already visited and thus won't be on the stack.
                    if (!nodeId.ContainsKey(d))
                    {
                        dfs(d, isSmith);
                        linkId[c] = Math.Min(linkId[c], linkId[d]);
                    }
                    // It's on the stack, so set linkId to the nodeId
                    else if (s.Contains(d))
                        linkId[c] = Math.Min(linkId[c], nodeId[d]);
                }
                // We've visited all neighbors, did we find a SCC?
                if (linkId[c] == nodeId[c])
                {
                    // move this SCC from the stack to our list of SCCs
                    List<Candidate> scc = new List<Candidate>();                    
                    do
                    {
                        scc.Add(s.Pop());
                    } while (!scc.Contains(c));
                    stronglyConnectedComponents.Add(scc);
                }
            }

            List<Candidate> getSet(bool isSmith)
            {
                linkId = new Dictionary<Candidate, int>();
                nodeId = new Dictionary<Candidate, int>();
                s = new Stack<Candidate>();
                stronglyConnectedComponents = new List<List<Candidate>>();
                i = 0;
                // Visit each node in the graph as a starting point.
                foreach (Candidate c in graph.Candidates)
                    dfs(c, isSmith);

                // Find every SCC that cannot be reached by any other SCC.
                // In the Smith Set, this is one SCC; in the Schwartz Set,
                // we may have several.
                Dictionary<(List<Candidate>, List<Candidate>), bool> reachable = new Dictionary<(List<Candidate>, List<Candidate>), bool>();
                List<List<Candidate>> dominating = new List<List<Candidate>>();
                List<Candidate> output = new List<Candidate>();

                // Special thanks to https://stackoverflow.com/a/55526085/5601193
                foreach (Candidate k in linkId.Keys)
                {
                    List<Candidate> scck = stronglyConnectedComponents.Where(x => x.Contains(k)).Single();
                    foreach(Candidate l in linkId.Keys)
                    {
                        List<Candidate> sccl = stronglyConnectedComponents.Where(x => x.Contains(l)).Single();
                        foreach (Candidate m in linkId.Keys)
                        {
                            List<Candidate> sccm = stronglyConnectedComponents.Where(x => x.Contains(m)).Single();
                            // Assigns from itself, below, sometimes, so must exist
                            if (!reachable.ContainsKey((sccl, sccm)))
                                reachable[(sccl, sccm)] = false;
                            if (reachable[(sccl, sccm)])
                                continue;
                            // The SCC containing (l) can reach the SCC containing (m) if
                            //  - (l) defeats (m)
                            //  - (l) ties with (m) and it's the Smith Set
                            //  - (l) is already known to reach (m)
                            //  - (l) can reach (k) and (k) can reach (m)
                            reachable[(sccl, sccm)] =
                                graph.Wins(l).Contains(m) ||
                                (isSmith && graph.Ties(l).Contains(m)) ||
                                reachable[(sccl, sccm)] ||
                                (reachable[(sccl, scck)] && reachable[(scck, sccm)]);
                        }
                    }
                }

                // Time to find all dominating SCCs
                dominating.AddRange(stronglyConnectedComponents);
                foreach (List<Candidate> j in stronglyConnectedComponents)
                {
                    foreach (List<Candidate> k in stronglyConnectedComponents)
                    {
                        // Reaching itself doesn't count
                        if (j == k)
                            continue;
                        // If we can reach j from k, j is not dominating
                        if (reachable[(k, j)])
                            dominating.Remove(j);
                    }
                }

                // Select all the candidates from the SCCs
                foreach (List<Candidate> scc in dominating)
                        output.AddRange(scc);
                return output;
            }

            // Return whichever
            if (set == TopCycleSets.smith)
                return getSet(true);
            else
                return getSet(false);
        }
    }
}
