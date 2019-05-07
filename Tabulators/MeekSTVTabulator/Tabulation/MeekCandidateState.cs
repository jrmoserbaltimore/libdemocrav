using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public class MeekCandidateState : CandidateState
    {
        public decimal KeepFactor { get; set; }

        public MeekCandidateState()
            : base()
        {
            KeepFactor = 1;
        }
    }
}
