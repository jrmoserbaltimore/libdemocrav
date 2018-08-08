//
// Copyright (c) Secure Democratic Election Services, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//


using System;
using System.Collections;
using System.Collections.Generic;
using DemocraticElections.Voting.VotingRules;

namespace DemocraticElections.Voting.Analysis
{
    /* PairwiseCounts
     * Create a ballot sheet of pairwise races between candidates.
     * 
     * If you feed this a Plurality race (non-ranked), it will treat each
     * ballot as if it contains one candidate ranked as #1.
     */
    public class PairwiseCounts : IBallotSheet
    {
        private List<IRace> pairwiseRaces = new List<IRace>();

        public PairwiseCounts(IRace race)
        {
            List<ICandidate> candidates = new List<ICandidate>();
            foreach (ICandidate c in (IEnumerable<ICandidate>)race)
            {
                candidates.Add(c);
            }

            if (candidates.Count == 0)
            {
                // FIXME:  error
            }
            else if (candidates.Count == 1)
            {
                // TODO:  Uncontested, return Uncontested Race
            }
            else {
                /* For each candidate, loop through all remaining candidates
                 * and add a Plurality race for each pair */
                for (int i = 0; i < candidates.Count - 1; i++)
                {
                    for (int j = i + 1; j < candidates.Count; j++)
                    {
                        List<ICandidate> c = new List<ICandidate>
                        {
                            candidates[i],
                            candidates[j]
                        };
                        this.pairwiseRaces.Add(new Plurality(c));
                    }
                }
            }
        }

        public void Cast(IBallot votes)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<IRace> GetEnumerator()
        {
            List<IRace> races = new List<IRace>();
            throw new NotImplementedException();
            foreach (ICandidate c in (IEnumerable<ICandidate>)pairwiseRaces)
            {

            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

}