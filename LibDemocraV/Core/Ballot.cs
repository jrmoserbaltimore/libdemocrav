//
// Copyright (c) Moonset Technology Holdings, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//

using System;
using System.Collections;
using System.Collections.Generic;

namespace MoonsetTechnologies.Voting
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

    public abstract class Ballot : IReadOnlyCollection<Vote>, ICloneable
    {
        public Race Race { get; }
        protected List<Vote> Votes { get; } = new List<Vote>();

        // Vote collection
        int IReadOnlyCollection<Vote>.Count => Votes.Count;
        IEnumerator<Vote> IEnumerable<Vote>.GetEnumerator() => Votes.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<Vote>)this).GetEnumerator();

        private Ballot() => throw new NotImplementedException();

        public Ballot(Race race) => Race = race;

        /* Copy constructor:  iterate each vote and cast it */
        protected Ballot(Ballot ballot)
        {
            foreach (Vote v in ballot)
                Cast(v.Clone());
        }

        public abstract void Cast(Vote vote);

        public abstract object Clone();
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