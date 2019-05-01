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
//     g.GetVoteCount(c1, g2);
//
// Aside from that, the internals are ham-fisted.
namespace MoonsetTechnologies.Voting.Analytics
{
    /// <summary>
    /// Converts a set of candidates and a set of ballots into a graph of wins and ties.
    /// </summary>
    public class PairwiseGraph
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

            public (int v1, int v2) GetVoteCount(Candidate opponent)
            {
                int v1, v2;
                GraphNode g;
                if (!opponents.ContainsKey(opponent))
                    throw new ArgumentOutOfRangeException("opponent", "Opponent does not appear to be an opponent in this graph.");
                v1 = opponents[opponent].Votes;
                g = opponents[opponent].Node;

                if (!g.opponents.ContainsKey(Candidate))
                    throw new ArgumentOutOfRangeException("opponent", "Opponent does not reference this node as an opponent.");
                v2 = g.opponents[Candidate].Votes;
                return new ValueTuple<int, int>(v1, v2);
            }
        }

        // The whole graph
        protected Dictionary<Candidate, GraphNode> graph = new Dictionary<Candidate, GraphNode>();

        /// <summary>
        /// All candidates in this graph.
        /// </summary>
        public virtual IEnumerable<Candidate> Candidates => graph.Keys;

        private PairwiseGraph(IEnumerable<Candidate> candidates)
        {
            // Initialize to an empty graph including only those candidates
            // about whom we care.  Ballots may include eliminated candidates.
            foreach (Candidate c in candidates)
                graph[c] = new GraphNode(c);
            // Connect all candidates.
            foreach (GraphNode g in graph.Values)
            {
                foreach (GraphNode j in graph.Values)
                {
                    if (g != j)
                        g.ConnectNeighbor(j);
                }
            }
        }
        /// <summary>
        /// Converts a set of candidates and ballots to a graph of wins and ties.
        /// </summary>
        /// <param name="candidates">Candidates to be considered in the race.</param>
        /// <param name="ballots">Ranked ballots in the election.</param>
        public PairwiseGraph(IEnumerable<Candidate> candidates, IEnumerable<IRankedBallot> ballots)
            : this(candidates)
        {
            // Iterate each ballot and count who wins and who ties.
            // This can support tied ranks and each ballot is O(SUM(1..n)) and o(n).
            foreach (IRankedBallot b in ballots)
            {
                // Track who is not ranked by the end
                List<Candidate> unranked = new List<Candidate>(graph.Keys);
                foreach (IRankedVote v in b.Votes)
                    unranked.Remove(v.Candidate);

                // Iterate to compare each pair.
                List<IRankedVote> votes = new List<IRankedVote>(b.Votes);
                for (int i = 0; i < votes.Count; i++)
                {
                    // Candidate is not counted, so skip
                    if (!graph.ContainsKey(votes[i].Candidate))
                        continue;
                    for (int j=i+1; j < votes.Count; j++)
                    {
                        // Candidate is not counted, so skip
                        if (!graph.ContainsKey(votes[j].Candidate))
                            continue;
                        // Who is ranked first?  No action if a tie.
                        if (votes[i].Beats(votes[j]))
                            graph[votes[i].Candidate].Increment(votes[j].Candidate);
                        else if (votes[j].Beats(votes[i]))
                            graph[votes[j].Candidate].Increment(votes[i].Candidate);
                    }
                    // Defeat all unranked candidates
                    foreach (Candidate c in unranked)
                        graph[votes[i].Candidate].Increment(c);
                }
            }
        }

        /// <summary>
        /// Merge two PairwiseGraphs.
        /// </summary>
        /// <param name="g1">Graph 1</param>
        /// <param name="g2">Graph 2, which contains a strict superset of candidates in g1.</param>
        public PairwiseGraph(PairwiseGraph g1, PairwiseGraph g2)
            : this(g1.Candidates)
        {
            AddGraph(g1);
            AddGraph(g2);
        }

        /// <summary>
        /// Create a PairwiseGraph for a subset of candidates.
        /// </summary>
        /// <param name="source">The source graph.</param>
        /// <param name="candidates">The candidates to include.</param>
        public PairwiseGraph(PairwiseGraph source, IEnumerable<Candidate> candidates)
            : this(candidates)
        {
            AddGraph(source);
        }

        private void AddGraph(PairwiseGraph g)
        {
            // Must use a superset of our candidates.
            // XXX:  Validate the exceptions here are correct practice.
            foreach (Candidate c in Candidates)
            {
                if (!g.graph.ContainsKey(c))
                    throw new ArgumentException("Graph does not contain a strict superset of current candidates.");
                // Merge the graph nodes for this candidate
                graph[c].Add(g.graph[c]);
            }  
        }

        public virtual (int v1, int v2) GetVoteCount(Candidate c1, Candidate c2)
        {
            return graph[c1].GetVoteCount(c2);
        }

        protected virtual Dictionary<Candidate, (int v1, int v2)> VoteCounts(Candidate candidate)
        {
            Dictionary<Candidate, (int, int)> output = new Dictionary<Candidate, (int, int)>();
            foreach (Candidate c in graph.Keys)
            {
                if (c == candidate)
                    continue;
                output[c] = GetVoteCount(candidate, c);
            }
            return output;
        }

        public IEnumerable<Candidate> Wins(Candidate candidate)
        {
            List<Candidate> output = new List<Candidate>();
            Dictionary<Candidate, (int v1, int v2)> votes = VoteCounts(candidate);

            foreach (Candidate c in votes.Keys)
                if (votes[c].v1 > votes[c].v2)
                    output.Add(c);

            return output;
        }

        public IEnumerable<Candidate> Ties(Candidate candidate)
        {
            List<Candidate> output = new List<Candidate>();
            Dictionary<Candidate, (int v1, int v2)> votes = VoteCounts(candidate);

            foreach (Candidate c in votes.Keys)
                if (votes[c].v1 == votes[c].v2)
                    output.Add(c);

            return output;
        }

        public IEnumerable<Candidate> Losses(Candidate candidate)
        {
            List<Candidate> output = new List<Candidate>();
            Dictionary<Candidate, (int v1, int v2)> votes = VoteCounts(candidate);

            foreach (Candidate c in votes.Keys)
                if (votes[c].v1 < votes[c].v2)
                    output.Add(c);

            return output;
        }

        protected PairwiseGraph()
        {
            throw new InvalidOperationException();
        }
    }
    // TODO:  PairwiseGraph derivative class which divides the ballots into (n)
    // equal segments and parallel-executes (n) counts, then puts this all together.
    
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
}
