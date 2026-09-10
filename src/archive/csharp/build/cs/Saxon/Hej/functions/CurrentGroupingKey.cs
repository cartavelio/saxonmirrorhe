using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class CurrentGroupingKey : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.expr.Expression makeFunctionCall(params Saxon.Hej.expr.Expression[]  arguments) {
            return new Saxon.Hej.functions.CurrentGroupingKeyCall();
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            throw new Saxon.Hej.trans.XPathException("Dynamic call on current-grouping-key() fails (the current group is absent)", "XTDE1071");
        }
    
    }
}