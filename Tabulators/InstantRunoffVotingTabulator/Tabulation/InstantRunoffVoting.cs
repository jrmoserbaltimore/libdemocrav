using System;
using System.Collections.Generic;
using System.Text;
using MoonsetTechnologies.Voting.Analytics;

namespace MoonsetTechnologies.Voting.Tabulation
{
    class InstantRunoffVoting : IRankedTabulator
    {
        protected IEnumerable<IRankedBallot> Ballots { get; }
        public bool Complete => candidates.Count == 1;
        public IEnumerable<Candidate> SmithSet => topCycle.SmithSet;
        public IEnumerable<Candidate> SchwartzSet => topCycle.SchwartzSet;

        protected TopCycle topCycle;
        protected List<Candidate> candidates;
        public IEnumerable<Candidate> Candidates => candidates;
        public InstantRunoffVoting(IEnumerable<Candidate> candidates, IEnumerable<IRankedBallot> ballots)
        {
            Ballots = new List<IRankedBallot>(ballots);
            this.candidates = new List<Candidate>(candidates);
            topCycle = new TopCycle(Candidates, Ballots);
        }

        // General algorithm:
        //   if one Candidate has >50% of Votes
        //     Winner is Candidate with >50% of Votes
        //   else
        //     Eliminate Candidates who, combined, have fewer votes than the candidate with the next-fewest votes

        /// <inheritdoc/>
        public void TabulateRound()
        {
            throw new NotImplementedException();
        }
    }
}
