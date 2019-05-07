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
    public class MeekSTVTabulator : AbstractRankedTabulator
    {
        private readonly MeekVoteCount voteCount;
        // number to elect
        private readonly int seats;


        public MeekSTVTabulator(IEnumerable<Candidate> candidates,
            IEnumerable<IRankedBallot> ballots, int seats, int precision = 9)
        {
            ITiebreaker firstDifference = new FirstDifferenceTiebreaker();
            ITiebreaker tiebreaker = new SeriesTiebreaker(
                new ITiebreaker[] {
                    new SequentialTiebreaker(
                        new ITiebreaker[] {
                          new LastDifferenceTiebreaker(),
                          firstDifference,
                        }.ToList()
                    ),
                    new LastDifferenceTiebreaker(),
                    firstDifference,
                }.ToList()
            );

            voteCount = new MeekVoteCount(candidates, ballots,
                new RunoffBatchEliminator(tiebreaker, seats),  seats, precision);
            this.seats = seats;
            // Do this once just to avoid (Complete == true) before the first count
            voteCount.CountBallots();
        }

        public IEnumerable<Candidate> SchwartzSet => throw new NotImplementedException();

        public IEnumerable<Candidate> SmithSet => throw new NotImplementedException();

        public IEnumerable<Candidate> Candidates => throw new NotImplementedException();

        // It's okay to use CheckComplete() here because it will only
        // change state when there are no more rounds to tabulate.
        // This only occurs in a special case where the election has
        // no more candidates in total than seats and CheckComplete()
        // is called before TabulateRound();
        /// <inheritdoc/>
        public bool Complete => voteCount.GetTabulation().Count() == 0;

        public void TabulateRound()
        {
            Dictionary<Candidate, CandidateState.States> tabulation;

            // B.1 Test Count complete
            if (Complete)
                return;

            // Perform iteration B.2
            voteCount.CountBallots();

            // Elect or defeat
            tabulation = voteCount.GetTabulation();

            // B.2.c Elect candidates, or B.3 defeat low candidates
            // We won't have defeats if there were elections in B.2.c,
            // but rule C may provide both winners and losers
            voteCount.ApplyTabulation();

            // B.4:  Next call enters at B.1
        }
    }
}