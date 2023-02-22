using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.watch {
    public class Trigger : Saxon.Eej.ee.stream.watch.PatternWatch {
        private readonly Saxon.Eej.ee.stream.feed.ItemFeed action;
        private readonly Saxon.Hej.expr.XPathContext selectionContext;
        private bool matchCurrentGroup = false;
        public Trigger(Saxon.Hej.pattern.Pattern selection, Saxon.Eej.ee.stream.feed.ItemFeed action, Saxon.Hej.expr.XPathContext selectionContext)  {
            this.action = action;
            this.selectionContext = selectionContext;
            setSelection(selection);
        }
        public override bool matchesNode(Saxon.Eej.ee.stream.om.FleetingNode node, Saxon.Hej.expr.XPathContext context) {
            return base.matchesNode(node, selectionContext);
        }
        public Saxon.Eej.ee.stream.feed.ItemFeed getAction() {
            return action;
        }
        public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
            Saxon.Eej.ee.stream.om.FleetingParentNode anchor = getAnchorNode();
            if (((anchor != null) && (anchor.getNodeKind() == Saxon.Hej.type.Type.DOCUMENT))) {
                terminator = new Inner_Terminator_d385e227(this);
            }
            action.open(terminator);
        }
        public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
            return action.startSelectedParentNode(node, locationId);
        }
        public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
            action.endSelectedParentNode(locationId);
        }
        public override void processItem(Saxon.Hej.om.Item item) {
            action.append(item);
        }
        public override void close() {
            action.close();
        }
        public bool isMatchCurrentGroup() {
            return matchCurrentGroup;
        }
        public override void setSelection(Saxon.Hej.pattern.Pattern selection) {
            base.setSelection(selection);
            if (selection.matchesCurrentGroup()) {
                setMatchCurrentGroup(true);
            }
        }
        public void setMatchCurrentGroup(bool matchCurrentGroup) {
            this.matchCurrentGroup = matchCurrentGroup;
        }
    
        private class Inner_Terminator_d385e227 : Saxon.Eej.ee.stream.watch.Terminator {
            private readonly Trigger outerInstance;
            public Inner_Terminator_d385e227(Trigger outerInstance) {
                this.outerInstance = outerInstance;
            }
            public override void terminate() {
                throw new Saxon.Hej.trans.QuitParsingException(false);
            }
        }

    }
}