////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.IntegerValue;
import net.sf.saxon.value.SequenceExtent;

import java.util.ArrayList;
import java.util.Comparator;
import java.util.List;

/**
 * This class implements the function fn:sort-with#2, introduced in XPath 4.0
 */

public class SortWith extends SystemFunction {

    /**
     * Evaluate the expression
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as SequenceIterators
     * @return the result of the evaluation, in the form of a SequenceIterator
     * @throws XPathException if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        SequenceIterator inputIter = arguments[0].iterate();
        ArrayList<Item> inputList = new ArrayList<>();
        Item it;
        while ((it = inputIter.next()) != null) {
            inputList.add(it);
        }
        GroundedValue comparatorFunctions = arguments[1].materialize();
        List<Comparator<Item>> javaComparators = new ArrayList<>(comparatorFunctions.getLength());
        for (Item item : comparatorFunctions.asIterable()) {
            javaComparators.add((a, b) -> {
                try {
                    Sequence result = ((FunctionItem)item).call(context, new Sequence[]{a, b});
                    return Long.signum(((IntegerValue)result.head()).longValue());
                } catch (XPathException e) {
                    throw new UncheckedXPathException(e);
                }
            });
        }
        sort(inputList, javaComparators);
        return SequenceExtent.makeSequenceExtent(inputList);
    }


    private static void sort(List<Item> input, List<Comparator<Item>> comparators) {
        input.sort(multiComparator(comparators));
    }

    private static Comparator<Item> multiComparator(List<Comparator<Item>> comparators) {
        return (a,b) -> {
            for (Comparator<? super Item> comparator : comparators) {
                int c = comparator.compare(a, b);
                if (c != 0) {
                    return c;
                }
            }
            return 0;
        };
    }


}
