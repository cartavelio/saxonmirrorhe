using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.sxpath {
    public sealed class XPathVariable : Saxon.Hej.expr.LocalBinding {
        private Saxon.Hej.om.StructuredQName name;
        private Saxon.Hej.value.SequenceType requiredType = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
        private Saxon.Hej.om.Sequence defaultValue;
        private int slotNumber;
        private XPathVariable()  {
        }
        public static Saxon.Hej.sxpath.XPathVariable make(Saxon.Hej.om.StructuredQName name) {
            Saxon.Hej.sxpath.XPathVariable v = new Saxon.Hej.sxpath.XPathVariable();
            v.name = name;
            return v;
        }
        public bool isGlobal() {
            return false;
        }
        public bool isAssignable() {
            return false;
        }
        public void setRequiredType(Saxon.Hej.value.SequenceType requiredType) {
            this.requiredType = requiredType;
        }
        public Saxon.Hej.value.SequenceType getRequiredType() {
            return requiredType;
        }
        public Saxon.Hej.value.IntegerValue[] getIntegerBoundsForVariable() {
            return null;
        }
        public void setSlotNumber(int slotNumber) {
            this.slotNumber = slotNumber;
        }
        public int getLocalSlotNumber() {
            return slotNumber;
        }
        public Saxon.Hej.om.StructuredQName getVariableQName() {
            return name;
        }
        public void addReference(Saxon.Hej.expr.VariableReference @ref, bool isLoopingReference) {
        }
        public void setDefaultValue(Saxon.Hej.om.Sequence defaultValue) {
            this.defaultValue = defaultValue;
        }
        public Saxon.Hej.om.Sequence getDefaultValue() {
            return defaultValue;
        }
        public Saxon.Hej.om.Sequence evaluateVariable(Saxon.Hej.expr.XPathContext context) {
            return context.evaluateLocalVariable(slotNumber);
        }
        public void setIndexedVariable() {
        }
        public bool isIndexedVariable() {
            return false;
        }
    
    }
}