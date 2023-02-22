using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.hof {
    public class CallableWithBoundFocus : Saxon.Hej.expr.Callable {
        private readonly Saxon.Hej.expr.Callable target;
        private readonly Saxon.Hej.expr.XPathContext boundContext;
        public CallableWithBoundFocus(Saxon.Hej.expr.Callable target, Saxon.Hej.expr.XPathContext context)  {
            this.target = target;
            boundContext = context.newContext();
            if ((context.getCurrentIterator() == null)) {
                boundContext.setCurrentIterator(null);
            } else {
                Saxon.Hej.tree.iter.ManualIterator iter = new Saxon.Hej.tree.iter.ManualIterator(context.getContextItem(), context.getCurrentIterator().position());
                iter.setLengthFinder(context.getLast /* instance method reference */);
                boundContext.setCurrentIterator(iter);
            }
        }
        public Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return target.call(boundContext, arguments);
        }
    
    }
}