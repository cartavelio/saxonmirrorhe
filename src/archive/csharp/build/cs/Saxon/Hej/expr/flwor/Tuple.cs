using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class Tuple : Saxon.Hej.value.ObjectValue<Saxon.Hej.om.Sequence[]> {
        public Tuple(Saxon.Hej.om.Sequence[] members) : base (members) {
        }
        public Saxon.Hej.om.Sequence[] getMembers() {
            return getObject();
        }
    
    }
}