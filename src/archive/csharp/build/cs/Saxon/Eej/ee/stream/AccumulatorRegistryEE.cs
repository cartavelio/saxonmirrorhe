using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream {
    public class AccumulatorRegistryEE : Saxon.Hej.expr.accum.AccumulatorRegistry {
        public static void registerSelectedAccumulators(Saxon.Eej.ee.stream.watch.WatchManager watchManager, System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> accumulators) {
            System.Collections.Generic.IDictionary<Saxon.Hej.expr.accum.Accumulator,Saxon.Eej.ee.stream.watch.AccumulatorWatch> watchMap = new System.Collections.Generic.Dictionary<Saxon.Hej.expr.accum.Accumulator, Saxon.Eej.ee.stream.watch.AccumulatorWatch>();
            foreach (Saxon.Hej.expr.accum.Accumulator acc in accumulators) {
                Saxon.Eej.ee.stream.watch.AccumulatorWatch watch = new Saxon.Eej.ee.stream.watch.AccumulatorWatch(acc);
                watch.setPipelineConfiguration(watchManager.getPipelineConfiguration());
                watch.setWatchManager(watchManager);
                Saxon.Impl.Helpers.MapUtils.Put(watchMap, acc, watch);
            }
            Saxon.Eej.ee.stream.watch.MultiAccumulatorWatch maw = new Saxon.Eej.ee.stream.watch.MultiAccumulatorWatch(watchMap);
            watchManager.watchAccumulators(maw);
        }
        public override Saxon.Hej.om.Sequence getStreamingAccumulatorValue(Saxon.Hej.om.NodeInfo node, Saxon.Hej.expr.accum.Accumulator accumulator, Saxon.Hej.functions.AccumulatorFn.Phase phase) {
            if ((node is Saxon.Eej.ee.stream.om.FleetingNode)) {
                if (!(accumulator.isDeclaredStreamable())) {
                    throw new Saxon.Hej.trans.XPathException("Cannot get accumulator value for a streamed node unless the accumulator is declared streamable", "XTDE3362");
                }
                Saxon.Eej.ee.stream.om.FleetingDocumentNode root = (Saxon.Eej.ee.stream.om.FleetingDocumentNode)(node.getRoot());
                System.Collections.Generic.IDictionary<Saxon.Hej.expr.accum.Accumulator,Saxon.Eej.ee.stream.watch.AccumulatorWatch> watchMap = root.getMultiAccumulatorWatch().getAccumulatorMap();
                Saxon.Eej.ee.stream.watch.AccumulatorWatch accumulatorWatch = ((watchMap == null) ? null : Saxon.Impl.Helpers.MapUtils.Get(watchMap, accumulator));
                if ((accumulatorWatch == null)) {
                    throw new Saxon.Hej.trans.XPathException((("The accumulator "+accumulator.getAccumulatorName().getDisplayName())+" is not in use for the current streamed document (check @use-accumulators)"), "XTDE3362");
                }
                if ((phase == Saxon.Hej.functions.AccumulatorFn.Phase.BEFORE)) {
                    if (accumulatorWatch.isBeingComputed(Saxon.Hej.functions.AccumulatorFn.Phase.BEFORE)) {
                        throw new Saxon.Hej.trans.XPathException((("Evaluation of accumulator "+accumulator.getAccumulatorName().getDisplayName())+" requires access to its own value"), "XTDE3400");
                    }
                    return accumulatorWatch.getPreDescentValue((Saxon.Eej.ee.stream.om.FleetingNode)(node));
                } else {
                    if (accumulatorWatch.isBeingComputed(Saxon.Hej.functions.AccumulatorFn.Phase.AFTER)) {
                        throw new Saxon.Hej.trans.XPathException((("Evaluation of accumulator "+accumulator.getAccumulatorName().getDisplayName())+" requires access to its own value"), "XTDE3400");
                    }
                    return accumulatorWatch.getPostDescentValue((Saxon.Eej.ee.stream.om.FleetingNode)(node));
                }
            } else {
                return null;
            }
        }
    
    }
}