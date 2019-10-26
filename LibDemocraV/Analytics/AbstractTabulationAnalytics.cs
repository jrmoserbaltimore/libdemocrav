using MoonsetTechnologies.Voting.Ballots;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Analytics
{
    public class AbstractTabulationAnalytics
    {
        public AbstractTabulationAnalytics(BallotSet ballots, int seats = 1)
        {

        }

        public HashSet<Candidate> GetCandidates(Type t)
        {
            return null;
        }
    }
}
