using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public class RankedTabulator : AbstractTabulator<AbstractRankedVoteCount>
    {
        IEnumerable<Candidate> SchwartzSet { get; }
        IEnumerable<Candidate> SmithSet { get; }

        /// <inheritdoc/>
        public RankedTabulator(AbstractRankedVoteCount voteCount)
            : base(voteCount)
        {
        }
    }
}