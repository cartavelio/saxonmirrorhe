using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public sealed class TailCallLoop : Saxon.Hej.expr.UnaryExpression {
        internal Saxon.Hej.expr.instruct.UserFunction containingFunction;
        public TailCallLoop(Saxon.Hej.expr.instruct.UserFunction function, Saxon.Hej.expr.Expression body) : base (body) {
            containingFunction = function;
        }
        public Saxon.Hej.expr.instruct.UserFunction getContainingFunction() {
            return containingFunction;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getOperand().typeCheck(visitor, contextInfo);
            return this;
        }
        public override int getImplementationMethod() {
            return getBaseExpression().getImplementationMethod();
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole() {
            return Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("TailCallLoop.copy()");
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.XPathContextMajor cm = (Saxon.Hej.expr.XPathContextMajor)(context);
            while (true) {
                Saxon.Hej.om.SequenceIterator iter = getBaseExpression().iterate(cm);
                Saxon.Hej.om.GroundedValue extent = Saxon.Hej.om.SequenceTool.toGroundedValue(iter);
                Saxon.Hej.expr.TailCallLoop.TailCallInfo tail = cm.getTailCallInfo();
                if ((tail == null)) {
                    return extent.iterate();
                } else {
                    Saxon.Hej.expr.instruct.UserFunction target = establishTargetFunction(tail, cm);
                    if ((target != containingFunction)) {
                        return tailCallDifferentFunction(target, cm).iterate();
                    }
                }
            }
        }
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.XPathContextMajor cm = (Saxon.Hej.expr.XPathContextMajor)(context);
            while (true) {
                Saxon.Hej.om.Item item = getBaseExpression().evaluateItem(context);
                Saxon.Hej.expr.TailCallLoop.TailCallInfo tail = cm.getTailCallInfo();
                if ((tail == null)) {
                    return item;
                } else {
                    Saxon.Hej.expr.instruct.UserFunction target = establishTargetFunction(tail, cm);
                    if ((target != containingFunction)) {
                        return tailCallDifferentFunction(target, cm).head();
                    }
                }
            }
        }
        private Saxon.Hej.expr.instruct.UserFunction establishTargetFunction(Saxon.Hej.expr.TailCallLoop.TailCallInfo tail, Saxon.Hej.expr.XPathContextMajor cm) {
            if ((tail is Saxon.Hej.expr.TailCallLoop.TailCallFunction)) {
                return ((Saxon.Hej.expr.TailCallLoop.TailCallFunction)(tail)).function;
            } else {
                if ((tail is Saxon.Hej.expr.TailCallLoop.TailCallComponent)) {
                    Saxon.Hej.expr.Component targetComponent = ((Saxon.Hej.expr.TailCallLoop.TailCallComponent)(tail)).component;
                    cm.setCurrentComponent(targetComponent);
                    return (Saxon.Hej.expr.instruct.UserFunction)(targetComponent.getActor());
                } else {
                    throw new Saxon.Ejava.lang.AssertionError();
                }
            }
        }
        public override void process(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.XPathContextMajor cm = (Saxon.Hej.expr.XPathContextMajor)(context);
            Saxon.Hej.expr.Expression operand = getBaseExpression();
            while (true) {
                operand.process(output, context);
                Saxon.Hej.expr.TailCallLoop.TailCallInfo tail = cm.getTailCallInfo();
                if ((tail == null)) {
                    return ;
                } else {
                    Saxon.Hej.expr.instruct.UserFunction target = establishTargetFunction(tail, cm);
                    if ((target != containingFunction)) {
                        Saxon.Hej.om.SequenceTool.process(tailCallDifferentFunction(target, cm), output, operand.getLocation());
                        return ;
                    }
                }
            }
        }
        private Saxon.Hej.om.Sequence tailCallDifferentFunction(Saxon.Hej.expr.instruct.UserFunction userFunction, Saxon.Hej.expr.XPathContextMajor cm) {
            cm.resetStackFrameMap(userFunction.getStackFrameMap(), userFunction.getArity());
            try {
                return userFunction.getEvaluator().evaluate(userFunction.getBody(), cm);
            } catch (Saxon.Hej.trans.XPathException err) {
                err.maybeSetLocation(getLocation());
                err.maybeSetContext(cm);
                throw;
            }
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return getBaseExpression().getItemType();
        }
        public override string getExpressionName() {
            return "tailCallLoop";
        }
        public interface TailCallInfo {
        
        }
        public class TailCallComponent : Saxon.Hej.expr.TailCallLoop.TailCallInfo {
            public Saxon.Hej.expr.Component component;
            public Saxon.Hej.expr.instruct.UserFunction function;
        
        }
        public class TailCallFunction : Saxon.Hej.expr.TailCallLoop.TailCallInfo {
            public Saxon.Hej.expr.instruct.UserFunction function;
        
        }
    
    }
}