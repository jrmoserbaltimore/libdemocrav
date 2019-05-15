using MoonsetTechnologies.Voting.Ballots;
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
using Xunit;
using Xunit.Abstractions;

namespace MoonsetTechnologies.Voting.Development.Tests.theories
{

    public class TidemansAlternativeTabulatorTester
        : TabulatorTester<TidemansAlternativeTabulatorFactory>
    {
        public TidemansAlternativeTabulatorTester(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory]
        [MemberData(nameof(TestFile),
            new object[] { "BurlingtonTest.json", typeof(TidemansAlternativeTabulator), 1 })]
        public override void TabulationTest(IEnumerable<Ballot> ballots, string[] winners)
        {
            base.TabulationTest(ballots, winners);
        }
    }

    public abstract class TabulatorTester<T>
        where T : AbstractTabulatorFactory, new()
    {
        // indexed by candidate number in the input
        public Dictionary<int, Candidate> Candidates { get; private set; }
        public List<Ballot> Ballots { get; private set; }
        public AbstractTiebreaker tiebreaker;
        public ITestOutputHelper output;

        public TabulatorTester(ITestOutputHelper testOutputHelper)
        {
            output = testOutputHelper;
        }

        public virtual void TabulationTest(IEnumerable<Ballot> ballots, string[] winners)
        {
            List<string> elected = null;
            AbstractTabulatorFactory f;
            AbstractTabulator t;

            void Monitor_TabulationComplete(object sender, TabulationStateEventArgs e)
            {
                elected = e.CandidateStates
                .Where(x => x.Value.State == CandidateState.States.elected)
                .Select(x => x.Key.Name).ToList();

                output.WriteLine("Tabulation completion data:");
                PrintTabulationState(e);
            }

            void Monitor_RoundComplete(object sender, TabulationStateEventArgs e)
            {
                output.WriteLine("Round completion data:");
                PrintTabulationState(e);
            }

            f = new T();

            // Use Last Difference
            f.SetTiebreaker(new TiebreakerFactory<LastDifferenceTiebreaker>());

            t = f.CreateTabulator();

            Assert.NotNull(t);
            Assert.IsType<TidemansAlternativeTabulator>(t);

            t.Monitor.TabulationComplete += Monitor_TabulationComplete;
            t.Monitor.RoundComplete += Monitor_RoundComplete;

            t.Tabulate(ballots);

            t.Monitor.TabulationComplete -= Monitor_TabulationComplete;
            t.Monitor.RoundComplete -= Monitor_RoundComplete;

            Assert.NotNull(elected);
            Assert.Single(elected);
            Assert.Equal(winners.Single(), elected.Single());
        }

        // Constraints:
        //
        //  [SmithEfficient]
        //    Always elects from the Smith Set
        //  [SchwartzEfficient]
        //    Always elects from the Schwartz Set
        //  Condorcet
        //    Either [SmithEfficient] or [SchwartzEfficient] must always elect from the
        //    respective set if that set is one.  Schwartz can be a subset of Smith, so
        //    they're equivalent with one candidate.
        //  Elimination Order
        //    For multi-round, follow the same elimination order, unless eliminations are
        //    batched.
        //  Vote Counts
        //    Match vote counts at each round or tabulation completion.
        //
        // When a tabulator declares [TabulationAlgorithm()], it receives all matching result
        // sets for testing.
        //
        // We pass the Smith and Schwartz sets to [SmithEfficient] and [SchwartzEfficient]
        // tabulators, even if they don't implement an algorithm for which we have results.
        // Notably, when these sets are one candidate, the algorithms should select that
        // candidate as winner; for multiple candidates, a single winner may vary, and is
        // questionable.
        public static IEnumerable<object[]> TestFile(string path, Type tabulator, int seats)
        {
            var allData = new List<object[]>();

            bool condorcet;
            condorcet = tabulator.GetCustomAttributes(typeof(SmithEfficient)).Count() > 0;
            List<JsonElement> testCases = new List<JsonElement>();
            FileStream file = new FileStream(path, FileMode.Open);

            JsonDocument dom = JsonDocument.Parse(file);

            if (condorcet)
                testCases.AddRange(dom.RootElement.EnumerateArray()
                .Where(x => x.GetProperty("attributes")
                     .GetProperty("smith set").GetArrayLength() == 1));
            
            return allData;

        }
        protected void PrintTabulationState(TabulationStateEventArgs e)
        {
            foreach (Candidate c in e.CandidateStates.Keys)
            {
                output.WriteLine("  {0}\t{1}\t{2}", e.CandidateStates[c].VoteCount,
                    c.Name,
                    e.CandidateStates[c].State.ToString());
            }

            RankedTabulationStateEventArgs re = e as RankedTabulationStateEventArgs;
            if (!(re is null))
            {
                output.WriteLine("  Smith Set:");
                foreach (Candidate c in re.SmithSet)
                    output.WriteLine("    {0}", c.Name);
                output.WriteLine("  Schwartz Set:");
                foreach (Candidate c in re.SchwartzSet)
                    output.WriteLine("    {0}", c.Name);
                if (!(re.PairwiseGraph is null))
                {
                    output.WriteLine("  Pairwise Contests:");
                    List<Candidate> c = re.PairwiseGraph.Candidates.ToList();
                    for (int i = 0; i < c.Count; i++)
                    {
                        for (int j = i + 1; j < c.Count; j++)
                        {
                            (decimal vi, decimal vj) = re.PairwiseGraph.GetVoteCount(c[i], c[j]);
                            if (vi >= vj)
                                output.WriteLine("    {0}, {1}\t{2}\tvs\t{3}", vi, vj, c[i].Name, c[j].Name);
                            else
                                output.WriteLine("    {0}, {1}\t{2}\tvs\t{3}", vj, vi, c[j].Name, c[i].Name);
                        }
                    }
                }
            }
            output.WriteLine("  Notes:\t{0}", e.Note);
            output.WriteLine("\n");
        }
    }
}
