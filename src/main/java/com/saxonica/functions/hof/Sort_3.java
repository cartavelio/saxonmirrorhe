package com.saxonica.functions.hof;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.Sort_2;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;

import java.util.ArrayList;
import java.util.List;

/**
 * This class implements the function fn:sort#2, which is a higher-order function in XPath 3.1
 * available only in Saxon-PE and above
 */

public class Sort_3 extends Sort_2 {

    /**
     * Evaluate the expression
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as SequenceIterators
     * @return the result of the evaluation, in the form of a SequenceIterator
     * @throws XPathException if a dynamic error occurs during the evaluation of the expression
     */
    public Sequence<?> call(XPathContext context, Sequence[] arguments) throws XPathException {
        Sequence<?> input = arguments[0];
        final List<ItemToBeSorted> inputList = new ArrayList<>();
        int i = 0;
        Function key = (Function) arguments[2].head();
        SequenceIterator<?> iterator = input.iterate();
        Item item;
        while ((item = iterator.next()) != null) {
            ItemToBeSorted member = new ItemToBeSorted();
            member.value = item;
            member.originalPosition = i++;
            member.sortKey = dynamicCall(key, context, new Sequence[]{item}).materialize();
            inputList.add(member);
        }
        return doSort(inputList, getCollation(context, arguments[1]), context);
    }


}


// Copyright (c) 2018 Saxonica Limited.