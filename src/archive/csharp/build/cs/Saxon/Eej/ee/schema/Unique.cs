using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class Unique : Saxon.Eej.ee.schema.IdentityConstraint {
        public override string getConstraintCategory() {
            return "unique";
        }
    
    }
}