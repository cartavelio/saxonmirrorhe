using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class CurrentOutputUri : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {
        public override int getSpecialProperties(Saxon.Hej.expr.Expression[] arguments) {
            return (base.getSpecialProperties(arguments)|Saxon.Hej.expr.StaticProperty.HAS_SIDE_EFFECTS);
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