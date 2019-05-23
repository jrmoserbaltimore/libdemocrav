// This algoritm is based on Voting Matters Issue 22, Random Tie-Breaking in STV
//   http://www.votingmatters.org.uk/ISSUE22/I22P1.pdf
// Lundell describes a random tiebreaker in case of sub-ties; we prefer a
// Last Difference fallback tiebreaker, which is equivalent to First Difference
// for tied candidates whose first difference was the prior round.  The
// fallback tiebreaker of Last Difference would be a random tiebreaker.
using System;
using System.Collections.Generic;
using System.Linq;
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Tabulation;

namespace MoonsetTechnologies.Voting.Tiebreaking
{
    public class LundellTiebreaker : AbstractTiebreaker
    {
        public LundellTiebreaker() : base()
        {
        }

        // Always fully-informed:  it uses ballots and weights
        /// <inheritdoc/>
        public override bool FullyInformed => true;

        /// <inheritdoc/>
        protected override Candidate BreakTie(IEnumerable<Candidate> candidates,
            BallotSet ballots,
            Dictionary<Ballot, decimal> ballotWeights,
            bool findWinner)
        {
            List<Candidate> eliminees = candidates.ToList();
            Dictionary<Candidate, CandidateState> cS = new Dictionary<Candidate, CandidateState>();

            void countVotes()
            {
                cS = eliminees.ToDictionary(x => x,
                    y => new CandidateState
                    {
                        State = CandidateState.States.hopeful,
                        VoteCount = 0
                    });

                foreach (CountedBallot b in ballots)
                {
                    List<Vote> votes = b.Votes.Where(x => cS.Keys.Contains(x.Candidate)).ToList();
                    if (votes.Count == 0)
                        continue;
                    Vote top = votes.First();
                    foreach (Vote u in votes.Except(new[] { top }))
                    {
                        if (u.Beats(top))
                            top = u;
                    }
                    // Either null or must contain a weight for every ballot
                    cS[top.Candidate].VoteCount += b.Count * (ballotWeights is null ? 1 : ballotWeights[b]);
                }
            }

            Candidate findSingleCandidate(List<Candidate> selectees)
            {
                Candidate Cd = null;
                if (selectees.Count == 1)
                    Cd = selectees.Single();
                else
                {
                    // We want to remove the losers if finding the winner,
                    // and this recurses by finding the biggest loser to remove
                    if (selectees.Count < eliminees.Count)
                        Cd = BreakTie(selectees, ballots, ballotWeights, !findWinner);
                    else
                    {
                        // FIXME:  use fallback tiebreaker
                    }
                }
                return Cd;
            }

            Candidate getTopCandidate()
            {
                // Get all candidates with the single maximum vote value (winners)
                decimal maxVotes = cS.Max(x => x.Value.VoteCount);
                List<Candidate> selectees = cS.Where(x => x.Value.VoteCount == maxVotes).Select(x => x.Key).ToList();

                return findSingleCandidate(selectees);
            }

            Candidate getBottomCandidate()
            {
                // Get all candidates with the single maximum vote value (winners)
                decimal maxVotes = cS.Min(x => x.Value.VoteCount);
                List<Candidate> selectees = cS.Where(x => x.Value.VoteCount == maxVotes).Select(x => x.Key).ToList();

                return findSingleCandidate(selectees);
            }

            while (eliminees.Count > 1)
            {
                countVotes();
                if (findWinner)
                    eliminees.Remove(getBottomCandidate());
                else
                    eliminees.Remove(getTopCandidate());
            }
            return eliminees.Single();
        }

        protected override Dictionary<Candidate, Dictionary<Candidate, bool>> GetWinPairs()
        {
            throw new NotImplementedException();
        }

        protected override void UpdateTiebreaker(Dictionary<Candidate, CandidateState> candidateStates)
        {
            throw new NotImplementedException();
        }
    }
}
