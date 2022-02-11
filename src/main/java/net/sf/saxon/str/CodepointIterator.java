package net.sf.saxon.str;

import net.sf.saxon.tree.iter.AtomicIterator;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.z.IntIterator;

/**
 * Iterate over a string to produce a sequence of single character strings
 */
public class CodepointIterator implements AtomicIterator {

    final IntIterator codepoints;
    public CodepointIterator(IntIterator codepoints) {
        this.codepoints = codepoints;
    }

    @Override
    public AtomicValue next() {
        return codepoints.hasNext() ? new Int64Value(codepoints.next()) : null;
    }

}

