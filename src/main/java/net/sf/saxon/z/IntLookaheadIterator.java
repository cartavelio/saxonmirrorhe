package net.sf.saxon.z;

/**
 * An {@link IntIterator} that looks ahead by one item (providing a peekAhead method to
 * access the next item without consuming it). The value -1 is treated specially,
 * used to indicate the end of the sequence.
 */

public class IntLookaheadIterator implements IntIterator {
    private final IntIterator baseIterator;
    private int pendingChar;
    private int currentChar = -1;

    /**
     * Create an iterator that provides a single-item lookahead relative to a supplied iterator
     *
     * @param baseIterator the supplied iterator. A lookahead value of -1 indicates
     *                     the end of the stream, so the base iterator should not return
     *                     -1 as an ordinary result.
     */
    public IntLookaheadIterator(IntIterator baseIterator) {
        this.baseIterator = baseIterator;
        this.pendingChar = baseIterator.hasNext() ? baseIterator.next() : -1;
    }

    /**
     * Test whether there are any more integers in the sequence; and change the state of
     * the iterator so a call on {@link #next()} delivers the next integer.
     *
     * <p>The effect of calling {@code #hasNext()} a second time without an intervening
     * call on {@code #next()} is undefined.</p>
     *
     * @return true if there are more integers to come
     */
    @Override
    public boolean hasNext() {
        return pendingChar != -1;
    }

    /**
     * Return the next integer in the sequence. The result is undefined unless {@code #hasNext()} has been called
     * and has returned true.
     *
     * @return the next integer in the sequence
     */
    @Override
    public int next() {
        currentChar = pendingChar;
        pendingChar = baseIterator.hasNext() ? baseIterator.next() : -1;
        return currentChar;
    }

    /**
     * Get the integer that has been most recently read
     *
     * @return the integer most recently returned by the next() method
     */

    public int current() {
        return currentChar;
    }

    /**
     * Return the next integer that will be read, without advancing the position
     *
     * @return the next integer in the stream, or -1 if there are no more to come.
     */
    public int peekAhead() {
        return pendingChar;
    }
}

