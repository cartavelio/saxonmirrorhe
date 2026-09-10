using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public class AnySimpleType : Saxon.Hej.type.SimpleType {
        // Private constructor (generated for singleton enum class)
        private AnySimpleType() {}

        // Singleton instance (generated for singleton enum class)
        public static AnySimpleType INSTANCE = new AnySimpleType();

        public string getName() {
            return "anySimpleType";
        }
        public Saxon.Hej.om.NamespaceUri getTargetNamespace() {
            return Saxon.Hej.om.NamespaceUri.SCHEMA;
        }
        public string getEQName() {
            return (("Q{"+Saxon.Hej.lib.NamespaceConstant.SCHEMA)+"}anySimpleType");
        }
        public bool isBuiltInType() {
            return true;
        }
        public bool isIdType() {
            return false;
        }
        public bool isIdRefType() {
            return false;
        }
        public int getRedefinitionLevel() {
            return 0;
        }
        public string getSystemId() {
            return null;
        }
        public static Saxon.Hej.type.AnySimpleType getInstance() {
            return /*implicit enum*/ Saxon.Hej.type.AnySimpleType.INSTANCE;
        }
        public Saxon.Hej.type.SchemaValidationStatus getValidationStatus() {
            return /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED;
        }
        public Saxon.Hej.type.SchemaType getBaseType() {
            return Saxon.Hej.type.AnyType.getInstance();
        }
        public bool isComplexType() {
            return false;
        }
        public bool isSimpleType() {
            return true;
        }
        public int getFingerprint() {
            return Saxon.Hej.om.StandardNames.XS_ANY_SIMPLE_TYPE;
        }
        public Saxon.Hej.om.StructuredQName getStructuredQName() {
            return /*explicit enum*/ Saxon.Hej.type.AnySimpleType.NAME;
        }
        public readonly static Saxon.Hej.om.StructuredQName NAME = new Saxon.Hej.om.StructuredQName("xs", Saxon.Hej.om.NamespaceUri.SCHEMA, "anySimpleType");
        public string getDescription() {
            return "xs:anySimpleType";
        }
        public string getDisplayName() {
            return "xs:anySimpleType";
        }
        public bool isSameType(Saxon.Hej.type.SchemaType other) {
            return (other == /*implicit enum*/ Saxon.Hej.type.AnySimpleType.INSTANCE);
        }
        public Saxon.Hej.om.AtomicSequence atomize(Saxon.Hej.om.NodeInfo node) {
            return Saxon.Hej.value.StringValue.makeUntypedAtomic(node.getUnicodeStringValue());
        }
        public void checkTypeDerivationIsOK(Saxon.Hej.type.SchemaType type, int block) {
            if ((type == this)) {
                return ;
            }
            throw new Saxon.Hej.type.SchemaException("Cannot derive xs:anySimpleType from another type");
        }
        public bool isAtomicType() {
            return false;
        }
        public bool isAnonymousType() {
            return false;
        }
        public bool isListType() {
            return false;
        }
        public bool isUnionType() {
            return false;
        }
        public Saxon.Hej.type.SchemaType getBuiltInBaseType() {
            return this;
        }
        public Saxon.Hej.om.AtomicSequence getTypedValue(Saxon.Hej.str.UnicodeString value, Saxon.Hej.om.NamespaceResolver resolver, Saxon.Hej.lib.ConversionRules rules) {
            return Saxon.Hej.value.StringValue.makeUntypedAtomic(value);
        }
        public Saxon.Hej.type.ValidationFailure validateContent(Saxon.Hej.str.UnicodeString value, Saxon.Hej.om.NamespaceResolver nsResolver, Saxon.Hej.lib.ConversionRules rules) {
            return null;
        }
        public bool isNamespaceSensitive() {
            return false;
        }
        public int getBlock() {
            return 0;
        }
        public int getDerivationMethod() {
            return Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION;
        }
        public bool allowsDerivation(int derivation) {
            return true;
        }
        public int getFinalProhibitions() {
            return 0;
        }
        public int getWhitespaceAction() {
            return Saxon.Hej.value.Whitespace.PRESERVE;
        }
        public void analyzeContentExpression(Saxon.Hej.expr.Expression expression, int kind) {
        }
        public Saxon.Hej.str.UnicodeString preprocess(Saxon.Hej.str.UnicodeString input) {
            return input;
        }
        public Saxon.Hej.str.UnicodeString postprocess(Saxon.Hej.str.UnicodeString input) {
            return input;
        }
        public Saxon.Hej.om.FunctionItem getComponentAsFunction() {
            return /*134*/Saxon.Eej.ee.schema.UserSimpleType.getComponentAsFunction(this);
        }
    }
}