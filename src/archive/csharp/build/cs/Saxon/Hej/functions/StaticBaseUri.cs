using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class StaticBaseUri : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.value.AnyURIValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] args) {
            return new Saxon.Hej.value.AnyURIValue(getRetainedStaticContext().getStaticBaseUriString());
        }
        public override Saxon.Hej.expr.Expression makeFunctionCall(Saxon.Hej.expr.Expression[] arguments) {
            Saxon.Hej.expr.PackageData pd = getRetainedStaticContext().getPackageData();
            if (pd.isRelocatable()) {
                return base.makeFunctionCall(arguments);
            } else {
                return Saxon.Hej.expr.Literal.makeLiteral(new Saxon.Hej.value.AnyURIValue(getRetainedStaticContext().getStaticBaseUriString()));
            }
        }
    
    }
}