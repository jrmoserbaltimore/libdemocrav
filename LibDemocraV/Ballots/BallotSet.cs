using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Ballots
{
    public class BallotSet : IEnumerable<Ballot>
    {
        IEnumerable<Ballot> ballots;

        public BallotSet(IEnumerable<Ballot> ballots)
        {
            this.ballots = ballots;
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
