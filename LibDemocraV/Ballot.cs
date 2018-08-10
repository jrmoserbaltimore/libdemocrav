//
// Copyright (c) Secure Democratic Election Services, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//


using System;
using System.Collections;
using System.Collections.Generic;

namespace DemocraticElections.Voting
{
    public interface IVoter
    {
        uint VoterID { get; }
    }

    public interface ICandidate
    {
        string Name { get; }
    }

    /* Vote to Cast */
    public interface IVote
    {
        ICandidate Candidate { get; }
        uint Value { get; }
    }

    public interface IBallot : IEnumerable<IVote>
    {
        void Cast(IVote vote);
    }

    public interface IResult
    {
        ICandidate Candidate { get; }
        uint Votes { get; }
    }

    public interface IRace : IEnumerable<IBallot>, IEnumerable<IResult>, IEnumerable<ICandidate>
    {
        void Cast(IBallot ballot);
        /* Returns an IRace which enumerates IResult objects for winners only*/
        IRace Winners();
    }

    public interface IBallotSheet : IEnumerable<IRace>
    {
        void Cast(IBallot votes);

    }

    public class Result : IResult
    {
        public Result(ICandidate c, uint v)
        {
            Candidate = c;
            Votes = v;
        }

        public Result(IResult r)
            : this(r.Candidate, r.Votes + 1)
        {
            /* This space intentionally left blank */
        }

        public ICandidate Candidate { get; private set; }

        public uint Votes { get; private set; }
    }

    public class RankedBallot : IBallot
    {
        /* eliminates any votes for candidates not in (c) from ballot (b) */
        public RankedBallot(IBallot b, IEnumerable<ICandidate> c)
        {
            throw new NotImplementedException();
        }

        public void Cast(IVote vote)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<IVote> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
