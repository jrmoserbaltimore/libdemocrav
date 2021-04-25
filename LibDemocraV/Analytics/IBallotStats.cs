using MoonsetTechnologies.Voting.Ballots;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Analytics
{
    public interface IBallotStats
    {
        HashSet<Candidate> GetCandidates();
        Dictionary<Candidate, decimal> GetFirstPreferences();        
    }
}
