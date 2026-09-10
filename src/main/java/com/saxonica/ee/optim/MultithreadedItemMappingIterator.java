package com.saxonica.ee.optim;

import net.sf.saxon.expr.ItemMappingFunction;
import net.sf.saxon.expr.ItemMappingIterator;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.ObjectValue;

import java.util.concurrent.*;

/**
 * Parallel-processing version of the ItemMappingIterator class, which executes a mapping function
 * for each item in a sequence. In this implementation (which is designed initially for use
 * by the collection() function), the order of items in the result sequence retains
 * the order of items in the input sequence.
 */
public class MultithreadedItemMappingIterator<F extends Item<?>, T extends Item<?>> extends ItemMappingIterator<F, T> {

    private ExecutorService service;
    private BlockingQueue<Future<T>> resultQueue = new LinkedBlockingQueue<>();

    public MultithreadedItemMappingIterator(SequenceIterator<F> base, ItemMappingFunction<F, T> action) throws XPathException {
        super(base, action);

        int maxThreads = getMaxThreads();
        service = makeExecutorService(maxThreads);

        // prime the queue
        int n = 0;
        while (++n < maxThreads * 3) { // see bug 3958
            F item = base.next();
            if (item == null) {
                return;
            }
            mapOneItem(item);
        }
    }

    /**
     * Overridable method that decides how many threads to use. The default implementation
     * returns the value of <code>Runtime.getRuntime().availableProcessors()</code>, or 1 if greater.
     * @return the maximum number of threads to use.
     */

    protected int getMaxThreads() {
        int maxThreads = Runtime.getRuntime().availableProcessors();
        return maxThreads > 0 ? maxThreads : 1;
    }

    /**
     * Overridable method to create an <code>ExecutorService</code> with a given concurrency
     * @param maxThreads the maximum number of threads to be used
     * @return a suitable <code>ExecutorService</code>
     */

    protected ExecutorService makeExecutorService(int maxThreads) {
        return Executors.newFixedThreadPool(maxThreads);
    }

    public T next() throws XPathException {
        while (true) {
            F nextIn = getBaseIterator().next();
            if (nextIn != null) {
                mapOneItem(nextIn);
            }
            try {
                Future<T> future = resultQueue.poll();
                if (future == null) {
                    service.shutdown();
                    return null;
                } else {
                    T result = future.get();
                    if (!(result instanceof NullItem)) {
                        return result;
                        // otherwise continue;
                    }
                }
            } catch (InterruptedException e) {
                throw new XPathException(e);
            } catch (ExecutionException e) {
                if (e.getCause() instanceof XPathException) {
                    throw (XPathException)e.getCause();
                }
                throw new XPathException(e);
            }
        }
    }

    /**
     * The user of a {@link SequenceIterator} is encouraged to call the <code>close()</code> method if
     * no further items are required from the sequence of items delivered by the <code>SequenceIterator</code>.
     * This allows resources to be released. A <code>SequenceIterator</code> is NOT normally closed explicitly
     * if it has been read to completion. This implementation of the <code>close()</code> method shuts down
     * the <code>ExecutorService</code> used to schedule threads.
     */

    @Override
    public void close() {
        super.close();
        service.shutdown();
    }

    private void mapOneItem(F in) {
        Future<T> future = service.submit(new CallableAction(getMappingFunction(), in));
        resultQueue.add(future);
    }

    private class CallableAction<F extends Item<?>, T extends Item<?>> implements Callable<Item> {
        private ItemMappingFunction<F, T> action;
        private F input;
        public CallableAction(ItemMappingFunction<F, T> action, F input) {
            this.action = action;
            this.input = input;
        }
        public Item call() throws XPathException {
            try {
                T out = action.mapItem(input);
                if (out == null) {
                    return new NullItem(true);
                } else {
                    return out;
                }
            } catch (XPathException e) {
                service.shutdown();
                throw e;
            }
        }
    }

    /**
     * An {@link ItemMappingFunction} returns zero or one items in the output corresponding to each item
     * in the input, representing an item as an object and no item as null. The {@link NullItem} class provides
     * a representation of the "no item" case as a pseudo-item, making it easier to communicate between threads.
     */

    private static class NullItem extends ObjectValue<Boolean> {
        public NullItem(Boolean success) {
            super(success);
        }
    }

}

// Copyright (c) 2018 Saxonica Limited.