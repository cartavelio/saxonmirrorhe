using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class AllDifferent : Saxon.Hej.functions.CollatingFunctionFixed {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.lib.StringCollator collator = getStringCollator();
            Saxon.Hej.functions.DistinctValues.DistinctIterator iter = new Saxon.Hej.functions.DistinctValues.DistinctIterator(arguments[0].iterate(), collator, context);
            System.Collections.Generic.IList<string> listener = new System.Collections.Generic.List<string>(1);
            iter.notifyDuplicates(() => listener.Add("gotcha!"));
            while ((((/*1430*/Saxon.Hej.value.AtomicValue)iter.next()) != null)) {
                if (!(listener.Count==0)) {
                    iter.close();
                    return Saxon.Hej.value.BooleanValue.FALSE;
                }
            }
            return Saxon.Hej.value.BooleanValue.get(listener.Count==0);
        }
    
    }
}