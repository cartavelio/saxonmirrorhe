using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Sort_2 : Saxon.Hej.functions.Sort_1 {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            System.Collections.Generic.List<Saxon.Hej.functions.Sort_1.ItemToBeSorted> inputList = getItemsToBeSorted(arguments[0]);
            return doSort(inputList, getCollation(context, arguments[1]), context);
        }
        protected internal Saxon.Hej.lib.StringCollator getCollation(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence collationArg) {
            Saxon.Hej.value.StringValue secondArg = (Saxon.Hej.value.StringValue)(collationArg.head());
            if ((secondArg == null)) {
                return context.getConfiguration().getCollation(getRetainedStaticContext().getDefaultCollationName());
            } else {
                return context.getConfiguration().getCollation(secondArg.getStringValue(), getStaticBaseUriString());
            }
        }
    
    }
}