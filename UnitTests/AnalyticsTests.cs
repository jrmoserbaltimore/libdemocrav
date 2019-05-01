﻿using System;
using System.Collections.Generic;
using Xunit;
using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting;

namespace MoonsetTechnologies.Voting.Development.Tests
{
    public class BallotSetFixture
    {
        // indexed by candidate number in the input
        public Dictionary<int, Candidate> Candidates { get;  private set; }
        public List<IRankedBallot> Ballots { get;  private set; }

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

        BallotSetFixture ballotSetFixture;

        public AnalyticsTests(BallotSetFixture fixture)
        {
            ballotSetFixture = fixture;
        }

        [Fact]
        public void PairwiseGraphBuildTest()
        {
            PairwiseGraph graph = new PairwiseGraph(ballotSetFixture.Candidates.Values, ballotSetFixture.Ballots);
            Assert.Equal((20, 28), graph.GetVoteCount(ballotSetFixture.Candidates[0], ballotSetFixture.Candidates[1]));
            Assert.NotNull(graph);
        }

        [Fact]
        public void TopCycleTest()
        {
            TopCycle t = new TopCycle(ballotSetFixture.Candidates.Values, ballotSetFixture.Ballots);
            List<Candidate> c = new List<Candidate>();

            c.AddRange(t.SmithSet);
            Assert.Equal("Chris", c[0].Name);

            c.Clear();
            c.AddRange(t.SchwartzSet);
            Assert.Equal("Chris", c[0].Name);

        }

        [Fact]
        public void RankedVoteCountTest()
        {
            IVoteCount vc = new RankedVoteCount(ballotSetFixture.Candidates.Values, ballotSetFixture.Ballots);
            Assert.Equal(20, vc.GetVoteCount(ballotSetFixture.Candidates[0]));
            Assert.Equal(13, vc.GetVoteCount(ballotSetFixture.Candidates[1]));
        }

        [Fact]
        public void CachedVoteCountTest()
        {
            IVoteCount vc = new RankedVoteCount(ballotSetFixture.Candidates.Values, ballotSetFixture.Ballots);
            IVoteCount cvc = new CachedVoteCount(ballotSetFixture.Candidates.Values, vc);
            
            foreach (int i in ballotSetFixture.Candidates.Keys)
                Assert.Equal(vc.GetVoteCount(ballotSetFixture.Candidates[i]), cvc.GetVoteCount(ballotSetFixture.Candidates[i]));
        }

        [Fact]
        public void RankedVoteCountsTest()
        {
            IVoteCount vc = new RankedVoteCount(ballotSetFixture.Candidates.Values, ballotSetFixture.Ballots);
            IVoteCount cvc = new CachedVoteCount(ballotSetFixture.Candidates.Values, vc);

            Dictionary<Candidate, int> vcd;
            vcd = cvc.GetVoteCounts();
            // Do the numbers match expected?
            Assert.Equal(20, vcd[ballotSetFixture.Candidates[0]]);
            Assert.Equal(13, vcd[ballotSetFixture.Candidates[1]]);

            // Are the totals coequal?
            foreach (int i in ballotSetFixture.Candidates.Keys)
                Assert.Equal(vc.GetVoteCount(ballotSetFixture.Candidates[i]), cvc.GetVoteCount(ballotSetFixture.Candidates[i]));
        }

        [Fact]
        public void RankedVoteCountsEliminationTest()
        {
            List<Candidate> c = new List<Candidate>(ballotSetFixture.Candidates.Values);
            c.Remove(ballotSetFixture.Candidates[2]);

            IVoteCount vc = new RankedVoteCount(c, ballotSetFixture.Ballots);
            IVoteCount cvc = new CachedVoteCount(c, vc);

            Dictionary<Candidate, int> vcd;
            vcd = cvc.GetVoteCounts();

            // Do the numbers match expected?
            Assert.Equal(20, vcd[ballotSetFixture.Candidates[0]]);
            Assert.Equal(13+15, vcd[ballotSetFixture.Candidates[1]]);

            // Are the totals coequal?
            foreach (int i in ballotSetFixture.Candidates.Keys)
                Assert.Equal(vc.GetVoteCount(ballotSetFixture.Candidates[i]), cvc.GetVoteCount(ballotSetFixture.Candidates[i]));
        }
    }
}
