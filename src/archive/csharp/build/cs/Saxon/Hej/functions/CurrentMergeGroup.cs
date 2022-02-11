using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class CurrentMergeGroup : Saxon.Hej.functions.SystemFunction {
        private bool inLoop = false;
        private Saxon.Hej.expr.sort.MergeInstr controllingInstruction = null;
        private readonly System.Collections.Generic.ISet<string> allowedNames = new System.Collections.Generic.HashSet<string>();
        public void setControllingInstruction(Saxon.Hej.expr.sort.MergeInstr instruction, bool isInLoop) {
            this.controllingInstruction = instruction;
            this.inLoop = isInLoop;
            foreach (Saxon.Hej.expr.sort.MergeInstr.MergeSource m in instruction.getMergeSources()) {
                string name = m.sourceName;
                if ((name != null)) {
                    allowedNames.Add(name);
                }
            }
        }
        public Saxon.Hej.expr.sort.MergeInstr getControllingInstruction() {
            return controllingInstruction;
        }
        public bool isInLoop() {
            return inLoop;
        }
        public override Saxon.Hej.type.ItemType getResultItemType() {
            return Saxon.Hej.type.AnyItemType.getInstance();
        }
        public override int getSpecialProperties(Saxon.Hej.expr.Expression[] arguments) {
            return 0;
        }
        public override Saxon.Hej.expr.Expression makeFunctionCall(params Saxon.Hej.expr.Expression[]  arguments) {
            return new Inner_SystemFunctionCall_9(this, this, arguments);
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            string source = null;
            if ((arguments.Length>0)) {
                source = arguments[0].head().getStringValue();
            }
            return Saxon.Hej.om.SequenceTool.toLazySequence(currentGroup(source, context));
        }
        private Saxon.Hej.om.SequenceIterator currentGroup(string source, Saxon.Hej.expr.XPathContext c) {
            Saxon.Hej.expr.sort.GroupIterator gi = c.getCurrentMergeGroupIterator();
            if ((gi == null)) {
                throw new Saxon.Hej.trans.XPathException("There is no current merge group", "XTDE3480");
            }
            if ((source == null)) {
                return gi.iterateCurrentGroup();
            } else {
                if (!(allowedNames.Contains(source))) {
                    throw new Saxon.Hej.trans.XPathException((("Supplied argument ("+source)+") is not the name of any xsl:merge-source in the containing xsl:merge instruction"), "XTDE3490");
                }
                return ((Saxon.Hej.expr.sort.MergeGroupingIterator)(gi)).iterateCurrentGroup(source);
            }
        }
        public override string getStreamerName() {
            return "CurrentMergeGroup";
        }
    
        private class Inner_SystemFunctionCall_9 : Saxon.Hej.expr.SystemFunctionCall {
            private readonly CurrentMergeGroup outerInstance;
            public Inner_SystemFunctionCall_9(CurrentMergeGroup outerInstance, CurrentMergeGroup arg1, /*AA*/Saxon.Hej.expr.Expression[] arg2) : base(arg1, arg2) {
                this.outerInstance = outerInstance;
            }
            public override Saxon.Hej.expr.Expression getScopingExpression() {
                return outerInstance.getControllingInstruction();
            }
        }

    }
}