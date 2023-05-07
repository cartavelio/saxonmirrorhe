using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class CurrentGroup : Saxon.Hej.functions.ContextAccessorFunction {
        public override Saxon.Hej.expr.Expression makeFunctionCall(params Saxon.Hej.expr.Expression[]  arguments) {
            return new Saxon.Hej.functions.CurrentGroupCall();
        }
        public override Saxon.Hej.om.FunctionItem bindContext(Saxon.Hej.expr.XPathContext context) {
            if ((getRetainedStaticContext().getPackageData().getHostLanguageVersion()<40)) {
                throw new Saxon.Hej.trans.XPathException("Dynamic call on current-group() fails (the current group is absent)", "XTDE1061");
            }
            Saxon.Hej.expr.sort.GroupIterator gi = context.getCurrentGroupIterator();
            if ((gi == null)) {
                throw new Saxon.Hej.trans.XPathException("There is no current group", "XTDE1061");
            }
            Saxon.Hej.om.GroundedValue group = gi.currentGroup();
            Saxon.Hej.functions.ConstantFunction fn = new Saxon.Hej.functions.ConstantFunction(group);
            fn.setDetails(getDetails());
            fn.setRetainedStaticContext(getRetainedStaticContext());
            return fn;
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            throw new Saxon.Hej.trans.XPathException("Dynamic call on current-group() fails (the current group is absent)", "XTDE1061");
        }
    
    }
}