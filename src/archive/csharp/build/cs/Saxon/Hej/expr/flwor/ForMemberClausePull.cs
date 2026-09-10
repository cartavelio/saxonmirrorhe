using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class ForMemberClausePull : Saxon.Hej.expr.flwor.ForClausePull {
        public ForMemberClausePull(Saxon.Hej.expr.flwor.TuplePull @base, Saxon.Hej.expr.flwor.ForMemberClause forClause) : base (@base, forClause) {
        }
        protected internal override Saxon.Hej.om.GroundedValue variableValue(Saxon.Hej.om.Item item) {
            return ((Saxon.Hej.ma.Parcel)(item)).getValue();
        }
    
    }
}