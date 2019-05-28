using MoonsetTechnologies.Voting.Ballots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoonsetTechnologies.Voting.Utility
{
    /// <summary>
    /// A deduplicating ballot factory.
    /// </summary>
    public class BallotFactory
    {
        private DeduplicatorHashSet<Vote> Votes { get; set; }
        private DeduplicatorHashSet<Ballot> Ballots { get; set; }
        private AbstractPeopleFactory peopleFactory = new ByNamePeopleFactory();

        /// <summary>
        /// Creates a new BallotFactory.
        /// </summary>
        public BallotFactory()
        {
            Ballots = new DeduplicatorHashSet<Ballot>();
            Votes = new DeduplicatorHashSet<Vote>();
        }

        /// <summary>
        /// Creates a Ballot containing the given votes.
        /// </summary>
        /// <param name="votes">The votes to record on the ballot.</param>
        /// <returns>A deduplicated Ballot with all Votes deduplicated.</returns>
        public Ballot CreateBallot(IEnumerable<Vote> votes)
        {
            HashSet<Vote> vout = new HashSet<Vote>();
            Ballot b, bin;

            // Deduplicate the votes
            foreach (Vote v in votes)
                vout.Add(CreateVote(v.Candidate, v.Value));
            bin = new Ballot(vout);
            Ballots.TryGetValue(bin, out b);
            b = Ballots[bin];
            return b;
        }
        /// <summary>
        /// Condenses an enumerable of Ballots into a BallotSet. Combines duplicate
        /// Ballots and CountedBallots into single CountedBallots.
        /// </summary>
        /// <param name="ballots">The Ballots to return as a set.</param>
        /// <returns>A BallotSet with any duplicate Ballots combined into CountedBallots.</returns>
        public BallotSet CreateBallotSet(IEnumerable<Ballot> ballots)
        {
            HashSet<Ballot> outBallots = new HashSet<Ballot>();
            Dictionary<Ballot, long> ballotCounts = new Dictionary<Ballot, long>();

            int threadCount = Environment.ProcessorCount;

            // We need to create and count each single, identical ballot,
            // and count the number of such ballots in any CountedBallot we
            // encounter.  To do this, we create uncounted, single ballots
            // and specifically avoid looking up CountedBallot.
            async Task CountBallots()
            {
                List<Ballot> bList = ballots.ToList();
                Dictionary<Ballot, long>[] tasks = new Dictionary<Ballot, long>[threadCount];

                async Task<Dictionary<Ballot, long>> CountSubsets(int start, int end)
                {
                    Dictionary<Ballot, long> bC = new Dictionary<Ballot, long>();
                   
                    for (int i = start; i <= end; i++)
                    {
                        Ballot oneBallot = CreateBallot(bList[i].Votes);
                        long count = (bList[i] is CountedBallot) ? (bList[i] as CountedBallot).Count : 1;

                        if (!bC.ContainsKey(oneBallot))
                            bC[oneBallot] = 0;
                        bC[oneBallot] += count;
                    }

                    return bC;
                }

                // First divide all the processes up for background run
                for (int i = 0; i < threadCount; i++)
                {
                    tasks[i] = await CountSubsets(bList.Count() * i / threadCount, (bList.Count() * (i + 1) / threadCount) - 1);
                }

                // Now merge them
                for (int i = 0; i < threadCount; i++)
                {
                    foreach (Ballot b in tasks[i].Keys)
                    {
                        // Count this in the full ballot counts
                        if (!ballotCounts.ContainsKey(b))
                            ballotCounts[b] = 0;
                        ballotCounts[b] += tasks[i][b];

                        // Check for identical ballots found in each further thread
                        for (int j = i+1; j < threadCount; j++)
                        {
                            if (tasks[j].ContainsKey(b))
                            {
                                ballotCounts[b] += tasks[j][b];
                                // It's been counted, so remove it
                                tasks[j].Remove(b);
                            }
                        }
                    }
                    // We've counted all these, so clear them.
                    tasks[i].Clear();
                }
            }

            // Count in a threaded model
            CountBallots().Wait();

            // Generate CountedBallots from the counts made
            foreach (Ballot b in ballotCounts.Keys)
            {
                Ballot newBallot;
                if (ballotCounts[b] == 1)
                    newBallot = b;
                else
                    newBallot = new CountedBallot(b, ballotCounts[b]);
                // Look itself up to store or deduplicate
                newBallot = Ballots[newBallot];
                outBallots.Add(newBallot);
            }

            return new BallotSet(outBallots);
        }
        /// <summary>
        /// Produces a single BallotSet from a set of BallotSet objects.
        /// </summary>
        /// <param name="sets">An enumerable set of BallotSet objects.</param>
        /// <returns>A BallotSet created from the total of all Ballots.</returns>
        public BallotSet MergeBallotSets(IEnumerable<BallotSet> sets)
        {
            List<CountedBallot> ballots = new List<CountedBallot>();
            foreach (BallotSet b in sets)
                ballots.AddRange(b.Ballots);
            return CreateBallotSet(ballots);
        }
        /// <summary>
        /// Creates a vote object.
        /// </summary>
        /// <param name="candidate">The candidate for whom the vote is cast.</param>
        /// <param name="value">The value of the vote.</param>
        /// <returns></returns>
        public Vote CreateVote(Candidate candidate, decimal value)
        {
            Person c;
            Vote v = null;

            c = peopleFactory.GetCandidate(candidate);
            v = new Vote(c as Candidate, value);
            return Votes[v];
        }
    }
}
