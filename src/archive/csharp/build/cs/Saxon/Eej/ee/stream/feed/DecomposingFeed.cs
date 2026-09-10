using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class DecomposingFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
        private readonly System.Collections.Generic.Stack<bool> nodeKindStack = new System.Collections.Generic.Stack<bool>();
        public DecomposingFeed(Saxon.Hej.@event.Outputter receiver, Saxon.Hej.expr.XPathContext context) : base (receiver, context, true) {
        }
        public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
        }
        public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
            bool isDocNode = (node is Saxon.Eej.ee.stream.om.FleetingDocumentNode);
            nodeKindStack.Push(isDocNode);
            if (isDocNode) {
                getNextOutputter().startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
            } else {
                Saxon.Eej.ee.stream.om.FleetingElementNode element = (Saxon.Eej.ee.stream.om.FleetingElementNode)(node);
                getNextOutputter().startElement(Saxon.Hej.om.NameOfNode.makeName(element), node.getSchemaType(), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
            }
            return null;
        }
        public override void append(Saxon.Hej.om.Item item) {
            getNextOutputter().append(item);
        }
        public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
            bool isDocNode = nodeKindStack.Pop();
            if (isDocNode) {
                getNextOutputter().endDocument();
            } else {
                getNextOutputter().endElement();
            }
        }
        public override void close() {
        }
    
    }
}