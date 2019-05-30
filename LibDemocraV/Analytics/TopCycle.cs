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
        public enum TopCycleSets
        {
            smith = 1, // Generalize Top-Choice Assumption, GETCHA
            schwartz = 2 // Generalized Optimal-Choice Axiom, GOCHA
        }

        private readonly TopCycleSets defaultSet;
        private readonly PairwiseGraph graph;

        public TopCycle(PairwiseGraph graph, TopCycleSets set = TopCycleSets.smith)
        {
            defaultSet = set;
            this.graph = graph;
        }

        public TopCycle(BallotSet ballots, TopCycleSets set = TopCycleSets.smith)
            : this(new PairwiseGraph(ballots))
        {
        }

        public IEnumerable<Candidate> GetTopCycle(IEnumerable<Candidate> withdrawn, TopCycleSets set)
            => ComputeSets(withdrawn, set);

        public IEnumerable<Candidate> GetTopCycle(IEnumerable<Candidate> withdrawn)
            => GetTopCycle(withdrawn, defaultSet);

        /// <summary>
        /// Compute Smith and Schwartz sets with Tarjan's Algorithm.
        /// </summary>
        /// <param name="graph">The pairwise graph.</param>
        private IEnumerable<Candidate> ComputeSets(IEnumerable<Candidate> withdrawn, TopCycleSets set)
        {
            Dictionary<Candidate, int> linkId;
            Dictionary<Candidate, int> nodeId;
            HashSet<HashSet<Candidate>> stronglyConnectedComponents;
            Stack<Candidate> s;
            int i = 0;

            void dfs(Candidate c, bool isSmith)
            {
                // skip withdrawn candidates
                if (withdrawn.Contains(c))
                    return;
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
                HashSet<Candidate> neighbors = graph.Wins(c).Except(withdrawn).ToHashSet();
                if (isSmith)
                    neighbors.UnionWith(graph.Ties(c).Except(withdrawn));

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
                    HashSet<Candidate> scc = new HashSet<Candidate>();                    
                    do
                    {
                        scc.Add(s.Pop());
                    } while (!scc.Contains(c));
                    stronglyConnectedComponents.Add(scc);
                }
            }

            HashSet<Candidate> getSet(bool isSmith)
            {
                linkId = new Dictionary<Candidate, int>();
                nodeId = new Dictionary<Candidate, int>();
                s = new Stack<Candidate>();
                stronglyConnectedComponents = new HashSet<HashSet<Candidate>>();
                i = 0;
                // Visit each node in the graph as a starting point, ignoring withdrawn candidates
                foreach (Candidate c in graph.Candidates.Except(withdrawn))
                    dfs(c, isSmith);

                // Find every SCC that cannot be reached by any other SCC.
                // In the Smith Set, this is one SCC; in the Schwartz Set,
                // we may have several.
                Dictionary<(HashSet<Candidate>, HashSet<Candidate>), bool> reachable = new Dictionary<(HashSet<Candidate>, HashSet<Candidate>), bool>();
                HashSet<HashSet<Candidate>> dominating = new HashSet<HashSet<Candidate>>();
                HashSet<Candidate> output = new HashSet<Candidate>();

                // Special thanks to https://stackoverflow.com/a/55526085/5601193
                foreach (Candidate k in linkId.Keys.Except(withdrawn))
                {
                    HashSet<Candidate> scck = stronglyConnectedComponents.Where(x => x.Contains(k)).Single();
                    foreach(Candidate l in linkId.Keys.Except(withdrawn))
                    {
                        HashSet<Candidate> sccl = stronglyConnectedComponents.Where(x => x.Contains(l)).Single();
                        foreach (Candidate m in linkId.Keys.Except(withdrawn))
                        {
                            HashSet<Candidate> sccm = stronglyConnectedComponents.Where(x => x.Contains(m)).Single();
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
                dominating.UnionWith(stronglyConnectedComponents);
                foreach (HashSet<Candidate> j in stronglyConnectedComponents)
                {
                    foreach (HashSet<Candidate> k in stronglyConnectedComponents)
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
                foreach (HashSet<Candidate> scc in dominating)
                        output.UnionWith(scc);
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
