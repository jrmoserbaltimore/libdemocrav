using System;
using System.Collections.Generic;
using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Ballots;

namespace MoonsetTechnologies.Voting.Utility
{
    public class TabulationDetailsEventArgs
    {
        public Dictionary<Candidate, CandidateState> CandidateStates { get; set; }
        public BallotSet Ballots { get; set; }
        public int Seats { get; set; }
        public string Note { get; set; }
    }
}
