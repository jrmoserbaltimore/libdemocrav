//
// Copyright (c) Secure Democratic Election Services, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//


using System;
using System.Collections;
using System.Collections.Generic;

namespace DemocraticElections.Voting.VotingRules
{
    /* XXX:  This should perhaps be broken out to "Eliminate Plurality Loser"
     * which IRV calls in a loop.
     */
    public class InstantRunoffVoting : IRace
    {
        private List<ICandidate> candidates = new List<ICandidate>();
        private List<IBallot> ballots = new List<IBallot>();
        private Dictionary<ICandidate, IResult> results = new Dictionary<ICandidate, IResult>();

        public InstantRunoffVoting(IEnumerable<ICandidate> candidates)
        {
            foreach (ICandidate c in candidates)
            {
                this.candidates.Add(c);
            }
        }

        /* Constructor passed another IRace
         * This constructor performs one round of elimination.
         */
        private InstantRunoffVoting(IRace race)
        : this((IEnumerable<ICandidate>)race)
        {
            foreach (IBallot b in (IEnumerable<IBallot>)race) {
                Cast(b);
            }
            // TODO:  Create a Plurality vote from this to compute loser
            // TODO:  Delete candidate
            // TODO:  Re-filter ballots
        }

        public void Cast(IBallot votes)
        {
            /* Filter the ballot to just the candidates */
            IBallot b = new RankedBallot(votes, candidates);
            ballots.Add(votes);
        }

        public IRace Winners()
        {
            throw new NotImplementedException();
        }


        IEnumerator<IResult> IEnumerable<IResult>.GetEnumerator()
        {
            return results.Values.GetEnumerator();
        }

        IEnumerator<ICandidate> IEnumerable<ICandidate>.GetEnumerator()
        {
            List<ICandidate> c = new List<ICandidate>();
            foreach (IResult r in results.Values)
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