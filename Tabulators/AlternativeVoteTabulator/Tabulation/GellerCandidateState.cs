using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public class GellerCandidateState : CandidateState
    {
        public decimal BordaScore { get; set; }

        public GellerCandidateState()
            : base()
        {
            BordaScore = 0;
        }
    }
}
