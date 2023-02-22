using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Current : Saxon.Hej.functions.SystemFunction {
        public static readonly Saxon.Hej.om.StructuredQName FN_CURRENT = new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.FN, "current");
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            throw new Saxon.Hej.trans.XPathException("Dynamic evaluation of the current() function is not supported", "XTDE1360");
        }
    
    }
}