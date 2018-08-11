//
// Copyright (c) Secure Democratic Election Services, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//


using System;
using System.Collections;
using System.Collections.Generic;

namespace DemocraticElections.Voting
{
    /* Vote to Cast */
    public interface IVote
    {
        Candidate Candidate { get; }
        int Value { get; }
    }

    public interface IBallot : IEnumerable<IVote>
    {
        void Cast(IVote vote);
    }

    public interface IResult
    {
        Candidate Candidate { get; }
        int Votes { get; }
    }

    public interface IRace : IEnumerable<IBallot>, IEnumerable<IResult>, IEnumerable<Candidate>
    {
        void Cast(IBallot ballot);
        /* Returns an IRace which enumerates IResult objects for winners only*/
        IRace Results { get; }
        /* Compute one round of eliminations or such */
        IRace NextRound { get; }
    }

    public interface IBallotSheet : IEnumerable<IRace>
    {
        void Cast(IBallot votes);

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

    /* A ranked ballot with no ties.
     * 
     * Ties don't translate to plurality, which breaks methods such as
     * Instant Run-off Voting,*/
    public class RankedBallot : Ballot
    {
        /* eliminates any votes for candidates not in (c) from ballot (b) */
        public RankedBallot(IBallot b, IEnumerable<Candidate> c)
        {
            throw new NotImplementedException();
        }

        public override void Cast(IVote vote)
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

    public class Vote : IVote
    {
        public Candidate Candidate { get; private set; }
        public uint Value { get; private set; }

        public Vote(Candidate c, uint v)
        {
            Candidate = c;
            Value = v;
        }
    }

    /* A plurality ballot */
    public abstract class Ballot : IBallot
    {
        protected int seats;
        protected List<Candidate> candidates = new List<Candidate>();

        public Ballot(IVote vote)
            : this(vote, 1)
        {
         
        }

        /* Multiple seats in a multi-seat race */
        public Ballot(IVote vote, int seats)
        {
            this.seats = seats;
            candidates.Add(vote.Candidate);
        }

        /* Insert the vote at the top and delete the last candidate if
         * more votes than seats
         */
        public Ballot(Ballot ballot, IVote vote)
            : this(vote, ballot.seats)
        {
            candidates.AddRange(ballot.candidates);
            while (candidates.Count > seats)
            {
                candidates.RemoveAt(seats - 1);
            }
        }

        public virtual void Cast(IVote vote)
        {

        }
        public abstract IEnumerator<IVote> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
