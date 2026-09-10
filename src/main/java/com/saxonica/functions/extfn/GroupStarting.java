package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.ListIterator;

import java.util.ArrayList;
import java.util.List;

/**
 * This class supports the extension function saxon:group-starting. This takes two arguments:
 * the first is a sequence of items, the second is a function used as a predicate. The function
 * is applied to each item in the sequence: if the effective boolean value of the result is true,
 * the item is called a matching item. The result of the function is a sequence of arrays, where
 * the members of each array are single items, and the flattened sequence of arrays is the same
 * as the input sequence.
 * <p>
 * If the input sequence is empty, the result sequence is empty.
 * <p>
 * In all other cases (a) the first item in the result is an array containing those items
 * from the input that precede the first matching item; this array will be empty if the
 * first item in the input is a matching item; (b) every other item in the result is an non-empty array
 * containing a subsequence of the input in which the first member is a matching item and
 * the remaining members are non-matching items.
 */

public class GroupStarting extends SystemFunction {

    /**
     * Evaluate this function call at run-time
     *
     * @param context   The XPath dynamic evaluation context
     * @param arguments The values of the arguments to the function call.
     * @return the results of the function, as a sequence of arrays
     * @throws XPathException if a dynamic error occurs during evaluation of the function. The Saxon run-time
     *                        code will add information about the error location.
     */

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        Sequence input = arguments[0];
        FunctionItem function = (FunctionItem) arguments[1].head();
        return new LazySequence(new GroupStartingFunctionIterator(input.iterate(), function, context));
    }

    private static class GroupStartingFunctionIterator implements SequenceIterator {

        private final SequenceIterator baseIterator;
        private final FunctionItem function;
        private final XPathContext context;
        private Item pending = null;
        private boolean finished = false;

        public GroupStartingFunctionIterator(SequenceIterator in, FunctionItem f, XPathContext context) {
            this.baseIterator = in;
            this.function = f;
            this.context = context;
        }

        @Override
        public ArrayItem next() {
            if (finished) {
                return null;
            }
            List<Item> group = new ArrayList<>();
            if (pending != null) {
                group.add(pending);
            }
            try {
                for (Item item; (item = baseIterator.next()) != null; ) {
                    Sequence test = function.call(context, SequenceTool.fromItems(item));
                    boolean match = ExpressionTool.effectiveBooleanValue(test.head());
                    if (match) {
                        pending = item;
                        return makeSimpleArray(group);
                    } else {
                        group.add(item);
                    }
                }
            } catch (XPathException e) {
                throw new UncheckedXPathException(e);
            }
            pending = null;
            finished = true;
            return makeSimpleArray(group);
        }

        private SimpleArrayItem makeSimpleArray(List<Item> group) {
            try {
                return SimpleArrayItem.makeSimpleArrayItem(new ListIterator.Of<>(group));
            } catch (XPathException e) {
                throw new AssertionError(e);
            }
        }

        @Override
        public void close() {
            baseIterator.close();
        }

        @Override
        public void discharge() {
            baseIterator.discharge();
        }

    }





}

// Copyright (c) 2005-2022 Saxonica Limited
