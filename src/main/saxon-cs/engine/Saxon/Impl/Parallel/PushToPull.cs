using Saxon.Eej.config;
using Saxon.Eej.ee.parallel;
using Saxon.Eej.ee.trans;
using Saxon.Hej.expr;
using Saxon.Hej.expr.elab;

namespace Saxon.Impl.Parallel {
    using System;
    using System.Threading;
    using System.Threading.Channels;
    using System.Threading.Tasks;
    using Saxon.Hej.om;
    using Saxon.Hej.trans;
    using Saxon.Hej.value;

    /// <summary>
    /// A C# rewrite of the class com.saxonica.ee.parallel.PushToPull.
    /// </summary>
    /// <remarks>
    /// <para>One thread (the pusher) writes XDM items to a shared channel. A second thread
    /// (the puller) reads the items in the form of a <c>SequenceIterator</c>, delivering
    /// them synchronously. The class is used in some streaming scenarios, notably to
    /// implement xsl:merge, where two or more input streams are being read concurrently.
    /// </para>
    /// </remarks>

    public class PushToPull {
        // The stopper item is placed on the channel to indicate that the input is complete
        private static readonly Item STOPPER = new StringValue("stopper");
        private readonly PushEvaluator pusher;
        private readonly XPathContext context;
        private readonly Channel<Outcome<Item>> channel = Channel.CreateBounded<Outcome<Item>>(50);

        public PushToPull(PushEvaluator pusher,
            Saxon.Hej.expr.XPathContext context) {
            this.pusher = pusher;
            this.context = context;
        }

        private class QueueWriter : Saxon.Hej.@event.SequenceWriter {
            private readonly Channel<Outcome<Item>> queue;

            public QueueWriter(Channel<Outcome<Item>> queue, Saxon.Hej.@event.PipelineConfiguration pipe) :
                base(pipe) {
                this.queue = queue;
            }

            public override void write(Saxon.Hej.om.Item item) {
                if (Saxon.Ejava.lang.Thread.currentThread().isInterrupted()) {
                    throw new Saxon.Hej.trans.QuitParsingException(true);
                }

                try {
                    queue.Writer.WriteAsync(new Outcome<Item>(item));
                    //Console.WriteLine("HAVE WRITTEN " + item.getStringValue());
                }
                catch (System.Threading.ThreadInterruptedException e) {
                    throw new Saxon.Hej.trans.QuitParsingException(true);
                }
            }

            public void notify(XPathException exception) {
                try {
                    queue.Writer.WriteAsync(new Outcome<Item>(exception));
                }
                catch (System.Threading.ThreadInterruptedException e) {
                }
            }
        }

        private class Provider  {
            private readonly XPathContext context;
            private readonly Channel<Outcome<Saxon.Hej.om.Item>> queue;
            private readonly PushEvaluator pusher;

            public Provider(PushEvaluator pusher,
                Channel<Outcome<Item>> queue, XPathContext context) {
                this.pusher = pusher;
                this.queue = queue;
                this.context = context;
            }

            public void run() {
                Saxon.Hej.@event.PipelineConfiguration pipe = context.getController().makePipelineConfiguration();
                pipe.setXPathContext(context);
                PushToPull.QueueWriter q = new PushToPull.QueueWriter(queue, pipe);
                try {
                    pusher(new Saxon.Hej.@event.ComplexContentOutputter(q), context);
                    queue.Writer.WriteAsync(new Outcome<Item>(STOPPER));
                    //queue.Writer.Complete();
                }
                catch (Saxon.Hej.trans.QuitParsingException quit) {
                    if (!(quit.isNotifiedByConsumer())) {
                        try {
                            queue.Writer.Complete();
                        }
                        catch (Saxon.Hej.trans.XPathException e) {
                            System.Console.Error.WriteLine(e);
                        }
                    }
                }
                catch (Saxon.Hej.trans.XPathException err) {
                    q.notify(err);
                }
            }
        }

        public Saxon.Hej.om.SequenceIterator getIterator() {
            EnterpriseConfiguration config = (EnterpriseConfiguration) context.getConfiguration();
            PushToPull.Provider provider = new PushToPull.Provider(pusher, channel, context);
            Thread thread = new Thread(() => provider.run());
            thread.Start();
            return new PushToPull.PushToPullIterator(thread, channel);
        }

        private class PushToPullIterator : Saxon.Hej.om.SequenceIterator, Saxon.Ejava.io.Closeable {
            private readonly Thread provider;
            private readonly Channel<Outcome<Saxon.Hej.om.Item>> queue;
            private bool allDone = false;

            public PushToPullIterator(Thread provider, Channel<Outcome<Item>> queue) {
                this.provider = provider;
                this.queue = queue;
            }

            public Saxon.Hej.om.Item next() {
                if (allDone) {
                    return null;
                }

                try {
                    Task<Outcome<Item>> readTask = queue.Reader.ReadAsync().AsTask();
                    //var task = Task.Run(async () => { await WriteToLogAsync(text); });
                    try {
                        readTask.Wait();
                    }
                    catch (System.Exception e) when (e is ChannelClosedException || e is System.AggregateException) {
                        return null;
                    }
                    //if (value.IsCompleted) {
                    Outcome<Item> next = readTask.Result;
                    if (next.isSuccess()) {
                        //Console.WriteLine("HAVE READ " + next.getResult().getStringValue());
                        Item item = next.getResult();
                        return item == STOPPER ? null : item;
                    }
                    else {
                        Saxon.Hej.trans.XPathException xpe =
                            Saxon.Hej.trans.XPathException.makeXPathException(next.getException());
                        throw new Saxon.Hej.trans.UncheckedXPathException(xpe);
                    }
                }
                catch (ChannelClosedException e) {
                    allDone = true;
                    return null;
                }
                catch (System.Threading.ThreadInterruptedException e) {
                    Saxon.Hej.trans.XPathException xpie = Saxon.Hej.trans.XPathException.makeXPathException(e);
                    throw new Saxon.Hej.trans.UncheckedXPathException(xpie);
                }
            }

            public void close() {
                allDone = true;
                //provider.interrupt();
            }
            
            public void discharge() {
            }
        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited