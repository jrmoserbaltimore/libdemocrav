using MoonsetTechnologies.Voting.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonsetTechnologies.Voting.Ballots
{
    // A ranked ballot
    [BallotTypeId("eaf87c88-6352-42d0-a048-250c09da2d89")]
    public class Ballot : IEquatable<Ballot>
    {
        protected readonly HashSet<Vote> votes = new HashSet<Vote>();
        public HashSet<Vote> Votes => votes.ToHashSet();

        public Ballot(IEnumerable<Vote> votes)
        {
            foreach (Vote v in votes)
                this.votes.Add(v);
        }

        public Ballot(Ballot ballot, Vote vote)
            : this(ballot.Votes)
        {
            this.votes.Add(vote);
        }

        /// <inheritdoc/>
        public string Encode()
        {
            string output;
            List<Vote> vs = Votes.ToList();
            vs.Sort();

            // Start with the first candidate
            output = vs[0].Candidate.Id.ToString("D");

            // Encode A>B>C=D>E
            // This encoding supports equal votes.
            for (int i = 1; i < vs.Count; i++)
            {
                if (vs[i].Value == vs[i - 1].Value)
                    output += "=";
                else if (vs[i].Value > vs[i - 1].Value)
                    output += ">";
                output += vs[i].Candidate.Id.ToString("D");
            }

            return output;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            // Combine all the votes
            int h = 0;
            foreach (Vote v in Votes)
                h = HashCode.Combine(h, v.GetHashCode());

            return h;
        }

        public bool Equals(Ballot other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (votes.Except(other.votes).Count() != 0)
                return false;
            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj is Ballot)
                return Equals(obj as Ballot);
            return base.Equals(obj);
        }
    }
}
