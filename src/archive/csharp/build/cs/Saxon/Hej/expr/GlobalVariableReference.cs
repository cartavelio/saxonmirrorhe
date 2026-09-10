using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class GlobalVariableReference : Saxon.Hej.expr.VariableReference, Saxon.Hej.expr.ComponentInvocation {
        internal int bindingSlot = -1;
        public GlobalVariableReference(Saxon.Hej.om.StructuredQName name) : base (name) {
        }
        public GlobalVariableReference(Saxon.Hej.expr.instruct.GlobalVariable var) : base (var) {
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            if ((binding == null)) {
                throw new Saxon.Ejava.lang.UnsupportedOperationException("Cannot copy a variable reference whose binding is unknown");
            }
            Saxon.Hej.expr.GlobalVariableReference @ref = new Saxon.Hej.expr.GlobalVariableReference(getVariableName());
            @ref.copyFrom(this);
            return @ref;
        }
        public void setBindingSlot(int slot) {
            if ((bindingSlot != -1)) {
                throw new Saxon.Ejava.lang.AssertionError("Duplicate binding slot assignment");
            }
            bindingSlot = slot;
        }
        public int getBindingSlot() {
            return bindingSlot;
        }
        public Saxon.Hej.trans.SymbolicName getSymbolicName() {
            return new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_VARIABLE, getVariableName());
        }
        public void setTarget(Saxon.Hej.expr.Component target) {
            binding = (Saxon.Hej.expr.instruct.GlobalVariable)(target.getActor());
        }
        public Saxon.Hej.expr.Component getTarget() {
            return ((Saxon.Hej.expr.instruct.GlobalVariable)(binding)).getDeclaringComponent();
        }
        public Saxon.Hej.expr.Component getFixedTarget() {
            Saxon.Hej.expr.Component c = getTarget();
            Saxon.Hej.trans.Visibility v = c.getVisibility();
            if (((v == Saxon.Hej.trans.Visibility.PRIVATE) || (v == Saxon.Hej.trans.Visibility.FINAL))) {
                return c;
            } else {
                return null;
            }
        }
        public override Saxon.Hej.om.GroundedValue evaluateVariable(Saxon.Hej.expr.XPathContext c) {
            if ((bindingSlot>=0)) {
                if ((c.getCurrentComponent() == null)) {
                    throw new Saxon.Ejava.lang.AssertionError("No current component");
                }
                Saxon.Hej.expr.Component target = c.getTargetComponent(bindingSlot);
                if (target.isHiddenAbstractComponent()) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Cannot evaluate an abstract variable ("+getVariableName().getDisplayName())+") with no overriding declaration"), "XTDE3052");
                    err.setLocation(getLocation());
                    throw err;
                }
                Saxon.Hej.expr.instruct.GlobalVariable p = (Saxon.Hej.expr.instruct.GlobalVariable)(target.getActor());
                return p.evaluateVariable(c, target);
            } else {
                Saxon.Hej.expr.instruct.GlobalVariable b = (Saxon.Hej.expr.instruct.GlobalVariable)(binding);
                return b.evaluateVariable(c, b.getDeclaringComponent());
            }
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("gVarRef", this);
            @out.emitAttribute("name", getVariableName());
            @out.emitAttribute("bSlot", (""+getBindingSlot()));
            @out.endElement();
        }
        public System.Collections.Generic.ISet<Saxon.Hej.expr.Expression> getPreconditions() {
            System.Collections.Generic.ISet<Saxon.Hej.expr.Expression> pre = new System.Collections.Generic.HashSet<Saxon.Hej.expr.Expression>();
            return pre;
        }
        public override string getExpressionName() {
            return "gVarRef";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.GlobalVariableReference.GlobalVariableReferenceElaborator();
        }
        public class GlobalVariableReferenceElaborator : Saxon.Hej.expr.elab.PullElaborator, Saxon.Hej.expr.elab.SequenceEvaluator {
            public Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.XPathContext context) {
                Saxon.Hej.expr.GlobalVariableReference varRef = (Saxon.Hej.expr.GlobalVariableReference)(getExpression());
                return varRef.evaluateVariable(context);
            }
            public override Saxon.Hej.expr.elab.SequenceEvaluator eagerly() {
                return this;
            }
            public override Saxon.Hej.expr.elab.SequenceEvaluator lazily(bool repeatable) {
                return this;
            }
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.expr.GlobalVariableReference varRef = (Saxon.Hej.expr.GlobalVariableReference)(getExpression());
                return context => varRef.evaluateVariable(context).iterate();
            }
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.GlobalVariableReference varRef = (Saxon.Hej.expr.GlobalVariableReference)(getExpression());
                return (@out, context) => {
                    Saxon.Hej.om.SequenceIterator value = varRef.evaluateVariable(context).iterate();
                    for (Saxon.Hej.om.Item it; ((it = value.next()) != null); ) {
                        @out.append(it);
                    }
                    return null;
                } ;
            }
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.GlobalVariableReference varRef = (Saxon.Hej.expr.GlobalVariableReference)(getExpression());
                return context => varRef.evaluateVariable(context).head();
            }
        
        }
    
    }
}