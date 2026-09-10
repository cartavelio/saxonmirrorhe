using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public sealed class ErrorType : Saxon.Hej.pattern.NodeTest, Saxon.Hej.type.AtomicType, Saxon.Hej.type.UnionType, Saxon.Hej.type.PlainType {
        private static readonly Saxon.Hej.type.ErrorType theInstance = new Saxon.Hej.type.ErrorType();
        private ErrorType()  {
        }
        public override Saxon.Hej.om.Genre getGenre() {
            return Saxon.Hej.om.Genre.ANY;
        }
        public override Saxon.Hej.type.UType getUType() {
            return Saxon.Hej.type.UType.VOID;
        }
        public string getName() {
            return "error";
        }
        public Saxon.Hej.om.NamespaceUri getTargetNamespace() {
            return Saxon.Hej.om.NamespaceUri.SCHEMA;
        }
        public string getEQName() {
            return (("Q{"+Saxon.Hej.lib.NamespaceConstant.SCHEMA)+"}error");
        }
        public bool containsListType() {
            return false;
        }
        public System.Collections.Generic.IList<Saxon.Hej.type.PlainType> getPlainMemberTypes() {
            return new System.Collections.Generic.List<Saxon.Hej.type.PlainType>();
        }
        public bool isBuiltInType() {
            return true;
        }
        public int getRedefinitionLevel() {
            return 0;
        }
        public string getSystemId() {
            return null;
        }
        public static Saxon.Hej.type.ErrorType getInstance() {
            return theInstance;
        }
        public Saxon.Hej.type.SchemaValidationStatus getValidationStatus() {
            return /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED;
        }
        public Saxon.Hej.type.SchemaType getBaseType() {
            return Saxon.Hej.type.AnySimpleType.getInstance();
        }
        public Saxon.Hej.type.SchemaType getKnownBaseType() {
            return getBaseType();
        }
        public bool isComplexType() {
            return false;
        }
        public bool isSimpleType() {
            return true;
        }
        public override int getFingerprint() {
            return Saxon.Hej.om.StandardNames.XS_ERROR;
        }
        public override Saxon.Hej.om.StructuredQName getMatchingNodeName() {
            return Saxon.Hej.om.StandardNames.getStructuredQName(Saxon.Hej.om.StandardNames.XS_ERROR);
        }
        public Saxon.Hej.om.StructuredQName getTypeName() {
            return new Saxon.Hej.om.StructuredQName("xs", Saxon.Hej.om.NamespaceUri.SCHEMA, "error");
        }
        public string getDescription() {
            return "xs:error";
        }
        public string getDisplayName() {
            return "xs:error";
        }
        public bool isSameType(Saxon.Hej.type.SchemaType other) {
            return (other is Saxon.Hej.type.ErrorType);
        }
        public Saxon.Hej.om.AtomicSequence atomize(Saxon.Hej.om.NodeInfo node) {
            return Saxon.Hej.value.StringValue.makeUntypedAtomic(node.getUnicodeStringValue());
        }
        public void checkTypeDerivationIsOK(Saxon.Hej.type.SchemaType type, int block) {
            if (((type == this) || (type == Saxon.Hej.type.AnySimpleType.getInstance()))) {
                return ;
            }
            throw new Saxon.Hej.type.SchemaException(("Type xs:error is not validly derived from "+type.getDescription()));
        }
        public override bool isAtomicType() {
            return false;
        }
        public bool isIdType() {
            return false;
        }
        public bool isIdRefType() {
            return false;
        }
        public bool isAnonymousType() {
            return false;
        }
        public bool isListType() {
            return false;
        }
        public bool isUnionType() {
            return true;
        }
        public Saxon.Hej.type.SchemaType getBuiltInBaseType() {
            return this;
        }
        public Saxon.Hej.om.AtomicSequence getTypedValue(Saxon.Hej.str.UnicodeString value, Saxon.Hej.om.NamespaceResolver resolver, Saxon.Hej.lib.ConversionRules rules) {
            throw new Saxon.Hej.type.ValidationFailure("Cast to xs:error always fails").makeException();
        }
        public Saxon.Hej.type.StringConverter getStringConverter(Saxon.Hej.lib.ConversionRules rules) {
            return null;
        }
        public Saxon.Hej.type.ValidationFailure validateContent(Saxon.Hej.str.UnicodeString value, Saxon.Hej.om.NamespaceResolver nsResolver, Saxon.Hej.lib.ConversionRules rules) {
            return new Saxon.Hej.type.ValidationFailure("No content is ever valid against the type xs:error");
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
            return false;
        }
        public int getFinalProhibitions() {
            return (((Saxon.Hej.type.Derivation.DERIVATION_EXTENSION|Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION)|Saxon.Hej.type.Derivation.DERIVATION_LIST)|Saxon.Hej.type.Derivation.DERIVATION_UNION);
        }
        public int getWhitespaceAction() {
            return Saxon.Hej.value.Whitespace.COLLAPSE;
        }
        public void analyzeContentExpression(Saxon.Hej.expr.Expression expression, int kind) {
            throw new Saxon.Hej.trans.XPathException("No expression can ever return a value of type xs:error");
        }
        public Saxon.Hej.str.UnicodeString preprocess(Saxon.Hej.str.UnicodeString input) {
            return input;
        }
        public Saxon.Hej.str.UnicodeString postprocess(Saxon.Hej.str.UnicodeString input) {
            return input;
        }
        public override bool isPlainType() {
            return true;
        }
        public override bool matches(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            return false;
        }
        public override bool matches(int nodeKind, Saxon.Hej.om.NodeName name, Saxon.Hej.type.SchemaType annotation) {
            return false;
        }
        public override Saxon.Hej.type.AtomicType getPrimitiveItemType() {
            return this;
        }
        public override int getPrimitiveType() {
            return Saxon.Hej.type.Type.ITEM;
        }
        public override double getDefaultPriority() {
            return -1000;
        }
        public override Saxon.Hej.type.AtomicType getAtomizedItemType() {
            return Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
        }
        public override bool isAtomizable(Saxon.Hej.type.TypeHierarchy th) {
            return false;
        }
        public Saxon.Hej.value.SequenceType getResultTypeOfCast() {
            return Saxon.Hej.value.SequenceType.OPTIONAL_ITEM;
        }
        public override string toExportString() {
            return ToString();
        }
        public override string ToString() {
            return "xs:error";
        }
        public Saxon.Hej.type.ValidationFailure validate(Saxon.Hej.value.AtomicValue primValue, Saxon.Hej.str.UnicodeString lexicalValue, Saxon.Hej.lib.ConversionRules rules) {
            return new Saxon.Hej.type.ValidationFailure("No value is valid against type xs:error");
        }
        public bool isOrdered(bool optimistic) {
            return false;
        }
        public bool isAbstract() {
            return true;
        }
        public bool isPrimitiveType() {
            return false;
        }
        public Saxon.Hej.om.StructuredQName getStructuredQName() {
            return new Saxon.Hej.om.StructuredQName("xs", Saxon.Hej.om.NamespaceUri.SCHEMA, "error");
        }
        public Saxon.Hej.type.ValidationFailure checkAgainstFacets(Saxon.Hej.value.AtomicValue value, Saxon.Hej.lib.ConversionRules rules) {
            return null;
        }
        public override Saxon.Ejava.util.Optional<string> explainMismatch(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            return Saxon.Ejava.util.Optional<string>.of("Evaluation of the supplied expression will always fail");
        }
        public Saxon.Hej.om.FunctionItem getComponentAsFunction() {
            return /*134*/Saxon.Eej.ee.schema.UserSimpleType.getComponentAsFunction(this);
        }
    
        /* Expanding default method getPrimitiveAtomicType from interface AtomicType */
        public Saxon.Hej.type.BuiltInAtomicType getPrimitiveAtomicType() {
            return (Saxon.Hej.type.BuiltInAtomicType)(((/*1430*/Saxon.Hej.type.AtomicType)getPrimitiveItemType()));
        }

    }
}