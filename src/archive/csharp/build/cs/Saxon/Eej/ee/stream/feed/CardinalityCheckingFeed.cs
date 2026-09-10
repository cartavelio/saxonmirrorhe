using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class CardinalityCheckingFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
        private int level = 0;
        private int count = 0;
        private readonly int requiredCardinality;
        private readonly Saxon.Hej.expr.parser.RoleDiagnostic role;
        public CardinalityCheckingFeed(Saxon.Hej.expr.Expression checker, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (checker, result, context) {
            if ((checker is Saxon.Hej.expr.CardinalityChecker)) {
                requiredCardinality = ((Saxon.Hej.expr.CardinalityChecker)(checker)).getRequiredCardinality();
                role = ((Saxon.Hej.expr.CardinalityChecker)(checker)).getRoleLocator();
            } else {
                if (checker.isCallOn(typeof(Saxon.Hej.functions.TreatFn))) {
                    Saxon.Hej.functions.TreatFn fn = (Saxon.Hej.functions.TreatFn)(((Saxon.Hej.expr.SystemFunctionCall)(checker)).getTargetFunction());
                    requiredCardinality = fn.getRequiredCardinality();
                    role = fn.makeRoleDiagnostic();
                } else {
                    throw new Saxon.Ejava.lang.IllegalArgumentException();
                }
            }
        }
        public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
            if (!(hasFailed())) {
                checkCount();
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
                checkCount();
                getNextOutputter().append(item);
            }
        }
        private void checkCount() {
            if ((!(hasFailed()) && (count<=2))) {
                count++;
                if (((count == 1) && (requiredCardinality == Saxon.Hej.expr.StaticProperty.EMPTY))) {
                    typeError((("The only value allowed for the "+role.getMessage())+" is an empty sequence"), role.getErrorCode());
                } else {
                    if (((count == 2) && !(Saxon.Hej.value.Cardinality.allowsMany(requiredCardinality)))) {
                        typeError(("A sequence of more than one item is not allowed as the "+role.getMessage()), role.getErrorCode());
                    }
                }
            }
        }
        public override void close() {
            if (!(hasFailed())) {
                if (((count == 0) && !(Saxon.Hej.value.Cardinality.allowsZero(requiredCardinality)))) {
                    typeError(("An empty sequence is not allowed as the "+role.getMessage()), role.getErrorCode());
                    return ;
                }
                getNextOutputter().close();
            }
        }
        private void typeError(string message, string errorCode) {
            Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException(message, errorCode, getExpression().getLocation());
            e.setIsTypeError((getExpression() is Saxon.Hej.expr.CardinalityChecker));
            dynamicError(e);
        }
        public override void startDocument(int properties) {
            if ((level++ == 0)) {
                checkCount();
            }
            base.startDocument(properties);
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.s9api.Location location, int properties) {
            if (!(hasFailed())) {
                if ((level++ == 0)) {
                    checkCount();
                }
                base.startElement(elemName, type, location, properties);
            }
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            if (!(hasFailed())) {
                if ((level++ == 0)) {
                    checkCount();
                }
                base.startElement(elemName, type, attributes, namespaces, location, properties);
            }
        }
        public override void @namespace(string prefix, Saxon.Hej.om.NamespaceUri namespaceUri, int properties) {
            if (!(hasFailed())) {
                if ((level++ == 0)) {
                    checkCount();
                }
                base.@namespace(prefix, namespaceUri, properties);
            }
        }
        public override void attribute(Saxon.Hej.om.NodeName attName, Saxon.Hej.type.SimpleType typeCode, string value, Saxon.Hej.s9api.Location location, int properties) {
            if (!(hasFailed())) {
                if ((level++ == 0)) {
                    checkCount();
                }
                base.attribute(attName, typeCode, value, location, properties);
            }
        }
        public override void endElement() {
            if (!(hasFailed())) {
                level--;
                base.endElement();
            }
        }
        public override void endDocument() {
            level--;
            base.endDocument();
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((level == 0)) {
                checkCount();
            }
            base.characters(chars, locationId, properties);
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((level == 0)) {
                checkCount();
            }
            base.processingInstruction(target, data, locationId, properties);
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((level == 0)) {
                checkCount();
            }
            base.comment(chars, locationId, properties);
        }
    
    }
}