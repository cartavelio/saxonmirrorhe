using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class NoElementsSpaceStrippingRule : Saxon.Hej.om.SpaceStrippingRule {
        private readonly static Saxon.Hej.om.NoElementsSpaceStrippingRule THE_INSTANCE = new Saxon.Hej.om.NoElementsSpaceStrippingRule();
        public static Saxon.Hej.om.NoElementsSpaceStrippingRule getInstance() {
            return THE_INSTANCE;
        }
        public int isSpacePreserving(Saxon.Hej.om.NodeName fingerprint, Saxon.Hej.type.SchemaType schemaType) {
            return Saxon.Hej.@event.Stripper.ALWAYS_PRESERVE;
        }
        public Saxon.Hej.@event.ProxyReceiver makeStripper(Saxon.Hej.@event.Receiver next) {
            return null;
        }
        public void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            presenter.startElement("strip.none");
            presenter.endElement();
        }
    
    }
}