using MoonsetTechnologies.Voting.Tabulation;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public class TabulationStateEventArgs : EventArgs
    {
        public Dictionary<Candidate, CandidateState> CandidateStates { get; set; }
        public string Note { get; set; }
    }
}
