package com.saxonica.ee.stream;

import com.saxonica.ee.stream.om.FleetingDocumentNode;
import com.saxonica.ee.stream.om.FleetingNode;
import com.saxonica.ee.stream.watch.AccumulatorWatch;
import com.saxonica.ee.stream.watch.MultiAccumulatorWatch;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.accum.Accumulator;
import net.sf.saxon.expr.accum.AccumulatorRegistry;
import net.sf.saxon.functions.AccumulatorFn;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;

import java.util.HashMap;
import java.util.Map;
import java.util.Set;

/**
 * Static registry containing the accumulators defined within a package; the Saxon-EE subclass includes support
 * for streamed accumulators.
 */
public class AccumulatorRegistryEE extends AccumulatorRegistry {

    /**
     * Register all the rules for all the accumulators with a WatchManager handling events for a streaming
     * pass over a source document
     *
     * @param watchManager the WatchManager with which the rules are to be registered
     * @throws XPathException if an error occurs
     */

    public static void registerSelectedAccumulators(
            WatchManager watchManager, Set<? extends Accumulator> accumulators) throws XPathException {
        if (!accumulators.isEmpty()) {
            Map<Accumulator, AccumulatorWatch> watchMap = new HashMap<>();

            for (Accumulator acc : accumulators) {
                AccumulatorWatch watch = new AccumulatorWatch(acc);
                watch.setPipelineConfiguration(watchManager.getPipelineConfiguration());
                watch.setWatchManager(watchManager);
                watchMap.put(acc, watch);
            }
            MultiAccumulatorWatch maw = new MultiAccumulatorWatch(watchMap);
            watchManager.watchAccumulators(maw);
        }
    }

    /**
     * Get the run-time value of a streamed accumulator
     *
     * @param node           the context node
     * @param accumulator the accumulator whose value is required
     * @param phase       pre-descent or post-descent
     * @return the value of the accumulator, or null if the context node is not streamed
     * @throws XPathException if a dynamic error occurs
     */


    @Override
    public Sequence getStreamingAccumulatorValue(NodeInfo node, Accumulator accumulator, AccumulatorFn.Phase phase) throws XPathException {
        if (node instanceof FleetingNode) {
            if (!accumulator.isDeclaredStreamable()) {
                throw new XPathException("Cannot get accumulator value for a streamed node unless the accumulator is declared streamable", "XTDE3362");
            }
            FleetingDocumentNode root = (FleetingDocumentNode) node.getRoot();
            MultiAccumulatorWatch maw = root.getMultiAccumulatorWatch();
            AccumulatorWatch accumulatorWatch = null;
            if (maw != null) {
                Map<Accumulator, AccumulatorWatch> watchMap = maw.getAccumulatorMap();
                accumulatorWatch = watchMap == null ? null : watchMap.get(accumulator);
            }
            if (accumulatorWatch == null) {
                throw new XPathException("The accumulator " + accumulator.getAccumulatorName().getDisplayName() +
                    " is not in use for the current streamed document (check @use-accumulators)", "XTDE3362");
            }
            if (phase == AccumulatorFn.Phase.BEFORE) {
                if (accumulatorWatch.isBeingComputed(AccumulatorFn.Phase.BEFORE)) {
                    throw new XPathException("Evaluation of accumulator " + accumulator.getAccumulatorName().getDisplayName() +
                            " requires access to its own value", "XTDE3400");
                }
                return accumulatorWatch.getPreDescentValue((FleetingNode) node);
            } else {
                if (accumulatorWatch.isBeingComputed(AccumulatorFn.Phase.AFTER)) {
                    throw new XPathException("Evaluation of accumulator " + accumulator.getAccumulatorName().getDisplayName() +
                                                     " requires access to its own value", "XTDE3400");
                }
                return accumulatorWatch.getPostDescentValue((FleetingNode)node);
            }
        } else {
            return null;
        }
    }


}

// Copyright (c) 2018-2022 Saxonica Limited