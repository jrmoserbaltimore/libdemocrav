using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoonsetTechnologies.Voting.Tabulation;

namespace MoonsetTechnologies.Voting.Tiebreaking
{
    class TieBreakerValidator : ITiebreaker
    {
        private readonly ITiebreaker t;
        public bool FullyInformed => t.FullyInformed;

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

        public void UpdateTiebreaker(Dictionary<Candidate, CandidateState> candidateStates)
        {
            t.UpdateTiebreaker(candidateStates);
        }
    }
}
