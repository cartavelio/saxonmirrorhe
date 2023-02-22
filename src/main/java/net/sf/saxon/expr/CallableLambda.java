package net.sf.saxon.expr;

import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;

/**
 * An implementation of {@link Callable} that allows the logic to be supplied as a lambda expression.
 *
 * <p>On Java, it's possible to assign a lambda expression directly to a Callable. But on C#, a Callable
 * isn't a delegate, so lambda expressions can't be used directly; instead, this class is provided as a proxy.</p>
 */

public class CallableLambda implements Callable {

    @FunctionalInterface
    public interface Lambda {
        Sequence call(XPathContext context, Sequence[] arguments) throws XPathException;
    }

    private final Lambda expression;

    public CallableLambda (Lambda expression) {
        this.expression = expression;
    }

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        return expression.call(context, arguments);
    }
}

