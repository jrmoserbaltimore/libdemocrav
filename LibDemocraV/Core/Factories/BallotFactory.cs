//
// Copyright (c) Secure Democratic Election Services, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//

using System;
using System.Collections;
using System.Collections.Generic;

namespace DemocraticElections.Voting.Factories
{
    abstract class BallotFactory
    {
        public abstract Ballot CreateBallot();
        public abstract Ballot CopyBallot(Ballot ballot);
        public abstract Vote CreateVote();
    }


}