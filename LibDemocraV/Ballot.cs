using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting
{
    public interface IBallot
    {
        IEnumerable<IVote> Votes { get; }
        string Encode();
    }
    /// <summary>
    /// A Vote object.  Immutable.
    /// </summary>
     public interface IVote
    {
        Candidate Candidate { get; }
    }

    /// <summary>
    /// Attach this to classes implementing IBallot and to types related to those.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class |
        System.AttributeTargets.Struct,
        AllowMultiple = true,
        Inherited = false)
    ]
    public class BallotTypeId : System.Attribute
    {
        private Guid Id;

        public BallotTypeId(Guid id)
        {
            this.Id = id;
        }

        public BallotTypeId(string id)
            : this(Guid.Parse(id))
        {

        }

        public BallotTypeId(Type t)
        {
            var a = t.GetCustomAttributes(GetType(), false);
            if (a.Length != 1)
                throw new ArgumentOutOfRangeException();
            this.Id = ((BallotTypeId)a[0]).Id;
        }
    }
}
