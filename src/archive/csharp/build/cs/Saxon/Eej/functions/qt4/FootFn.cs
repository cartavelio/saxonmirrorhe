using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class FootFn : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            try {
                Saxon.Hej.om.Item foot = Saxon.Hej.expr.LastItemExpression.getLast(arguments[0].iterate());
                return Saxon.Hej.om.SequenceTool.itemOrEmpty(foot);
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw e.getXPathException();
            }
        }
    
    }
}