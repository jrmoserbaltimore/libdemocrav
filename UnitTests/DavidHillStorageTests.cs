using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Storage;
using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Tiebreaking;
using MoonsetTechnologies.Voting.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace MoonsetTechnologies.Voting.Development.Tests
{
    public class DavidHillStorageTests : IClassFixture<BallotSetFixture>
    {
        private readonly ITestOutputHelper output;

        readonly BallotSetFixture fixture;

        public DavidHillStorageTests(ITestOutputHelper testOutputHelper, BallotSetFixture fixture)
        {
            this.fixture = fixture;
            fixture.output = output = testOutputHelper;
        }

        [Fact]
        public void A1Test()
        {
            AbstractBallotStorage s = new DavidHillFormat();
            FileStream file = new FileStream("./resources/tidemandata/A1.HIL", FileMode.Open);
            IEnumerable<CountedBallot> ballots = s.LoadBallots(file);

            Assert.NotNull(ballots);


            BallotSet bset = new BallotSet(ballots);

            List<string> winners = null;
            AbstractTabulatorFactory f;
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

            f = new MeekSTVTabulatorFactory();

            // Use Last Difference
            f.SetTiebreaker(new TiebreakerFactory<LastDifferenceTiebreaker>());

            t = f.CreateTabulator();

            Assert.NotNull(t);

            t.Monitor.TabulationComplete += Monitor_TabulationComplete;
            t.Monitor.RoundComplete += Monitor_RoundComplete;

            t.Tabulate(bset,null,3);

            t.Monitor.TabulationComplete -= Monitor_TabulationComplete;
            t.Monitor.RoundComplete -= Monitor_RoundComplete;
        }

    }
}
