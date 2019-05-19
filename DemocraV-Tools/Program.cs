using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Storage;
using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;

namespace MoonsetTechnologies.Voting.Utility
{
    class Program
    {
        static void Main(string[] args)
        {
            AbstractBallotStorage s = new DavidHillFormat();
            FileStream file;

            List<string> winners = new List<string>();
            HashSet<Candidate> candidates = new HashSet<Candidate>();
            TopCycle t;
            BallotSet bset;
            int smithSetCount = 0;

            for (int j = 0; j < 1000; j++)
            {
                winners.Clear();
                candidates.Clear();
                bset = null;

                for (int i = 0; i < 1000; i++)
                {
                    using (file = new FileStream(args[0], FileMode.Open))
                    {
                        List<BallotSet> bsets = new List<BallotSet>();
                        bsets.Clear();
                        bsets.Add(s.LoadBallots(file));
                        if (!(bset is null))
                            bsets.Add(bset);
                        bset = s.ballotFactory.MergeBallotSets(bsets);
                    }
                }

                PairwiseGraph g = new PairwiseGraph(bset);

                t = new TopCycle(g);

                foreach (Ballot b in bset)
                {
                    foreach (Vote v in b.Votes)
                        candidates.Add(v.Candidate);
                }

                foreach (Candidate c in t.GetTopCycle(new List<Candidate>(), TopCycle.TopCycleSets.smith))
                {
                    winners.Add(c.Name);
                }

                Console.Write(@"""{0}"" ""smith set"" {1}", args[0], winners.Count());

                foreach (string w in winners)
                    Console.Write(@" ""{0}""", w);
                Console.Write("\n");

                smithSetCount = winners.Count();

                winners.Clear();

                foreach (Candidate c in t.GetTopCycle(new List<Candidate>(), TopCycle.TopCycleSets.schwartz))
                {
                    winners.Add(c.Name);
                }

                if (winners.Count() != smithSetCount)
                {

                    Console.Write(@"""{0}"" ""schwartz set"" {1}", args[0], winners.Count());

                    foreach (string w in winners)
                        Console.Write(@" ""{0}""", w);
                    Console.Write("\n");
                }

                foreach (Candidate c in candidates)
                {
                    break;
                    foreach (var v in g.Losses(c))
                    {
                        Console.WriteLine("{0} defeated by {1}\t{2}:{3}", c.Name, v.Name, g.GetVoteCount(c, v).v1, g.GetVoteCount(c, v).v2);
                    }
                    foreach (var v in g.Ties(c))
                    {
                        Console.WriteLine("{0} ties with {1}\t{2}:{3}", c.Name, v.Name, g.GetVoteCount(c, v).v1, g.GetVoteCount(c, v).v2);
                    }
                }
            }
        }
    }
}
