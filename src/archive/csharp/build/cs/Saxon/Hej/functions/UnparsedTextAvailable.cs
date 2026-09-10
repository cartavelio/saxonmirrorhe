using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class UnparsedTextAvailable : Saxon.Hej.functions.UnparsedTextFunction, Saxon.Hej.expr.Callable {
        public override Saxon.Hej.value.BooleanValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.StringValue hrefVal = (Saxon.Hej.value.StringValue)(arguments[0].head());
            if ((hrefVal == null)) {
                return Saxon.Hej.value.BooleanValue.FALSE;
            }
            string encoding = ((getArity() == 2) ? arguments[1].head().getStringValue() : null);
            return Saxon.Hej.value.BooleanValue.get(evalUnparsedTextAvailable(hrefVal, encoding, context));
        }
        public bool evalUnparsedTextAvailable(Saxon.Hej.value.StringValue hrefVal, string encoding, Saxon.Hej.expr.XPathContext context) {
            try {
                Saxon.Hej.functions.UnparsedText.evalUnparsedText(hrefVal, getStaticBaseUriString(), encoding, context);
                return true;
            } catch (Saxon.Hej.trans.XPathException err) {
                return false;
            }
        }
    
    }
}