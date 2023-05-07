using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class CurrentOutputUri : Saxon.Hej.functions.ContextAccessorFunction, Saxon.Hej.expr.Callable {
        public override int getSpecialProperties(Saxon.Hej.expr.Expression[] arguments) {
            return (base.getSpecialProperties(arguments)|Saxon.Hej.expr.StaticProperty.HAS_SIDE_EFFECTS);
        }
        public override Saxon.Hej.om.FunctionItem bindContext(Saxon.Hej.expr.XPathContext context) {
            string uri = context.getCurrentOutputUri();
            Saxon.Hej.functions.ConstantFunction fn = new Saxon.Hej.functions.ConstantFunction(((uri == null) ? Saxon.Hej.value.EmptySequence.getInstance() : new Saxon.Hej.value.AnyURIValue(uri)));
            fn.setDetails(getDetails());
            fn.setRetainedStaticContext(getRetainedStaticContext());
            return fn;
        }
        public Saxon.Hej.value.AnyURIValue evaluateItem(Saxon.Hej.expr.XPathContext context) {
            string uri = context.getCurrentOutputUri();
            return ((uri == null) ? null : new Saxon.Hej.value.AnyURIValue(uri));
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return Saxon.Hej.om.SequenceTool.itemOrEmpty(evaluateItem(context));
        }
    
    }
}