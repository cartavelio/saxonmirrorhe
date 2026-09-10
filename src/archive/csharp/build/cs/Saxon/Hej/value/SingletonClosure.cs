using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public class SingletonClosure : Saxon.Hej.value.Closure, Saxon.Hej.om.Sequence {
        private bool built = false;
        private Saxon.Hej.om.Item value = null;
        public SingletonClosure(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.elab.PullEvaluator inputEvaluator, Saxon.Hej.expr.XPathContext context)  {
            setInputEvaluator(inputEvaluator);
            savedXPathContext = context.newContext();
            saveContext(exp, context);
            Saxon.Hej.trace.Instrumentation.count("SingletonClosure.new()");
        }
        public SingletonClosure(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.XPathContext context)  {
            setInputEvaluator(exp.makeElaborator().elaborateForPull());
            savedXPathContext = context.newContext();
            saveContext(exp, context);
            Saxon.Hej.trace.Instrumentation.count("SingletonClosure.new()");
        }
        public override Saxon.Hej.om.SequenceIterator iterate() {
            try {
                Saxon.Hej.om.Item item = asItem();
                if ((item == null)) {
                    return Saxon.Hej.tree.iter.EmptyIterator.getInstance();
                } else {
                    if ((learningEvaluator != null)) {
                        return new Saxon.Hej.tree.iter.ReportingSingletonIterator(item, learningEvaluator, serialNumber);
                    } else {
                        return new Saxon.Hej.tree.iter.SingletonIterator(item);
                    }
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.trans.UncheckedXPathException(e);
            }
        }
        public override Saxon.Hej.om.Item head() {
            try {
                return asItem();
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw e.getXPathException();
            }
        }
        public Saxon.Hej.om.Item asItem() {
            if (!(built)) {
                value = /*Delegate.iterate()*/inputEvaluator(savedXPathContext).next();
                built = true;
                savedXPathContext = null;
                if ((learningEvaluator != null)) {
                    learningEvaluator.reportCompletion(serialNumber);
                    Saxon.Hej.trace.Instrumentation.count("SingletonClosure.reportCompletion()");
                    learningEvaluator = null;
                }
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
        public override  /* REPLACING public override */ Saxon.Hej.om.GroundedValue materialize() {
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