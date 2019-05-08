using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public abstract class AbstractTabulatorFactory<T,U>
        where T : IBallot
        where U : ITabulator
    {
        public abstract U CreateTabulator(IEnumerable<Candidate> candidates,
            IEnumerable<T> ballots);

        public abstract IBatchEliminator CreateBatchEliminator();

        public abstract ITiebreaker CreateTiebreaker();
    }
}