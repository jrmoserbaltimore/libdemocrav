using System;
using System.Collections.Generic;
using System.Composition;
using System.Text;
using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Tiebreaking;
using MoonsetTechnologies.Voting.Utility;

namespace MoonsetTechnologies.Voting.Tabulation
{
    /// <inheritdoc/>
    [Export(typeof(AbstractTabulator))]
    [ExportMetadata("Algorithm", "tideman-alternative")]
    [ExportMetadata("Factory", typeof(TidemansAlternativeTabulatorFactory))]
    [ExportMetadata("Title", "Tideman's Alternative")]
    [ExportMetadata("Description", "Uses the Tideman's Alternative algorithm, which " +
        "elects a Condorcet candidate or, if no such candidate exists, eliminates all " +
        "candidates not in the top cycle, performs one round of runoff to eliminate " +
        "the candidate with the fewest first-preference votes, and repeats the whole " +
        "tabulation.")]
    //[ExportMetadata("Constraints", new[] { "condorcet", "majority", "condorcet-loser",
    // "majority-loser", "mutual-majority", "smith", "isda", "clone-independence",
    // "polynomial-time", "resolvability" })]
    public class TidemansAlternativeTabulator : RunoffTabulator
    {
        TopCycle.TopCycleSets condorcetSet = TopCycle.TopCycleSets.schwartz;
        TopCycle.TopCycleSets retainSet = TopCycle.TopCycleSets.smith;

        public TidemansAlternativeTabulator(TabulationMediator mediator,
            AbstractTiebreakerFactory tiebreakerFactory)
            : base(mediator, tiebreakerFactory)
        {

        }

        protected override void InitializeTabulation(BallotSet ballots, IEnumerable<Candidate> withdrawn, int seats)
        {
            base.InitializeTabulation(ballots, withdrawn, seats);

            RankedTabulationAnalytics analytics;
            analytics = new RankedTabulationAnalytics(ballots, seats);

            batchEliminator = new TidemansAlternativeBatchEliminator(analytics, seats,
                condorcetSet, retainSet);
        }
    }
}
