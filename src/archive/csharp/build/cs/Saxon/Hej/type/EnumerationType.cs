using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public class EnumerationType : Saxon.Hej.type.AtomicType {
        private System.Collections.Generic.ISet<string> values;
        public EnumerationType(System.Collections.Generic.ISet<string> values)  {
            this.values = values;
        }
        public Saxon.Hej.type.ValidationFailure validate(Saxon.Hej.value.AtomicValue primValue, Saxon.Hej.str.UnicodeString lexicalValue, Saxon.Hej.lib.ConversionRules rules) {
            if (((primValue.getPrimitiveType() == Saxon.Hej.type.BuiltInAtomicType.STRING) && values.Contains(primValue.getStringValue()))) {
                return null;
            } else {
                return new Saxon.Hej.type.ValidationFailure(((("The string '"+primValue.getStringValue())+" is not valid for the enumeration type ")+ToString()));
            }
        }
        public bool isOrdered(bool optimistic) {
            return true;
        }
        public bool isAbstract() {
            return false;
        }
        public bool isPrimitiveType() {
            return false;
        }
        public bool isIdType() {
            return false;
        }
        public bool isIdRefType() {
            return false;
        }
        public bool isBuiltInType() {
            return false;
        }
        public Saxon.Hej.om.StructuredQName getTypeName() {
            return null;
        }
        public double getDefaultPriority() {
            return 0;
        }
        public Saxon.Hej.type.StringConverter getStringConverter(Saxon.Hej.lib.ConversionRules rules) {
            return null;
        }
        public System.Collections.Generic.IList<Saxon.Hej.type.PlainType> getPlainMemberTypes() {
            return Saxon.Ejava.util.Collections.singletonList((Saxon.Hej.type.PlainType)(this));
        }
        public bool matches(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            return (((item is Saxon.Hej.value.AtomicValue) && (((Saxon.Hej.value.AtomicValue)(item)).getPrimitiveType() == Saxon.Hej.type.BuiltInAtomicType.STRING)) && values.Contains(item.getStringValue()));
        }
        public Saxon.Hej.type.ItemType getPrimitiveItemType() {
            return Saxon.Hej.type.BuiltInAtomicType.STRING;
        }
        public bool isPlainType() {
            return true;
        }
        public int getPrimitiveType() {
            return Saxon.Hej.om.StandardNames.XS_STRING;
        }
        public Saxon.Hej.type.UType getUType() {
            return Saxon.Hej.type.UType.STRING;
        }
        public Saxon.Hej.type.PlainType getAtomizedItemType() {
            return Saxon.Hej.type.BuiltInAtomicType.STRING;
        }
        public bool isAtomizable(Saxon.Hej.type.TypeHierarchy th) {
            return true;
        }
        public string getBasicAlphaCode() {
            return "E";
        }
        public bool isAtomicType() {
            return true;
        }
        public bool isListType() {
            return false;
        }
        public bool isUnionType() {
            return false;
        }
        public Saxon.Hej.type.SchemaType getBuiltInBaseType() {
            return Saxon.Hej.type.BuiltInAtomicType.STRING;
        }
        public Saxon.Hej.om.AtomicSequence getTypedValue(Saxon.Hej.str.UnicodeString value, Saxon.Hej.om.NamespaceResolver resolver, Saxon.Hej.lib.ConversionRules rules) {
            return new Saxon.Hej.value.StringValue(value);
        }
        public Saxon.Hej.type.ValidationFailure validateContent(Saxon.Hej.str.UnicodeString value, Saxon.Hej.om.NamespaceResolver nsResolver, Saxon.Hej.lib.ConversionRules rules) {
            return null;
        }
        public bool isNamespaceSensitive() {
            return false;
        }
        public int getWhitespaceAction() {
            return 0;
        }
        public Saxon.Hej.str.UnicodeString preprocess(Saxon.Hej.str.UnicodeString input) {
            return input;
        }
        public Saxon.Hej.str.UnicodeString postprocess(Saxon.Hej.str.UnicodeString input) {
            return input;
        }
        public string getName() {
            return null;
        }
        public Saxon.Hej.om.NamespaceUri getTargetNamespace() {
            return null;
        }
        public int getFingerprint() {
            return -1;
        }
        public string getDisplayName() {
            return null;
        }
        public Saxon.Hej.om.StructuredQName getStructuredQName() {
            return null;
        }
        public string getEQName() {
            return null;
        }
        public bool isComplexType() {
            return false;
        }
        public bool isSimpleType() {
            return true;
        }
        public bool isAnonymousType() {
            return true;
        }
        public int getBlock() {
            return 0;
        }
        public Saxon.Hej.type.SchemaType getBaseType() {
            return Saxon.Hej.type.BuiltInAtomicType.STRING;
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
            Saxon.Hej.type.BuiltInAtomicType.STRING.analyzeContentExpression(expression, kind);
        }
        public Saxon.Hej.om.AtomicSequence atomize(Saxon.Hej.om.NodeInfo node) {
            return null;
        }
        public bool isSameType(Saxon.Hej.type.SchemaType other) {
            return ((other is Saxon.Hej.type.EnumerationType) && values.Equals(((Saxon.Hej.type.EnumerationType)(other)).values));
        }
        public string getDescription() {
            return ToString();
        }
        public void checkTypeDerivationIsOK(Saxon.Hej.type.SchemaType @base, int block) {
        }
        public string getSystemId() {
            return null;
        }
        public Saxon.Hej.type.SchemaValidationStatus getValidationStatus() {
            return Saxon.Hej.type.SchemaValidationStatus.VALIDATED;
        }
        public int getRedefinitionLevel() {
            return 0;
        }
        public Saxon.Hej.om.FunctionItem getComponentAsFunction() {
            return /*134*/Saxon.Eej.ee.schema.UserSimpleType.getComponentAsFunction(this);
        }
    
        /* Expanding default method getGenre from interface AtomicType */
        public virtual Saxon.Hej.om.Genre getGenre() {
            return Saxon.Hej.om.Genre.ATOMIC;
        }

        /* Expanding default method getPrimitiveAtomicType from interface AtomicType */
        public virtual Saxon.Hej.type.BuiltInAtomicType getPrimitiveAtomicType() {
            return (Saxon.Hej.type.BuiltInAtomicType)(((/*1430*/Saxon.Hej.type.AtomicType)getPrimitiveItemType()));
        }

        /* Expanding default method explainMismatch from interface ItemType */
        public virtual Saxon.Ejava.util.Optional<string> explainMismatch(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            return Saxon.Ejava.util.Optional<string>.empty();
        }

        /* Expanding default method getNormalizedDefaultPriority from interface ItemType */
        public virtual double getNormalizedDefaultPriority() {
            return (((getDefaultPriority()+1))/2);
        }

        /* Expanding default method getFullAlphaCode from interface ItemType */
        public virtual string getFullAlphaCode() {
            return getBasicAlphaCode();
        }

        /* Expanding default method toExportString from interface ItemType */
        public virtual string toExportString() {
            return ToString();
        }

    }
}