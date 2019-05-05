using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting
{
   
    [BallotTypeId("eaf87c88-6352-42d0-a048-250c09da2d89")]
    public class RankedBallot : IRankedBallot
    {
        protected List<IRankedVote> votes = new List<IRankedVote>();
        public IEnumerable<IRankedVote> Votes => votes;
        IEnumerable<IVote> IBallot.Votes => Votes;

        public RankedBallot(IEnumerable<IRankedVote> votes)
        {
            foreach (RankedVote v in votes)
                this.votes.Add(new RankedVote(v.Candidate, v.Value));
        }

        public RankedBallot(IRankedBallot ballot, IRankedVote vote)
            : this(ballot.Votes)
        {
            this.votes.Add(vote);
        }
 
        /// <inheritdoc/>
        public string Encode()
        {
            string output = "";
            List<IRankedVote> vs = new List<IRankedVote>();
            vs.Sort();

            // Start with the first candidate
            output = vs[0].Candidate.Id.ToString("D");

            // Encode A>B>C=D>E
            // This encoding supports equal votes.
            for(int i=1; i < vs.Count; i++)
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
