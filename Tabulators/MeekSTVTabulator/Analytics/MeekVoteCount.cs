using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoonsetTechnologies.Voting.Tiebreaking;

namespace MoonsetTechnologies.Voting.Analytics
{
    public class MeekCandidateState : CandidateState
    {
        public decimal KeepFactor { get; set; }

        public MeekCandidateState()
            : base()
        {

        }
    }
    public class MeekVoteCount : AbstractVoteCount
    {
        // number to elect
        private readonly int seats;
        private readonly int precision = 9;

        public MeekVoteCount(int seats, int precision, ITiebreaker tiebreaker)
            : base(tiebreaker, new RunoffBatchEliminator(tiebreaker, seats))
        {
            //this.ballots = ballots.ToList();
            this.seats = seats;
            this.precision = precision;
        }

        // Reference rule B.2.c, returns null if no hopefuls get elected.
        public IEnumerable<Candidate> GetWinners(decimal quota,
            Dictionary<Candidate, decimal> hopefuls)
        {
            List<Candidate> elected = new List<Candidate>();
            foreach (Candidate c in hopefuls.Keys)
            {
                // Elect hopefuls who made it
                if (hopefuls[c] >= quota)
                    elected.Add(c);
            }
            if (elected.Count == 0)
                return null;
            return elected;
        }

        /// <inheritdoc/>
        public override IEnumerable<Candidate> GetEliminationCandidates
            (Dictionary<Candidate, decimal> hopefuls, int elected, decimal surplus)
        {
            List<Candidate> losers;

            // Disable batch elimination until a full set of first
            // differences has been seen.  Typically there are no
            // ties in round one and batch elimination is enabled
            // immediately.  If not, one-by-one elimination avoids
            // missing a tie-breaking first difference as such:
            //
            //   - Y and Z are both batched losers
            //   - W and X are tied
            //   - Z transfers n and m votes to W and X
            //   - Y transfers n and m votes to X and W
            //
            // After eliminating Z, W and X are no longer tied.
            // After eliminating Z and Y, W and X are again tied.
            bool enableBatchElimination = tiebreaker.AllTiesBreakable;

            losers = batchEliminator.GetEliminationCandidates(hopefuls, elected, surplus).ToList();

            // Batch elimination disabled, select from lowest votes
            if (!enableBatchElimination && losers.Count > 1)
            {
                Candidate min = losers.First();
                foreach (Candidate c in losers)
                {
                    if (hopefuls[c] < hopefuls[min])
                        min = c;
                }
                losers.Clear();
                losers.Add(min);
            }

            return losers;
        }

        public override decimal GetVoteCount(Candidate candidate)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<Candidate, decimal> GetVoteCounts()
        {
            throw new NotImplementedException();
        }
    }
}
