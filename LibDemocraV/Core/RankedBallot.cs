//
// Copyright (c) Moonset Technology Holdings, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MoonsetTechnologies.Voting
{
    /// <summary>
    /// A ranked ballot.  Sequential rankings and ties.
    /// 
    /// This ballot is suitable for InstantRunoffApprovalVoting, which
    /// simplifies to Instant Run-off Voting with no ties, Approval voting
    /// with only first-ranked candidates, and Plurality with no ties and
    /// only one first-ranked candidate.  Multi-seat plurality can also
    /// use such a ballot.
    /// </summary>
    public class RankedBallot : Ballot
    {
        /// <summary>
        /// Whether or not tied ranks are allowed.
        /// </summary>
        protected bool Ties { get; }

        public RankedBallot(Race race)
            : this(race, true)
        {

        }

        public RankedBallot(Race race, bool ties)
            : base(race)
        {
            Ties = ties;
        }

        /// <summary>
        /// Cast with ties, if allowed, otherwise Insert().
        /// </summary>
        /// <param name="vote">The vote to cast.</param>
        public override void Cast(Vote vote)
        {
            // Do nothing if vote already in vote list
            if (Votes.Contains(vote))
                return;
            if (!Ties)
            {
                Insert(vote);
                return;
            }
            Remove(vote.Candidate);
            OrderVotes();
            if (vote.Value > 0)
                Votes.Add(vote);
        }

        /// <summary>
        /// Inserts a vote by moving all votes of the cast rank or
        /// below down a rank and adding the vote.
        /// </summary>
        /// <param name="vote">The vote to cast.</param>
        public virtual void Insert(Vote vote)
        {
            Remove(vote.Candidate);
            OrderVotes();
            // Move down all votes with Value >= vote.Value.
            if (vote.Value > 0)
            {
                for (int i = 0; i < Votes.Count; i++)
                {
                    if (Votes[i].Value >= vote.Value)
                        Votes[i] = new Vote(Votes[i].Candidate, Votes[i].Value + 1);
                }
                Votes.Add(vote);
            }
        }

        protected virtual void OrderVotes()
        {
            int priorValue = 1;

            Votes.Sort();
            /* One vote, always rank 1 */
            if (Votes.Count >= 1 && Votes[0].Value != 1)
            {
                priorValue = Votes[0].Value;
                Votes[0] = new Vote(Votes[0].Candidate, 1);
            }
            /* Start at second vote and close gaps */
            for (int i = 1; i < Votes.Count; i++)
            {
                int newValue = Votes[i].Value;
                /* If was tied with previous value, set to previous value's current value */
                if (Votes[i].Value == priorValue)
                    Votes[i] = new Vote(Votes[i].Candidate, Votes[i - 1].Value);
                /* Else move up if not strictly sequential */
                else if (Votes[i].Value > Votes[i - 1].Value + 1)
                    Votes[i] = new Vote(Votes[i].Candidate, Votes[i - 1].Value);
                priorValue = newValue;
            }
        }
    }

    /* 
     * An Approval ballot.
     */
    public class ApprovalBallot : RankedBallot
    {

        public ApprovalBallot(Race race)
            : base(race)
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
}
