using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class MotionlessFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
        private readonly Saxon.Hej.expr.Expression expression;
        private bool selected;
        public MotionlessFeed(Saxon.Hej.expr.Expression expression, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (expression, result, context) {
            this.expression = expression;
        }
        public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
            Saxon.Hej.om.SequenceIterator iter = expression.iterate(getContext());
            Saxon.Hej.om.Item item;
            while (((item = iter.next()) != null)) {
                if ((item is Saxon.Eej.ee.stream.om.FleetingParentNode)) {
                    selected = true;
                    return getResultFeed().startSelectedParentNode((Saxon.Eej.ee.stream.om.FleetingParentNode)(item), locationId);
                } else {
                    getResultFeed().append(item);
                }
            }
            return null;
        }
        public override void append(Saxon.Hej.om.Item item) {
            Saxon.Hej.om.SequenceIterator iter = expression.iterate(getContext());
            processItems(iter, getNextOutputter());
        }
        public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
            if (selected) {
                getResultFeed().endSelectedParentNode(locationId);
                selected = false;
            }
        }
    
    }
}