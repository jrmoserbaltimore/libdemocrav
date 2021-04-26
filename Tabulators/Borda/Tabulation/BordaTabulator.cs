using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Utility;

namespace MoonsetTechnologies.Voting.Tabulation
{
    [Export(typeof(AbstractTabulator))]
    [ExportMetadata("Algorithm", "borda-count")]
    [ExportMetadata("Factory", typeof(BordaTabulatorFactory))]
    [ExportMetadata("Title", "Borda Count")]
    [ExportMetadata("Description", "Borda's method using tournament-style counting.  " +
        "Each candidate's rank on each ballot is subtracted from the number of " +
        "candidates, and as many points are awarded to that candidate.  Vulnerable " +
        "to sophisticated voting and strategic nomination.  Black's method or " +
        "Smith-constrained Borda are recommended if using Borda.")]
    [ExportMetadata("Settings", new[]
    {
        typeof(SmithConstrainedTabulatorSetting),
        typeof(BordaBlackSetting)
    })]
    //[ExportMetadata("Constraints", new[] {  })]
    // Borda is terrible.  Never use it.
    public class BordaTabulator : AbstractTabulator
    {
        /// <inheritdoc/>
        public BordaTabulator(TabulationMediator mediator,
            IEnumerable<ITabulatorSetting> tabulatorSettings)
            : base(mediator, tabulatorSettings)
        {

        }
        protected override void CountBallot(CountedBallot ballot)
        {
            // Only counts hopeful and elected candidates
            HashSet<Candidate> candidates
                = new HashSet<Candidate>(
                    from x in candidateStates
                    where new[] { CandidateState.States.hopeful, CandidateState.States.elected, CandidateState.States.defeated }
                      .Contains(x.Value.State)
                    select x.Key);

            foreach (Vote v in ballot.Votes)
            {
                // Skip candidates not included in this count.
                if (!candidates.Contains(v.Candidate))
                    continue;
                // Add borda score, which for rank r and c candidates is c-r
                if (!(v is null))
                    candidateStates[v.Candidate].VoteCount += ballot.Count * (candidates.Count - v.Value);
            }
        }

        protected override IEnumerable<Candidate> GetEliminationCandidates()
        {
            decimal bordaWin = (from x in candidateStates
                                orderby x.Value.VoteCount descending
                                select x).ElementAt(seats - 1).Value.VoteCount;
            // This may leave more hopefuls than seats if there are ties
            return from x in candidateStates
                   where x.Value.VoteCount < bordaWin
                   select x.Key;
        }
        protected override TabulationStateEventArgs TabulateRound()
        {
            return new TabulationStateEventArgs
            {
                CandidateStates = CandidateStatesCopy,
                Note = ""
            };
        }
    }
}