using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.watch {
    public class ForEachGroupParallelAction : Saxon.Eej.ee.stream.watch.ForEachAction {
        private readonly System.Collections.Generic.Dictionary<Saxon.Hej.expr.sort.CompositeAtomicKey,Saxon.Eej.ee.stream.watch.ForEachGroupParallelAction.GroupInfo> groups = new System.Collections.Generic.Dictionary<Saxon.Hej.expr.sort.CompositeAtomicKey, Saxon.Eej.ee.stream.watch.ForEachGroupParallelAction.GroupInfo>();
        private readonly System.Collections.Generic.IList<Saxon.Eej.ee.stream.watch.ForEachGroupParallelAction.GroupInfo> groupList = new System.Collections.Generic.List<Saxon.Eej.ee.stream.watch.ForEachGroupParallelAction.GroupInfo>();
        private int groupSequence = 1;
        private readonly Saxon.Hej.expr.XPathContext populationContext;
        private readonly Saxon.Hej.lib.StringCollator collator;
        private readonly int implicitTimezone;
        private Saxon.Eej.ee.stream.feed.ItemFeed currentItemFeed;
        private readonly bool compositeKeys;
        private Saxon.Hej.om.NodeInfo anchorNode;
        private class GroupInfo {
            public Saxon.Hej.expr.XPathContext bodyContext;
            public Saxon.Eej.ee.stream.feed.ItemFeed feed;
            public Saxon.Eej.ee.stream.adjunct.FeedToEventBuffer outputter;
            public System.Collections.Generic.IList<Saxon.Eej.ee.stream.watch.Watch> watchList;
            public Saxon.Eej.ee.stream.watch.WatchManager.GroupingScope scope;
        
        }
        public ForEachGroupParallelAction(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Hej.expr.instruct.ForEachGroup expression, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (watchManager, expression, result, context) {
            populationContext = context.newMinorContext();
            collator = expression.getCollation();
            implicitTimezone = context.getImplicitTimezone();
            compositeKeys = expression.isComposite();
        }
        public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
            base.open(terminator);
            populationContext.setCurrentIterator(new Saxon.Hej.tree.iter.ManualIterator());
        }
        private Saxon.Hej.om.AtomicArray getGroupingKey(Saxon.Hej.om.Item node, System.Collections.Generic.IList<Saxon.Hej.expr.sort.AtomicMatchKey> comparisonKey) {
            if ((currentItemFeed != null)) {
                throw new Saxon.Hej.trans.XPathException("xsl:for-each-group can't handle overlapping (crawling) nodes");
            }
            Saxon.Hej.tree.iter.ManualIterator popIter = (Saxon.Hej.tree.iter.ManualIterator)(populationContext.getCurrentIterator());
            popIter.setContextItem(node);
            popIter.incrementPosition();
            Saxon.Hej.expr.Expression groupingKey = ((Saxon.Hej.expr.instruct.ForEachGroup)(getExpression())).getGroupingKey();
            Saxon.Hej.om.SequenceIterator iter = groupingKey.iterate(populationContext);
            Saxon.Hej.om.AtomicArray key = new Saxon.Hej.om.AtomicArray(iter);
            foreach (Saxon.Hej.value.AtomicValue val in key) {
                comparisonKey.Add(val.getXPathMatchKey(collator, implicitTimezone));
            }
            return key;
        }
        public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
            System.Collections.Generic.IList<Saxon.Hej.expr.sort.AtomicMatchKey> comparisonKeyParts = new System.Collections.Generic.List<Saxon.Hej.expr.sort.AtomicMatchKey>();
            Saxon.Hej.om.AtomicArray key = getGroupingKey(node, comparisonKeyParts);
            anchorNode = node;
            System.Collections.Generic.IList<Saxon.Hej.expr.sort.CompositeAtomicKey> separateKeys;
            System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> distinctKeys = null;
            if (compositeKeys) {
                separateKeys = new System.Collections.Generic.List<Saxon.Hej.expr.sort.CompositeAtomicKey>(1);
                separateKeys.Add(new Saxon.Hej.expr.sort.CompositeAtomicKey(comparisonKeyParts));
            } else {
                separateKeys = new System.Collections.Generic.List<Saxon.Hej.expr.sort.CompositeAtomicKey>(comparisonKeyParts.Count);
                distinctKeys = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>();
                System.Collections.Generic.ISet<Saxon.Hej.expr.sort.AtomicMatchKey> duplicates = new System.Collections.Generic.HashSet<Saxon.Hej.expr.sort.AtomicMatchKey>();
                int kk = 0;
                foreach (Saxon.Hej.expr.sort.AtomicMatchKey oneKey in comparisonKeyParts) {
                    if (duplicates.Add(oneKey)) {
                        System.Collections.Generic.IList<Saxon.Hej.expr.sort.AtomicMatchKey> singleKey = new System.Collections.Generic.List<Saxon.Hej.expr.sort.AtomicMatchKey>(1);
                        singleKey.Add(oneKey);
                        separateKeys.Add(new Saxon.Hej.expr.sort.CompositeAtomicKey(singleKey));
                        distinctKeys.Add(((/*1430*/Saxon.Hej.value.AtomicValue)key.itemAt(kk)));
                    }
                    kk++;
                }
            }
            Saxon.Hej.@event.Receiver result = null;
            System.Collections.Generic.IList<Saxon.Eej.ee.stream.feed.ItemFeed> groupFeeds = new System.Collections.Generic.List<Saxon.Eej.ee.stream.feed.ItemFeed>();
            int k = 0;
            foreach (Saxon.Hej.expr.sort.CompositeAtomicKey groupingKey in separateKeys) {
                Saxon.Hej.om.AtomicSequence actualKey = (compositeKeys ? key : distinctKeys[k++]);
                Saxon.Eej.ee.stream.watch.ForEachGroupParallelAction.GroupInfo gp = Saxon.Impl.Helpers.MapUtils.Get(groups, groupingKey);
                bool newGroup = (gp == null);
                Saxon.Eej.ee.stream.watch.WatchManager.GroupingScope scope = getWatchManager().startGroupingScope();
                getWatchManager().startCapturingGroupingWatches(scope);
                if (newGroup) {
                    gp = makeNewGroup(node, groupingKey, actualKey);
                } else {
                    ((Saxon.Eej.ee.stream.ManualGroupIterator)(gp.bodyContext.getCurrentGroupIterator())).appendToCurrentGroup(node);
                }
                gp.scope = scope;
                if (isActionConsuming()) {
                    if (newGroup) {
                        Saxon.Eej.ee.stream.feed.ItemFeed toOutputter = gp.outputter;
                        Saxon.Eej.ee.stream.feed.ItemFeed thisFeed = new Saxon.Eej.ee.stream.feed.NoOpenOrCloseFeed(toOutputter, gp.bodyContext);
                        Saxon.Eej.ee.stream.watch.Trigger watch = getInversion().getWatch(getWatchManager(), thisFeed, gp.bodyContext);
                        Saxon.Hej.pattern.Pattern pattern = watch.getSelection();
                        Saxon.Eej.ee.stream.feed.ItemFeed actionFeed = watch.getAction();
                        if ((pattern is Saxon.Hej.pattern.AnchorPattern)) {
                            gp.feed = actionFeed;
                            gp.feed.open(getTerminator());
                        } else {
                            Saxon.Eej.ee.stream.feed.FilteringFeed.Filter filter = new Saxon.Eej.ee.stream.feed.FilteringFeed.FilterLambda((item, position) => (pattern.matchesBeneathAnchor((Saxon.Hej.om.NodeInfo)(item), anchorNode, bodyContext) ? /*explicit enum*/ Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.MATCHES : /*explicit enum*/ Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.SKIP));
                            gp.feed = new Saxon.Eej.ee.stream.feed.FilteringFeed(actionFeed, getContext(), filter);
                            if ((pattern is Saxon.Hej.pattern.AncestorQualifiedPattern)) {
                                getWatchManager().addWatch(watch, true);
                                gp.feed = new Saxon.Eej.ee.stream.feed.NoCloseFeed(gp.feed, getContext());
                            }
                        }
                    } else {
                        if ((gp.watchList != null)) {
                            foreach (Saxon.Eej.ee.stream.watch.Watch w in gp.watchList) {
                                w.setAnchorNode(node);
                            }
                        }
                    }
                    groupFeeds.Add(gp.feed);
                } else {
                    if (newGroup) {
                        Saxon.Hej.expr.Expression action = ((Saxon.Hej.expr.instruct.ForEachGroup)(getExpression())).getActionExpression();
                        Saxon.Eej.ee.stream.adjunct.FeedToEventBuffer @out = gp.outputter;
                        action.process(@out, gp.bodyContext);
                    }
                }
                if (newGroup) {
                    gp.watchList = gp.scope.watches;
                }
                getWatchManager().stopCapturingGroupingWatches(scope);
            }
            if (!(groupFeeds.Count==0)) {
                if ((groupFeeds.Count == 1)) {
                    currentItemFeed = groupFeeds[0];
                    result = currentItemFeed.startSelectedParentNode(node, locationId);
                } else {
                    currentItemFeed = new Saxon.Eej.ee.stream.watch.ForEachGroupParallelAction.ForkingFeed(groupFeeds, getResultFeed(), getContext());
                    result = currentItemFeed.startSelectedParentNode(node, locationId);
                }
            }
            return result;
        }
        private Saxon.Eej.ee.stream.watch.ForEachGroupParallelAction.GroupInfo makeNewGroup(Saxon.Hej.om.Item node, Saxon.Hej.expr.sort.CompositeAtomicKey comparisonKey, Saxon.Hej.om.AtomicSequence key) {
            Saxon.Eej.ee.stream.watch.ForEachGroupParallelAction.GroupInfo gp = new Saxon.Eej.ee.stream.watch.ForEachGroupParallelAction.GroupInfo();
            gp.outputter = new Saxon.Eej.ee.stream.adjunct.FeedToEventBuffer(getNextOutputter(), this, getContext());
            gp.outputter.setPipelineConfiguration(getPipelineConfiguration());
            Saxon.Hej.expr.XPathContextMajor bodyContext = Saxon.Hej.expr.XPathContextMajor.newThreadContext((Saxon.Hej.expr.XPathContextMinor)(getContext()));
            Saxon.Eej.ee.stream.ManualGroupIterator mgi = new Saxon.Eej.ee.stream.ManualGroupIterator(node, groupSequence++);
            mgi.setCurrentGroupingKey(key);
            mgi.startNewCurrentGroup(node);
            bodyContext.setCurrentIterator(mgi);
            bodyContext.setCurrentGroupIterator(mgi);
            gp.bodyContext = bodyContext;
            Saxon.Impl.Helpers.MapUtils.Put(groups, comparisonKey, gp);
            groupList.Add(gp);
            return gp;
        }
        public override void append(Saxon.Hej.om.Item node) {
            System.Collections.Generic.IList<Saxon.Hej.expr.sort.AtomicMatchKey> comparisonKeyParts = new System.Collections.Generic.List<Saxon.Hej.expr.sort.AtomicMatchKey>();
            Saxon.Hej.om.AtomicArray key = getGroupingKey(node, comparisonKeyParts);
            System.Collections.Generic.IList<Saxon.Hej.expr.sort.CompositeAtomicKey> separateKeys;
            if (compositeKeys) {
                separateKeys = new System.Collections.Generic.List<Saxon.Hej.expr.sort.CompositeAtomicKey>(1);
                separateKeys.Add(new Saxon.Hej.expr.sort.CompositeAtomicKey(comparisonKeyParts));
            } else {
                separateKeys = new System.Collections.Generic.List<Saxon.Hej.expr.sort.CompositeAtomicKey>(comparisonKeyParts.Count);
                System.Collections.Generic.ISet<Saxon.Hej.expr.sort.AtomicMatchKey> duplicates = new System.Collections.Generic.HashSet<Saxon.Hej.expr.sort.AtomicMatchKey>();
                foreach (Saxon.Hej.expr.sort.AtomicMatchKey oneKey in comparisonKeyParts) {
                    if (duplicates.Add(oneKey)) {
                        System.Collections.Generic.IList<Saxon.Hej.expr.sort.AtomicMatchKey> singleKey = new System.Collections.Generic.List<Saxon.Hej.expr.sort.AtomicMatchKey>(1);
                        singleKey.Add(oneKey);
                        separateKeys.Add(new Saxon.Hej.expr.sort.CompositeAtomicKey(singleKey));
                    }
                }
            }
            int k = 0;
            foreach (Saxon.Hej.expr.sort.CompositeAtomicKey groupingKey in separateKeys) {
                Saxon.Hej.om.AtomicSequence actualKey = (compositeKeys ? key : ((/*1430*/Saxon.Hej.value.AtomicValue)key.itemAt(k++)));
                Saxon.Eej.ee.stream.watch.ForEachGroupParallelAction.GroupInfo gp = Saxon.Impl.Helpers.MapUtils.Get(groups, groupingKey);
                bool newGroup = (gp == null);
                if (newGroup) {
                    gp = makeNewGroup(node, groupingKey, actualKey);
                }
                if (isActionConsuming()) {
                    if (newGroup) {
                        Saxon.Eej.ee.stream.feed.ItemFeed thisFeed = new Saxon.Eej.ee.stream.feed.NoOpenOrCloseFeed(getResultFeed(), getBodyContext());
                        Saxon.Eej.ee.stream.watch.Trigger watch = getInversion().getWatch(getWatchManager(), thisFeed, gp.bodyContext);
                        gp.feed = watch.getAction();
                        gp.feed.open(getTerminator());
                    }
                    gp.feed.append(node);
                } else {
                    if (newGroup) {
                        Saxon.Hej.expr.Expression action = ((Saxon.Hej.expr.instruct.ForEachGroup)(getExpression())).getActionExpression();
                        Saxon.Hej.om.SequenceTool.supply(action.iterate(gp.bodyContext), item => getResultFeed().append(item));
                    }
                }
            }
        }
        public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
            if (isActionConsuming()) {
                currentItemFeed.endSelectedParentNode(locationId);
                currentItemFeed = null;
            }
        }
        public override void close() {
            foreach (Saxon.Eej.ee.stream.watch.ForEachGroupParallelAction.GroupInfo gp in groupList) {
                if ((gp.watchList != null)) {
                    foreach (Saxon.Eej.ee.stream.watch.Watch w in gp.watchList) {
                        w.close();
                    }
                }
                if ((gp.feed != null)) {
                    gp.feed.close();
                }
                Saxon.Eej.ee.stream.adjunct.FeedToEventBuffer @out = gp.outputter;
                @out.replay(getNextOutputter());
            }
            base.close();
        }
        private class ForkingFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
            private readonly System.Collections.Generic.IList<Saxon.Eej.ee.stream.feed.ItemFeed> prongs;
            public ForkingFeed(System.Collections.Generic.IList<Saxon.Eej.ee.stream.feed.ItemFeed> prongs, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (result, context) {
                this.prongs = prongs;
            }
            public override void close() {
                foreach (Saxon.Eej.ee.stream.feed.ItemFeed prong in prongs) {
                    prong.close();
                }
            }
            public override Saxon.Hej.@event.PipelineConfiguration getPipelineConfiguration() {
                return null;
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                foreach (Saxon.Eej.ee.stream.feed.ItemFeed prong in prongs) {
                    prong.open(terminator);
                }
            }
            public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
                Saxon.Hej.@event.Receiver result = null;
                foreach (Saxon.Eej.ee.stream.feed.ItemFeed prong in prongs) {
                    Saxon.Hej.@event.Receiver r = prong.startSelectedParentNode(node, locationId);
                    if ((r != null)) {
                        result = ((result == null) ? r : new Saxon.Hej.@event.TeeOutputter(result, r));
                    }
                }
                return result;
            }
            public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
                foreach (Saxon.Eej.ee.stream.feed.ItemFeed prong in prongs) {
                    prong.endSelectedParentNode(locationId);
                }
            }
            public override void append(Saxon.Hej.om.Item item) {
                foreach (Saxon.Eej.ee.stream.feed.ItemFeed prong in prongs) {
                    prong.append(item);
                }
            }
        
        }
    
    }
}