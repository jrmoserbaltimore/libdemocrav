//
// Copyright (c) Secure Democratic Election Services, LLC. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//


using System;
using System.Collections;
using System.Collections.Generic;

namespace DemocraticElections.Voting
{
    public class Voter
    {
        protected Guid Id { get; set; }

        public virtual bool Equals(Voter v)
        {
            return (v.Id.Equals(Id));
        }

        public virtual int GetHashCode(Voter v)
        {
            return (v.Id.GetHashCode());
        }
    }
}
