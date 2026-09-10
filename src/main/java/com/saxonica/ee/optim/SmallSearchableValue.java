package com.saxonica.ee.optim;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.GeneralComparison;
import net.sf.saxon.expr.ValueComparison;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Token;
import net.sf.saxon.expr.sort.AtomicComparer;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.tree.iter.ListIterator;
import net.sf.saxon.tree.iter.ManualIterator;
import net.sf.saxon.tree.iter.SingletonIterator;
import net.sf.saxon.value.AtomicValue;

import java.util.ArrayList;
import java.util.List;

/**
 * A SearchableValue that is sufficiently small that construction of an index is
 * not worthwhile
 */

class SmallSearchableValue extends SearchableValue {

    SmallSearchableValue(Item[] array, int end) {
        this.value = array;
        this.end = end;
    }

    @FunctionalInterface
    private interface EqualityComparer {
        boolean compare(AtomicValue a, AtomicValue b) throws XPathException;
    }

    @Override
    public SequenceIterator findItems(Expression use, SequenceIterator values,
                                               boolean convertUntypedToOther, boolean firstOnly,
                                               AtomicComparer comparer, XPathContext context) throws XPathException {
        List<Item> selected = new ArrayList<>();

        XPathContext c2 = context.newContext();
        ManualIterator manual = new ManualIterator();
        c2.setCurrentIterator(manual);

        List<AtomicValue> soughtValues = new ArrayList<>();
        SequenceTool.supply(values, (ItemConsumer<? super Item>) item -> soughtValues.add((AtomicValue) item));
        if (soughtValues.isEmpty()) {
            return EmptyIterator.getInstance();
        }

        EqualityComparer compare;
        if (convertUntypedToOther) {
            compare = (a, b) -> GeneralComparison.compare(a, Token.FEQ, b, comparer, true, c2, use.getRetainedStaticContext());
        } else {
            compare = (a, b) -> ValueComparison.compare(a, Token.FEQ, b, comparer, true);
        }

        for (int i=0; i<end; i++) {
            manual.setContextItem(value[i]);
            SequenceIterator keys = use.iterate(c2);
            Item key;
            while ((key = keys.next()) != null) {
                AtomicSequence atomized = key.atomize();
                for (AtomicValue sought : soughtValues) {
                    for (AtomicValue val : atomized) {
                        if (compare.compare(val, sought)) {
                            if (firstOnly) {
                                return SingletonIterator.makeIterator(value[i]);
                            } else {
                                selected.add(value[i]);
                            }
                        }
                    }
                }
            }
        }

        return new ListIterator.Of<>(selected);


    }

}

// Copyright (c) 2018-2023 Saxonica Limited