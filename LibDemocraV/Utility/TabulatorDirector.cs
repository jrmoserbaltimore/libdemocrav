using MoonsetTechnologies.Voting.Tabulation;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public abstract class TabulatorDirector<T,U>
        where T : IBallot
        where U : ITabulator
    {
        public abstract U Build(TabulatorBuilder<T,U> builder);
    }
}