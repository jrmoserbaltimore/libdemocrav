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

        public void TabulateRound()
        {
            throw new NotImplementedException();
        }
    }
}
