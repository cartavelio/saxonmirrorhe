using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public class SingletonClosure : Saxon.Hej.value.Closure, Saxon.Hej.om.Sequence {
        private bool built = false;
        private Saxon.Hej.om.Item value = null;
        public SingletonClosure(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.XPathContext context)  {
            expression = exp;
            savedXPathContext = context.newContext();
            saveContext(exp, context);
        }
        public override Saxon.Hej.om.SequenceIterator iterate() {
            try {
                return Saxon.Hej.tree.iter.SingletonIterator.makeIterator(asItem());
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.trans.UncheckedXPathException(e);
            }
        }
        public Saxon.Hej.om.Item asItem() {
            if (!(built)) {
                value = expression.evaluateItem(savedXPathContext);
                built = true;
                savedXPathContext = null;
            }
            return value;
        }
        public Saxon.Hej.om.Item itemAt(int n) {
            if ((n != 0)) {
                return null;
            }
            return asItem();
        }
        public int getLength() {
            return ((asItem() == null) ? 0 : 1);
        }
        public override  /* REPLACING public */ Saxon.Hej.om.GroundedValue materialize() {
            try {
                return Saxon.Hej.om.SequenceTool.itemOrEmpty(asItem());
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.trans.UncheckedXPathException(e);
            }
        }
        public override Saxon.Hej.value.SingletonClosure makeRepeatable() {
            return this;
        }
    
    }
}