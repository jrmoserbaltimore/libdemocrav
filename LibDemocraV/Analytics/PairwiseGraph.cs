// This class computes pairwise victories from complete ranked ballot sets.
// These pairwise victories allow us to compute the Smith and Schwartz sets,
// and thus support Condorcet systems like Smith-constrained IRV and Tideman's
// Alternative.
using MoonsetTechnologies.Voting.Ballots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoonsetTechnologies.Voting.Analytics
{
    /// <summary>
    /// Converts a set of candidates and a set of ballots into a graph of wins and ties.
    /// </summary>
    public class PairwiseGraph
    {
        protected Dictionary<Candidate, Dictionary<Candidate, decimal>> nodes
            = new Dictionary<Candidate, Dictionary<Candidate, decimal>>();

        /// <summary>
        /// All candidates in this graph.
        /// </summary>
        public virtual IEnumerable<Candidate> Candidates => nodes.Keys;

        /// <summary>
        /// Converts a set of candidates and ballots to a graph of wins and ties.
        /// </summary>
        /// <param name="ballots">Ranked ballots in the election.</param>
        public PairwiseGraph(BallotSet ballots)
        {
            // Initialize candidate graph
            HashSet<Candidate> candidates = (ballots as IEnumerable<CountedBallot>).SelectMany(x => x.Votes.Select(y => y.Candidate)).ToHashSet();
            foreach (Candidate c in candidates)
            {
                nodes[c] = new Dictionary<Candidate, decimal>();
                foreach (Candidate d in candidates.Except(new[] { c }))
                    nodes[c][d] = 0.0m;
            }

            void BuildGraph()
            {
                List<CountedBallot> bList = ballots.ToList();

                int threadCount = Environment.ProcessorCount;
                PairwiseGraph[] subsets = new PairwiseGraph[threadCount];

                PairwiseGraph CountSubsets(int start, int end)
                {
                    PairwiseGraph g = new PairwiseGraph();

                    foreach (Candidate c in candidates)
                    {
                        g.nodes[c] = new Dictionary<Candidate, decimal>();
                        foreach (Candidate d in candidates.Except(new[] { c }))
                            g.nodes[c][d] = 0.0m;
                    }

                    // Iterate each ballot and count who wins and who ties.
                    // This can support tied ranks and each ballot is O(SUM(1..n)) and o(n).
                    //foreach (CountedBallot b in ballots)
                    for (int i = start; i <= end; i++)
                    {
                        CountedBallot b = bList[i];
                        HashSet<Candidate> ranked = b.Votes.Select(x => x.Candidate).ToHashSet();
                        HashSet<Candidate> unranked = candidates.Except(ranked).ToHashSet();

                        // Iterate to compare each pair.
                        Stack<Vote> votes = new Stack<Vote>(b.Votes);
                        while (votes.Count > 0)
                        {
                            Vote v = votes.Pop();
                            foreach (Vote u in votes)
                            {
                                // Who is ranked first?  No action if a tie.
                                if (v.Beats(u))
                                    g.nodes[v.Candidate][u.Candidate] += b.Count;
                                else if (u.Beats(v))
                                    g.nodes[u.Candidate][v.Candidate] += b.Count;
                            }
                            // Defeat all unranked candidates
                            foreach (Candidate c in unranked)
                                g.nodes[v.Candidate][c] += b.Count;
                        }
                    }
                    return g;
                }

                // First divide all the processes up for background run
                Parallel.For (0, threadCount, (i, state) =>
                    subsets[i] = CountSubsets(bList.Count() * i / threadCount, (bList.Count() * (i + 1) / threadCount) - 1));
                // Add them all together
                foreach (PairwiseGraph g in subsets)
                    AddGraph(g);
            }
            BuildGraph();
        }

        /// <summary>
        /// Merge two PairwiseGraphs.
        /// </summary>
        /// <param name="g1">Graph 1</param>
        /// <param name="g2">Graph 2</param>
        public PairwiseGraph(PairwiseGraph g1, PairwiseGraph g2)
        {
            AddGraph(g1);
            AddGraph(g2);
        }

        public PairwiseGraph(PairwiseGraph graph)
        {
            AddGraph(graph);
        }

        private void AddGraph(PairwiseGraph graph)
        {
            foreach (Candidate c in graph.Candidates)
            {
                // Merge the graph nodes for this candidate
                foreach (Candidate d in graph.Candidates)
                {
                    if (!nodes.ContainsKey(c))
                    {
                        nodes[c] = new Dictionary<Candidate, decimal>
                        {
                            [d] = 0.0m
                        };
                    }
                    if (graph.nodes[c].ContainsKey(d))
                        nodes[c][d] += graph.nodes[c][d];
                }
            }

            // This merger may disturb the graph, so fill in any gaps
            foreach (Candidate c in Candidates)
            {
                if (!nodes.ContainsKey(c))
                    nodes[c] = new Dictionary<Candidate, decimal>();
                foreach (Candidate d in Candidates)
                {
                    if (!nodes[c].Keys.Contains(d))
                        nodes[c][d] = 0.0m;
                }
            }
        }

        public virtual (decimal v1, decimal v2) GetVoteCount(Candidate c1, Candidate c2)
        {
            if (!(Candidates.Contains(c1) && Candidates.Contains(c2)))
                throw new ArgumentException("Candidates requested not Candidates in graph");
            return (nodes[c1][c2], nodes[c2][c1]);
        }

        protected virtual Dictionary<Candidate, (decimal v1, decimal v2)> VoteCounts(Candidate candidate)
          => Candidates.Except(new[] { candidate }).ToDictionary(x => x, x => GetVoteCount(candidate, x));

        public HashSet<Candidate> Wins(Candidate candidate)
          => VoteCounts(candidate).Where(x => x.Value.v1 > x.Value.v2).Select(x => x.Key).ToHashSet();

        public HashSet<Candidate> Ties(Candidate candidate)
          => VoteCounts(candidate).Where(x => x.Value.v1 == x.Value.v2).Select(x => x.Key).ToHashSet();

        public HashSet<Candidate> Losses(Candidate candidate)
          => VoteCounts(candidate).Where(x => x.Value.v1 < x.Value.v2).Select(x => x.Key).ToHashSet();

        protected PairwiseGraph()
        {
            
        }
    }
    // TODO:  PairwiseGraph derivative class which divides the ballots into (n)
    // equal segments and parallel-executes (n) counts, then puts this all together.
    //
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