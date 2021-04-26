using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Utility;
using MoonsetTechnologies.Voting.Analytics;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public class RankedPairsTabulator : AbstractPairwiseTabulator
    {
        /// <inheritdoc/>
        public RankedPairsTabulator(TabulationMediator mediator,
            AbstractTiebreakerFactory tiebreakerFactory,
            IEnumerable<ITabulatorSetting> tabulatorSettings)
            : base(mediator, tiebreakerFactory, tabulatorSettings)
        {

        }

        protected override void CountBallot(CountedBallot ballot)
        {
            // Only counts hopeful and elected candidates
            Dictionary<Candidate, CandidateState> candidates
                = candidateStates
                   .Where(x => new[] { CandidateState.States.hopeful, CandidateState.States.elected, CandidateState.States.defeated }
                     .Contains(x.Value.State))
                   .ToDictionary(x => x.Key, x => x.Value);
        }

        private struct Pair
        {
            public Pair(Candidate w, Candidate l)
            {
                winner = w;
                loser = l;
            }
            public Candidate winner { get; }
            public Candidate loser { get; }
        }

        protected override IEnumerable<Candidate> GetEliminationCandidates()
        {
            HashSet<Candidate> ec = new HashSet<Candidate>();

            Dictionary<Pair, decimal> WinMargins = new Dictionary<Pair, decimal>();
            foreach (Candidate c in candidateStates.Where(x => x.Value.State == CandidateState.States.hopeful).Select(x => x.Key))
            {
                foreach (Candidate d in pairwiseGraph.Wins(c))
                {
                    var (v1, v2) = pairwiseGraph.GetVoteCount(c, d);
                    WinMargins.Add(new Pair(c, d),  v1 - v2);
                }
                foreach (Candidate d in pairwiseGraph.Ties(c))
                {
                    // Only add ties that don't yet exist
                    var pair = new[] { c, d };
                    if (WinMargins.Keys.Where(x => pair.Contains(x.loser) && pair.Contains(x.winner)).Count() == 0)
                        WinMargins.Add(new Pair(c, d), 0);
                }
            }

            // Sort from largest win margin
            PairwiseGraph rp = null;
            foreach (KeyValuePair<Pair, decimal> p in WinMargins.OrderBy(x => x.Value))
            {
                // check if p.winner can be reached from p.loser
                // and add to the graph if not
                if (!(rp is null))
                {
                    if (rp.CanReach(p.Key.winner, p.Key.loser))
                    {
                        rp = new PairwiseGraph(rp, new PairwiseGraph(pairwiseGraph.GetAsBallots(p.Key.winner, p.Key.loser)));
                    }
                }
                else
                    rp = new PairwiseGraph(pairwiseGraph.GetAsBallots(p.Key.winner, p.Key.loser));
            }

            // Locate graph root
            TopCycle tc = new TopCycle(rp, TopCycle.TopCycleSets.smith);
            // Eliminate all candidates except the root candidate
            ec.UnionWith(candidateStates
                .Where(x => x.Value.State == CandidateState.States.hopeful)
                .Select(x => x.Key)
                .Except(tc.GetTopCycle(new Candidate[] { })));

            return ec;
        }
    }
}