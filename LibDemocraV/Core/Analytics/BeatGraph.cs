using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Analytics
{
    class GraphNode
    {
        private Dictionary<GraphNode, int> beats = new Dictionary<GraphNode, int>();
        public IEnumerator<KeyValuePair<GraphNode, int>> Beats => beats.GetEnumerator();

        public Candidate candidate { get; private set; }

        public GraphNode(Candidate candidate)
        {
            this.candidate = candidate;
        }

        public void Increment(GraphNode opponent)
        {
            // If we're the loser, reduce the winning strength of the winner.
            // Also appears in a tie.
            if (opponent.beats.ContainsKey(this))
            {
                opponent.beats[this]--;
                if (opponent.beats[this] == 0)
                    this.beats[opponent] = 0;
                // Remove the tie pointer from our opponent if we become the winner
                else if (opponent.beats[this] < 0)
                    opponent.beats.Remove(this);
                else if (beats.Remove(opponent))
                    throw new InvalidOperationException("Both neighbors in a pair have a beat value and they aren't both zero.");
            }
            else
            {
                // When neither has a vote yet, set our vote to zero
                if (!beats.ContainsKey(opponent))
                    beats[opponent] = 0;
                beats[opponent]++;
            }
        }

        public void CheckNeighbor(GraphNode opponent)
        {
            // Checks for a tie
            if (!opponent.beats.ContainsKey(this) && !beats.ContainsKey(opponent))
            {
                beats[opponent] = 0;
                opponent.beats[this] = 0;
            }

        }
    }

    class PairwiseGraph
    {
        private Dictionary<Candidate, GraphNode> graph = new Dictionary<Candidate, GraphNode>();

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
                    g.CheckNeighbor(j);
            }

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
                    for (int j=i+1; j < votes.Count; j++)
                    {
                        Candidate w = null, l = null;
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
                            graph[w].Increment(graph[l]);
                    }
                    // Defeat all unranked candidates
                    foreach (Candidate c in unranked)
                        graph[votes[i].Candidate].Increment(graph[c]);
                }
            }
        }

    }
}
