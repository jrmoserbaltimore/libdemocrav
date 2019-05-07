using System;
using System.Collections.Generic;
using Xunit;
using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting;
using MoonsetTechnologies.Voting.Tiebreaking;
using System.Linq;
using MoonsetTechnologies.Voting.Tabulation;

namespace MoonsetTechnologies.Voting.Development.Tests
{
    public class BallotSetFixture
    {
        // indexed by candidate number in the input
        public Dictionary<int, Candidate> Candidates { get;  private set; }
        public List<IRankedBallot> Ballots { get;  private set; }

        public ITiebreaker tiebreaker;
        public IBatchEliminator batchEliminator;

        public BallotSetFixture()
        {
            string ballotSet =
                "c|Alex|0\n" +
                "c|Chris|1\n" +
                "c|Sam|2\n" +
                "b|20|0 1 2\n" +
                "b|15|2 1\n" +
                "b|8|1 2 0\n"+
                "b|5|1 0";
            // Decode the above into a thing.
            (Candidates, Ballots) = DecodeBallots(ballotSet);

            ITiebreaker firstDifference = new FirstDifferenceTiebreaker();
            tiebreaker = new SeriesTiebreaker(
                new ITiebreaker[] {
                    new SequentialTiebreaker(
                        new ITiebreaker[] {
                          new LastDifferenceTiebreaker(),
                          firstDifference,
                        }.ToList()
                    ),
                    new LastDifferenceTiebreaker(),
                    firstDifference,
                }.ToList()
            );
            batchEliminator = new RunoffBatchEliminator(tiebreaker);
        }

        // XXX:  Horrendous parser.  Just enough to limp along.
        // FIXME:  use the real data format filter when implemented.
        private (Dictionary<int, Candidate> candidates, List<IRankedBallot>) DecodeBallots(string input)
        {
            Dictionary<int, Candidate> cout = new Dictionary<int, Candidate>();
            List<IRankedBallot> bout = new List<IRankedBallot>();

            List<string> lines = new List<string>(input.Split("\n"));

            foreach (string s in lines)
            {
                List<string> chunks = new List<string>(s.Split("|"));
                if (chunks.Count != 3)
                    throw new ArgumentException();
                //Add a candidate or add a ballot.  Candidates must come first.
                if (chunks[0] == "c")
                {
                    cout[Convert.ToInt32(chunks[2])] = new Candidate(new Person(chunks[1]));
                }
                else if (chunks[0] == "b")
                {
                    List<RankedVote> v = new List<RankedVote>();
                    List<string> votes = new List<string>(chunks[2].Split(" "));

                    // Add a ranked vote at each sequential rank for each sequential candidate.
                    for (int i = 0; i < votes.Count; i++)
                    {
                        int cnum = Convert.ToInt32(votes[i]);
                        v.Add(new RankedVote(cout[cnum], i + 1));
                    }

                    // Create as many ballots as there are noted in field 2
                    for (int i = 0; i < Convert.ToInt32(chunks[1]); i++)
                        bout.Add(new RankedBallot(v));

                }
            }
            return (cout, bout);
        }

    }

    public class AnalyticsTests : IClassFixture<BallotSetFixture>
    {

        readonly BallotSetFixture fixture;

        public AnalyticsTests(BallotSetFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void PairwiseGraphBuildTest()
        {
            PairwiseGraph graph = new PairwiseGraph(fixture.Candidates.Values, fixture.Ballots);
            Assert.Equal((20, 28), graph.GetVoteCount(fixture.Candidates[0], fixture.Candidates[1]));
            Assert.NotNull(graph);
        }

        [Fact]
        public void TopCycleTest()
        {
            TopCycle smithSet = new TopCycle(fixture.Ballots);
            TopCycle schwartzSet = new TopCycle(fixture.Ballots);
            List<Candidate> c = new List<Candidate>();

            c.AddRange(smithSet.GetTopCycle(fixture.Candidates.Values));
            Assert.Equal("Chris", c[0].Name);

            c.Clear();
            c.AddRange(schwartzSet.GetTopCycle(fixture.Candidates.Values));
            Assert.Equal("Chris", c[0].Name);

        }

        [Fact]
        public void RankedVoteCountTest()
        {
            IVoteCount vc = new RankedVoteCount(fixture.Candidates.Values, fixture.Ballots, fixture.batchEliminator);
            vc.CountBallots();
            Assert.Equal(20, vc.GetVoteCount(fixture.Candidates[0]));
            Assert.Equal(13, vc.GetVoteCount(fixture.Candidates[1]));
        }

        [Fact]
        public void RankedVoteCountsTest()
        {
            IVoteCount vc = new RankedVoteCount(fixture.Candidates.Values, fixture.Ballots, fixture.batchEliminator);

            vc.CountBallots();
            Dictionary<Candidate, decimal> vcd;
            vcd = vc.GetVoteCounts();
            // Do the numbers match expected?
            Assert.Equal(20, vcd[fixture.Candidates[0]]);
            Assert.Equal(13, vcd[fixture.Candidates[1]]);
        }

        [Fact]
        public void RankedVoteCountsEliminationTest()
        {
            List<Candidate> c = new List<Candidate>(fixture.Candidates.Values);

            IVoteCount vc = new RankedVoteCount(c, fixture.Ballots, fixture.batchEliminator);

            Dictionary<Candidate, CandidateState> cS;

            Dictionary<Candidate, decimal> vcd;
            vc.ApplyTabulation();
            vc.CountBallots();

            // Test after one round
            vcd = vc.GetVoteCounts();

            // Do the numbers match expected?
            Assert.Equal(20 + 5, vcd.Where(x => x.Key == fixture.Candidates[0]).Select(x => x.Value).First());
            Assert.Equal(15 + 8, vcd.Where(x => x.Key == fixture.Candidates[2]).Select(x => x.Value).First());

            // Complete tabulation
            while (vc.GetTabulation().Count() > 0)
            {
                vc.ApplyTabulation();
                vc.CountBallots();
            }

            vcd = vc.GetVoteCounts();

            cS = vc.GetFullTabulation();
            // Vote counts
            Assert.Equal("Alex", cS.First().Key.Name);
            Assert.Equal(20 + 8 + 5, cS.First().Value.VoteCount);
            Assert.Equal(cS.First().Value.VoteCount, vcd.First().Value);
            Assert.Single(vcd);
        }
    }
}
