using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility.Attributes
{
    /// <summary>
    /// Attach this to the Tabulator class for a Smith-effficient tabulator.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class,
        AllowMultiple = false,
        Inherited = false)
    ]
    public class SmithEfficient : System.Attribute
    {
        public SmithEfficient()
        {
            
        }
    }
}
