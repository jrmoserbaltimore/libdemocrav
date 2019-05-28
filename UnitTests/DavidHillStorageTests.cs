using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Storage;
using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Tiebreaking;
using MoonsetTechnologies.Voting.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
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
            BallotSet ballots;
            using (MemoryMappedFile file =
                MemoryMappedFile.CreateFromFile(
                new FileStream("./resources/electiondata/historic_elections/BurlingtonVT2009.HIL", FileMode.Open, FileAccess.Read, FileShare.Read),
                null, 0, MemoryMappedFileAccess.CopyOnWrite, HandleInheritability.None, false))
            {
                using (MemoryMappedViewStream vs = file.CreateViewStream(0,0,MemoryMappedFileAccess.Read))
                    ballots = s.LoadBallots(vs);
            }                

            Assert.NotNull(ballots);

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

            f = new TidemansAlternativeTabulatorFactory();

            // Use Last Difference
            f.SetTiebreaker(new LastDifferenceTiebreakerFactory());

            t = f.CreateTabulator();

            Assert.NotNull(t);

            t.Monitor.TabulationComplete += Monitor_TabulationComplete;
            t.Monitor.RoundComplete += Monitor_RoundComplete;

            t.Tabulate(ballots, null, 1);

            t.Monitor.TabulationComplete -= Monitor_TabulationComplete;
            t.Monitor.RoundComplete -= Monitor_RoundComplete;
        }

    }
}
