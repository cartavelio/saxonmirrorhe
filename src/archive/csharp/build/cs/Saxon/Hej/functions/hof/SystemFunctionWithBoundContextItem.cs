using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.hof {
    public class SystemFunctionWithBoundContextItem : Saxon.Hej.functions.AbstractFunction {
        private readonly Saxon.Hej.functions.SystemFunction target;
        private readonly Saxon.Hej.om.Item contextItem;
        public SystemFunctionWithBoundContextItem(Saxon.Hej.functions.SystemFunction target, Saxon.Hej.expr.XPathContext context)  {
            this.target = target;
            Saxon.Hej.om.Item contextItem = context.getContextItem();
            if (((contextItem is Saxon.Hej.om.NodeInfo) && contextItem.isStreamed())) {
                contextItem = null;
            }
            this.contextItem = contextItem;
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.expr.XPathContext c2 = context.newMinorContext();
            c2.setCurrentIterator(new Saxon.Hej.tree.iter.ManualIterator(contextItem));
            return target.call(c2, arguments);
        }
        public override int getArity() {
            return target.getArity();
        }
        public override Saxon.Hej.type.FunctionItemType getFunctionItemType() {
            return target.getFunctionItemType();
        }
        public override Saxon.Hej.om.StructuredQName getFunctionName() {
            return target.getFunctionName();
        }
        public override string getDescription() {
            return target.getDescription();
        }
    
    }
}