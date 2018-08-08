﻿//
// Copyright (c) Secure Democratic Election Services, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//


using System;
using System.Collections;
using System.Collections.Generic;

namespace DemocraticElections.Voting.VotingRules
{
    public class Plurality : IRace
    {
        private List<PluralityResult> results = new List<PluralityResult>();
        private class PluralityResult : IResult
        {
            public ICandidate Candidate { get; private set; }
            public uint Votes { get; private set; } = 0;

            public PluralityResult(ICandidate candidate)
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

        public Plurality(IEnumerable<ICandidate> candidates)
        {
            foreach (ICandidate c in candidates)
            {
                results.Add(new PluralityResult(c));
            }
        }

        private Plurality(IEnumerable<IResult> results)
        {
            foreach (IResult r in results)
            {
                this.results.Add(new PluralityResult(r));
            }
        }

        public void Cast(IBallot votes)
        {
            throw new NotImplementedException();
        }

        public IRace Winners()
        {
            List<IResult> c = new List<IResult>();
            foreach (IResult r in results)
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

        IEnumerator<IResult> IEnumerable<IResult>.GetEnumerator()
        {
            return results.GetEnumerator();
        }

        IEnumerator<ICandidate> IEnumerable<ICandidate>.GetEnumerator()
        {
            List<ICandidate> c = new List<ICandidate>();
            foreach (IResult r in results)
            {
                c.Add(r.Candidate);
            }
            return c.GetEnumerator();
        }

        IEnumerator<IBallot> IEnumerable<IBallot>.GetEnumerator()
        {
            throw new NotImplementedException();
            //List<IBallot> b = new List<IBallot>();
            //return b.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

}