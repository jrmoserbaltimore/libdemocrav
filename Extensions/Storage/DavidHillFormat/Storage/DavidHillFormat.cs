using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MoonsetTechnologies.Voting.Ballots;

namespace MoonsetTechnologies.Voting.Storage
{
    public class DavidHillFormat : AbstractBallotStorage
    {
        public override IEnumerable<CountedBallot> LoadBallots(Stream stream)
        {
            StreamReader sr = new StreamReader(stream);
            // Raw list of ballots, plus a count of how many seen
            Dictionary<List<int>, int> rawBallots = new Dictionary<List<int>, int>();

            int candidateCount, seats;

            List<Candidate> Candidates = new List<Candidate>();
            List<CountedBallot> ballots = new List<CountedBallot>();

            (int candidateCount, int seats) getFirstLine()
            {
                string s;
                int cCount, sCount;
                s = sr.ReadLine();
                string[] entries = s.Split(" ");
                if (entries.Length != 2)
                    throw new FormatException("Invalid first line in David Hill format.  Must be two integers.");
                cCount = Convert.ToInt32(entries[0]);
                sCount = Convert.ToInt32(entries[1]);

                // XXX:  <2 candidates, <1 seats, exception?

                return (cCount, sCount);
            }

            void readBallots()
            {
                string s;
                int bCount;
                string[] entries;

                do
                {
                    List<int> c;
                    s = sr.ReadLine();
                    entries = s.Split(" ");

                    bCount = Convert.ToInt32(entries[0]);
                    // a 0 indicates we're done with ballots
                    if (bCount == 0)
                        break;
                    if (entries.Length < 2)
                        throw new FormatException("Invalid ballot line in David Hill format.  Ballot with no votes.");

                    c = new List<int>();
                    for (int i = 1; i < entries.Length; i++)
                    {
                        int cid = Convert.ToInt32(entries[i]);
                        if (c.Contains(cid))
                            throw new FormatException("Invalid ballot line in David Hill format.  Same candidate in two ranks. {0} {1} | {2}");
                        // XXX:  Should be the last on each line
                        if (cid == 0)
                            break;
                        c.Add(cid);
                    }

                    // Check for an identical ballot
                    foreach (List<int> l in rawBallots.Keys)
                    {
                        if (l.SequenceEqual(c))
                        {
                            rawBallots[l] += bCount;
                            bCount = 0;
                            break;
                        }
                    }

                    // Didn't find a match, so add this entry
                    if (bCount > 0)
                        rawBallots[c] = bCount;
                } while (true);
            }

            void readCandidates()
            {
                string s;
                s = sr.ReadLine();
                List<string> parts = Regex.Matches(s, @"[\""].+?[\""]|[^ ]+")
                    .Cast<Match>()
                    .Select(m => m.Value.Replace("\"", string.Empty))
                    .ToList();
                if (parts.Count != candidateCount + 1)
                    throw new FormatException("Invalid candidate line in David Hill format.");
                for (int i = 0; i < candidateCount; i++)
                {
                    Candidates.Add(new Candidate(new Person(parts[i])));
                }
            }

            void createBallots()
            {
                HashSet<Vote> dedupVotes = new HashSet<Vote>();
                HashSet<CountedBallot> dedupBallots = new HashSet<CountedBallot>();
                foreach (List<int> l in rawBallots.Keys)
                {
                    HashSet<Vote> v = new HashSet<Vote>();
                    foreach (int i in l)
                    {
                        Vote u = new Vote(Candidates[i - 1], l.IndexOf(i) + 1);
                        List<Vote> dedups = dedupVotes.Where(x => x.Candidate == u.Candidate && x.Value == u.Value).ToList();
                        if (dedups.Count == 0)
                            dedupVotes.Add(u);
                        else if (dedups.Count > 1)
                            throw new InvalidOperationException("Duplicate votes in deduplication array");
                        else
                            u = dedups.Single();
                        v.Add(u);
                    }
                    CountedBallot b = new CountedBallot(new Ballot(v), rawBallots[l]);
                    List<CountedBallot> db = dedupBallots.Where(x => x.Votes.ToHashSet().SetEquals(b.Votes)).ToList();
                    if (db.Count == 0)
                        dedupBallots.Add(b);
                    else if (db.Count > 1)
                        throw new InvalidOperationException("Duplicate ballots in deduplication array");
                    else
                    {
                        CountedBallot ob = db.Single();
                        b = new CountedBallot(ob, ob.Count + b.Count);
                        dedupBallots.Remove(ob);
                        dedupBallots.Add(b);
                    }
                    ballots.Add(b);
                }
            }
            (candidateCount, seats) = getFirstLine();
            readBallots();
            readCandidates();

            createBallots();

            return ballots;
        }

        public override IEnumerable<Ballot> StoreBallots()
        {
            throw new NotImplementedException();
        }
    }
}
