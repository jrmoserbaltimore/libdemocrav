using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Tabulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonsetTechnologies.Voting.Analytics
{
    /// <summary>
    /// Analyzes a set of ranked ballots to deliver various approval
    /// voting outcomes by minimal manipulation of assumptions.
    /// </summary>
    public class ApprovalAlternateOutcomeAnalysis : AbstractAlternateOutcomeAnalysis
    {
        public ApprovalAlternateOutcomeAnalysis(BallotSet ballots, IEnumerable<Candidate> withdrawn, int seats = 1)
            : base(ballots, withdrawn, seats)
        {
        }
        /// <summary>
        /// Calculates the base outcome.
        /// </summary>
        /// <param name="depth">The number of ranks to count.  If 0, count all ranks as an approval vote.</param>
        /// <returns>The vote counts and candidate states.</returns>
        public Dictionary<Candidate, CandidateState> BaseOutcome(int depth = 0)
        {
            Dictionary<Candidate, CandidateState> cS = new Dictionary<Candidate, CandidateState>();

            HashSet<Candidate> query = (from x in Ballots
                                        from y in x.Votes
                                        select y.Candidate)
                                        .ToHashSet();
            foreach (Candidate c in query.Except(Withdrawn))
            {
                cS[c] = new CandidateState()
                {
                    State = CandidateState.States.hopeful,
                    VoteCount = 0m
                };

                cS[c].VoteCount = (from x in Ballots
                                   where (from y in x.Votes
                                          where y.Candidate == c
                                          where depth == 0 || y.Value <= depth
                                          select true).Contains(true)
                                   select x.Count).Sum();
            }

            // FIXME:  Runoff eliminator should be able to do this
            HashSet<Candidate> winners = (from x in cS
                                          orderby x.Value.VoteCount descending
                                          select x.Key).Take(Seats).ToHashSet();

            // If there are ties, select them too
            winners = (from x in cS
            where (from y in cS where winners.Contains(y.Key) select y.Value.VoteCount).Contains(x.Value.VoteCount)
            select x.Key).ToHashSet();

            foreach (Candidate c in cS.Keys.Except(Withdrawn.Union(winners)))
                cS[c].State = CandidateState.States.defeated;

            // FIXME:  will elect more than (seats) if ties
            foreach (Candidate c in winners)
                cS[c].State = CandidateState.States.elected;
            return cS;
        }

        public (BallotSet original, BallotSet modified) FindOutcome(IEnumerable<Candidate> candidates)
        {
            HashSet<CountedBallot> originalBallots = new HashSet<CountedBallot>();
            HashSet<CountedBallot> modifiedBallots = new HashSet<CountedBallot>();
            
            if (candidates.Count() != Seats)
                throw new ArgumentException("candidates must include a full winning set.");
            Dictionary<Candidate, CandidateState> baseOutcome = BaseOutcome();
            HashSet<Candidate> originalWinners = (from x in baseOutcome
                                                 where x.Value.State == CandidateState.States.elected
                                                 select x.Key).ToHashSet();
            // No change
            if (originalWinners.SetEquals(candidates))
                return (null, null);
            HashSet<Candidate> newWinners = candidates.Except(originalWinners).ToHashSet();
            // Reduce those votes to this count
            decimal maxLoserVotes = (from x in baseOutcome
                                     where newWinners.Contains(x.Key)
                                     select x.Value.VoteCount).Min() - 1;
            // We'll need to reduce the votes for all candidates who defeat the new winners.
            HashSet<Candidate> newLosers = (from x in baseOutcome
                                           where x.Value.VoteCount > maxLoserVotes
                                           select x.Key).Except(candidates).ToHashSet();

            // Strategy:  Reduce the depth of approval assumptions for ballots with newLosers
            // unti we have their vote counts below the lowest-voted newWinners.  Seek the deepest
            // rankings, where the newLosers are ranked last and the ballots have a lot of rankings.
            Dictionary<Candidate, decimal> maxChangedVotes = new Dictionary<Candidate, decimal>();
            // Maximum depth at which to approve the candidate
            Dictionary<Candidate, int> maxDepth = new Dictionary<Candidate, int>();

            // Find the minimum depth where reduced approval assumptions affect newLoser vote counts
            foreach (Candidate c in newLosers)
            {
                maxChangedVotes[c] = (from x in baseOutcome
                                      where x.Key == c
                                      select x.Value.VoteCount).Max() - maxLoserVotes;
                maxDepth[c] = Convert.ToInt32((from x in Ballots
                                               from y in x.Votes
                                               where y.Candidate == c
                                               select y.Value).Max());
            }

            // Pass 1:  iterate through depths until voting in each candidate.
            // Truncate all ballots with candidates below that depth.
            do
            {
                bool searching = false;
                foreach (Candidate c in newLosers)
                {
                    decimal changedVotes = (from x in Ballots
                                         where (from y in x.Votes
                                                where y.Candidate == c
                                                where y.Value >= maxDepth[c]
                                                select true).Contains(true)
                        // If A wins and we're trying to show an approval outcome where B wins instead,
                        // we would modify a ballot with X>B>A to only approve X instead of {X,B,A}.
                        // By this logic, ballots where B is ranked above A don't count.
                                         where !(from y in x.Votes
                                                where newWinners.Contains(y.Candidate)
                                                where y.Value >= maxDepth[c]
                                                select true).Contains(true)
                                         select x.Count).Sum();
                    if (changedVotes < maxChangedVotes[c])
                    {
                        maxDepth[c]--;
                        searching = true;
                    }
                }

                if (!searching)
                    break;
            } while (true);

            // One greater than maxDepth, thus we need to change all of these
            foreach (Candidate c in newLosers)
            {
                originalBallots.UnionWith(from x in Ballots
                                          where (from y in x.Votes
                                                 where y.Candidate == c
                                                 where y.Value > maxDepth[c]
                                                 select true).Contains(true)
                                          where !(from y in x.Votes
                                                 where newWinners.Contains(y.Candidate)
                                                 where y.Value > maxDepth[c]
                                                 select true).Contains(true)
                                          select x);
            }

            // Modified ballots strip out all the lower-ranked candidates
            // below a newLoser who is ranked below maxDepth
            modifiedBallots.UnionWith(from x in originalBallots
                                      select new CountedBallot(
                                          new Ballot(from y in x.Votes
                                                     where y.Value < (from z in x.Votes
                                                                      where maxDepth.ContainsKey(z.Candidate)
                                                                      where z.Value > maxDepth[z.Candidate]
                                                                      select z.Value).Min()
                                                     select y),
                                          x.Count));

            // FIXME:  elucidate the least change required:
            //   1.  From shallowest maxDepth to deepest, identify ballots which demote the most newLosers
            //   2.  Find such ballots with the highest counts
            //   3.  Modify the ballots which can be removed without demoting newWinners
            //   4.  Repeat until we've forced the the approval outcome
            throw new NotImplementedException();
        }
    }
}
