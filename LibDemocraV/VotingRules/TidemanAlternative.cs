//
// Copyright (c) Secure Democratic Election Services, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//


using System;
using System.Collections;
using System.Collections.Generic;

namespace DemocraticElections.Voting.VotingRules
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
    abstract class TidemanAlternative : IRace
    {
        public void Cast(Ballot ballot)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<Ballot> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public IRace Results => throw new NotImplementedException();

        IEnumerator<IResult> IEnumerable<IResult>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator<Candidate> IEnumerable<Candidate>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}