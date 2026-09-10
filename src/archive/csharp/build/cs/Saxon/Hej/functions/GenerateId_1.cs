using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class GenerateId_1 : Saxon.Hej.functions.ScalarSystemFunction {
        public override int getSpecialProperties(Saxon.Hej.expr.Expression[] arguments) {
            int p = base.getSpecialProperties(arguments);
            return (p&~(Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED));
        }
        public override Saxon.Hej.om.Sequence resultWhenEmpty() {
            return Saxon.Hej.value.StringValue.EMPTY_STRING;
        }
        public override Saxon.Hej.value.AtomicValue evaluate(Saxon.Hej.om.Item arg, Saxon.Hej.expr.XPathContext context) {
            return generateId((Saxon.Hej.om.NodeInfo)(arg));
        }
        public static Saxon.Hej.value.StringValue generateId(Saxon.Hej.om.NodeInfo node) {
            Saxon.Ejava.lang.StringBuilder buffer = new Saxon.Ejava.lang.StringBuilder(16);
            node.generateId(buffer);
            return new Saxon.Hej.value.StringValue(buffer.ToString());
        }
        public override string getCompilerName() {
            return "GenerateIdCompiler";
        }
    
    }
}