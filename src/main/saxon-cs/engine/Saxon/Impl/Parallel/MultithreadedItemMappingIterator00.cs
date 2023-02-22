using System;
using SysCollGen = System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Saxon.Eej.ee.config;
using Saxon.Hej.expr;
using Saxon.Hej.om;

namespace Saxon.Impl.Parallel {

    public class MultithreadedItemMappingIterator00 : Saxon.Hej.expr.ItemMappingIterator {
        private readonly ConcurrentQueue<Item> resultQueue = new();
        private readonly CancellationTokenSource tokenSource = new();
        
        public MultithreadedItemMappingIterator00(SequenceIterator inputIterator, ItemMappingFunction action, MultithreadingFactory factory) : base (inputIterator, action) {
            CancellationToken token = tokenSource.Token;
            int maxThreads = getMaxThreads();
            ParallelOptions options = new() {
                MaxDegreeOfParallelism = maxThreads,
                CancellationToken = token
            };
            SysCollGen.List<Item> inputSequence = new SysCollGen.List<Item>();
            for (Item item; (item = inputIterator.next()) != null;) {
                inputSequence.Add(item);
            }
            //Console.Error.WriteLine("Input sequence size: " + inputSequence.Count);

            System.Threading.Tasks.Parallel.ForEach(inputSequence, options, (item) => {
                Item mapped = action.mapItem(item);
                if (mapped != null) {
                    resultQueue.Enqueue(mapped);
                }
            });
            
            //Console.Error.WriteLine("Output sequence size: " + resultQueue.Count);

        }

        private static int getMaxThreads() {
            int maxThreads = Environment.ProcessorCount;
            return ((maxThreads>0) ? maxThreads : 1);
        }
        public override Saxon.Hej.om.Item next() {
            bool success = resultQueue.TryDequeue(out Item nextItem);
            //Console.Error.WriteLine("After Dequeue, size = " + resultQueue.Count + ", success=" + success + ", nextItem=" + nextItem?.getGenre());
            return success ? nextItem : null;
        }
        public override void close() {
            base.close();
            tokenSource.Cancel();
        }

    }
}
