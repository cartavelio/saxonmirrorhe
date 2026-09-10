using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class NewMapStreamer : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        private Saxon.Eej.ee.stream.Inversion[] actionInversions;
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.expr.SystemFunctionCall exp = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
            Saxon.Hej.expr.Expression block = exp.getArg(0);
            if (!(((block is Saxon.Hej.expr.instruct.Block)))) {
                return base.computeStreamability(contextInfo, reasons);
            }
            Saxon.Hej.expr.Operand[] children = ((Saxon.Hej.expr.instruct.Block)(block)).getOperanda();
            if ((children.Length == 0)) {
                return Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS;
            } else {
                Saxon.Eej.ee.stream.Sweep sweep = Saxon.Eej.ee.stream.Sweep.MOTIONLESS;
                foreach (Saxon.Hej.expr.Operand o in children) {
                    Saxon.Hej.expr.Expression e = o.getChildExpression();
                    if (!(e.isCallOn(typeof(Saxon.Hej.ma.map.MapFunctionSet.MapEntry)))) {
                        return base.computeStreamability(contextInfo, reasons);
                    }
                    Saxon.Eej.ee.stream.PostureAndSweep operandPS = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(e, contextInfo, reasons);
                    if (((operandPS.getPosture() == Saxon.Eej.ee.stream.Posture.ROAMING) || (operandPS.getSweep() == Saxon.Eej.ee.stream.Sweep.FREE_RANGING))) {
                        /*134*/Saxon.Eej.ee.stream.Streamability.setPostureAndSweep(block, Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING);
                        return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
                    } else {
                        sweep = /*134*/Saxon.Eej.ee.stream.Streamability.wider(sweep, operandPS.getSweep());
                    }
                }
                Saxon.Eej.ee.stream.PostureAndSweep resultPS = new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.GROUNDED, sweep);
                /*134*/Saxon.Eej.ee.stream.Streamability.setPostureAndSweep(block, resultPS);
                return resultPS;
            }
        }
        private Saxon.Hej.expr.Operand[] getChildren() {
            Saxon.Hej.expr.SystemFunctionCall exp = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
            Saxon.Hej.expr.Expression arg = exp.getArg(0);
            return ((Saxon.Hej.expr.instruct.Block)(arg)).getOperanda();
        }
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            Saxon.Hej.expr.SystemFunctionCall exp = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
            Saxon.Hej.expr.Expression block = exp.getArg(0);
            if (!(((block is Saxon.Hej.expr.instruct.Block)))) {
                return base.getWatchMaker(forGrouping);
            }
            Saxon.Hej.expr.Operand[] actions = getChildren();
            if ((actionInversions == null)) {
                actionInversions = new Saxon.Eej.ee.stream.Inversion[actions.Length];
                for (int i = 0; (i<actions.Length); i++) {
                    Saxon.Hej.expr.Expression action = actions[i].getChildExpression();
                    if ((/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(action) == Saxon.Eej.ee.stream.Sweep.CONSUMING)) {
                        actionInversions[i] = /*134*/Saxon.Eej.ee.stream.Inversion.invertExpression(action, forGrouping);
                    } else {
                        actionInversions[i] = null;
                    }
                }
            }
            return (watchManager, @out, context) => new Saxon.Eej.ee.stream.watch.Trigger(Saxon.Hej.pattern.AnchorPattern.getInstance(), new Saxon.Eej.ee.stream.adjunct.NewMapStreamer.NewMapForkingFeed(getExpression(), this, @out, context, watchManager), context);
        }
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.stream.adjunct.NewMapStreamer.NewMapSimpleFeed(getExpression(), @out, context);
        }
        private class NewMapForkingFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
            private readonly Saxon.Eej.ee.stream.adjunct.NewMapStreamer streamer;
            private readonly Saxon.Eej.ee.stream.watch.WatchManager watchManager;
            private Saxon.Hej.ma.map.MapItem resultMap;
            internal NewMapForkingFeed(Saxon.Hej.expr.Expression exp, Saxon.Eej.ee.stream.adjunct.NewMapStreamer streamer, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context, Saxon.Eej.ee.stream.watch.WatchManager watchManager) : base (exp, result, context) {
                this.streamer = streamer;
                this.watchManager = watchManager;
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                base.open(terminator);
                resultMap = new Saxon.Impl.DataModel.ImmMap();
                Saxon.Hej.expr.Operand[] actions = streamer.getChildren();
                for (int i = 0; (i<actions.Length); i++) {
                    Saxon.Hej.expr.Expression action = actions[i].getChildExpression();
                    if ((/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(action) == Saxon.Eej.ee.stream.Sweep.CONSUMING)) {
                        Saxon.Eej.ee.stream.feed.ItemFeed feed = new Saxon.Eej.ee.stream.adjunct.NewMapStreamer.SingleProngFeed(getExpression(), getResultFeed(), getContext(), this);
                        watchManager.addWatch(streamer.actionInversions[i].getWatch(watchManager, feed, getContext()), true);
                    } else {
                        Saxon.Hej.ma.map.MapItem sm = (Saxon.Hej.ma.map.MapItem)(action.evaluateItem(getContext()));
                        foreach (Saxon.Hej.ma.map.KeyValuePair pair in sm.keyValuePairs()) {
                            if ((resultMap.get(pair.key) != null)) {
                                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Duplicate key value in map: "+Saxon.Hej.trans.Err.depict(pair.key)), "XTDE3365");
                                dynamicError(err);
                            } else {
                                resultMap = resultMap.addEntry(pair.key, pair.value);
                            }
                        }
                    }
                }
            }
            public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
                return null;
            }
            public override void append(Saxon.Hej.om.Item item) {
                throw new Saxon.Ejava.lang.UnsupportedOperationException();
            }
            public override void close() {
                if (!(hasFailed())) {
                    Saxon.Hej.@event.Outputter @out = getNextOutputter();
                    @out.append(resultMap);
                    @out.close();
                }
                resultMap = null;
            }
            public bool containsKey(Saxon.Hej.value.AtomicValue key) {
                return (resultMap.get(key) != null);
            }
            public void addEntry(Saxon.Hej.value.AtomicValue key, Saxon.Hej.om.GroundedValue value) {
                resultMap = resultMap.addEntry(key, value);
            }
        
        }
        private class SingleProngFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
            private readonly Saxon.Eej.ee.stream.adjunct.NewMapStreamer.NewMapForkingFeed forkingFeed;
            public SingleProngFeed(Saxon.Hej.expr.Expression expression, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context, Saxon.Eej.ee.stream.adjunct.NewMapStreamer.NewMapForkingFeed forkingFeed) : base (expression, @out, context) {
                this.forkingFeed = forkingFeed;
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
            }
            public override void append(Saxon.Hej.om.Item item) {
                foreach (Saxon.Hej.ma.map.KeyValuePair pair in ((Saxon.Hej.ma.map.MapItem)(item)).keyValuePairs()) {
                    if (forkingFeed.containsKey(pair.key)) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Duplicate key value in map: "+Saxon.Hej.trans.Err.depict(pair.key)), "XTDE3365");
                        dynamicError(err);
                    } else {
                        forkingFeed.addEntry(pair.key, pair.value);
                    }
                }
            }
            public override void close() {
            }
            public override void setHasFailed() {
                forkingFeed.setHasFailed();
            }
        
        }
        private class NewMapSimpleFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
            internal Saxon.Impl.DataModel.ImmMap resultMap;
            internal NewMapSimpleFeed(Saxon.Hej.expr.Expression exp, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (exp, result, context) {
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                base.open(terminator);
                resultMap = new Saxon.Impl.DataModel.ImmMap();
            }
            public override void append(Saxon.Hej.om.Item item) {
                if ((item is Saxon.Hej.ma.map.MapItem)) {
                    foreach (Saxon.Hej.ma.map.KeyValuePair pair in ((Saxon.Hej.ma.map.MapItem)(item)).keyValuePairs()) {
                        bool existing = resultMap.initialPut(pair.key, pair.value);
                        if (existing) {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Duplicate key value in map: "+Saxon.Hej.trans.Err.depict(pair.key)), "XTDE3365");
                            dynamicError(err);
                        }
                    }
                } else {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Argument to map constructor contains an item that is not a map", "XPTY0004");
                    err.setIsTypeError(true);
                    dynamicError(err);
                }
            }
            public override void close() {
                if (!(hasFailed())) {
                    Saxon.Hej.@event.Outputter @out = getNextOutputter();
                    @out.append(resultMap);
                    @out.close();
                }
                resultMap = null;
            }
        
        }
    
    }
}