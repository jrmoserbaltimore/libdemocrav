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
        protected Dictionary<int, WeakReference<Person>> People { get; set; } = new Dictionary<int, WeakReference<Person>>();

        protected Dictionary<string, int> Candidates { get; set; } = new Dictionary<string, int>();

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
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override Vote CreateVote(Candidate candidate, decimal value)
        {
            Person c;
            Vote u, v = null;

            c = GetCandidate(candidate);
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

        // FIXME:  Port this to a PeopleFactory, which may provide loading from
        // database, data file, and so forth
        public override Candidate GetCandidate(Candidate candidate)
        {
            Person c = null;
            // Go through the trouble of deduplicating the candidate
            if (!(People.ContainsKey(candidate.GetHashCode())
                  && People[candidate.GetHashCode()].TryGetTarget(out c)))
            {
                if (!(Candidates.ContainsKey(candidate.Name)
                    && People[Candidates[candidate.Name]].TryGetTarget(out c)))
                {
                    c = candidate;
                }
                Candidates[c.Name] = c.GetHashCode();

                People[c.GetHashCode()] = new WeakReference<Person>(c);
            }
            return c as Candidate;
        }
    }
}
