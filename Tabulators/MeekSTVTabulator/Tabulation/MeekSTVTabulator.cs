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
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Tiebreaking;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public class MeekSTVTabulator : RankedTabulator
    {
        private decimal quota = 0.0m;
        private decimal surplus = 0.0m;
        private readonly int precision = 9;
        // Round up to precision
        private decimal RoundUp(decimal d)
        {
            // The reference rule says to round UP, so we need
            // to Round(weight + 10^(-precision) / 2).
            decimal r = 0.5m * Convert.ToDecimal(Math.Pow(10.0D,
                Convert.ToDouble(0 - precision)));
            return decimal.Round(d + r, precision);
        }

        public MeekSTVTabulator(IEnumerable<Candidate> candidates, IEnumerable<Ballot> ballots,
            IBatchEliminator batchEliminator, int seats = 1, int precision = 9)
            : base(candidates, ballots, batchEliminator, seats)
        {
            this.precision = precision;
        }


        // AbstractTabulator's implementation follows:
        //   B.1 - if (Complete) return
        //   B.2 - voteCount.CountBallots()
        //   B.2.c etc. - voteCount.ApplyTabulation()
        //   B.4 - Re-enter TabulateRound() at B.1

        /// <inheritdoc/>
        protected override void SetStates(Dictionary<Candidate, CandidateState.States> candidates)
        {
            foreach (Candidate c in candidates.Keys)
            {
                if (!candidateStates.ContainsKey(c))
                    throw new ArgumentOutOfRangeException();
                MeekCandidateState cs = candidateStates[c] as MeekCandidateState;
                if (cs is null)
                    throw new InvalidOperationException("candidateState contains objects that aren't of type MeekCandidateState");
                cs.State = candidates[c];
                // Set KeepFactor to 0 for losers as per B.3
                if (cs.State == CandidateState.States.defeated)
                    cs.KeepFactor = 0.0m;
            }
        }
        /// <inheritdoc/>
        protected override void InitializeCandidateStates(IEnumerable<Candidate> candidates)
        {
            // As per reference rule A
            foreach (Candidate c in candidates)
            {
                MeekCandidateState cs = new MeekCandidateState
                {
                    KeepFactor = 1,
                    State = CandidateState.States.hopeful
                };
                candidateStates[c] = cs;
            }
        }
    }
}