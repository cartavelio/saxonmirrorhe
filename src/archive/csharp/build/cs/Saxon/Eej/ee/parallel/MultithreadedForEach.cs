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
            return /*Delegate.iterate()*/makeElaborator().elaborateForPull()(context);
        }
        public override string getExpressionName() {
            return "multithreadedForEach";
        }
        protected internal override void explainThreads(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.setChildRole("threads");
            getThreads().export(@out);
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Eej.ee.parallel.MultithreadedForEach.MultithreadedForEachElaborator();
        }
        public class MultithreadedForEachElaborator : Saxon.Hej.expr.elab.PullElaborator {
            private Saxon.Hej.expr.elab.StringEvaluator threadsEvaluator;
            private void makeThreadsEvaluator(Saxon.Eej.ee.parallel.MultithreadedForEach forEach) {
                if ((forEach.threadsOp == null)) {
                    threadsEvaluator = context => "1";
                } else {
                    threadsEvaluator = forEach.getThreads().makeElaborator().elaborateForString(true);
                }
            }
            private int evaluateThreads(Saxon.Eej.ee.parallel.MultithreadedForEach forEach, Saxon.Hej.expr.XPathContext context) {
                if ((forEach.threadsOp == null)) {
                    return 1;
                } else {
                    string threadsValue = /*Delegate.eval()*/threadsEvaluator(context);
                    int threads;
                    try {
                        threads = Saxon.Impl.Helpers.Utils.ParseInt(threadsValue);
                    } catch (Saxon.Ejava.lang.NumberFormatException e) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Value of 'threads' is not an integer ("+threadsValue)+")"));
                        err.setLocation(forEach.getLocation());
                        err.setXPathContext(context);
                        throw err;
                    }
                    return threads;
                }
            }
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Eej.ee.parallel.MultithreadedForEach forEach = (Saxon.Eej.ee.parallel.MultithreadedForEach)(getExpression());
                Saxon.Hej.expr.elab.PullEvaluator select = forEach.getSelectExpression().makeElaborator().elaborateForPull();
                Saxon.Hej.expr.elab.PullEvaluator action = forEach.getActionExpression().makeElaborator().elaborateForPull();
                makeThreadsEvaluator(forEach);
                return context => {
                    int threads = evaluateThreads(forEach, context);
                    Saxon.Hej.expr.XPathContextMinor c2 = context.newMinorContext();
                    c2.trackFocus(/*Delegate.iterate()*/select(context));
                    return new Saxon.Impl.Parallel.MultithreadedContextMappingIterator(action, c2, threads);
                } ;
            }
            public override Saxon.Hej.expr.elab.UpdateEvaluator elaborateForUpdate() {
                throw new Saxon.Ejava.lang.UnsupportedOperationException("Multi-threading is not supported for updating expressions");
            }
        
        }
    
    }
}