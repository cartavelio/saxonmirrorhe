using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class CurrentMergeKey : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {
        private Saxon.Hej.expr.sort.MergeInstr controllingInstruction = null;
        public void setControllingInstruction(Saxon.Hej.expr.sort.MergeInstr instruction) {
            this.controllingInstruction = instruction;
        }
        public Saxon.Hej.expr.sort.MergeInstr getControllingInstruction() {
            return controllingInstruction;
        }
        public override Saxon.Hej.expr.Expression makeFunctionCall(params Saxon.Hej.expr.Expression[]  arguments) {
            return new Inner_SystemFunctionCall_3(this, this, arguments);
        }
        public Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext c) {
            Saxon.Hej.expr.sort.GroupIterator gi = c.getCurrentMergeGroupIterator();
            if ((gi == null)) {
                throw new Saxon.Hej.trans.XPathException("There is no current merge key", "XTDE3510");
            }
            return ((/*1430*/Saxon.Hej.tree.iter.AtomicIterator)gi.getCurrentGroupingKey().iterate());
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return Saxon.Hej.om.SequenceTool.toLazySequence(iterate(context));
        }
    
        private class Inner_SystemFunctionCall_3 : Saxon.Hej.expr.SystemFunctionCall {
            private readonly CurrentMergeKey outerInstance;
            public Inner_SystemFunctionCall_3(CurrentMergeKey outerInstance, CurrentMergeKey arg1, /*AA*/Saxon.Hej.expr.Expression[] arg2) : base(arg1, arg2) {
                this.outerInstance = outerInstance;
            }
            public override Saxon.Hej.expr.Expression getScopingExpression() {
                return outerInstance.getControllingInstruction();
            }
        }

    }
}