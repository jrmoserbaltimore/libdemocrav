using System;
using System.Collections.Generic;
using System.Linq;
using MoonsetTechnologies.Voting.Utility;

namespace MoonsetTechnologies.Voting.Tabulation
{
    class MeekGellerSTVTabulator : MeekSTVTabulator
    {
        public MeekGellerSTVTabulator(TabulationMediator mediator,
        AbstractTiebreakerFactory tiebreakerFactory,
        IEnumerable<ITabulatorSetting> tabulatorSettings)
        : base(mediator, tiebreakerFactory, tabulatorSettings)
        {

        }

        // Finds the candidates with the lowest Borda score
        protected override IEnumerable<Candidate> GetEliminationCandidates()
        {
            decimal minScore = (candidateStates.Select(x => (x.Value as MeekCandidateState).BordaScore).Min());
            return candidateStates.Where(x => (x.Value as MeekCandidateState).BordaScore == minScore).Select(x=> x.Key);
        }
    }
}
