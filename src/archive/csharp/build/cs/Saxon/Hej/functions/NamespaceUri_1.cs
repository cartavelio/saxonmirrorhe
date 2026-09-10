using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class NamespaceUri_1 : Saxon.Hej.functions.ScalarSystemFunction {
        public override Saxon.Hej.value.AtomicValue evaluate(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
            string uri = ((Saxon.Hej.om.NodeInfo)(item)).getURI();
            return new Saxon.Hej.value.AnyURIValue(uri);
        }
        public override Saxon.Hej.value.AnyURIValue resultWhenEmpty() {
            return new Saxon.Hej.value.AnyURIValue("");
        }
        public override string getCompilerName() {
            return "NamespaceUriFnCompiler";
        }
    
    }
}