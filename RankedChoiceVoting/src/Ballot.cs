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
    }
}
