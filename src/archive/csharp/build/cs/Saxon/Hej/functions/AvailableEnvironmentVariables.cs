using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class AvailableEnvironmentVariables : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.lib.EnvironmentVariableResolver resolver = context.getConfiguration().getConfigurationProperty(Saxon.Hej.lib.Feature<Saxon.Hej.lib.EnvironmentVariableResolver>.ENVIRONMENT_VARIABLE_RESOLVER);
            System.Collections.Generic.IList<Saxon.Hej.om.Item> myList = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
            if (context.getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_EXTERNAL_FUNCTIONS)) {
                foreach (string s in resolver.getAvailableEnvironmentVariables()) {
                    myList.Add(new Saxon.Hej.value.StringValue(s));
                }
            }
            return new Saxon.Hej.value.SequenceExtent.Of<Saxon.Hej.om.Item>(myList);
        }
        public override Saxon.Hej.expr.Expression makeFunctionCall(Saxon.Hej.expr.Expression[] arguments) {
            return new Inner_SystemFunctionCall_d1029e204(this, this, arguments);
        }
    
        private class Inner_SystemFunctionCall_d1029e204 : Saxon.Hej.expr.SystemFunctionCall {
            private readonly AvailableEnvironmentVariables outerInstance;
            public Inner_SystemFunctionCall_d1029e204(AvailableEnvironmentVariables outerInstance, AvailableEnvironmentVariables arg1, /*AA*/Saxon.Hej.expr.Expression[] arg2) : base(arg1, arg2) {
                this.outerInstance = outerInstance;
            }
            public override Saxon.Hej.expr.Expression preEvaluate(Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
                return this;
            }
        }

    }
}