package net.sf.saxon.om;

import net.sf.saxon.tree.jiter.WrappingJavaIterator;
import org.jetbrains.annotations.NotNull;

import java.util.Iterator;

public class GroundedValueAsIterable<T extends Item> implements Iterable<T> {

    private final GroundedValue value;
    public GroundedValueAsIterable(GroundedValue value) {
        this.value = value;
    }

    public @NotNull Iterator<T> iterator() {
        return new WrappingJavaIterator<T>(value.iterate());
    }

}

