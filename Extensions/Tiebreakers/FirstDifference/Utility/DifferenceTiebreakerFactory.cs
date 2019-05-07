using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonsetTechnologies.Voting.Tiebreaking;

namespace MoonsetTechnologies.Voting.Utility
{
    class DifferenceTiebreakerFactory : AbstractTiebreakerFactory
    {
        public DifferenceTiebreakerFactory()
        {
        }

        // XXX:  This shouldn't be any different than a LastDifference-FirstDifference
        // tiebreaker.  Sequence shouldn't matter between these two.  Verify.
        /// <summary>
        /// Creates a LastDifference-FirstDifference, FirstDifference tiebreaker.
        /// </summary>
        /// <returns></returns>
        public override ITiebreaker CreateTiebreaker()
        {
            ITiebreaker firstDifference = new FirstDifferenceTiebreaker();
            ITiebreaker tiebreaker = new SeriesTiebreaker(
                new ITiebreaker[] {
                    new SequentialTiebreaker(
                        new ITiebreaker[] {
                          new LastDifferenceTiebreaker(),
                          firstDifference,
                        }.ToList()
                    ),
                    new LastDifferenceTiebreaker(),
                    firstDifference,
                }.ToList());
            return tiebreaker;
        }
    }
}
