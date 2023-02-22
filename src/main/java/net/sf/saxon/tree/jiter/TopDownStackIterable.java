package net.sf.saxon.tree.jiter;

import org.jetbrains.annotations.NotNull;

import java.util.Iterator;
import java.util.Stack;

/**
 * Provides an iterable over a stack that iterates in order from top to bottom.
 * @param <T> the type of the items on the stack
 * @implNote Java stacks natively iterate from bottom to top; C# stacks from top to bottom.
 */

public class TopDownStackIterable<T> implements Iterable<T> {

    private final Stack<T> stack;
    public TopDownStackIterable(Stack<T> stack) {
        this.stack = stack;
    }

    /**
     * Returns an iterator over elements in the stack, starting at the top of stack and proceeding to the bottom
     * @return a descending Iterator.
     */
    @NotNull
    @Override
    public Iterator<T> iterator() {
        return new Iterator<T>() {
            int i = stack.size()-1;

            public boolean hasNext() {
                return i >= 0;
            }

            public T next() {
                return stack.get(i--);
            }
        };
    }
}


