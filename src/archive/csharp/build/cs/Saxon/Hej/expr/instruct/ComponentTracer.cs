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
        public override Saxon.Hej.expr.instruct.TailCall processLeavingTail(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.Controller controller = context.getController();
            System.Diagnostics.Debug.Assert((controller != null));
            Saxon.Hej.lib.TraceListener listener = controller.getTraceListener();
            if (controller.isTracing()) {
                System.Diagnostics.Debug.Assert((listener != null));
                listener.enter(component, properties, context);
                getChild().process(output, context);
                listener.leave(component);
            } else {
                getChild().process(output, context);
            }
            return null;
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
            Saxon.Hej.Controller controller = context.getController();
            System.Diagnostics.Debug.Assert((controller != null));
            if (controller.isTracing()) {
                Saxon.Hej.lib.TraceListener listener = controller.getTraceListener();
                listener.enter(component, properties, context);
                Saxon.Hej.om.Item result = getChild().evaluateItem(context);
                listener.leave(component);
                return result;
            } else {
                return getChild().evaluateItem(context);
            }
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.Controller controller = context.getController();
            System.Diagnostics.Debug.Assert((controller != null));
            if (controller.isTracing()) {
                Saxon.Hej.lib.TraceListener listener = controller.getTraceListener();
                listener.enter(component, properties, context);
                Saxon.Hej.om.SequenceIterator result = getChild().iterate(context);
                listener.leave(component);
                return result;
            } else {
                return getChild().iterate(context);
            }
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
        public override void evaluatePendingUpdates(Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.PendingUpdateList pul) {
            Saxon.Hej.Controller controller = context.getController();
            System.Diagnostics.Debug.Assert((controller != null));
            if (controller.isTracing()) {
                Saxon.Hej.lib.TraceListener listener = controller.getTraceListener();
                listener.enter(component, properties, context);
                getChild().evaluatePendingUpdates(context, pul);
                listener.leave(component);
            } else {
                getChild().evaluatePendingUpdates(context, pul);
            }
        }
        public override string toShortString() {
            return getChild().toShortString();
        }
    
    }
}