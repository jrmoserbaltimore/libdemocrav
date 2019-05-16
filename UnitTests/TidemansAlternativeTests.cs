using System;
using System.Collections.Generic;
using Xunit;
using MoonsetTechnologies.Voting.Tiebreaking;
using MoonsetTechnologies.Voting.Utility;
using Xunit.Abstractions;


namespace MoonsetTechnologies.Voting.Development.Tests
{
    public class TidemansAlternativeTests : AbstractTabulatorTest
    {
        public TidemansAlternativeTests(ITestOutputHelper testOutputHelper, BallotSetFixture fixture)
            : base(testOutputHelper, fixture)
        {
            tabulatorFactory = new TidemansAlternativeTabulatorFactory();
            // Use Last Difference by default
            tabulatorFactory.SetTiebreaker(new TiebreakerFactory<LastDifferenceTiebreaker>());
        }

        [Theory]
        [MemberData(nameof(GetFlatTests), parameters: new object[] { @"resources\testcases\TidemanTests.simpletabulatortest", "condorcet" })]
        [MemberData(nameof(GetFlatTests), parameters: new object[] { @"resources\testcases\TidemanTests.simpletabulatortest", "tideman's alternative" })]
        [MemberData(nameof(GetFlatTests), parameters: new object[] { @"resources\testcases\HistoricElections.simpletabulatortest", "condorcet" })]
        public void TidemansAlternativeTest(string filename, int seats, IEnumerable<string> winners)
            => TabulatorTest(filename, seats, winners);
    }
}
