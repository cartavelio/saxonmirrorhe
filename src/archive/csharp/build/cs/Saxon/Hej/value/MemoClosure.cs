using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public class MemoClosure : Saxon.Hej.value.Closure, Saxon.Hej.expr.ContextOriginator {
        private Saxon.Hej.om.Sequence sequence;
        public MemoClosure()  {
        }
        public MemoClosure(Saxon.Hej.expr.Expression expr, Saxon.Hej.expr.XPathContext context)  {
            setExpression(expr);
            Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
            c2.setOrigin(this);
            setSavedXPathContext(c2);
            saveContext(expr, context);
        }
        public override Saxon.Hej.om.SequenceIterator iterate() { lock(this) {
            try {
                makeSequence();
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.trans.UncheckedXPathException(e);
            }
            return sequence.iterate();
        }}
        private void makeSequence() {
            if ((sequence == null)) {
                inputIterator = expression.iterate(savedXPathContext);
                inputIterator.discharge();
                if (((inputIterator is Saxon.Hej.tree.iter.GroundedIterator) && ((Saxon.Hej.tree.iter.GroundedIterator)(inputIterator)).isActuallyGrounded())) {
                    sequence = ((Saxon.Hej.tree.iter.GroundedIterator)(inputIterator)).materialize();
                } else {
                    sequence = Saxon.Hej.om.SequenceTool.toMemoSequence(inputIterator);
                }
            }
        }
        public Saxon.Hej.om.Item itemAt(int n) { lock(this) {
            makeSequence();
            if ((sequence is Saxon.Hej.om.GroundedValue)) {
                return ((Saxon.Hej.om.GroundedValue)(sequence)).itemAt(n);
            } else {
                if ((sequence is Saxon.Hej.om.MemoSequence)) {
                    return ((Saxon.Hej.om.MemoSequence)(sequence)).itemAt(n);
                } else {
                    throw new Saxon.Ejava.lang.IllegalStateException();
                }
            }
        }}
        public override Saxon.Hej.om.GroundedValue reduce() {
            try {
                if ((sequence is Saxon.Hej.om.GroundedValue)) {
                    return (Saxon.Hej.om.GroundedValue)(sequence);
                } else {
                    return Saxon.Hej.om.SequenceTool.toGroundedValue(iterate());
                }
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw e.getXPathException();
            }
        }
        public override Saxon.Hej.om.Sequence makeRepeatable() {
            return this;
        }
    
    }
}