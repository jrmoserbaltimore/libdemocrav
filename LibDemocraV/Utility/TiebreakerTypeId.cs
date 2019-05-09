using System;
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
    public class TiebreakerTypeId : System.Attribute
    {
        public Guid Id { get; set; }

        public TiebreakerTypeId(Guid id)
        {
            Id = id;
        }

        public TiebreakerTypeId(string id)
            : this(Guid.Parse(id))
        {

        }

        public TiebreakerTypeId(Type t)
        {
            TiebreakerTypeId[] a = t.GetCustomAttributes(GetType(), false) as TiebreakerTypeId[];

            if (a.Length != 1)
                throw new ArgumentOutOfRangeException();
            Id = a[0].Id;
        }
    }
}
