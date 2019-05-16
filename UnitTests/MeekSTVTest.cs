using System;
using System.Collections.Generic;
using Xunit;
using MoonsetTechnologies.Voting.Tiebreaking;
using MoonsetTechnologies.Voting.Utility;
using Xunit.Abstractions;


namespace MoonsetTechnologies.Voting.Development.Tests
{
    public class MeekSTVTabulatorTests : AbstractTabulatorTest
    {
        public MeekSTVTabulatorTests(ITestOutputHelper testOutputHelper, BallotSetFixture fixture)
            : base(testOutputHelper, fixture)
        {
            tabulatorFactory = new MeekSTVTabulatorFactory();
            // Use Last Difference by default
            tabulatorFactory.SetTiebreaker(new TiebreakerFactory<LastDifferenceTiebreaker>());
        }

        [Theory]
        [MemberData(nameof(GetFlatTests), parameters: new object[] { @"resources\testcases\TidemanTests.simpletabulatortest", "meek-stv" })]
        [MemberData(nameof(GetFlatTests), parameters: new object[] { @"resources\testcases\HistoricElections.simpletabulatortest", "meek-stv" })]
        public void MeekSTVTest(string filename, int seats, IEnumerable<string> winners)
            => TabulatorTest(filename, seats, winners);
    }
}
