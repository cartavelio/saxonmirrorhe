using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class TraceExpression : Saxon.Hej.expr.instruct.Instruction {
        private readonly Saxon.Hej.expr.Operand baseOp;
        private System.Collections.Generic.Dictionary<string,System.Object> properties = new System.Collections.Generic.Dictionary<string, System.Object>(10);
        public TraceExpression(Saxon.Hej.expr.Expression child)  {
            baseOp = new Saxon.Hej.expr.Operand(this, child, Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION);
            adoptChildExpression(child);
            child.gatherProperties((k, v) => Saxon.Impl.Helpers.MapUtils.Put(properties, k, v));
        }
        public Saxon.Hej.expr.Expression getChild() {
            return baseOp.getChildExpression();
        }
        public Saxon.Hej.expr.Expression getBody() {
            return baseOp.getChildExpression();
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return baseOp;
        }
        public void setProperty(string name, System.Object value) {
            Saxon.Impl.Helpers.MapUtils.Put(properties, name, value);
        }
        public override System.Object getProperty(string name) {
            return Saxon.Impl.Helpers.MapUtils.Get(properties, name);
        }
        public override System.Collections.Generic.IEnumerator<string> getProperties() {
            return Saxon.Impl.Helpers.MapUtils.KeySet(properties).GetEnumerator();
        }
        public override string getExpressionName() {
            return "trace";
        }
        public override string getStreamerName() {
            return "TraceExpr";
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.TraceExpression t = new Saxon.Hej.expr.instruct.TraceExpression(getChild().copy(rebindings));
            t.setLocation(getLocation());
            t.properties = properties;
            return t;
        }
        public override bool isUpdatingExpression() {
            return getChild().isUpdatingExpression();
        }
        public override bool isVacuousExpression() {
            return getChild().isVacuousExpression();
        }
        public override void checkForUpdatingSubexpressions() {
            getChild().checkForUpdatingSubexpressions();
        }
        public override int getImplementationMethod() {
            return getChild().getImplementationMethod();
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return getChild().getItemType();
        }
        public override int getCardinality() {
            return getChild().getCardinality();
        }
        public override int getDependencies() {
            return getChild().getDependencies();
        }
        public override bool mayCreateNewNodes() {
            return !(getChild().hasSpecialProperty(Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED));
        }
        public override int getNetCost() {
            return 0;
        }
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
            return /*Delegate.eval()*/makeElaborator().elaborateForItem()(context);
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            return /*Delegate.iterate()*/makeElaborator().elaborateForPull()(context);
        }
        public override int getInstructionNameCode() {
            if ((getChild() is Saxon.Hej.expr.instruct.Instruction)) {
                return ((Saxon.Hej.expr.instruct.Instruction)(getChild())).getInstructionNameCode();
            } else {
                return -1;
            }
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            getChild().export(@out);
        }
        public override string toShortString() {
            return getChild().toShortString();
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.instruct.TraceExpression.TraceExpressionElaborator();
        }
        private class TraceExpressionElaborator : Saxon.Hej.expr.elab.FallbackElaborator {
            public override Saxon.Hej.expr.elab.StringEvaluator elaborateForString(bool zeroLengthWhenAbsent) {
                Saxon.Hej.expr.instruct.TraceExpression expr = (Saxon.Hej.expr.instruct.TraceExpression)(getExpression());
                Saxon.Hej.expr.elab.StringEvaluator baseEval = expr.getBody().makeElaborator().elaborateForString(false);
                return context => {
                    Saxon.Hej.Controller controller = context.getController();
                    System.Diagnostics.Debug.Assert((controller != null));
                    if (controller.isTracing()) {
                        Saxon.Hej.lib.TraceListener listener = controller.getTraceListener();
                        listener.enter(expr, expr.properties, context);
                        string result = /*Delegate.eval()*/baseEval(context);
                        listener.leave(expr);
                        return result;
                    } else {
                        return /*Delegate.eval()*/baseEval(context);
                    }
                } ;
            }
            public override Saxon.Hej.expr.elab.UpdateEvaluator elaborateForUpdate() {
                Saxon.Hej.expr.instruct.TraceExpression expr = (Saxon.Hej.expr.instruct.TraceExpression)(getExpression());
                Saxon.Hej.expr.elab.UpdateEvaluator baseEval = expr.getBody().makeElaborator().elaborateForUpdate();
                return (context, pul) => {
                    Saxon.Hej.Controller controller = context.getController();
                    System.Diagnostics.Debug.Assert((controller != null));
                    if (controller.isTracing()) {
                        Saxon.Hej.lib.TraceListener listener = controller.getTraceListener();
                        listener.enter(expr, expr.properties, context);
                        /*Delegate.registerUpdates()*/baseEval(context, pul);
                        listener.leave(expr);
                    } else {
                        /*Delegate.registerUpdates()*/baseEval(context, pul);
                    }
                } ;
            }
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.expr.instruct.TraceExpression expr = (Saxon.Hej.expr.instruct.TraceExpression)(getExpression());
                Saxon.Hej.expr.elab.PullEvaluator baseEval = expr.getBody().makeElaborator().elaborateForPull();
                return context => {
                    Saxon.Hej.Controller controller = context.getController();
                    System.Diagnostics.Debug.Assert((controller != null));
                    if (controller.isTracing()) {
                        Saxon.Hej.lib.TraceListener listener = controller.getTraceListener();
                        listener.enter(expr, expr.properties, context);
                        Saxon.Hej.om.SequenceIterator result = /*Delegate.iterate()*/baseEval(context);
                        listener.leave(expr);
                        return result;
                    } else {
                        return /*Delegate.iterate()*/baseEval(context);
                    }
                } ;
            }
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.instruct.TraceExpression expr = (Saxon.Hej.expr.instruct.TraceExpression)(getExpression());
                Saxon.Hej.expr.Expression body = expr.getBody();
                Saxon.Hej.expr.elab.PushEvaluator baseEval = body.makeElaborator().elaborateForPush();
                return (output, context) => {
                    Saxon.Hej.Controller controller = context.getController();
                    System.Diagnostics.Debug.Assert((controller != null));
                    if (controller.isTracing()) {
                        Saxon.Hej.lib.TraceListener listener = controller.getTraceListener();
                        listener.enter(body, expr.properties, context);
                        Saxon.Hej.expr.instruct.TailCall tc = /*Delegate.processLeavingTail()*/baseEval(output, context);
                        dispatchTailCall(tc);
                        listener.leave(body);
                    } else {
                        dispatchTailCall(/*Delegate.processLeavingTail()*/baseEval(output, context));
                    }
                    return null;
                } ;
            }
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.instruct.TraceExpression expr = (Saxon.Hej.expr.instruct.TraceExpression)(getExpression());
                Saxon.Hej.expr.elab.ItemEvaluator baseEval = expr.getBody().makeElaborator().elaborateForItem();
                return context => {
                    Saxon.Hej.Controller controller = context.getController();
                    System.Diagnostics.Debug.Assert((controller != null));
                    if (controller.isTracing()) {
                        Saxon.Hej.lib.TraceListener listener = controller.getTraceListener();
                        listener.enter(expr, expr.properties, context);
                        Saxon.Hej.om.Item result = /*Delegate.eval()*/baseEval(context);
                        listener.leave(expr);
                        return result;
                    } else {
                        return /*Delegate.eval()*/baseEval(context);
                    }
                } ;
            }
            public override Saxon.Hej.expr.elab.BooleanEvaluator elaborateForBoolean() {
                Saxon.Hej.expr.instruct.TraceExpression expr = (Saxon.Hej.expr.instruct.TraceExpression)(getExpression());
                Saxon.Hej.expr.elab.BooleanEvaluator baseEval = expr.getBody().makeElaborator().elaborateForBoolean();
                return context => {
                    Saxon.Hej.Controller controller = context.getController();
                    System.Diagnostics.Debug.Assert((controller != null));
                    if (controller.isTracing()) {
                        Saxon.Hej.lib.TraceListener listener = controller.getTraceListener();
                        listener.enter(expr, expr.properties, context);
                        bool result = /*Delegate.eval()*/baseEval(context);
                        listener.leave(expr);
                        return result;
                    } else {
                        return /*Delegate.eval()*/baseEval(context);
                    }
                } ;
            }
            public override Saxon.Hej.expr.elab.UnicodeStringEvaluator elaborateForUnicodeString(bool zeroLengthWhenAbsent) {
                Saxon.Hej.expr.instruct.TraceExpression expr = (Saxon.Hej.expr.instruct.TraceExpression)(getExpression());
                Saxon.Hej.expr.elab.UnicodeStringEvaluator baseEval = expr.getBody().makeElaborator().elaborateForUnicodeString(zeroLengthWhenAbsent);
                return context => {
                    Saxon.Hej.Controller controller = context.getController();
                    System.Diagnostics.Debug.Assert((controller != null));
                    if (controller.isTracing()) {
                        Saxon.Hej.lib.TraceListener listener = controller.getTraceListener();
                        listener.enter(expr, expr.properties, context);
                        Saxon.Hej.str.UnicodeString result = /*Delegate.eval()*/baseEval(context);
                        listener.leave(expr);
                        return result;
                    } else {
                        return /*Delegate.eval()*/baseEval(context);
                    }
                } ;
            }
        
        }
    
    }
}