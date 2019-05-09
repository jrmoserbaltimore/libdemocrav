using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public abstract class AbstractTabulatorFactory
    {
        public abstract AbstractTabulator CreateTabulator(IEnumerable<Candidate> candidates,
            IEnumerable<Ballot> ballots);

        public abstract Ballot CreateBallot(IEnumerable<Vote> votes);

        public abstract Vote CreateVote(Candidate candidate, decimal value);
    }
}