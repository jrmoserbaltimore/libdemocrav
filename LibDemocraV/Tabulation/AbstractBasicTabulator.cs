using MoonsetTechnologies.Voting.Ballots;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public abstract class AbstractBasicTabulator : AbstractTabulator
    {
        /// <inheritdoc/>
        public override void TabulateRound()
        {
            Dictionary<Candidate, CandidateState.States> tabulation = GetTabulation();

            if (tabulation.Count() == 0)
                return;

            // B.2.c Elect candidates, or B.3 defeat low candidates
            // We won't have defeats if there were elections in B.2.c,
            // but rule C may provide both winners and losers
            SetStates(tabulation);

            batchEliminator.UpdateTiebreaker(candidateStates);

            // Update tabulation for next round
            ComputeTabulation();
        }

        /// <inheritdoc/>
        public override Dictionary<Candidate, CandidateState> GetFullTabulation()
        {

            Dictionary<Candidate, CandidateState> fullTabulation =
              candidateStates.Where(x => x.Value.State == CandidateState.States.elected
                || x.Value.State == CandidateState.States.hopeful)
                .ToDictionary(x => x.Key, x => new CandidateState { VoteCount = x.Value.VoteCount, State = x.Value.State });
            Dictionary<Candidate, CandidateState.States> tabulation = GetTabulation();

            // Merge the tabulation into the full tabulation
            foreach (Candidate c in tabulation.Keys)
            {
                // FIXME:  Improve exception
                if (!fullTabulation.ContainsKey(c))
                    throw new InvalidOperationException();
                fullTabulation[c].State = tabulation[c];
            }

            return fullTabulation;
        }

        /// <summary>
        /// Compute the next round of tabulation.  Does not move to the next tabulation round.
        /// </summary>
        protected virtual void ComputeTabulation()
        {
            Dictionary<Candidate, CandidateState.States> tabulation;

            // B.1 Test Count complete
            if (Complete)
                return;

            // Perform iteration B.2
            CountBallots();
        }

        // TODO:  Public pairwise graph implementation, for use by TopCycle


        /// <inheritdoc/>
        public override Dictionary<Candidate, CandidateState.States> GetTabulation()
        {
            Dictionary<Candidate, CandidateState> hopefuls =
                candidateStates.Where(x => x.Value.State == CandidateState.States.hopeful)
                .ToDictionary(x => x.Key, x => x.Value);

            Dictionary<Candidate, CandidateState> elected =
                candidateStates.Where(x => x.Value.State == CandidateState.States.elected)
                .ToDictionary(x => x.Key, x => x.Value);

            Dictionary<Candidate, CandidateState.States> result;

            // Fill remaining seats
            if (hopefuls.Count() + elected.Count() <= seats)
                result = hopefuls.ToDictionary(x => x.Key, x => CandidateState.States.elected);
            else
            {
                // Check for elimination
                result = batchEliminator.GetEliminationCandidates(candidateStates)
                    .ToDictionary(x => x, x => CandidateState.States.defeated);
            }
            // No elimination, despite more candidats than seats or everyone elected?  It's broken.
            if (result.Count() == 0 && (hopefuls.Count() + elected.Count() > seats || hopefuls.Count() != 0))
                throw new InvalidOperationException();
            return result;
        }

        protected AbstractBasicTabulator(IEnumerable<Candidate> candidates, IEnumerable<Ballot> ballots,
            IBatchEliminator batchEliminator, int seats = 1)
            : base(candidates, ballots, batchEliminator, seats)
        {

        }
    }
}