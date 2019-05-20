using System;
using System.Collections.Generic;
using System.Linq;
using MoonsetTechnologies.Voting.Ballots;

namespace MoonsetTechnologies.Voting.Utility
{
    /// <summary>
    /// A factory to create ballots, votes, and people.  Deduplicates immutable objects.
    /// </summary>
    public class BallotFactory : AbstractBallotFactory
    {
        protected Dictionary<int, WeakReference<Vote>> Votes { get; set; } = new Dictionary<int, WeakReference<Vote>>();
        protected Dictionary<int, WeakReference<Ballot>> Ballots { get; set; } = new Dictionary<int, WeakReference<Ballot>>();

        private AbstractPeopleFactory peopleFactory = new ByNamePeopleFactory();

        /// <inheritdoc/>
        public override Ballot CreateBallot(IEnumerable<Vote> votes)
        {
            HashSet<Vote> vout = new HashSet<Vote>();
            Ballot b, bin;

            bin = new Ballot(votes);

            if (Ballots.ContainsKey(bin.GetHashCode()))
            {
                if (Ballots[bin.GetHashCode()].TryGetTarget(out b))
                    return b;
            }

            // Deduplicate the votes
            foreach (Vote v in votes)
                vout.Add(CreateVote(v.Candidate, v.Value));

            b = new Ballot(vout);
            Ballots[b.GetHashCode()] = new WeakReference<Ballot>(b);
            
            return b;
        }

        /// <inheritdoc/>
        public override BallotSet CreateBallotSet(IEnumerable<Ballot> ballots)
        {
            HashSet<Ballot> outBallots = new HashSet<Ballot>();
            Dictionary<Ballot, int> ballotCounts = new Dictionary<Ballot, int>();

            foreach (Ballot b in ballots)
            {
                Ballot oneBallot = CreateBallot(b.Votes);
                int count = (b is CountedBallot) ? (b as CountedBallot).Count : 1;

                if (!ballotCounts.ContainsKey(oneBallot))
                    ballotCounts[oneBallot] = 0;
                ballotCounts[oneBallot] += count;
            }

            foreach (Ballot b in ballotCounts.Keys)
            {
                Ballot newBallot;
                if (ballotCounts[b] == 1)
                    newBallot = b;
                else
                    newBallot = new CountedBallot(b, ballotCounts[b]);
                Ballots[newBallot.GetHashCode()] = new WeakReference<Ballot>(newBallot);
                outBallots.Add(newBallot);
            }

            return new BallotSet(outBallots);
        }
        /// <inheritdoc/>
        public override BallotSet MergeBallotSets(IEnumerable<BallotSet> sets)
        {
            List<CountedBallot> ballots = new List<CountedBallot>();
            foreach (BallotSet b in sets)
                ballots.AddRange(b.Ballots);
            return CreateBallotSet(ballots);
        }

        /// <inheritdoc/>
        public override Vote CreateVote(Candidate candidate, decimal value)
        {
            Person c;
            Vote u, v = null;

            c = peopleFactory.GetCandidate(candidate);
            u = new Vote(c as Candidate, value);
            // New vote if we don't have an equivalent one
            if (Votes.ContainsKey(u.GetHashCode()))
            {
                if (Votes[u.GetHashCode()].TryGetTarget(out v))
                    return v;
            }

            // Create a vote with the deduplicated candidate and track it
            v = new Vote(c as Candidate, value);
            Votes[v.GetHashCode()] = new WeakReference<Vote>(v);

            return v;
        }
    }
}
