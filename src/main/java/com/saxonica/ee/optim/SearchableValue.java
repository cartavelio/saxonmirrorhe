package com.saxonica.ee.optim;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.expr.sort.AtomicComparer;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.ArrayIterator;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.SequenceExtent;

import java.util.Arrays;

/**
 * A sequence that is held in a form that permits rapid searching, typically by use of indexes.
 * This is an abstract class with several implementations
 */

public abstract class SearchableValue implements GroundedValue {

    protected Item[] value = null;
    protected int end;

    public static SearchableValue makeSearchableValue(SequenceIterator iter) throws XPathException {
        Item[] array;
        int end;
        if (SequenceTool.supportsGetLength(iter)) {
            end = SequenceTool.getLength(iter);
            array = new Item[end];
            int i = 0;
            Item it;
            while ((it = iter.next()) != null) {
                array[i++] = it;
            }
        } else {
            int used = 0;
            array = new Item[20];
            Item it;
            while ((it = iter.next()) != null) {
                if (used >= array.length - 1) {
                    array = Arrays.copyOf(array, used * 2);
                }
                array[used++] = it;
            }

            if (array.length - used > 100) {
                array = Arrays.copyOf(array, used);
            }
            end = used;
        }
        if (end < 8) {
            return new SmallSearchableValue(array, end);
        } else {
            return new IndexedValue(array, end);
        }

    }

    /**
     * Return an Iterator to iterate over the values of a sequence. The value of every
     * expression can be regarded as a sequence, so this method is supported for all
     * expressions. This default implementation handles iteration for expressions that
     * return singleton values: for non-singleton expressions, the subclass must
     * provide its own implementation.
     *
     * @return a SequenceIterator that can be used to iterate over the result
     *         of the expression
     */

    /*@NotNull*/
    @Override
    public SequenceIterator iterate() {
        return new ArrayIterator.Of<>(value, 0, end);
    }

    /**
     * Get the n'th item in the sequence (starting from 0). This is defined for all
     * Values, but its real benefits come for a sequence Value stored extensionally
     * (or for a MemoClosure, once all the values have been read)
     *
     * @param n position of the required item, counting from zero.
     * @return the n'th item in the sequence, where the first item in the sequence is
     *         numbered zero. If n is negative or &gt;= the length of the sequence, returns null.
     */

    @Override
    public Item itemAt(int n) {
        if (n < 0 || n >= end) {
            return null;
        }
        return value[n];
    }

    /**
     * Get the length of the sequence
     */

    @Override
    public int getLength() {
        return end;
    }

    /**
     * Retrieve items that match a given index value
     *
     * @param use                   the expression whose value is to be matched
     * @param values                the set of values to be found
     * @param convertUntypedToOther true if untypedAtomic values are to be converted to the
     *                              type of the other operand
     * @param comparer              AtomicComparer used to compare values for equality (encapsulates a collation
     *                              if necessary)
     * @param context               the dynamic evaluation context @return an iterator over the items that match the specified values
     * @return an iterator over the items that matched
     * @throws XPathException if an error occurs
     */

    public abstract SequenceIterator findItems(Expression use, SequenceIterator values,
                                               boolean convertUntypedToOther,
                                               AtomicComparer comparer, XPathContext context) throws XPathException;

    /**
     * Get a subsequence of the value
     *
     * @param start  the index of the first item to be included in the result, counting from zero.
     *               A negative value is taken as zero. If the value is beyond the end of the sequence, an empty
     *               sequence is returned
     * @param length the number of items to be included in the result. Specify Integer.MAX_VALUE to
     *               get the subsequence up to the end of the base sequence. If the value is negative, an empty sequence
     *               is returned. If the value goes off the end of the sequence, the result returns items up to the end
     *               of the sequence
     * @return the required subsequence. If min is
     */
    @Override
    public GroundedValue subsequence(int start, int length) {
        // Method is not actually used
        if (length > end) {
            length = end;
        }
        return new SequenceExtent.Of<>(Arrays.asList(value).subList(start, start + length));
    }

    @Override
    public boolean effectiveBooleanValue() throws XPathException {
        return ExpressionTool.effectiveBooleanValue(iterate());
    }

    @Override
    public UnicodeString getUnicodeStringValue() throws XPathException {
        return SequenceTool.getStringValue(this);
    }

    @Override
    public String getStringValue() throws XPathException {
        return SequenceTool.stringify(this);
    }

    /**
     * Get the first item in the sequence.
     *
     * @return the first item in the sequence if there is one, or null if the sequence
     *         is empty
     */
    @Override
    public Item head() {
        return value.length > 0 ? value[0] : null;
    }

    /**
     * Reduce the sequence to its simplest form. If the value is an empty sequence, the result will be
     * EmptySequence.getInstance(). If the value is a single atomic value, the result will be an instance
     * of AtomicValue. If the value is a single item of any other kind, the result will be an instance
     * of SingletonItem. Otherwise, the result will typically be unchanged.
     *
     * @return the simplified sequence
     */
    @Override
    public GroundedValue reduce() {
        int len = getLength();
        if (len == 0) {
            return EmptySequence.getInstance();
        } else if (len == 1) {
            return itemAt(0);
        } else {
            return this;
        }
    }

}

// Copyright (c) 2018-2023 Saxonica Limited