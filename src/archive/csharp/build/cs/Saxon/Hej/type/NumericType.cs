using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public class NumericType : Saxon.Hej.type.LocalUnionType, Saxon.Hej.type.SimpleType {
        private static Saxon.Hej.type.NumericType THE_INSTANCE;
        public static Saxon.Hej.type.NumericType getInstance() {
            lock (typeof(Saxon.Hej.type.NumericType)) {
                if ((THE_INSTANCE == null)) {
                    THE_INSTANCE = new Saxon.Hej.type.NumericType();
                    Saxon.Hej.type.BuiltInType.register(Saxon.Hej.om.StandardNames.XS_NUMERIC, THE_INSTANCE);
                }
                return THE_INSTANCE;
            }
        }
        /* C# Replacement Method */
        private NumericType() : base (Saxon.Ejava.util.Arrays.asList<Saxon.Hej.type.AtomicType>(Saxon.Hej.type.BuiltInAtomicType.DOUBLE, Saxon.Hej.type.BuiltInAtomicType.FLOAT, Saxon.Hej.type.BuiltInAtomicType.DECIMAL)) {}
        /* End of C# Replacement Method */
        public override Saxon.Hej.om.StructuredQName getTypeName() {
            return new Saxon.Hej.om.StructuredQName("xs", Saxon.Hej.lib.NamespaceConstant.SCHEMA, "numeric");
        }
        public override Saxon.Hej.om.Genre getGenre() {
            return Saxon.Hej.om.Genre.ATOMIC;
        }
        public override string getBasicAlphaCode() {
            return "A";
        }
        public override bool containsListType() {
            return false;
        }
        public override System.Collections.Generic.IList<Saxon.Hej.type.PlainType> getPlainMemberTypes() {
            return new System.Collections.Generic.List<Saxon.Hej.type.PlainType>(getMemberTypes());
        }
        public static bool isNumericType(Saxon.Hej.type.ItemType type) {
            return (type.isPlainType() && Saxon.Hej.type.UType.NUMERIC.subsumes(type.getUType()));
        }
        public override Saxon.Hej.value.SequenceType getResultTypeOfCast() {
            return Saxon.Hej.value.SequenceType.ATOMIC_SEQUENCE;
        }
        public override bool isPlainType() {
            return true;
        }
        public override double getDefaultPriority() {
            return 0.125;
        }
        public override bool matches(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            return (item is Saxon.Hej.value.NumericValue);
        }
        public override Saxon.Hej.type.AtomicType getPrimitiveItemType() {
            return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
        }
        public override int getPrimitiveType() {
            return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC.getFingerprint();
        }
        public override Saxon.Hej.type.UType getUType() {
            return Saxon.Hej.type.UType.NUMERIC;
        }
        public override Saxon.Hej.type.PlainType getAtomizedItemType() {
            return this;
        }
        public override bool isAtomizable(Saxon.Hej.type.TypeHierarchy th) {
            return true;
        }
        public override bool isAtomicType() {
            return false;
        }
        public override bool isListType() {
            return false;
        }
        public override bool isUnionType() {
            return true;
        }
        public override bool isBuiltInType() {
            return true;
        }
        public Saxon.Hej.type.SchemaType getBuiltInBaseType() {
            return Saxon.Hej.type.AnySimpleType.getInstance();
        }
        public override Saxon.Hej.value.DoubleValue getTypedValue(Saxon.Hej.str.UnicodeString value, Saxon.Hej.om.NamespaceResolver resolver, Saxon.Hej.lib.ConversionRules rules) {
            try {
                double d = Saxon.Hej.type.StringToDouble.getInstance().stringToNumber(value);
                return new Saxon.Hej.value.DoubleValue(d);
            } catch (Saxon.Ejava.lang.NumberFormatException e) {
                string message = (("Cannot convert string \""+value)+"\" to xs:numeric");
                throw new Saxon.Hej.type.ValidationFailure(message).makeException();
            }
        }
        public override Saxon.Hej.type.ValidationFailure validateContent(Saxon.Hej.str.UnicodeString value, Saxon.Hej.om.NamespaceResolver nsResolver, Saxon.Hej.lib.ConversionRules rules) {
            try {
                Saxon.Hej.type.StringToDouble.getInstance().stringToNumber(value);
                return null;
            } catch (Saxon.Ejava.lang.NumberFormatException e) {
                return new Saxon.Hej.type.ValidationFailure(e.Message);
            }
        }
        public override Saxon.Hej.type.ValidationFailure checkAgainstFacets(Saxon.Hej.value.AtomicValue value, Saxon.Hej.lib.ConversionRules rules) {
            return null;
        }
        public override bool isNamespaceSensitive() {
            return false;
        }
        public int getWhitespaceAction() {
            return Saxon.Hej.value.Whitespace.COLLAPSE;
        }
        public Saxon.Hej.str.UnicodeString preprocess(Saxon.Hej.str.UnicodeString input) {
            return input;
        }
        public Saxon.Hej.str.UnicodeString postprocess(Saxon.Hej.str.UnicodeString input) {
            return input;
        }
        public string getName() {
            return "numeric";
        }
        public string getTargetNamespace() {
            return Saxon.Hej.lib.NamespaceConstant.SCHEMA;
        }
        public int getFingerprint() {
            return Saxon.Hej.om.StandardNames.XS_NUMERIC;
        }
        public string getDisplayName() {
            return "xs:numeric";
        }
        public string getEQName() {
            return (("Q("+Saxon.Hej.lib.NamespaceConstant.SCHEMA)+"}numeric");
        }
        public override  /* REPLACING public */ Saxon.Hej.om.StructuredQName getStructuredQName() {
            return new Saxon.Hej.om.StructuredQName("xs", Saxon.Hej.lib.NamespaceConstant.SCHEMA, "numeric");
        }
        public bool isComplexType() {
            return false;
        }
        public bool isSimpleType() {
            return true;
        }
        public bool isAnonymousType() {
            return false;
        }
        public int getBlock() {
            return 0;
        }
        public Saxon.Hej.type.SchemaType getBaseType() {
            return Saxon.Hej.type.AnySimpleType.getInstance();
        }
        public int getDerivationMethod() {
            return Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION;
        }
        public int getFinalProhibitions() {
            return 0;
        }
        public bool allowsDerivation(int derivation) {
            return true;
        }
        public void analyzeContentExpression(Saxon.Hej.expr.Expression expression, int kind) {
            Saxon.Hej.type.BuiltInAtomicType.analyzeContentExpression(this, expression, kind);
        }
        public Saxon.Hej.om.AtomicSequence atomize(Saxon.Hej.om.NodeInfo node) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
        public bool isSameType(Saxon.Hej.type.SchemaType other) {
            return (other is Saxon.Hej.type.NumericType);
        }
        public override  /* REPLACING public */ string getDescription() {
            return "xs:numeric";
        }
        public string getSystemId() {
            return null;
        }
        public override bool isIdType() {
            return false;
        }
        public override bool isIdRefType() {
            return false;
        }
        public Saxon.Hej.type.SchemaValidationStatus getValidationStatus() {
            return /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED;
        }
        public int getRedefinitionLevel() {
            return 0;
        }
        public override string toExportString() {
            return ToString();
        }
        public override string ToString() {
            return "xs:numeric";
        }
        public void checkTypeDerivationIsOK(Saxon.Hej.type.SchemaType @base, int block) {
            string s = /*134*/Saxon.Eej.ee.schema.UserSimpleType.isTypeDerivationOK(this, @base, block);
            if ((s != null)) {
                throw new Saxon.Hej.type.SchemaException(s);
            }
        }
        public Saxon.Hej.om.Function getComponentAsFunction() {
            return /*134*/Saxon.Eej.ee.schema.UserSimpleType.getComponentAsFunction(this);
        }
    
    }
}