using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public sealed class Floor : Saxon.Hej.functions.ScalarSystemFunction {
        public override Saxon.Hej.value.AtomicValue evaluate(Saxon.Hej.om.Item arg, Saxon.Hej.expr.XPathContext context) {
            return ((Saxon.Hej.value.NumericValue)(arg)).floor();
        }
        public override string getCompilerName() {
            return "RoundingCompiler";
        }
    
    }
}