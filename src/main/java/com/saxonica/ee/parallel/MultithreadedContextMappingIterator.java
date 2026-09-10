package com.saxonica.ee.parallel;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.XPathContextMajor;
import net.sf.saxon.expr.XPathContextMinor;
import net.sf.saxon.expr.elab.PullEvaluator;
import net.sf.saxon.java.CleanerProxy;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharp;
import net.sf.saxon.tree.iter.ManualIterator;

import java.util.concurrent.*;
import java.util.function.Supplier;


/**
 * This class performs the same role as ContextMappingIterator, but using
 * multiple threads.
 * <p>It merges a sequence of sequences into a single flat
 * sequence. It takes as inputs an iteration, and an "action" expression to be
 * applied to each Item returned by that iteration, with that item as the context item.
 * The action expression itself returns another iterator. The result is an iteration of the concatenation of all
 * the iterations returned by the action expression.</p>
 */

public final class MultithreadedContextMappingIterator implements SequenceIterator {

    private final FocusIterator base;
    private final PullEvaluator action;
    private final XPathContextMinor context;
    private SequenceIterator stepIterator = null;
    private final Supplier<Integer> lastPositionFinder;
    private final ExecutorService service;
    private final BlockingQueue<Future<GroundedValue>> resultQueue = new LinkedBlockingQueue<>();
    private SequenceIterator dischargedIterator;
    private CleanerProxy.CleanableProxy cleanable;

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

    public MultithreadedContextMappingIterator(PullEvaluator action, final XPathContextMinor context, int threads)
            throws XPathException {
        //context.getConfiguration().getStandardErrorOutput().println("Using MultithreadedContextMappingIterator");
        base = context.getCurrentIterator();
        this.action = action;
        this.context = context;
        // Don't pre-calculate last(); get it on demand. The focus tracking iterator is synchronised so that
        // if two threads both try to calculate last(), one of them will do the work, and the other
        // will take advantage of the known answer.
        lastPositionFinder = CSharp.methodRef(context::getLast);
        EnterpriseConfiguration config = (EnterpriseConfiguration) context.getConfiguration();
        service = config.getMultithreadingFactory().makeExecutorService(threads);
        // prime the queue
        int n = 0;
        int max  = threads * 3; // bug 3958
        try {
            while (++n <= max) {   // see bug 3958
                Item item = base.next();
                if (item == null) {
                    return;
                }
                mapOneItem(context);
            }
        } catch (UncheckedXPathException e) {
            close();
            throw e.getXPathException();
        }

        // This code modelled on https://howtodoinjava.com/java/basics/java-cleaners/
        cleanable = config.registerCleanupAction(this, getCleaningAction(service));
    }

    @Override
    public Item next() {
        if (dischargedIterator != null) {
            return dischargedIterator.next();
        }
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
            Future<GroundedValue> future = resultQueue.poll();
            if (future == null) {
                close();
                return null;
            } else {
                try {
                    stepIterator = future.get().iterate();
                } catch (InterruptedException e) {
                    throw new UncheckedXPathException(e);
                } catch (ExecutionException e) {
                    close();
                    if (e.getCause() instanceof XPathException) {
                        throw new UncheckedXPathException((XPathException) e.getCause());
                    } else if (e.getCause() instanceof UncheckedXPathException) {
                        throw (UncheckedXPathException)e.getCause();
                    } else {
                        throw new UncheckedXPathException(e);
                    }
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

    @Override
    public void close() {
        base.close();
        if (cleanable != null) {
            cleanable.clean();
        } else if (service != null) {
            service.shutdown();
        }
    }

    private static Runnable getCleaningAction(final ExecutorService service) {
        return service::shutdown;
    }


    private void mapOneItem(XPathContextMinor context) {
        final XPathContext c3 = XPathContextMajor.newThreadContext(context);
        FocusIterator base = context.getCurrentIterator();
        ManualIterator manualIterator =
                new ManualIterator(base.current(), base.position());
        manualIterator.setLengthFinder(lastPositionFinder);
        c3.setCurrentIterator(manualIterator);

        PipelineConfiguration pipe = context.getController().makePipelineConfiguration();
        pipe.setXPathContext(context);
        //SequenceCollector outputter = new SequenceCollector(pipe);
        //c3.setReceiver(new ComplexContentOutputter(outputter));

        int attempts = 0;
        Future<GroundedValue> future;
        while (true) {
            try {
                future = service.submit(
                        () -> {
                            try {
                                return SequenceTool.toGroundedValue(action.iterate(c3));
                            } catch (XPathException err) {
                                close();
                                throw err;
                            } catch (UncheckedXPathException err) {
                                close();
                                throw err.getXPathException();
                            }
                        });
                break;
            } catch (RejectedExecutionException e) {  // bug 4075
                if (service.isShutdown()) {
                    return;  // bug 4788
                }
                if (attempts++ < 10) {
                    try {
                        Thread.sleep(1L << attempts);
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


// Copyright (c) 2010-2023 Saxonica Limited