using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class LocalVariableReference : Saxon.Hej.expr.VariableReference {
        internal int slotNumber = -999;
        public LocalVariableReference(Saxon.Hej.om.StructuredQName name) : base (name) {
        }
        public LocalVariableReference(Saxon.Hej.expr.LocalBinding binding) : base (binding) {
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            if ((binding == null)) {
                throw new Saxon.Ejava.lang.UnsupportedOperationException("Cannot copy a variable reference whose binding is unknown");
            }
            Saxon.Hej.expr.LocalVariableReference @ref = new Saxon.Hej.expr.LocalVariableReference(getVariableName());
            @ref.copyFrom(this);
            @ref.slotNumber = slotNumber;
            Saxon.Hej.expr.Binding newBinding = rebindings.get(binding);
            if ((newBinding != null)) {
                @ref.binding = newBinding;
            }
            @ref.binding.addReference(@ref, isInLoop());
            return @ref;
        }
        public void setBinding(Saxon.Hej.expr.LocalBinding binding) {
            this.binding = binding;
        }
        public override Saxon.Hej.expr.LocalBinding getBinding() {
            return (Saxon.Hej.expr.LocalBinding)(base.getBinding());
        }
        public void setSlotNumber(int slotNumber) {
            this.slotNumber = slotNumber;
        }
        public int getSlotNumber() {
            return slotNumber;
        }
        public override Saxon.Hej.om.Sequence evaluateVariable(Saxon.Hej.expr.XPathContext c) {
            try {
                return c.getStackFrame().slots[slotNumber];
            } catch (System.IndexOutOfRangeException err) {
                if ((slotNumber == -999)) {
                    if ((binding != null)) {
                        try {
                            slotNumber = getBinding().getLocalSlotNumber();
                            return c.getStackFrame().slots[slotNumber];
                        } catch (System.IndexOutOfRangeException err2) {
                        }
                    }
                    throw new System.IndexOutOfRangeException((("Local variable $"+getDisplayName())+" has not been allocated a stack frame slot"));
                } else {
                    int actual = c.getStackFrame().slots.Length;
                    throw new System.IndexOutOfRangeException(((((((("Local variable $"+getDisplayName())+" uses slot ")+slotNumber)+" but ")+(((actual == 0) ? "no" : ("only "+c.getStackFrame().slots.Length))))+" slots")+" are allocated on the stack frame"));
                }
            }
        }
        public override string getExpressionName() {
            return "locVarRef";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.LocalVariableReference.LocalVariableReferenceElaborator();
        }
        public class LocalVariableReferenceElaborator : Saxon.Hej.expr.elab.PullElaborator {
            public override void setExpression(Saxon.Hej.expr.Expression expr) {
                base.setExpression(expr);
                if ((((Saxon.Hej.expr.LocalVariableReference)(expr)).getSlotNumber()<0)) {
                    throw new Saxon.Ejava.lang.IllegalStateException("Can't elaborate a local variable reference before slot numbers have been allocated");
                }
            }
            public override Saxon.Hej.expr.elab.SequenceEvaluator eagerly() {
                Saxon.Hej.expr.LocalVariableReference varRef = (Saxon.Hej.expr.LocalVariableReference)(getExpression());
                int slot = varRef.getSlotNumber();
                return new Saxon.Hej.expr.elab.LocalVariableEvaluator(slot);
            }
            public override Saxon.Hej.expr.elab.SequenceEvaluator lazily(bool repeatable) {
                return eagerly();
            }
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.expr.LocalVariableReference varRef = (Saxon.Hej.expr.LocalVariableReference)(getExpression());
                int slot = varRef.getSlotNumber();
                return context => context.evaluateLocalVariable(slot).iterate();
            }
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.LocalVariableReference varRef = (Saxon.Hej.expr.LocalVariableReference)(getExpression());
                int slot = varRef.getSlotNumber();
                return (@out, context) => {
                    try {
                        Saxon.Hej.om.SequenceIterator value = context.evaluateLocalVariable(slot).iterate();
                        for (Saxon.Hej.om.Item it; ((it = value.next()) != null); ) {
                            @out.append(it);
                        }
                        return null;
                    } catch (Saxon.Hej.trans.XPathException e) {
                        e.maybeSetLocation(getExpression().getLocation());
                        e.maybeSetContext(context);
                        throw;
                    }
                } ;
            }
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.LocalVariableReference varRef = (Saxon.Hej.expr.LocalVariableReference)(getExpression());
                int slot = varRef.getSlotNumber();
                return context => {
                    try {
                        return context.evaluateLocalVariable(slot).head();
                    } catch (Saxon.Hej.trans.XPathException e) {
                        e.maybeSetLocation(getExpression().getLocation());
                        e.maybeSetContext(context);
                        throw;
                    }
                } ;
            }
        
        }
    
    }
}