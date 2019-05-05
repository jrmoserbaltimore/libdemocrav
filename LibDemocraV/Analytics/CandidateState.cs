using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Analytics
{
    public class CandidateState
    {
        public enum States
        {
            defeated = 0,
            withdrawn = 1,
            hopeful = 2,
            elected = 3
        };
        public decimal VoteCount { get; set; }
        public States State { get; set; }

        public CandidateState()
        {
        }
    }
}
