//
// Copyright (c) Secure Democratic Election Services, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//

using System;
using System.Collections;
using System.Collections.Generic;

namespace DemocraticElections.Voting
{
    /* A ranked ballot.  Sequential rankings and ties.
     * 
     * This ballot is suitable for InstantRunoffApprovalVoting, which
     * simplifies to Instant Run-off Voting with no ties, Approval voting
     * with only first-ranked candidates, and Plurality with no ties and
     * only one first-ranked candidate.  Multi-seat plurality can also
     * use such a ballot.
     */
    public class RankedBallot : Ballot
    {
        //protected Dictionary<Candidate, Vote> Votes { get; set; } = new Dictionary<Candidate, Vote>();

        protected List<Vote> Votes { get; set; } = new List<Vote>();

        /* Copying the Dictionary is not a deep copy, so a RankedBallot
         * copy constructor isn't useful.
         */
        public RankedBallot(Ballot b)
            : base(b)
        {

        }

        public RankedBallot()
            : base()
        {

        }

        public override void Cast(Vote vote)
        {
            for (int i = 0; i < Votes.Count; i++)
            {
                if (Votes[i].Candidate.Equals(vote.Candidate))
                    Votes.RemoveAt(i);
            }

            Votes.Add(new Vote(vote));
            OrderVotes();
        }

        /* Just hand back all the Vote objects cast.  They're immutable. */
        public override IEnumerator<Vote> GetEnumerator()
        {
            return Votes.GetEnumerator();
        }

        protected virtual void OrderVotes()
        {
            int priorValue = 1;

            Votes.Sort();
            /* One vote, always rank 1 */
            if (Votes.Count >= 1 && Votes[0].Value != 1)
            {
                priorValue = Votes[0].Value;
                Votes[0] = new Vote(Votes[0], 1);
            }
            /* Start at second vote and close gaps */
            for (int i = 1; i < Votes.Count; i++)
            {
                int newValue = Votes[i].Value;
                /* If was tied with previous value, set to previous value's current value */
                if (Votes[i].Value == priorValue)
                    Votes[i] = new Vote(Votes[i], Votes[i - 1].Value);
                /* Else move up if not strictly sequential */
                else if (Votes[i].Value > Votes[i - 1].Value + 1)
                    Votes[i] = new Vote(Votes[i], Votes[i - 1].Value);
                priorValue = newValue;
            }
        }
    }

    /* 
     * A Ranked Ballot that prohibits ties.
     */
    public class RankedNoTies : RankedBallot
    {

        public RankedNoTies()
            : base()
        {
        }

        public RankedNoTies(Ballot b)
            : base(b)
        {

        }

        public override void Cast(Vote vote)
        {
            /* If not deleting a vote, sort the List and then open a space
               to accept the vote being cast. */
            if (vote.Value > 0)
            {
                Votes.Sort();
                /* Add 1 to all vote values ahead of insertion point */
                for (int i = vote.Value - 1; i < Votes.Count; i++)
                    Votes[i] = new Vote(Votes[i], Votes[i].Value + 1);
            }

            /* Cast the vote, which will also sort and compact the list */
            base.Cast(vote);
        }
    }

    /* 
     * An Approval ballot.
     */
    public class ApprovalBallot : RankedBallot
    {
        public ApprovalBallot()
            : base()
        {
        }

        public ApprovalBallot(Ballot b)
            : base(b)
        {

        }

        /* Casts the vote with a rank of 0 or 1 */
        public override void Cast(Vote vote)
        {
            int value = 0;

            if (vote.Value > 1)
                value = 1;

            base.Cast(new Vote(vote, value));
        }
    }

    /* A plurality ballot */
    public abstract class PluralityBallot : ApprovalBallot
    {

        protected int seats;

        public PluralityBallot(Vote vote)
            : this(vote, 1)
        {

        }

        /* Multiple seats in a multi-seat race */
        public PluralityBallot(Vote vote, int seats)
            : base()
        {
            this.seats = seats;
            Cast(vote);
        }


        public PluralityBallot(PluralityBallot b)
            : base(b)
        {
            this.seats = b.seats;
            foreach (Vote v in b)
                Cast(v);
        }

        /* Insert the vote at the top and delete the last candidate if
           more votes than seats */
        public override void Cast(Vote vote)
        {
            if (vote.Value >= 1)
                Votes.Insert(0, new Vote(vote, 1));
            else
                for (int i = 0; i < Votes.Count; i++)
                    if (Votes[i].Candidate.Equals(vote.Candidate))
                        Votes.RemoveAt(i);
            /* Trim excess votes */
            if (Votes.Count > seats)
                Votes.RemoveRange(seats, Votes.Count - seats);
        }

    }
}
