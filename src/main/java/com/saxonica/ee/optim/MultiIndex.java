package com.saxonica.ee.optim;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.LastPositionFinder;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.tree.iter.LookaheadIterator;
import net.sf.saxon.type.*;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.z.*;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

/**
 * A MultiIndex provides fast access to the items in a sequence. The sequence may consist of nodes
 * and/or atomic values. The items are identified by means of a key value computed
 * using an expression called the "use" expression.
 * This may be multivalued: each item in the result of the use expression is regarded as a separate key, and
 * a match occurs if any of the items is matched.
 * <p>A MultiIndex handles a single "use" expression, for a single population.</p>
 * <p>The class is used to support indexes over the value of a variable, as determined by the Saxon-EE optimizer.</p>
 * <p>The class supports two kinds of equality matching. In <b>ConvertUntypedToOther</b> mode, corresponding
 * to the XPath "=" operator, an untyped value is converted to the type of the other item, so the double 5e0 will
 * match the untypedAtomic value "5". In <b>ConvertUntypedToString</b> mode, untyped atomic values are always
 * converted to strings: this corresponds to the XPath "eq" operator (and to the rules for xsl:key).</p>
 * <p>Each MultiIndex index is in fact a set of sub-indexes, one for each primitive type encountered.
 * In an expression such as $seq[./A = $B], one sub-index is built for each primitive type that occurs in the
 * sequence $B. In this index, any untyped atomic values in ./A are converted to that primitive type. For an expression
 * such as $seq[./A eq $B], a single index is built, in which untyped atomic values in ./A are converted to strings.</p>
 * <p>A particular difficulty arises where $B (in the expression $seq[./A eq $B]) is untypedAtomic. In this situation
 * an index is built in which any untypedAtomic values in ./A are indexed as strings. While building the index, we
 * note all the primitive types that occur in ./A, and then we convert the search term $B to each of these types in turn
 * before doing the index lookup. In practice, of course, it is very rare for the index to be heterogeneous.</p>
 */
public class MultiIndex {

    private final GroundedValue indexedValue;

    private final List<ValueIndex> indexList = new ArrayList<>(4);
    // An list of sub-indexes: the entries are ValueIndex objects. Each sub-index handles one primitive
    // type. Usually there is only one sub-index, because all the values are the same type. In principle
    // there can be one for each primitive type returned by the use expression (for example xs:boolean,
    // xs:double, xs:date) but this will only work if the sought key value is untypedAtomic and the equality
    // matching mode is ConvertUntypedToOther.

    /**
     * Create a MultiIndex
     *
     * @param indexedValue the sequence to be indexed
     */

    public MultiIndex(GroundedValue indexedValue) {
        this.indexedValue = indexedValue;
    }

    /**
     * Build an index over the items in the sequence, for one particular target type
     *
     * @param use         the expression returning the atomic values to act as the indexing keys
     * @param untypedType the type to which untypedAtomic values are to be converted
     * @param tester      callback to test the primitive type of the items
     * @param context     dynamic context for evaluating the indexing expression
     * @param collator    Collation used to get a comparison key for indexing purposes
     * @return the constructed index
     * @throws net.sf.saxon.trans.XPathException
     *          if a dynamic error occurs
     */

    private ValueIndex buildIndex(Expression use, BuiltInAtomicType untypedType,
                                  PrimitiveTypeTester tester,
                                  XPathContext context, StringCollator collator)
            throws XPathException {
        // Construct an index as a HashMap. The key is the "use" value, the value is
        // the integer position of the item being indexed in the array (zero-based).
        // We don't hold the items directly because we need to return them in sorted order,
        // sans duplicates.
        HashMap<AtomicMatchKey, MonotonicIntSet> index = new HashMap<>(100);
        ValueIndex vi = new ValueIndex(untypedType, index);
        IntHashSet typesPresent = new IntHashSet(4);
        XPathContext c2 = context.newContext();
        FocusIterator iter = c2.trackFocus(indexedValue.iterate());
        ConversionRules rules = context.getConfiguration().getConversionRules();
        StringConverter untypedConverter = untypedType.getStringConverter(rules);
        int implicitTimezone = context.getImplicitTimezone();

        int pos = 0;

        // iterate over all the values in the population
        while (iter.next() != null) {
            // iterate over the result of the use expression
            SequenceIterator vals = use.iterate(c2);
            AtomicValue val;
            while ((val = (AtomicValue) vals.next()) != null) {
                // if the "use" value is untyped, convert it to the target type
                AtomicValue val2 = val;
                if (val.isUntypedAtomic()) {
                    ConversionResult result = untypedConverter.convert(val);
                    if (result instanceof ValidationFailure) {
                        String msg = "Failure converting untypedAtomic value to " +
                                untypedType.getDisplayName() +
                                " during comparison operation. " +
                                ((ValidationFailure) result).getMessage();
                        XPathException de = new XPathException(msg);
                        de.setErrorCode("FORG0001");
                        de.setXPathContext(context);
                        throw de;
                    } else {
                        val2 = (AtomicValue) result;
                    }
                }
                // maintain the set of all types encountered in the index
                boolean added = typesPresent.add(val2.getPrimitiveType().getFingerprint());

                // if it's a new type, then check that it's allowed
                if (added) {
                    tester.test(val.getPrimitiveType());
                }

                // calculate the comparison key, and add the position of this item in the
                // population to the index entry for this comparison key
                AtomicMatchKey x = val2.getXPathMatchKey(collator, implicitTimezone);
                MonotonicIntSet list = index.get(x);
                if (list == null) {
                    list = new MonotonicIntSet();
                    index.put(x, list);
                }
                list.add(pos);
                //targetHandler.addToNodeList(list, item, pos);
            }
            pos++;
        }
        vi.typesPresent = new IntArraySet(typesPresent);
        return vi;
    }

    /**
     * Find the index for a given "use" expression and target type,
     * or return null if it doesn't yet exist
     *
     * @param soughtType the type of value being sought
     * @return the index for this type if it exists
     */

    private ValueIndex findIndex(BuiltInAtomicType soughtType) {
        for (int i = indexList.size() - 1; i >= 0; i--) {
            ValueIndex vi = indexList.get(i);
            if (vi.untypedType.equals(soughtType)) {
                return vi;
            }
        }
        return null;
    }

    /**
     * Retrieve items that match a given index value
     *
     * @param use                   the expression whose value is to be matched
     * @param values                the set of values to be found
     * @param convertUntypedToOther true if untypedAtomic values are to be converted to the
     *                              type of the other operand
     * @param firstOnly             true if only the first matching value is needed
     * @param collator              StringCollator used to compare values for equality (encapsulates a collation
     *                              if necessary)
     * @param context               the dynamic evaluation context
     * @return an iterator over the items that match the specified values
     * @throws net.sf.saxon.trans.XPathException
     *          if a failure occurs
     */

    /*@Nullable*/
    public SequenceIterator findItems(Expression use, SequenceIterator values,
                                      boolean convertUntypedToOther, boolean firstOnly,
                                      StringCollator collator, XPathContext context) throws XPathException {
        //System.err.println("*** Using index");

        // Accumulate the positions of all required items
        IntSet selectedItems = null;
        ConversionRules rules = context.getConfiguration().getConversionRules();
        int implicitTimezone = context.getImplicitTimezone();

        // iterate over all the values being sought
        AtomicValue value;
        while ((value = (AtomicValue)values.next()) != null) {
            BuiltInAtomicType prim;
            if (convertUntypedToOther) {
                prim = value.getPrimitiveType();
                if (prim.equals(BuiltInAtomicType.INTEGER)) {
                    prim = BuiltInAtomicType.DECIMAL;
                }
            } else {
                prim = BuiltInAtomicType.STRING;
            }
            final BuiltInAtomicType soughtType = value.getPrimitiveType();

            // find the index relevant to the type of this value
            ValueIndex vi = findIndex(prim);

            // if no index exists for this type, build it now
            if (vi == null) {
                PrimitiveTypeTester tester;
                if (convertUntypedToOther) {
                    tester = type -> {
                        if (type == BuiltInAtomicType.UNTYPED_ATOMIC ||
                                soughtType == BuiltInAtomicType.UNTYPED_ATOMIC) {
                            return;
                        }
                        if (!Type.isGuaranteedComparable(type, soughtType, false)) {
                            String message =
                                    "General comparison: cannot compare " + type + " to " + soughtType;
                            XPathException err = new XPathException(message);
                            err.setErrorCode("XPTY0004");
                            err.setIsTypeError(true);
                            throw err;
                        }
                    };
                } else {
                    tester = type -> {
                        BuiltInAtomicType t0 = type;
                        if (t0 == BuiltInAtomicType.UNTYPED_ATOMIC) {
                            t0 = BuiltInAtomicType.STRING;
                        }
                        BuiltInAtomicType t1 = soughtType;
                        if (t1 == BuiltInAtomicType.UNTYPED_ATOMIC) {
                            t1 = BuiltInAtomicType.STRING;
                        }
                        if (!Type.isGuaranteedComparable(t0, t1, false)) {
                            String message = "Value comparison: cannot compare " + type + " to " + soughtType;
                            XPathException err = new XPathException(message);
                            err.setErrorCode("XPTY0004");
                            err.setIsTypeError(true);
                            throw err;
                        }
                    };
                }
                vi = buildIndex(use, prim, tester, context, collator);
                indexList.add(vi);
            } else {
                // the index already exists, we need to check that the type we are looking for is comparable
                // with each of the types present in the index.
                // NOTE: the optimization rules actually allow us to dispense with this check
                if (soughtType != BuiltInAtomicType.UNTYPED_ATOMIC &&
                        !vi.typesChecked.contains(soughtType.getFingerprint())) {
                    IntIterator ii = vi.typesPresent.iterator();
                    // iterate for each type found in the result of the "use" expression
                    while (ii.hasNext()) {
                        int type = ii.next();
                        BuiltInAtomicType indexedType = (BuiltInAtomicType) BuiltInType.getSchemaType(type);
                        if (!Type.isGuaranteedComparable(soughtType, indexedType, false)) {
                            String message = "Cannot compare " + indexedType + " to " + soughtType;
                            XPathException err = new XPathException(message);
                            err.setErrorCode("XPTY0004");
                            err.setIsTypeError(true);
                            throw err;
                        }
                    }
                    vi.typesChecked.add(soughtType.getFingerprint());
                }
            }

            // if the value we are looking for is untyped atomic, we have to convert it to each
            // of the types present in the index in turn, looking for a match against each one. If
            // any of the conversions fails, we have a fatal error
            if (prim.equals(BuiltInAtomicType.UNTYPED_ATOMIC)) {
                IntIterator ii = vi.typesPresent.iterator();
                // iterate for each type found in the result of the "use" expression
                while (ii.hasNext()) {
                    int type = ii.next();
                    final BuiltInAtomicType requiredType = (BuiltInAtomicType) BuiltInType.getSchemaType(type);
                    AtomicValue conv;

                    // convert the value being sought to this type
                    ConversionResult result = Converter.convert(value, requiredType, rules);
                    if (result instanceof ValidationFailure) {
                        String msg = "Failure converting untypedAtomic value to " +
                                requiredType.getDisplayName() +
                                " during comparison operation. " +
                                ((ValidationFailure) result).getMessage();
                        XPathException de = new XPathException(msg);
                        de.setErrorCode("FORG0001");
                        de.setXPathContext(context);
                        throw de;
                    } else {
                        conv = (AtomicValue) result;
                    }

                    // look up the index to find the positions of items with this "use" value,
                    // and combine it with the result found for other values being sought at the same time
                    IntSet list = vi.index.get(conv.getXPathMatchKey(collator, implicitTimezone));
                    if (list != null) {
                        if (selectedItems == null) {
                            selectedItems = list.copy();
                        } else {
                            selectedItems = selectedItems.union(list);
                        }
                    }
                }

            } else {
                // The value being sought is not untyped atomic.
                // Look up the index to find the positions of items with this "use" value,
                // and combine it with the result found for other values being sought at the same time
                IntSet list = vi.index.get(value.getXPathMatchKey(collator, implicitTimezone));
                if (list != null) {
                    if (selectedItems == null) {
                        selectedItems = list.copy();
                        //selectedItems = targetHandler.copyNodeList(list);
                    } else {
                        selectedItems = selectedItems.union(list);
                        //selectedItems = targetHandler.mergeNodeLists(selectedItems, list);
                    }
                }
            }
            if (firstOnly && selectedItems != null && !selectedItems.isEmpty()) {
                IntIterator it = selectedItems.iterator();
                if (it.hasNext()) {
                    // always true
                    return singletonIterator(it.next());
                }
            }
        }

        // if no values found, return an empty sequence
        if (selectedItems == null) {
            return EmptyIterator.getInstance();
        }

        int size = selectedItems.size();
        if (size == 0) {
            return EmptyIterator.getInstance();
        } else if (size == 1) {
            IntIterator it = selectedItems.iterator();
            if (it.hasNext()) {
                // always true
                return singletonIterator(it.next());
            }
        }
        return new SelectedItemsIterator(indexedValue, selectedItems);
    }

    private SequenceIterator singletonIterator(int pos) throws XPathException{
        return SequenceTool.itemAt(indexedValue, pos).iterate();
    }

    /**
     * Iterator over a subset of the items in this IndexedValue, identified by an IntSet holding
     * the positions of the items in the selected subset (zero-based)
     */

    public static class SelectedItemsIterator
            implements SequenceIterator, LastPositionFinder, LookaheadIterator {

        IntSet selection;
        IntIterator iter;
        int current = 0;
        GroundedValue indexedValue;

        public SelectedItemsIterator(GroundedValue indexedValue, IntSet selection) {
            this.indexedValue = indexedValue;
            this.selection = selection;
            iter = selection.iterator();
        }

        @Override
        public boolean supportsHasNext() {
            return true;
        }


        /**
         * Determine whether there are more items to come. Note that this operation
         * is stateless and it is not necessary (or usual) to call it before calling
         * next(). It is used only when there is an explicit need to tell if we
         * are at the last element.
         * <p>This method must not be called unless the result of {@link #supportsHasNext()}
         * is true.</p>
         *
         * @return true if there are more items in the sequence
         */

        @Override
        public boolean hasNext() {
            return iter.hasNext();
        }

        /**
         * Get the next item in the sequence. This method changes the state of the
         * iterator, in particular it affects the result of subsequent calls of
         * position() and current().
         *
         * @return the next item, or null if there are no more items. Once a call
         *         on next() has returned null, no further calls should be made. The preferred
         *         action for an iterator if subsequent calls on next() are made is to return
         *         null again, and all implementations within Saxon follow this rule.
         * @since 8.4
         */

        @Override
        public Item next() {
            if (iter.hasNext()) {
                current = iter.next();
                return SequenceTool.itemAt(indexedValue, current);
            } else {
                return null;
            }
        }

        /**
         * Ask whether this iterator supports use of the {@link #getLength()} method. This
         * method should always be called before calling {@link #getLength()}, because an iterator
         * that implements this interface may support use of {@link #getLength()} in some situations
         * and not in others
         *
         * @return true if the {@link #getLength()} method can be called to determine the length
         * of the underlying sequence.
         */
        @Override
        public boolean supportsGetLength() {
            return true;
        }

        /**
         * Get the last position (that is, the number of items in the sequence). This method is
         * non-destructive: it does not change the state of the iterator.
         * <p>The result is undefined if the next() method of the iterator has already returned null.</p>
         * <p>This method must not be called unless the result of {@link #supportsGetLength()}
         * is true.</p>
         */

        @Override
        public int getLength() {
            return selection.size();
        }

    }


    /**
     * An index
     */

    private static class ValueIndex {

        public BuiltInAtomicType untypedType;   // the type to which untyped atomic values are converted
        public IntSet typesPresent;             // the types of value that are present in this index
        public IntSet typesChecked;             // the types that are known to be safely comparable with all
        // types present in the index
        public HashMap<AtomicMatchKey, MonotonicIntSet> index;
        // index that maps atomic values to lists of integer positions in the
        // indexed sequence

        public ValueIndex(BuiltInAtomicType untypedType, HashMap<AtomicMatchKey, MonotonicIntSet> index) {
            this.untypedType = untypedType;
            typesPresent = new IntArraySet();
            typesChecked = new IntArraySet();
            this.index = index;
        }
    }

    @FunctionalInterface
    private interface PrimitiveTypeTester {
        void test(BuiltInAtomicType type) throws XPathException;
    }


}

// Copyright (c) 2006-2022 Saxonica Limited