using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class IndexOf : Saxon.Hej.functions.CollatingFunctionFixed {
        public override Saxon.Hej.value.IntegerValue[] getIntegerBounds() {
            return new Saxon.Hej.value.IntegerValue[]{Saxon.Hej.value.Int64Value.PLUS_ONE, Saxon.Hej.expr.Expression.MAX_SEQUENCE_LENGTH};
        }
        public override void supplyTypeInformation(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType, Saxon.Hej.expr.Expression[] arguments) {
            Saxon.Hej.type.ItemType type0 = arguments[0].getItemType();
            Saxon.Hej.type.ItemType type1 = arguments[1].getItemType();
            if (((type0 is Saxon.Hej.type.AtomicType) && (type1 is Saxon.Hej.type.AtomicType))) {
                preAllocateComparer((Saxon.Hej.type.AtomicType)(type0), (Saxon.Hej.type.AtomicType)(type1), visitor.getStaticContext());
            }
        }
        private class IndexIterator : Saxon.Hej.om.SequenceIterator {
            private int index = 0;
            private readonly Saxon.Hej.om.SequenceIterator @base;
            private readonly Saxon.Hej.type.BuiltInAtomicType searchType;
            private readonly Saxon.Hej.expr.sort.AtomicComparer comparer;
            private readonly Saxon.Hej.value.AtomicValue key;
            public IndexIterator(Saxon.Hej.om.SequenceIterator @base, Saxon.Hej.type.BuiltInAtomicType searchType, Saxon.Hej.value.AtomicValue key, Saxon.Hej.expr.sort.AtomicComparer comparer)  {
                this.@base = @base;
                this.searchType = searchType;
                this.key = key;
                this.comparer = comparer;
            }
            public void close() {
                @base.close();
            }
            public Saxon.Hej.om.Item next() {
                try {
                    Saxon.Hej.value.AtomicValue baseItem;
                    while (((baseItem = (Saxon.Hej.value.AtomicValue)(@base.next())) != null)) {
                        index++;
                        if ((Saxon.Hej.type.Type.isGuaranteedComparable(searchType, baseItem.getPrimitiveType(), false) && comparer.comparesEqual(baseItem, key))) {
                            return new Saxon.Hej.value.Int64Value(index);
                        }
                    }
                    return null;
                } catch (Saxon.Hej.trans.NoDynamicContextException e) {
                    throw new Saxon.Hej.trans.UncheckedXPathException(e);
                }
            }
        
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.expr.sort.AtomicComparer comparer = getAtomicComparer(context);
            Saxon.Hej.om.SequenceIterator seq = arguments[0].iterate();
            Saxon.Hej.value.AtomicValue val = (Saxon.Hej.value.AtomicValue)(arguments[1].head());
            Saxon.Hej.type.BuiltInAtomicType searchType = val.getPrimitiveType();
            return Saxon.Hej.om.SequenceTool.toLazySequence(new Saxon.Hej.functions.IndexOf.IndexIterator(seq, searchType, val, comparer));
        }
        public override string getStreamerName() {
            return "IndexOf";
        }
    
    }
}