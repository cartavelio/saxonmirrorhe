using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.elab {
    public abstract class SimpleNodePushElaborator : Saxon.Hej.expr.elab.Elaborator {
        public override Saxon.Hej.expr.elab.SequenceEvaluator eagerly() {
            Saxon.Hej.expr.elab.ItemEvaluator itemEval = elaborateForItem();
            return new Saxon.Hej.expr.elab.OptionalItemEvaluator(itemEval);
        }
        public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
            Saxon.Hej.expr.elab.ItemEvaluator itemEval = elaborateForItem();
            return context => Saxon.Hej.tree.iter.SingletonIterator.makeIterator(/*Delegate.eval()*/itemEval(context));
        }
        public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
        public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
            Saxon.Hej.expr.instruct.SimpleNodeConstructor instr = (Saxon.Hej.expr.instruct.SimpleNodeConstructor)(getExpression());
            Saxon.Hej.expr.elab.ItemEvaluator select = instr.getSelect().makeElaborator().elaborateForItem();
            short kind = (short)(instr.getItemType().getPrimitiveType());
            Saxon.Hej.Configuration config = getConfiguration();
            return context => {
                Saxon.Hej.om.Item contentItem = /*Delegate.eval()*/select(context);
                Saxon.Hej.str.UnicodeString content;
                if ((contentItem == null)) {
                    content = Saxon.Hej.str.EmptyUnicodeString.getInstance();
                } else {
                    content = contentItem.getUnicodeStringValue();
                    content = instr.checkContent(content, context);
                }
                Saxon.Hej.tree.util.Orphan o = new Saxon.Hej.tree.util.Orphan(config);
                o.setNodeKind(kind);
                o.setStringValue(content);
                o.setNodeName(instr.evaluateNodeName(context));
                return o;
            } ;
        }
        public override Saxon.Hej.expr.elab.BooleanEvaluator elaborateForBoolean() {
            Saxon.Hej.expr.elab.ItemEvaluator ie = elaborateForItem();
            return context => Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(/*Delegate.eval()*/ie(context));
        }
        public override Saxon.Hej.expr.elab.UnicodeStringEvaluator elaborateForUnicodeString(bool zeroLengthWhenAbsent) {
            Saxon.Hej.expr.elab.ItemEvaluator ie = elaborateForItem();
            return context => {
                Saxon.Hej.om.Item item = /*Delegate.eval()*/ie(context);
                return ((item == null) ? handleNullUnicodeString(zeroLengthWhenAbsent) : item.getUnicodeStringValue());
            } ;
        }
    
    }
}