using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MoonsetTechnologies.Voting.Ballots
{
    public class BallotSet : IEnumerable<CountedBallot>
    {
        private List<Ballot> ballots = new List<Ballot>();
        public IEnumerable<CountedBallot> Ballots => (this as IEnumerable<CountedBallot>).ToList();

        /// <summary>
        /// Total count of ballots.
        /// </summary>
        /// <returns>A total count of ballots.</returns>
        public int TotalCount() => (this as IEnumerable<CountedBallot>).Sum(x => x.Count);

        public BallotSet(IEnumerable<Ballot> ballots)
        {
            this.ballots.AddRange(ballots);
        }

        IEnumerator<CountedBallot> IEnumerable<CountedBallot>.GetEnumerator()
        {
            foreach (Ballot b in ballots)
            {
                CountedBallot cb = b as CountedBallot;
                if (cb is null)
                {
                    yield return new CountedBallot(b, 1);
                }
                else yield return cb;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
