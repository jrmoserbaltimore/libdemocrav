using MoonsetTechnologies.Voting.Ballots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonsetTechnologies.Voting.Analytics
{
    /// <summary>
    /// Analyzes alternative outcomes.
    /// </summary>
    public abstract class AbstractAlternateOutcomeAnalysis
    {
        protected BallotSet Ballots { get; set; }
        protected IEnumerable<Candidate> Withdrawn { get; set; }
        protected int Seats { get; set; }

        public AbstractAlternateOutcomeAnalysis(BallotSet ballots, IEnumerable<Candidate> withdrawn, int seats = 1)
        {
            Ballots = ballots;
            Withdrawn = withdrawn.ToHashSet();
            Seats = seats;
        }
    }
}
