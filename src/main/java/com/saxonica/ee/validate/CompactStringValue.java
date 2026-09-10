package com.saxonica.ee.validate;

import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.AtomicIterator;
import net.sf.saxon.tree.iter.SingleAtomicIterator;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.StringValue;

import java.util.Iterator;

/**
 * A compact representation of an atomic xs:string value used in saved tables for key and keyref constraints,
 * designed to economize on space.
 * @since 9.6
 */
public class CompactStringValue implements AtomicSequence {

    char[] value;

    // Note: we could go further and store a UTF-8 byte array. The difficulty would be that getSchemaComparable()
    // would either (a) have to reconstruct the string, which would make for expensive comparisons, or (b) generate
    // a value that does not compare equal with the original; which is not a problem if all strings are represented
    // using this class, but is problematic in the more general case. For the moment, getting the storage down to a
    // single char[] is a big win over 9.5 and earlier releases.

    public CompactStringValue(String content) {
        this.value = content.toCharArray();
    }

    @Override
    public StringValue head() {
        return new StringValue(new String(value));
    }

    @Override
    public AtomicIterator<StringValue> iterate() {
        return new SingleAtomicIterator<>(head());
    }

    @Override
    public AtomicValue itemAt(int n) {
        return n==0 ? head() : null;
    }

    @Override
    public int getLength() {
        return 1;
    }

    @Override
    public CharSequence getCanonicalLexicalRepresentation() {
        return head().getStringValueCS();
    }

    @Override
    public Comparable<?> getSchemaComparable() {
        return new String(value);
    }

    @Override
    public CharSequence getStringValueCS() {
        return head().getStringValueCS();
    }

    @Override
    public String getStringValue() {
        return head().getStringValue();
    }

    @Override
    public GroundedValue subsequence(int start, int length) {
        return head().subsequence(start, length);
    }

    @Override
    public boolean effectiveBooleanValue() throws XPathException {
        return value.length != 0;
    }

    @Override
    public GroundedValue reduce() {
        return head();
    }

    @Override
    public Iterator<AtomicValue> iterator() {
        return head().iterator();
    }

    /*@NotNull*/
    public String toString() {
        return "\"" + new String(value) + '\"';
    }

}

// Copyright (c) 2018-2020 Saxonica Limited
