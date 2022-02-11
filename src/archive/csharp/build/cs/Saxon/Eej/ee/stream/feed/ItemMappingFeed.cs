using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class ItemMappingFeed : Saxon.Eej.ee.stream.feed.GroundedItemFeed {
        private readonly Saxon.Hej.expr.ItemMappingFunction function;
        public ItemMappingFeed(Saxon.Hej.expr.Expression expr, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.ItemMappingFunction function) : base (expr, result, context) {
            this.function = function;
        }
        public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
            if (!(hasFailed())) {
                Saxon.Hej.om.Item mapped = null;
                try {
                    mapped = function.mapItem(node);
                } catch (Saxon.Hej.trans.XPathException err) {
                    dynamicError(err);
                }
                if ((mapped != null)) {
                    if ((mapped is Saxon.Eej.ee.stream.om.FleetingParentNode)) {
                        return getResultFeed().startSelectedParentNode((Saxon.Eej.ee.stream.om.FleetingParentNode)(mapped), locationId);
                    } else {
                        throw new Saxon.Ejava.lang.AssertionError("Item mapping feed must map events to events");
                    }
                }
            }
            return null;
        }
        public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
            if (!(hasFailed())) {
                getResultFeed().endSelectedParentNode(locationId);
            }
        }
        public override void append(Saxon.Hej.om.Item item) {
            if (!(hasFailed())) {
                Saxon.Hej.om.Item mapped = null;
                try {
                    mapped = function.mapItem(item);
                } catch (Saxon.Hej.trans.XPathException err) {
                    dynamicError(err);
                }
                if ((mapped != null)) {
                    getResultFeed().append(mapped);
                }
            }
        }
        public override void close() {
            if (!(hasFailed())) {
                base.close();
            }
        }
    
    }
}