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
            HashSet<Candidate> eliminated = new HashSet<Candidate>();

            // We're out of seats, so eliminate everybody
            // FIXME:  Do we just want to throw an exception here?
            if (bypass == 0)
                return hopefuls.Keys;

            while (bypass > 0)
            {
                retained.Add(hopefuls.Where(x => !retained.Contains(x.Key)).Aggregate((x, y) => x.Value > y.Value ? x : y).Key);
                bypass--;
            }

            // Select lowest
            if (!batchElimination)
            {
                Candidate min = hopefuls.Where(x => !retained.Contains(x.Key)).Aggregate((x, y) => x.Value < y.Value ? x : y).Key;
            }
            else while (eliminated.Count == 0)
            {
                // select batch.  Check sum against minimum retained candidate.
                decimal max = hopefuls.Where(x => retained.Contains(x.Key)).Aggregate((x, y) => x.Value < y.Value ? x : y).Value;
                // If we sum them all and the vote total is bigger than the lowest-voted retained candidate,
                // move all highest-voted candidates not retained to the retained set.
                if (hopefuls.Where(x => !retained.Contains(x.Key)).Sum(x => x.Value) + surplus >= max)
                {
                    max = hopefuls.Where(x => !retained.Contains(x.Key)).Aggregate((x, y) => x.Value > y.Value ? x : y).Value;
                    retained.UnionWith(hopefuls.Where(x => x.Value == max).Select(x => x.Key));
                }
                else
                    eliminated.UnionWith(hopefuls.Where(x => !retained.Contains(x.Key)).Select(x => x.Key).ToList());
                // we just eliminated the last candidate(s), so break
                if (hopefuls.Keys.Count == retained.Count)
                    break;
            }

            // No results, so make a call for a single eliminee
            if (eliminated.Count == 0)
            {
                // This should not happen when requesting a single
                if (!batchElimination)
                    throw new InvalidOperationException("Somehow pulled zero minimum candidates in elimination!");

                eliminated = GetSingleElimination(candidateStates, surplus).ToHashSet();
                // Return null to indicate a tie for a batch request.
                if (eliminated.Count > 1)
                    return null;
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
