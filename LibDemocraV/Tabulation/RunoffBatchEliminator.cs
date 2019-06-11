using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoonsetTechnologies.Voting.Tiebreaking;
using MoonsetTechnologies.Voting.Analytics;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public class RunoffBatchEliminator : AbstractBatchEliminator
    {
        public RunoffBatchEliminator(RankedTabulationAnalytics analytics,
            int seats = 1)
            : base(analytics, seats)
        {

        }

        /// <inheritdoc/>
        protected IEnumerable<Candidate> GetEliminationCandidates(Dictionary<Candidate, CandidateState> candidateStates, decimal surplus, bool batchElimination = true)
        {
            Dictionary<Candidate, decimal> hopefuls = (from x in candidateStates
                                                      where x.Value.State == CandidateState.States.hopeful
                                                      select x)
                                                      .ToDictionary(x => x.Key, x => x.Value.VoteCount);
            int bypass = seats - (from x in candidateStates
                                  where x.Value.State == CandidateState.States.elected
                                  select x).Count();

            HashSet<Candidate> retained = new HashSet<Candidate>();
            HashSet<Candidate> eliminated = null;

            // We're out of seats, so eliminate everybody
            if (bypass == 0)
                return hopefuls.Keys;

            // This doesn't account for ties on its own
            if (bypass > 0)
            {
                var q = from x in hopefuls
                            orderby x.Value descending
                            select x.Key;
                retained.UnionWith(q.Take(bypass));
            }

            if (!batchElimination)
            {
                // Select all candidates with the minimum
                var query = from x in hopefuls
                            where !retained.Contains(x.Key)
                            select x;
                decimal min = (from x in query select x.Value).Min();
                // Select into the bypass set if there are ties across sets
                eliminated = (from x in hopefuls
                              where x.Value == min
                              select x.Key).ToHashSet();
                // This should not happen when requesting a single
                if (eliminated.Count == 0)
                    throw new InvalidOperationException("Somehow pulled zero minimum candidates in elimination!");
                return eliminated;
            }

            // We're doing batch elimination, so find the least
            while (eliminated is null)
            {
                var qretain = from x in hopefuls
                              where retained.Contains(x.Key)
                              select x;
                var qeliminate = from x in hopefuls
                                 where !retained.Contains(x.Key)
                                 select x;

                // Ties inherently cannot split in batch eliminations,
                // unless all hopefuls are tied.  Will always select
                // a candidate who is untied for last place.
                if (qeliminate.Sum(x => x.Value) >= qretain.Min(x => x.Value) + surplus)
                {
                    // Throw out all candidates tied at this vote count
                    var qmax = from x in qeliminate
                               where x.Value == qeliminate.Max(y => y.Value)
                               select x.Key;
                    // Resolve the query or it modifies retained while using it to query
                    retained.UnionWith(qmax.ToArray());
                    // Last-place ties that we can't batch eliminate.  Use GetSingleElimination()
                    // and expect multiple candidates.
                    if (qeliminate.Count() == 0)
                        return null; 
                }
                else
                    eliminated = qeliminate.Select(x => x.Key).ToHashSet();
            }

            return eliminated;
        }

        /// <inheritdoc/>
        public override IEnumerable<Candidate> GetSingleElimination(Dictionary<Candidate, CandidateState> candidateStates, decimal surplus = 0.0m)
            => GetEliminationCandidates(candidateStates, surplus, false);

        /// <inheritdoc/>
        public override IEnumerable<Candidate> GetBatchElimination(Dictionary<Candidate, CandidateState> candidateStates, decimal surplus = 0.0m)
            => GetEliminationCandidates(candidateStates, surplus, true);
    }
}
