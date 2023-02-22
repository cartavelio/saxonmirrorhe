using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.parallel {
    public class MultithreadedForEach : Saxon.Hej.expr.instruct.ForEach {
        public MultithreadedForEach(Saxon.Hej.expr.Expression select, Saxon.Hej.expr.Expression action, bool containsTailCall, Saxon.Hej.expr.Expression threads) : base (select, action, false, threads) {
        }
        public override bool isMultiThreaded(Saxon.Hej.Configuration config) {
            return true;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.expr.Expression e2 = base.optimize(visitor, contextInfo);
            if ((e2 != this)) {
                return e2;
            }
            Saxon.Hej.expr.Expression e = getParentExpression();
            while ((e != null)) {
                if (((e is Saxon.Hej.expr.LetExpression) && Saxon.Hej.expr.parser.ExpressionTool.dependsOnVariable(getAction(), new Saxon.Hej.expr.Binding[]{(Saxon.Hej.expr.LetExpression)(e)}))) {
                    ((Saxon.Hej.expr.LetExpression)(e)).setNeedsEagerEvaluation(true);
                }
                e = e.getParentExpression();
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Eej.ee.parallel.MultithreadedForEach result = new Saxon.Eej.ee.parallel.MultithreadedForEach(getSelect().copy(rebindings), getAction().copy(rebindings), containsTailCall, getThreads());
            result.setInstruction(isInstruction());
            return result;
        }
        public override int getImplementationMethod() {
            return PROCESS_METHOD;
        }
        public override Saxon.Hej.expr.instruct.TailCall processLeavingTail(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
            Saxon.Hej.om.FocusIterator fit = c2.getController().getFocusTrackerFactory(true)(getSelect().iterate(context));
            c2.setCurrentIterator(fit);
            int maxThreads = evaluateThreads(context);
            if ((maxThreads<=1)) {
                return base.processLeavingTail(output, context);
            } else {
                Saxon.Impl.Parallel.MultithreadedContextMappingIterator mapper = new Saxon.Impl.Parallel.MultithreadedContextMappingIterator(getAction(), c2, maxThreads);
                Saxon.Hej.om.SequenceTool.supply(mapper, (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)(it => output.append(it) /* converted from instance method reference */));
                mapper.close();
                return null;
            }
        }
        private int evaluateThreads(Saxon.Hej.expr.XPathContext context) {
            if ((threadsOp == null)) {
                return 1;
            } else {
                string threadsValue = getThreads().evaluateAsString(context).ToString();
                int threads = 0;
                try {
                    threads = Saxon.Impl.Helpers.Utils.ParseInt(threadsValue.ToString());
                } catch (Saxon.Ejava.lang.NumberFormatException e) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Value of 'threads' is not an integer ("+threadsValue)+")"));
                    err.setLocation(getLocation());
                    err.setXPathContext(context);
                    throw err;
                }
                return threads;
            }
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            int threads = evaluateThreads(context);
            Saxon.Hej.expr.XPathContextMinor c2 = context.newMinorContext();
            c2.trackFocus(getSelect().iterate(context));
            return new Saxon.Impl.Parallel.MultithreadedContextMappingIterator(getAction(), c2, threads);
        }
        public override string getExpressionName() {
            return "multithreadedForEach";
        }
        protected internal override void explainThreads(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.setChildRole("threads");
            getThreads().export(@out);
        }
    
    }
}