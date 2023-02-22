using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class TypeCheckingFilter : Saxon.Hej.@event.ProxyOutputter {
        private Saxon.Hej.type.ItemType itemType;
        private int cardinality;
        private Saxon.Hej.expr.parser.RoleDiagnostic roleDiagnostic;
        private Saxon.Hej.s9api.Location locator;
        private int count = 0;
        private int level = 0;
        private readonly System.Collections.Generic.HashSet<System.Int64> checkedElements = new System.Collections.Generic.HashSet<System.Int64>(10);
        private readonly Saxon.Hej.type.TypeHierarchy typeHierarchy;
        public TypeCheckingFilter(Saxon.Hej.@event.Outputter next) : base (next) {
            typeHierarchy = getConfiguration().getTypeHierarchy();
        }
        public void setRequiredType(Saxon.Hej.type.ItemType type, int cardinality, Saxon.Hej.expr.parser.RoleDiagnostic roleDiagnostic, Saxon.Hej.s9api.Location locator) {
            itemType = type;
            this.cardinality = cardinality;
            this.roleDiagnostic = roleDiagnostic;
            this.locator = locator;
        }
        public override void @namespace(string prefix, Saxon.Hej.om.NamespaceUri namespaceUri, int properties) {
            if ((level == 0)) {
                if ((++count == 2)) {
                    checkAllowsMany(Saxon.Hej.expr.parser.Loc.NONE);
                }
                checkItemType(Saxon.Hej.pattern.NodeKindTest.NAMESPACE, Saxon.Hej.expr.parser.Loc.NONE);
            }
            getNextOutputter().@namespace(prefix, namespaceUri, properties);
        }
        public override void attribute(Saxon.Hej.om.NodeName attName, Saxon.Hej.type.SimpleType typeCode, string value, Saxon.Hej.s9api.Location location, int properties) {
            if ((level == 0)) {
                if ((++count == 2)) {
                    checkAllowsMany(location);
                }
                Saxon.Hej.type.ItemType type = new Saxon.Hej.pattern.CombinedNodeTest(new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ATTRIBUTE, attName, getConfiguration().getNamePool()), Saxon.Hej.expr.parser.Token.INTERSECT, new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ATTRIBUTE, typeCode, getConfiguration(), false));
                checkItemType(type, nodeSupplier(Saxon.Hej.type.Type.ATTRIBUTE, attName, typeCode, Saxon.Hej.str.StringView.tidy(value)), location);
            }
            getNextOutputter().attribute(attName, typeCode, value, location, properties);
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((level == 0)) {
                if ((++count == 2)) {
                    checkAllowsMany(locationId);
                }
                checkItemType(Saxon.Hej.pattern.NodeKindTest.TEXT, nodeSupplier(Saxon.Hej.type.Type.TEXT, null, null, chars.tidy()), locationId);
            }
            getNextOutputter().characters(chars, locationId, properties);
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((level == 0)) {
                if ((++count == 2)) {
                    checkAllowsMany(locationId);
                }
                checkItemType(Saxon.Hej.pattern.NodeKindTest.COMMENT, nodeSupplier(Saxon.Hej.type.Type.COMMENT, null, null, chars.tidy()), locationId);
            }
            getNextOutputter().comment(chars, locationId, properties);
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((level == 0)) {
                if ((++count == 2)) {
                    checkAllowsMany(locationId);
                }
                checkItemType(Saxon.Hej.pattern.NodeKindTest.PROCESSING_INSTRUCTION, nodeSupplier(Saxon.Hej.type.Type.PROCESSING_INSTRUCTION, new Saxon.Hej.om.NoNamespaceName(target), null, data.tidy()), locationId);
            }
            getNextOutputter().processingInstruction(target, data, locationId, properties);
        }
        public override void startDocument(int properties) {
            if ((level == 0)) {
                if ((++count == 2)) {
                    checkAllowsMany(Saxon.Hej.expr.parser.Loc.NONE);
                }
                checkItemType(Saxon.Hej.pattern.NodeKindTest.DOCUMENT, nodeSupplier(Saxon.Hej.type.Type.DOCUMENT, null, null, Saxon.Hej.str.EmptyUnicodeString.getInstance()), Saxon.Hej.expr.parser.Loc.NONE);
            }
            level++;
            getNextOutputter().startDocument(properties);
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType elemType, Saxon.Hej.s9api.Location location, int properties) {
            checkElementStart(elemName, elemType, location);
            getNextOutputter().startElement(elemName, elemType, location, properties);
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            checkElementStart(elemName, type, location);
            getNextOutputter().startElement(elemName, type, attributes, namespaces, location, properties);
        }
        private void checkElementStart(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType elemType, Saxon.Hej.s9api.Location location) {
            Saxon.Hej.Configuration config = getConfiguration();
            Saxon.Hej.om.NamePool namePool = config.getNamePool();
            if ((level == 0)) {
                if ((++count == 1)) {
                    Saxon.Hej.type.ItemType type = new Saxon.Hej.pattern.CombinedNodeTest(new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, elemName, namePool), Saxon.Hej.expr.parser.Token.INTERSECT, new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ELEMENT, elemType, config, false));
                    checkItemType(type, nodeSupplier(Saxon.Hej.type.Type.ELEMENT, elemName, elemType, Saxon.Hej.str.EmptyUnicodeString.getInstance()), location);
                } else {
                    if ((count == 2)) {
                        checkAllowsMany(location);
                    }
                    long key = (((long)(elemName.obtainFingerprint(namePool))<<32)|(long)(elemType.getFingerprint()));
                    if (!(checkedElements.Contains(key))) {
                        Saxon.Hej.type.ItemType type = new Saxon.Hej.pattern.CombinedNodeTest(new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, elemName, namePool), Saxon.Hej.expr.parser.Token.INTERSECT, new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ELEMENT, elemType, config, false));
                        checkItemType(type, nodeSupplier(Saxon.Hej.type.Type.ELEMENT, elemName, elemType, Saxon.Hej.str.EmptyUnicodeString.getInstance()), location);
                        checkedElements.Add(key);
                    }
                }
            }
            level++;
        }
        public override void endDocument() {
            level--;
            getNextOutputter().endDocument();
        }
        public override void endElement() {
            level--;
            getNextOutputter().endElement();
        }
        public override void close() {
            finalCheck();
            base.close();
        }
        public void finalCheck() {
            if (((count == 0) && !(Saxon.Hej.value.Cardinality.allowsZero(cardinality)))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("An empty sequence is not allowed as the "+roleDiagnostic.getMessage()));
                string errorCode = roleDiagnostic.getErrorCode();
                err.setErrorCode(errorCode);
                if (!("XPDY0050".Equals(errorCode))) {
                    err.setIsTypeError(true);
                }
                throw err;
            }
        }
        private Saxon.Ejava.util.function.Supplier<Saxon.Hej.om.NodeInfo> nodeSupplier(short nodeKind, Saxon.Hej.om.NodeName name, Saxon.Hej.type.SchemaType type, Saxon.Hej.str.UnicodeString value) {
            return () => {
                Saxon.Hej.tree.util.Orphan o = new Saxon.Hej.tree.util.Orphan(getPipelineConfiguration().getConfiguration());
                o.setNodeKind(nodeKind);
                if ((name != null)) {
                    o.setNodeName(name);
                }
                o.setTypeAnnotation(type);
                o.setStringValue(value);
                return o;
            } ;
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int copyNamespaces) {
            if ((level == 0)) {
                if ((++count == 2)) {
                    checkAllowsMany(locationId);
                }
                checkItem(item, locationId);
            }
            getNextOutputter().append(item, locationId, copyNamespaces);
        }
        public override void append(Saxon.Hej.om.Item item) {
            if ((level == 0)) {
                if ((++count == 2)) {
                    checkAllowsMany(Saxon.Hej.expr.parser.Loc.NONE);
                }
                checkItem(item, Saxon.Hej.expr.parser.Loc.NONE);
            }
            getNextOutputter().append(item);
        }
        public override bool usesTypeAnnotations() {
            return true;
        }
        /* C# Replacement Method */
            private void checkItemType<T>(Saxon.Hej.type.ItemType type, Saxon.Ejava.util.function.Supplier<T> itemSupplier, Saxon.Hej.s9api.Location locationId) where T : Saxon.Hej.om.Item {
                if (!(typeHierarchy.isSubType(type, itemType))) {
                    throwTypeError(type, itemSupplier(), locationId);
                }
            }
        /* End of C# Replacement Method */
        private void checkItemType(Saxon.Hej.type.ItemType type, Saxon.Hej.s9api.Location locationId) {
            if (!(typeHierarchy.isSubType(type, itemType))) {
                throwTypeError(type, null, locationId);
            }
        }
        private void checkItem(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId) {
            if (!(itemType.matches(item, typeHierarchy))) {
                throwTypeError(null, item, locationId);
            }
        }
        private void throwTypeError(Saxon.Hej.type.ItemType suppliedType, Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId) {
            string message;
            if ((item == null)) {
                message = roleDiagnostic.composeErrorMessage(itemType, suppliedType);
            } else {
                message = roleDiagnostic.composeErrorMessage(itemType, item, typeHierarchy);
            }
            string errorCode = roleDiagnostic.getErrorCode();
            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(message);
            err.setErrorCode(errorCode);
            if (!("XPDY0050".Equals(errorCode))) {
                err.setIsTypeError(true);
            }
            if ((locationId == null)) {
                err.setLocation(locator);
            } else {
                err.setLocation(locationId.saveLocation());
            }
            throw err;
        }
        private void checkAllowsMany(Saxon.Hej.s9api.Location locationId) {
            if (!(Saxon.Hej.value.Cardinality.allowsMany(cardinality))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("A sequence of more than one item is not allowed as the "+roleDiagnostic.getMessage()));
                string errorCode = roleDiagnostic.getErrorCode();
                err.setErrorCode(errorCode);
                if (!("XPDY0050".Equals(errorCode))) {
                    err.setIsTypeError(true);
                }
                if (((locationId == null) || (locationId == Saxon.Hej.expr.parser.Loc.NONE))) {
                    err.setLocator(locator);
                } else {
                    err.setLocator(locationId);
                }
                throw err;
            }
        }
    
    }
}