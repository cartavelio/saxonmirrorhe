package net.sf.saxon.tree.iter;

import net.sf.saxon.om.GNode;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.UncheckedXPathException;

public interface AxisIterator extends SequenceIterator {

    /**
     * Get the next item in the sequence. This method changes the state of the
     * iterator.
     *
     * @return the next item, or null if there are no more items. Once a call
     * on next() has returned null, no further calls should be made. The preferred
     * action for an iterator if subsequent calls on next() are made is to return
     * null again, and all implementations within Saxon follow this rule.
     * @throws UncheckedXPathException if an error occurs retrieving the next item
     * @since 8.4. Changed in 11 so it no longer throws a checked exception;
     * instead, any error that occurs is thrown as an unchecked exception.
     */
    @Override
    GNode next();
}

