using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using SysCollGen = System.Collections.Generic;
using Saxon.Hej.expr;
using Saxon.Hej.om;
using Saxon.Hej.trans;
using Saxon.Hej.tree.iter;

namespace Saxon.Impl.Parallel {
    
    // Implementation of xsl:for-each with saxon:threads="n".
    // Note that unlike the Java implementation, this implementation fully evaluates both the input
    // and output sequences as lists in memory. This enables the C# Parallel.For construct to be used.
    // It also makes implementation of last() rather straightforward. But it is memory-hungry.
    public sealed class MultithreadedContextMappingIterator : Saxon.Hej.om.SequenceIterator {
        
        private readonly SysCollGen.List<Item>[] outputSequence;
        private SequenceIterator stepIterator;
        private int index = 0;

        public MultithreadedContextMappingIterator(Saxon.Hej.expr.elab.PullEvaluator action,
                Saxon.Hej.expr.XPathContextMinor context, int threads) {
            SysCollGen.List<Item> inputSequence = new SysCollGen.List<Item>();
            SequenceIterator inputIter = context.currentIterator;
            for (Item item; (item = inputIter.next()) != null;) {
                inputSequence.Add(item);
            }
            outputSequence = new SysCollGen.List<Item>[inputSequence.Count];
            ParallelOptions options = new() {
                MaxDegreeOfParallelism = threads
            };
            try {
                System.Threading.Tasks.Parallel.For(0, inputSequence.Count, options, (i) => {
                    XPathContext c3 = XPathContextMajor.newThreadContext(context);
                    ManualIterator manualIterator = new (inputSequence[i], i+1);
                    manualIterator.setLengthFinder(() => inputSequence.Count);
                    c3.setCurrentIterator(manualIterator);
                    SequenceIterator oneOutputIter = action(c3);
                    SysCollGen.List<Item> oneOutput = new SysCollGen.List<Item>();
                    for (Item item; (item = oneOutputIter.next()) != null;) {
                        oneOutput.Add(item);
                    }
                    outputSequence[i] = oneOutput;
                });
            }
            catch (System.AggregateException e) {
                foreach (Exception ie in e.InnerExceptions) {
                    switch (ie) {
                        case XPathException:
                            throw ie;
                        case UncheckedXPathException uxe:
                            throw uxe.getXPathException();
                    }
                }
                throw;
            }

        }

        public Item next() {
            while (true) {
                if (stepIterator != null) {
                    Item nextStep = stepIterator.next();
                    if (nextStep != null) {
                        return nextStep;
                    }
                }

                if (index >= outputSequence.Length) {
                    return null;
                }

                SysCollGen.List<Item> subList = outputSequence[index++];
                stepIterator = new ListIterator.Of<Item>(subList);
            }
        }
        
        public void close() {
            //
        }
        
        public void discharge() {
        }
    }

}

// Copyright (c) 2021 - 2023 Saxonica Limited