using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class UnionCastableFunction : Saxon.Hej.expr.UnionConstructorFunction {
        public UnionCastableFunction(Saxon.Hej.type.UnionType targetType, Saxon.Hej.om.NamespaceResolver resolver, bool allowEmpty) : base (targetType, resolver, allowEmpty) {
        }
        public override Saxon.Hej.type.FunctionItemType getFunctionItemType() {
            return new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.ANY_SEQUENCE}, Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
        }
        public override Saxon.Hej.om.StructuredQName getFunctionName() {
            return null;
        }
        private bool effectiveBooleanValue(Saxon.Hej.om.SequenceIterator iter, Saxon.Hej.expr.XPathContext context) {
            int count = 0;
            for (Saxon.Hej.om.Item item; ((item = iter.next()) != null); ) {
                if ((item is Saxon.Hej.om.NodeInfo)) {
                    Saxon.Hej.om.AtomicSequence atomizedValue = item.atomize();
                    int length = Saxon.Hej.om.SequenceTool.getLength(atomizedValue);
                    count += length;
                    if ((count>1)) {
                        return false;
                    }
                    if ((length != 0)) {
                        Saxon.Hej.value.AtomicValue av = ((/*1430*/Saxon.Hej.value.AtomicValue)atomizedValue.head());
                        if (!(castable(av, context))) {
                            return false;
                        }
                    }
                } else {
                    if ((item is Saxon.Hej.value.AtomicValue)) {
                        Saxon.Hej.value.AtomicValue av = (Saxon.Hej.value.AtomicValue)(item);
                        count++;
                        if ((count>1)) {
                            return false;
                        }
                        if (!(castable(av, context))) {
                            return false;
                        }
                    } else {
                        throw new Saxon.Hej.trans.XPathException("Input to 'castable' operator cannot be atomized", "XPTY0004");
                    }
                }
            }
            return ((count != 0) || allowEmpty);
        }
        private bool castable(Saxon.Hej.value.AtomicValue value, Saxon.Hej.expr.XPathContext context) {
            try {
                cast(value, context);
                return true;
            } catch (Saxon.Hej.trans.XPathException err) {
                return false;
            }
        }
        public override Saxon.Hej.value.BooleanValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] args) {
            bool value = effectiveBooleanValue(args[0].iterate(), context);
            return Saxon.Hej.value.BooleanValue.get(value);
        }
    
    }
}