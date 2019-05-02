using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting
{
    public interface IRankedBallot : IBallot
    {
        new IEnumerable<IRankedVote> Votes { get; }
    }

    public interface IRankedVote : IVote
    {
        /// <summary>
        /// The ordinal value, with lower indicating more preferred.
        /// </summary>
        int Value { get; }
        /// <summary>
        /// Check if this vote is ranked higher in preference to (vote).
        /// </summary>
        /// <param name="vote">The vote to compare.</param>
        /// <returns>true if this vote is ranked higher in preference.  false on tie or loss.</returns>
        bool Beats(IRankedVote vote);
    }

    /// <summary>
    /// A Vote object.  Allows placing a value on a vote.
    /// Immutable.
    /// </summary>
    public class RankedVote : IRankedVote, IEquatable<RankedVote>
    {
        public Candidate Candidate { get; }
        /// <inheritdoc/>
        public int Value { get; }

        public RankedVote(Candidate candidate, int value)
        {
            Candidate = candidate;
            Value = value;
        }

        /// <inheritdoc/>
        public virtual bool Beats(IRankedVote vote) => Value < vote.Value;

        /// <inheritdoc/>
        public virtual bool Equals(RankedVote v)
        {
            if (v is null)
                return false;
            else if (ReferenceEquals(this, v))
                return true;
            return Candidate.Equals(v.Candidate) && Value.Equals(v.Value);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as RankedVote);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Candidate, Value);
    }

    
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

            // Sort the votes
            foreach (IRankedVote v in votes)
            {
                for (int i=0; i < vs.Count; i++)
                {
                    if (v.Value > vs[i].Value)
                    {
                        vs.Insert(i, v);
                        break;
                    }
                }
                // Append if not inserted
                if (!vs.Contains(v))
                    vs.Add(v);
            }

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
