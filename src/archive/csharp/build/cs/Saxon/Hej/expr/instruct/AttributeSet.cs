using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class AttributeSet : Saxon.Hej.expr.instruct.Actor {
        internal Saxon.Hej.om.StructuredQName attributeSetName;
        private bool declaredStreamable;
        private Saxon.Hej.expr.elab.PushEvaluator bodyEvaluator;
        public AttributeSet()  {
        }
        public override Saxon.Hej.trans.SymbolicName getSymbolicName() {
            return new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_ATTRIBUTE_SET, attributeSetName);
        }
        public void setName(Saxon.Hej.om.StructuredQName attributeSetName) {
            this.attributeSetName = attributeSetName;
        }
        public void setDeclaredStreamable(bool value) {
            this.declaredStreamable = value;
        }
        public bool isDeclaredStreamable() {
            return this.declaredStreamable;
        }
        public override void setStackFrameMap(Saxon.Hej.expr.instruct.SlotManager stackFrameMap) {
            if ((stackFrameMap != null)) {
                base.setStackFrameMap(stackFrameMap);
            }
        }
        public int getFocusDependencies() {
            return (body.getDependencies()&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_FOCUS);
        }
        public void expand(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            lock (this) {
                if ((bodyEvaluator == null)) {
                    bodyEvaluator = getBody().makeElaborator().elaborateForPush();
                }
            }
            System.Collections.Generic.Stack<Saxon.Hej.expr.instruct.AttributeSet> stack = ((Saxon.Hej.trans.XsltController)(context.getController())).getAttributeSetEvaluationStack();
            if (stack.Contains(this)) {
                throw new Saxon.Hej.trans.XPathException((("Attribute set "+getObjectName().getEQName())+" invokes itself recursively"), "XTDE0640");
            }
            stack.Push(this);
            Saxon.Hej.expr.Expression.dispatchTailCall(/*Delegate.processLeavingTail()*/bodyEvaluator(output, context));
            stack.Pop();
            if ((stack.Count==0)) {
                ((Saxon.Hej.trans.XsltController)(context.getController())).releaseAttributeSetEvaluationStack();
            }
        }
        public Saxon.Hej.om.StructuredQName getObjectName() {
            return attributeSetName;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            presenter.startElement("attributeSet");
            presenter.emitAttribute("name", getObjectName());
            presenter.emitAttribute("line", (getLineNumber()+""));
            presenter.emitAttribute("module", getSystemId());
            presenter.emitAttribute("slots", (getStackFrameMap().getNumberOfVariables()+""));
            presenter.emitAttribute("binds", (getDeclaringComponent().getComponentBindings().Count+""));
            if (isDeclaredStreamable()) {
                presenter.emitAttribute("flags", "s");
            }
            getBody().export(presenter);
            presenter.endElement();
        }
    
    }
}