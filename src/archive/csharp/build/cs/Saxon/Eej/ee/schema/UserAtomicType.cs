using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class UserAtomicType : Saxon.Eej.ee.schema.UserSimpleType, Saxon.Hej.type.AtomicType {

        static UserAtomicType() {
            STRING_FACETS.Add(typeof(Saxon.Eej.ee.schema.LengthFacet));
            STRING_FACETS.Add(typeof(Saxon.Eej.ee.schema.MinLengthFacet));
            STRING_FACETS.Add(typeof(Saxon.Eej.ee.schema.MaxLengthFacet));
            STRING_FACETS.Add(typeof(Saxon.Eej.ee.schema.PatternFacet));
            STRING_FACETS.Add(typeof(Saxon.Eej.ee.schema.EnumerationFacet));
            STRING_FACETS.Add(typeof(Saxon.Eej.ee.schema.EnumerationFacetSet));
            STRING_FACETS.Add(typeof(Saxon.Eej.ee.schema.WhitespaceFacet));
            STRING_FACETS.Add(typeof(Saxon.Eej.ee.schema.AssertionFacet));
            STRING_FACETS.Add(typeof(Saxon.Eej.ee.schema.PreprocessFacet));
            BOOLEAN_FACETS.Add(typeof(Saxon.Eej.ee.schema.PatternFacet));
            BOOLEAN_FACETS.Add(typeof(Saxon.Eej.ee.schema.WhitespaceFacet));
            BOOLEAN_FACETS.Add(typeof(Saxon.Eej.ee.schema.AssertionFacet));
            BOOLEAN_FACETS.Add(typeof(Saxon.Eej.ee.schema.PreprocessFacet));
            ORDERED_FACETS.Add(typeof(Saxon.Eej.ee.schema.PatternFacet));
            ORDERED_FACETS.Add(typeof(Saxon.Eej.ee.schema.EnumerationFacet));
            ORDERED_FACETS.Add(typeof(Saxon.Eej.ee.schema.EnumerationFacetSet));
            ORDERED_FACETS.Add(typeof(Saxon.Eej.ee.schema.WhitespaceFacet));
            ORDERED_FACETS.Add(typeof(Saxon.Eej.ee.schema.MinInclusiveFacet));
            ORDERED_FACETS.Add(typeof(Saxon.Eej.ee.schema.MaxInclusiveFacet));
            ORDERED_FACETS.Add(typeof(Saxon.Eej.ee.schema.MinExclusiveFacet));
            ORDERED_FACETS.Add(typeof(Saxon.Eej.ee.schema.MaxExclusiveFacet));
            ORDERED_FACETS.Add(typeof(Saxon.Eej.ee.schema.AssertionFacet));
            ORDERED_FACETS.Add(typeof(Saxon.Eej.ee.schema.PreprocessFacet));
            CALENDAR_FACETS.Add(typeof(Saxon.Eej.ee.schema.PatternFacet));
            CALENDAR_FACETS.Add(typeof(Saxon.Eej.ee.schema.EnumerationFacet));
            CALENDAR_FACETS.Add(typeof(Saxon.Eej.ee.schema.EnumerationFacetSet));
            CALENDAR_FACETS.Add(typeof(Saxon.Eej.ee.schema.WhitespaceFacet));
            CALENDAR_FACETS.Add(typeof(Saxon.Eej.ee.schema.MinInclusiveFacet));
            CALENDAR_FACETS.Add(typeof(Saxon.Eej.ee.schema.MaxInclusiveFacet));
            CALENDAR_FACETS.Add(typeof(Saxon.Eej.ee.schema.MinExclusiveFacet));
            CALENDAR_FACETS.Add(typeof(Saxon.Eej.ee.schema.MaxExclusiveFacet));
            CALENDAR_FACETS.Add(typeof(Saxon.Eej.ee.schema.ExplicitTimezoneFacet));
            CALENDAR_FACETS.Add(typeof(Saxon.Eej.ee.schema.AssertionFacet));
            CALENDAR_FACETS.Add(typeof(Saxon.Eej.ee.schema.PreprocessFacet));
            DECIMAL_FACETS.Add(typeof(Saxon.Eej.ee.schema.TotalDigitsFacet));
            DECIMAL_FACETS.Add(typeof(Saxon.Eej.ee.schema.FractionDigitsFacet));
            DECIMAL_FACETS.Add(typeof(Saxon.Eej.ee.schema.PatternFacet));
            DECIMAL_FACETS.Add(typeof(Saxon.Eej.ee.schema.EnumerationFacet));
            DECIMAL_FACETS.Add(typeof(Saxon.Eej.ee.schema.EnumerationFacetSet));
            DECIMAL_FACETS.Add(typeof(Saxon.Eej.ee.schema.WhitespaceFacet));
            DECIMAL_FACETS.Add(typeof(Saxon.Eej.ee.schema.MinInclusiveFacet));
            DECIMAL_FACETS.Add(typeof(Saxon.Eej.ee.schema.MaxInclusiveFacet));
            DECIMAL_FACETS.Add(typeof(Saxon.Eej.ee.schema.MinExclusiveFacet));
            DECIMAL_FACETS.Add(typeof(Saxon.Eej.ee.schema.MaxExclusiveFacet));
            DECIMAL_FACETS.Add(typeof(Saxon.Eej.ee.schema.AssertionFacet));
            DECIMAL_FACETS.Add(typeof(Saxon.Eej.ee.schema.PreprocessFacet));
        }
        private Saxon.Ejava.util.Optional<bool> namespaceSensitive = Saxon.Ejava.util.Optional<bool>.empty();
        private Saxon.Hej.type.StringConverter stringConverter;
        private readonly static System.Collections.Generic.ISet<System.Type> STRING_FACETS = newFacetSet();
        private readonly static System.Collections.Generic.ISet<System.Type> BOOLEAN_FACETS = newFacetSet();
        private readonly static System.Collections.Generic.ISet<System.Type> ORDERED_FACETS = newFacetSet();
        private readonly static System.Collections.Generic.ISet<System.Type> CALENDAR_FACETS = newFacetSet();
        private readonly static System.Collections.Generic.ISet<System.Type> DECIMAL_FACETS = newFacetSet();
        private static System.Collections.Generic.ISet<System.Type> newFacetSet() {
            return new System.Collections.Generic.HashSet<System.Type>(16);
        }
        public UserAtomicType(Saxon.Eej.config.EnterpriseConfiguration config)  {
            setConfiguration(config);
        }
        public override bool isAtomicType() {
            return true;
        }
        public bool isPlainType() {
            return true;
        }
        public override bool isIdType() {
            return getBuiltInBaseType().isIdType();
        }
        public override bool isIdRefType() {
            return getBaseType().isIdRefType();
        }
        public override bool isBuiltInType() {
            return false;
        }
        public string getBasicAlphaCode() {
            return ((Saxon.Hej.type.AtomicType)(getBuiltInBaseType())).getBasicAlphaCode();
        }
        public bool isPrimitiveType() {
            return false;
        }
        public Saxon.Hej.type.UType getUType() {
            return ((/*1430*/Saxon.Hej.type.AtomicType)getPrimitiveItemType()).getUType();
        }
        public bool isOrdered(bool optimistic) {
            return ((/*1430*/Saxon.Hej.type.AtomicType)getPrimitiveItemType()).isOrdered(false);
        }
        public bool isAbstract() {
            return false;
        }
        public Saxon.Hej.type.StringConverter getStringConverter(Saxon.Hej.lib.ConversionRules rules) {
            if ((stringConverter == null)) {
                Saxon.Hej.type.StringConverter sc;
                Saxon.Hej.type.AtomicType prim = ((/*1430*/Saxon.Hej.type.AtomicType)getPrimitiveItemType());
                if ((prim.getFingerprint() == Saxon.Hej.om.StandardNames.XS_STRING)) {
                    if ((getBuiltInBaseType() == Saxon.Hej.type.BuiltInAtomicType.STRING)) {
                        sc = new Saxon.Hej.type.StringConverter.StringToStringSubtype(rules, this);
                    } else {
                        sc = new Saxon.Hej.type.StringConverter.StringToDerivedStringSubtype(rules, this);
                    }
                } else {
                    Saxon.Hej.type.StringConverter first = ((/*1430*/Saxon.Hej.type.AtomicType)getPrimitiveItemType()).getStringConverter(rules);
                    Saxon.Hej.type.Converter.DownCastingConverter second = new Saxon.Hej.type.Converter.DownCastingConverter(this, rules);
                    sc = new Saxon.Hej.type.StringConverter.StringToNonStringDerivedType(first, second);
                }
                if (((rules is Saxon.Eej.ee.validate.ValidationContext) && ((Saxon.Eej.ee.validate.ValidationContext)(rules)).hasValidationParams())) {
                    return sc;
                } else {
                    stringConverter = sc;
                }
            }
            return stringConverter;
        }
        public override bool validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if (!(base.validate(compiler))) {
                return false;
            }
            if ((getBaseType() is Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)) {
                Saxon.Hej.type.SimpleType simpleType = ((Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)(getBaseType())).getWorkingType();
                getBaseTypeReference().setTarget(simpleType);
            }
            Saxon.Hej.type.AtomicType prim = ((/*1430*/Saxon.Hej.type.AtomicType)getPrimitiveItemType());
            System.Collections.Generic.ISet<System.Type> allowed = null;
            switch (prim.getFingerprint()) {
              case Saxon.Hej.om.StandardNames.XS_STRING:
              case Saxon.Hej.om.StandardNames.XS_HEX_BINARY:
              case Saxon.Hej.om.StandardNames.XS_BASE64_BINARY:
              case Saxon.Hej.om.StandardNames.XS_ANY_URI:
              case Saxon.Hej.om.StandardNames.XS_QNAME:
              case Saxon.Hej.om.StandardNames.XS_NOTATION:
                allowed = STRING_FACETS;
                break ;
              case Saxon.Hej.om.StandardNames.XS_BOOLEAN:
                allowed = BOOLEAN_FACETS;
                break ;
              case Saxon.Hej.om.StandardNames.XS_FLOAT:
              case Saxon.Hej.om.StandardNames.XS_DOUBLE:
              case Saxon.Hej.om.StandardNames.XS_DURATION:
              case Saxon.Hej.om.StandardNames.XS_DAY_TIME_DURATION:
              case Saxon.Hej.om.StandardNames.XS_YEAR_MONTH_DURATION:
                allowed = ORDERED_FACETS;
                break ;
              case Saxon.Hej.om.StandardNames.XS_DATE_TIME:
              case Saxon.Hej.om.StandardNames.XS_TIME:
              case Saxon.Hej.om.StandardNames.XS_DATE:
              case Saxon.Hej.om.StandardNames.XS_G_YEAR_MONTH:
              case Saxon.Hej.om.StandardNames.XS_G_YEAR:
              case Saxon.Hej.om.StandardNames.XS_G_MONTH:
              case Saxon.Hej.om.StandardNames.XS_G_MONTH_DAY:
              case Saxon.Hej.om.StandardNames.XS_G_DAY:
                allowed = CALENDAR_FACETS;
                break ;
              case Saxon.Hej.om.StandardNames.XS_DECIMAL:
              case Saxon.Hej.om.StandardNames.XS_INTEGER:
                allowed = DECIMAL_FACETS;
                break ;
            }
            foreach (Saxon.Eej.ee.schema.Facet fac in getExtendedFacetList()) {
                if (((allowed == null) || !(allowed.Contains(fac.GetType())))) {
                    compiler.error(((("The "+fac.getName())+" facet is not applicable to types derived from ")+prim.getDisplayName()), this);
                    return false;
                }
            }
            return true;
        }
        public bool matches(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            return ((item is Saxon.Hej.value.AtomicValue) && Saxon.Hej.type.Type.isSubType(((Saxon.Hej.value.AtomicValue)(item)).getItemType(), this));
        }
        public override Saxon.Hej.type.ValidationFailure validateContent(Saxon.Hej.str.UnicodeString value, Saxon.Hej.om.NamespaceResolver nsResolver, Saxon.Hej.lib.ConversionRules rules) {
            Saxon.Hej.type.StringConverter converter = getStringConverter(rules);
            if (isNamespaceSensitive()) {
                if ((nsResolver == null)) {
                    throw new Saxon.Ejava.lang.UnsupportedOperationException("Cannot validate a QName without a namespace resolver");
                }
                converter = (Saxon.Hej.type.StringConverter)(converter.setNamespaceResolver(nsResolver));
            }
            return converter.validate(value);
        }
        public override Saxon.Hej.value.AtomicValue getTypedValue(Saxon.Hej.str.UnicodeString value, Saxon.Hej.om.NamespaceResolver resolver, Saxon.Hej.lib.ConversionRules rules) {
            Saxon.Hej.type.StringConverter converter = getStringConverter(rules);
            System.Diagnostics.Debug.Assert((converter != null));
            if (isNamespaceSensitive()) {
                converter = (Saxon.Hej.type.StringConverter)(converter.setNamespaceResolver(resolver));
            }
            return converter.convertString(value).asAtomic();
        }
        public Saxon.Hej.type.ItemType getPrimitiveItemType() {
            try {
                Saxon.Hej.type.ItemType s = (Saxon.Hej.type.ItemType)(getBaseType());
                if (s.isPlainType()) {
                    return (Saxon.Hej.type.AtomicType)(s.getPrimitiveItemType());
                } else {
                    return this;
                }
            } catch (System.InvalidCastException err) {
                if ((getBaseType() == Saxon.Hej.type.AnySimpleType.getInstance())) {
                    return this;
                }
                if (Saxon.Hej.type.Type.isPrimitiveAtomicType(getFingerprint())) {
                    return this;
                }
                getConfiguration().getLogger().info(("Internal failure on type "+getDescription()));
                throw;
            }
        }
        public int getPrimitiveType() {
            Saxon.Hej.type.SchemaType s = getBaseType();
            if (s.isAtomicType()) {
                return ((Saxon.Hej.type.AtomicType)(s)).getPrimitiveType();
            } else {
                return getFingerprint();
            }
        }
        public override bool isNamespaceSensitive() {
            if (!(namespaceSensitive.isPresent())) {
                Saxon.Hej.type.SimpleType @base = this;
                int fp = @base.getFingerprint();
                while ((fp>1023)) {
                    Saxon.Hej.type.SchemaType b = @base.getBaseType();
                    if ((b is Saxon.Hej.type.ComplexType)) {
                        try {
                            @base = ((Saxon.Hej.type.ComplexType)(b)).getSimpleContentType();
                        } catch (Saxon.Hej.type.MissingComponentException e) {
                            return true;
                        }
                    } else {
                        @base = (Saxon.Hej.type.SimpleType)(b);
                    }
                    fp = @base.getFingerprint();
                }
                bool sensitive = ((fp == Saxon.Hej.om.StandardNames.XS_QNAME) || (fp == Saxon.Hej.om.StandardNames.XS_NOTATION));
                namespaceSensitive = Saxon.Ejava.util.Optional<bool>.of(sensitive);
                return sensitive;
            } else {
                return namespaceSensitive.get();
            }
        }
        public Saxon.Hej.type.PlainType getAtomizedItemType() {
            return this;
        }
        public bool isAtomizable(Saxon.Hej.type.TypeHierarchy th) {
            return true;
        }
        public string getSharedInstance(string value) {
            Saxon.Eej.ee.schema.EnumerationFacetSet facet = (Saxon.Eej.ee.schema.EnumerationFacetSet)(getFacet(typeof(Saxon.Eej.ee.schema.EnumerationFacet)));
            if ((facet == null)) {
                return value;
            }
            return facet.getSharedInstance(value);
        }
        public override string ToString() {
            return this.getTypeName().getEQName();
        }
        public string toExportString() {
            if (isAnonymousType()) {
                return ((Saxon.Hej.type.AtomicType)(Saxon.Hej.type.TypeHierarchy.getNearestNamedType(this))).toExportString();
            } else {
                return ToString();
            }
        }
        public override int GetHashCode() {
            return getFingerprint();
        }
        public Saxon.Hej.type.ValidationFailure validate(Saxon.Hej.value.AtomicValue primValue, Saxon.Hej.str.UnicodeString lexicalValue, Saxon.Hej.lib.ConversionRules rules) {
            Saxon.Hej.type.BuiltInAtomicType @base = (Saxon.Hej.type.BuiltInAtomicType)(getBuiltInBaseType());
            if (!(@base.isPrimitiveType())) {
                Saxon.Hej.type.ValidationFailure vf = @base.validate(primValue, lexicalValue, rules);
                if ((vf != null)) {
                    return vf;
                }
            }
            return checkAgainstFacets(primValue, lexicalValue, getExtendedFacetList(), rules);
        }
        protected internal override void serializeVariety(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            serializer.emitAttribute("variety", "atomic");
            serializer.emitAttribute("primitiveType", ("#"+((/*1430*/Saxon.Hej.type.AtomicType)getPrimitiveItemType()).getName()));
        }
        public System.Collections.Generic.IList<Saxon.Hej.type.PlainType> getPlainMemberTypes() {
            return Saxon.Ejava.util.Collections.singletonList<Saxon.Hej.type.PlainType>(this);
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

    }
}