package com.saxonica.ee.elab.gen;

import com.saxonica.ee.elab.eval.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.instruct.SimpleNodeConstructor;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.om.Item;
import net.sf.saxon.str.EmptyUnicodeString;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.tree.iter.SingletonIterator;
import net.sf.saxon.tree.util.Orphan;

public abstract class SimpleNodePushElaborator extends Elaborator {

    @Override
    public PullEvaluator elaborateForPull() {
        ItemEvaluator itemEval = elaborateForItem();
        return context -> SingletonIterator.makeIterator(itemEval.eval(context));
    }

    @Override
    public PushEvaluator elaborateForPush() {
        // Must be implemented in a subclass
        throw new UnsupportedOperationException();
    }

    @Override
    public ItemEvaluator elaborateForItem() {
        final SimpleNodeConstructor instr = (SimpleNodeConstructor) getExpression();
        final ItemEvaluator select = makeElaborator(instr.getSelect()).elaborateForItem();
        final short kind = (short)instr.getItemType().getPrimitiveType();
        final Configuration config = getConfiguration();
        return context -> {
            Item contentItem = select.eval(context);
            UnicodeString content;
            if (contentItem == null) {
                content = EmptyUnicodeString.getInstance();
            } else {
                content = contentItem.getUnicodeStringValue();
                content = instr.checkContent(content, context);
            }
            Orphan o = new Orphan(config);
            o.setNodeKind(kind);
            o.setStringValue(content);
            o.setNodeName(instr.evaluateNodeName(context));
            return o;
        };
    }

    public BooleanEvaluator elaborateForBoolean() {
        ItemEvaluator ie = elaborateForItem();
        return context -> ExpressionTool.effectiveBooleanValue(ie.eval(context));
    }

    @Override
    public StringEvaluator elaborateForString(boolean zeroLengthWhenAbsent) {
        ItemEvaluator ie = elaborateForItem();
        return context -> {
            Item item = ie.eval(context);
            return item == null ? handleNullString(zeroLengthWhenAbsent) : item.getUnicodeStringValue();
        };
    }


}

