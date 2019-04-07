// These classes compute pairwise victories from complete ranked ballot sets.
// These pairwise victories allow us to compute the Smith and Schwartz sets,
// and thus support Condorcet systems like Smith-constrained IRV and Tideman's
// Alternative.
using System;
using System.Collections.Generic;
using System.Text;


// FIXME:  The API for this is simply:
//
//     PairwiseGraph g = new PairwiseGraph(candidates, ballots);
//     g.Wins(candidate);
//     g.Ties(candidate);
//     g.Losses(candidate);
//
// We need to extend this to track the number of actual votes for and against
// each candidate in the pairs, allowing more-robust statistics and compuattions.
//
// Aside from that, the internals are ham-fisted.
namespace MoonsetTechnologies.Voting.Analytics
{
    /// <summary>
    /// Converts a set of candidates and a set of ballots into a graph of wins and ties.
    /// </summary>
    class PairwiseGraph
    {
        // XXX:  Not exposing this ugly thing to clients.
        protected class GraphNode
        {
            private struct Path
            {
                public GraphNode Node;
                public int Votes;
                public Path(GraphNode node)
                    : this(node,0)
                {

                }

                public Path(GraphNode node, int votes)
                {
                    Node = node;
                    Votes = votes;
                }
            }

            // FIXME:  Refactor to track votes cast instead of running difference.
            private Dictionary<Candidate, Path> opponents = new Dictionary<Candidate, Path>();

            public Candidate Candidate { get; private set; }

            public GraphNode(Candidate candidate)
            {
                this.Candidate = candidate;
            }

            /// <summary>
            /// Add a vote for this Candidate against another Candidate.
            /// </summary>
            /// <param name="opponent">The opponent in the pairwise race.</param>
            public void Increment(Candidate opponent)
            {
                Path p;
                if (!opponents.ContainsKey(opponent))
                    throw new ArgumentOutOfRangeException("opponent", "Opponent does not appear to be an opponent in this graph.");
                p = opponents[opponent];
                // Sanity check
                if (!p.Node.opponents.ContainsKey(Candidate))
                    throw new ArgumentOutOfRangeException("opponent", "Opponent does not reference this node as an opponent.");
                // Add a vote for us against this opponent.
                opponents[opponent] = new Path(p.Node, p.Votes+1);
            }

            /// <summary>
            /// Add a GraphNode to this one, summing the vote counts.
            /// 
            /// Ignores candidates not in this graph.
            /// </summary>
            /// <param name="input">The GraphNode to add.</param>
            public void Add(GraphNode input)
            {
                // Candidates must be the same
                if (input.Candidate != Candidate)
                    throw new ArgumentException("Candidate does not match the candidate for this node!");

                foreach (Candidate c in opponents.Keys)
                {
                    if (!input.opponents.ContainsKey(c))
                        throw new ArgumentException("Provided graph node is missing candidates in this graph.", "input");
                    if (input.opponents[c].Votes < 0)
                        throw new ArgumentOutOfRangeException("input", "Provided graph node contains negative vote counts.");
                    opponents[c] = new Path(opponents[c].Node, opponents[c].Votes + input.opponents[c].Votes);
                }
            }

            /// <summary>
            /// Connects a neighboring node.
            /// </summary>
            /// <param name="opponent">The opponent.</param>
            public void ConnectNeighbor(GraphNode opponent)
            {
                if (opponent.Candidate == Candidate)
                    throw new InvalidOperationException("Cannot connect to self as neighbor.");
                // Create our reference if it doesn't exist.
                if (opponents.ContainsKey(opponent.Candidate))
                {
                    if (opponents[opponent.Candidate].Node != opponent)
                        throw new ArgumentException("Already have an opponent node for this candidate.", "opponent");
                }
                else
                    opponents[opponent.Candidate] = new Path(opponent);

                if (!opponent.opponents.ContainsKey(Candidate))
                    opponent.ConnectNeighbor(this);
            }
        }

        // The whole graph
        private Dictionary<Candidate, GraphNode> graph = new Dictionary<Candidate, GraphNode>();

        private PairwiseGraph(IEnumerable<Candidate> candidates)
        {

        }
        /// <summary>
        /// Converts a set of candidates and ballots to a graph of wins and ties.
        /// </summary>
        /// <param name="candidates">Candidates to be considered in the race.</param>
        /// <param name="ballots">Ranked ballots in the election.</param>
        public PairwiseGraph(IEnumerable<Candidate> candidates, IEnumerable<IBallot> ballots)
        {
            // Initialize to include only those candidates about whom we care.
            // Ballots may include eliminated candidates.
            foreach (Candidate c in candidates)
                graph[c] = new GraphNode(c);
            // Set all to ties
            foreach (GraphNode g in graph.Values)
            {
                foreach (GraphNode j in graph.Values)
                    g.ConnectNeighbor(j);
            }

            // Iterate each ballot and count who wins and who ties.
            // This can support tied ranks and each ballot is O(SUM(1..n)) and o(n).
            foreach (IBallot b in ballots)
            {
                // Track who is not ranked by the end
                List<Candidate> unranked = new List<Candidate>(graph.Keys);
                foreach (Vote v in b.Votes)
                    unranked.Remove(v.Candidate);

                // Iterate to compare each pair.
                List<Vote> votes = new List<Vote>(b.Votes);
                for (int i = 0; i < votes.Count; i++)
                {
                    // Candidate is not counted, so skip
                    if (!graph.ContainsKey(votes[i].Candidate))
                        continue;
                    for (int j=i+1; j < votes.Count; j++)
                    {
                        Candidate w = null, l = null;
                        // Candidate is not counted, so skip
                        if (!graph.ContainsKey(votes[j].Candidate))
                            continue;
                        // Who is ranked first?
                        if (votes[i].Value < votes[j].Value)
                        {
                            w = votes[i].Candidate;
                            l = votes[j].Candidate;
                        }
                        else if (votes[j].Value < votes[i].Value)
                        {
                            w = votes[j].Candidate;
                            l = votes[i].Candidate;
                        }
                        // No change if a tie, otherwise increment the winner.
                        if (w != null)
                            graph[w].Increment(l);
                    }
                    // Defeat all unranked candidates
                    foreach (Candidate c in unranked)
                        graph[votes[i].Candidate].Increment(c);
                }
            }
        }

        // XXX:  Do we want to do this, or to expose a Merge() member?
        /// <summary>
        /// Merge two PairwiseGraphs.
        /// </summary>
        /// <param name="g1">Graph 1</param>
        /// <param name="g2">Graph 2</param>
        public PairwiseGraph(PairwiseGraph g1, PairwiseGraph g2)
        {
            // With 100,000,000 ballots—a fully-counted Presidential election—this comes
            // to about O((n^2)/2 + n/2) times a linear hundred million.  Assuming nine
            // candidates—the maximum for Unified Majority with 54 primary candidates—gives
            // around 4.5 seconds per 1GHz per clock cycle required to compute this entire
            // loop.  Approximating 250 cycles per loop iteration gives ten minutes of
            // computation time at 2GHz.
            //
            // To reduce this computation time, we can split the ballots into smaller sets
            // and count them separately on separate cores.
            // 
            // With 4 cores, the above computation falls to 2 minutes 20 seconds.  With a
            // six -core AMD Ryzen with SMT, this falls to 47 seconds.  With a Ryzen
            // Threadripper 2990WX at 4.2GHz with 64 threads, we get 4.18 seconds.
            //
            // Computing an election may take up to n-2 iterations for n candidates, so
            // an $800 CPU instead of a $100 CPU is a worthwhile investment for a central
            // tabulator.

            IEnumerable<Candidate> candidates = g1.graph.Keys;

            // These must use the same candidates.
            // FIXME:  Validate the exceptions here are correct practice.
            foreach (Candidate c in g1.graph.Keys)
            {
                if (!g2.graph.ContainsKey(c))
                    throw new ArgumentException("Graphs do not contain the same candidates.");
            }
            foreach (Candidate c in g2.graph.Keys)
            {
                if (!g1.graph.ContainsKey(c))
                    throw new ArgumentException("Graphs do not contain the same candidates.");
            }

            // Initialize to an empty graph
            foreach (GraphNode g in g1.graph.Values)
                graph[g.Candidate] = new GraphNode(g.Candidate);
            foreach (GraphNode g in graph.Values)
            {
                foreach (GraphNode j in graph.Values)
                    g.ConnectNeighbor(j);
            }

            // Add the two graphs to this empty graph.
            foreach (GraphNode g in g1.graph.Values)
                graph[g.Candidate].Add(g);
            foreach (GraphNode g in g2.graph.Values)
                graph[g.Candidate].Add(g);
        }
    }

    // TODO:  PairwiseGraph derivative class which divides the ballots into (n) equal segments and
    // parallel-executes (n) counts, then puts this all together.
}
