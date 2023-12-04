using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class AllDifferent : Saxon.Hej.functions.CollatingFunctionFixed {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.lib.StringCollator collator = getStringCollator();
            Saxon.Eej.functions.qt4.DuplicateValues.DuplicatesIterator iter = new Saxon.Eej.functions.qt4.DuplicateValues.DuplicatesIterator(arguments[0].iterate(), collator, context);
            if ((((/*1430*/Saxon.Hej.value.AtomicValue)iter.next()) != null)) {
                iter.close();
                return Saxon.Hej.value.BooleanValue.FALSE;
            }
            return Saxon.Hej.value.BooleanValue.TRUE;
        }
    
    }
}