package net.sf.saxon.functions.hof;

import net.sf.saxon.expr.TailIterator;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.AtomicComparer;
import net.sf.saxon.expr.sort.GenericAtomicComparer;
import net.sf.saxon.functions.DeepEqual;
import net.sf.saxon.functions.Reverse;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.SingletonIterator;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.BooleanValue;

/**
 * This class supports the two extension functions starts-with-sequence,
 * ends-with-sequence, contains-sequence
 */

public abstract class SequenceMatching extends SystemFunction {

    protected BooleanValue startsWith(SequenceIterator iter0,
                                      SequenceIterator iter1,
                                      XPathContext context,
                                      Function comparator) throws XPathException {
        Item it0, it1;
        if (comparator == null) {
            AtomicComparer comparer = GenericAtomicComparer.makeAtomicComparer(
                    BuiltInAtomicType.ANY_ATOMIC,
                    BuiltInAtomicType.ANY_ATOMIC,
                    context.getConfiguration().getCollation(getRetainedStaticContext().getDefaultCollationName()),
                    context);
            while (true) {
                it0 = iter0.next();
                it1 = iter1.next();
                if (it1 == null) {
                    return BooleanValue.TRUE;
                }
                if (it0 == null) {
                    return BooleanValue.FALSE;
                }
                boolean match = DeepEqual.deepEqual(SingletonIterator.makeIterator(it0),
                                                    SingletonIterator.makeIterator(it1),
                                                    comparer, context, 0);
                if (!match) {
                    return BooleanValue.FALSE;
                }
            }
        } else {
            while (true) {
                it0 = iter0.next();
                it1 = iter1.next();
                if (it1 == null) {
                    return BooleanValue.TRUE;
                }
                if (it0 == null) {
                    return BooleanValue.FALSE;
                }
                Sequence result = comparator.call(context, new Sequence[]{it0, it1});
                if (!((BooleanValue) result.head()).getBooleanValue()) {
                    return BooleanValue.FALSE;
                }
            }
        }
    }


    public static class StartsWithSequence extends SequenceMatching {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            SequenceIterator iter0 = arguments[0].iterate();
            SequenceIterator iter1 = arguments[1].iterate();
            Function comparator = arguments.length == 3 ? (Function)arguments[2].head() : null;
            return startsWith(iter0, iter1, context, comparator);
        }
    }

    public static class EndsWithSequence extends SequenceMatching {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            SequenceIterator iter0 = Reverse.getReverseIterator(arguments[0].iterate());
            SequenceIterator iter1 = Reverse.getReverseIterator(arguments[1].iterate());
            Function comparator = arguments.length == 3 ? (Function) arguments[2].head() : null;
            return startsWith(iter0, iter1, context, comparator);
        }
    }

    public static class ContainsSequence extends SequenceMatching {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            GroundedValue seq0 = arguments[0].materialize();
            GroundedValue seq1 = arguments[1].materialize();
            Function comparator = arguments.length == 3 ? (Function) arguments[2].head() : null;
            int len0 = seq0.getLength();
            int len2 = seq1.getLength();
            if (len2 == 0) {
                return BooleanValue.TRUE;
            }
            int latestStart = len0 - len2;
            for (int i=0; i<=latestStart; i++) {
                SequenceIterator iter0 = TailIterator.make(seq0.iterate(), i+1);
                SequenceIterator iter1 = seq1.iterate();
                BooleanValue result = startsWith(iter0, iter1, context, comparator);
                if (result.getBooleanValue()) {
                    return result;
                }
            }
            return BooleanValue.FALSE;
        }
    }
}






// Copyright (c) 2018-2022 Saxonica Limited
