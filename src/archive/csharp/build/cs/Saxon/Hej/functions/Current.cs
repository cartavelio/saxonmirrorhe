using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Current : Saxon.Hej.functions.SystemFunction {
        public static readonly Saxon.Hej.om.StructuredQName FN_CURRENT = Saxon.Hej.om.NamespaceUri.FN.qName("current");
        public Saxon.Hej.om.FunctionItem bindContext(Saxon.Hej.expr.XPathContext context) {
            return null;
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            throw new Saxon.Hej.trans.XPathException("Dynamic evaluation of the current() function is not supported", "XTDE1360");
        }
    
    }
}