using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Storage;
using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Tiebreaking;
using MoonsetTechnologies.Voting.Utility;
using MoonsetTechnologies.Voting.Utility.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
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
            FileStream file;

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

            BallotSet bset;
            using (file = new FileStream(filename, FileMode.Open))
            {
                IEnumerable<CountedBallot> ballots = s.LoadBallots(file);
                Assert.NotNull(ballots);
                bset = new BallotSet(ballots);
            }

            // Use Last Difference
            tabulatorFactory.SetTiebreaker(new TiebreakerFactory<LastDifferenceTiebreaker>());

            t = tabulatorFactory.CreateTabulator();

            Assert.NotNull(t);

            t.Monitor.TabulationComplete += Monitor_TabulationComplete;
            t.Monitor.RoundComplete += Monitor_RoundComplete;

            t.Tabulate(bset, seats: seats);

            t.Monitor.TabulationComplete -= Monitor_TabulationComplete;
            t.Monitor.RoundComplete -= Monitor_RoundComplete;

            Assert.NotNull(w);
            HashSet<string> wdiff = w.ToHashSet();
            wdiff.ExceptWith(winners);
            Assert.Empty(wdiff);
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
            FileStream file;
            StreamReader sr;
            List<object[]> allData = new List<object[]>();
            string dname = Path.GetDirectoryName(Path.GetFullPath(filename));

            (string fn, string algo, int seats, List<string> winners) readTestLine()
            {
                string s;
                s = sr.ReadLine();
                List<string> w = new List<string>();

                List<string> parts = Regex.Matches(s, @"[\""].+?[\""]|[^ ]+")
                    .Cast<Match>()
                    .Select(m => m.Value.Replace("\"", string.Empty))
                    .ToList();
                if (parts.Count < 4)
                    throw new FormatException("Entry in .simpletabulatortest has fewer than 4 fields.");

                for (int i = 3; i < parts.Count; i++)
                {
                    w.Add(parts[i]);
                }
                return (parts[0], parts[1], Convert.ToInt32(parts[2]), w);
            }

            using (file = new FileStream(filename, FileMode.Open))
            {
                using (sr = new StreamReader(file))
                {
                    while (!sr.EndOfStream)
                    {
                        (string fn, string algo, int seats, List<string> winners)
                            = readTestLine();
                        // Get absolute path of file
                        fn = Path.GetFullPath(dname + Path.DirectorySeparatorChar + fn);
                        if (algo != algorithm)
                            continue;
                        allData.Add(new object[] { fn, seats, winners });
                    }
                }
            }
            return allData;
        }
    }
}
