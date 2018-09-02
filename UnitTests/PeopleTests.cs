using System;
using Xunit;
using MoonsetTechnologies.Voting;

namespace MoonsetTechnologies.Voting.Development.Tests
{
    public class PeopleUnitTest
    {
        [Fact]
        public void VoterIsPerson()
        {
            Person p = new Person();
            Voter v = new Voter(p);
            Assert.Equal(p.Id, v.Id);
        }
    }
}
