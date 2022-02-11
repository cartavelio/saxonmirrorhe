package net.sf.saxon.tree.iter;

import net.sf.saxon.om.Item;
import net.sf.saxon.transpile.CSharpSuppressWarnings;

import java.util.Iterator;
import java.util.function.Function;

public class SequenceIteratorOverJavaIterator<J> implements UnfailingIterator {

    // See also class IteratorWrapper, which does the same thing without mapping,
    // and WrappingJavaIterator, which does the converse.

    private final Iterator<J> javaIterator;
    private final Function<J, Item> mapper;

    public SequenceIteratorOverJavaIterator(Iterator<J> javaIterator, Function<J, Item> mapper) {
        this.javaIterator = javaIterator;
        this.mapper = mapper;
    }

    @Override
    @CSharpSuppressWarnings("UnsafeIteratorConversion")
    public Item next() {
        if (javaIterator.hasNext()) {
            return mapper.apply(javaIterator.next());
        } else {
            return null;
        }
    }
}


