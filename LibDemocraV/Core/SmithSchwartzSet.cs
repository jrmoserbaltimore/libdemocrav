//
// Copyright (c) Secure Democratic Election Services, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//


using System;
using System.Collections;
using System.Collections.Generic;

namespace DemocraticElections.Voting.Analysis
{
    /* Smith and Schwartz Set
     * 
     * These classes identify the Smith and Schwartz sets from the pairwise
     * contests.
     */
    public abstract class SmithSchwartzSet : IBallotSheet, IRace
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

    /* Exposes candidates and results from the Smith Set */
    public class SmithSet : SmithSchwartzSet
    {

    }

    /* Exposes candidates and results from the Schwartz Set */
    public class SchwartzSet : SmithSchwartzSet
    {

    }
}