using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using com.saxonica.ee.config;
using net.sf.saxon.expr;
using net.sf.saxon.om;

namespace Saxon.Impl.Parallel {

    public class MultithreadedItemMappingIterator : net.sf.saxon.expr.ItemMappingIterator {
        private readonly ConcurrentQueue<Task<Item>> resultQueue = new();
        private readonly CancellationTokenSource tokenSource = new();
        private readonly CancellationToken token;
        
        public MultithreadedItemMappingIterator(SequenceIterator inputIterator, ItemMappingFunction action, MultithreadingFactory factory) : base (inputIterator, action) {
            token = tokenSource.Token;
            int maxThreads = getMaxThreads();
            int n = 0;
            int prime = (maxThreads*3);
            try {
                while ((++n<prime)) {
                    Item item = inputIterator.next();
                    if ((item == null)) {
                        return ;
                    }
                    mapOneItem(item);
                }
            }
            catch (net.sf.saxon.trans.UncheckedXPathException e) {
                close();
                throw e.getXPathException();
            }
        }

        private int getMaxThreads() {
            int maxThreads = Environment.ProcessorCount;
            return ((maxThreads>0) ? maxThreads : 1);
        }
        public override net.sf.saxon.om.Item next() {
            while (true) {
                net.sf.saxon.om.Item nextIn = getBaseIterator().next();
                if ((nextIn != null)) {
                    mapOneItem(nextIn);
                }
                bool available = resultQueue.TryDequeue(out Task<Item> first);
                if (!available) {
                    return null;
                } else {
                    return first.Result;
                    // TODO: exception handling
                }
            }
        }
        public override void close() {
            base.close();
            tokenSource.Cancel();
        }
        
        private void mapOneItem(net.sf.saxon.om.Item inputItem) {
            net.sf.saxon.expr.ItemMappingFunction action = getMappingFunction();
            Task<net.sf.saxon.om.Item> task = Task.Run(() => action.mapItem(inputItem), token);
            resultQueue.Enqueue(task);
        }
        
    }
}
