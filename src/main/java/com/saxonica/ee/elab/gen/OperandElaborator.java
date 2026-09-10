package com.saxonica.ee.elab.gen;

import com.saxonica.ee.elab.eval.*;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.TailCall;
import net.sf.saxon.expr.oper.OperandEvaluator;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;

/**
 * This class, while general-purpose, is used primarily for evaluating expressions that use
 * a fallback Elaboration strategy. Specifically, these classes do not elaborate their operands
 * in advance; instead, they evaluate their operands by calling {@code Operand#getOperandEvaluator},
 * which in Saxon-EE returns an instance of this class. When the {@code OperandElaborator} is
 * called to evaluate an expression, it first creates an evaluator for the expression which
 * can be re-used on subsequent evaluations.
 */

public class OperandElaborator implements OperandEvaluator {

    private final Expression expression;

    private PushEvaluator pushEvaluator = null;
    private PullEvaluator pullEvaluator = null;
    private ItemEvaluator itemEvaluator = null;
    private StringEvaluator stringEvaluator = null;
    private BooleanEvaluator booleanEvaluator = null;

    public OperandElaborator(Expression expr) {
        this.expression = expr;
    }

    public TailCall processLeavingTail(Outputter outputter, XPathContext context) throws XPathException {
        if (pushEvaluator == null) {
            pushEvaluator = Elaborator.makeElaborator(expression).elaborateForPush();
        }
        return pushEvaluator.process(outputter, context);
    }

    public SequenceIterator iterate(XPathContext context) throws XPathException {
        if (pullEvaluator == null) {
            pullEvaluator = Elaborator.makeElaborator(expression).elaborateForPull();
        }
        return pullEvaluator.eval(context);
    }

    public Item evaluateItem(XPathContext context) throws XPathException {
        if (itemEvaluator == null) {
            itemEvaluator = Elaborator.makeElaborator(expression).elaborateForItem();
        }
        return itemEvaluator.eval(context);
    }

    public UnicodeString evaluateAsString(XPathContext context) throws XPathException {
        if (stringEvaluator == null) {
            stringEvaluator = Elaborator.makeElaborator(expression).elaborateForString(true);
        }
        return stringEvaluator.eval(context);
    }

    public boolean effectiveBooleanValue(XPathContext context) throws XPathException {
        if (booleanEvaluator == null) {
            booleanEvaluator = Elaborator.makeElaborator(expression).elaborateForBoolean();
        }
        return booleanEvaluator.eval(context);
    }
}

