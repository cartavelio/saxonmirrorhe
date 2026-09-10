using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class LocalName_1 : Saxon.Hej.functions.ScalarSystemFunction {
        public override Saxon.Hej.value.AtomicValue evaluate(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
            return Saxon.Hej.value.StringValue.makeStringValue(((Saxon.Hej.om.NodeInfo)(item)).getLocalPart());
        }
        public override Saxon.Hej.om.Sequence resultWhenEmpty() {
            return Saxon.Hej.value.StringValue.EMPTY_STRING;
        }
        public override string getCompilerName() {
            return "LocalNameCompiler";
        }
    
    }
}