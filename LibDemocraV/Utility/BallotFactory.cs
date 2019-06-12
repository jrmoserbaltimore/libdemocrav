using MoonsetTechnologies.Voting.Ballots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoonsetTechnologies.Voting.Utility
{
    /// <summary>
    /// A deduplicating ballot factory.  Members are thread-safe.
    /// </summary>
    public class BallotFactory
    {
        private DeduplicatorHashSet<Vote> Votes { get; set; }
        private DeduplicatorHashSet<Ballot> Ballots { get; set; }
        // FIXME:  this needs to be set on creation of the BallotFactory
        // to something sane, such as back-end storage
        private AbstractPeopleFactory peopleFactory = new ByNamePeopleFactory();

        /// <summary>
        /// Creates a new BallotFactory.
        /// </summary>
        public BallotFactory()
        {
            Ballots = new DeduplicatorHashSet<Ballot>();
            Votes = new DeduplicatorHashSet<Vote>();
        }

        /// <summary>
        /// Creates a Ballot containing the given votes.
        /// </summary>
        /// <param name="votes">The votes to record on the ballot.</param>
        /// <returns>A deduplicated Ballot with all Votes deduplicated.</returns>
        public Ballot CreateBallot(IEnumerable<Vote> votes)
        {
            HashSet<Vote> vout = new HashSet<Vote>();
            Ballot b, bin;

            bin = new Ballot(votes);
            // Try a fast lookup, or else deduplicate everything
            if (!Ballots.TryGetValue(bin, out b))
            {
                // Deduplicate the votes
                foreach (Vote v in votes)
                    vout.Add(CreateVote(v.Candidate, v.Value));
                bin = new Ballot(vout);
                // Adds without triggering a call that might CreateBallot
                b = Ballots.GetOrAdd(bin);
            }
            return b;
        }
        /// <summary>
        /// Condenses an enumerable of Ballots into a BallotSet. Combines duplicate
        /// Ballots and CountedBallots into single CountedBallots.
        /// </summary>
        /// <param name="ballots">The Ballots to return as a set.</param>
        /// <returns>A BallotSet with any duplicate Ballots combined into CountedBallots.</returns>
        public BallotSet CreateBallotSet(IEnumerable<Ballot> ballots)
        {
            // We need to create and count each single, identical ballot,
            // and count the number of such ballots in any CountedBallot we
            // encounter.  To do this, we create uncounted, single ballots
            // and specifically avoid looking up CountedBallot.
            var outBallots = from b in ballots.AsParallel()
                             group b is CountedBallot ? (b as CountedBallot).Count : 1 by CreateBallot(b.Votes) into bCount
                             select Ballots[bCount.Sum() == 1 ? bCount.Key : new CountedBallot(bCount.Key, bCount.Sum())];

            return new BallotSet(outBallots);
        }

        /// <summary>
        /// Produces a single BallotSet from a set of BallotSet objects.
        /// </summary>
        /// <param name="sets">An enumerable set of BallotSet objects.</param>
        /// <returns>A BallotSet created from the total of all Ballots.</returns>
        public BallotSet MergeBallotSets(IEnumerable<BallotSet> sets)
        {
            var q = from s in sets
                    from b in s.Ballots
                    select b;
            return CreateBallotSet(q);
        }
        /// <summary>
        /// Creates a vote object.
        /// </summary>
        /// <param name="candidate">The candidate for whom the vote is cast.</param>
        /// <param name="value">The value of the vote.</param>
        /// <returns></returns>
        public Vote CreateVote(Candidate candidate, decimal value)
        {
            Person c;
            Vote v = null;

            c = peopleFactory.GetCandidate(candidate);
            v = new Vote(c as Candidate, value);
            return Votes[v];
        }
    }
}
