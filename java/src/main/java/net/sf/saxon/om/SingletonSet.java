package net.sf.saxon.om;

import net.sf.saxon.tree.jiter.MonoIterator;

import java.util.AbstractSet;
import java.util.Iterator;

public class SingletonSet<T> extends AbstractSet<T> {

    private T element;

    public SingletonSet(T element) {
        this.element = element;
    }

    /**
     * Returns an iterator over the elements contained in this collection.
     * @return an iterator over the elements contained in this collection
     */
    @Override
    public Iterator<T> iterator() {
        return new MonoIterator<>(element);
    }

    @Override
    public int size() {
        return 1;
    }
}


