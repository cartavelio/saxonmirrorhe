package net.sf.saxon.tree.iter;

import net.sf.saxon.om.Item;

import java.util.Iterator;
import java.util.function.Function;

public class SequenceIteratorOverJavaIterator<J> implements UnfailingIterator {

    private final Iterator<J> javaIterator;
    private final Function<J, Item> mapper;

    public SequenceIteratorOverJavaIterator(Iterator<J> javaIterator, Function<J, Item> mapper) {
        this.javaIterator = javaIterator;
        this.mapper = mapper;
    }

    @Override
    public Item next() {
        if (javaIterator.hasNext()) {
            return mapper.apply(javaIterator.next());
        } else {
            return null;
        }
    }
}


