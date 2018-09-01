//
// Copyright (c) Moonset Technology Holdings, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//


using System;
using System.Collections;
using System.Collections.Generic;
using MoonsetTechnologies.Voting.VotingRules;

namespace MoonsetTechnologies.Voting.Analysis
{
    /* PairwiseCounts
     * Create a ballot sheet of pairwise races between candidates.
     * 
     * If you feed this a Plurality race (non-ranked), it will treat each
     * ballot as if it contains one candidate ranked as #1.
     */
    public class PairwiseCounts : IBallotSheet
    {
        private Dictionary<Candidate, Dictionary<Candidate, IRace>> pairwiseHash = new Dictionary<Candidate, Dictionary<Candidate, IRace>>();
        private List<IRace> pairwiseRaces = new List<IRace>();

        public PairwiseCounts(IRace race)
        {
            List<Candidate> candidates = new List<Candidate>();
            foreach (Candidate c in (IEnumerable<Candidate>)race)
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
            /* For each candidate, loop through all remaining candidates
             * and add a Plurality race for each pair */
            for (int i = 0; i < candidates.Count - 1; i++)
            {
                for (int j = i + 1; j < candidates.Count; j++)
                {
                    IRace r;
                    List<Candidate> c = new List<Candidate>
                    {
                        candidates[i],
                        candidates[j]
                    };
                    r = new Plurality(c);
                    this.pairwiseRaces.Add(r);
                    pairwiseHash[c[0]][c[1]] = pairwiseHash[c[1]][c[0]] = r;
                }
            }
            /* For each ballot, cast votes to the Plurality race for
             * each pair of candidates */
            foreach (Ballot b in (IEnumerable<Ballot>)race)
            {
                List<Candidate> unranked = new List<Candidate>(candidates);
                Dictionary<Candidate,Vote> rankedHash = new Dictionary<Candidate,Vote>();
                List<Candidate> ranked;
                /* Move each candidate from unranked to ranked */
                foreach (Vote v in b)
                {
                    unranked.Remove(v.Candidate);
                    rankedHash[v.Candidate] = v;
                }
                ranked = rankedHash.Keys;
                /* Cast a bunch of plurality votes */
                for (int i = 0; i < ranked.Count; i++)
                {
                    /* All ranked candidates. Skips on last ranked. */
                    for (int j = i+1; j < ranked.Count; j++)
                    {
                        IRace r = pairwiseHash[ranked[i].Candidate][ranked[j].Candidate];
                        Candidate c;
                        if (rankedHash[ranked[i]].Value > rankedHash[ranked[j].Value])
                        {
                            c = ranked[i];
                        }
                        else if (rankedHash[ranked[j]].Value > rankedHash[ranked[i].Value])
                        {
                            c = ranked[j];
                        }
                        else
                        {
                            /* Skip ties */
                            continue;
                        }
                        /* Cast a plurality vote for the winner */
                        Vote v = new Vote(c, 1);
                        Ballot b = new RankedBallot(v);
                        r.Cast(b);
                    }
                    /* Cast against all unranked */
                    for (int j = 0; j < unranked.Count; j++)
                    {
                        IRace r = pairwiseHash[ranked[i].Candidate][unranked[j].Candidate];
                        Vote v = new Vote(ranked[i].Candidate, 1);
                        Ballot b = new RankedBallot(v);
                        r.Cast(b);
                    }
                }
            }
        }

        public void Cast(Ballot votes)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<IRace> GetEnumerator()
        {
            List<IRace> races = new List<IRace>();
            throw new NotImplementedException();
            foreach (Candidate c in (IEnumerable<Candidate>)pairwiseRaces)
            {

            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

}
