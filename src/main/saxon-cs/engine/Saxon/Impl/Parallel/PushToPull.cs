using net.sf.saxon.expr;

namespace Saxon.Impl.Parallel {
    using System;
    using System.Threading;
    using System.Threading.Channels;
    using System.Threading.Tasks;
    using com.saxonica.config;
    using com.saxonica.ee.trans;
    using net.sf.saxon.om;
    using net.sf.saxon.trans;
    using net.sf.saxon.value;

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
        private readonly PushToPull.Pusher pusher;
        private readonly XPathContext context;
        private readonly Channel<Outcome<Item>> channel = Channel.CreateBounded<Outcome<Item>>(50);

        public PushToPull(PushToPull.Pusher pusher,
            net.sf.saxon.expr.XPathContext context) {
            this.pusher = pusher;
            this.context = context;
        }

        public interface Pusher {
            void push(net.sf.saxon.@event.Outputter output, net.sf.saxon.expr.XPathContext context);
        }

        private class QueueWriter : net.sf.saxon.@event.SequenceWriter {
            private readonly Channel<Outcome<Item>> queue;

            public QueueWriter(Channel<Outcome<Item>> queue, net.sf.saxon.@event.PipelineConfiguration pipe) :
                base(pipe) {
                this.queue = queue;
            }

            public override void write(net.sf.saxon.om.Item item) {
                if (java.lang.Thread.currentThread().isInterrupted()) {
                    throw new net.sf.saxon.trans.QuitParsingException(true);
                }

                try {
                    queue.Writer.WriteAsync(new Outcome<Item>(item));
                    //Console.WriteLine("HAVE WRITTEN " + item.getStringValue());
                }
                catch (System.Threading.ThreadInterruptedException e) {
                    throw new net.sf.saxon.trans.QuitParsingException(true);
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
            private readonly Channel<com.saxonica.ee.trans.Outcome<net.sf.saxon.om.Item>> queue;
            private readonly PushToPull.Pusher pusher;

            public Provider(PushToPull.Pusher pusher,
                Channel<Outcome<Item>> queue, XPathContext context) {
                this.pusher = pusher;
                this.queue = queue;
                this.context = context;
            }

            public void run() {
                net.sf.saxon.@event.PipelineConfiguration pipe = context.getController().makePipelineConfiguration();
                pipe.setXPathContext(context);
                PushToPull.QueueWriter q = new PushToPull.QueueWriter(queue, pipe);
                try {
                    pusher.push(new net.sf.saxon.@event.ComplexContentOutputter(q), context);
                    queue.Writer.WriteAsync(new Outcome<Item>(STOPPER));
                    //queue.Writer.Complete();
                }
                catch (net.sf.saxon.trans.QuitParsingException quit) {
                    if (!(quit.isNotifiedByConsumer())) {
                        try {
                            queue.Writer.Complete();
                        }
                        catch (net.sf.saxon.trans.XPathException e) {
                            System.Console.Error.WriteLine(e);
                        }
                    }
                }
                catch (net.sf.saxon.trans.XPathException err) {
                    q.notify(err);
                }
            }
        }

        public net.sf.saxon.om.SequenceIterator getIterator() {
            EnterpriseConfiguration config = (EnterpriseConfiguration) context.getConfiguration();
            PushToPull.Provider provider = new PushToPull.Provider(pusher, channel, context);
            Thread thread = new Thread(() => provider.run());
            thread.Start();
            return new PushToPull.PushToPullIterator(thread, channel);
        }

        private class PushToPullIterator : net.sf.saxon.om.SequenceIterator, java.io.Closeable {
            private readonly Thread provider;
            private readonly Channel<Outcome<net.sf.saxon.om.Item>> queue;
            private bool allDone = false;

            public PushToPullIterator(Thread provider, Channel<Outcome<Item>> queue) {
                this.provider = provider;
                this.queue = queue;
            }

            public net.sf.saxon.om.Item next() {
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
                        net.sf.saxon.trans.XPathException xpe =
                            net.sf.saxon.trans.XPathException.makeXPathException(next.getException());
                        throw new net.sf.saxon.trans.UncheckedXPathException(xpe);
                    }
                }
                catch (ChannelClosedException e) {
                    allDone = true;
                    return null;
                }
                catch (System.Threading.ThreadInterruptedException e) {
                    net.sf.saxon.trans.XPathException xpie = net.sf.saxon.trans.XPathException.makeXPathException(e);
                    throw new net.sf.saxon.trans.UncheckedXPathException(xpie);
                }
            }

            public void close() {
                allDone = true;
                //provider.interrupt();
            }
        }
    }
}