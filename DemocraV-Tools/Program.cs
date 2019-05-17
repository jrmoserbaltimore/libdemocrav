using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Storage;
using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MoonsetTechnologies.Voting.Utility
{
    class Program
    {
        static void Main(string[] args)
        {
            AbstractBallotStorage s = new DavidHillFormat();
            FileStream file = new FileStream(args[0], FileMode.Open);
            IEnumerable<CountedBallot> ballots = s.LoadBallots(file);

            BallotSet bset = new BallotSet(ballots);

            List<string> winners = null;
            int smithSetCount = 0;

            TopCycle t = new TopCycle(bset);

            HashSet<Candidate> candidates = new HashSet<Candidate>();

            foreach (Ballot b in bset)
            {
                foreach (Vote v in b.Votes)
                    candidates.Add(v.Candidate);
            }

            winners = new List<string>();
            
            foreach (Candidate c in t.GetTopCycle(candidates,TopCycle.TopCycleSets.smith))
            {
                winners.Add(c.Name);
            }

            Console.Write(@"""{0}"" ""smith set"" {1}", args[0], winners.Count());

            foreach (string w in winners)
                Console.Write(@" ""{0}""", w);
            Console.Write("\n");

            smithSetCount = winners.Count();

            winners.Clear();

            foreach (Candidate c in t.GetTopCycle(candidates, TopCycle.TopCycleSets.schwartz))
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

            PairwiseGraph g = new PairwiseGraph(candidates, bset);

            foreach (Candidate c in candidates)
            {
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
