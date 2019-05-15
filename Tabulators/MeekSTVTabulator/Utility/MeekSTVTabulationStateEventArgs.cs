using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public class MeekSTVTabulationStateEventArgs : RankedTabulationStateEventArgs
    {
        public decimal Quota { get; set; }
        public decimal Surplus { get; set; }
    }
}
