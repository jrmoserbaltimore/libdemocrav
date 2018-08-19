//
// Copyright (c) Secure Democratic Election Services, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//

using System;
using System.Collections;
using System.Collections.Generic;

namespace DemocraticElections.Voting
{
    /* Vote to Cast.  Immutable object. */
    public class Vote : IComparable
    {
        public Candidate Candidate { get; }
        public int Value { get; }

        public Vote(Vote v)
            : this(v, v.Value)
        {

        }

        public Vote(Vote v, int value)
            : this(v.Candidate, value)
        {

        }

        public Vote(Candidate c, int value)
        {
            Candidate = new Candidate(c);
            Value = value;
        }


        public int CompareTo(object obj)
        {
            Vote v = obj as Vote;

            if (obj == null)
                return 1;

            if (v != null)
                return this.Value.CompareTo(v.Value);
            else
                throw new ArgumentException("Object is not a Vote");
        }
    }

    public abstract class Ballot : ICollection<Vote>
    {
        protected List<Vote> Votes { get; } = new List<Vote>();
        protected Race Race { get; }

        // Vote collection
        int ICollection<Vote>.Count => Votes.Count;
        bool ICollection<Vote>.IsReadOnly => true;
        void ICollection<Vote>.Add(Vote item) => throw new NotImplementedException();
        bool ICollection<Vote>.Remove(Vote item) => throw new NotImplementedException();
        void ICollection<Vote>.Clear() => throw new NotImplementedException();
        bool ICollection<Vote>.Contains(Vote item) => throw new NotImplementedException();
        void ICollection<Vote>.CopyTo(Vote[] array, int arrayIndex) => throw new NotImplementedException();
        IEnumerator<Vote> IEnumerable<Vote>.GetEnumerator() => Votes.GetEnumerator();

        private Ballot() => throw new NotImplementedException();

        public Ballot(Race race) => Race = race;

        /* Copy constructor:  iterate each vote and cast it */
        public Ballot(Ballot ballot)
            : this(ballot.Race)
        {
            foreach (Vote v in ballot)
                Cast(new Vote(v));
        }

        public abstract void Cast(Vote vote);

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<Vote>)this).GetEnumerator();
    }

    public interface IBallotSheet : IEnumerable<IRace>
    {
        void Cast(Ballot votes);
    }

    /* Decorator to limit the ballot to specific candidates.
     * 
     * Does not clone the Ballot; it's a Decorator.
     */
    public class BallotCandidateFilter : Ballot
    {
        protected Ballot Ballot { get; }
        protected List<Candidate> Candidates { get; } = new List<Candidate>();

        /* Doesn't make sense to instantiate this without a ballot */
        private BallotCandidateFilter()
        {

        }

        public BallotCandidateFilter(Ballot b, IEnumerable<Candidate> candidates)
        {
            Ballot = b;
            Candidates.AddRange(candidates);
        }

        public override void Cast(Vote vote)
        {
            if (Candidates.Contains(vote.Candidate))
            {
                Ballot.Cast(vote);
            }
            // TODO:  Throw an exception here.
        }

        /* Return an enumerator with only votes for the candidates given */
        public override IEnumerator<Vote> GetEnumerator()
        {
            List<Vote> votes = new List<Vote>();
            foreach (Vote v in (IEnumerable<Vote>)Ballot.GetEnumerator())
            {
                if (Candidates.Contains(v.Candidate))
                {
                    votes.Add(new Vote(v));
                }
            }
            return votes.GetEnumerator();
        }
    }
}