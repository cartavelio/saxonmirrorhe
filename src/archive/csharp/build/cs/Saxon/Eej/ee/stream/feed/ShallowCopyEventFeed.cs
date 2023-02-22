using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class ShallowCopyEventFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
        private readonly Saxon.Hej.trans.Mode mode;
        public ShallowCopyEventFeed(Saxon.Hej.trans.Mode mode, Saxon.Eej.ee.stream.feed.ItemFeed parentExpressionFeed, Saxon.Hej.expr.XPathContext context) : base (null, parentExpressionFeed, context) {
            this.mode = mode;
        }
        public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
            base.open(terminator);
            Saxon.Hej.expr.XPathContext context = getContext();
            Saxon.Hej.@event.Outputter receiver = getNextOutputter();
            Saxon.Hej.expr.XPathContextMajor c2;
            Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(context.getContextItem());
            if ((node.getNodeKind() == Saxon.Hej.type.Type.DOCUMENT)) {
                receiver.startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
            } else {
                c2 = context.newContext();
                bool schemaAware = context.getController().getExecutable().isSchemaAware();
                Saxon.Hej.type.SchemaType schemaType = (schemaAware ? Saxon.Hej.type.AnyType.getInstance() : Saxon.Hej.type.Untyped.getInstance());
                receiver.startElement(Saxon.Hej.om.NameOfNode.makeName(node), schemaType, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                foreach (Saxon.Hej.om.NamespaceBinding ns in node.getAllNamespaces()) {
                    receiver.@namespace(ns.getPrefix(), ns.getNamespaceUri(), Saxon.Hej.@event.ReceiverOption.NONE);
                }
                Saxon.Hej.tree.iter.AxisIterator attributes = node.iterateAxis(Saxon.Hej.om.AxisInfo.ATTRIBUTE);
                if ((attributes != Saxon.Hej.tree.iter.EmptyIterator.ofNodes())) {
                    c2.trackFocus(attributes);
                    mode.applyTemplates(context.getLocalParameters(), context.getTunnelParameters(), null, receiver, c2, Saxon.Hej.expr.parser.Loc.NONE);
                }
            }
        }
        public override void append(Saxon.Hej.om.Item item) {
            getNextOutputter().append(item);
        }
        public override void close() {
            Saxon.Hej.expr.XPathContext context = getContext();
            Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(context.getContextItem());
            if ((node.getNodeKind() == Saxon.Hej.type.Type.DOCUMENT)) {
                getNextOutputter().endDocument();
            } else {
                getNextOutputter().endElement();
            }
            base.close();
        }
    
    }
}