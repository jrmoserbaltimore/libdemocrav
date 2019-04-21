//
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
        public void TabulateRound()
        {
            TopCycle t = new TopCycle(Candidates, Ballots);
            List<Candidate> schwartzSet = new List<Candidate>(t.SchwartzSet);
            List<Candidate> smithSet = new List<Candidate>(t.SmithSet);

            if (schwartzSet.Count == 1)
                candidates = schwartzSet;
            else
            {
                // Drop everyone outside the Smith Set
                IVoteCount vc = new CachedVoteCount(smithSet, new RankedVoteCount(smithSet, Ballots));

                // Get rid of the candidate with the fewest votes
                Candidate c = vc.GetLeastVotedCandidate();
                candidates = new List<Candidate>(smithSet);
                candidates.Remove(c);
            }
        }
    }


    class TidemansAlternativeSmithTabulator : TidemansAlternativeTabulator
    {
        IEnumerable<IRankedBallot> Ballots { get; }
        public bool Complete => candidates.Count == 1;

        protected List<Candidate> candidates;
        public TidemansAlternativeSmithTabulator(IEnumerable<Candidate> candidates, IEnumerable<IRankedBallot> ballots)
            : base(candidates, ballots)
        {

        }

        public IEnumerable<Candidate> Candidates => candidates;

        // General algorithm:
        //   if SmithSet is One Candidate
        //     Winner is Candidate in SmithSet
        //   else
        //     Eliminate Candidates not in SmithSet
        //     Eliminate Candidate with Fewest Votes

        /// <inheritdoc/>
        public void TabulateRound()
        {
            TopCycle t = new TopCycle(Candidates, Ballots);
            List<Candidate> smithSet = new List<Candidate>(t.SmithSet);

            if (smithSet.Count == 1)
                candidates = smithSet;
            else
            {
                // Drop everyone outside the Smith Set
                IVoteCount vc = new CachedVoteCount(smithSet, new RankedVoteCount(smithSet, Ballots));

                // Get rid of the candidate with the fewest votes
                Candidate c = vc.GetLeastVotedCandidate();
                candidates = new List<Candidate>(smithSet);
                candidates.Remove(c);
            }
        }
    }

    class TidemansAlternativeSchwartzTabulator : TidemansAlternativeTabulator
    {
        IEnumerable<IRankedBallot> Ballots { get; }
        public bool Complete => candidates.Count == 1;

        protected List<Candidate> candidates;
        public IEnumerable<Candidate> Candidates => candidates;

        public TidemansAlternativeSchwartzTabulator(IEnumerable<Candidate> candidates, IEnumerable<IRankedBallot> ballots)
           : base(candidates, ballots)
        {

        }
        // General algorithm:
        //   if SchwartzSet is One Candidate
        //     Winner is Candidate in SchwartzSet
        //   else
        //     Eliminate Candidates not in SchwartzSet
        //     Eliminate Candidate with Fewest Votes

        /// <inheritdoc/>
        public void TabulateRound()
        {
            TopCycle t = new TopCycle(Candidates, Ballots);
            List<Candidate> schwartzSet = new List<Candidate>(t.SchwartzSet);
            List<Candidate> smithSet = new List<Candidate>(t.SmithSet);

            if (schwartzSet.Count == 1)
                candidates = schwartzSet;
            else
            {
                // Drop everyone outside the Smith Set
                IVoteCount vc = new CachedVoteCount(schwartzSet, new RankedVoteCount(smithSet, Ballots));

                // Get rid of the candidate with the fewest votes
                Candidate c = vc.GetLeastVotedCandidate();
                candidates = new List<Candidate>(schwartzSet);
                candidates.Remove(c);
            }
        }
    }
}
