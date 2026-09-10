using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class AllEqual : Saxon.Hej.functions.CollatingFunctionFixed {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.lib.StringCollator collator = getStringCollator();
            Saxon.Hej.functions.DistinctValues.DistinctIterator iter = new Saxon.Hej.functions.DistinctValues.DistinctIterator(arguments[0].iterate(), collator, context);
            int count = Saxon.Hej.functions.Count.count(iter);
            return Saxon.Hej.value.BooleanValue.get((count<=1));
        }
    
    }
}