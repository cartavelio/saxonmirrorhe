using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class LocalVariableBinding : Saxon.Hej.expr.LocalBinding {
        private Saxon.Hej.om.StructuredQName variableName;
        private Saxon.Hej.value.SequenceType requiredType;
        private int slotNumber = -999;
        private int refCount = 0;
        public LocalVariableBinding(Saxon.Hej.om.StructuredQName name, Saxon.Hej.value.SequenceType type)  {
            variableName = name;
            requiredType = type;
        }
        public Saxon.Hej.expr.flwor.LocalVariableBinding copy() {
            Saxon.Hej.expr.flwor.LocalVariableBinding lb2 = new Saxon.Hej.expr.flwor.LocalVariableBinding(variableName, requiredType);
            lb2.slotNumber = slotNumber;
            lb2.refCount = refCount;
            return lb2;
        }
        public Saxon.Hej.om.StructuredQName getVariableQName() {
            return variableName;
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
        public int getNominalReferenceCount() {
            return refCount;
        }
        public void addReference(Saxon.Hej.expr.VariableReference @ref, bool isLoopingReference) {
            if ((refCount != Saxon.Hej.expr.FilterExpression.FILTERED)) {
                refCount += ((isLoopingReference ? 10 : 1));
            }
        }
        public void setIndexedVariable() {
            refCount = Saxon.Hej.expr.FilterExpression.FILTERED;
        }
        public bool isIndexedVariable() {
            return (refCount == Saxon.Hej.expr.FilterExpression.FILTERED);
        }
        public void setVariableQName(Saxon.Hej.om.StructuredQName variableName) {
            this.variableName = variableName;
        }
        public void setSlotNumber(int nr) {
            slotNumber = nr;
        }
        public int getLocalSlotNumber() {
            return slotNumber;
        }
        public Saxon.Hej.om.Sequence evaluateVariable(Saxon.Hej.expr.XPathContext context) {
            return context.evaluateLocalVariable(slotNumber);
        }
        public bool isAssignable() {
            return false;
        }
        public bool isGlobal() {
            return false;
        }
    
    }
}