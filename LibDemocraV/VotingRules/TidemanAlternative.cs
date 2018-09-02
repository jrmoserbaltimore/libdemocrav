//
// Copyright (c) Moonset Technology Holdings, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//


using System;
using System.Collections;
using System.Collections.Generic;
using MoonsetTechnologies.Voting.Analysis;

namespace MoonsetTechnologies.Voting.VotingRules
{
    /* Alternative Race
     * 
     * The Tideman Alternative rules will:
     *  - Accept ranked ballots without equal rankings
     *  
     * To compute:
     * 
     *  - Filter results to SmithSet or SchwartzSet
     *  - If more than one result, one round of InstantRunoffVoting
     *  - Repeat until one result
     */
    abstract class TidemanAlternative : Race
    {

        public void Cast(ReadOnlyBallot ballot)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<ReadOnlyBallot> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Race Results => throw new NotImplementedException();

    }
}