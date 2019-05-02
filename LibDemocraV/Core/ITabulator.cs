//
// Copyright (c) Moonset Technologies, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//

using System;
using System.Collections;
using System.Collections.Generic;

namespace MoonsetTechnologies.Voting.Tabulators
{
    /// <summary>
    /// A voting rule to tabulate votes.
    /// </summary>
    public interface ITabulator
    {
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
}