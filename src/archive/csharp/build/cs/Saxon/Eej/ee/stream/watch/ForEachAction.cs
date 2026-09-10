using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.watch {
    public class ForEachAction : Saxon.Eej.ee.stream.feed.ItemFeed {
        protected internal Saxon.Hej.expr.XPathContext bodyContext;
        protected internal readonly Saxon.Eej.ee.stream.watch.WatchManager watchManager;
        private readonly Saxon.Hej.expr.Expression action;
        private Saxon.Eej.ee.stream.Inversion inversion;
        protected internal Saxon.Hej.expr.elab.PullEvaluator actionPuller;
        private readonly bool actionIsConsuming;
        public ForEachAction(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Hej.expr.Expression expression, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (expression, result, context) {
            this.watchManager = watchManager;
            action = ((Saxon.Hej.expr.ContextSwitchingExpression)(expression)).getActionExpression();
            actionIsConsuming = (/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(action) == Saxon.Eej.ee.stream.Sweep.CONSUMING);
            if (((actionPuller == null) && !(actionIsConsuming))) {
                actionPuller = action.makeElaborator().elaborateForPull();
            }
        }
        public void setInversion(Saxon.Eej.ee.stream.Inversion inversion) {
            this.inversion = inversion;
        }
        protected internal Saxon.Eej.ee.stream.Inversion getInversion() {
            return inversion;
        }
        protected internal Saxon.Eej.ee.stream.watch.WatchManager getWatchManager() {
            return watchManager;
        }
        protected internal Saxon.Hej.expr.Expression getAction() {
            return action;
        }
        protected internal bool isActionConsuming() {
            return actionIsConsuming;
        }
        protected internal Saxon.Hej.expr.XPathContext getBodyContext() {
            return bodyContext;
        }
        public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
            base.open(terminator);
            makeBodyContext();
        }
        protected internal void makeBodyContext() {
            bodyContext = getContext().newMinorContext();
            Saxon.Hej.tree.iter.ManualIterator iter = new Saxon.Hej.tree.iter.ManualIterator();
            bodyContext.setCurrentIterator(iter);
        }
        public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
            if (actionIsConsuming) {
                startSelectedNode(node);
                Saxon.Eej.ee.stream.feed.ItemFeed thisFeed = new Saxon.Eej.ee.stream.feed.NoOpenOrCloseFeed(getResultFeed(), bodyContext);
                Saxon.Eej.ee.stream.watch.Watch watch = inversion.getWatch(watchManager, thisFeed, bodyContext);
                watch.setAnchorNode(node);
                watchManager.addWatch(watch, true);
                return null;
            } else {
                Saxon.Hej.tree.iter.ManualIterator iter = (Saxon.Hej.tree.iter.ManualIterator)(bodyContext.getCurrentIterator());
                iter.setContextItem(node);
                iter.incrementPosition();
                Saxon.Hej.om.SequenceIterator stepIter = /*Delegate.iterate()*/actionPuller(bodyContext);
                processItems(stepIter, getResultFeed());
                return null;
            }
        }
        private void startSelectedNode(Saxon.Eej.ee.stream.om.FleetingNode node) {
            Saxon.Hej.tree.iter.ManualIterator iter = (Saxon.Hej.tree.iter.ManualIterator)(bodyContext.getCurrentIterator());
            iter.setContextItem(node);
            iter.incrementPosition();
        }
        public override void append(Saxon.Hej.om.Item it) {
            if ((actionIsConsuming && (it is Saxon.Eej.ee.stream.om.FleetingParentNode))) {
                Saxon.Hej.expr.XPathContext cxt = bodyContext;
                Saxon.Eej.ee.stream.feed.ItemFeed thisFeed = new Saxon.Eej.ee.stream.feed.NoOpenOrCloseFeed(getResultFeed(), cxt);
                Saxon.Eej.ee.stream.watch.Watch watch = inversion.getWatch(watchManager, thisFeed, cxt);
                if (watch.matchesNode((Saxon.Eej.ee.stream.om.FleetingParentNode)(it), cxt)) {
                    startSelectedNode((Saxon.Eej.ee.stream.om.FleetingNode)(it));
                    watch.open(/*134*/Saxon.Eej.ee.stream.watch.Terminator.getInstance());
                    watch.processItem(it);
                    watch.close();
                    endSelectedParentNode(Saxon.Hej.expr.parser.Loc.NONE);
                } else {
                    watch.open(/*134*/Saxon.Eej.ee.stream.watch.Terminator.getInstance());
                    getWatchManager().addWatch(watch, true);
                }
            } else {
                if ((actionPuller == null)) {
                    actionPuller = action.makeElaborator().elaborateForPull();
                }
                Saxon.Hej.expr.XPathContext context = bodyContext;
                Saxon.Hej.tree.iter.ManualIterator iterator = (Saxon.Hej.tree.iter.ManualIterator)(context.getCurrentIterator());
                iterator.setContextItem(it);
                iterator.incrementPosition();
                Saxon.Hej.@event.Outputter @out = getNextOutputter();
                try {
                    Saxon.Hej.om.SequenceTool.supply(/*Delegate.iterate()*/actionPuller(context), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)(/* Unresolved method reference */@out.append));
                } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                    throw e.getXPathException();
                }
            }
        }
        public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
        }
        public override void close() {
            base.close();
        }
    
    }
}