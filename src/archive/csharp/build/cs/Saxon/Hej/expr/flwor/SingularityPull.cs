using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class SingularityPull : Saxon.Hej.expr.flwor.TuplePull {
        private bool done = false;
        public override bool nextTuple(Saxon.Hej.expr.XPathContext context) {
            if (done) {
                return false;
            } else {
                done = true;
                return true;
            }
        }
    
    }
}