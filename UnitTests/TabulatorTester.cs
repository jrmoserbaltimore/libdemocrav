using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Storage;
using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Tiebreaking;
using MoonsetTechnologies.Voting.Utility;
using MoonsetTechnologies.Voting.Utility.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace MoonsetTechnologies.Voting.Development.Tests
{
    public abstract class AbstractTabulatorTest : IClassFixture<BallotSetFixture>
    {
        protected readonly ITestOutputHelper output;

        protected readonly BallotSetFixture fixture;

        protected AbstractTabulatorFactory tabulatorFactory;

        public AbstractTabulatorTest(ITestOutputHelper testOutputHelper, BallotSetFixture fixture)
        {
            this.fixture = fixture;
            fixture.output = output = testOutputHelper;
        }

        protected void TabulatorTest(string filename, int seats, IEnumerable<string> winners)
        {
            List<string> w = null;
            AbstractTabulator t;

            AbstractBallotStorage s = new DavidHillFormat();

            void Monitor_TabulationBegin(object sender, TabulationDetailsEventArgs e)
            {
                output.WriteLine("Tabulation initial state data:");

                output.WriteLine("  Approval Rankings:");

                Dictionary<Candidate, CandidateState>[] cS = new Dictionary<Candidate, CandidateState>[4];
                ApprovalAlternateOutcomeAnalysis analysis = new ApprovalAlternateOutcomeAnalysis(e.Ballots,
                    from x in e.CandidateStates
                    where x.Value.State == CandidateState.States.withdrawn
                    select x.Key,
                    e.Seats);
                for (int i = 0; i <= 3; i++)
                {
                    cS[i] = analysis.BaseOutcome(i);
                }

                foreach (Candidate c in e.CandidateStates.Keys)
                {
                    long[] approvals = new long[4];
                    
                    // Count totals for each assumption where a candidate ranked at (i) or better
                    // would be voted for on an approval ballot
                    for (int i = 1; i <= 3; i++)
                    {
                        approvals[i - 1] = e.Ballots.Aggregate(0L, (x, y) => y.Votes.Where(v => v.Value <= i && v.Candidate == c).Count() * y.Count + x);
                    }

                    // Any ranking is an approval
                    approvals[3] = e.Ballots.Aggregate(0L, (x, y) => y.Votes.Where(v => v.Candidate == c).Count() * y.Count + x);

                    output.WriteLine("  {0}:\t{1}\t{2}\t{3}\t{4}", c.Name, cS[1][c].VoteCount, cS[2][c].VoteCount, cS[3][c].VoteCount, cS[0][c].VoteCount);
                }

                // Analysis against Approval Voting and how variable the outcome is
                //output.WriteLine("  Ballot changes to elect candidates:");
                //foreach (Candidate c in e.CandidateStates.Keys)
                //{
                //    (BallotSet originalBallots, BallotSet newBallots) = analysis.FindOutcome(new[] { c });
                //    output.WriteLine("    {0}:\t{1}", c.Name, (from x in newBallots
                //                                               where (from y in originalBallots
                //                                                      where y.Votes.ToHashSet().SetEquals(x.Votes)
                //                                                      select true).Contains(true)
                //                                               select x.Count).Sum());
                //}

                PairwiseGraph g = new PairwiseGraph(e.Ballots);
                TopCycle tC = new TopCycle(g);
                HashSet<Candidate> withdrawn =
                    (from x in e.CandidateStates
                     where new[] {
                         CandidateState.States.withdrawn,
                         CandidateState.States.defeated }.Contains(x.Value.State)
                     select x.Key).ToHashSet();
                TabulationStateEventArgs e1 = new RankedTabulationStateEventArgs
                {
                    CandidateStates = e.CandidateStates,
                    PairwiseGraph = g,
                    SmithSet = tC.GetTopCycle(withdrawn, TopCycle.TopCycleSets.smith),
                    SchwartzSet = tC.GetTopCycle(withdrawn, TopCycle.TopCycleSets.schwartz)
                };
                fixture.PrintTabulationState(e1);
            }

            void Monitor_TabulationComplete(object sender, TabulationStateEventArgs e)
            {
                w = e.CandidateStates
                .Where(x => x.Value.State == CandidateState.States.elected)
                .Select(x => x.Key.Name).ToList();

                output.WriteLine("Tabulation completion data:");
                fixture.PrintTabulationState(e);
            }

            void Monitor_RoundComplete(object sender, TabulationStateEventArgs e)
            {
                output.WriteLine("Round completion data:");
                fixture.PrintTabulationState(e);
            }

            BallotSet ballots;
            using (MemoryMappedFile file = MemoryMappedFile.CreateFromFile(
                new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read),
                null, 0, MemoryMappedFileAccess.CopyOnWrite, HandleInheritability.None, false))
            {
                using (MemoryMappedViewStream vs = file.CreateViewStream(0, 0, MemoryMappedFileAccess.Read))
                    ballots = s.LoadBallots(vs);
            }
            Assert.NotNull(ballots);

            // Use Last Difference
            tabulatorFactory.SetTiebreaker(new LastDifferenceTiebreakerFactory());

            t = tabulatorFactory.CreateTabulator();

            Assert.NotNull(t);

            output.WriteLine("Testing file {0}", filename);
            t.Monitor.TabulationComplete += Monitor_TabulationComplete;
            t.Monitor.RoundComplete += Monitor_RoundComplete;
            t.Monitor.TabulationBegin += Monitor_TabulationBegin;

            t.Tabulate(ballots, seats: seats);

            t.Monitor.TabulationComplete -= Monitor_TabulationComplete;
            t.Monitor.RoundComplete -= Monitor_RoundComplete;
            t.Monitor.TabulationBegin -= Monitor_TabulationBegin;

            Assert.NotNull(w);
            HashSet<string> unexpectedWinners = w.Except(winners).ToHashSet();
            HashSet<string> unexpectedLosers = winners.Except(w).ToHashSet();

            Assert.Empty(unexpectedWinners);
            Assert.Empty(unexpectedLosers);
        }

        class BasicTabulatorTestData : IXunitSerializable
        {
            public string filename { get; set; }
            public int seats { get; set; }
            public List<string> winners { get; set; }

            public void Deserialize(IXunitSerializationInfo info)
            {
                filename = info.GetValue<string>("filename");
                seats = info.GetValue<int>("seats");
                winners = info.GetValue<string[]>("winners").ToList();
            }

            public void Serialize(IXunitSerializationInfo info)
            {
                info.AddValue("filename", filename);
                info.AddValue("seats", seats);
                info.AddValue("winners", winners.ToArray());
            }
        }

        public static IEnumerable<object[]> GetFlatTests(string filename, string algorithm)
        {
            StreamReader sr;
            List<object[]> allData = new List<object[]>();
            string dname = Path.GetDirectoryName(Path.GetFullPath(filename));

            (string fn, string algo, int seats, List<string> winners) readTestLine()
            {
                string s;
                s = sr.ReadLine();
                List<string> w = new List<string>();

                // FIXME:  Without the \0 it adds a 3,000-byte \0\0\0... element to the
                // last entry in the file.  I have no idea why.
                List<string> parts = Regex.Matches(s, @"[\""].+?[\""]|[^ \0]+")
                    .Cast<Match>()
                    .Select(m => m.Value.Replace("\"", string.Empty))
                    .ToList();
                if (parts.Count < 4)
                    throw new FormatException("Entry in .simpletabulatortest has fewer than 4 fields.");

                for (int i = 3; i < parts.Count; i++)
                    w.Add(parts[i]);
                
                return (parts[0], parts[1], Convert.ToInt32(parts[2]), w);
            }

            using (MemoryMappedFile file = MemoryMappedFile.CreateFromFile(
                new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read),
                null, 0, MemoryMappedFileAccess.CopyOnWrite, HandleInheritability.None, false))
            {
                using (MemoryMappedViewStream vs = file.CreateViewStream(0, 0, MemoryMappedFileAccess.Read))
                {
                    using (sr = new StreamReader(vs))
                    {
                        while (!sr.EndOfStream)
                        {
                            (string fn, string algo, int seats, List<string> winners)
                                = readTestLine();
                            // Get absolute path of file
                            fn = Path.GetFullPath(dname + Path.DirectorySeparatorChar + fn);

                            // When a Condorcet method encounters a one-seat
                            if (((algorithm == "condorcet-smith" && algo == "smith set")
                                || (algorithm == "condorcet-schwartz"
                                    && new[] { "smith set", "schwartz set" }.Contains(algo)))
                                && seats == 1)
                            {
                                // just don't hit the continue block
                            }
                            else if (algo != algorithm)
                            {
                                continue;
                            }
                            allData.Add(new object[] { fn, seats, winners });
                        }
                    }
                }
            }
            return allData;
        }
    }
}
