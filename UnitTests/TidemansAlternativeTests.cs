using System;
using System.Collections.Generic;
using Xunit;
using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting;
using MoonsetTechnologies.Voting.Tiebreaking;
using System.Linq;
using MoonsetTechnologies.Voting.Utility;
using MoonsetTechnologies.Voting.Tabulation;

namespace MoonsetTechnologies.Voting.Development.Tests
{
    public class TidemansAlternativeTests : IClassFixture<BallotSetFixture>
    {

        readonly BallotSetFixture fixture;

        public TidemansAlternativeTests(BallotSetFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void TidemansAlternativeTest()
        {
            AbstractTabulatorFactory f
                = new TidemansAlternativeTabulatorFactory();

            AbstractTabulator t 
                = f.CreateTabulator(fixture.Candidates.Values, fixture.Ballots);

            Assert.NotNull(t);
            Assert.IsType<RankedTabulator>(t);

            while (!t.Complete)
                t.TabulateRound();
            Assert.True(t.Complete);

            List<string> winners = t.GetFullTabulation()
                .Where(x => x.Value.State == CandidateState.States.elected)
                .Select(x => x.Key.Name).ToList();

            Assert.Single(winners);
            Assert.Equal("Chris", winners.First());
        }
    }
}
