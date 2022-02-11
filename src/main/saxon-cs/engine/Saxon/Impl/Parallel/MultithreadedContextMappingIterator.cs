using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using com.saxonica.config;
using net.sf.saxon.om;

namespace Saxon.Impl.Parallel {
    public sealed class MultithreadedContextMappingIterator : net.sf.saxon.om.SequenceIterator {
        private readonly net.sf.saxon.om.FocusIterator contextIterator;
        private readonly net.sf.saxon.expr.Expression action;
        private readonly net.sf.saxon.expr.XPathContextMinor context;
        private net.sf.saxon.om.SequenceIterator stepIterator = null;
        private readonly java.util.function.Supplier<System.Int32> lastPositionFinder;
        private readonly java.util.concurrent.ExecutorService service;
        
        private readonly ConcurrentQueue<Task<GroundedValue>> resultQueue = new();
        private readonly CancellationTokenSource tokenSource = new();
        private readonly CancellationToken token;

        
        public MultithreadedContextMappingIterator(net.sf.saxon.expr.Expression action,
                net.sf.saxon.expr.XPathContextMinor context, int threads) {
            token = tokenSource.Token;
            contextIterator = context.getCurrentIterator();
            this.action = action;
            this.context = context;
            lastPositionFinder = context.getLast;
            EnterpriseConfiguration config = (EnterpriseConfiguration) context.getConfiguration();
            service = config.getMultithreadingFactory().makeExecutorService(threads);
            int n = 0;
            int max = (threads * 3);
            try {
                while ((++n <= max)) {
                    net.sf.saxon.om.Item item = contextIterator.next();
                    if ((item == null)) {
                        return;
                    }

                    mapOneItem(context);
                }
            }
            catch (net.sf.saxon.trans.UncheckedXPathException e) {
                close();
                throw e.getXPathException();
            }
        }

        public net.sf.saxon.om.Item next() {
            net.sf.saxon.om.Item nextItem;
            while (true) {
                if ((stepIterator != null)) {
                    nextItem = stepIterator.next();
                    if ((nextItem != null)) {
                        break;
                    }
                    else {
                        stepIterator = null;
                    }
                }
                
                // Get the next stepIterator off the queue
                bool available = resultQueue.TryDequeue(out Task<GroundedValue> first);
                if (!available) {
                    close();
                    return null;
                } else {
                    
                    
                    GroundedValue seq = first.Result;
                    stepIterator = seq.iterate();
                    
                    if ((contextIterator.current() != null)) {
                        net.sf.saxon.om.Item item = contextIterator.next();
                        if ((item != null)) {
                            mapOneItem(context);
                        }
                    }

                    nextItem = stepIterator.next();
                    if ((nextItem == null)) {
                        stepIterator = null;
                    }
                    else {
                        break;
                    }
                    
                }
            }

            return nextItem;
        }

        public void close() {
            contextIterator.close();
            service.shutdown();
        }

        private void mapOneItem(net.sf.saxon.expr.XPathContextMinor c2) {
            net.sf.saxon.expr.XPathContext c3 = net.sf.saxon.expr.XPathContextMajor.newThreadContext(c2);
            net.sf.saxon.om.FocusIterator inputIterator = c2.getCurrentIterator();
            net.sf.saxon.tree.iter.ManualIterator manualIterator =
                new net.sf.saxon.tree.iter.ManualIterator(inputIterator.current(), inputIterator.position());
            manualIterator.setLengthFinder(lastPositionFinder);
            c3.setCurrentIterator(manualIterator);
            net.sf.saxon.@event.PipelineConfiguration pipe = c2.getController().makePipelineConfiguration();
            pipe.setXPathContext(c2);


            Task<GroundedValue> task = Task.Run(() => {
                try {
                    return net.sf.saxon.om.SequenceTool.toGroundedValue(action.iterate(c3));
                }
                catch (net.sf.saxon.trans.XPathException err) {
                    close();
                    throw;
                }
                catch (net.sf.saxon.trans.UncheckedXPathException err) {
                    close();
                    throw err.getXPathException();
                }
            }, token);

            resultQueue.Enqueue(task);
        }
    }
}