using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.watch {
    public class ForEachGroupPartitionAction : Saxon.Eej.ee.stream.feed.ItemFeed {
        protected internal Saxon.Hej.expr.XPathContext bodyContext;
        private readonly Saxon.Eej.ee.stream.watch.WatchManager watchManager;
        private Saxon.Eej.ee.stream.adjunct.GroupBoundaryTester boundaryTester;
        private Saxon.Eej.ee.stream.feed.ItemFeed currentGroupFeed;
        private readonly Saxon.Hej.expr.XPathContext populationContext;
        private readonly Saxon.Hej.expr.Expression action;
        private Saxon.Eej.ee.stream.Inversion inversion;
        private readonly bool actionIsConsuming;
        private bool closed = true;
        private System.Collections.Generic.IList<Saxon.Eej.ee.stream.watch.Watch> currentGroupWatches;
        public bool feedAppliesToWholeGroup = false;
        private bool closeAtEndElement;
        private Saxon.Hej.om.NodeInfo anchorNode;
        private Saxon.Eej.ee.stream.watch.Watch thisGroupWatch = null;
        private Saxon.Eej.ee.stream.watch.WatchManager.GroupingScope currentGroupingScope = null;
        public ForEachGroupPartitionAction(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Hej.expr.instruct.ForEachGroup expression, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (expression, result, context) {
            this.watchManager = watchManager;
            action = expression.getActionExpression();
            actionIsConsuming = (/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(action) == Saxon.Eej.ee.stream.Sweep.CONSUMING);
            populationContext = context.newMinorContext();
        }
        public void setInversion(Saxon.Eej.ee.stream.Inversion inversion) {
            this.inversion = inversion;
        }
        protected internal Saxon.Eej.ee.stream.Inversion getInversion() {
            return inversion;
        }
        public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
            base.open(terminator);
            makeBodyContext();
            populationContext.setCurrentIterator(new Saxon.Hej.tree.iter.ManualIterator());
            boundaryTester = /*134*/Saxon.Eej.ee.stream.adjunct.ForEachGroupAdjunct.makeBoundaryTester((Saxon.Hej.expr.instruct.ForEachGroup)(getExpression()), populationContext);
            closed = false;
        }
        protected internal void makeBodyContext() {
            bodyContext = getContext().newContext();
            Saxon.Eej.ee.stream.ManualGroupIterator mgi = new Saxon.Eej.ee.stream.ManualGroupIterator(null, 0);
            bodyContext.setCurrentIterator(mgi);
            ((Saxon.Hej.expr.XPathContextMajor)(bodyContext)).setCurrentGroupIterator(mgi);
        }
        public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
            Saxon.Hej.tree.iter.ManualIterator popIter = (Saxon.Hej.tree.iter.ManualIterator)(populationContext.getCurrentIterator());
            popIter.setContextItem(node);
            popIter.incrementPosition();
            anchorNode = node;
            bool needToStartNewGroup = boundaryTester.notifyItem();
            if (actionIsConsuming) {
                if (needToStartNewGroup) {
                    startNewGroup(node);
                    closeAtEndElement = true;
                    return currentGroupFeed.startSelectedParentNode(node, locationId);
                } else {
                    ((Saxon.Eej.ee.stream.ManualGroupIterator)(bodyContext.getCurrentGroupIterator())).appendToCurrentGroup(node);
                    foreach (Saxon.Eej.ee.stream.watch.Watch w in currentGroupWatches) {
                        if (((w is Saxon.Eej.ee.stream.watch.Trigger) && ((Saxon.Eej.ee.stream.watch.Trigger)(w)).isMatchCurrentGroup())) {
                            w.setAnchorNode(node);
                        }
                    }
                    closeAtEndElement = feedAppliesToWholeGroup;
                    if (feedAppliesToWholeGroup) {
                        return currentGroupFeed.startSelectedParentNode(node, locationId);
                    }
                }
                return currentGroupFeed.startSelectedParentNode(node, locationId);
            } else {
                maybeStartNewGroup(node, needToStartNewGroup);
                return null;
            }
        }
        private void maybeStartNewGroup(Saxon.Hej.om.Item node, bool startNewGroup) {
            if (startNewGroup) {
                Saxon.Hej.tree.iter.ManualIterator iter = (Saxon.Hej.tree.iter.ManualIterator)(bodyContext.getCurrentIterator());
                iter.setContextItem(node);
                iter.incrementPosition();
                Saxon.Hej.om.SequenceIterator stepIter = action.iterate(bodyContext);
                processItems(stepIter, getNextOutputter());
            }
        }
        public override void append(Saxon.Hej.om.Item node) {
            Saxon.Hej.tree.iter.ManualIterator popIter = (Saxon.Hej.tree.iter.ManualIterator)(populationContext.getCurrentIterator());
            popIter.setContextItem(node);
            popIter.incrementPosition();
            bool needToStartNewGroup = boundaryTester.notifyItem();
            if (actionIsConsuming) {
                if (needToStartNewGroup) {
                    startNewGroup(node);
                }
                currentGroupFeed.append(node);
            } else {
                maybeStartNewGroup(node, needToStartNewGroup);
            }
        }
        private void startNewGroup(Saxon.Hej.om.Item node) {
            if ((currentGroupFeed != null)) {
                watchManager.endGroupingScope(currentGroupingScope);
                watchManager.stopCapturingGroupingWatches(currentGroupingScope);
                currentGroupFeed.close();
            }
            if ((thisGroupWatch != null)) {
                watchManager.removeWatch(thisGroupWatch);
            }
            currentGroupingScope = watchManager.startGroupingScope();
            watchManager.startCapturingGroupingWatches(currentGroupingScope);
            Saxon.Eej.ee.stream.ManualGroupIterator firstItemsIter = (Saxon.Eej.ee.stream.ManualGroupIterator)(bodyContext.getCurrentIterator());
            firstItemsIter.setContextItem(node);
            firstItemsIter.incrementPosition();
            firstItemsIter.startNewCurrentGroup(node);
            if ((boundaryTester is Saxon.Eej.ee.stream.adjunct.GroupAdjacentTester)) {
                firstItemsIter.setCurrentGroupingKey(((Saxon.Eej.ee.stream.adjunct.GroupAdjacentTester)(boundaryTester)).getCurrentGroupingKey());
            }
            if ((node is Saxon.Eej.ee.stream.om.FleetingNode)) {
                Saxon.Eej.ee.stream.feed.ItemFeed thisFeed = new Saxon.Eej.ee.stream.feed.NoOpenOrCloseFeed(getResultFeed(), bodyContext);
                Saxon.Eej.ee.stream.watch.Trigger watch = getInversion().getWatch(watchManager, thisFeed, bodyContext);
                Saxon.Hej.pattern.Pattern pattern = watch.getSelection();
                Saxon.Eej.ee.stream.feed.ItemFeed actionFeed = watch.getAction();
                if (pattern.matchesCurrentGroup()) {
                    feedAppliesToWholeGroup = true;
                }
                bool opened = false;
                if ((pattern is Saxon.Hej.pattern.AnchorPattern)) {
                    currentGroupFeed = actionFeed;
                } else {
                    Saxon.Eej.ee.stream.feed.FilteringFeed.Filter filter = new Saxon.Eej.ee.stream.feed.FilteringFeed.FilterLambda((item, position) => (pattern.matchesBeneathAnchor((Saxon.Hej.om.NodeInfo)(item), anchorNode, bodyContext) ? Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.MATCHES : Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.SKIP));
                    currentGroupFeed = new Saxon.Eej.ee.stream.feed.FilteringFeed(watchManager, actionFeed, getContext(), filter);
                    if ((pattern is Saxon.Hej.pattern.AncestorQualifiedPattern)) {
                        watchManager.addWatch(watch, true);
                        opened = true;
                        thisGroupWatch = watch;
                    }
                }
                if (!(opened)) {
                    currentGroupFeed.open(getTerminator());
                }
            } else {
                currentGroupFeed = new Saxon.Eej.ee.stream.feed.NoOpenOrCloseFeed(findCurrentGroupFeed(), bodyContext);
                currentGroupFeed.open(getTerminator());
            }
            currentGroupWatches = currentGroupingScope.watches;
            currentGroupingScope.isActive = false;
        }
        private Saxon.Eej.ee.stream.feed.ItemFeed findCurrentGroupFeed() {
            Saxon.Eej.ee.stream.feed.ItemFeed thisFeed = new Saxon.Eej.ee.stream.feed.NoOpenOrCloseFeed(getResultFeed(), bodyContext);
            Saxon.Eej.ee.stream.watch.Trigger watch = getInversion().getWatch(watchManager, thisFeed, bodyContext);
            Saxon.Eej.ee.stream.feed.ItemFeed feed = watch.getAction();
            while (!(((feed.getExpression() is Saxon.Hej.functions.CurrentGroupCall)))) {
                feed = feed.getResultFeed();
            }
            return feed;
        }
        public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
            if (((currentGroupFeed != null) && closeAtEndElement)) {
                currentGroupFeed.endSelectedParentNode(locationId);
            }
        }
        public override void close() {
            if (((currentGroupFeed != null) && !(closed))) {
                closed = true;
                watchManager.endGroupingScope(currentGroupingScope);
                watchManager.stopCapturingGroupingWatches(currentGroupingScope);
                currentGroupFeed.close();
            }
            base.close();
        }
    
    }
}