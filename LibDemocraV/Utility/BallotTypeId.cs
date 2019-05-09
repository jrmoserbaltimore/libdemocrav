using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    /// <summary>
    /// Attach this to classes implementing Ballot and to types related to those.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class |
        System.AttributeTargets.Struct,
        AllowMultiple = false,
        Inherited = false)
    ]
    public class BallotTypeId : System.Attribute
    {
        public Guid Id { get; set; }

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
