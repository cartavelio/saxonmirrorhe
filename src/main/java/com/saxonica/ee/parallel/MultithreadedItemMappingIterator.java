package com.saxonica.ee.parallel;

import com.saxonica.ee.config.MultithreadingFactory;
import net.sf.saxon.expr.ItemMappingFunction;
import net.sf.saxon.expr.ItemMappingIterator;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.ObjectValue;

import java.util.concurrent.*;

/**
 * Parallel-processing version of the ItemMappingIterator class, which executes a mapping function
 * for each item in a sequence. In this implementation (which is designed initially for use
 * by the collection() function), the order of items in the result sequence retains
 * the order of items in the input sequence.
 */
public class MultithreadedItemMappingIterator extends ItemMappingIterator {

    private ExecutorService service;
    private BlockingQueue<Future<Item>> resultQueue = new LinkedBlockingQueue<>();
    private SequenceIterator dischargedIterator = null;

    public MultithreadedItemMappingIterator(SequenceIterator base, ItemMappingFunction action, MultithreadingFactory factory) throws XPathException {
        super(base, action);

        int maxThreads = getMaxThreads();
        service = factory.makeExecutorService(maxThreads);

        // prime the queue
        int n = 0;
        int prime = maxThreads * 3; // bug 3958
        try {
            while (++n < prime) {
                Item item = base.next();
                if (item == null) {
                    return;
                }
                mapOneItem(item);
            }
        } catch (UncheckedXPathException e) {
            close();
            throw e.getXPathException();
        }
    }

    /**
     * Overridable method that decides how many threads to use. The default implementation
     * returns the value of <code>Runtime.getRuntime().availableProcessors()</code>, or 1 if greater.
     *
     * @return the maximum number of threads to use.
     */

    protected int getMaxThreads() {
        int maxThreads = Runtime.getRuntime().availableProcessors();
        return maxThreads > 0 ? maxThreads : 1;
    }

    @Override
    public Item next() {
        if (dischargedIterator != null) {
            return dischargedIterator.next();
        }
        while (true) {
            Item nextIn = getBaseIterator().next();
            try {
                if (nextIn != null) {
                    mapOneItem(nextIn);
                }
                Future<Item> future = resultQueue.poll();
                if (future == null) {
                    service.shutdown();
                    return null;
                } else {
                    Item result = future.get();
                    if (!(result instanceof NullItem)) {
                        return result;
                        // otherwise continue;
                    }
                }
            } catch (InterruptedException e) {
                throw new UncheckedXPathException(new XPathException(e));
            } catch (ExecutionException | RejectedExecutionException e) {
                if (e.getCause() instanceof XPathException) {
                    throw new UncheckedXPathException((XPathException) e.getCause());
                }
                if (!e.getMessage().contains("Shutting down")) {
                    throw new UncheckedXPathException(new XPathException(e));
                } // else {
                // No action, error reported elsewhere
                //}
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
        getBaseIterator().close();
        service.shutdown();
    }

    private void mapOneItem(Item in) {
        //Future<Item> future = service.submit(new CallableAction(getMappingFunction(), in, service));
        ItemMappingFunction action = getMappingFunction();
        Future<Item> future = service.submit(() -> {
            try {
                Item out = action.mapItem(in);
                if (out == null) {
                    return new NullItem(true);
                } else {
                    return out;
                }
            } catch (XPathException e) {
                service.shutdown();
                throw e;
            }
        });
        resultQueue.add(future);
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

// Copyright (c) 2018-2022 Saxonica Limited