using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public class BuiltInAtomicType : Saxon.Hej.type.AtomicType, Saxon.Hej.type.ItemTypeWithSequenceTypeCache {

        static BuiltInAtomicType() {
            ANY_ATOMIC.stringConverter = Saxon.Hej.type.StringConverter.StringToString.INSTANCE;
            STRING.stringConverter = Saxon.Hej.type.StringConverter.StringToString.INSTANCE;
            LANGUAGE.stringConverter = Saxon.Hej.type.StringConverter.StringToLanguage.INSTANCE;
            NORMALIZED_STRING.stringConverter = Saxon.Hej.type.StringConverter.StringToNormalizedString.INSTANCE;
            TOKEN.stringConverter = Saxon.Hej.type.StringConverter.StringToToken.INSTANCE;
            NCNAME.stringConverter = Saxon.Hej.type.StringConverter.StringToNCName.TO_NCNAME;
            NAME.stringConverter = Saxon.Hej.type.StringConverter.StringToName.INSTANCE;
            NMTOKEN.stringConverter = Saxon.Hej.type.StringConverter.StringToNMTOKEN.INSTANCE;
            ID.stringConverter = Saxon.Hej.type.StringConverter.StringToNCName.TO_ID;
            IDREF.stringConverter = Saxon.Hej.type.StringConverter.StringToNCName.TO_IDREF;
            ENTITY.stringConverter = Saxon.Hej.type.StringConverter.StringToNCName.TO_ENTITY;
            DECIMAL.stringConverter = Saxon.Hej.type.StringConverter.StringToDecimal.INSTANCE;
            INTEGER.stringConverter = Saxon.Hej.type.StringConverter.StringToInteger.INSTANCE;
            DURATION.stringConverter = Saxon.Hej.type.StringConverter.StringToDuration.INSTANCE;
            G_MONTH.stringConverter = Saxon.Hej.type.StringConverter.StringToGMonth.INSTANCE;
            G_MONTH_DAY.stringConverter = Saxon.Hej.type.StringConverter.StringToGMonthDay.INSTANCE;
            G_DAY.stringConverter = Saxon.Hej.type.StringConverter.StringToGDay.INSTANCE;
            DAY_TIME_DURATION.stringConverter = Saxon.Hej.type.StringConverter.StringToDayTimeDuration.INSTANCE;
            YEAR_MONTH_DURATION.stringConverter = Saxon.Hej.type.StringConverter.StringToYearMonthDuration.INSTANCE;
            TIME.stringConverter = Saxon.Hej.type.StringConverter.StringToTime.INSTANCE;
            BOOLEAN.stringConverter = Saxon.Hej.type.StringConverter.StringToBoolean.INSTANCE;
            HEX_BINARY.stringConverter = Saxon.Hej.type.StringConverter.StringToHexBinary.INSTANCE;
            BASE64_BINARY.stringConverter = Saxon.Hej.type.StringConverter.StringToBase64Binary.INSTANCE;
            UNTYPED_ATOMIC.stringConverter = Saxon.Hej.type.StringConverter.StringToUntypedAtomic.INSTANCE;
            NON_POSITIVE_INTEGER.stringConverter = new Saxon.Hej.type.StringConverter.StringToIntegerSubtype(NON_POSITIVE_INTEGER);
            NEGATIVE_INTEGER.stringConverter = new Saxon.Hej.type.StringConverter.StringToIntegerSubtype(NEGATIVE_INTEGER);
            LONG.stringConverter = new Saxon.Hej.type.StringConverter.StringToIntegerSubtype(LONG);
            INT.stringConverter = new Saxon.Hej.type.StringConverter.StringToIntegerSubtype(INT);
            SHORT.stringConverter = new Saxon.Hej.type.StringConverter.StringToIntegerSubtype(SHORT);
            BYTE.stringConverter = new Saxon.Hej.type.StringConverter.StringToIntegerSubtype(BYTE);
            NON_NEGATIVE_INTEGER.stringConverter = new Saxon.Hej.type.StringConverter.StringToIntegerSubtype(NON_NEGATIVE_INTEGER);
            POSITIVE_INTEGER.stringConverter = new Saxon.Hej.type.StringConverter.StringToIntegerSubtype(POSITIVE_INTEGER);
            UNSIGNED_LONG.stringConverter = new Saxon.Hej.type.StringConverter.StringToIntegerSubtype(UNSIGNED_LONG);
            UNSIGNED_INT.stringConverter = new Saxon.Hej.type.StringConverter.StringToIntegerSubtype(UNSIGNED_INT);
            UNSIGNED_SHORT.stringConverter = new Saxon.Hej.type.StringConverter.StringToIntegerSubtype(UNSIGNED_SHORT);
            UNSIGNED_BYTE.stringConverter = new Saxon.Hej.type.StringConverter.StringToIntegerSubtype(UNSIGNED_BYTE);
        }
        private readonly int fingerprint;
        private int baseFingerprint;
        private int primitiveFingerprint;
        private Saxon.Hej.type.UType uType;
        private string alphaCode;
        private bool ordered = false;
        public Saxon.Hej.type.StringConverter stringConverter;
        private Saxon.Hej.value.SequenceType _one;
        private Saxon.Hej.value.SequenceType _oneOrMore;
        private Saxon.Hej.value.SequenceType _zeroOrOne;
        private Saxon.Hej.value.SequenceType _zeroOrMore;
        private static readonly System.Collections.Generic.IDictionary<string,Saxon.Hej.type.BuiltInAtomicType> byAlphaCode = new System.Collections.Generic.Dictionary<string, Saxon.Hej.type.BuiltInAtomicType>(60);
        public readonly static Saxon.Hej.type.BuiltInAtomicType ANY_ATOMIC = makeAtomicType(Saxon.Hej.om.StandardNames.XS_ANY_ATOMIC_TYPE, Saxon.Hej.type.AnySimpleType.getInstance(), "A", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType STRING = makeAtomicType(Saxon.Hej.om.StandardNames.XS_STRING, ANY_ATOMIC, "AS", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType BOOLEAN = makeAtomicType(Saxon.Hej.om.StandardNames.XS_BOOLEAN, ANY_ATOMIC, "AB", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType DURATION = makeAtomicType(Saxon.Hej.om.StandardNames.XS_DURATION, ANY_ATOMIC, "AR", false);
        public readonly static Saxon.Hej.type.BuiltInAtomicType DATE_TIME = makeAtomicType(Saxon.Hej.om.StandardNames.XS_DATE_TIME, ANY_ATOMIC, "AM", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType DATE = makeAtomicType(Saxon.Hej.om.StandardNames.XS_DATE, ANY_ATOMIC, "AA", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType TIME = makeAtomicType(Saxon.Hej.om.StandardNames.XS_TIME, ANY_ATOMIC, "AT", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType G_YEAR_MONTH = makeAtomicType(Saxon.Hej.om.StandardNames.XS_G_YEAR_MONTH, ANY_ATOMIC, "AH", false);
        public readonly static Saxon.Hej.type.BuiltInAtomicType G_MONTH = makeAtomicType(Saxon.Hej.om.StandardNames.XS_G_MONTH, ANY_ATOMIC, "AI", false);
        public readonly static Saxon.Hej.type.BuiltInAtomicType G_MONTH_DAY = makeAtomicType(Saxon.Hej.om.StandardNames.XS_G_MONTH_DAY, ANY_ATOMIC, "AJ", false);
        public readonly static Saxon.Hej.type.BuiltInAtomicType G_YEAR = makeAtomicType(Saxon.Hej.om.StandardNames.XS_G_YEAR, ANY_ATOMIC, "AG", false);
        public readonly static Saxon.Hej.type.BuiltInAtomicType G_DAY = makeAtomicType(Saxon.Hej.om.StandardNames.XS_G_DAY, ANY_ATOMIC, "AK", false);
        public readonly static Saxon.Hej.type.BuiltInAtomicType HEX_BINARY = makeAtomicType(Saxon.Hej.om.StandardNames.XS_HEX_BINARY, ANY_ATOMIC, "AX", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType BASE64_BINARY = makeAtomicType(Saxon.Hej.om.StandardNames.XS_BASE64_BINARY, ANY_ATOMIC, "A2", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType ANY_URI = makeAtomicType(Saxon.Hej.om.StandardNames.XS_ANY_URI, ANY_ATOMIC, "AU", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType QNAME = makeAtomicType(Saxon.Hej.om.StandardNames.XS_QNAME, ANY_ATOMIC, "AQ", false);
        public readonly static Saxon.Hej.type.BuiltInAtomicType NOTATION = makeAtomicType(Saxon.Hej.om.StandardNames.XS_NOTATION, ANY_ATOMIC, "AN", false);
        public readonly static Saxon.Hej.type.BuiltInAtomicType UNTYPED_ATOMIC = makeAtomicType(Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC, ANY_ATOMIC, "AZ", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType DECIMAL = makeAtomicType(Saxon.Hej.om.StandardNames.XS_DECIMAL, ANY_ATOMIC, "AD", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType FLOAT = makeAtomicType(Saxon.Hej.om.StandardNames.XS_FLOAT, ANY_ATOMIC, "AF", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType DOUBLE = makeAtomicType(Saxon.Hej.om.StandardNames.XS_DOUBLE, ANY_ATOMIC, "AO", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType INTEGER = makeAtomicType(Saxon.Hej.om.StandardNames.XS_INTEGER, DECIMAL, "ADI", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType NON_POSITIVE_INTEGER = makeAtomicType(Saxon.Hej.om.StandardNames.XS_NON_POSITIVE_INTEGER, INTEGER, "ADIN", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType NEGATIVE_INTEGER = makeAtomicType(Saxon.Hej.om.StandardNames.XS_NEGATIVE_INTEGER, NON_POSITIVE_INTEGER, "ADINN", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType LONG = makeAtomicType(Saxon.Hej.om.StandardNames.XS_LONG, INTEGER, "ADIL", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType INT = makeAtomicType(Saxon.Hej.om.StandardNames.XS_INT, LONG, "ADILI", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType SHORT = makeAtomicType(Saxon.Hej.om.StandardNames.XS_SHORT, INT, "ADILIS", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType BYTE = makeAtomicType(Saxon.Hej.om.StandardNames.XS_BYTE, SHORT, "ADILISB", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType NON_NEGATIVE_INTEGER = makeAtomicType(Saxon.Hej.om.StandardNames.XS_NON_NEGATIVE_INTEGER, INTEGER, "ADIP", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType POSITIVE_INTEGER = makeAtomicType(Saxon.Hej.om.StandardNames.XS_POSITIVE_INTEGER, NON_NEGATIVE_INTEGER, "ADIPP", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType UNSIGNED_LONG = makeAtomicType(Saxon.Hej.om.StandardNames.XS_UNSIGNED_LONG, NON_NEGATIVE_INTEGER, "ADIPL", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType UNSIGNED_INT = makeAtomicType(Saxon.Hej.om.StandardNames.XS_UNSIGNED_INT, UNSIGNED_LONG, "ADIPLI", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType UNSIGNED_SHORT = makeAtomicType(Saxon.Hej.om.StandardNames.XS_UNSIGNED_SHORT, UNSIGNED_INT, "ADIPLIS", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType UNSIGNED_BYTE = makeAtomicType(Saxon.Hej.om.StandardNames.XS_UNSIGNED_BYTE, UNSIGNED_SHORT, "ADIPLISB", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType YEAR_MONTH_DURATION = makeAtomicType(Saxon.Hej.om.StandardNames.XS_YEAR_MONTH_DURATION, DURATION, "ARY", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType DAY_TIME_DURATION = makeAtomicType(Saxon.Hej.om.StandardNames.XS_DAY_TIME_DURATION, DURATION, "ARD", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType NORMALIZED_STRING = makeAtomicType(Saxon.Hej.om.StandardNames.XS_NORMALIZED_STRING, STRING, "ASN", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType TOKEN = makeAtomicType(Saxon.Hej.om.StandardNames.XS_TOKEN, NORMALIZED_STRING, "ASNT", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType LANGUAGE = makeAtomicType(Saxon.Hej.om.StandardNames.XS_LANGUAGE, TOKEN, "ASNTL", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType NAME = makeAtomicType(Saxon.Hej.om.StandardNames.XS_NAME, TOKEN, "ASNTN", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType NMTOKEN = makeAtomicType(Saxon.Hej.om.StandardNames.XS_NMTOKEN, TOKEN, "ASNTK", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType NCNAME = makeAtomicType(Saxon.Hej.om.StandardNames.XS_NCNAME, NAME, "ASNTNC", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType ID = makeAtomicType(Saxon.Hej.om.StandardNames.XS_ID, NCNAME, "ASNTNCI", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType IDREF = makeAtomicType(Saxon.Hej.om.StandardNames.XS_IDREF, NCNAME, "ASNTNCR", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType ENTITY = makeAtomicType(Saxon.Hej.om.StandardNames.XS_ENTITY, NCNAME, "ASNTNCE", true);
        public readonly static Saxon.Hej.type.BuiltInAtomicType DATE_TIME_STAMP = makeAtomicType(Saxon.Hej.om.StandardNames.XS_DATE_TIME_STAMP, DATE_TIME, "AMP", true);
        public static Saxon.Hej.type.BuiltInAtomicType fromAlphaCode(string code) {
            return Saxon.Impl.Helpers.MapUtils.Get(byAlphaCode, code);
        }
        private BuiltInAtomicType(int fingerprint)  {
            this.fingerprint = fingerprint;
        }
        public string getName() {
            return Saxon.Hej.om.StandardNames.getLocalName(fingerprint);
        }
        public Saxon.Hej.type.UType getUType() {
            return uType;
        }
        public string getTargetNamespace() {
            return Saxon.Hej.lib.NamespaceConstant.SCHEMA;
        }
        public string getEQName() {
            return ((("Q{"+Saxon.Hej.lib.NamespaceConstant.SCHEMA)+"}")+getName());
        }
        public bool isAbstract() {
            switch (fingerprint) {
              case Saxon.Hej.om.StandardNames.XS_NOTATION:
              case Saxon.Hej.om.StandardNames.XS_ANY_ATOMIC_TYPE:
              case Saxon.Hej.om.StandardNames.XS_NUMERIC:
              case Saxon.Hej.om.StandardNames.XS_ANY_SIMPLE_TYPE:
                return true;
              default:
                return false;
            }
        }
        public bool isBuiltInType() {
            return true;
        }
        public Saxon.Hej.om.StructuredQName getTypeName() {
            return new Saxon.Hej.om.StructuredQName(Saxon.Hej.om.StandardNames.getPrefix(fingerprint), Saxon.Hej.om.StandardNames.getURI(fingerprint), Saxon.Hej.om.StandardNames.getLocalName(fingerprint));
        }
        public string getBasicAlphaCode() {
            return alphaCode;
        }
        public Saxon.Hej.value.SequenceType one() {
            if ((_one == null)) {
                _one = new Saxon.Hej.value.SequenceType(this, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE);
            }
            return _one;
        }
        public Saxon.Hej.value.SequenceType zeroOrOne() {
            if ((_zeroOrOne == null)) {
                _zeroOrOne = new Saxon.Hej.value.SequenceType(this, Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE);
            }
            return _zeroOrOne;
        }
        public Saxon.Hej.value.SequenceType oneOrMore() {
            if ((_oneOrMore == null)) {
                _oneOrMore = new Saxon.Hej.value.SequenceType(this, Saxon.Hej.expr.StaticProperty.ALLOWS_ONE_OR_MORE);
            }
            return _oneOrMore;
        }
        public Saxon.Hej.value.SequenceType zeroOrMore() {
            if ((_zeroOrMore == null)) {
                _zeroOrMore = new Saxon.Hej.value.SequenceType(this, Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE);
            }
            return _zeroOrMore;
        }
        public int getRedefinitionLevel() {
            return 0;
        }
        public bool isOrdered(bool optimistic) {
            return (ordered || ((optimistic && (((this == DURATION) || (this == ANY_ATOMIC))))));
        }
        public string getSystemId() {
            return null;
        }
        public bool isPrimitiveNumeric() {
            switch (getFingerprint()) {
              case Saxon.Hej.om.StandardNames.XS_INTEGER:
              case Saxon.Hej.om.StandardNames.XS_DECIMAL:
              case Saxon.Hej.om.StandardNames.XS_DOUBLE:
              case Saxon.Hej.om.StandardNames.XS_FLOAT:
                return true;
              default:
                return false;
            }
        }
        public Saxon.Hej.type.SchemaValidationStatus getValidationStatus() {
            return /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED;
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
        public void setBaseTypeFingerprint(int baseFingerprint) {
            this.baseFingerprint = baseFingerprint;
        }
        public int getFingerprint() {
            return fingerprint;
        }
        public Saxon.Hej.om.StructuredQName getStructuredQName() {
            return new Saxon.Hej.om.StructuredQName("xs", Saxon.Hej.lib.NamespaceConstant.SCHEMA, Saxon.Hej.om.StandardNames.getLocalName(fingerprint));
        }
        public string getDisplayName() {
            return Saxon.Hej.om.StandardNames.getDisplayName(fingerprint);
        }
        public bool isPrimitiveType() {
            return Saxon.Hej.type.Type.isPrimitiveAtomicType(fingerprint);
        }
        public bool isComplexType() {
            return false;
        }
        public bool isAnonymousType() {
            return false;
        }
        public bool isPlainType() {
            return true;
        }
        public Saxon.Hej.type.SchemaType getBaseType() {
            if ((baseFingerprint == -1)) {
                return null;
            } else {
                return Saxon.Hej.type.BuiltInType.getSchemaType(baseFingerprint);
            }
        }
        public bool matches(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            return ((item is Saxon.Hej.value.AtomicValue) && Saxon.Hej.type.Type.isSubType(((Saxon.Hej.value.AtomicValue)(item)).getItemType(), this));
        }
        public Saxon.Hej.type.ItemType getPrimitiveItemType() {
            if (isPrimitiveType()) {
                return this;
            } else {
                Saxon.Hej.type.ItemType s = (Saxon.Hej.type.ItemType)(getBaseType());
                System.Diagnostics.Debug.Assert((s != null));
                if (s.isPlainType()) {
                    return (Saxon.Hej.type.BuiltInAtomicType)(s.getPrimitiveItemType());
                } else {
                    return this;
                }
            }
        }
        public int getPrimitiveType() {
            return primitiveFingerprint;
        }
        public bool isAllowedInXSD10() {
            return (getFingerprint() != Saxon.Hej.om.StandardNames.XS_DATE_TIME_STAMP);
        }
        public override string ToString() {
            return getDisplayName();
        }
        public Saxon.Hej.type.PlainType getAtomizedItemType() {
            return this;
        }
        public bool isAtomizable(Saxon.Hej.type.TypeHierarchy th) {
            return true;
        }
        public Saxon.Hej.type.SchemaType getKnownBaseType() {
            return getBaseType();
        }
        public bool isSameType(Saxon.Hej.type.SchemaType other) {
            return (other.getFingerprint() == getFingerprint());
        }
        public string getDescription() {
            return getDisplayName();
        }
        public void checkTypeDerivationIsOK(Saxon.Hej.type.SchemaType type, int block) {
            if ((type == Saxon.Hej.type.AnySimpleType.getInstance())) {
            } else {
                if (isSameType(type)) {
                } else {
                    Saxon.Hej.type.SchemaType @base = getBaseType();
                    if ((@base == null)) {
                        throw new Saxon.Hej.type.SchemaException(((("The type "+getDescription())+" is not validly derived from the type ")+type.getDescription()));
                    }
                    try {
                        @base.checkTypeDerivationIsOK(type, block);
                    } catch (Saxon.Hej.type.SchemaException se) {
                        throw new Saxon.Hej.type.SchemaException(((("The type "+getDescription())+" is not validly derived from the type ")+type.getDescription()));
                    }
                }
            }
        }
        public bool isSimpleType() {
            return true;
        }
        public bool isAtomicType() {
            return true;
        }
        public bool isIdType() {
            return (fingerprint == Saxon.Hej.om.StandardNames.XS_ID);
        }
        public bool isIdRefType() {
            return (fingerprint == Saxon.Hej.om.StandardNames.XS_IDREF);
        }
        public bool isListType() {
            return false;
        }
        public bool isUnionType() {
            return false;
        }
        public int getWhitespaceAction() {
            switch (getFingerprint()) {
              case Saxon.Hej.om.StandardNames.XS_STRING:
                return Saxon.Hej.value.Whitespace.PRESERVE;
              case Saxon.Hej.om.StandardNames.XS_NORMALIZED_STRING:
                return Saxon.Hej.value.Whitespace.REPLACE;
              default:
                return Saxon.Hej.value.Whitespace.COLLAPSE;
            }
        }
        public Saxon.Hej.type.SchemaType getBuiltInBaseType() {
            Saxon.Hej.type.BuiltInAtomicType @base = this;
            while ((((@base != null)) && ((@base.getFingerprint()>1023)))) {
                @base = (Saxon.Hej.type.BuiltInAtomicType)(@base.getBaseType());
            }
            return @base;
        }
        public bool isNamespaceSensitive() {
            Saxon.Hej.type.BuiltInAtomicType @base = this;
            int fp = @base.getFingerprint();
            while ((fp>1023)) {
                @base = (Saxon.Hej.type.BuiltInAtomicType)(@base.getBaseType());
                System.Diagnostics.Debug.Assert((@base != null));
                fp = @base.getFingerprint();
            }
            return ((fp == Saxon.Hej.om.StandardNames.XS_QNAME) || (fp == Saxon.Hej.om.StandardNames.XS_NOTATION));
        }
        public Saxon.Hej.type.ValidationFailure validateContent(Saxon.Hej.str.UnicodeString value, Saxon.Hej.om.NamespaceResolver nsResolver, Saxon.Hej.lib.ConversionRules rules) {
            int f = getFingerprint();
            if (((((f == Saxon.Hej.om.StandardNames.XS_STRING) || (f == Saxon.Hej.om.StandardNames.XS_ANY_SIMPLE_TYPE)) || (f == Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC)) || (f == Saxon.Hej.om.StandardNames.XS_ANY_ATOMIC_TYPE))) {
                return null;
            }
            Saxon.Hej.type.StringConverter converter = stringConverter;
            if ((converter == null)) {
                converter = getStringConverter(rules);
                if (isNamespaceSensitive()) {
                    if ((nsResolver == null)) {
                        throw new Saxon.Ejava.lang.UnsupportedOperationException("Cannot validate a QName without a namespace resolver");
                    }
                    converter = (Saxon.Hej.type.StringConverter)(converter.setNamespaceResolver(nsResolver));
                    Saxon.Hej.type.ConversionResult result = converter.convertString(value);
                    if ((result is Saxon.Hej.type.ValidationFailure)) {
                        return (Saxon.Hej.type.ValidationFailure)(result);
                    }
                    if ((fingerprint == Saxon.Hej.om.StandardNames.XS_NOTATION)) {
                        Saxon.Hej.value.NotationValue nv = (Saxon.Hej.value.NotationValue)(result);
                        if (!(rules.isDeclaredNotation(nv.getNamespaceURI(), nv.getLocalName()))) {
                            return new Saxon.Hej.type.ValidationFailure((((("Notation {"+nv.getNamespaceURI())+"}")+nv.getLocalName())+" is not declared in the schema"));
                        }
                    }
                    return null;
                }
            }
            return converter.validate(value);
        }
        public Saxon.Hej.type.StringConverter getStringConverter(Saxon.Hej.lib.ConversionRules rules) {
            if ((stringConverter != null)) {
                return stringConverter;
            }
            switch (fingerprint) {
              case Saxon.Hej.om.StandardNames.XS_DOUBLE:
              case Saxon.Hej.om.StandardNames.XS_NUMERIC:
                return rules.getStringToDoubleConverter();
              case Saxon.Hej.om.StandardNames.XS_FLOAT:
                return new Saxon.Hej.type.StringConverter.StringToFloat(rules);
              case Saxon.Hej.om.StandardNames.XS_DATE_TIME:
                return new Saxon.Hej.type.StringConverter.StringToDateTime(rules);
              case Saxon.Hej.om.StandardNames.XS_DATE_TIME_STAMP:
                return new Saxon.Hej.type.StringConverter.StringToDateTimeStamp(rules);
              case Saxon.Hej.om.StandardNames.XS_DATE:
                return new Saxon.Hej.type.StringConverter.StringToDate(rules);
              case Saxon.Hej.om.StandardNames.XS_G_YEAR:
                return new Saxon.Hej.type.StringConverter.StringToGYear(rules);
              case Saxon.Hej.om.StandardNames.XS_G_YEAR_MONTH:
                return new Saxon.Hej.type.StringConverter.StringToGYearMonth(rules);
              case Saxon.Hej.om.StandardNames.XS_ANY_URI:
                return new Saxon.Hej.type.StringConverter.StringToAnyURI(rules);
              case Saxon.Hej.om.StandardNames.XS_QNAME:
                return new Saxon.Hej.type.StringConverter.StringToQName(rules);
              case Saxon.Hej.om.StandardNames.XS_NOTATION:
                return new Saxon.Hej.type.StringConverter.StringToNotation(rules);
              default:
                throw new Saxon.Ejava.lang.AssertionError(("No string converter available for "+this));
            }
        }
        public Saxon.Hej.om.AtomicSequence atomize(Saxon.Hej.om.NodeInfo node) {
            Saxon.Hej.str.UnicodeString stringValue = node.getUnicodeStringValue();
            if ((stringValue.isEmpty() && node.isNilled())) {
                return Saxon.Hej.om.AtomicArray.EMPTY_ATOMIC_ARRAY;
            }
            if ((fingerprint == Saxon.Hej.om.StandardNames.XS_STRING)) {
                return new Saxon.Hej.value.StringValue(stringValue.tidy());
            } else {
                if ((fingerprint == Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC)) {
                    return Saxon.Hej.value.StringValue.makeUntypedAtomic(stringValue);
                }
            }
            Saxon.Hej.type.StringConverter converter = stringConverter;
            if ((converter == null)) {
                converter = getStringConverter(node.getConfiguration().getConversionRules());
                if (isNamespaceSensitive()) {
                    Saxon.Hej.om.NodeInfo container = ((node.getNodeKind() == Saxon.Hej.type.Type.ELEMENT) ? node : node.getParent());
                    converter = (Saxon.Hej.type.StringConverter)(converter.setNamespaceResolver(container.getAllNamespaces()));
                }
            }
            return converter.convertString(stringValue).asAtomic();
        }
        public Saxon.Hej.om.AtomicSequence getTypedValue(Saxon.Hej.str.UnicodeString value, Saxon.Hej.om.NamespaceResolver resolver, Saxon.Hej.lib.ConversionRules rules) {
            if ((fingerprint == Saxon.Hej.om.StandardNames.XS_STRING)) {
                return new Saxon.Hej.value.StringValue(value.tidy());
            } else {
                if ((fingerprint == Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC)) {
                    return Saxon.Hej.value.StringValue.makeUntypedAtomic(value);
                }
            }
            Saxon.Hej.type.StringConverter converter = getStringConverter(rules);
            if (isNamespaceSensitive()) {
                converter = (Saxon.Hej.type.StringConverter)(converter.setNamespaceResolver(resolver));
            }
            return converter.convertString(value).asAtomic();
        }
        public override bool Equals(System.Object obj) {
            return ((obj is Saxon.Hej.type.BuiltInAtomicType) && (getFingerprint() == ((Saxon.Hej.type.BuiltInAtomicType)(obj)).getFingerprint()));
        }
        public override int GetHashCode() {
            return getFingerprint();
        }
        public Saxon.Hej.type.ValidationFailure validate(Saxon.Hej.value.AtomicValue primValue, Saxon.Hej.str.UnicodeString lexicalValue, Saxon.Hej.lib.ConversionRules rules) {
            switch (fingerprint) {
              case Saxon.Hej.om.StandardNames.XS_NUMERIC:
              case Saxon.Hej.om.StandardNames.XS_STRING:
              case Saxon.Hej.om.StandardNames.XS_BOOLEAN:
              case Saxon.Hej.om.StandardNames.XS_DURATION:
              case Saxon.Hej.om.StandardNames.XS_DATE_TIME:
              case Saxon.Hej.om.StandardNames.XS_DATE:
              case Saxon.Hej.om.StandardNames.XS_TIME:
              case Saxon.Hej.om.StandardNames.XS_G_YEAR_MONTH:
              case Saxon.Hej.om.StandardNames.XS_G_MONTH:
              case Saxon.Hej.om.StandardNames.XS_G_MONTH_DAY:
              case Saxon.Hej.om.StandardNames.XS_G_YEAR:
              case Saxon.Hej.om.StandardNames.XS_G_DAY:
              case Saxon.Hej.om.StandardNames.XS_HEX_BINARY:
              case Saxon.Hej.om.StandardNames.XS_BASE64_BINARY:
              case Saxon.Hej.om.StandardNames.XS_ANY_URI:
              case Saxon.Hej.om.StandardNames.XS_QNAME:
              case Saxon.Hej.om.StandardNames.XS_NOTATION:
              case Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC:
              case Saxon.Hej.om.StandardNames.XS_DECIMAL:
              case Saxon.Hej.om.StandardNames.XS_FLOAT:
              case Saxon.Hej.om.StandardNames.XS_DOUBLE:
                return null;
              case Saxon.Hej.om.StandardNames.XS_INTEGER:
                if ((primValue.getItemType() == Saxon.Hej.type.BuiltInAtomicType.DECIMAL)) {
                    if (((Saxon.Hej.value.DecimalValue)(primValue)).isWholeNumber()) {
                        return null;
                    } else {
                        return new Saxon.Hej.type.ValidationFailure((("xs:decimal value "+primValue.toShortString())+" cannot be down-cast to xs:integer"));
                    }
                } else {
                    return null;
                }
              case Saxon.Hej.om.StandardNames.XS_NON_POSITIVE_INTEGER:
              case Saxon.Hej.om.StandardNames.XS_NEGATIVE_INTEGER:
              case Saxon.Hej.om.StandardNames.XS_LONG:
              case Saxon.Hej.om.StandardNames.XS_INT:
              case Saxon.Hej.om.StandardNames.XS_SHORT:
              case Saxon.Hej.om.StandardNames.XS_BYTE:
              case Saxon.Hej.om.StandardNames.XS_NON_NEGATIVE_INTEGER:
              case Saxon.Hej.om.StandardNames.XS_POSITIVE_INTEGER:
              case Saxon.Hej.om.StandardNames.XS_UNSIGNED_LONG:
              case Saxon.Hej.om.StandardNames.XS_UNSIGNED_INT:
              case Saxon.Hej.om.StandardNames.XS_UNSIGNED_SHORT:
              case Saxon.Hej.om.StandardNames.XS_UNSIGNED_BYTE:
                return ((Saxon.Hej.value.IntegerValue)(primValue)).validateAgainstSubType(this);
              case Saxon.Hej.om.StandardNames.XS_YEAR_MONTH_DURATION:
              case Saxon.Hej.om.StandardNames.XS_DAY_TIME_DURATION:
                return null;
              case Saxon.Hej.om.StandardNames.XS_DATE_TIME_STAMP:
                return ((((Saxon.Hej.value.CalendarValue)(primValue)).getTimezoneInMinutes() == Saxon.Hej.value.CalendarValue.NO_TIMEZONE) ? new Saxon.Hej.type.ValidationFailure("xs:dateTimeStamp value must have a timezone") : null);
              case Saxon.Hej.om.StandardNames.XS_NORMALIZED_STRING:
              case Saxon.Hej.om.StandardNames.XS_TOKEN:
              case Saxon.Hej.om.StandardNames.XS_LANGUAGE:
              case Saxon.Hej.om.StandardNames.XS_NAME:
              case Saxon.Hej.om.StandardNames.XS_NMTOKEN:
              case Saxon.Hej.om.StandardNames.XS_NCNAME:
              case Saxon.Hej.om.StandardNames.XS_ID:
              case Saxon.Hej.om.StandardNames.XS_IDREF:
              case Saxon.Hej.om.StandardNames.XS_ENTITY:
                return stringConverter.validate(primValue.getUnicodeStringValue());
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException();
            }
        }
        public void analyzeContentExpression(Saxon.Hej.expr.Expression expression, int kind) {
            analyzeContentExpression(this, expression, kind);
        }
        public static void analyzeContentExpression(Saxon.Hej.type.SimpleType simpleType, Saxon.Hej.expr.Expression expression, int kind) {
            if ((kind == Saxon.Hej.type.Type.ELEMENT)) {
                expression.checkPermittedContents(simpleType, true);
            } else {
                if ((kind == Saxon.Hej.type.Type.ATTRIBUTE)) {
                    if (((expression is Saxon.Hej.expr.instruct.ValueOf) || (expression is Saxon.Hej.expr.Literal))) {
                        expression.checkPermittedContents(simpleType, true);
                    }
                }
            }
        }
        private static Saxon.Hej.type.BuiltInAtomicType makeAtomicType(int fingerprint, Saxon.Hej.type.SimpleType baseType, string code, bool ordered) {
            Saxon.Hej.type.BuiltInAtomicType t = new Saxon.Hej.type.BuiltInAtomicType(fingerprint);
            t.setBaseTypeFingerprint(baseType.getFingerprint());
            if (t.isPrimitiveType()) {
                t.primitiveFingerprint = fingerprint;
            } else {
                t.primitiveFingerprint = ((Saxon.Hej.type.AtomicType)(baseType)).getPrimitiveType();
            }
            t.uType = Saxon.Hej.type.UType.fromTypeCode(t.primitiveFingerprint);
            t.ordered = ordered;
            t.alphaCode = code;
            Saxon.Hej.type.BuiltInType.register(fingerprint, t);
            Saxon.Impl.Helpers.MapUtils.Put(byAlphaCode, code, t);
            return t;
        }
        public Saxon.Hej.str.UnicodeString preprocess(Saxon.Hej.str.UnicodeString input) {
            return input;
        }
        public Saxon.Hej.str.UnicodeString postprocess(Saxon.Hej.str.UnicodeString input) {
            return input;
        }
        public System.Collections.Generic.IList<Saxon.Hej.type.PlainType> getPlainMemberTypes() {
            return Saxon.Ejava.util.Collections.singletonList((Saxon.Hej.type.PlainType)(this));
        }
        public bool isNumericType() {
            Saxon.Hej.type.ItemType p = ((/*1430*/Saxon.Hej.type.BuiltInAtomicType)getPrimitiveItemType());
            return (((((p == Saxon.Hej.type.NumericType.getInstance()) || (p == DECIMAL)) || (p == DOUBLE)) || (p == FLOAT)) || (p == INTEGER));
        }
        public Saxon.Hej.om.Function getComponentAsFunction() {
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

        /* Expanding default method getDefaultPriority from interface AtomicType */
        public virtual double getDefaultPriority() {
            if (getBasicAlphaCode().Equals("A")) {
                return 0;
            }
            double factor = 1;
            Saxon.Hej.type.SchemaType at = this;
            do {
                factor *= 0.5;
                at = at.getBaseType();
            } while ((at != Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC));
            return (1-factor);
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