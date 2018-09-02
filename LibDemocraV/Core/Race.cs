//
// Copyright (c) Moonset Technologies, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//

using System;
using System.Collections;
using System.Collections.Generic;

namespace MoonsetTechnologies.Voting
{
    /// <summary>
    /// Results of a Race.
    /// </summary>
    public abstract class Result
    {
        /// <summary>
        /// The set of winners in this Race.
        /// </summary>
        public IReadOnlyCollection<Candidate> Winners => WinnerList.AsReadOnly();

        /// <summary>
        /// The internal list provided by Winners.
        /// </summary>
        protected List<Candidate> WinnerList { get; } = new List<Candidate>();

        /// <summary>
        /// Returns the ballots which ultimately went to this Candidate.
        /// </summary>
        /// <param name="candidate">The Candidate to whom the ballots went.</param>
        /// <returns></returns>
        public abstract IReadOnlyCollection<ReadOnlyBallot> GetBallots(Candidate candidate);
    }

    /// <summary>
    /// A class representing a race in an election.
    /// </summary>
    public abstract class Race : IEquatable<Race>
    {
        /// <summary>
        /// Unique identifier for this Race.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// The ballots cast in this Race.
        /// </summary>
        public IReadOnlyCollection<ReadOnlyBallot> Ballots => BallotList.AsReadOnly();

        /// <summary>
        /// The ballots cast in this race, available to derived types.
        /// </summary>
        protected List<ReadOnlyBallot> BallotList { get; } = new List<ReadOnlyBallot>();

        /// <summary>
        /// Candidates in this Race.
        /// </summary>
        public IReadOnlyCollection<Candidate> Candidates { get; }

        /// <summary>
        /// Cast a ballot in this Race.
        /// </summary>
        /// <param name="ballot"></param>
        public abstract void Cast(ReadOnlyBallot ballot);

        /// <summary>
        /// Computes the results and returns a collection thereof.
        /// </summary>
        /// <returns></returns>
        public abstract Result GetResults();

        /// <summary>
        /// Determines whether the current Race object refers to the
        /// same race as the other Race object.
        /// </summary>
        /// <param name="other">The object to compare with the current
        /// object.</param>
        /// <returns></returns>
        public virtual bool Equals(Race other) => Id.Equals(other.Id);
    }
}
