using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class IgnorableSpaceStrippingRule : Saxon.Hej.om.SpaceStrippingRule {
        private readonly static Saxon.Hej.om.IgnorableSpaceStrippingRule THE_INSTANCE = new Saxon.Hej.om.IgnorableSpaceStrippingRule();
        public static Saxon.Hej.om.IgnorableSpaceStrippingRule getInstance() {
            return THE_INSTANCE;
        }
        public int isSpacePreserving(Saxon.Hej.om.NodeName name, Saxon.Hej.type.SchemaType schemaType) {
            if (((((schemaType != Saxon.Hej.type.Untyped.getInstance()) && schemaType.isComplexType()) && !(((Saxon.Hej.type.ComplexType)(schemaType)).isSimpleContent())) && !(((Saxon.Hej.type.ComplexType)(schemaType)).isMixedContent()))) {
                return Saxon.Hej.@event.Stripper.ALWAYS_STRIP;
            } else {
                return Saxon.Hej.@event.Stripper.ALWAYS_PRESERVE;
            }
        }
        public Saxon.Hej.@event.ProxyReceiver makeStripper(Saxon.Hej.@event.Receiver next) {
            return new Saxon.Hej.@event.IgnorableWhitespaceStripper(next);
        }
        public void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            presenter.startElement("strip.ignorable");
            presenter.endElement();
        }
    
    }
}