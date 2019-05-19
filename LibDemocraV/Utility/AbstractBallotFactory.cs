using MoonsetTechnologies.Voting.Ballots;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public abstract class AbstractBallotFactory
    {
        public AbstractBallotFactory()
        {

        }

        /// <summary>
        /// Creates a ballot containing the given votes.
        /// </summary>
        /// <param name="votes">The votes to record on the ballot.</param>
        /// <returns></returns>
        public abstract Ballot CreateBallot(IEnumerable<Vote> votes);
        /// <summary>
        /// Condenses an enumerable of ballots into a BallotSet.
        /// </summary>
        /// <param name="ballots">The ballots to return as a set.</param>
        /// <returns></returns>
        public abstract BallotSet CreateBallotSet(IEnumerable<Ballot> ballots);
        /// <summary>
        /// Creates a vote object.
        /// </summary>
        /// <param name="candidate">The candidate for whom the vote is cast.</param>
        /// <param name="value">The value of the vote.</param>
        /// <returns></returns>
        public abstract Vote CreateVote(Candidate candidate, decimal value);

        public abstract Candidate GetCandidate(Candidate candidate);
    }
}
