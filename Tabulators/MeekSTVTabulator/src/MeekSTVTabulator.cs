//
// Copyright (c) Moonset Technologies, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
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

using System;
using System.Collections.Generic;
using System.Text;
using MoonsetTechnologies.Voting.Analytics;

namespace MoonsetTechnologies.Voting.Tabulators
{
    public class MeekSTVTabulator : ISTVTabulator
    {
        // number to elect
        private int seats;
        // Number of decimal places. The decimal type avoids
        // rounding error for up to 27 digits. For 9 billion
        // votes, precision up to 17 decimal places is possible.
        // Beyond ten is not generally necessary:
        // OpaVote uses 6, and the reference algorithm uses 9.
        private int precision = 9;
        private decimal omega = 0.000001m;
        private decimal quota = 0.0m;
        private List<IRankedBallot> Ballots;

        private Dictionary<Candidate, CandidateState> candidates;

        private class CandidateState
        {
            public enum States
            {
                defeated = 0,
                withdrawn = 1,
                hopeful = 2,
                elected = 3
            };
            public decimal KeepFactor;
            public decimal VoteCount;
            public States State;
        }

        public MeekSTVTabulator()
        {
        }

        // Reference rule B.2.a
        protected void DistributeVotes()
        {
            // The reference rule says to round UP, so we need
            // to Round(weight + 10^(-precision) / 2).
            decimal r = 0.5m * Convert.ToDecimal(Math.Pow(10.0D,
                Convert.ToDouble(0 - precision)));

            // Zero the vote counts
            foreach (CandidateState c in candidates.Values)
                c.VoteCount = 0.0m;

            // Distribute the ballots among the votes.
            // Meek also considered an alternative formulation in which
            // voters would be allowed to indicate equal preference for
            // some candidates instead of a strict ordering; we have not
            // implemented this alternative.
            foreach (IRankedBallot b in Ballots)
            {
                decimal weight = 1.0m;
                List<IRankedVote> votes = new List<IRankedVote>(b.Votes);
                votes.Sort();
                foreach (IRankedVote v in votes)
                {
                    // Get value to transfer to this candidate, restricted to
                    // the specified precision
                    decimal value = weight * candidates[v.Candidate].KeepFactor;
                    weight = decimal.Round(weight + r, precision);

                    // Add this to the candidate's vote and remove from ballot's
                    //weight
                    candidates[v.Candidate].VoteCount += value;
                    weight -= value;

                    // Do this until weight hits zero, or we run out of rankings.
                    // Note:  Already-elected candidates have keep factors between
                    // 0 and 1; hopefuls all have 1; defeated will have 0.  The
                    // remaining voting power is either all transfered to the first
                    // hopeful candidate or exhausted as the ballot runs out of
                    // non-defeated candidates.
                    //
                    // We only hit 0 if a non-elected hopeful is on the ballot.
                    if (weight <= 0.0m)
                        break;
                }
            }
        }

        // Reference rule B.2.b
        protected decimal ComputeQuota()
        {
            decimal p = Convert.ToDecimal(Math.Pow(10.0D, Convert.ToDouble(0 - precision)));
            decimal q = 0;
            foreach (CandidateState c in candidates.Values)
                q += c.VoteCount;
            q /= seats + 1;
            // truncate and add the precision digit
            q = decimal.Round(q - 0.5m * p) + p;
            return q;
        }

        // Reference rule B.2.c, returns true if hopefuls get elected.
        protected bool ElectWinners()
        {
            bool elected = false;
            foreach (CandidateState c in candidates.Values)
            {
                // Elect hopefuls who made it
                if (c.State == CandidateState.States.hopeful && c.VoteCount >= quota)
                {
                    c.State = CandidateState.States.elected;
                    elected = true;
                }
            }
            return elected;
        }

        protected decimal ComputeSurplus()
        {
            decimal s = 0.0m;
            // FIXME:  does this mean total surplus not less than zero,
            // or only count candidates whose surplus is greater than zero?
            foreach (CandidateState c in candidates.Values)
            {
                if (c.State == CandidateState.States.elected)
                    s += c.VoteCount - quota;
            }
            if (s < 0.0m)
                s = 0.0m;
            return s;
        }

        protected bool DetectStasis()
        {
            throw new NotImplementedException();
        }
        public IEnumerable<Candidate> SchwartzSet => throw new NotImplementedException();

        public IEnumerable<Candidate> SmithSet => throw new NotImplementedException();

        public IEnumerable<Candidate> Candidates => candidates.Keys;

        public bool Complete => throw new NotImplementedException();

        public void TabulateRound()
        {
            // The s>=surplus check is skipped the first iteration.
            // We implement this by having surplus greater than the
            // number of whole ballots.
            decimal surplus = Convert.ToDecimal(Ballots.Count) + 1;
            // XXX:  B.1 Test Count complete

            do
            {
                decimal s = surplus;
                bool elected = false;
                DistributeVotes();
                quota = ComputeQuota();
                elected = ElectWinners();
                surplus = ComputeSurplus();
                // Counting continues at B1, next tabulation round.
                if (elected)
                    return;
                // Iteration complete, no election, defeat candidates
                else if (surplus >= s || omega > surplus)
                    break;
                // XXX:  B.2.f Update keep factors.

                // XXX:  Detect stasis.
            } while (true);
            // XXX:  B.3 Defeat low candidates
            throw new NotImplementedException();
        }
    }
}