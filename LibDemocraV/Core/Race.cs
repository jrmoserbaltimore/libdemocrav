//
// Copyright (c) Secure Democratic Election Services, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//


using System;
using System.Collections;
using System.Collections.Generic;

namespace DemocraticElections.Voting
{
    public interface IResult
    {
        Candidate Candidate { get; }
        int Votes { get; }
    }

    public class Result : IResult
    {
        public Candidate Candidate { get; private set; }

        public int Votes { get; private set; }
        public Result(Candidate c, int v)
        {
            Candidate = c;
            Votes = v;
        }

        public Result(IResult r)
            : this(r.Candidate, r.Votes + 1)
        {
            /* This space intentionally left blank */
        }
    }

    public interface IRace : IEnumerable<Ballot>, IEnumerable<IResult>, IEnumerable<Candidate>
    {
        void Cast(Ballot ballot);
        /* Returns an IRace which enumerates IResult objects for winners only*/
        IRace Results { get; }
        /* Compute one round of eliminations or such */
        IRace NextRound { get; }
    }

    public abstract class Race : IEnumerable<Ballot>, IEnumerable<IResult>, IEnumerable<Candidate>
    {

    }
}
