using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class ExpressionFn : Saxon.Eej.functions.extfn.Evaluate {
        public override Saxon.Hej.value.ObjectValue<Saxon.Eej.functions.extfn.Evaluate.PreparedExpression> call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.expr.XPathContextMajor c2 = context.newCleanContext();
            string exprText = arguments[0].head().getStringValue();
            Saxon.Hej.om.NodeInfo namespaceOrigin = null;
            if ((arguments.Length>1)) {
                namespaceOrigin = (Saxon.Hej.om.NodeInfo)(arguments[1].head());
            }
            Saxon.Eej.functions.extfn.Evaluate.PreparedExpression pexpr = prepareExpression(exprText, namespaceOrigin, getStaticBaseUriString(), "saxon:expression", c2);
            return new Saxon.Hej.value.ObjectValue<Saxon.Eej.functions.extfn.Evaluate.PreparedExpression>(pexpr);
        }
    
    }
}