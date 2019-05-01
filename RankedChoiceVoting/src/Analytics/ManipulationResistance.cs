using System;
using System.Collections.Generic;
using System.Text;
using MoonsetTechnologies.Voting.Tabulators;

namespace MoonsetTechnologies.Voting.Analytics
{
    // This class analyzes and attempts a number of manipulations, reporting
    // on resistance in practice to alteration.  It also looks for evidence of
    // manipulation or natural failure.
    //
    // Analysis is hard for non-ranked races:  we can only conjecture on vote
    // splitting and voter preference.  Ranked ballot races can demonstrate
    // plurality failures by a hypothetical plurality race.
    //
    // For ranked races, we can perform a number of tests:
    //
    //   - Normalized ideological dimension:  Identify which candidates most
    //     frequently are ranked together above or below each other candidate,
    //     and generate an ideological ordering.
    //
    //     Consider if B is frequently ranked either below A and above C, D,
    //     and E *or* above A and below C, D, and E:
    //
    //     A > B > C > D > E
    //     A > B > D > C > E
    //     D > E > C > B > A
    //
    //     This indicates B sits between A and {C,D,E} in a compound
    //     ideological dimension.  Order all candidates as such and you have
    //     a normalized ideological dimension—for example, from liberal to
    //     moderate to conservative.
    //
    //   - Center Squeeze:  Identify if the Condorcet candidate or the
    //     Smith and Schwartz Sets are squeezed out.  In some systems, such
    //     as IRV and Majority-Runoff, we can reverse an A vs. B race by
    //     adding Candidate C.  We can test for this, and also test for how
    //     many ballot changes we would need to force the outcome, as well
    //     as if voters could bloc vote to force this and, if so, how far
    //     they could overshoot while still successfully manipulating the
    //     system.
    //
    //   - Burying:  identify if burying is likely and, if so, who buried,
    //     and how effective they were.  Some systems resist burying, and
    //     can fail by electing the least-favorable candidate.
    //
    //     Consider the ideological span A>B>C>D>E, where E voters dislike
    //     candidate A more than any other candidate. Knowing C is the
    //     Condorcet candidate, or expecting D or B to win, E voters may
    //     vote as such:
    //
    //     E > A > D > C
    //
    //     Most likely, D voters will vote D > E > C or D > C > E—we know
    //     candidate C is the more-moderate because E voters put E over C
    //     and D, while D voters may place them in either order but
    //     usually place them above B and A.
    //
    //     Because of how D, C, and B voters tend to vote, we can identify
    //     the E > A > ... vote as anomalous even if no E voters vote
    //     honestly.
    //
    //     In some systems, such an attempt can require 30%-40% of the
    //     votes, and so may not even be possible.  In all of them, too
    //     many such votes will elect A instead of eliminating C.  In
    //     Tideman's Alternative, it's quite easy to get CLOSE to
    //     electing E and, having insufficient strategic votes, elect A
    //     instead—the worst outcome for E voters.
    //
    //     We can detect the attempt, its proximity to success, and its
    //     failure outcomes.  We can figure out if it succeeded, failed,
    //     can succeed with the voters present, or failed by electing
    //     a candidate the strategic voters disliked.

    /// <summary>
    /// Strategically manipulates the election to test for manipulation resistance.
    /// </summary>
    class ManipulationResistance
    {
        ITabulator tabulator;
        List<IBallot> ballots;

        public ManipulationResistance(ITabulator tabulator)
        {
            this.tabulator = tabulator;
        }
    }
}
