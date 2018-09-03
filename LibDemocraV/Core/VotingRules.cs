//
// Copyright (c) Moonset Technology Holdings, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//

using System;
using System.Collections;
using System.Collections.Generic;
using MoonsetTechnologies.Voting.Factories;

namespace MoonsetTechnologies.Voting.VotingRules 
{
    public abstract class VotingRule
    {
        private AbstractBallotFactory BallotFactory { get; }

        public Ballot GetNewBallot() => BallotFactory.CreateBallot();
        public Ballot CopyBallot(ReadOnlyBallot ballot) => BallotFactory.CopyBallot(ballot);
    }
}