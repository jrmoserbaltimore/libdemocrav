//
// Copyright (c) Moonset Technology Holdings, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//

using System;
using System.Collections;
using System.Collections.Generic;

namespace MoonsetTechnologies.Voting.Factories
{
    /// <summary>
    /// Base class for implementing a factory for ballots.
    /// </summary>
    public abstract class AbstractBallotFactory
    {
        public abstract Ballot CreateBallot();
        public abstract Ballot CopyBallot(ReadOnlyBallot ballot);
        public abstract Vote CreateVote();
    }


}