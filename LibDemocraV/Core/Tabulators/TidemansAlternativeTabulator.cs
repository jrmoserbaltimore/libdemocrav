//
// Copyright (c) Moonset Technologies, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//

using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Tabulators
{
    class TidemansAlternativeTabulator : ITabulator
    {
        public IEnumerable<IBallot> Ballots { get; }
        public bool Complete { get; protected set; }

        protected List<Candidate> candidates;
        public IEnumerable<Candidate> Candidates => candidates;

        /// <inheritdoc/>
        public void TabulateRound()
        {
            throw new NotImplementedException();

            // General algorithm:
            //   Set is SmithSet or SchwartzSet

            //   if Set is One Candidate
            //     Winner is Candidate in Set
            //   else if Set is All Candidates
            //     Eliminate Candidate with Fewest Votes
            //   else
            //     Eliminate Candidates Not In Set

            // Variants eliminating non-Smith and not non-Schwartz candidates:
            //   Schwartz Winner:
            //     When the Schwartz Set is one, elect that candidate.
        }
    }
}
