﻿//
// Copyright (c) Moonset Technologies, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//

using System;
using System.Collections.Generic;
using System.Text;
using MoonsetTechnologies.Voting.Analytics;

namespace MoonsetTechnologies.Voting.Tabulators
{
    class TidemansAlternativeTabulator : IRankedTabulator
    {
        protected IEnumerable<IRankedBallot> Ballots { get; }
        public bool Complete => candidates.Count == 1;
        public IEnumerable<Candidate> SmithSet => topCycle.SmithSet;
        public IEnumerable<Candidate> SchwartzSet => topCycle.SchwartzSet;

        protected TopCycle topCycle;
        protected List<Candidate> candidates;
        public IEnumerable<Candidate> Candidates => candidates;

        protected virtual IEnumerable<Candidate> CondorcetCheck(TopCycle t) => t.SchwartzSet;
        protected virtual IEnumerable<Candidate> RetainSet(TopCycle t) => t.SmithSet;

        public TidemansAlternativeTabulator(IEnumerable<Candidate> candidates, IEnumerable<IRankedBallot> ballots)
        {
            Ballots = new List<IRankedBallot>(ballots);
            this.candidates = new List<Candidate>(candidates);
            topCycle = new TopCycle(Candidates, Ballots);
        }
        // General algorithm:
        //   if SchwartzSet is One Candidate
        //     Winner is Candidate in SchwartzSet
        //   else
        //     Eliminate Candidates not in SmithSet
        //     Eliminate Candidate with Fewest Votes

        /// <inheritdoc/>
        public virtual void TabulateRound()
        {
            TopCycle t = new TopCycle(Candidates, Ballots);
            List<Candidate> cCheck = new List<Candidate>(CondorcetCheck(t));
            List<Candidate> rSet = new List<Candidate>(RetainSet(t));

            if (cCheck.Count == 1)
                candidates = cCheck;
            else
            {
                // Drop everyone outside the Smith Set
                IVoteCount vc = new CachedVoteCount(rSet, new RankedVoteCount(rSet, Ballots));

                // Get rid of the candidate with the fewest votes
                Candidate c = vc.GetLeastVotedCandidate();
                candidates = new List<Candidate>(rSet);
                candidates.Remove(c);
            }
        }
    }

    class TidemansAlternativeSmithTabulator : TidemansAlternativeTabulator
    {
        // General algorithm:
        //   if SmithSet is One Candidate
        //     Winner is Candidate in SmithSet
        //   else
        //     Eliminate Candidates not in SmithSet
        //     Eliminate Candidate with Fewest Votes

        // Reconfiguration of algorithm
        protected override IEnumerable<Candidate> CondorcetCheck(TopCycle t) => t.SmithSet;
        protected override IEnumerable<Candidate> RetainSet(TopCycle t) => t.SmithSet;

        public TidemansAlternativeSmithTabulator(IEnumerable<Candidate> candidates, IEnumerable<IRankedBallot> ballots)
            : base(candidates, ballots)
        {

        }
    }

    class TidemansAlternativeSchwartzTabulator : TidemansAlternativeTabulator
    {
        // General algorithm:
        //   if SchwartzSet is One Candidate
        //     Winner is Candidate in SchwartzSet
        //   else
        //     Eliminate Candidates not in SchwartzSet
        //     Eliminate Candidate with Fewest Votes

        // Reconfiguration of algorithm
        protected override IEnumerable<Candidate> CondorcetCheck(TopCycle t) => t.SchwartzSet;
        protected override IEnumerable<Candidate> RetainSet(TopCycle t) => t.SchwartzSet;

        public TidemansAlternativeSchwartzTabulator(IEnumerable<Candidate> candidates, IEnumerable<IRankedBallot> ballots)
           : base(candidates, ballots)
        {

        }
    }
}