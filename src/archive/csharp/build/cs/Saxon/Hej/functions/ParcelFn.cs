using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class ParcelFn : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return new Saxon.Hej.ma.Parcel(arguments[0].materialize());
        }
    
    }
}