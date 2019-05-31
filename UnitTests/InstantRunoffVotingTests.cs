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
    public class InstantRunoffVotingTests : AbstractTabulatorTest
    {
       public InstantRunoffVotingTests(ITestOutputHelper testOutputHelper, BallotSetFixture fixture)
            : base(testOutputHelper, fixture)
        {
            tabulatorFactory = new InstantRunoffVotingTabulatorFactory();
            // Use Last Difference by default
            tabulatorFactory.SetTiebreaker(new LastDifferenceTiebreakerFactory());
        }

        [Theory]
        [MemberData(nameof(GetFlatTests), parameters: new object[] { @"resources\testcases\HistoricElections.simpletabulatortest", "instant runoff voting" })]
        [MemberData(nameof(GetFlatTests), parameters: new object[] { @"resources\testcases\TidemanTests.simpletabulatortest", "instant runoff voting" })]
        public void InstantRunoffVotingTabulatorTest(string filename, int seats, IEnumerable<string> winners)
            => TabulatorTest(filename, seats, winners);
    }
}
