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
    public PullEvaluator getPullEvaluator() {
        ItemEvaluator itemEval = getItemEvaluator();
        return context -> new SingletonIterator(itemEval.eval(context));
    }

    @Override
    public PushEvaluator getPushEvaluator() {
        // Must be implemented in a subclass
        throw new UnsupportedOperationException();
    }

    @Override
    public ItemEvaluator getItemEvaluator() {
        final SimpleNodeConstructor instr = (SimpleNodeConstructor) getExpression();
        final ItemEvaluator select = Elaborator.make(instr.getSelect()).getItemEvaluator();
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

    public BooleanEvaluator getBooleanEvaluator() {
        ItemEvaluator ie = getItemEvaluator();
        return context -> ExpressionTool.effectiveBooleanValue(ie.eval(context));
    }

    @Override
    public StringEvaluator getStringEvaluator(UnicodeString resultOnEmpty) {
        ItemEvaluator ie = getItemEvaluator();
        return context -> {
            Item item = ie.eval(context);
            return item == null ? resultOnEmpty : item.getUnicodeStringValue();
        };
    }


}

