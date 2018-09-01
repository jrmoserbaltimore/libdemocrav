//
// Copyright (c) Moonset Technology Holdings, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//


using System;
using System.Collections;
using System.Collections.Generic;

namespace MoonsetTechnologies.Voting.VotingRules
{
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

    public class Plurality : IRace
    {
        private Dictionary<Candidate, PluralityResult> results = new Dictionary<Candidate, PluralityResult>();
        private class PluralityResult : IResult
        {
            public Candidate Candidate { get; private set; }
            public uint Votes { get; private set; } = 0;

            public PluralityResult(Candidate candidate)
            {
                Candidate = candidate;
            }

            /* Convert to a PluralityResult, losing information */
            public PluralityResult(IResult result)
            {
                Candidate = result.Candidate;
                Votes = result.Votes;
            }

            public void AddVote()
            {
                Votes++;
            }
        }

        public Plurality(IEnumerable<Candidate> candidates)
        {
            foreach (Candidate c in candidates)
            {
                results[c] = new PluralityResult(c);
            }
        }

        private Plurality(IEnumerable<IResult> results)
        {
            foreach (IResult r in results)
            {
                this.results[r.Candidate] = new PluralityResult(r);
            }
        }

        public void Cast(Ballot votes)
        {
            foreach (Vote v in votes)
            {
                /* First-ranked vote  on a ranked ballot */
                if (v.Value == 1)
                {
                    results[v.Candidate].AddVote();
                }
            }
        }

        public IRace Results
        {
            get
            {
                List<IResult> c = new List<IResult>();
                foreach (IResult r in results.Values)
                {
                    /* If nothing yet or a tie, add */
                    if (c.Count == 0 || c[0].Votes == r.Votes)
                    {
                        c.Add(r);
                    }
                    /* if this result has more votes, make it the only result */
                    else if (r.Votes > c[0].Votes)
                    {
                        c.Clear();
                        c.Add(r);
                    }
                }
                /* Return only the winner */
                return new Plurality(c);
            }
        }

        IEnumerator<IResult> IEnumerable<IResult>.GetEnumerator()
        {
            return results.GetEnumerator();
        }

        IEnumerator<Candidate> IEnumerable<Candidate>.GetEnumerator()
        {
            List<Candidate> c = new List<Candidate>();
            foreach (IResult r in results.Values)
            {
                c.Add(r.Candidate);
            }
            return c.GetEnumerator();
        }

        IEnumerator<Ballot> IEnumerable<Ballot>.GetEnumerator()
        {
            throw new NotImplementedException();
            //List<Ballot> b = new List<Ballot>();
            //return b.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

}