//
// Copyright (c) Moonset Technology Holdings, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//


using System;
using System.Collections;
using System.Collections.Generic;

namespace MoonsetTechnologies.Voting.VotingRules
{
    /* XXX:  This should perhaps be broken out to "Eliminate Plurality Loser"
     * which IRV calls in a loop.
     */
    public class InstantRunoffVoting : IRace
    {
        private List<Candidate> candidates = new List<Candidate>();
        private List<Ballot> ballots = new List<Ballot>();
        private Dictionary<Candidate, IResult> results = new Dictionary<Candidate, IResult>();

        public InstantRunoffVoting(IEnumerable<Candidate> candidates)
        {
            foreach (Candidate c in candidates)
            {
                this.candidates.Add(c);
            }
        }

        /* Constructor passed another IRace
         * This constructor performs one round of elimination.
         */
        private InstantRunoffVoting(IRace race)
        : this((IEnumerable<Candidate>)race)
        {
            foreach (Ballot b in (IEnumerable<Ballot>)race) {
                Cast(b);
            }
            // TODO:  Create a Plurality vote from this to compute loser
            // TODO:  Delete candidate
            // TODO:  Re-filter ballots
        }

        public void Cast(Ballot votes)
        {
            /* Filter the ballot to just the candidates */
            Ballot b = new RankedBallot(votes, candidates);
            ballots.Add(votes);
        }

        public IRace Results => throw new NotImplementedException();


        IEnumerator<IResult> IEnumerable<IResult>.GetEnumerator()
        {
            return results.Values.GetEnumerator();
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

namespace MoonsetTechnologies.Voting.Analysis
{
    public class EliminatePluralityLoser : IRace
    {
        public EliminatePluralityLoser(IRace votes)
        {
            // TODO:  Identify vote counts.
            // TODO:  drop loser from ballots.
        }

}