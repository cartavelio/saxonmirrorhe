using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public abstract class DynamicContextAccessor : Saxon.Hej.functions.SystemFunction {
        private Saxon.Hej.value.AtomicValue boundValue;
        public void bindContext(Saxon.Hej.expr.XPathContext context) {
            boundValue = evaluate(context);
        }
        public abstract Saxon.Hej.value.AtomicValue evaluate(Saxon.Hej.expr.XPathContext context);
        public override Saxon.Hej.value.AtomicValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            if ((boundValue != null)) {
                return boundValue;
            } else {
                return evaluate(context);
            }
        }
        public override Saxon.Hej.expr.Expression makeFunctionCall(Saxon.Hej.expr.Expression[] arguments) {
            return new Inner_SystemFunctionCall_4(this, this, arguments);
        }
        public class ImplicitTimezone : Saxon.Hej.functions.DynamicContextAccessor {
            public override Saxon.Hej.value.AtomicValue evaluate(Saxon.Hej.expr.XPathContext context) {
                Saxon.Hej.value.DateTimeValue now = Saxon.Hej.value.DateTimeValue.getCurrentDateTime(context);
                return now.getComponent(Saxon.Hej.functions.AccessorFn.Component.TIMEZONE);
            }
        
        }
        public class CurrentDateTime : Saxon.Hej.functions.DynamicContextAccessor {
            public override Saxon.Hej.value.AtomicValue evaluate(Saxon.Hej.expr.XPathContext context) {
                return Saxon.Hej.value.DateTimeValue.getCurrentDateTime(context);
            }
        
        }
        public class CurrentDate : Saxon.Hej.functions.DynamicContextAccessor {
            public override Saxon.Hej.value.AtomicValue evaluate(Saxon.Hej.expr.XPathContext context) {
                Saxon.Hej.value.DateTimeValue now = Saxon.Hej.value.DateTimeValue.getCurrentDateTime(context);
                return now.toDateValue();
            }
        
        }
        public class CurrentTime : Saxon.Hej.functions.DynamicContextAccessor {
            public override Saxon.Hej.value.AtomicValue evaluate(Saxon.Hej.expr.XPathContext context) {
                Saxon.Hej.value.DateTimeValue now = Saxon.Hej.value.DateTimeValue.getCurrentDateTime(context);
                return now.toTimeValue();
            }
        
        }
        public class DefaultLanguage : Saxon.Hej.functions.DynamicContextAccessor {
            public override Saxon.Hej.value.AtomicValue evaluate(Saxon.Hej.expr.XPathContext context) {
                string lang = context.getConfiguration().getDefaultLanguage();
                return new Saxon.Hej.value.StringValue(lang, Saxon.Hej.type.BuiltInAtomicType.LANGUAGE);
            }
        
        }
    
        private class Inner_SystemFunctionCall_4 : Saxon.Hej.expr.SystemFunctionCall {
            private readonly DynamicContextAccessor outerInstance;
            public Inner_SystemFunctionCall_4(DynamicContextAccessor outerInstance, DynamicContextAccessor arg1, /*AA*/Saxon.Hej.expr.Expression[] arg2) : base(arg1, arg2) {
                this.outerInstance = outerInstance;
            }
            public override  /* REPLACING public */ Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
                return outerInstance.evaluate(/*localparam*/context);
            }
            public override int getIntrinsicDependencies() {
                return Saxon.Hej.expr.StaticProperty.DEPENDS_ON_RUNTIME_ENVIRONMENT;
            }
        }

    }
}