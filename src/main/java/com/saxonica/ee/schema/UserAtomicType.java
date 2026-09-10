package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.sdoc.SimpleTypeDefinition;
import com.saxonica.ee.validate.ValidationContext;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NamespaceResolver;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.type.*;
import net.sf.saxon.value.AtomicValue;

import java.util.*;

/**
 * An object belonging to this class represents an atomic type: either a built-in
 * atomic type, or a user-defined atomic type. There is at most one AtomicType
 * object for each distinct type in the schema (so equality can safely be tested
 * using the == operator).
 */
public class UserAtomicType extends UserSimpleType implements AtomicType {

    // Flag indicating whether the type is namespace-sensitive: null means not yet computed
    private Optional<Boolean> namespaceSensitive = Optional.empty();
    // Converter to be used for converting strings to this type
    private StringConverter stringConverter;

    private final static Set<Class<? extends Facet>> STRING_FACETS = newFacetSet();
    private final static Set<Class<? extends Facet>> BOOLEAN_FACETS = newFacetSet();
    private final static Set<Class<? extends Facet>> ORDERED_FACETS = newFacetSet();
    private final static Set<Class<? extends Facet>> CALENDAR_FACETS = newFacetSet();
    private final static Set<Class<? extends Facet>> DECIMAL_FACETS = newFacetSet();

    private static Set<Class<? extends Facet>> newFacetSet() {
        // Written this way for C# conversion
        return new HashSet<Class<? extends Facet>>(16);
    }

    static {
        STRING_FACETS.add(LengthFacet.class);
        STRING_FACETS.add(MinLengthFacet.class);
        STRING_FACETS.add(MaxLengthFacet.class);
        STRING_FACETS.add(PatternFacet.class);
        STRING_FACETS.add(EnumerationFacet.class);
        STRING_FACETS.add(EnumerationFacetSet.class);
        STRING_FACETS.add(WhitespaceFacet.class);
        STRING_FACETS.add(AssertionFacet.class);
        STRING_FACETS.add(PreprocessFacet.class);

        BOOLEAN_FACETS.add(PatternFacet.class);
        BOOLEAN_FACETS.add(WhitespaceFacet.class);
        BOOLEAN_FACETS.add(AssertionFacet.class);
        BOOLEAN_FACETS.add(PreprocessFacet.class);

        ORDERED_FACETS.add(PatternFacet.class);
        ORDERED_FACETS.add(EnumerationFacet.class);
        ORDERED_FACETS.add(EnumerationFacetSet.class);
        ORDERED_FACETS.add(WhitespaceFacet.class);
        ORDERED_FACETS.add(MinInclusiveFacet.class);
        ORDERED_FACETS.add(MaxInclusiveFacet.class);
        ORDERED_FACETS.add(MinExclusiveFacet.class);
        ORDERED_FACETS.add(MaxExclusiveFacet.class);
        ORDERED_FACETS.add(AssertionFacet.class);
        ORDERED_FACETS.add(PreprocessFacet.class);

        CALENDAR_FACETS.add(PatternFacet.class);
        CALENDAR_FACETS.add(EnumerationFacet.class);
        CALENDAR_FACETS.add(EnumerationFacetSet.class);
        CALENDAR_FACETS.add(WhitespaceFacet.class);
        CALENDAR_FACETS.add(MinInclusiveFacet.class);
        CALENDAR_FACETS.add(MaxInclusiveFacet.class);
        CALENDAR_FACETS.add(MinExclusiveFacet.class);
        CALENDAR_FACETS.add(MaxExclusiveFacet.class);
        CALENDAR_FACETS.add(ExplicitTimezoneFacet.class);
        CALENDAR_FACETS.add(AssertionFacet.class);
        CALENDAR_FACETS.add(PreprocessFacet.class);

        DECIMAL_FACETS.add(TotalDigitsFacet.class);
        DECIMAL_FACETS.add(FractionDigitsFacet.class);
        DECIMAL_FACETS.add(PatternFacet.class);
        DECIMAL_FACETS.add(EnumerationFacet.class);
        DECIMAL_FACETS.add(EnumerationFacetSet.class);
        DECIMAL_FACETS.add(WhitespaceFacet.class);
        DECIMAL_FACETS.add(MinInclusiveFacet.class);
        DECIMAL_FACETS.add(MaxInclusiveFacet.class);
        DECIMAL_FACETS.add(MinExclusiveFacet.class);
        DECIMAL_FACETS.add(MaxExclusiveFacet.class);
        DECIMAL_FACETS.add(AssertionFacet.class);
        DECIMAL_FACETS.add(PreprocessFacet.class);

    }

    /**
     * Create a UserAtomicType
     *
     * @param config the Saxon configuration
     */

    public UserAtomicType(EnterpriseConfiguration config) {
        setConfiguration(config);
    }

    /**
     * Test whether this Simple Type is an atomic type
     *
     * @return true, this is an atomic type
     */

    @Override
    public boolean isAtomicType() {
        return true;
    }

    /**
     * Ask whether this is a plain type (a type whose instances are always atomic values)
     *
     * @return true
     */

    @Override
    public boolean isPlainType() {
        return true;
    }

    /**
     * Ask whether this type is an ID type. This is defined to be any simple type
     * who typed value may contain atomic values of type xs:ID: that is, it includes types derived
     * from ID by restriction, list, or union. Note that for a node to be treated
     * as an ID, its typed value must be a *single* atomic value of type ID; the type of the
     * node, however, can still allow a list.
     */

    @Override
    public boolean isIdType() {
        return getBuiltInBaseType().isIdType();
    }

    /**
     * Ask whether this type is an IDREF or IDREFS type. This is defined to be any simple type
     * who typed value may contain atomic values of type xs:IDREF: that is, it includes types derived
     * from IDREF or IDREFS by restriction, list, or union
     */

    @Override
    public boolean isIdRefType() throws MissingComponentException {
        return getBaseType().isIdRefType();
    }

    /**
     * Determine whether this is a built-in type or a user-defined type
     */

    @Override
    public boolean isBuiltInType() {
        return false;
    }



    /**
     * Get an alphabetic code representing the type, or at any rate, the nearest built-in type
     * from which this type is derived. The codes are designed so that for any two built-in types
     * A and B, alphaCode(A) is a prefix of alphaCode(B) if and only if A is a supertype of B.
     *
     * @return the alphacode for the nearest containing built-in type
     */
    @Override
    public String getBasicAlphaCode() {
        return ((AtomicType)getBuiltInBaseType()).getBasicAlphaCode();
    }

    /**
     * Get the name of this type as a StructuredQName, unless the type is anonymous, in which case
     * return null
     *
     * @return the name of the atomic type, or null if the type is anonymous.
     */

//    public StructuredQName getTypeName() {
//        if (isAnonymousType()) {
//            return null;
//        } else {
//            return this.getTypeName();
//        }
//    }

    /**
     * Determine whether the atomic type is a primitive type.  The primitive types are
     * the 19 primitive types of XML Schema, plus xs:integer, xs:dayTimeDuration and xs:yearMonthDuration;
     * xs:untypedAtomic; and all supertypes of these (xs:anyAtomicType, xs:numeric, ...)
     *
     * @return true if the type is considered primitive under the above rules
     */

    @Override
    public boolean isPrimitiveType() {
        return false;
    }

    /**
     * Get the corresponding {@link net.sf.saxon.type.UType}. A UType is a union of primitive item
     * types.
     *
     * @return the smallest UType that subsumes this item type
     */
    @Override
    public UType getUType() {
        return getPrimitiveItemType().getUType();
    }

    /**
     * Determine whether the atomic type is ordered, that is, whether less-than and greater-than comparisons
     * are permitted
     *
     * @param optimistic  if true, the function takes an optimistic view, returning true if ordering comparisons
     *                   are available for some subtype. This mainly affects xs:duration, where the function returns true if
     *                   optimistic is true, false if it is false.
     * @return true if ordering operations are permitted
     */

    @Override
    public boolean isOrdered(boolean optimistic) {
        return getPrimitiveItemType().isOrdered(false);
    }


    /**
     * Determine whether the type is abstract, that is, whether it cannot have instances that are not also
     * instances of some concrete subtype
     *
     * @return false (always)
     */

    @Override
    public boolean isAbstract() {
        return false;
    }

    /**
     * Get a StringConverter, an object which converts strings in the lexical space of this
     * data type to instances (in the value space) of the data type.
     *
     * @param rules the conversion rules to be used
     * @return a StringConverter to do the conversion, or null if no built-in converter is available.
     */
    @Override
    public StringConverter getStringConverter(ConversionRules rules) {
        if (stringConverter == null) {
            StringConverter sc;
            AtomicType prim = getPrimitiveItemType();
            if (prim.getFingerprint() == StandardNames.XS_STRING) {
                if (getBuiltInBaseType() == BuiltInAtomicType.STRING) {
                    // converter to user-defined subtypes of xs:string
                    sc = new StringConverter.StringToStringSubtype(rules, this);
                } else {
                    // converter to user-defined subtypes of built-in subtypes of xs:string
                    sc = new StringConverter.StringToDerivedStringSubtype(rules, this);
                }
            } else {
                // converter to user-defined types derived from types other than xs:string
                StringConverter first = getPrimitiveItemType().getStringConverter(rules);
                Converter.DownCastingConverter second = new Converter.DownCastingConverter(this, rules);
                sc = new StringConverter.StringToNonStringDerivedType(first, second);
            }
            if (rules instanceof ValidationContext && ((ValidationContext)rules).hasValidationParams()) {
                // In this case we cannot cache the stringconverter because the validation parameters may change
                return sc;
            } else {
                stringConverter = sc;
            }
        }
        return stringConverter;
    }

    /**
     * Checks the validity of this SimpleType definition.
     *
     * @throws net.sf.saxon.type.SchemaException
     *          when this SimpleType definition
     *          is invalid.
     */
    @Override
    public boolean validate(SchemaCompiler compiler) throws SchemaException {
        if (!super.validate(compiler)) {
            return false;
        }
        if (getBaseType() instanceof SimpleTypeDefinition) {
            SimpleType simpleType = ((SimpleTypeDefinition) getBaseType()).getWorkingType();
            getBaseTypeReference().setTarget(simpleType);
        }
        AtomicType prim = getPrimitiveItemType();
        Set<Class<? extends Facet>> allowed = null;
        // See XML Schema Part 2 section 4.1.5
        switch (prim.getFingerprint()) {
            case StandardNames.XS_STRING:
            case StandardNames.XS_HEX_BINARY:
            case StandardNames.XS_BASE64_BINARY:
            case StandardNames.XS_ANY_URI:
            case StandardNames.XS_QNAME:
            case StandardNames.XS_NOTATION:
                allowed = STRING_FACETS;
                break;
            case StandardNames.XS_BOOLEAN:
                allowed = BOOLEAN_FACETS;
                break;
            case StandardNames.XS_FLOAT:
            case StandardNames.XS_DOUBLE:
            case StandardNames.XS_DURATION:
            case StandardNames.XS_DAY_TIME_DURATION:
            case StandardNames.XS_YEAR_MONTH_DURATION:
                allowed = ORDERED_FACETS;
                break;
            case StandardNames.XS_DATE_TIME:
            case StandardNames.XS_TIME:
            case StandardNames.XS_DATE:
            case StandardNames.XS_G_YEAR_MONTH:
            case StandardNames.XS_G_YEAR:
            case StandardNames.XS_G_MONTH:
            case StandardNames.XS_G_MONTH_DAY:
            case StandardNames.XS_G_DAY:
                allowed = CALENDAR_FACETS;
                break;
            case StandardNames.XS_DECIMAL:
            case StandardNames.XS_INTEGER:
                allowed = DECIMAL_FACETS;
                break;
        }
        for (Facet fac : getExtendedFacetList()) {
            if (allowed == null || !allowed.contains(fac.getClass())) {
                compiler.error("The " + fac.getName() + " facet is not applicable to types derived from " +
                        prim.getDisplayName(), this);
                return false;
            }
        }
        return true;
    }

    /**
     * Test whether a given item conforms to this type
     *
     *
     *
     * @param item    The item to be tested
     * @param th  the type hierarchy cache
     * @return true if the item is an instance of this type; false otherwise
     */
    @Override
    public boolean matches(Item item, TypeHierarchy th) {
        return item instanceof AtomicValue && Type.isSubType(((AtomicValue) item).getItemType(), this);
    }


    /**
     * Check whether a given input string is valid according to this SimpleType
     *
     * @param value      the input string to be checked
     * @param nsResolver a namespace resolver used to resolve namespace prefixes if the type
     *                   is namespace sensitive. The value supplied may be null; in this case any namespace-sensitive
     *                   content will throw an UnsupportedOperationException.
     * @param rules      the conversion rules for this Configuration
     * @return null if validation succeeds; return a ValidationFailure describing the validation failure
     *         if validation fails.
     * @throws UnsupportedOperationException if the type is namespace-sensitive and no namespace
     *                                       resolver is supplied
     */

    /*@Nullable*/
    @Override
    public ValidationFailure validateContent(
            /*@NotNull*/ UnicodeString value, /*@Nullable*/ NamespaceResolver nsResolver, /*@NotNull*/ ConversionRules rules) {
        StringConverter converter = getStringConverter(rules);
        if (isNamespaceSensitive()) {
            if (nsResolver == null) {
                throw new UnsupportedOperationException("Cannot validate a QName without a namespace resolver");
            }
            converter = (StringConverter) converter.setNamespaceResolver(nsResolver);
        }
        return converter.validate(value);
    }

    /**
     * Get the typed value corresponding to a given string value, assuming it is
     * valid against this type
     *
     * @param value    the string value
     * @param resolver the namespace resolver
     * @param rules    the conversion rules for the Saxon configuration
     * @return an iterator over the atomic sequence comprising the typed value
     */

    @Override
    public AtomicValue getTypedValue(UnicodeString value, NamespaceResolver resolver, ConversionRules rules)
            throws ValidationException {
        StringConverter converter = getStringConverter(rules);
        assert converter != null;
        if (isNamespaceSensitive()) {
            converter = (StringConverter)converter.setNamespaceResolver(resolver);
        }
        return converter.convertString(value).asAtomic();
    }

    /**
     * Get the primitive type from which this type is derived. For the definition
     * of primitive types, see {@link Type#isPrimitiveAtomicType}
     *
     * @return the type code of the primitive type
     */

    /*@NotNull*/
    @Override
    public AtomicType getPrimitiveItemType() {
        try {
            ItemType s = (ItemType) getBaseType();
            if (s.isPlainType()) {
                return (AtomicType)s.getPrimitiveItemType();
            } else {
                return this;
            }
        } catch (ClassCastException err) {
            // A user-defined type can be a restriction of xs:anySimpleType
            if (getBaseType() == AnySimpleType.getInstance()) {
                return this;
            }
            // Exceptionally, when processing the S4S, a UserAtomicType can be a primitive type
            if (Type.isPrimitiveAtomicType(getFingerprint())) {
                return this;
            }
            getConfiguration().getLogger().info("Internal failure on type " + getDescription());
            throw err;
        }
    }


    /**
     * Get the primitive type from which this type is derived. For the definition
     * of primitive types, see {@link Type#isPrimitiveAtomicType}
     *
     * @return the type code of the primitive type
     */
    @Override
    public int getPrimitiveType() {
        SchemaType s = getBaseType();
        if (s.isAtomicType()) {
            return ((AtomicType) s).getPrimitiveType();
        } else {
            return getFingerprint();
        }
    }

    /**
     * Test whether this simple type is namespace-sensitive, that is, whether
     * it is derived from xs:QName or xs:NOTATION
     *
     * @return true if this type is derived from xs:QName or xs:NOTATION
     */

    @Override
    public boolean isNamespaceSensitive() {
        // This is implemented as a memo function
        if (!namespaceSensitive.isPresent()) {
            SimpleType base = this;
            int fp = base.getFingerprint();
            while (fp > 1023) {
                SchemaType b = base.getBaseType();
                if (b instanceof ComplexType) {
                    try {
                        base = ((ComplexType) b).getSimpleContentType();
                    } catch (MissingComponentException e) {
                        return true;
                    }
                } else {
                    base = (SimpleType) b;
                }
                fp = base.getFingerprint();
            }
            boolean sensitive = fp == StandardNames.XS_QNAME || fp == StandardNames.XS_NOTATION;
            namespaceSensitive = Optional.of(sensitive);
            return sensitive;
        } else {
            return namespaceSensitive.get();
        }
    }


    /**
     * Get the item type of the atomic values that will be produced when an item
     * of this type is atomized
     */

    /*@NotNull*/
    @Override
    public AtomicType getAtomizedItemType() {
        return this;
    }

    /**
     * Ask whether values of this type are atomizable
     *
     * @return true unless it is known that these items will be elements with element-only
     *         content, in which case return false
     * @param th The type hierarchy cache
     */

    @Override
    public boolean isAtomizable(TypeHierarchy th) {
        return true;
    }

    /**
     * Get an instance of the facet value that is stored in the schema rather than the instance
     *
     * @param value the value required
     * @return a string which is equal to the value required, but which if possible is the instance
     *         held in the compiled schema, reducing storage requirements in instances.
     */

    public String getSharedInstance(String value) {
        EnumerationFacetSet facet = (EnumerationFacetSet) getFacet(EnumerationFacet.class);
        if (facet == null) {
            return value;
        }
        return facet.getSharedInstance(value);
    }

    /**
     * Produce a string representation of the type name. If the type is anonymous, an internally-allocated
     * type name will be returned.
     *
     * @return the name of the atomic type in the form Q{uri}local
     */

    public String toString() {
        return this.getTypeName().getEQName();
    }

    /**
     * Return a string representation of this ItemType suitable for use in stylesheet
     * export files. This differs from the result of toString() in that it will not contain
     * any references to anonymous types. Note that it may also use the Saxon extended syntax
     * for union types and tuple types. The default implementation returns the result of
     * calling {@code toString()}.
     *
     * @return the string representation as an instance of the XPath SequenceType construct
     */
    @Override
    public String toExportString() {
        if (isAnonymousType()) {
            return ((AtomicType) TypeHierarchy.getNearestNamedType(this)).toExportString();
        } else {
            return toString();
        }
    }

    /**
     * Returns a hash code value for the object.
     */

    public int hashCode() {
        return getFingerprint();
    }

    /**
     * Validate that a primitive atomic value is a valid instance of a type derived from the
     * same primitive type.
     *
     * @param primValue    the value in the value space of the primitive type.
     * @param lexicalValue the value in the lexical space. If null, the string value of primValue
     *                     is used. This value is checked against the pattern facet (if any)
     * @param rules        the conversion rules used for this configuration
     * @return null if the value is valid; otherwise, a ValidationFailure object indicating
     *         the nature of the error.
     * @throws UnsupportedOperationException in the case of an external object type
     */

    @Override
    public ValidationFailure validate(AtomicValue primValue, UnicodeString lexicalValue, ConversionRules rules) {
        BuiltInAtomicType base = (BuiltInAtomicType) getBuiltInBaseType();
        if (!base.isPrimitiveType()) {
            ValidationFailure vf = base.validate(primValue, lexicalValue, rules);
            if (vf != null) {
                return vf;
            }
        }
        return checkAgainstFacets(primValue, lexicalValue, getExtendedFacetList(), rules);
    }

    /**
     * Serialize the schema component
     */

    @Override
    protected void serializeVariety(SchemaModelSerializer serializer) throws XPathException {
        serializer.emitAttribute("variety", "atomic");
        serializer.emitAttribute("primitiveType", "#" + getPrimitiveItemType().getName());
    }

    /**
     * Get the set of atomic types that are subsumed by this type
     *
     * @return for an atomic type, the type itself; for a union type, the set of atomic types
     *         in its transitive membership
     */
    @Override
    @CSharpReplaceBody(code="return Saxon.Ejava.util.Collections.singletonList<Saxon.Hej.type.PlainType>(this);")
    public List<? extends PlainType> getPlainMemberTypes() {
        return Collections.singletonList(this);
    }


}

// Copyright (c) 2004-2023 Saxonica Limited