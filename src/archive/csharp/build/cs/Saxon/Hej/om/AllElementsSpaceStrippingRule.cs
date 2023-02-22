using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class AllElementsSpaceStrippingRule : Saxon.Hej.om.SpaceStrippingRule {
        private readonly static Saxon.Hej.om.AllElementsSpaceStrippingRule THE_INSTANCE = new Saxon.Hej.om.AllElementsSpaceStrippingRule();
        public static Saxon.Hej.om.AllElementsSpaceStrippingRule getInstance() {
            return THE_INSTANCE;
        }
        public int isSpacePreserving(Saxon.Hej.om.NodeName fingerprint, Saxon.Hej.type.SchemaType schemaType) {
            return Saxon.Hej.@event.Stripper.STRIP_DEFAULT;
        }
        public Saxon.Hej.@event.ProxyReceiver makeStripper(Saxon.Hej.@event.Receiver next) {
            return new Saxon.Hej.@event.Stripper(this, next);
        }
        public void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            presenter.startElement("strip.all");
            presenter.endElement();
        }
    
    }
}