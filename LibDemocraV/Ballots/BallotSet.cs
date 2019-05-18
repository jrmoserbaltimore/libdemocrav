using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MoonsetTechnologies.Voting.Ballots
{
    public class BallotSet : IEnumerable<Ballot>
    {
        private List<Ballot> ballots = new List<Ballot>();
        private HashSet<Vote> votes = new HashSet<Vote>();

        public BallotSet(IEnumerable<Ballot> ballots)
        {
            HashSet<Vote> dedupVotes = new HashSet<Vote>();
            foreach (Ballot b in ballots)
            {
                dedupVotes.Clear();
                Ballot newBallot;
                foreach (Vote v in b.Votes)
                {
                    List<Vote> dedups = votes.Where(x => x.Candidate == v.Candidate && x.Value == v.Value).ToList();
                    Vote u;
                    if (dedups.Count == 0)
                    {
                        // add to deduplication hash set
                        votes.Add(v);
                        u = v;
                    }
                    else if (dedups.Count > 1)
                        throw new InvalidOperationException("Multiple duplicate votes in deduplication array");
                    else
                        u = dedups.Single();
                    dedupVotes.Add(u);
                }
                newBallot = new Ballot(dedupVotes);
                if (b is CountedBallot)
                {
                    CountedBallot cb = b as CountedBallot;
                    newBallot = new CountedBallot(newBallot, cb.Count);
                }
                this.ballots.Add(newBallot);
            }
        }

        public IEnumerator<Ballot> GetEnumerator()
        {
            foreach (Ballot b in ballots)
            {
                CountedBallot cb = b as CountedBallot;
                if (cb is null)
                    yield return b;
                else for (int i = 0; i < cb.Count; i++)
                        yield return cb;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
