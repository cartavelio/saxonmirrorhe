using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class ComponentTracer : Saxon.Hej.expr.instruct.Instruction {
        private Saxon.Hej.expr.Operand baseOp;
        private System.Collections.Generic.Dictionary<string,System.Object> properties = new System.Collections.Generic.Dictionary<string, System.Object>(10);
        private Saxon.Hej.trace.TraceableComponent component;
        public ComponentTracer(Saxon.Hej.trace.TraceableComponent component)  {
            this.component = component;
            baseOp = new Saxon.Hej.expr.Operand(this, component.getBody(), Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION);
            component.gatherProperties((k, v) => Saxon.Impl.Helpers.MapUtils.Put(properties, k, v));
        }
        private ComponentTracer()  {
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
        public override string getExpressionName() {
            return "trace";
        }
        public override string getStreamerName() {
            return "TraceExpr";
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.ComponentTracer t = new Saxon.Hej.expr.instruct.ComponentTracer();
            t.component = component;
            t.properties = properties;
            Saxon.Hej.expr.Expression newBody = getChild().copy(rebindings);
            t.baseOp = new Saxon.Hej.expr.Operand(t, newBody, Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION);
            t.adoptChildExpression(newBody);
            t.setLocation(getLocation());
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
            return new Saxon.Hej.expr.instruct.ComponentTracer.ComponentTracerElaborator();
        }
        private class ComponentTracerElaborator : Saxon.Hej.expr.elab.PullElaborator {
            public override Saxon.Hej.expr.elab.UpdateEvaluator elaborateForUpdate() {
                Saxon.Hej.expr.instruct.ComponentTracer expr = (Saxon.Hej.expr.instruct.ComponentTracer)(getExpression());
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
                Saxon.Hej.expr.instruct.ComponentTracer expr = (Saxon.Hej.expr.instruct.ComponentTracer)(getExpression());
                Saxon.Hej.expr.elab.PullEvaluator baseEval = expr.getBody().makeElaborator().elaborateForPull();
                return context => {
                    Saxon.Hej.Controller controller = context.getController();
                    System.Diagnostics.Debug.Assert((controller != null));
                    if (controller.isTracing()) {
                        Saxon.Hej.lib.TraceListener listener = controller.getTraceListener();
                        listener.enter(expr.component, expr.properties, context);
                        Saxon.Hej.om.SequenceIterator result = /*Delegate.iterate()*/baseEval(context);
                        Saxon.Hej.om.GroundedValue extent = Saxon.Hej.om.SequenceTool.toGroundedValue(result);
                        listener.leave(expr.component);
                        return extent.iterate();
                    } else {
                        return /*Delegate.iterate()*/baseEval(context);
                    }
                } ;
            }
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.instruct.ComponentTracer expr = (Saxon.Hej.expr.instruct.ComponentTracer)(getExpression());
                Saxon.Hej.expr.elab.PushEvaluator baseEval = expr.getBody().makeElaborator().elaborateForPush();
                return (output, context) => {
                    Saxon.Hej.Controller controller = context.getController();
                    System.Diagnostics.Debug.Assert((controller != null));
                    if (controller.isTracing()) {
                        Saxon.Hej.lib.TraceListener listener = controller.getTraceListener();
                        listener.enter(expr.component, expr.properties, context);
                        Saxon.Hej.expr.instruct.TailCall tc = /*Delegate.processLeavingTail()*/baseEval(output, context);
                        dispatchTailCall(tc);
                        listener.leave(expr.component);
                    } else {
                        dispatchTailCall(/*Delegate.processLeavingTail()*/baseEval(output, context));
                    }
                    return null;
                } ;
            }
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.instruct.ComponentTracer expr = (Saxon.Hej.expr.instruct.ComponentTracer)(getExpression());
                Saxon.Hej.expr.elab.ItemEvaluator baseEval = expr.getBody().makeElaborator().elaborateForItem();
                return context => {
                    Saxon.Hej.Controller controller = context.getController();
                    System.Diagnostics.Debug.Assert((controller != null));
                    if (controller.isTracing()) {
                        Saxon.Hej.lib.TraceListener listener = controller.getTraceListener();
                        listener.enter(expr.component, expr.properties, context);
                        Saxon.Hej.om.Item result = /*Delegate.eval()*/baseEval(context);
                        listener.leave(expr.component);
                        return result;
                    } else {
                        return /*Delegate.eval()*/baseEval(context);
                    }
                } ;
            }
        
        }
    
    }
}