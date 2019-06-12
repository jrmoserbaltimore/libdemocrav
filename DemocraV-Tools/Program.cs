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
using System.Threading.Tasks;

namespace MoonsetTechnologies.Voting.Utility
{
    class Program
    {
        static void Main(string[] args)
        {
            AbstractBallotStorage s = new DavidHillFormat();
            object displayLock = new object();
            for (int j = 0; j < 100; j++)
            {
                Console.Write("Entering thread {0}\n", j);
                FileStream file;

                List<string> winners = new List<string>();
                HashSet<Candidate> candidates = new HashSet<Candidate>();
                TopCycle t;
                BallotSet bset = null;
                int smithSetCount = 0;

                winners.Clear();
                candidates.Clear();
                bset = null;
                List<BallotSet> bsets = new List<BallotSet>();
                bsets.Clear();

                if (bset is null)
                {

                    using (file = new FileStream(args[0], FileMode.Open, FileAccess.Read))
                    {
                        bset = s.LoadBallots(file);
                    }
                }
                Console.Write("{0}\tLoaded ballots.\n", j);
                for (int i = 0; i < 1000; i++)
                {
                    bsets.Add(bset);
                    //GC.Collect(2, GCCollectionMode.Forced, true, true);
                }

                Console.Write("{0}\tBuilt sets.\n", j);
                bset = s.ballotFactory.MergeBallotSets(bsets);
                Console.Write("{0}\tMerged sets.\n", j);
                PairwiseGraph g = new PairwiseGraph(bset);
                Console.Write("{0}\tBuilt pairwise graph\n", j);

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

                lock (displayLock)
                {
                    Console.Write(@"""{0}"" ""smith set"" {1}", args[0], winners.Count());

                    foreach (string w in winners)
                        Console.Write(@" ""{0}""", w);
                    Console.Write("\n");
                }

                smithSetCount = winners.Count();

                winners.Clear();

                foreach (Candidate c in t.GetTopCycle(new List<Candidate>(), TopCycle.TopCycleSets.schwartz))
                {
                    winners.Add(c.Name);
                }

                if (winners.Count() != smithSetCount)
                {
                    lock (displayLock)
                    {
                        Console.Write(@"""{0}"" ""schwartz set"" {1}", args[0], winners.Count());

                        foreach (string w in winners)
                            Console.Write(@" ""{0}""", w);
                        Console.Write("\n");
                    }
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
