using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public class RankedTabulationStateEventArgs : TabulationStateEventArgs
    {
        public IEnumerable<Candidate> SmithSet { get; set; }
        public IEnumerable<Candidate> SchwartzSet { get; set; }
    }
}
