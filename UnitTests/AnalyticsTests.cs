using System;
using System.Collections.Generic;
using Xunit;
using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting;
using MoonsetTechnologies.Voting.Tiebreaking;
using System.Linq;
using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Utility;
using Xunit.Abstractions;

namespace MoonsetTechnologies.Voting.Development.Tests
{
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
            PairwiseGraph graph = new PairwiseGraph(fixture.Ballots);
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
    }
}
