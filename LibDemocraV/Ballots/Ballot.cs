using MoonsetTechnologies.Voting.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Ballots
{
    // A ranked ballot
    [BallotTypeId("eaf87c88-6352-42d0-a048-250c09da2d89")]
    public class Ballot
    {
        protected List<Vote> votes = new List<Vote>();
        public IEnumerable<Vote> Votes => votes;

        public Ballot(IEnumerable<Vote> votes)
        {
            foreach (Vote v in votes)
                this.votes.Add(new Vote(v.Candidate, v.Value));
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
            List<Vote> vs = new List<Vote>();
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
    }
}
