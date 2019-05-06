// Tideman's Alternative single-winner ranked ballot tabulator
// Implements three forms:
//   - Smith-constrained, but elect Schwartz Set if one Candidate
//   - Smith-constrained
//   - Schwartz-constrained

using System;
using System.Collections.Generic;
using System.Text;
using MoonsetTechnologies.Voting.Analytics;
using System.Linq;
using MoonsetTechnologies.Voting.Tiebreaking;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public class TidemansAlternativeTabulator : IRankedTabulator
    {
        protected IEnumerable<IRankedBallot> Ballots { get; }
        protected IBatchEliminator batchEliminator;
        public bool Complete => candidates.Count == 1;
        public IEnumerable<Candidate> SmithSet => topCycle.SmithSet;
        public IEnumerable<Candidate> SchwartzSet => topCycle.SchwartzSet;

        protected TopCycle topCycle;
        protected List<Candidate> candidates;
        public IEnumerable<Candidate> Candidates => candidates;

        protected virtual IEnumerable<Candidate> CondorcetCheck(TopCycle t) => t.SchwartzSet;
        protected virtual IEnumerable<Candidate> RetainSet(TopCycle t) => t.SmithSet;

        public TidemansAlternativeTabulator(IEnumerable<Candidate> candidates,
            IEnumerable<IRankedBallot> ballots, IBatchEliminator batchEliminator)
        {
            Ballots = new List<IRankedBallot>(ballots);
            this.candidates = new List<Candidate>(candidates);
            topCycle = new TopCycle(Candidates, Ballots);

            this.batchEliminator = batchEliminator;
        }
        // General algorithm:
        //   if SchwartzSet is One Candidate
        //     Winner is Candidate in SchwartzSet
        //   else
        //     Eliminate Candidates not in SmithSet
        //     Eliminate Candidate with Fewest Votes

        /// <inheritdoc/>
        public virtual void TabulateRound()
        {
            TopCycle t = new TopCycle(Candidates, Ballots);
            List<Candidate> cCheck = new List<Candidate>(CondorcetCheck(t));
            List<Candidate> rSet = new List<Candidate>(RetainSet(t));

            if (cCheck.Count == 1)
                candidates = cCheck;
            else
            {
                // Drop everyone outside the Smith Set
                IVoteCount vc = new RankedVoteCount(rSet, Ballots, batchEliminator);

                // Get rid of the candidate with the fewest votes
                Candidate c = vc.GetVoteCounts().OrderBy(x => x.Value).First().Key;
                candidates = new List<Candidate>(rSet);
                candidates.Remove(c);
            }
            // FIXME:  Update tiebreaker
        }
    }

    public class TidemansAlternativeSmithTabulator : TidemansAlternativeTabulator
    {
        // General algorithm:
        //   if SmithSet is One Candidate
        //     Winner is Candidate in SmithSet
        //   else
        //     Eliminate Candidates not in SmithSet
        //     Eliminate Candidate with Fewest Votes

        // Reconfiguration of algorithm
        protected override IEnumerable<Candidate> CondorcetCheck(TopCycle t) => t.SmithSet;
        protected override IEnumerable<Candidate> RetainSet(TopCycle t) => t.SmithSet;

        public TidemansAlternativeSmithTabulator(IEnumerable<Candidate> candidates,
            IEnumerable<IRankedBallot> ballots,
            IBatchEliminator batchEliminator)
            : base(candidates, ballots, batchEliminator)
        {

        }
    }

    public class TidemansAlternativeSchwartzTabulator : TidemansAlternativeTabulator
    {
        // General algorithm:
        //   if SchwartzSet is One Candidate
        //     Winner is Candidate in SchwartzSet
        //   else
        //     Eliminate Candidates not in SchwartzSet
        //     Eliminate Candidate with Fewest Votes

        // Reconfiguration of algorithm
        protected override IEnumerable<Candidate> CondorcetCheck(TopCycle t) => t.SchwartzSet;
        protected override IEnumerable<Candidate> RetainSet(TopCycle t) => t.SchwartzSet;

        public TidemansAlternativeSchwartzTabulator(IEnumerable<Candidate> candidates,
            IEnumerable<IRankedBallot> ballots,
            IBatchEliminator batchEliminator)
            : base(candidates, ballots, batchEliminator)
        {

        }
    }
}
