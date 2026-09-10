package com.saxonica.expr;

import net.sf.saxon.Controller;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.UserFunction;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.FastStringBuffer;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.QNameValue;

import java.util.HashMap;

/**
 * A user-defined function that is declared as a memo function, meaning that it remembers results
 * of previous calls.
 */

public class MemoFunction extends UserFunction {

    /**
     * Determine the preferred evaluation mode for this function
     */

    public void computeEvaluationMode() {
        evaluator = ExpressionTool.eagerEvaluator(getBody());
    }

    /**
     * Ask whether this function is a memo function
     *
     * @return true if this function is marked as a memo function
     */

    public boolean isMemoFunction() {
        return true;
    }

    /**
     * Call this function to return a value.
     *
     * @param actualArgs the arguments supplied to the function. These must have the correct
     *                   types required by the function signature (it is the caller's responsibility to check this).
     *                   It is acceptable to supply a {@link net.sf.saxon.value.Closure} to represent a value whose
     *                   evaluation will be delayed until it is needed. The array must be the correct size to match
     *                   the number of arguments: again, it is the caller's responsibility to check this.
     * @param context    This provides the run-time context for evaluating the function. It is the caller's
     *                   responsibility to allocate a "clean" context for the function to use; the context that is provided
     *                   will be overwritten by the function.
     * @return a Value representing the result of the function.
     */

    @Override
    public Sequence<?> call(XPathContext context, Sequence[] actualArgs) throws XPathException {

        // See if the result is already known
        String key = getCombinedKey(actualArgs);
        Controller controller = context.getController();
        HashMap<String, Sequence<?>> map = (HashMap<String, Sequence<?>>) controller.getUserData(this, "memo-function-cache");
        Sequence<?> value = map == null ? null : map.get(key);
        if (value != null) {
            return value;
        }

        value = super.call(context, actualArgs);

        // Save the result in the cache
        if (map == null) {
            map = new HashMap<>(32);
            controller.setUserData(this, "memo-function-cache", map);
        }
        map.put(key, value);

        return value;
    }

    /**
     * Get a key value representing the values of all the supplied arguments
     *
     * @param params the supplied values of the function arguments
     * @return a key value which can be used to index the cache of remembered function results
     * @throws XPathException if an error occurs evaluating the key, for example because one
     *                        of the supplied parameters uses lazy evaluation
     */

    private static String getCombinedKey(Sequence[] params) throws XPathException {
        FastStringBuffer sb = new FastStringBuffer(FastStringBuffer.C256);

        for (Sequence<?> val : params) {
            SequenceIterator<?> iter = val.iterate();
            Item item;
            while ((item = iter.next()) != null) {
                if (item instanceof NodeInfo) {
                    NodeInfo node = (NodeInfo) item;
                    node.generateId(sb);
                } else if (item instanceof QNameValue) {
                    sb.append("" + Type.displayTypeName(item));
                    sb.append('/');
                    sb.append(((QNameValue) item).getClarkName());
                } else if (item instanceof AtomicValue) {
                    sb.append("" + Type.displayTypeName(item));
                    sb.append('/');
                    sb.append(item.getStringValueCS());
                } else if (item instanceof Function) {
                    sb.append(item.getClass().getName() + "@" + System.identityHashCode(item));
                }
                sb.append('\u0001');
            }
            sb.append('\u0002');
        }
        return sb.toString();
    }

}

// Copyright (c) 2009 Saxonica Limited.


