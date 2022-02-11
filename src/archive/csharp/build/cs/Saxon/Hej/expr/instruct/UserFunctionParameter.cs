using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class UserFunctionParameter : Saxon.Hej.expr.LocalBinding {
        private Saxon.Hej.value.SequenceType requiredType;
        private Saxon.Hej.om.StructuredQName variableQName;
        private int slotNumber;
        private readonly int referenceCount = 999;
        private bool isIndexed = false;
        private Saxon.Hej.trans.FunctionStreamability functionStreamability = Saxon.Hej.trans.FunctionStreamability.UNCLASSIFIED;
        public UserFunctionParameter()  {
        }
        public bool isGlobal() {
            return false;
        }
        public bool isAssignable() {
            return false;
        }
        public void setSlotNumber(int slot) {
            slotNumber = slot;
        }
        public int getLocalSlotNumber() {
            return slotNumber;
        }
        public void setRequiredType(Saxon.Hej.value.SequenceType type) {
            requiredType = type;
        }
        public Saxon.Hej.value.SequenceType getRequiredType() {
            return requiredType;
        }
        public Saxon.Hej.value.IntegerValue[] getIntegerBoundsForVariable() {
            return null;
        }
        public void setVariableQName(Saxon.Hej.om.StructuredQName name) {
            variableQName = name;
        }
        public Saxon.Hej.om.StructuredQName getVariableQName() {
            return variableQName;
        }
        public void addReference(Saxon.Hej.expr.VariableReference @ref, bool isLoopingReference) {
        }
        public int getReferenceCount() {
            return referenceCount;
        }
        public void setIndexedVariable(bool indexed) {
            isIndexed = indexed;
        }
        public void setIndexedVariable() {
            setIndexedVariable(true);
        }
        public bool isIndexedVariable() {
            return isIndexed;
        }
        public Saxon.Hej.om.Sequence evaluateVariable(Saxon.Hej.expr.XPathContext context) {
            return context.evaluateLocalVariable(slotNumber);
        }
        public void setFunctionStreamability(Saxon.Hej.trans.FunctionStreamability ability) {
            this.functionStreamability = ability;
        }
        public Saxon.Hej.trans.FunctionStreamability getFunctionStreamability() {
            return functionStreamability;
        }
    
    }
}