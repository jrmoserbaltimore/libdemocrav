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
using System.IO.MemoryMappedFiles;
using System.IO;
using MoonsetTechnologies.Voting.Storage;

namespace MoonsetTechnologies.Voting.Development.Tests
{
    public class AnalyticsTests : IClassFixture<BallotSetFixture>
    {

        readonly BallotSetFixture fixture;

        public AnalyticsTests(BallotSetFixture fixture)
        {
            this.fixture = fixture;
        }

        [Theory]
        [MemberData(nameof(AbstractTabulatorTest.GetFlatTests), parameters: new object[] { @"resources\testcases\TidemanTests.simpletabulatortest", "condorcet-schwartz" }, MemberType = typeof(AbstractTabulatorTest))]
        [MemberData(nameof(AbstractTabulatorTest.GetFlatTests), parameters: new object[] { @"resources\testcases\TidemanTests.simpletabulatortest", "tideman's alternative" }, MemberType = typeof(AbstractTabulatorTest))]
        [MemberData(nameof(AbstractTabulatorTest.GetFlatTests), parameters: new object[] { @"resources\testcases\HistoricElections.simpletabulatortest", "condorcet-schwartz" }, MemberType = typeof(AbstractTabulatorTest))]
        public void PairwiseGraphTest(string filename, int seats, IEnumerable<string> winners)
        {
            BallotSet ballots;
            AbstractBallotStorage s = new DavidHillFormat();

            using (MemoryMappedFile file = MemoryMappedFile.CreateFromFile(
                new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read),
                null, 0, MemoryMappedFileAccess.CopyOnWrite, HandleInheritability.None, false))
            {
                using (MemoryMappedViewStream vs = file.CreateViewStream(0, 0, MemoryMappedFileAccess.Read))
                    ballots = s.LoadBallots(vs);
            }
            Assert.NotNull(ballots);

            PairwiseGraph g;
            g = new PairwiseGraph(ballots);
        }
        [Theory]
        [MemberData(nameof(AbstractTabulatorTest.GetFlatTests), parameters: new object[] { @"resources\testcases\TidemanTests.simpletabulatortest", "smith set" }, MemberType = typeof(AbstractTabulatorTest))]
        public void TopCycleSmithSetTest(string filename, int seats, IEnumerable<string> winners)
        {
            BallotSet ballots;
            AbstractBallotStorage s = new DavidHillFormat();

            using (MemoryMappedFile file = MemoryMappedFile.CreateFromFile(
                new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read),
                null, 0, MemoryMappedFileAccess.CopyOnWrite, HandleInheritability.None, false))
            {
                using (MemoryMappedViewStream vs = file.CreateViewStream(0, 0, MemoryMappedFileAccess.Read))
                    ballots = s.LoadBallots(vs);
            }
            Assert.NotNull(ballots);

            PairwiseGraph g;
            g = new PairwiseGraph(ballots);
            TopCycle t = new TopCycle(g);

            HashSet<string> w = t.GetTopCycle(new Candidate[] { }).Select(x => x.Name).ToHashSet();
            Assert.NotNull(w);
            HashSet<string> unexpectedWinners = w.Except(winners).ToHashSet();
            HashSet<string> unexpectedLosers = winners.Except(w).ToHashSet();

            Assert.Empty(unexpectedWinners);
            Assert.Empty(unexpectedLosers);
        }
    }
}
