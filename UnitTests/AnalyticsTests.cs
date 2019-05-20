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
    }
}
