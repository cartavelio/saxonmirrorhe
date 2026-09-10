package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.*;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;

/**
 * Elaborator for a literal.
 *
 * <p>Evaluators are supplied for evaluation in pull, push, and singleton mode, as well as boolean
 * evaluation</p>
 */

public class LiteralElaborator extends PullElaborator {

    @Override
    public PullEvaluator elaborateForPull() {
        GroundedValue value = ((Literal)getExpression()).getGroundedValue();
        return context -> value.iterate();
    }

    @Override
    public PushEvaluator elaborateForPush() {
        GroundedValue value = ((Literal) getExpression()).getGroundedValue();
        if (value instanceof Item) {
            return (out, context) -> {
                out.append((Item)value);
                return null;
            };
        } else {
            return (out, context) -> {
                for (Item item : value.asIterable()) {
                     out.append(item);
                }
                return null;
            };
        }
    }

    @Override
    public ItemEvaluator elaborateForItem() {
        GroundedValue value = ((Literal) getExpression()).getGroundedValue();
        assert value.getLength() < 2;
        Item item = value.head();
        return context -> item;
    }

    @Override
    public BooleanEvaluator elaborateForBoolean() {
        GroundedValue value = ((Literal) getExpression()).getGroundedValue();
        try {
            boolean ebv = value.effectiveBooleanValue();
            return context -> ebv;
        } catch (XPathException e) {
            throw new UncheckedXPathException(e);
        }
    }

    @Override
    public UnicodeStringEvaluator elaborateForUnicodeString(boolean zeroLengthWhenAbsent) {
        GroundedValue value = ((Literal) getExpression()).getGroundedValue();
        try {
            UnicodeString str = value.getUnicodeStringValue();
            return context -> str;
        } catch (XPathException e) {
            throw new UncheckedXPathException(e);
        }
    }

    @Override
    public StringEvaluator elaborateForString(boolean zeroLengthWhenAbsent) {
        GroundedValue value = ((Literal) getExpression()).getGroundedValue();
        try {
            String str = value.getStringValue();
            return context -> str;
        } catch (XPathException e) {
            throw new UncheckedXPathException(e);
        }
    }


}

