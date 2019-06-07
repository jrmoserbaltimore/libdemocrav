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

            void firstPass(Candidate c)
            {
                bool searching = false;
                decimal countChangedVotes(Candidate d)
                {
                    return
                        (from x in Ballots
                             // All ballots currently selected for all candidates
                     where (from y in x.Votes
                                where maxDepth.Keys.Contains(y.Candidate)
                                where y.Value >= maxDepth[y.Candidate]
                                select true).Contains(true)
                     // Only ballots where the candidate appears ranked below the minimum max depth
                     where (from y in x.Votes
                                where y.Candidate == d
                                where y.Value >= maxDepth.Values.Min()
                                select true).Contains(true)
                     // Select ballots where the highest-ranked candidate to be deranked is ranked
                     // higher than (c)
                     where (from y in x.Votes
                                where maxDepth.Keys.Contains(y.Candidate)
                                where y.Value >= maxDepth[y.Candidate]
                                orderby y.Value
                                select (y.Candidate, y.Value)).First().Candidate != d
                                // ... or where the candidate appears at its own maxDepth
                                || (from y in x.Votes
                                    where y.Candidate == d
                                    where y.Value >= maxDepth.Values.Min()
                                    select true).Contains(true)
                     // If A wins and we're trying to show an approval outcome where B wins instead,
                     // we would modify a ballot with X>B>A to only approve X instead of {X,B,A}.
                     // By this logic, ballots where B is ranked above A don't count.
                     where !(from y in x.Votes
                                 where newWinners.Contains(y.Candidate)
                                 where y.Value >= maxDepth[d]
                                 select true).Contains(true)
                         select x.Count).Sum();
                }

                do
                {
                    // This will use the ABSOLUTE maxDepth for each candidate when checking
                    // for shared ballots (i.e. ballots where (c) is ranked above maxDepth[c],
                    // but below some other candidate's maxDepth).
                    //
                    // There may be e.g. 100 ballots for candidate X as such, but we only need
                    // 50 of those.  In such a case, we'll come up short after modifying the
                    // ballots for the candidates reaching the shallowest maxDepth, and will
                    // need to take further action.
                    decimal changedVotes = countChangedVotes(c);
                    if (changedVotes < maxChangedVotes[c])
                    {
                        maxDepth[c]--;
                        searching = true;
                    }
                    else
                        break;
                } while (true);

                // Move any candidates with a greater depth back to the start.  They might
                // be covered incidentally by freeriding on existing ballots.
                if (searching)
                {
                    var query = from x in newLosers
                                where x != c
                                where maxDepth[x] > maxDepth[c]
                                select x;

                    HashSet<Candidate> qresult = query.ToHashSet();

                    foreach (Candidate d in qresult)
                        maxDepth[d] = Convert.ToInt32((from x in Ballots
                                                       from y in x.Votes
                                                       where y.Candidate == d
                                                       select y.Value).Max());

                    // Horrifying recursive loop recursion
                    foreach (Candidate d in qresult)
                    {
                        if (query.Contains(d))
                            firstPass(d);
                    }
                }
            }

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
            foreach (Candidate c in newLosers)
            {
                firstPass(c);
            }

            // One greater than maxDepth, thus we need to change all of these in any case
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

            // Pass 2:  we've identified all ballots that WILL be in the minimal change set.
            // Now we must figure out what remaining ballots give the most impact.

            // FIXME:  elucidate the least change required:
            //   1.  From shallowest maxDepth to deepest, identify ballots which demote the most newLosers
            //   2.  Find such ballots with the highest counts
            //   3.  Modify the ballots which can be removed without demoting newWinners
            //   4.  Repeat until we've forced the the approval outcome
            void secondPass()
            {
                // Candidates who aren't yet losing
                HashSet<Candidate> remainingCandidates = (from x in newLosers
                                                          where (from y in modifiedBallots
                                                                 where (from z in y.Votes
                                                                        select z.Candidate).Contains(x)
                                                                 select y.Count).Sum() > maxChangedVotes[x]
                                                          select x).ToHashSet();

                // Maximum number of votes which need changing to eliminate any one candidate
                decimal maxVotesToChange = (from x in remainingCandidates
                                            select maxChangedVotes[x]).Max();
                // Get all candidates at that level
                HashSet<Candidate> maxChangeCandidates = (from x in remainingCandidates
                                                         where maxChangedVotes[x] > maxVotesToChange
                                                         select x).ToHashSet();
                // Select the ballot with the most candidates present below the given candidates
                CountedBallot possibleBallot = (from b in Ballots.Except(originalBallots)
                                                where (from x in b.Votes
                                                       where maxChangeCandidates.Contains(x.Candidate)
                                                       where x.Value == maxDepth[x.Candidate]
                                                       select true).Contains(true)
                                                where !(from x in b.Votes
                                                        where newWinners.Contains(x.Candidate)
                                                        where x.Value >= (from y in maxDepth
                                                                          where newWinners.Contains(y.Key)
                                                                          select y.Value).Min()
                                                        select true).Contains(true)
                                                orderby (from x in b.Votes
                                                         where remainingCandidates.Contains(x.Candidate)
                                                         select x).Count(),
                                                         (from x in b.Votes
                                                          where maxChangeCandidates.Contains(x.Candidate)
                                                          where x.Value == maxDepth[x.Candidate]
                                                          select x).Count()
                                                select b).First();

                Ballot newBallot = new Ballot(from y in possibleBallot.Votes
                                              where y.Value < (from z in possibleBallot.Votes
                                                               where maxChangeCandidates.Contains(z.Candidate)
                                                               where z.Value == maxDepth[z.Candidate]
                                                               select z.Value).Min()
                                              select y);
                originalBallots.Add(possibleBallot);
                if (possibleBallot.Count <= maxVotesToChange)
                {
                    // All of them, so just move it
                    modifiedBallots.Add(new CountedBallot(newBallot, possibleBallot.Count));
                }
                else
                {
                    // Split into two sets
                    modifiedBallots.Add(new CountedBallot(newBallot, Convert.ToInt64(maxVotesToChange)));
                    modifiedBallots.Add(new CountedBallot(new Ballot(possibleBallot.Votes), possibleBallot.Count - Convert.ToInt64(maxVotesToChange)));
                }
            }
            // FIXME:  Loop the above until resolution or out of ballots

            throw new NotImplementedException();
        }
    }
}
