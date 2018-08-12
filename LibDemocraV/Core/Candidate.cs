//
// Copyright (c) Secure Democratic Election Services, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//


using System;
using System.Collections;
using System.Collections.Generic;

namespace DemocraticElections.Voting
{
    public class Candidate
    {
        protected Guid Id { get; set; }

        public Candidate(Candidate c)
        {
            Id = c.Id;
        }

        public virtual bool Equals(Candidate c)
        {
            return (c.Id.Equals(Id));
        }

        public virtual int GetHashCode(Candidate c)
        {
            return (c.Id.GetHashCode());
        }
    }
}
