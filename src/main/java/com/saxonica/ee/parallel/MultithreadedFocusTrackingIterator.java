package com.saxonica.ee.parallel;

import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.trans.UncheckedXPathException;

/**
 * This is a thread-safe version of the parent class {@link FocusTrackingIterator}. It is identical
 * to the superclass except that the methods are synchronized. It is used when there is any possibility
 * that the iterator will be used from multiple threads.
 * <p>The class is not designed to allow arbitrary multi-threaded operation. For example, one thread
 * might exhaust the input after another thread has called {@link #hasNext()}, causing the first thread
 * to get no results from a call of {@link #next()}. Instead, the synchronization is designed only to
 * allow a call on {@link #getLength()} (caused by an XPath invocation of <code>last()</code>) to occur
 * asynchronously with the thread that advances the iterator. This happens specifically when an
 * <code>xsl:result-document</code> instruction is executed asynchronously, and the code within
 * the body of the <code>xsl:result-document</code> instruction calls <code>last()</code>. In this situation
 * the main thread (the one that advances the focus, and spawns new threads for <code>xsl:result-document</code>)
 * must be suspended while the value of <code>last()</code> is computed, which is done by reading the iterator
 * to completion; when the main thread resumes, there will be a new base iterator positioned over the materialized
 * sequence that was read into memory as a side-effect of calling <code>last()</code>.</p>
 * <p>See bug 3927.</p>
 */

public class MultithreadedFocusTrackingIterator extends FocusTrackingIterator {

    public MultithreadedFocusTrackingIterator(SequenceIterator base) {
        super(base);
    }

    /**
     * Get the underlying iterator
     *
     * @return the iterator underlying this FocusIterator
     */
    @Override
    public synchronized SequenceIterator getUnderlyingIterator() {
        return super.getUnderlyingIterator();
    }

    /**
     * Get the next item in the sequence. This method changes the state of the
     * iterator, in particular it affects the result of subsequent calls of
     * position() and current().
     *
     * @return the next item, or null if there are no more items. Once a call
     * on next() has returned null, no further calls should be made. The preferred
     * action for an iterator if subsequent calls on next() are made is to return
     * null again, and all implementations within Saxon follow this rule.
     * @since 8.4
     */
    @Override
    public synchronized Item next() {
        return super.next();
    }

    /**
     * Get the current value in the sequence (the one returned by the
     * most recent call on next()). This will be null before the first
     * call of next(). This method does not change the state of the iterator.
     *
     * @return the current item, the one most recently returned by a call on
     * next(). Returns null if next() has not been called, or if the end
     * of the sequence has been reached.
     * @since 8.4
     */
    @Override
    public synchronized Item current() {
        return super.current();
    }

    /**
     * Get the current position. This will usually be zero before the first call
     * on next(), otherwise it will be the number of times that next() has
     * been called. Once next() has returned null, the preferred action is
     * for subsequent calls on position() to return -1, but not all existing
     * implementations follow this practice. (In particular, the EmptyIterator
     * is stateless, and always returns 0 as the value of position(), whether
     * or not next() has been called.)
     * <p>This method does not change the state of the iterator.</p>
     *
     * @return the current position, the position of the item returned by the
     * most recent call of next(). This is 1 after next() has been successfully
     * called once, 2 after it has been called twice, and so on. If next() has
     * never been called, the method returns zero. If the end of the sequence
     * has been reached, the value returned will always be &lt;= 0; the preferred
     * value is -1.
     * @since 8.4
     */
    @Override
    public synchronized int position() {
        return super.position();
    }

    public boolean supportsGetLength() {
        return true;
    }

    /**
     * Get the position of the last item in the sequence. The method is stateless in its external effect:
     * that is, it does not change the values returned by position(), next(), hasNext(), etc. However,
     * it creates a new base iterator which means that the result of calling getUnderlyingIterator()
     * may change.
     *
     * @return the position of the last item
     */
    @Override
    public synchronized int getLength() {
        return super.getLength();
    }

    /**
     * Determine whether there are more items to come. Note that this operation
     * is stateless and it is not necessary (or usual) to call it before calling
     * next(). It is used only when there is an explicit need to tell if we
     * are at the last element.
     * <p>This method must not be called unless the result of {@link #supportsHasNext()} is true.</p>
     *
     * @return true if there are more items in the sequence
     * @throws ClassCastException if the base iterator does not support lookahead processing
     */
    @Override
    public synchronized boolean hasNext() {
        return super.hasNext();
    }

    /**
     * Return a GroundedValue containing all the items in the sequence returned by this
     * SequenceIterator. This should be an "in-memory" value, not a Closure.
     *
     * @return the corresponding Value
     * @throws UncheckedXPathException     in the cases of subclasses (such as the iterator over a MemoClosure)
     *                            which cause evaluation of expressions while materializing the value.
     */
    @Override
    public synchronized GroundedValue materialize()  {
        return SequenceTool.toGroundedValue(this);
    }

    /**
     * Return a GroundedValue containing all the remaining items in the sequence returned by this
     * SequenceIterator, starting at the current position. This should be an "in-memory" value, not a Closure.
     *
     * @return the corresponding Value
     * @throws UncheckedXPathException     in the cases of subclasses (such as the iterator over a MemoClosure)
     *                            which cause evaluation of expressions while materializing the value.
     */
    @Override
    public synchronized GroundedValue getResidue() {
        return super.getResidue();
    }

    /**
     * Close the iterator. This indicates to the supplier of the data that the client
     * does not require any more items to be delivered by the iterator. This may enable the
     * supplier to release resources. After calling close(), no further calls on the
     * iterator should be made; if further calls are made, the effect of such calls is undefined.
     * <p>(Currently, closing an iterator is important only when the data is being "pushed" in
     * another thread. Closing the iterator terminates that thread and means that it needs to do
     * no additional work. Indeed, failing to close the iterator may cause the push thread to hang
     * waiting for the buffer to be emptied.)</p>
     *
     * @since 9.1
     */
    @Override
    public synchronized void close() {
        super.close();
    }

    /**
     * Get the sibling position of a node: specifically, count how many preceding siblings
     * of a node satisfy the nodetest. This method appears here because it can potentially
     * make use of cached information. When an instruction such as {@code xsl:apply-templates
     * select="*"} (which selects a set of sibling nodes) is used in conjunction with patterns
     * such as {@code match="*[position() mod 2 = 1]}, then calculation of the position of one
     * node in the sequence of siblings can take advantage of the fact that the position of the
     * immediately preceding sibling is already known.
     *
     * <p>This optimization was suggested by the late Sebastian Rahtz, one of Saxon's earliest
     * power users, and it is dedicated to his memory.</p>
     *
     * @param node     the starting node, which is assumed to satisfy the node test
     * @param nodeTest the node test
     * @param max      the maximum number of nodes to be counted
     * @return the number of preceding siblings that satisfy the node test, plus one, unless the
     * number exceeds max, in which case return some number greater than or equal to max.
     */
    @Override
    public synchronized int getSiblingPosition(NodeInfo node, NodeTest nodeTest, int max) {
        return super.getSiblingPosition(node, nodeTest, max);
    }
}

// Copyright (c) 2018-2020 Saxonica Limited