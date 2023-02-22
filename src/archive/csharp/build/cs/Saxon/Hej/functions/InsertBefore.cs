using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class InsertBefore : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.NumericValue n = (Saxon.Hej.value.NumericValue)(arguments[1].head());
            int pos = (int)(n.longValue());
            return Saxon.Hej.om.SequenceTool.toLazySequence(new Saxon.Hej.functions.InsertBefore.InsertIterator(arguments[0].iterate(), arguments[2].iterate(), pos));
        }
        public override Saxon.Hej.expr.Expression makeFunctionCall(Saxon.Hej.expr.Expression[] arguments) {
            return new Inner_SystemFunctionCall_6(this, this, arguments);
        }
        public class InsertIterator : Saxon.Hej.om.SequenceIterator {
            private readonly Saxon.Hej.om.SequenceIterator @base;
            private readonly Saxon.Hej.om.SequenceIterator insert;
            private readonly int insertPosition;
            private int position = 0;
            private bool inserting;
            public InsertIterator(Saxon.Hej.om.SequenceIterator @base, Saxon.Hej.om.SequenceIterator insert, int insertPosition)  {
                this.@base = @base;
                this.insert = insert;
                this.insertPosition = System.Math.Max(insertPosition, 1);
                this.inserting = (insertPosition == 1);
            }
            public Saxon.Hej.om.Item next() {
                Saxon.Hej.om.Item nextItem;
                if (inserting) {
                    nextItem = insert.next();
                    if ((nextItem == null)) {
                        inserting = false;
                        nextItem = @base.next();
                    }
                } else {
                    if ((position == (insertPosition-1))) {
                        nextItem = insert.next();
                        if ((nextItem == null)) {
                            nextItem = @base.next();
                        } else {
                            inserting = true;
                        }
                    } else {
                        nextItem = @base.next();
                        if (((nextItem == null) && (position<(insertPosition-1)))) {
                            inserting = true;
                            nextItem = insert.next();
                        }
                    }
                }
                if ((nextItem == null)) {
                    position = -1;
                    return null;
                } else {
                    position++;
                    return nextItem;
                }
            }
            public void close() {
                @base.close();
                insert.close();
            }
        
        }
        public override string getStreamerName() {
            return "InsertBefore";
        }
    
        private class Inner_SystemFunctionCall_6 : Saxon.Hej.expr.SystemFunctionCall {
            private readonly InsertBefore outerInstance;
            public Inner_SystemFunctionCall_6(InsertBefore outerInstance, InsertBefore arg1, /*AA*/Saxon.Hej.expr.Expression[] arg2) : base(arg1, arg2) {
                this.outerInstance = outerInstance;
            }
            public override Saxon.Hej.type.ItemType getItemType() {
                return Saxon.Hej.type.Type.getCommonSuperType(/*superMethod*/getArg(0).getItemType(), /*superMethod*/getArg(2).getItemType());
            }
        }

    }
}