using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{

    public class TiebreakerStateEventArgs : EventArgs
    {
        public Dictionary<Candidate, Dictionary<Candidate, bool>> WinPairs { get; set; }
        public string Note { get; set; }
    }
}
