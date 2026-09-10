using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class SimpleContentFeed : Saxon.Eej.ee.stream.feed.AbsorptionFeed {
        public SimpleContentFeed(Saxon.Eej.ee.stream.feed.ItemFeed result) : base (result, null) {
        }
        protected internal override Saxon.Eej.ee.stream.feed.AbsorptionFeed.PendingItem makePendingItem(int sequenceNr, Saxon.Eej.ee.stream.om.FleetingParentNode node) {
            return new Saxon.Eej.ee.stream.feed.SimpleContentFeed.PendingContent(sequenceNr, node);
        }
        protected internal override Saxon.Hej.om.Sequence processGroundedItem(Saxon.Hej.om.Item item) {
            if (((item is Saxon.Hej.om.NodeInfo) && (((Saxon.Hej.om.NodeInfo)(item)).getNodeKind() == Saxon.Hej.type.Type.TEXT))) {
                Saxon.Hej.str.UnicodeString cs = item.getUnicodeStringValue();
                if (cs.isEmpty()) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                } else {
                    return item.atomize();
                }
            } else {
                if ((item is Saxon.Hej.value.AtomicValue)) {
                    return item;
                } else {
                    if ((item is Saxon.Hej.om.NodeInfo)) {
                        return item.atomize();
                    } else {
                        Saxon.Hej.trans.XPathException xe = new Saxon.Hej.trans.XPathException(("Cannot atomize item of type "+Saxon.Hej.type.Type.displayTypeName(item)), "FOTY0013", getExpression().getLocation());
                        xe.setIsTypeError(true);
                        throw xe;
                    }
                }
            }
        }
        private class PendingContent : Saxon.Eej.ee.stream.feed.AbsorptionFeed.PendingItem {
            public Saxon.Hej.str.UnicodeBuilder buffer;
            public Saxon.Eej.ee.stream.om.FleetingParentNode node;
            public PendingContent(int sequence, Saxon.Eej.ee.stream.om.FleetingParentNode node) : base (sequence) {
                buffer = new Saxon.Hej.str.UnicodeBuilder();
                this.node = node;
            }
            public override Saxon.Hej.@event.Receiver getGatherer() {
                return new Saxon.Eej.ee.stream.feed.StringValueGatherer(buffer);
            }
            public override Saxon.Hej.om.Sequence deliver() {
                if ((node is Saxon.Eej.ee.stream.om.FleetingElementNode)) {
                    Saxon.Eej.ee.stream.om.FleetingElementNode element = (Saxon.Eej.ee.stream.om.FleetingElementNode)(node);
                    element.setStringValue(buffer.toUnicodeString());
                    return element.atomize();
                } else {
                    Saxon.Eej.ee.stream.om.FleetingDocumentNode document = (Saxon.Eej.ee.stream.om.FleetingDocumentNode)(node);
                    document.setStringValue(buffer.toUnicodeString());
                    return document.atomize();
                }
            }
            public override void rollback() {
                buffer.clear();
            }
        
        }
    
    }
}