using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class UnparcelFn : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.Item arg = arguments[0].head();
            return unparcel(arg, context);
        }
        public static Saxon.Hej.om.GroundedValue unparcel(Saxon.Hej.om.Item arg, Saxon.Hej.expr.XPathContext context) {
            if ((arg is Saxon.Hej.ma.Parcel)) {
                return ((Saxon.Hej.ma.Parcel)(arg)).getValue();
            } else {
                if (Saxon.Hej.ma.Parcel.TYPE.matches(arg, context.getConfiguration().getTypeHierarchy())) {
                    return ((Saxon.Hej.ma.map.MapItem)(arg)).get(Saxon.Hej.ma.Parcel.parcelKey);
                } else {
                    throw new Saxon.Hej.trans.XPathException("The first argument of fn:unparcel must be a parcel", "XPTY0004");
                }
            }
        }
    
    }
}