using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Class,
    AllowMultiple = false,
    Inherited = false)
]
    public class TabulationAlgorithm : System.Attribute
    {
        public string Algorithm { get; set; }
        public TabulationAlgorithm(string algorithm)
        {
            Algorithm = algorithm;
        }
    }
}
