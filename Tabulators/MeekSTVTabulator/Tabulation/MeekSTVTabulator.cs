// Meek STV proportional election implementation.
//
// Special thanks goes to the work of Brian Wichmann and David Hill:
//
//   Tie Breaking in STV, Voting Matters Issue 19
//     http://www.votingmatters.org.uk/ISSUE19/I19P1.PDF
//   Implementing STV by Meek's Method, Voting Matters Issue 22
//     http://www.votingmatters.org.uk/ISSUE22/I22P2.pdf
//   Validation of Implementation of the Meek Algorithm for STV
//     http://www.votingmatters.org.uk/RES/MKVAL.pdf
//   Single Transferable Vote by Meek's Method
//     http://www.dia.govt.nz/diawebsite.NSF/Files/meekm/%24file/meekm.pdf
//   The Meek STV reference rule
//     https://prfound.org/resources/reference/reference-meek-rule/
//
// The decimal type avoids rounding error for up to 27 digits.
// For 9 billion votes, precision up to 17 decimal places is
// possible. Beyond ten is not generally necessary: OpaVote
// uses 6, and the reference algorithm uses 9.

using System;
using System.Collections.Generic;
using System.Linq;
using MoonsetTechnologies.Voting.Tiebreaking;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public class MeekSTVTabulator : RankedTabulator
    {
        // number to elect
        private readonly int seats;

        public MeekSTVTabulator(AbstractRankedVoteCount voteCount)
            : base(voteCount)
        {

        }

        // AbstractTabulator's implementation follows:
        //   B.1 - if (Complete) return
        //   B.2 - voteCount.CountBallots()
        //   B.2.c etc. - voteCount.ApplyTabulation()
        //   B.4 - Re-enter TabulateRound() at B.1
    }
}