////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.expr.sort.AtomicSortComparer;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.Action;
import net.sf.saxon.om.LazySequence;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.AtomicValue;

import java.util.HashSet;

/**
 * The XPath 2.0 distinct-values() function, with the collation argument already known
 */

public class DistinctValues extends CollatingFunctionFixed {


    @Override
    public String getStreamerName() {
        return "DistinctValues";
    }

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        StringCollator collator = getStringCollator();
        boolean transitive = getRetainedStaticContext().getPackageData().getHostLanguageVersion() >= 40;
        return new LazySequence(new DistinctIterator(arguments[0].iterate(), collator, context, transitive));
    }


    /**
     * Iterator class to return the distinct values in a sequence
     */

    public static class DistinctIterator implements SequenceIterator {

        private final SequenceIterator base;
        private final StringCollator collator;
        private final XPathContext context;
        private final HashSet<AtomicMatchKey> lookup = new HashSet<>(40);
        private Action onDuplicates = null;
        private final boolean transitive;

        /**
         * Create an iterator over the distinct values in a sequence
         *
         * @param base     the input sequence. This must return atomic values only.
         * @param collator The comparer used to obtain comparison keys from each value;
         *                 these comparison keys are themselves compared using equals().
         * @param context the XPath dynamic context
         * @param transitive true if transitive equality semantics are used (specifically, for comparing
         *                   numeric values of different types)
         */

        public DistinctIterator(SequenceIterator base, StringCollator collator, XPathContext context, boolean transitive) {
            this.base = base;
            this.collator = collator;
            this.context = context;
            this.transitive = transitive;
        }

        /**
         * Get the next item in the sequence. <BR>
         *
         * @return the next item, or null if there are no more items.
         */

        @Override
        public AtomicValue next() {
            int implicitTimezone = context.getImplicitTimezone();
            while (true) {
                AtomicValue nextBase = (AtomicValue)base.next();
                if (nextBase == null) {
                    return null;
                }
                AtomicMatchKey key;
                if (nextBase.isNaN()) {
                    key = AtomicSortComparer.COLLATION_KEY_NaN;
                } else {
                    if (transitive) {
                        key = nextBase.getXPathTransitiveMatchKey(collator, implicitTimezone);
                    } else {
                        key = nextBase.getXPathMatchKey(collator, implicitTimezone);
                    }
                }
                if (lookup.add(key)) {
                    // returns true if newly added (if not, keep looking)
                    return nextBase;
                } else if (onDuplicates != null) {
                    try {
                        onDuplicates.doAction();
                    } catch (XPathException e) {
                        // should not happen
                        throw new UncheckedXPathException(e);
                    }
                }
            }
        }

        @Override
        public void close() {
            base.close();
        }

        public void notifyDuplicates(Action onDuplicates) {
            this.onDuplicates = onDuplicates;
        }

    }



}

