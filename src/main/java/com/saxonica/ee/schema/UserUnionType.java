package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.sdoc.SimpleTypeDefinition;
import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.om.*;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.type.coercion.CoercionPlan;
import net.sf.saxon.type.coercion.UnionCoercionPlan;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.Whitespace;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Stack;

import static net.sf.saxon.type.SchemaValidationStatus.VALIDATED;
import static net.sf.saxon.type.SchemaValidationStatus.VALIDATING;

/**
 * A class that represents the XML Schema simple-type with variety union.
 * <p>A "plain" union is a union that is not derived by restriction and whose
 * member types are all either atomic types or plain union types. Only plain union
 * types can be named in the XPath SequenceType production. (All union types, however,
 * can act as the target of a cast expression). This class implements the interface
 * {@link PlainType} despite the fact that not all its instances represent plain
 * unions; to test whether a union is a plain union, use the {@link #isPlainType()}
 * method.
 * </p>
 */

public class UserUnionType extends UserSimpleType implements PlainType, UnionType {

    /**
     * Get the genre of this item
     *
     * @return the genre
     */
    @Override
    public Genre getGenre() {
        return Genre.ATOMIC;
    }

//    public StructuredQName getTypeName() {
//        return new StructuredQName("", getTargetNamespace(), getName());
//    }

    /**
     * Get the name of the type as a StructuredQName
     *
     * @return a StructuredQName identifying the type.  In the case of an anonymous type, an internally-generated
     * name is returned
     */
    @Override
    public StructuredQName getStructuredQName() {
        return getTypeName();
    }

    /**
     * The members of this Union. The members of this list are TypeReference objects
     */

    private List<TypeReference> memberTypeReferences = null;

    /**
     * Flag indicating whether this is a "plain union". Values -1: not yet computed, 0: false, 1: true
     */

    private int isPlainUnion = -1;

    /**
     * Creates a new Union type.
     *
     * @param config the Configuration for this Union (Cannot be null)
     */

    public UserUnionType(EnterpriseConfiguration config) {
        setConfiguration(config);
        memberTypeReferences = new ArrayList<>(3);
    }

    /**
     * Set the member types of this union type.
     *
     * @param members a list of {@link TypeReference} objects
     */

    public void setMemberTypeReferences(List<TypeReference> members) {
        memberTypeReferences = members;
    }

    /**
     * Add a member type to the list of member types
     *
     * @param member a reference to a new member type
     */

    public void addMemberTypeReference(TypeReference member) {
        if (memberTypeReferences == null) {
            memberTypeReferences = new ArrayList<>(5);
        }
        memberTypeReferences.add(member);
    }

    /**
     * Get the list of member types
     *
     * @return a List containing {@link TypeReference} objects
     */

    public List<TypeReference> getMemberTypeReferences() {
        if (memberTypeReferences == null) {
            // either the union is empty,
            // or it is derived by restriction from another union
            if (getBaseType() instanceof UserUnionType) {
                memberTypeReferences = ((UserUnionType) getBaseType()).getMemberTypeReferences();
            } else {
                memberTypeReferences = Collections.emptyList();
            }
        }
        return memberTypeReferences;
    }

    /**
     * Ask whether this Simple Type is an atomic type
     *
     * @return false, this is not an atomic type
     */

    @Override
    public boolean isAtomicType() {
        return false;
    }

    /**
     * Ask whether this union type includes any list types among its members
     */

    @Override
    public boolean containsListType() throws MissingComponentException {
        for (TypeReference ref : getMemberTypeReferences()) {
            SimpleType type = (SimpleType) ref.getTarget();
            if (type.isListType()) {
                return true;
            } else if (type.isUnionType()) {
                return ((UserUnionType) type).containsListType();
            }
        }
        return false;
    }

    /**
     * Ask whether this Union type is a "plain type", defined as a union
     * type whose member types are all atomic types or plain unions. That is,
     * it disallows unions that are derived by restriction from another union.
     * The significance of this is that an atomic value will never match
     * a non-plain union type
     */

    @Override
    public boolean isPlainType() {
        // implemented as a memo function
        if (isPlainUnion == -1) {
            if (((SimpleType) getBaseType()).isUnionType()) {
                isPlainUnion = 0;
                return false;
            }
            for (TypeReference ref : getMemberTypeReferences()) {
                try {
                    SimpleType st = (SimpleType) ref.getTarget();
                    if (!(st instanceof ItemType && ((ItemType) st).isPlainType())) {
                        isPlainUnion = 0;
                        return false;
                    }
                } catch (MissingComponentException e) {
                    return false;
                }
            }
            isPlainUnion = 1;
        }
        return isPlainUnion == 1;
    }

    /**
     * Get the SequenceType that most accurately describes the result of casting a value to this union type
     *
     * @return the most precise SequenceType for the result of casting
     */

    @Override
    public SequenceType getResultTypeOfCast() {
        try {
            if (isPlainType()) {
                return SequenceType.makeSequenceType(this, StaticProperty.ALLOWS_ZERO_OR_ONE);
            } else if (containsListType()) {
                // can do better than this
                return SequenceType.ATOMIC_SEQUENCE;
            } else {
                return ((UserUnionType) getBaseType()).getResultTypeOfCast();
            }
        } catch (MissingComponentException e) {
            return SequenceType.ATOMIC_SEQUENCE;
        }
    }

    /**
     * Ask whether this type is an ID type. This is defined to be any simple type
     * who typed value may contain atomic values of type xs:ID: that is, it includes types derived
     * from ID by restriction, list, or union. Note that for a node to be treated
     * as an ID in XSD 1.0, its typed value must be a *single* atomic value of type ID; the type of the
     * node, however, can still allow a list or union. This changes in XSD 1.1, where a list of IDs is allowed.
     */

    @Override
    public boolean isIdType() throws MissingComponentException {
        for (TypeReference ref : getMemberTypeReferences()) {
            SimpleType st = (SimpleType) ref.getTarget();
            if (st.isIdType()) {
                return true;
            }
        }
        return false;
    }


    /**
     * Ask whether this type is an IDREF or IDREFS type. This is defined to be any simple type
     * who typed value may contain atomic values of type xs:IDREF: that is, it includes types derived
     * from IDREF or IDREFS by restriction, list, or union
     */

    @Override
    public boolean isIdRefType() throws MissingComponentException {
        for (TypeReference ref : getMemberTypeReferences()) {
            SimpleType st = (SimpleType) ref.getTarget();
            if (st.isIdRefType()) {
                return true;
            }
        }
        return false;
    }

    /**
     * Determine whether this is a built-in type or a user-defined type
     */

    @Override
    public boolean isBuiltInType() {
        return false;
    }

    /**
     * Determine whether this is a list type
     */

    @Override
    public boolean isListType() {
        return false;
    }

    /**
     * Return true if this type is a union type (that is, if its variety is union)
     *
     * @return true for a union type
     */

    @Override
    public boolean isUnionType() {
        return true;
    }

    /**
     * Get the corresponding {@link net.sf.saxon.type.UType}. A UType is a union of primitive item
     * types.
     *
     * @return the smallest UType that subsumes this item type
     */
    @Override
    public UType getUType() {
        UType u = UType.VOID;
        for (TypeReference ref : getMemberTypeReferences()) {
            try {
                SimpleType st = (SimpleType) ref.getTarget();
                if (st instanceof PlainType) {
                    u = u.union(((PlainType) st).getUType());
                } else {
                    throw new UnsupportedOperationException("Union type is not a plain type");
                }
            } catch (MissingComponentException e) {
                throw new UnsupportedOperationException("Union type is not a plain type. " + e.getMessage());
            }
        }
        return u;
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
        return "A";
    }

    /**
     * Test whether this type is namespace sensitive, that is, if a namespace context is needed
     * to translate between the lexical space and the value space. This is true for types derived
     * from, or containing, QNames and NOTATIONs
     *
     * @return true if any of the member types is namespace-sensitive, or if namespace sensitivity
     * cannot be determined because there are components missing from the schema.
     */

    @Override
    public boolean isNamespaceSensitive() {
        try {
            for (TypeReference ref : getMemberTypeReferences()) {
                SimpleType st = (SimpleType) ref.getTarget();
                if (st.isNamespaceSensitive()) {
                    return true;
                }
            }
        } catch (MissingComponentException e) {
            return true;
        }
        return false;
    }

    /**
     * Validate this Union, replacing all unresolved forwards
     * references at the same time
     *
     * @throws SchemaException if the definition is invalid
     */

    @Override
    public boolean validate(SchemaCompiler compiler)
            throws SchemaException {

        boolean result = true;

        // check for circularities and for previous validation
        switch (getValidationStatus()) {
            case VALIDATED:
            case INVALID:
                return true;
            case VALIDATING:
                compiler.error("The definition of the union type " +
                        getDescription() + " is circular", this);
                return false;
            case UNVALIDATED:
                break;
        }

        super.validate(compiler);
        setValidationStatus(VALIDATING);

        getMemberTypeReferences();

        lookForCycles(new Stack<SchemaComponent>(), compiler);
        setValidationStatus(VALIDATED);
        return result;
    }

    @Override
    public void elaborate(SchemaCompiler compiler) throws SchemaException {
        // before elaborating a union type we must elaborate its member types
        for (TypeReference ref : getMemberTypeReferences()) {
            SimpleType member = (SimpleType) ref.getTarget();
            if (member instanceof UserSimpleType) {
                ((UserSimpleType) member).elaborate(compiler);
            }
        }
        super.elaborate(compiler);
    }

    /**
     * This method is called to look for cycles. The object implementing this
     * method is required (a) to raise an exception if the object itself appears
     * in the list of references, (b) to add itself to the list of references, and
     * (c) to call the lookForCycles method on all the objects that it references.
     *
     * @param references A list of objects that contain direct or indirect references
     *                   to this object, and that must therefore not be referred to from this object.
     * @param compiler   used for error reporting
     * @throws SchemaException if cycles are found
     */

    @Override
    public void lookForCycles(Stack<SchemaComponent> references, SchemaCompiler compiler) throws SchemaException {
        for (Object reference : references) {
            if (((SchemaType) reference).isSameType(this)) {
                compiler.error("The definition of the union type " + getDescription() +
                        " is circular", this);
                throw new SchemaException("Circular definition found");
            }
        }
        references.push(this);
        for (TypeReference ref : getMemberTypeReferences()) {
            ref.tryToResolve(compiler, false);
            if (ref.isResolved()) {
                SchemaType st = (SchemaType) ref.getTarget();
                if (st instanceof SimpleTypeDefinition) {
                    ((SimpleTypeDefinition) st).fixup(compiler);
                    st = ((SimpleTypeDefinition) st).getWorkingType();
                }
                if (st instanceof UserUnionType) {
                    ((UserUnionType) st).lookForCycles(references, compiler);
                    while (true) {
                        SchemaType stb = st.getBaseType();
                        if (stb instanceof UserUnionType) {
                            ((UserUnionType) stb).lookForCycles(references, compiler);
                            st = stb;
                        } else {
                            break;
                        }
                    }
                } else if (st instanceof UserListType) {
                    SimpleType it = ((UserListType) st).getItemType();
                    if (it instanceof SimpleTypeDefinition) {
                        ((SimpleTypeDefinition) it).fixup(compiler);
                        it = ((SimpleTypeDefinition) it).getWorkingType();
                    }
                    if (it.isSameType(this)) {
                        compiler.error("The definition of the union type " + getDescription() +
                                " is circular", this);
                        throw new SchemaException("Circular definition found");
                    } else if (it instanceof UserUnionType) {
                        ((UserUnionType) it).lookForCycles(references, compiler);
                        while (true) {
                            SchemaType stb = it.getBaseType();
                            if (stb instanceof UserUnionType) {
                                ((UserUnionType) stb).lookForCycles(references, compiler);
                                it = (UserUnionType) stb;
                            } else {
                                break;
                            }
                        }

                    }
                }
            }
        }
        references.pop();
    }


    /**
     * Check whether a given input string is valid according to this SimpleType
     *
     * @param value      the input string to be checked
     * @param nsResolver a namespace resolver used to resolve namespace prefixes if the type
     *                   is namespace sensitive. The value supplied may be null; in this case any namespace-sensitive
     *                   content will throw an UnsupportedOperationException.
     * @param rules      the configuration-wide conversion rules
     * @return null if validation succeeds; return a ValidationFailure describing the validation failure
     *         if validation fails
     * @throws UnsupportedOperationException if the type is namespace-sensitive and no namespace
     *                                       resolver is supplied
     */

    /*@Nullable*/
    @Override
    public ValidationFailure validateContent(/*@NotNull*/ UnicodeString value, NamespaceResolver nsResolver, /*@NotNull*/ ConversionRules rules) {
        ValidationFailure result = null;
        SimpleType matchingType = null;
        for (TypeReference ref : getMemberTypeReferences()) {
            SimpleType type = null;
            try {
                type = (SimpleType) ref.getTarget();
            } catch (MissingComponentException e) {
                result = new ValidationFailure(
                    "Cannot validate against union type because the schema is incomplete. " + e.getMessage());
            }
            if (type != null) {
                ValidationFailure err = type.validateContent(value, nsResolver, rules);
                if (err == null) {
                    matchingType = type;
                    break;
                }
            }
        }
        if (matchingType != null) {
            // A union type can have enumeration and pattern facets. To check against these we
            // need first to get the typed value as an instance of the base type. This is only necessary if
            // this union type is derived by restriction from another union type, so we check for that first.
            SchemaType base = getBaseType();
            if (base instanceof UserUnionType) {
                try {
                    //SimpleType member = ((UserUnionType) base).getMatchingMemberType(value, nsResolver, rules);
                    // The challenge now (see bug 2247) is that we need to normalize the lexical value against
                    // the whitespace facet of whichever member type validation succeeded against
                    int action = matchingType.getWhitespaceAction();
                    value = Whitespace.applyWhitespaceNormalization(action, value);
                    AtomicSequence typedValue = matchingType.getTypedValue(value, nsResolver, rules);
                    result = checkAgainstFacets(typedValue, value, getExtendedFacetList(), rules);
                } catch (XPathException e) {
                    // shouldn't happen, but it's easier to deal with it in case it does.
                    result = new ValidationFailure("Value " + Err.wrap(value, Err.VALUE) +
                            " does not match base type of union type " + getDescription());
                }
            } else {
                result = null;
            }
        } else if (result == null) {
            result = new ValidationFailure(
                    "Value " + Err.wrap(value, Err.VALUE) +
                            " does not match any member of union type " + getDescription());
        }
        return result;
    }

    /**
     * Validate an atomic value, which is known to be an instance of one of the member types of the
     * union, against any facets (pattern facets or enumeration facets) defined at the level of the
     * union itself.
     *
     * @param value the Atomic Value to be checked. This must be an instance of a member type of the
     *              union
     * @param rules the ConversionRules for the Configuration
     * @return a ValidationFailure if the value is not valid; null if it is valid.
     */
    @Override
    public ValidationFailure checkAgainstFacets(AtomicValue value, ConversionRules rules) {
        return checkAgainstFacets(value, value.getCanonicalLexicalRepresentation(), getExtendedFacetList(), rules);
    }

    private SimpleType getMatchingMemberType(UnicodeString value, NamespaceResolver resolver, ConversionRules rules) {
        for (TypeReference ref : getMemberTypeReferences()) {
            try {
                // This method actually attempts conversion to each member type in turn; when validation
                // succeeds, it returns that type
                SimpleType type = (SimpleType) ref.getTarget();
                ValidationFailure err = type.validateContent(value, resolver, rules);
                if (err == null) {
                    // if it validates, use this type to get the typed value
                    return type;
                }
            } catch (MissingComponentException e) {
                // ignore the failure
            }
        }
        return null;
    }


    @Override
    public AtomicSequence getTypedValue(UnicodeString value, NamespaceResolver resolver, ConversionRules rules)
            throws ValidationException {
        AtomicSequence result = null;
        for (TypeReference ref : getMemberTypeReferences()) {
            // This method actually attempts conversion to each member type in turn; when validation
            // succeeds, it then repeats the conversion for the type that succeeded. This is wasteful,
            // but better than throwing exceptions for the conversions that don't succeed.
            SimpleType type = null;
            try {
                type = (SimpleType) ref.getTarget();
            } catch (MissingComponentException e) {
                ValidationFailure ve = new ValidationFailure(
                    "Cannot validate against union type because the schema is incomplete. " + e.getMessage());
                ve.setSchemaType(this);
                throw ve.makeException();
            }
            try {
                ValidationFailure err = type.validateContent(value, resolver, rules);
                if (err == null) {
                    // if it validates, use this type to get the typed value
                    result = type.getTypedValue(value, resolver, rules);
                    break;
                }
            } catch (ValidationException err) {
                // validation errors shouldn't be thrown, but if they are then we ignore them and test
                // against other types
            }
        }

        if (result != null) {
            // A union type can have enumeration and pattern facets. To check against these we
            // need first to get the typed value as an instance of the base type. This is only necessary if
            // this union type is derived by restriction from another union type, so we check for that first.
            SchemaType base = getBaseType();
            if (base instanceof UserUnionType) {
                AtomicSequence typedValue = ((UserUnionType) base).getTypedValue(value, resolver, rules);
                ValidationFailure failure = checkAgainstFacets(typedValue, value, getExtendedFacetList(), rules);
                if (failure != null) {
                    throw failure.makeException();
                }
            }
            return result;
        } else {

            ValidationFailure ve = new ValidationFailure(
                    "Value " + Err.wrap(value, Err.VALUE) +
                            " does not match any member of union type " + getDescription());
            ve.setSchemaType(this);
            throw ve.makeException();
        }
    }

    /**
     * Serialize the schema component
     */

    @Override
    protected void serializeVariety(SchemaModelSerializer serializer) throws XPathException {
        serializer.emitAttribute("variety", "union");
        StringBuilder sb = new StringBuilder(64);
        for (TypeReference ref : getMemberTypeReferences()) {
            SimpleType type = (SimpleType) ref.getTarget();
            if (sb.length() != 0) {
                sb.append(" ");
            }
            sb.append(serializer.getTypeLink(type));
        }
        serializer.emitAttribute("memberTypes", sb.toString());
    }

    /**
     * Test whether a given item conforms to this type
     *
     *
     *
     * @param item    The item to be tested
     * @param th   The type hierarchy
     * @return true if the item is an instance of this type; false otherwise
     */
    @Override
    public boolean matches(Item item, TypeHierarchy th) {
        if (!(item instanceof AtomicValue)) {
            return false;
        }
        for (TypeReference ref : getMemberTypeReferences()) {
            try {
                PlainType pt = (PlainType) ref.getTarget();
                if (pt.matches(item, th)) {
                    return true;
                }
            } catch (MissingComponentException e) {
                return false;
            }
        }
        return false;
    }


    /**
     * Method defined in ItemType: get a primitive supertype in the ItemType type hierarchy
     *
     * @return BuiltInAtomicType.ANY_ATOMIC
     */

    /*@NotNull*/
    @Override
    public AtomicType getPrimitiveItemType() {
        return BuiltInAtomicType.ANY_ATOMIC;
    }

    /**
     * Method defined in ItemType: get a primitive supertype in the ItemType type hierarchy
     *
     * @return StandardNames.XS_ANY_ATOMIC_TYPE
     */

    @Override
    public int getPrimitiveType() {
        return StandardNames.XS_ANY_ATOMIC_TYPE;
    }

    /*@NotNull*/
    @Override
    public PlainType getAtomizedItemType() {
        return this;
    }

    @Override
    public boolean isAtomizable(TypeHierarchy th) {
        return true;
    }

    /**
     * Get the list of plain types that are subsumed by this type
     *
     * @return for an atomic type, the type itself; for a plain union type, the list of plain types
     *         in its transitive membership, in declaration order
     */
    @Override
    public List<? extends PlainType> getPlainMemberTypes() throws MissingComponentException {
        List<PlainType> s = new ArrayList<>(10);
        gatherPlainMemberTypes(s);
        return s;
    }

    private void gatherPlainMemberTypes(List<PlainType> s) throws MissingComponentException {
        for (TypeReference ref : getMemberTypeReferences()) {
            SimpleType pt = (SimpleType) ref.getTarget();
            if (pt instanceof PlainType) {
                s.add((PlainType) pt);
            }
            if (pt instanceof UserUnionType) {
                ((UserUnionType) pt).gatherPlainMemberTypes(s);
            }
        }
    }

    /**
     * Get the default priority when this ItemType is used as an XSLT pattern
     *
     * @return the default priority. For a union type this is defined as the product
     * of the default priorities of the member types. Because the priorities of member
     * types are in the range 0 to 1, their product is also in the range 0 to 1.
     */
    @Override
    public double getDefaultPriority() {
        try {
            double result = 1;
            for (PlainType t : getPlainMemberTypes()) {
                 result *= t.getDefaultPriority();
            }
            return result;
        } catch (MissingComponentException e) {
            return -1;
        }
    }

    /**
     * Produce a string representation of the type name. If the type is anonymous, an internally-allocated
     * type name will be returned.
     *
     * @return the name of the atomic type in the form Q{uri}local
     */

    public String toString() {
        if (isAnonymousType()) {
            StringBuilder fsb = new StringBuilder(256);
            fsb.append("union(");
            for (TypeReference ref : getMemberTypeReferences()) {
                String member = ref.getTargetComponentName().getDisplayName();
                fsb.append(member);
                fsb.append(", ");
            }
            fsb.setLength(fsb.length() - 2);
            fsb.append(")");
            return fsb.toString();
        } else {
            return getDisplayName();
        }
    }

    @Override
    public String toExportString() {
        if (isAnonymousType()) {
            StringBuilder fsb = new StringBuilder(256);
            fsb.append("union(");
            for (TypeReference ref : getMemberTypeReferences()) {
                try {
                    PlainType member = (PlainType)ref.getTarget();
                    fsb.append(member.toExportString());
                } catch (MissingComponentException err) {
                    fsb.append("xs:anyAtomicType");
                }
                fsb.append(", ");
            }
            fsb.setLength(fsb.length() - 2);
            fsb.append(")");
            return fsb.toString();
        } else {
            return getEQName();
        }
    }

    /**
     * Get a description of this type for use in diagnostics. In the case of a named type, this is the
     * same as the display name. In the case of a type known to be defined immediately within an element
     * or attribute declaration, it is a phrase that identifies the containing declaration. In other cases,
     * it is a phrase of the form "defined at line L of URI". The description is designed to be inserted
     * in a context such as "the type X is ..."
     */
    @Override
    public String getDescription() {
        if (isAnonymousType()) {
            return toString();
        } else {
            return super.getDescription();
        }
    }

    /**
     * Get the coercion plan for use when this type is the required type for (say) coercion
     * of arguments in a function call
     *
     * @param version the XPath language version (40 or 31)
     */
    @Override
    public CoercionPlan getCoercionPlan(int version) {
        return UnionCoercionPlan.INSTANCE;
    }
}

// Copyright (c) 2004-2023 Saxonica Limited