using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Saxon.Eej.ee.config;
using Saxon.Eej.ee.trans;
using Saxon.Hej.expr;
using Saxon.Hej.om;
using Saxon.Hej.value;

namespace Saxon.Impl.Parallel {
    public class MultithreadedItemMappingIterator : Saxon.Hej.expr.ItemMappingIterator {
        private readonly Channel<Outcome<Item>> resultQueue = Channel.CreateBounded<Outcome<Item>>(64);
        private readonly CancellationTokenSource tokenSource = new();
        private bool allDone;
        private static readonly Item STOPPER = new StringValue("stopper");
        
        public MultithreadedItemMappingIterator(SequenceIterator inputIterator, ItemMappingFunction action, MultithreadingFactory factory) : base (inputIterator, action) {
            CancellationToken token = tokenSource.Token;
            int maxThreads = getMaxThreads();
            ParallelOptions options = new() {
                MaxDegreeOfParallelism = maxThreads,
                CancellationToken = token
            };
            
            //Console.Error.WriteLine("Input sequence size: " + inputSequence.Count);

            ChannelWriter<Outcome<Item>> writer = resultQueue.Writer;
            System.Threading.Tasks.Parallel.ForEach(InputAsEnumerable(inputIterator), options, (item) => {
                Item mapped = action.mapItem(item);
                if (mapped != null) {
                    writer.WriteAsync(new Outcome<Item>(mapped));
                }
            });
            writer.WriteAsync(new Outcome<Item>(STOPPER));
            
            //Console.Error.WriteLine("Output sequence size: " + resultQueue.Count);

        }

        private static IEnumerable<Item> InputAsEnumerable(SequenceIterator iter)
        {
            for (Item item; (item = iter.next()) != null; ) {
                yield return item;
            }
        }

        private static int getMaxThreads() {
            int maxThreads = Environment.ProcessorCount;
            return ((maxThreads>0) ? maxThreads : 1);
        }

        public override Saxon.Hej.om.Item next() {
            if (allDone) {
                return null;
            }

            try {
                Task<Outcome<Item>> readTask = resultQueue.Reader.ReadAsync().AsTask();
                //var task = Task.Run(async () => { await WriteToLogAsync(text); });
                try {
                    readTask.Wait();
                }
                catch (System.Exception e) when (e is ChannelClosedException || e is System.AggregateException) {
                    return null;
                }

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

        public override void close() {
            base.close();
            tokenSource.Cancel();
        }

    }
    
    
}

// Copyright (c) 2021 Saxonica Limited