using System;
using System.Collections.Generic;
using System.Text;
using MoonsetTechnologies.Voting.Analytics;
using System.Linq;

namespace MoonsetTechnologies.Voting.Tiebreakers
{
    class TieBreakerValidator : ITiebreaker
    {
        private readonly ITiebreaker t;
        public bool AllTiesBreakable => t.AllTiesBreakable;

        public TieBreakerValidator(ITiebreaker tiebreaker)
        {
            t = tiebreaker;
        }

        public IEnumerable<Candidate> GetTieWinners(IEnumerable<Candidate> candidates)
        {
            IEnumerable<Candidate> winners = t.GetTieWinners(candidates);
            if (winners is null)
                throw new NotImplementedException();
            else if (winners.Count() == 0)
                throw new NotImplementedException();
            return winners;
        }

        public void UpdateTiebreaker<T>(Dictionary<Candidate, T> CandidateStates) where T : CandidateState
        {
            t.UpdateTiebreaker(CandidateStates);
        }
    }
}
