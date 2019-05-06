using System;
using System.Collections.Generic;
using System.Text;
using MoonsetTechnologies.Voting.Tiebreaking;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public class TidemansAlternativeBatchEliminator : RunoffBatchEliminator
    {
        public TidemansAlternativeBatchEliminator(ITiebreaker tiebreakers, int seats = 1)
            : base(tiebreakers, seats)
        {
        }
    }
}
