using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class ItemCheckingFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
        private readonly Saxon.Hej.type.ItemType requiredType;
        private readonly Saxon.Hej.expr.parser.RoleDiagnostic role;
        private readonly Saxon.Hej.Configuration config;
        private int level = 0;
        private readonly bool allowAnyElement;
        public ItemCheckingFeed(Saxon.Hej.expr.Expression checker, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (checker, result, context) {
            requiredType = ((Saxon.Hej.expr.ItemChecker)(checker)).getRequiredType();
            role = ((Saxon.Hej.expr.ItemChecker)(checker)).getRoleLocator();
            config = context.getConfiguration();
            Saxon.Hej.type.Affinity aff = config.getTypeHierarchy().relationship(requiredType, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
            allowAnyElement = ((aff == Saxon.Hej.type.Affinity.SAME_TYPE) || (aff == Saxon.Hej.type.Affinity.SUBSUMES));
        }
        public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
            if (!(hasFailed())) {
                checkItemType(node);
                return getResultFeed().startSelectedParentNode(node, locationId);
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
                checkItemType(item);
                getNextOutputter().append(item);
            }
        }
        private void checkItemType(Saxon.Hej.om.Item item) {
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            if ((!(hasFailed()) && !(requiredType.matches(item, th)))) {
                string message = role.composeErrorMessage(requiredType, item, th);
                string errorCode = role.getErrorCode();
                if ("XPDY0050".Equals(errorCode)) {
                    Saxon.Hej.trans.XPathException te = new Saxon.Hej.trans.XPathException(message, errorCode, getExpression().getLocation());
                    te.setIsTypeError(false);
                    dynamicError(te);
                } else {
                    typeError(message, errorCode);
                }
            }
        }
        public override void close() {
            if (!(hasFailed())) {
                getResultFeed().close();
            }
        }
        private void typeError(string message, string errorCode) {
            Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException(message, errorCode, getExpression().getLocation());
            e.setIsTypeError(true);
            dynamicError(e);
        }
        public override void startDocument(int properties) {
            if ((level++ == 0)) {
                checkItemType(new Saxon.Eej.ee.stream.om.FleetingDocumentNode(-1));
            }
            base.startDocument(properties);
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.s9api.Location location, int properties) {
            if (((level++ == 0) && !(allowAnyElement))) {
                Saxon.Hej.tree.util.Orphan node = new Saxon.Hej.tree.util.Orphan(config);
                node.setNodeKind(Saxon.Hej.type.Type.ELEMENT);
                node.setNodeName(elemName);
                node.setTypeAnnotation(type);
                checkItemType(node);
            }
            base.startElement(elemName, type, location, properties);
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            if (((level++ == 0) && !(allowAnyElement))) {
                Saxon.Hej.tree.util.Orphan node = new Saxon.Hej.tree.util.Orphan(config);
                node.setNodeKind(Saxon.Hej.type.Type.ELEMENT);
                node.setNodeName(elemName);
                node.setTypeAnnotation(type);
                checkItemType(node);
            }
            base.startElement(elemName, type, attributes, namespaces, location, properties);
        }
        public override void endElement() {
            level--;
            base.endElement();
        }
        public override void endDocument() {
            level--;
            base.endDocument();
        }
    
    }
}