using System;
using Xunit;

namespace MoonsetTechnologies.Voting.Development.Tests
{
    public class PeopleTest
    {
        [Fact]
        public void VoterIsPerson()
        {
            Person p = new Voter();
            Voter v = new Voter(p);
            Assert.Equal(p.Id, v.Id);
        }
    }
}
