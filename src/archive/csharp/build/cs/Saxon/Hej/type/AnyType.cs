using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public class AnyType : Saxon.Hej.type.ComplexType {
        // Private constructor (generated for singleton enum class)
        private AnyType() {}

        // Singleton instance (generated for singleton enum class)
        public static AnyType INSTANCE = new AnyType();

        public static Saxon.Hej.type.AnyType getInstance() {
            return /*implicit enum*/ Saxon.Hej.type.AnyType.INSTANCE;
        }
        public string getName() {
            return "anyType";
        }
        public string getEQName() {
            return (("Q{"+Saxon.Hej.lib.NamespaceConstant.SCHEMA)+"}anyType");
        }
        public Saxon.Hej.om.NamespaceUri getTargetNamespace() {
            return Saxon.Hej.om.NamespaceUri.SCHEMA;
        }
        public Saxon.Hej.type.ComplexVariety getVariety() {
            return Saxon.Hej.type.ComplexVariety.MIXED;
        }
        public Saxon.Hej.type.SchemaValidationStatus getValidationStatus() {
            return /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED;
        }
        public int getRedefinitionLevel() {
            return 0;
        }
        public Saxon.Hej.type.SchemaType getBaseType() {
            return null;
        }
        public Saxon.Hej.type.SchemaType getKnownBaseType() {
            return null;
        }
        public int getDerivationMethod() {
            return 0;
        }
        public bool allowsDerivation(int derivation) {
            return true;
        }
        public int getFinalProhibitions() {
            return 0;
        }
        public bool isAbstract() {
            return false;
        }
        public bool isComplexType() {
            return true;
        }
        public bool isAnonymousType() {
            return false;
        }
        public bool isSimpleType() {
            return false;
        }
        public bool isAtomicType() {
            return false;
        }
        public bool isIdType() {
            return false;
        }
        public bool isIdRefType() {
            return false;
        }
        public int getBlock() {
            return 0;
        }
        public bool isComplexContent() {
            return true;
        }
        public bool isSimpleContent() {
            return false;
        }
        public bool isAllContent() {
            return false;
        }
        public Saxon.Hej.type.SimpleType getSimpleContentType() {
            return null;
        }
        public bool isRestricted() {
            return false;
        }
        public bool isEmptyContent() {
            return false;
        }
        public bool isEmptiable() {
            return true;
        }
        public bool isMixedContent() {
            return true;
        }
        public int getFingerprint() {
            return Saxon.Hej.om.StandardNames.XS_ANY_TYPE;
        }
        public Saxon.Hej.om.StructuredQName getStructuredQName() {
            return /*explicit enum*/ Saxon.Hej.type.AnyType.QNAME;
        }
        public static readonly Saxon.Hej.om.StructuredQName QNAME = new Saxon.Hej.om.StructuredQName("xs", Saxon.Hej.om.NamespaceUri.SCHEMA, "anyType");
        public string getDescription() {
            return "xs:anyType";
        }
        public string getDisplayName() {
            return "xs:anyType";
        }
        public string getSystemId() {
            return null;
        }
        public bool isSameType(Saxon.Hej.type.SchemaType other) {
            return (other is Saxon.Hej.type.AnyType);
        }
        public void analyzeContentExpression(Saxon.Hej.expr.Expression expression, int kind) {
        }
        public Saxon.Hej.om.AtomicSequence atomize(Saxon.Hej.om.NodeInfo node) {
            return Saxon.Hej.value.StringValue.makeUntypedAtomic(node.getUnicodeStringValue());
        }
        public void checkTypeDerivationIsOK(Saxon.Hej.type.SchemaType type, int block) {
            if (!(((type is Saxon.Hej.type.AnyType)))) {
                throw new Saxon.Hej.type.SchemaException("Cannot derive xs:anyType from another type");
            }
        }
        public Saxon.Hej.type.SchemaType getElementParticleType(int elementName, bool considerExtensions) {
            return this;
        }
        public int getElementParticleCardinality(int elementName, bool considerExtensions) {
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
        }
        public Saxon.Hej.type.SimpleType getAttributeUseType(Saxon.Hej.om.StructuredQName attributeName) {
            return Saxon.Hej.type.AnySimpleType.getInstance();
        }
        public int getAttributeUseCardinality(Saxon.Hej.om.StructuredQName attributeName) {
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
        }
        public bool allowsAttributes() {
            return true;
        }
        public void gatherAllPermittedChildren(Saxon.Hej.z.IntHashSet children, bool ignoreWildcards) {
            children.add(-1);
        }
        public void gatherAllPermittedDescendants(Saxon.Hej.z.IntHashSet descendants) {
            descendants.add(-1);
        }
        public Saxon.Hej.type.SchemaType getDescendantElementType(int fingerprint) {
            return this;
        }
        public int getDescendantElementCardinality(int elementFingerprint) {
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
        }
        public bool containsElementWildcard() {
            return true;
        }
        public bool hasAssertions() {
            return false;
        }
        public Saxon.Hej.om.FunctionItem getComponentAsFunction() {
            return /*134*/Saxon.Eej.ee.schema.UserComplexType.getComponentAsFunction(this);
        }
    }
}