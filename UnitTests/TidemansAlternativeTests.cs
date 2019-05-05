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
        public void TidemansAlternativeFactoryTest()
        {
            AbstractTabulatorFactory<IRankedBallot, IRankedTabulator> factory
                = new TidemansAlternativeTabulatorFactory();

            IRankedTabulator t = factory.CreateTabulator(fixture.Candidates.Values, fixture.Ballots);

            Assert.NotNull(t);
            Assert.IsType<TidemansAlternativeTabulator>(t);
        }

        [Fact]
        public void TidemansAlternativeTest()
        {
            AbstractTabulatorFactory<IRankedBallot, IRankedTabulator> factory
                = new TidemansAlternativeTabulatorFactory();

            IRankedTabulator t = factory.CreateTabulator(fixture.Candidates.Values, fixture.Ballots);

            Assert.NotNull(t);
            Assert.IsType<TidemansAlternativeTabulator>(t);

            t.TabulateRound();
            Assert.True(t.Complete);
            Assert.Single(t.Candidates);
            Assert.Equal("Chris", t.Candidates.First().Name);
        }
    }
}
