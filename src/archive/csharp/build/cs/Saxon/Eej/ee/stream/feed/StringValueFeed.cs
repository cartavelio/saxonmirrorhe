using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class StringValueFeed : Saxon.Eej.ee.stream.feed.AbsorptionFeed {
        public StringValueFeed(Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (result, context) {
        }
        protected internal override Saxon.Eej.ee.stream.feed.AbsorptionFeed.PendingItem makePendingItem(int sequenceNr, Saxon.Eej.ee.stream.om.FleetingParentNode node) {
            return new Saxon.Eej.ee.stream.feed.StringValueFeed.PendingStringValue(valuesReadyToBeOutput.Count, node);
        }
        protected internal override Saxon.Hej.om.Sequence processGroundedItem(Saxon.Hej.om.Item item) {
            return new Saxon.Hej.value.StringValue(item.getUnicodeStringValue());
        }
        public override void close() {
            if (!(hasFailed())) {
                getNextOutputter().close();
            }
        }
        private class PendingStringValue : Saxon.Eej.ee.stream.feed.AbsorptionFeed.PendingItem {
            public Saxon.Hej.str.UnicodeBuilder buffer;
            public Saxon.Eej.ee.stream.om.FleetingParentNode node;
            public PendingStringValue(int sequence, Saxon.Eej.ee.stream.om.FleetingParentNode node) : base (sequence) {
                buffer = new Saxon.Hej.str.UnicodeBuilder();
                this.node = node;
            }
            public override Saxon.Hej.@event.Receiver getGatherer() {
                return new Saxon.Eej.ee.stream.feed.StringValueGatherer(buffer);
            }
            public override Saxon.Hej.om.Sequence deliver() {
                return new Saxon.Hej.value.StringValue(buffer.toUnicodeString());
            }
            public override void rollback() {
                buffer.clear();
            }
        
        }
    
    }
}