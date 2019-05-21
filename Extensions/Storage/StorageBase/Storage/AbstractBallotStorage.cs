using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MoonsetTechnologies.Voting.Storage
{
    public abstract class AbstractBallotStorage
    {
        public BallotFactory ballotFactory { get; set; } = new BallotFactory(null);
        public abstract BallotSet LoadBallots(Stream stream);
        public abstract IEnumerable<Ballot> StoreBallots();
    }
}
