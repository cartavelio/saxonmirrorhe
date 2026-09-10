using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public abstract class Closure : Saxon.Hej.om.Sequence, Saxon.Hej.expr.ContextOriginator {
        protected internal Saxon.Hej.expr.elab.PullEvaluator inputEvaluator;
        protected internal Saxon.Hej.expr.XPathContextMajor savedXPathContext;
        protected internal int depth = 0;
        protected internal Saxon.Hej.expr.elab.LearningEvaluator learningEvaluator;
        protected internal int serialNumber;
        protected internal Saxon.Hej.om.SequenceIterator inputIterator;
        public Closure()  {
        }
        public void saveContext(Saxon.Hej.expr.Expression expression, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.trace.Instrumentation.count("Closure.saveContext()");
            if ((((expression.getDependencies()&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_LOCAL_VARIABLES)) != 0)) {
                Saxon.Hej.expr.StackFrame localStackFrame = context.getStackFrame();
                Saxon.Hej.om.Sequence[] local = localStackFrame.getStackFrameValues();
                int[] slotsUsed = expression.getSlotsUsed();
                if ((local != null)) {
                    Saxon.Hej.expr.instruct.SlotManager stackFrameMap = localStackFrame.getStackFrameMap();
                    Saxon.Hej.om.Sequence[] savedStackFrame = new Saxon.Hej.om.Sequence[stackFrameMap.getNumberOfVariables()];
                    foreach (int i in slotsUsed) {
                        if ((local[i] is Saxon.Hej.value.Closure)) {
                            int cdepth = ((Saxon.Hej.value.Closure)(local[i])).depth;
                            if ((cdepth>=10)) {
                                local[i] = Saxon.Hej.om.SequenceTool.toGroundedValue(local[i].iterate());
                            } else {
                                if (((cdepth+1)>depth)) {
                                    depth = (cdepth+1);
                                }
                            }
                        }
                        savedStackFrame[i] = local[i];
                    }
                    savedXPathContext.setStackFrame(stackFrameMap, savedStackFrame);
                }
            }
            Saxon.Hej.om.FocusIterator currentIterator = context.getCurrentIterator();
            if ((currentIterator != null)) {
                Saxon.Hej.om.Item contextItem = currentIterator.current();
                Saxon.Hej.tree.iter.ManualIterator single = new Saxon.Hej.tree.iter.ManualIterator(contextItem);
                savedXPathContext.setCurrentIterator(single);
            }
        }
        public void setLearningEvaluator(Saxon.Hej.expr.elab.LearningEvaluator learningEvaluator, int serialNumber) {
            this.learningEvaluator = learningEvaluator;
            this.serialNumber = serialNumber;
        }
        public virtual Saxon.Hej.om.Item head() {
            try {
                return iterate().next();
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw e.getXPathException();
            }
        }
        public Saxon.Hej.expr.XPathContextMajor getSavedXPathContext() {
            return savedXPathContext;
        }
        public void setInputEvaluator(Saxon.Hej.expr.elab.PullEvaluator inputEvaluator) {
            this.inputEvaluator = inputEvaluator;
        }
        public void setSavedXPathContext(Saxon.Hej.expr.XPathContextMajor savedXPathContext) {
            this.savedXPathContext = savedXPathContext;
        }
        public abstract Saxon.Hej.om.SequenceIterator iterate();
        public virtual Saxon.Hej.om.GroundedValue reduce() {
            return Saxon.Hej.om.SequenceTool.toGroundedValue(iterate());
        }
        public virtual Saxon.Hej.om.Sequence makeRepeatable() {
            return materialize();
        }
    
        /* Expanding default method materialize from interface Sequence */
        public virtual Saxon.Hej.om.GroundedValue materialize() {
            return Saxon.Hej.om.SequenceTool.toGroundedValue(iterate());
        }

    }
}