package com.saxonica.ee.parallel;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.trans.Outcome;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.SequenceWriter;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.elab.PushEvaluator;
import net.sf.saxon.expr.instruct.TailCall;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.QuitParsingException;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.StringValue;

import java.io.Closeable;
import java.util.concurrent.ArrayBlockingQueue;
import java.util.concurrent.BlockingQueue;

/**
 * This class evaluates an expression in push mode in one thread, and provides an iterator to read
 * the results of the expression in pull mode in the original thread.
 *
 * <p>It is used when an xsl:stream instruction is executed in pull mode, for example when it
 * is called within a function. It is also used for the implicit xsl:stream instruction executed
 * during streamed merging.</p>
 *
 * @since 9.7. This class replaces the Conduit class used in previous Saxon releases, which used
 * a home-grown equivalent of the BlockingQueue.
 */
public class PushToPull {

    private final PushEvaluator expression;
    private final XPathContext context;
    private final BlockingQueue<Outcome<Item>> queue = new ArrayBlockingQueue<>(50);
    private final static Item STOPPER = StringValue.bmp("finished");

    public PushToPull(PushEvaluator expression, XPathContext context) {
        this.expression = expression;
        this.context = context;
    }

    /**
     * An implementation of SequenceWriter that allows expression.process() to write its
     * output to a blocking queue, from which the consumer can read it. Note that the queue
     * holds items in composed form, so this only works well if the producer is generating
     * a sequence of independent items, rather than a single large tree.
     * <p>
     * The queue does not hold items directly, but wraps them in an Outcome object, which
     * can hold either an Item or an exception.
     */

    private static class QueueWriter extends SequenceWriter {

        private final BlockingQueue<Outcome<Item>> queue;

        public QueueWriter(BlockingQueue<Outcome<Item>> queue, PipelineConfiguration pipe) {
            super(pipe);
            this.queue = queue;
        }

        @Override
        public void write(Item item) throws XPathException {
            //System.err.println("In thread " + Thread.currentThread().getName() + " + Provider.write " + item);
            if (Thread.currentThread().isInterrupted()) {
                //System.err.println("Already interrupted");
                throw new QuitParsingException(true);
            }
            try {
                queue.put(new Outcome<Item>(item));
            } catch (InterruptedException e) {
                throw new QuitParsingException(true);
            }
        }

        public void notify(XPathException exception) {
            try {
                queue.clear();
                queue.put(new Outcome<Item>(exception));
            } catch (InterruptedException e) {
                //throw new IllegalStateException("Interrupted while trying to notify exception");
            }
        }
    }

    /**
     * Runnable class to evaluate the supplied expression in push mode, in a separate thread.
     * The output of the expression (a sequence) is written as a sequence of items to the
     * shared BlockingQueue.
     */

    private static class Provider implements Runnable {

        private final XPathContext context;
        private final BlockingQueue<Outcome<Item>> queue;
        private final PushEvaluator expression;

        public Provider(PushEvaluator expression, BlockingQueue<Outcome<Item>> queue, XPathContext context) {
            this.expression = expression;
            this.queue = queue;
            this.context = context;
        }

        @Override
        public void run() {
            PipelineConfiguration pipe = context.getController().makePipelineConfiguration();
            pipe.setXPathContext(context);
            QueueWriter q = new QueueWriter(queue, pipe);
            try {
                TailCall tc = expression.processLeavingTail(new ComplexContentOutputter(q), context);
                Expression.dispatchTailCall(tc);
                // On completion, write a stopper item which the consumer recognizes as representing
                // the end of the sequence.
                q.write(STOPPER);
            } catch (QuitParsingException quit) {
                if (!quit.isNotifiedByConsumer()) {
                    try {   // Bug 2934, 3080
                        q.write(STOPPER);
                    } catch (XPathException e) {
                        e.printStackTrace();
                    }
                }
                // no action, we can shut down (this exception was notified by the consumer)
            } catch (XPathException err) {
                q.notify(err);
            }
        }
    }

    /**
     * Get an iterator over the result of evaluating the supplied expression.
     *
     * @return an iterator. Calls to next() on this iterator will read items from the
     * queue, waiting if necessary until the next item becomes available.
     */

    public SequenceIterator getIterator() {
        EnterpriseConfiguration config = (EnterpriseConfiguration) context.getConfiguration();
        final Thread provider = config.getMultithreadingFactory().makeThread(new Provider(expression, queue, context));
        provider.start();
        return new PushToPullIterator(provider, queue);
    }

    private static class PushToPullIterator implements SequenceIterator, Closeable {

        private final Thread provider;
        private final BlockingQueue<Outcome<Item>> queue;
        private boolean allDone = false;

        public PushToPullIterator(Thread provider, BlockingQueue<Outcome<Item>> queue) {
            this.provider = provider;
            this.queue = queue;
        }

        @Override
        //@CSharpModifiers(code = {"public", "override"})
        public Item next() {
            //System.err.println("Consumer.read()");
            if (allDone) {
                // Just in case the caller does another call on next() after completion
                return null;
            }
            try {
                Outcome<Item> next = queue.take();
                if (next.isSuccess()) {
                    Item item = next.getResult();
                    if (item == STOPPER) {
                        // this is a special item indicating the end of the sequence
                        allDone = true;
                        return null;
                    } else {
                        return item;
                    }
                } else {
                    XPathException xpe = XPathException.makeXPathException(next.getException());
                    throw new UncheckedXPathException(xpe);
                }
            } catch (InterruptedException e) {
                XPathException xpie = XPathException.makeXPathException(e);
                throw new UncheckedXPathException(xpie);
            }
        }

        /**
         * If the consumer does not need any more items from the iterator, it should call close(); this will signal
         * the producer to stop producing any more items. In cases where the producer is parsing an input XML document,
         * this may cause the parse() to be aborted early, which gives substantial performance benefits.
         */

        @Override
        //@CSharpModifiers(code = {"public", "override"})
        public void close() {
            allDone = true;
            provider.interrupt();
        }

        @Override
        public void discharge() {
            throw new UnsupportedOperationException();
            // TODO: implement me?
        }
    }


}

// Copyright (c) 2018-2022 Saxonica Limited