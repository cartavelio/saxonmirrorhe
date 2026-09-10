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
    
    }
}