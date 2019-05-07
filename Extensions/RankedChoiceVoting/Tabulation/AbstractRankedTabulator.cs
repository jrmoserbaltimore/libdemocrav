using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public abstract class AbstractRankedTabulator : AbstractTabulator<IRankedBallot, AbstractRankedVoteCount>
    {
        IEnumerable<Candidate> SchwartzSet { get; }
        IEnumerable<Candidate> SmithSet { get; }
    }
}