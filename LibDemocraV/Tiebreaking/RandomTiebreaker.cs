using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Tabulation;

namespace MoonsetTechnologies.Voting.Tiebreaking
{
    public class RandomTiebreaker : AbstractTiebreaker
    {
        private class WellRandom512a : Random
        {
            // Based on the work of Francois Panneton, Pierre L'Ecuyer, and Makoto Matsumoto.
            // Their code is too trivial for much deviation outside generalized implementation.
            const int R = 16;
            const int M1 = 13;
            const int M2 = 9;
            uint state_i = 0;
            uint[] state = new uint[R];
            uint z0, z1, z2;
            public WellRandom512a(int seed)
            {
                state_i = 0;
                Random r = new Random(seed);
                for (int j = 0; j < R; j++)
                {
                    byte[] b = new byte[4];
                    r.NextBytes(b);
                    state[j] = BitConverter.ToUInt32(b); ;
                }
            }

            public WellRandom512a() : this(new Random().Next())
            {
            }

            /// <inheritdoc/>
            public override int Next()
            {
                return Convert.ToInt32(Math.Round(GetRandom() * int.MaxValue));
            }
            /// <inheritdoc/>
            public override int Next(int maxValue)
            {
                return Next() % (maxValue + 1);
            }
            /// <inheritdoc/>
            public override int Next(int minValue, int maxValue)
            {
                return Next(maxValue) + minValue;
            }
            /// <inheritdoc/>
            public override double NextDouble()
            {
                return Convert.ToDouble(GetRandom());
            }
            /// <inheritdoc/>
            public override void NextBytes(byte[] buffer)
            {
                Span<byte> b = buffer;
                NextBytes(b);
            }
            /// <inheritdoc/>
            public override void NextBytes(Span<byte> buffer)
            {
                for (int i = 0; i < buffer.Length;)
                {
                    foreach (byte b in BitConverter.GetBytes(Next()))
                        buffer[i++] = b;
                }
            }
            decimal GetRandom()
            {
                const decimal f = 2.32830643653869628906e-10m;

                uint matrix0Positive(int t, uint v) => v ^ (v >> t);
                uint matrix0Negative(int t, uint v) => v ^ (v << -t);
                uint matrix3Negative(int t, uint v) => v << -t;
                uint matrix4Negative(int t, uint b, uint v) => v ^ ((v << -t) & b);

                uint v0() => state[state_i];
                uint vm1() => state[(state_i + M1) & 0xf];
                uint vm2() => state[(state_i + M2) & 0xf];
                uint vrm1() => state[(state_i + 15) & 0xf];
                uint newV0() => (state_i + 15) & 0xf;

                decimal WELLRNG512a()
                {
                    z0 = vrm1();
                    z1 = matrix0Negative(-16, v0()) ^ matrix0Negative(-15, vm1());
                    z2 = matrix0Positive(11, vm2());
                    state[state_i] = z1 ^ z2;
                    state[newV0()] = matrix0Negative(-2, z0) ^ matrix0Negative(-18, z1) ^ matrix3Negative(-28, z2) ^ matrix4Negative(-5, 0xda442d24U, state[state_i]);
                    state_i = newV0();
                    return state[state_i] * f;
                }
                return WELLRNG512a();
            }
        }

        Random rng = new WellRandom512a();
        public RandomTiebreaker(AbstractTiebreaker tiebreaker = null) : base(tiebreaker)
        {

        }

        public RandomTiebreaker()
            : this(null)
        {
        }

        public override bool FullyInformed => true;


        protected override Candidate BreakTie(IEnumerable<Candidate> candidates, Dictionary<Ballot, decimal> ballotWeights, bool findWinner)
          => candidates.ToList()[rng.Next(candidates.Count() - 1)];

        protected override void UpdateTiebreaker(Dictionary<Candidate, CandidateState> candidateStates)
        {
            return;
        }
    }
}
