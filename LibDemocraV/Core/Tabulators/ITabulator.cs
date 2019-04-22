//
// Copyright (c) Moonset Technologies, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//

using System;
using System.Collections;
using System.Collections.Generic;
using MoonsetTechnologies.Voting.Analytics;

namespace MoonsetTechnologies.Voting.Tabulators 
{
    /// <summary>
    /// A voting rule to tabulate votes.
    /// </summary>
    public interface ITabulator
    {
        /// <summary>
        /// The ballots as they stand in the current round.
        /// </summary>
        //IEnumerable<IBallot> Ballots { get; }

        /// <summary>
        /// The Candidates still remaining in the current round.
        /// </summary>
        IEnumerable<Candidate> Candidates { get; }

        /// <summary>
        /// True if tabulation is complete; else false.
        /// </summary>
        bool Complete { get; } 
        /// <summary>
        /// Iterate through the next round of tabulation
        /// </summary>
        void TabulateRound();
    }

    public interface IRankedTabulator : ITabulator
    {
        IEnumerable<Candidate> SchwartzSet { get; }
        IEnumerable<Candidate> SmithSet { get; }
    }
}