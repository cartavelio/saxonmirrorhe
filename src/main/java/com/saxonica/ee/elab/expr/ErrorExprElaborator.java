package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.eval.PushEvaluator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.ErrorExpression;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.s9api.XmlProcessingError;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;

public class ErrorExprElaborator extends PullElaborator {

    @Override
    public PullEvaluator getPullEvaluator() {
        XmlProcessingError err = ((ErrorExpression)getExpression()).getException();
        GroundedValue value = ((Literal)getExpression()).getGroundedValue();
        return context -> value.iterate();
    }

    @Override
    public PushEvaluator getPushEvaluator() {
        GroundedValue value = ((Literal) getExpression()).getGroundedValue();
        if (value instanceof Item) {
            return (out, context) -> out.append((Item)value);
        } else {
            return (out, context) -> {
                for (Item item : value.asIterable()) {
                     out.append(item);
                }
            };
        }
    }

    @Override
    public ItemEvaluator getItemEvaluator() {
        GroundedValue value = ((Literal) getExpression()).getGroundedValue();
        assert value.getLength() < 2;
        Item item = value.head();
        return context -> item;
    }

    @Override
    public BooleanEvaluator getBooleanEvaluator() {
        GroundedValue value = ((Literal) getExpression()).getGroundedValue();
        try {
            boolean ebv = value.effectiveBooleanValue();
            return context -> ebv;
        } catch (XPathException e) {
            throw new UncheckedXPathException(e);
        }
    }

}

