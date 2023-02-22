using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class StackFrame {
        protected internal Saxon.Hej.expr.instruct.SlotManager map;
        protected internal Saxon.Hej.om.Sequence[] slots;
        protected internal System.Collections.Generic.Stack<Saxon.Hej.om.Sequence> dynamicStack;
        public static readonly Saxon.Hej.expr.StackFrame EMPTY = new Saxon.Hej.expr.StackFrame(Saxon.Hej.expr.instruct.SlotManager.EMPTY, new Saxon.Hej.om.Sequence[0]);
        public StackFrame(Saxon.Hej.expr.instruct.SlotManager map, Saxon.Hej.om.Sequence[] slots)  {
            this.map = map;
            this.slots = slots;
        }
        public Saxon.Hej.expr.instruct.SlotManager getStackFrameMap() {
            return map;
        }
        public Saxon.Hej.om.Sequence[] getStackFrameValues() {
            return slots;
        }
        public void setStackFrameValues(Saxon.Hej.om.Sequence[] values) {
            slots = values;
        }
        public Saxon.Hej.expr.StackFrame copy() {
            Saxon.Hej.om.Sequence[] v2 = Saxon.Ejava.util.Arrays.copyOf(slots, slots.Length);
            Saxon.Hej.expr.StackFrame s = new Saxon.Hej.expr.StackFrame(map, v2);
            if ((dynamicStack != null)) {
                s.dynamicStack = shallowCopy(dynamicStack);
            }
            return s;
        }
        public void pushDynamicValue(Saxon.Hej.om.Sequence value) {
            if ((this == Saxon.Hej.expr.StackFrame.EMPTY)) {
                throw new Saxon.Ejava.lang.IllegalStateException("Immutable stack frame");
            }
            if ((dynamicStack == null)) {
                dynamicStack = newStack();
            }
            dynamicStack.Push(value);
        }
        private System.Collections.Generic.Stack<Saxon.Hej.om.Sequence> newStack() {
            return new System.Collections.Generic.Stack<Saxon.Hej.om.Sequence>();
        }
        private System.Collections.Generic.Stack<Saxon.Hej.om.Sequence> shallowCopy(System.Collections.Generic.Stack<Saxon.Hej.om.Sequence> old) {
            return new Stack<Saxon.Hej.om.Sequence>(new Stack<Saxon.Hej.om.Sequence>(old));
        }
        public Saxon.Hej.om.Sequence popDynamicValue() {
            return dynamicStack.Pop();
        }
        public bool holdsDynamicValue() {
            return ((dynamicStack != null) && !((dynamicStack.Count==0)));
        }
    
    }
}