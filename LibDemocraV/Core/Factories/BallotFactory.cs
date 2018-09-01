//
// Copyright (c) Moonset Technology Holdings, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//

using System;
using System.Collections;
using System.Collections.Generic;

namespace MoonsetTechnologies.Voting.Factories
{
    abstract class AbstractBallotFactory
    {
        public abstract Ballot CreateBallot();
        public abstract Ballot CopyBallot(Ballot ballot);
        public abstract Vote CreateVote();

        /// <summary>
        /// Get a ballot factory.
        /// </summary>
        /// <param name="race">A race for which we are creating ballots.</param>
        /// <returns>Returns a concrete factory producing ballots for the race.</returns>
        public static AbstractBallotFactory GetFactory(Race race)
        {
            // TODO:  Identify the type of ballot required for the race.
            // TODO:  Identify the appropriate candidates.
            // TODO:  Create the appropriate factory.


        }
    }


}