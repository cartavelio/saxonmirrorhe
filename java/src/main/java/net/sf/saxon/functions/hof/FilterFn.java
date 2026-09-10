////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions.hof;

import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.ContextItemStaticInfo;
import net.sf.saxon.expr.parser.ExpressionVisitor;
import net.sf.saxon.expr.parser.RetainedStaticContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.Int64Value;

import java.util.ArrayList;
import java.util.List;

/**
 * This class implements the function fn:filter(), which is a standard function in XQuery 3.0;
 * it is extended in 4.0 so the callback function also has access to the current position in the sequence
 */

public class FilterFn extends SystemFunction {

    /**
     * Attempt to rewrite fn:filter(S, F) as S[F(.)]
     * @param visitor the expression visitor
     * @param contextInfo information about the context item
     * @param arguments the supplied arguments to the function call. Note: modifying the contents
     *                  of this array should not be attempted, it is likely to have no effect.
     * @return
     * @throws XPathException
     */

    public Expression makeOptimizedFunctionCall(
            ExpressionVisitor visitor, ContextItemStaticInfo contextInfo, Expression... arguments)
            throws XPathException {
        if (arguments[1] instanceof FunctionLiteral f &&
                f.getGroundedValue() instanceof InlineFunctionItem ifi) {
            RetainedStaticContext rsc = arguments[0].getRetainedStaticContext();
            List<Expression> callbackArgs = new ArrayList<>(2);
            if (ifi.getArity() > 0) {
                callbackArgs.add(new ContextItemExpression());
            }
            if (ifi.getArity() > 1) {
                callbackArgs.add(SystemFunction.makeCall("position", rsc));
            }
            DynamicFunctionCall dynamicFunctionCall = new DynamicFunctionCall(f, callbackArgs);
            Expression predicate = visitor.obtainOptimizer().tryInlineDynamicFunctionCall(
                    dynamicFunctionCall, visitor, contextInfo);
            return new FilterExpression(arguments[0], predicate)
                    .typeCheck(visitor, contextInfo)
                    .optimize(visitor, contextInfo);
        }
        return super.makeOptimizedFunctionCall(visitor, contextInfo, arguments);
    }

    /**
     * Evaluate the expression
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as SequenceIterators
     * @return the result of the evaluation, in the form of a SequenceIterator
     * @throws net.sf.saxon.trans.XPathException
     *          if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        return SequenceTool.toLazySequence(
                evalFilter((FunctionItem) arguments[1].head(),
                           arguments[0].iterate(), context));
    }

    private SequenceIterator evalFilter(
            final FunctionItem function, SequenceIterator basis, final XPathContext context) throws XPathException {
        switch (function.getArity()) {
            case 0: {
                final BooleanValue head = (BooleanValue) dynamicCall(function, context).head();
                return head != null && head.getBooleanValue() ? basis : EmptyIterator.INSTANCE;
            }
            case 1:
                return ItemMappingIterator.filter(
                        basis,
                        item -> {
                            final BooleanValue head = (BooleanValue) dynamicCall(function, context, item).head();
                            return head != null && head.getBooleanValue();
                        });
            case 2:
                // New in 4.0, position() as a second argument
                return new PositionalFilterIterator(
                            basis,
                            (item, pos) -> {
                                try {
                                    Int64Value position = Int64Value.makeIntegerValue(pos);
                                    BooleanValue head = (BooleanValue) dynamicCall(function, context, item, position).head();
                                    return head != null && head.getBooleanValue();
                                } catch (XPathException err) {
                                    throw new UncheckedXPathException(err);
                                }
                            });

            default:
                throw new XPathException("Unsupported arity " + function.getArity() + " in fn:filter callback");
        }
    }

    @Override
    public String getStreamerName() {
        return "FilterFn";
    }

    public static class PositionalFilterIterator implements SequenceIterator {

        private final SequenceIterator basis;
        private final java.util.function.BiFunction<Item, Integer, Boolean> predicate;
        private int position = 0;

        public PositionalFilterIterator(SequenceIterator basis, java.util.function.BiFunction<Item, Integer, Boolean> predicate) {
            this.basis = basis;
            this.predicate = predicate;
        }

        /**
         * Get the next item in the sequence. This method changes the state of the
         * iterator.
         */
        @Override
        public Item next() {
            Item nextIn;
            do {
                nextIn = basis.next();
                position++;
            } while (nextIn != null && !predicate.apply(nextIn, position));
            return nextIn;
        }

        /**
         * Close the iterator.
         */
        @Override
        public void close() {
            basis.close();
        }
    }
}


// Copyright (c) 2018-2026 Saxonica Limited