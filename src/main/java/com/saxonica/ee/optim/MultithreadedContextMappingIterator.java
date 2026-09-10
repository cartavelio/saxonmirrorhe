package com.saxonica.ee.optim;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.SequenceOutputter;
import net.sf.saxon.expr.*;
import net.sf.saxon.om.FocusIterator;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.ManualIterator;

import java.util.concurrent.*;


/**
 * This class performs the same role as ContextMappingIterator, but using
 * multiple threads.
 * <p>It merges a sequence of sequences into a single flat
 * sequence. It takes as inputs an iteration, and an "action" expression to be
 * applied to each Item returned by that iteration, with that item as the context item.
 * The action expression itself returns another iterator. The result is an iteration of the concatenation of all
 * the iterations returned by the action expression.</p>
 */

public final class MultithreadedContextMappingIterator implements SequenceIterator<Item<?>> {

    private FocusIterator<?> base;
    private Expression action;
    private XPathContextMinor context;
    private SequenceIterator stepIterator = null;
    private LastPositionFinder lastPositionFinder;
    private ExecutorService service;
    private BlockingQueue<Future<Sequence>> resultQueue = new LinkedBlockingQueue<>();

    /**
     * Construct an Iterator that will apply a specified action expression to
     * each Item returned by the current iterator of the supplied dynamic context.
     *
     * @param action  the mapping expression to be applied
     * @param context the processing context. The action expression is applied to each item returned
     *                by context.getCurrentIterator() in turn.
     * @param threads the number of threads to be used (in addition to the base evaluation thread)
     *                These threads are used to evaluate the action expression. The current thread is used
     *                to evaluate the base iterator and to gather the results of invoking the action expressions
     *                in the right order.
     */

    public MultithreadedContextMappingIterator(Expression action, final XPathContextMinor context, int threads)
            throws XPathException {
        //context.getConfiguration().getStandardErrorOutput().println("Using MultithreadedContextMappingIterator");
        base = context.getCurrentIterator();
        this.action = action;
        this.context = context;
        // Don't pre-calculate last(); get it on demand. The focus tracking iterator is synchronised so that
        // if two threads both try to calculate last(), one of them will do the work, and the other
        // will take advantage of the known answer.
        lastPositionFinder = context::getLast;
        EnterpriseConfiguration config = (EnterpriseConfiguration) context.getConfiguration();
        service = config.getMultithreadingFactory().makeExecutorService(threads);
        // prime the queue
        int n = 0;
        try {
            while (++n <= 3 * threads) {   // see bug 3958
                Item item = base.next();
                if (item == null) {
                    return;
                }
                mapOneItem(context);
            }
        } catch (XPathException e) {
            close();
            throw e;
        }
    }

    public Item next() throws XPathException {
        Item nextItem;
        while (true) {
            if (stepIterator != null) {
                nextItem = stepIterator.next();
                if (nextItem != null) {
                    break;
                } else {
                    stepIterator = null;
                }
            }

            // Get the next stepIterator off the queue
            Future<Sequence> future = resultQueue.poll();
            if (future == null) {
                close();
                return null;
            } else {
                try {
                    stepIterator = future.get().iterate();
                } catch (InterruptedException e) {
                    throw new XPathException(e);
                } catch (ExecutionException e) {
                    close();
                    if (e.getCause() instanceof XPathException) {
                        throw (XPathException) e.getCause();
                    }
                    throw new XPathException(e);
                }
                if (stepIterator == null) {
                    return null;
                } else {
                    if (base.current() != null) {
                        Item item = base.next();
                        if (item != null) {
                            mapOneItem(context);
                        }
                    }
                    nextItem = stepIterator.next();
                    if (nextItem == null) {
                        stepIterator = null;
                    } else {
                        break;
                    }
                }
            }
        }
        return nextItem;
    }

    public void close() {
        base.close();
        service.shutdown();
    }

    /**
     * Get properties of this iterator, as a bit-significant integer.
     *
     * @return the properties of this iterator. This will be some combination of
     *         properties such as {@link net.sf.saxon.om.SequenceIterator#GROUNDED}, {@link net.sf.saxon.om.SequenceIterator#LAST_POSITION_FINDER},
     *         and {@link net.sf.saxon.om.SequenceIterator#LOOKAHEAD}. It is always
     *         acceptable to return the value zero, indicating that there are no known special properties.
     *         It is acceptable for the properties of the iterator to change depending on its state.
     */

    public int getProperties() {
        return 0;
    }

    private void mapOneItem(XPathContextMinor context) {
        final XPathContext c3 = XPathContextMajor.newThreadContext(context);
        FocusIterator<?> base = context.getCurrentIterator();
        ManualIterator<?> manualIterator =
                new ManualIterator<>(base.current(), base.position());
        manualIterator.setLastPositionFinder(lastPositionFinder);
        c3.setCurrentIterator(manualIterator);

        PipelineConfiguration pipe = context.getController().makePipelineConfiguration();
        pipe.setXPathContext(context);
        SequenceOutputter outputter = new SequenceOutputter(pipe);
        c3.setReceiver(outputter);

        int attempts = 0;
        Future<Sequence> future = null;
        while (true) {
            try {
                future = service.submit(
                        () -> {
                            try {
                                return action.iterate(c3).materialize();
                            } catch (XPathException err) {
                                close();
                                throw err;
                            }
                        });
                break;
            } catch (RejectedExecutionException e) {  // bug 4075
                if (service.isShutdown()) {
                    return; // bug 4788
                }
                if (attempts++ < 10) {
                    try {
                        Thread.sleep(1 << attempts);
                    } catch (InterruptedException e1) {
                        close();
                        throw e;
                    }
                } else {
                    close();
                    throw e;
                }
            }
        }
        resultQueue.add(future);
    }


}


// Copyright (c) 2010-2015 Saxonica Limited.