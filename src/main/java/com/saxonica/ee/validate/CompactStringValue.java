package com.saxonica.ee.validate;

import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.str.Slice8;
import net.sf.saxon.str.Twine8;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.AtomicIterator;
import net.sf.saxon.tree.iter.SingleAtomicIterator;
import net.sf.saxon.tree.jiter.MonoIterator;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.z.IntIterator;

import java.util.Arrays;
import java.util.Iterator;

/**
 * A compact representation of an atomic xs:string value used in saved tables for key and keyref constraints,
 * designed to economize on space, used only when the string consists exclusively of 8-bit characters
 * @since 9.6. Changed in 13.0 to wrap byte[] rather than char[]
 */
public class CompactStringValue implements AtomicSequence {

    private final byte[] value;

    public CompactStringValue(byte[] content) {
        this.value = content;
    }

    /**
     * Try to make a CompactStringValue for a supplied UnicodeString
     * @param str the supplied string
     * @return a CompactStringValue if possible (specifically, if the characters are all <= 255); otherwise return null
     */
    public static CompactStringValue tryToMake(UnicodeString str) {
        if (str instanceof Twine8) {
            return new CompactStringValue(((Twine8)str).getByteArray());
        } else if (str instanceof Slice8) {
            return new CompactStringValue(((Slice8) str).asByteArray());
        } else {
            int len = str.length32();
            byte[] bytes = new byte[len];
            IntIterator iter = str.codePoints();
            int p=0;
            while (iter.hasNext()) {
                int cp = iter.next();
                if (cp > 255) {
                    return null;
                } else {
                    bytes[p++] = (byte)(cp & 255);
                }
            }
            return new CompactStringValue(bytes);
        }
    }

    @Override
    public StringValue head() {
        return new StringValue(new Twine8(value));
    }

    @Override
    public AtomicIterator iterate() {
        return SingleAtomicIterator.makeIterator(head());
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
    public UnicodeString getCanonicalLexicalRepresentation() {
        return getUnicodeStringValue();
    }

    @Override
    public boolean equals(Object obj) {
        return obj instanceof CompactStringValue && Arrays.equals(value, ((CompactStringValue) obj).value);
    }

    @Override
    public int hashCode() {
        int h = 0;
        for (int cp : value) {
            h = 31 * h + cp;
        }
        return h;
    }

    @Override
    public UnicodeString getUnicodeStringValue() {
        return new Twine8(value);
    }

    @Override
    public String getStringValue() throws XPathException {
        return getUnicodeStringValue().toString();
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
        return new MonoIterator<>(head());
    }

    /*@NotNull*/
    public String toString() {
        return getUnicodeStringValue().toString();
    }


}

// Copyright (c) 2018-2023 Saxonica Limited
