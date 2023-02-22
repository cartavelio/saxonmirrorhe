using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public class BuiltInListType : Saxon.Hej.type.ListType {
        private readonly int fingerprint;
        public static Saxon.Hej.type.BuiltInListType ENTITIES = makeListType(Saxon.Hej.lib.NamespaceConstant.SCHEMA, "ENTITIES");
        public static Saxon.Hej.type.BuiltInListType IDREFS = makeListType(Saxon.Hej.lib.NamespaceConstant.SCHEMA, "IDREFS");
        public static Saxon.Hej.type.BuiltInListType NMTOKENS = makeListType(Saxon.Hej.lib.NamespaceConstant.SCHEMA, "NMTOKENS");
        public static Saxon.Hej.type.BuiltInListType ANY_URIS = makeListType(Saxon.Hej.lib.NamespaceConstant.SCHEMA_INSTANCE, "anonymous_schemaLocationType");
        public bool isBuiltInType() {
            return true;
        }
        public string getSystemId() {
            return null;
        }
        public int getRedefinitionLevel() {
            return 0;
        }
        public int getWhitespaceAction() {
            return Saxon.Hej.value.Whitespace.COLLAPSE;
        }
        private Saxon.Hej.type.BuiltInAtomicType itemType;
        public BuiltInListType(int fingerprint)  {
            this.fingerprint = fingerprint;
            switch (fingerprint) {
              case Saxon.Hej.om.StandardNames.XS_ENTITIES:
                itemType = Saxon.Hej.type.BuiltInAtomicType.ENTITY;
                break ;
              case Saxon.Hej.om.StandardNames.XS_IDREFS:
                itemType = Saxon.Hej.type.BuiltInAtomicType.IDREF;
                break ;
              case Saxon.Hej.om.StandardNames.XS_NMTOKENS:
                itemType = Saxon.Hej.type.BuiltInAtomicType.NMTOKEN;
                break ;
              case Saxon.Hej.om.StandardNames.XSI_SCHEMA_LOCATION_TYPE:
                itemType = Saxon.Hej.type.BuiltInAtomicType.ANY_URI;
                break ;
            }
        }
        public Saxon.Hej.type.SchemaValidationStatus getValidationStatus() {
            return /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED;
        }
        public Saxon.Hej.type.SchemaType getBaseType() {
            return Saxon.Hej.type.AnySimpleType.getInstance();
        }
        public bool isAtomicType() {
            return false;
        }
        public bool isIdType() {
            return false;
        }
        public bool isIdRefType() {
            return (fingerprint == Saxon.Hej.om.StandardNames.XS_IDREFS);
        }
        public bool isListType() {
            return true;
        }
        public bool isUnionType() {
            return false;
        }
        public bool isAnonymousType() {
            return false;
        }
        public Saxon.Hej.type.SchemaType getBuiltInBaseType() {
            return this;
        }
        public bool isNamespaceSensitive() {
            return false;
        }
        public string getName() {
            return Saxon.Hej.om.StandardNames.getLocalName(fingerprint);
        }
        public string getTargetNamespace() {
            return Saxon.Hej.lib.NamespaceConstant.SCHEMA;
        }
        public string getEQName() {
            return ((("Q{"+Saxon.Hej.lib.NamespaceConstant.SCHEMA)+"}")+getName());
        }
        public int getFingerprint() {
            return fingerprint;
        }
        public string getDisplayName() {
            return Saxon.Hej.om.StandardNames.getDisplayName(fingerprint);
        }
        public bool isComplexType() {
            return false;
        }
        public bool isSimpleType() {
            return true;
        }
        public int getBlock() {
            return 0;
        }
        public Saxon.Hej.type.SchemaType getKnownBaseType() {
            return Saxon.Hej.type.AnySimpleType.getInstance();
        }
        public int getDerivationMethod() {
            return Saxon.Hej.type.Derivation.DERIVATION_LIST;
        }
        public bool allowsDerivation(int derivation) {
            return true;
        }
        public int getFinalProhibitions() {
            return 0;
        }
        public Saxon.Hej.om.AtomicSequence atomize(Saxon.Hej.om.NodeInfo node) {
            try {
                return getTypedValue(node.getUnicodeStringValue(), node.getAllNamespaces(), node.getConfiguration().getConversionRules());
            } catch (Saxon.Hej.type.ValidationException err) {
                throw new Saxon.Hej.trans.XPathException(("Internal error: value doesn't match its type annotation. "+err.Message));
            }
        }
        public bool isSameType(Saxon.Hej.type.SchemaType other) {
            return (other.getFingerprint() == getFingerprint());
        }
        public string getDescription() {
            return getDisplayName();
        }
        public void checkTypeDerivationIsOK(Saxon.Hej.type.SchemaType type, int block) {
        }
        public string getLocalName() {
            return Saxon.Hej.om.StandardNames.getLocalName(fingerprint);
        }
        public Saxon.Hej.om.StructuredQName getStructuredQName() {
            return new Saxon.Hej.om.StructuredQName("xs", Saxon.Hej.lib.NamespaceConstant.SCHEMA, getLocalName());
        }
        public Saxon.Hej.type.SimpleType getItemType() {
            return itemType;
        }
        public void analyzeContentExpression(Saxon.Hej.expr.Expression expression, int kind) {
            Saxon.Hej.type.BuiltInAtomicType.analyzeContentExpression(this, expression, kind);
        }
        public Saxon.Hej.type.ValidationFailure validateContent(Saxon.Hej.str.UnicodeString value, Saxon.Hej.om.NamespaceResolver nsResolver, Saxon.Hej.lib.ConversionRules rules) {
            Saxon.Hej.type.SimpleType @base = getItemType();
            Saxon.Hej.value.Whitespace.Tokenizer iter = new Saxon.Hej.value.Whitespace.Tokenizer(value);
            bool found = false;
            Saxon.Hej.value.StringValue val;
            while (((val = ((/*1430*/Saxon.Hej.value.StringValue)iter.next())) != null)) {
                found = true;
                Saxon.Hej.type.ValidationFailure v = @base.validateContent(val.getUnicodeStringValue(), nsResolver, rules);
                if ((v != null)) {
                    return v;
                }
            }
            if (!(found)) {
                return new Saxon.Hej.type.ValidationFailure((("The built-in list type "+Saxon.Hej.om.StandardNames.getDisplayName(fingerprint))+" does not allow a zero-length list"));
            }
            return null;
        }
        public Saxon.Hej.om.AtomicSequence getTypedValue(Saxon.Hej.str.UnicodeString value, Saxon.Hej.om.NamespaceResolver resolver, Saxon.Hej.lib.ConversionRules rules) {
            Saxon.Hej.value.Whitespace.Tokenizer iter = new Saxon.Hej.value.Whitespace.Tokenizer(value);
            Saxon.Hej.type.AtomicType atomicType = (Saxon.Hej.type.AtomicType)(getItemType());
            try {
                return new Saxon.Hej.om.AtomicArray(Saxon.Hej.expr.MappingIterator.map(iter, item => atomicType.getTypedValue(item.getUnicodeStringValue(), resolver, rules).iterate()));
            } catch (Saxon.Hej.trans.XPathException err) {
                throw new Saxon.Hej.type.ValidationException(err);
            }
        }
        private static Saxon.Hej.type.BuiltInListType makeListType(string @namespace, string lname) {
            Saxon.Hej.type.BuiltInListType t = new Saxon.Hej.type.BuiltInListType(Saxon.Hej.om.StandardNames.getFingerprint(@namespace, lname));
            Saxon.Hej.type.BuiltInType.register(t.getFingerprint(), t);
            return t;
        }
        public Saxon.Hej.str.UnicodeString preprocess(Saxon.Hej.str.UnicodeString input) {
            return input;
        }
        public Saxon.Hej.str.UnicodeString postprocess(Saxon.Hej.str.UnicodeString input) {
            return input;
        }
        public Saxon.Hej.om.Function getComponentAsFunction() {
            return /*134*/Saxon.Eej.ee.schema.UserSimpleType.getComponentAsFunction(this);
        }
    
    }
}