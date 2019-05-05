using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public interface IRankedTabulator : ITabulator
    {
        IEnumerable<Candidate> SchwartzSet { get; }
        IEnumerable<Candidate> SmithSet { get; }
    }
}