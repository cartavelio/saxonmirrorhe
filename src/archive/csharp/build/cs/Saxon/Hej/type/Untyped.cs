using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public class Untyped : Saxon.Hej.type.ComplexType {
        // Private constructor (generated for singleton enum class)
        private Untyped() {}

        // Singleton instance (generated for singleton enum class)
        public static Untyped INSTANCE = new Untyped();

        public static Saxon.Hej.type.Untyped getInstance() {
            return /*implicit enum*/ Saxon.Hej.type.Untyped.INSTANCE;
        }
        public Saxon.Hej.type.SchemaValidationStatus getValidationStatus() {
            return /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED;
        }
        public string getName() {
            return "untyped";
        }
        public string getEQName() {
            return (("Q{"+Saxon.Hej.lib.NamespaceConstant.SCHEMA)+"}untyped");
        }
        public int getRedefinitionLevel() {
            return 0;
        }
        public string getTargetNamespace() {
            return Saxon.Hej.lib.NamespaceConstant.SCHEMA;
        }
        public Saxon.Hej.type.ComplexVariety getVariety() {
            return Saxon.Hej.type.ComplexVariety.MIXED;
        }
        public string getSystemId() {
            return null;
        }
        public int getBlock() {
            return 0;
        }
        public int getDerivationMethod() {
            return 0;
        }
        public bool allowsDerivation(int derivation) {
            return false;
        }
        public int getFinalProhibitions() {
            return 0;
        }
        public void checkTypeDerivationIsOK(Saxon.Hej.type.SchemaType type, int block) {
        }
        public int getFingerprint() {
            return Saxon.Hej.om.StandardNames.XS_UNTYPED;
        }
        public string getDisplayName() {
            return "xs:untyped";
        }
        public Saxon.Hej.om.StructuredQName getStructuredQName() {
            return /*explicit enum*/ Saxon.Hej.type.Untyped.NAME;
        }
        public readonly static Saxon.Hej.om.StructuredQName NAME = new Saxon.Hej.om.StructuredQName("xs", Saxon.Hej.lib.NamespaceConstant.SCHEMA, "untyped");
        public bool isComplexType() {
            return true;
        }
        public bool isAnonymousType() {
            return false;
        }
        public Saxon.Hej.type.SchemaType getKnownBaseType() {
            return Saxon.Hej.type.AnyType.getInstance();
        }
        public bool isSameType(Saxon.Hej.type.SchemaType other) {
            return (other == /*implicit enum*/ Saxon.Hej.type.Untyped.INSTANCE);
        }
        public Saxon.Hej.type.SchemaType getBaseType() {
            return Saxon.Hej.type.AnyType.getInstance();
        }
        public bool isAbstract() {
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
            return true;
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
        public string getDescription() {
            return "xs:untyped";
        }
        public void analyzeContentExpression(Saxon.Hej.expr.Expression expression, int kind) {
        }
        public Saxon.Hej.om.AtomicSequence atomize(Saxon.Hej.om.NodeInfo node) {
            return new Saxon.Hej.value.UntypedAtomicValue(node.getUnicodeStringValue());
        }
        public Saxon.Hej.type.SchemaType getElementParticleType(int elementName, bool considerExtensions) {
            return this;
        }
        public int getElementParticleCardinality(int elementName, bool considerExtensions) {
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
        }
        public Saxon.Hej.type.SimpleType getAttributeUseType(Saxon.Hej.om.StructuredQName attributeName) {
            return Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
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
        public Saxon.Hej.om.Function getComponentAsFunction() {
            return /*134*/Saxon.Eej.ee.schema.UserComplexType.getComponentAsFunction(this);
        }
    }
}