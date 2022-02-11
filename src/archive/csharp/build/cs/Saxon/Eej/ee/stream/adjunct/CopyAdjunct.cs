using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class CopyAdjunct : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.stream.adjunct.CopyAdjunct.CopyFeed((Saxon.Hej.expr.instruct.Copy)(getExpression()), @out, context);
        }
        private class CopyFeed : Saxon.Eej.ee.stream.feed.ComplexNodeEventFeed {
            internal Saxon.Hej.expr.XPathContext context;
            internal int nodeKind;
            public CopyFeed(Saxon.Hej.expr.instruct.Copy expression, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (expression, result, context) {
            }
            protected internal override void processLeft(Saxon.Hej.expr.XPathContext context) {
                Saxon.Hej.om.Item item = context.getContextItem();
                Saxon.Hej.expr.instruct.Copy expression = (Saxon.Hej.expr.instruct.Copy)(getExpression());
                if ((item is Saxon.Eej.ee.stream.om.FleetingParentNode)) {
                    Saxon.Eej.ee.stream.om.FleetingParentNode node = (Saxon.Eej.ee.stream.om.FleetingParentNode)(item);
                    nodeKind = node.getNodeKind();
                    switch (nodeKind) {
                      case Saxon.Hej.type.Type.DOCUMENT:
                        {
                            if (!(expression.isPreservingTypes())) {
                                Saxon.Hej.Controller controller = context.getController();
                                Saxon.Hej.Configuration config = controller.getConfiguration();
                                Saxon.Hej.lib.ParseOptions options = new Saxon.Hej.lib.ParseOptions(expression.getValidationOptions());
                                options.setSpaceStrippingRule(Saxon.Hej.om.NoElementsSpaceStrippingRule.getInstance());
                                Saxon.Hej.@event.Receiver val = config.getDocumentValidator(next, node.getBaseURI(), options, expression.getLocation());
                                next = new Saxon.Hej.@event.ComplexContentOutputter(val);
                            }
                            next.startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
                            break ;
                        }
                      case Saxon.Hej.type.Type.ELEMENT:
                        {
                            Saxon.Hej.om.NodeName elemName = Saxon.Hej.om.NameOfNode.makeName(node);
                            Saxon.Hej.type.SchemaType typeCode = ((expression.getValidationAction() == Saxon.Hej.lib.Validation.PRESERVE) ? Saxon.Hej.type.AnyType.getInstance() : Saxon.Hej.type.Untyped.getInstance());
                            int properties = (expression.isBequeathNamespacesToChildren() ? Saxon.Hej.@event.ReceiverOption.NONE : Saxon.Hej.@event.ReceiverOption.DISINHERIT_NAMESPACES);
                            if (!(expression.isPreservingTypes())) {
                                Saxon.Hej.Controller controller = context.getController();
                                System.Diagnostics.Debug.Assert((controller != null));
                                Saxon.Hej.lib.ParseOptions options = new Saxon.Hej.lib.ParseOptions(expression.getValidationOptions());
                                options.setTopLevelElement(elemName.getStructuredQName());
                                try {
                                    Saxon.Hej.@event.Receiver val = context.getConfiguration().getElementValidator(next, options, expression.getLocation());
                                    next = new Saxon.Hej.@event.ComplexContentOutputter(val);
                                } catch (Saxon.Hej.type.ValidationException e) {
                                    dynamicError(e);
                                }
                            }
                            if ((next.getSystemId() == null)) {
                                next.setSystemId(expression.getNewBaseURI(context, node));
                            }
                            next.startElement(elemName, typeCode, expression.getLocation(), properties);
                            expression.outputNamespaceNodes(next, elemName, node);
                            break ;
                        }
                      default:
                        try {
                            Saxon.Hej.expr.instruct.TailCall tc = expression.processLeavingTail(next, context, node);
                            while ((tc != null)) {
                                tc = tc.processLeavingTail();
                            }
                        } catch (Saxon.Hej.trans.XPathException err) {
                            err.maybeSetLocation(expression.getLocation());
                            throw;
                        }
                        break ;
                    }
                }
            }
            protected internal override void processRight(Saxon.Hej.expr.XPathContext context) {
                try {
                    if ((nodeKind == Saxon.Hej.type.Type.DOCUMENT)) {
                        next.endDocument();
                    } else {
                        if ((nodeKind == Saxon.Hej.type.Type.ELEMENT)) {
                            next.endElement();
                        }
                    }
                } catch (Saxon.Hej.trans.XPathException e) {
                    dynamicError(e);
                }
            }
            public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
                return null;
            }
        
        }
    
    }
}