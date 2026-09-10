package net.sf.saxon.expr.sort;

/**
 * A one-way linked chain of entries in a {@link CustomSet}
 * @param <T> The type of the value held in each link of the chain
 * @implNote This is a separate top-level class (rather than an inner class) for ease of
 * conversion to C#. Making it a separate class avoids the problem of the full class
 * name requiring two type parameters.
 */
class CustomSetEntryChain<T> {
    public T value;
    public CustomSetEntryChain<T> next;

    public CustomSetEntryChain(T value) {
        this.value = value;
        this.next = null;
    }
}

