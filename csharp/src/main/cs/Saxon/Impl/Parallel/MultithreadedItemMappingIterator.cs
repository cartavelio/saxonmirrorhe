using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        // private readonly SequenceIterator inputIterator;
        private readonly IEnumerator<Item> result;
        
        public MultithreadedItemMappingIterator(SequenceIterator inputIterator, ItemMappingFunction action, MultithreadingFactory factory) 
            : base (inputIterator, action) {
            // this.inputIterator = inputIterator;
            result = MappingEnumerable(
                InputAsEnumerable(inputIterator), action.mapItem, getMaxThreads())
                .GetEnumerator();
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
            return result.MoveNext() ? result.Current : null;
        }
        
        // public override void close() {
        //     base.close();
        //     inputIterator.close();
        // }
        
        // From https://stackoverflow.com/questions/74805962/c-sharp-parallel-mapping-iterator-retaining-order
        private static IEnumerable<TResult> MappingEnumerable<TSource, TResult>(
            IEnumerable<TSource> source,
            Func<TSource, TResult> mapping,
            int degreeOfParallelism)
        {
            return Partitioner
                .Create(source, EnumerablePartitionerOptions.NoBuffering)
                .AsParallel()
                .AsOrdered()
                .WithDegreeOfParallelism(degreeOfParallelism)
                .WithMergeOptions(ParallelMergeOptions.NotBuffered)
                .Select(mapping);
        }

    }
    
    
}

// Copyright (c) 2022 - 2023 Saxonica Limited