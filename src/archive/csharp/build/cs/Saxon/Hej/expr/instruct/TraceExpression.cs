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
        public override Saxon.Hej.expr.instruct.TailCall processLeavingTail(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.Controller controller = context.getController();
            System.Diagnostics.Debug.Assert((controller != null));
            Saxon.Hej.lib.TraceListener listener = controller.getTraceListener();
            Saxon.Hej.expr.Expression child = getChild();
            if (controller.isTracing()) {
                System.Diagnostics.Debug.Assert((listener != null));
                listener.enter(child, properties, context);
                child.process(output, context);
                listener.leave(child);
            } else {
                child.process(output, context);
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
            Saxon.Hej.expr.Expression child = getChild();
            if (controller.isTracing()) {
                Saxon.Hej.lib.TraceListener listener = controller.getTraceListener();
                listener.enter(child, properties, context);
                Saxon.Hej.om.Item result = child.evaluateItem(context);
                listener.leave(child);
                return result;
            } else {
                return child.evaluateItem(context);
            }
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.Controller controller = context.getController();
            System.Diagnostics.Debug.Assert((controller != null));
            Saxon.Hej.expr.Expression child = getChild();
            if (controller.isTracing()) {
                Saxon.Hej.lib.TraceListener listener = controller.getTraceListener();
                listener.enter(child, properties, context);
                Saxon.Hej.om.SequenceIterator result = child.iterate(context);
                listener.leave(child);
                return result;
            } else {
                return child.iterate(context);
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
                listener.enter(getChild(), properties, context);
                getChild().evaluatePendingUpdates(context, pul);
                listener.leave(getChild());
            } else {
                getChild().evaluatePendingUpdates(context, pul);
            }
        }
        public override string toShortString() {
            return getChild().toShortString();
        }
    
    }
}