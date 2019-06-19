using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Storage;
using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;

namespace MoonsetTechnologies.Application
{
    static class Program
    {
        static void Main(string[] args)
        {
            switch (args[0])
            {
                case "tabulate":
                    break;
                default:
                    throw new ArgumentOutOfRangeException("args", "Command is not available");
            }
        }
    }
}
