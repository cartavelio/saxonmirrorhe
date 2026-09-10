using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public sealed class Ceiling : Saxon.Hej.functions.ScalarSystemFunction {
        public override Saxon.Hej.value.NumericValue evaluate(Saxon.Hej.om.Item arg, Saxon.Hej.expr.XPathContext context) {
            return ((Saxon.Hej.value.NumericValue)(arg)).ceiling();
        }
        public override string getCompilerName() {
            return "RoundingCompiler";
        }
    
    }
}