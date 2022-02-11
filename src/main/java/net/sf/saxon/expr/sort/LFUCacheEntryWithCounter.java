package net.sf.saxon.expr.sort;

/**
 * An entry in a least-recently-used cache
 * @param <W> the type of object held in the cache
 * @implNote Was an inner class of LFUCache; separated for convenience of C# transpilation
 */

class LFUCacheEntryWithCounter<W> {

    public W value;
    int counter;

    public LFUCacheEntryWithCounter(W value) {
        this.value = value;
        this.counter = 0;
    }
}

