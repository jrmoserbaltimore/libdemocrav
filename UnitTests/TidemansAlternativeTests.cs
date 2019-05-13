using System;
using System.Collections.Generic;
using Xunit;
using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting;
using MoonsetTechnologies.Voting.Tiebreaking;
using System.Linq;
using MoonsetTechnologies.Voting.Utility;
using MoonsetTechnologies.Voting.Tabulation;
using Xunit.Abstractions;

namespace MoonsetTechnologies.Voting.Development.Tests
{
    public class TidemansAlternativeTests : IClassFixture<BallotSetFixture>
    {
        private readonly ITestOutputHelper output;

        readonly BallotSetFixture fixture;

        public TidemansAlternativeTests(ITestOutputHelper testOutputHelper, BallotSetFixture fixture)
        {
            this.fixture = fixture;
            fixture.output = output = testOutputHelper;
        }

        [Fact]
        public void TidemansAlternativeTest()
        {
            List<string> winners = null;
            TidemansAlternativeTabulatorFactory f;
            AbstractTabulator t;

            void Monitor_TabulationComplete(object sender, TabulationStateEventArgs e)
            {
                winners = e.CandidateStates
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
            f = new TidemansAlternativeTabulatorFactory();

            // Use Last Difference
            f.SetTiebreaker(new TiebreakerFactory<LastDifferenceTiebreaker>());

            t = f.CreateTabulator();

            Assert.NotNull(t);
            Assert.IsType<TidemansAlternativeTabulator>(t);

            t.Monitor.TabulationComplete += Monitor_TabulationComplete;
            t.Monitor.RoundComplete += Monitor_RoundComplete;

            t.Tabulate(fixture.Ballots);

            t.Monitor.TabulationComplete -= Monitor_TabulationComplete;
            t.Monitor.RoundComplete -= Monitor_RoundComplete;

            Assert.NotNull(winners);
            Assert.Single(winners);
            Assert.Equal("Chris", winners.First());
        }
    }
}
