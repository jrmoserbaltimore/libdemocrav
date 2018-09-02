//
// Copyright (c) Moonset Technologies, LLC. All rights reserved.  
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
            if (Votes.Contains<Vote>(vote))
                return;
            if (!Ties)
            {
                Insert(vote);
                return;
            }
            Remove(vote.Candidate);
            CloseRankingGaps();
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
            CloseRankingGaps();
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

        /// <summary>
        /// Ensures rankings start at 1 and there are no skipped ranks.
        /// </summary>
        protected virtual void CloseRankingGaps()
        {
            Dictionary<int, List<int>> voteMap = new Dictionary<int, List<int>>();

            // Value x is at Votes indexes given by voteMap[x]
            for (int i = 0; i < Votes.Count; i++)
            {
                int rank = Votes[i].Value;
                if (!voteMap.ContainsKey(rank))
                    voteMap[rank] = new List<int>();
                voteMap[rank].Add(i);
            }

            int[] ranks = voteMap.Keys.ToArray();
            Array.Sort(ranks);

            // Close all gaps in rankings
            for (int i = 0; i < ranks.Length; i++)
            {
                for (int j = 0; j < voteMap[i].Count; j++)
                {
                    int n = voteMap[i][j];
                    if (Votes[n].Value != i + 1)
                        Votes[n] = new Vote(Votes[n].Candidate, i + 1);
                }
            }
        }
    }
}
