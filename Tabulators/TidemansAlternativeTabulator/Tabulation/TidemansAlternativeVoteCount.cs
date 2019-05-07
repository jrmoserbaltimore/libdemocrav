using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonsetTechnologies.Voting.Analytics;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public class TidemansAlternativeVoteCount : AbstractRankedVoteCount
    {
        public TidemansAlternativeVoteCount(IEnumerable<Candidate> candidates,
            IEnumerable<IRankedBallot> ballots,
            IBatchEliminator batchEliminator)
            : base(candidates, ballots, batchEliminator)
        {

        }

        public override void CountBallots()
        {
            throw new NotImplementedException();
        }


        protected virtual IEnumerable<Candidate> CondorcetCheck(TopCycle t) => t.SchwartzSet;
        protected virtual IEnumerable<Candidate> RetainSet(TopCycle t) => t.SmithSet;

        // XXX:  This is moving up into the BatchEliminator.
        public override Dictionary<Candidate, CandidateState.States> GetTabulation()
        {

        }
    }
}
