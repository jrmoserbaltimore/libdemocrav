using System;
using System.Collections.Generic;
using Xunit;
using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting;
using MoonsetTechnologies.Voting.Tiebreaking;
using System.Linq;
using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Utility;
using Xunit.Abstractions;

namespace MoonsetTechnologies.Voting.Development.Tests
{
    public class BallotSetFixture
    {
        // indexed by candidate number in the input
        public Dictionary<int, Candidate> Candidates { get; private set; }
        public List<Ballot> Ballots { get; private set; }

        public AbstractTiebreaker tiebreaker;

        public ITestOutputHelper output;

        public BallotSetFixture()
        {
            string ballotSet =
                "c|Alex|0\n" +
                "c|Chris|1\n" +
                "c|Sam|2\n" +
                "b|20|0 1 2\n" +
                "b|15|2 1\n" +
                "b|8|1 2 0\n" +
                "b|5|1 0";
            // Decode the above into a thing.
            (Candidates, Ballots) = DecodeBallots(ballotSet);

            AbstractTiebreaker firstDifference = new FirstDifferenceTiebreaker();
            tiebreaker = firstDifference;
        }

        // XXX:  Horrendous parser.  Just enough to limp along.
        // FIXME:  use the real data format filter when implemented.
        private (Dictionary<int, Candidate> candidates, List<Ballot>) DecodeBallots(string input)
        {
            Dictionary<int, Candidate> cout = new Dictionary<int, Candidate>();
            List<Ballot> bout = new List<Ballot>();

            List<string> lines = new List<string>(input.Split("\n"));

            foreach (string s in lines)
            {
                List<string> chunks = new List<string>(s.Split("|"));
                if (chunks.Count != 3)
                    throw new ArgumentException();
                //Add a candidate or add a ballot.  Candidates must come first.
                if (chunks[0] == "c")
                {
                    cout[Convert.ToInt32(chunks[2])] = new Candidate(new Person(chunks[1]));
                }
                else if (chunks[0] == "b")
                {
                    List<Vote> v = new List<Vote>();
                    List<string> votes = new List<string>(chunks[2].Split(" "));

                    // Add a ranked vote at each sequential rank for each sequential candidate.
                    for (int i = 0; i < votes.Count; i++)
                    {
                        int cnum = Convert.ToInt32(votes[i]);
                        v.Add(new Vote(cout[cnum], i + 1));
                    }

                    // Create as many ballots as there are noted in field 2
                    for (int i = 0; i < Convert.ToInt32(chunks[1]); i++)
                        bout.Add(new Ballot(v));

                }
            }
            return (cout, bout);
        }

        public void PrintTabulationState(TabulationStateEventArgs e)
        {
            foreach (Candidate c in e.CandidateStates.Keys)
            {
                MeekCandidateState mc = e.CandidateStates[c] as MeekCandidateState;
                if (!(mc is null))
                    output.WriteLine("  {0}\t{1}\t{2}\t{3}",
                        mc.VoteCount, c.Name, mc.State.ToString(), mc.KeepFactor);
                else
                    output.WriteLine("  {0}\t{1}\t{2}", e.CandidateStates[c].VoteCount,
                        c.Name,
                        e.CandidateStates[c].State.ToString());
            }

            MeekSTVTabulationStateEventArgs me = e as MeekSTVTabulationStateEventArgs;

            if (!(me is null))
            {
                output.WriteLine("  Surplus:\t{0}", me.Surplus);
                output.WriteLine("  Quota:\t{0}", me.Quota);
            }

            RankedTabulationStateEventArgs re = e as RankedTabulationStateEventArgs;
            if (!(re is null))
            {
                output.WriteLine("  Smith Set:");
                foreach (Candidate c in re.SmithSet)
                    output.WriteLine("    {0}", c.Name);
                output.WriteLine("  Schwartz Set:");
                foreach (Candidate c in re.SchwartzSet)
                    output.WriteLine("    {0}", c.Name);
                if (!(re.PairwiseGraph is null))
                {
                    output.WriteLine("  Pairwise Contests:");
                    List<Candidate> c = re.PairwiseGraph.Candidates
                     .Except(
                        e.CandidateStates.Where(
                            x => new[] { CandidateState.States.defeated, CandidateState.States.withdrawn }.Contains(x.Value.State)
                        ).Select(x => x.Key)).ToList();

                    for (int i = 0; i < c.Count; i++)
                    {
                        for (int j = i + 1; j < c.Count; j++)
                        {
                            (decimal vi, decimal vj) = re.PairwiseGraph.GetVoteCount(c[i], c[j]);
                            if (vi >= vj)
                                output.WriteLine("    {0}, {1}\t{2}\tvs\t{3}", vi, vj, c[i].Name, c[j].Name);
                            else
                                output.WriteLine("    {0}, {1}\t{2}\tvs\t{3}", vj, vi, c[j].Name, c[i].Name);
                        }
                    }
                }
            }
            output.WriteLine("  Notes:\t{0}", e.Note);
            output.WriteLine("\n");
        }
    }
}
