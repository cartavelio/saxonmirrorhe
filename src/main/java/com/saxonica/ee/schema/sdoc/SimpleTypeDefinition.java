package com.saxonica.ee.schema.sdoc;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.*;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.NamespaceResolver;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.regex.RegularExpression;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

import static net.sf.saxon.type.SchemaValidationStatus.INVALID;
import static net.sf.saxon.type.SchemaValidationStatus.UNVALIDATED;
import static net.sf.saxon.type.SchemaValidationStatus.VALIDATED;
import static net.sf.saxon.type.SchemaValidationStatus.VALIDATING;

/**
 * A simple type definition holds all the information about a simpleType
 * during the course of schema processing. On completion
 * of schema processing, a more specific SimpleType object is created,
 * called the WorkingType: this will be for example a
 * {@link UserListType}, {@link UserAtomicType}, or {@link UserUnionType}, and the SimpleTypeDefinition is then
 * discarded.
 */
public class SimpleTypeDefinition extends UserSimpleType implements UserSchemaComponent {

    /**
     * If the simple type represents the content type of a complex type with
     * simple content, then the base type is allowed (initially) to be a
     * complex type with simple content; the real base type is the content type
     * of that complex type, but we don't know this in advance
     */

    private boolean isSimpleContent = false;

    /**
     * The item type of a list. Null if this is not a list type.
     */

    private TypeReference itemTypeReference = null;

    /**
     * The token separator (saxon:separator) used for a list type. Null if this is not a list type,
     * or if no separator is specified.
     */

    private RegularExpression tokenSeparator = null;

    /**
     * The member types of a union are represented by a list whose items are
     * objects of type TypeReference
     */

    private List<TypeReference> members = null; // used for union types only

    private UserSimpleType workingType = null;     // the actual SimpleType object created

    /**
     * Create a SimpleTypeDefinition
     *
     * @param config the Saxon configuration
     */

    public SimpleTypeDefinition(EnterpriseConfiguration config) {
        setConfiguration(config);
    }

    /**
     * Set the reference to an item type of this list type
     *
     * @param itemType reference to a type forming the item type of this list type
     */

    public void setItemTypeReference(TypeReference itemType) {
        itemTypeReference = itemType;
    }

    /**
     * Test whether this Simple Type is an atomic type. On a SimpleTypeDefinition, the variety of the type
     * cannot be reliably determined until fixup() has been called to determine the base type.
     *
     * @return true if this type is known to be atomic
     */
    @Override
    public boolean isAtomicType() {
        return !(isListType() || isUnionType());
    }

    /**
     * On a SimpleTypeDefinition, isListType() always returns true if the type is "derived by List".
     * A list type that is derived by restricting another List type cannot be reliably recognized
     * until fixup() has been called to determine the base type.
     *
     * @return true if this type is "derived by List"
     */

    @Override
    public boolean isListType() {
        return itemTypeReference != null ||
                getDerivationMethod() == Derivation.DERIVATION_RESTRICTION &&
                        getBaseTypeReference() != null &&
                        ((SimpleType) getBaseType()).isListType();
    }

    /**
     * On a SimpleTypeDefinition, isUnionType() always returns true if the type is "derived by Union".
     * A union type that is derived by restricting another Union type cannot be reliably recognized
     * until the fixup() method has been called to determine the base type.
     *
     * @return true if this type is "derived by Union"
     */

    @Override
    public boolean isUnionType() {
        return members != null ||
                getDerivationMethod() == Derivation.DERIVATION_RESTRICTION &&
                        getBaseTypeReference() != null &&
                        ((SimpleType) getBaseType()).isUnionType();
    }

    /**
     * Indicate that this simple type represents the content type of a complex type with simple content
     *
     * @param b true if this is the content type of a complex type with simple content
     */

    public void setIsSimpleContent(boolean b) {
        isSimpleContent = b;
    }

    /**
     * Add a member type to this union type
     *
     * @param memberType the member type to be added
     */

    public void addMemberType(TypeReference memberType) {
        if (members == null) {
            members = new ArrayList<TypeReference>(5);
        }
        members.add(memberType);
    }

    public void setTokenSeparator(RegularExpression separator) {
        tokenSeparator = separator;
    }

    /**
     * Get the simple type created for this definition
     *
     * @return the UserSimpleType object if it has been created, otherwise null
     */

    /*@Nullable*/
    public UserSimpleType getWorkingType() {
        return workingType;
    }

    /**
     * Check references from this component to other components
     *
     * @param compiler used for error reporting
     * @return true if all is well, false if errors found
     */

    @Override
    public boolean fixup(SchemaCompiler compiler) throws SchemaException {
        if (getFixupStatus() == UNVALIDATED) {
            setFixupStatus(VALIDATING);
        } else {
            return true;
        }

        if (getBaseTypeReference() == null) {
            TypeReference ref = new TypeReference(StandardNames.XS_ANY_SIMPLE_TYPE, compiler.getConfiguration(), this);
            ref.setTarget(AnySimpleType.getInstance());
            setBaseTypeReference(ref);
        }

        // fixup the types referred to, and replace references with references
        // to the SimpleType objects created

        TypeReference baseRef = getBaseTypeReference();
        if (baseRef != null && !baseRef.isResolved()) {
            // Failure to resolve the base type is considered a fatal error
            SchemaType typeDef = (SchemaType) PreparedSchema.validateReference(baseRef, compiler, true);
            if (typeDef == null) {
                setFixupStatus(INVALID);
                return false;
            } else if (typeDef instanceof UserSchemaComponent) {
                boolean b = ((UserSchemaComponent) typeDef).fixup(compiler);
                if (!b) {
                    setFixupStatus(INVALID);
                    return false;
                }
                if (typeDef instanceof SimpleTypeDefinition) {
                    SchemaType s = ((SimpleTypeDefinition) typeDef).getWorkingType();
                    if (s != null) {
                        baseRef.setTarget(s);
                    }
                } else if (typeDef.isComplexType()) {
                    if (isSimpleContent) {
                        if (((ComplexType) typeDef).isSimpleContent()) {
                            if (typeDef instanceof UserComplexType) {
                                TypeReference ref = ((UserComplexType) typeDef).getSimpleContentTypeReference();
                                if (ref != null) {
                                    PreparedSchema.validateReference(ref, compiler, false);
                                    if (!ref.isResolved()) {
                                        setFixupStatus(INVALID);
                                        return false;
                                    }
                                }
                                setBaseTypeReference(ref);
                            }
                        } else {
                            compiler.error("A complex type with simple content " +
                                    Err.wrap(typeDef.getDescription()) +
                                    " cannot have a type with complex content as its base type", this);
                            setFixupStatus(INVALID);
                            setValidationStatus(INVALID); // to prevent further processing
                            return false;
                        }
                    } else {
                        compiler.error("A simple type cannot have a complex type " +
                                Err.wrap(typeDef.getDescription()) +
                                " as its base type", this);
                        setFixupStatus(INVALID);
                        return false;
                    }
                } else {
                    // base type is a simple type
//                    if (isSimpleContent) {
//                        if (getDerivationMethod() == SchemaType.DERIVATION_RESTRICTION) {
//                            compiler.error("A complex type with simple content " +
//                                    Err.wrap(typeDef.getDescription()) +
//                                    " cannot be derived by restriction from a simple type", this);
//                            setFixupStatus(INVALID);
//                            return false;
//                        }
//                    }
                }
            }
        }

        // If this is a list type, resolve the item type

        if (itemTypeReference != null) {
            boolean badItemType = false;
            SchemaType type = (SchemaType) PreparedSchema.validateReference(itemTypeReference, compiler, false);
            if (type == null) {
                badItemType = true;
            } else if (type instanceof UserSchemaComponent) {
                boolean r = ((UserSchemaComponent) type).fixup(compiler);
                if (!r) {
                    badItemType = true;
                } else if (type instanceof SimpleTypeDefinition) {
                    SchemaType s = ((SimpleTypeDefinition) type).getWorkingType();
                    if (s != null) {
                        itemTypeReference.setTarget(s);
                    }
                } else if (type.isComplexType()) {
                    badItemType = true;
                    compiler.error("A list type cannot have a complex type " +
                            Err.wrap(type.getDescription()) +
                            " as its item type", this);
                }
            }
            if (badItemType) {
                UserListType list = makeListType();
                list.setItemTypeReference(new TypeReference(StandardNames.XS_ERROR, getConfiguration(), this));
                workingType = list;
                setFixupStatus(INVALID);
                return false;
            }
        }

        // If this is a union type, fix up references to member types of a union
        if (members != null) {
            for (TypeReference o : members) {
                SchemaType typeDef = (SchemaType) PreparedSchema.validateReference(o, compiler, false);
                if (typeDef == null) {
                    setFixupStatus(INVALID);
                    return false;
                } else if (typeDef instanceof SimpleTypeDefinition) {
                    boolean b = ((SimpleTypeDefinition) typeDef).fixup(compiler);
                    if (!b) {
                        setFixupStatus(INVALID);
                        return false;
                    }
                    SchemaType s = ((SimpleTypeDefinition) typeDef).getWorkingType();
                    if (s != null) {
                        o.setTarget(s);
                    }
                } else if (typeDef.isComplexType()) {
                    compiler.error("A member of a union type cannot be a complex type " +
                            typeDef.getDescription(), this);
                    setFixupStatus(INVALID);
                    return false;
                }
            }
        }

        //lookForCycles(new Stack(), compiler);   //  this does nothing!
        boolean result = makeWorkingType(compiler);
        if (result) {
            setFixupStatus(VALIDATED);
            return true;
        } else {
            setFixupStatus(INVALID);
            setValidationStatus(INVALID);
            return false;
        }
    }

    private boolean makeWorkingType(SchemaCompiler compiler) throws SchemaException {

        // Now create a SimpleType object containing the relevant information,
        // for use at run-time

        if (getDerivationMethod() == Derivation.DERIVATION_LIST) {
            boolean badItemType = false;
            boolean b = checkFacetsForListType(compiler);
            if (b) {
                SimpleType itemType = (SimpleType) itemTypeReference.getTarget();
                if (!itemType.allowsDerivation(Derivation.DERIVATION_LIST)) {
                    badItemType = true;
                    compiler.error("The item type of the list type " +
                                           getDescription() + " disallows derivation by list", this);
                }
                if (itemType instanceof ListType) {
                    badItemType = true;
                    compiler.error("The item type of the list type " +
                                           getDescription() + " is itself a list type", this);
                }
                if (itemType == BuiltInAtomicType.ANY_ATOMIC || itemType == AnySimpleType.getInstance()) {
                    badItemType = true;
                    compiler.error("The item type of the list type " +
                                           getDescription() + " must not be xs:anySimpleType or xs:anyAtomicType", this);
                }
            } else {
                badItemType = true;
            }
            UserListType list = makeListType();
            workingType = list;
            if (badItemType) {
                list.setItemTypeReference(new TypeReference(StandardNames.XS_ERROR, getConfiguration(), this));
                return false;
            } else {
                list.setItemTypeReference(itemTypeReference);
            }

        } else if (getDerivationMethod() == Derivation.DERIVATION_UNION) {
            if (members == null) {
                members = Collections.emptyList();
            }
            if (members.isEmpty()) {
                compiler.error("The union type " + getDescription() + " has no members", this);
                setFixupStatus(INVALID);
                members.add(new TypeReference(StandardNames.XS_ERROR, getConfiguration(), this));
            }
            for (TypeReference member : members) {
                SimpleType mem = (SimpleType) member.getTarget();
                if (!mem.allowsDerivation(Derivation.DERIVATION_UNION)) {
                    compiler.error("The member type " + mem.getDescription() +
                            " of the type " + getDescription() +
                            " disallows derivation by union", this);
                    setFixupStatus(INVALID);
                    member.setTarget(ErrorType.getInstance());
                }

            }
            boolean b = checkFacetsForUnionType(compiler);
            if (!b) {
                setFixupStatus(INVALID);
            }
            UserUnionType union = makeUnionType();
            union.setMemberTypeReferences(members);
            workingType = union;
            return getFixupStatus() != INVALID;

        } else {
            // derivation by restriction. We create a list type, union type, or atomic
            // type, depending on whether the base type is a list type, union type, or
            // atomic type.
            SchemaType base = getBaseType();
            if (!base.allowsDerivation(Derivation.DERIVATION_RESTRICTION)) {
                compiler.error("The base type of the type " +
                        getDescription() + " disallows derivation by restriction", this);
                setFixupStatus(INVALID);
                //return false;
            }
            if (base instanceof SimpleTypeDefinition) {
                if (((SimpleTypeDefinition) base).workingType != null) {
                    base = ((SimpleTypeDefinition) base).workingType;
                } else if (base.getValidationStatus() == INVALID || ((SimpleTypeDefinition) base).getFixupStatus() == INVALID) {
                    setFixupStatus(INVALID);
                    //return false;
                } else {
                    ((SimpleTypeDefinition) base).makeWorkingType(compiler);
                    base = ((SimpleTypeDefinition) base).workingType;
                }
            }
            if (base != null && base.getFingerprint() == StandardNames.XS_ANY_SIMPLE_TYPE &&
                    !getTypeName().hasURI(NamespaceUri.SCHEMA)) {
                // XSD 1.1 makes clear that you can't derive a type directly from XS_ANY_SIMPLE_TYPE
                // We make an exception for types in the schema namespace, the S4S has special privileges
                compiler.error("A simple type cannot be derived by restricting xs:anySimpleType", this);
                setFixupStatus(INVALID);
                setBaseTypeReference(new TypeReference(BuiltInAtomicType.STRING.getFingerprint(), getConfiguration(), this));
            }
            if (base != null && base.getFingerprint() == StandardNames.XS_ANY_ATOMIC_TYPE &&
                    !getTypeName().hasURI(NamespaceUri.SCHEMA)) {
                // XSD 1.1 makes clear that you can't derive a type directly from XS_ANY_ATOMIC_TYPE
                // We make an exception for types in the schema namespace, the S4S has special privileges
                compiler.error("A simple type cannot be derived by restricting xs:anyAtomicType", this);
                setFixupStatus(INVALID);
                setBaseTypeReference(new TypeReference(BuiltInAtomicType.STRING.getFingerprint(), getConfiguration(), this));
            }
            if (base instanceof SimpleType) {
                try {
                    checkFacetsForRestrictedType((SimpleType) base, compiler);
                } catch (SchemaException | ValidationException err) {
                    compiler.error(err.getMessage(), this);
                    setFixupStatus(INVALID);
                    setFacetList(Collections.emptyList()); // recovery action
                    //return false;
                }
                if (base instanceof ListType || ((SimpleType) base).isListType()) {
                    boolean b = checkFacetsForListType(compiler);
                    if (!b) {
                        setFixupStatus(INVALID);
                        //return false;
                    }
                    UserListType list = makeListType();
                    if (base instanceof UserListType) {
                        list.setItemTypeReference(((UserListType) base).getItemTypeReference());
                        list.inheritSeparator((UserListType)base);
                    } else {
                        assert base instanceof ListType;
                        list.setItemTypeReference(
                                new TypeReference(((ListType) base).getItemType().getFingerprint(),
                                        compiler.getConfiguration(), this));
                    }
                    workingType = list;

                } else if (base instanceof UserUnionType || ((SimpleType) base).isUnionType()) {
                    boolean b = checkFacetsForUnionType(compiler);
                    if (!b) {
                        setFixupStatus(INVALID);
                        //return false;
                    }
                    UserUnionType union = makeUnionType();
                    if (base instanceof UserUnionType) {
                        union.setMemberTypeReferences(((UserUnionType) base).getMemberTypeReferences());
                    } else if (base instanceof SimpleTypeDefinition) {
                        union.setMemberTypeReferences(((SimpleTypeDefinition) base).members);
                    }
                    workingType = union;
                }
            }
            if (workingType == null) {
                UserAtomicType atomic = new UserAtomicType(getConfiguration());
                atomic.setBaseTypeReference(getBaseTypeReference());
                atomic.setDerivationMethod(getDerivationMethod());
                atomic.setTypeName(getTypeName(), getFingerprint());
                atomic.setFacetList(getLocalFacetList());
                atomic.convertFacetValues();
                atomic.setFinalProhibitions(finalProhibitions);
                atomic.setSystemId(getSystemId());
                atomic.setLineNumber(getLineNumber());
                atomic.setGeneratedId(getGeneratedId());
                atomic.setContainingDeclaration(getContainingDeclarationName(), containingDeclarationIsElement());
                //atomic.setExtendedTypes(extendedTypes);
                atomic.setRedefinitionLevel(getRedefinitionLevel());
                atomic.setSchemaDocumentURI(getSchemaDocumentURI());
                workingType = atomic;
            }
        }

        workingType.mergeEnumerationFacets(compiler);
        workingType.getExtendedFacetList();
        workingType.setContextComponent(getContextComponent());
        if (workingType.getValidationStatus() == INVALID || getFixupStatus() == INVALID) {
            setFixupStatus(INVALID);
            return false;
        } else {
            setFixupStatus(VALIDATED);
        }

//        if (getName() == null) {
//            // the type is anonymous
//            getConfiguration().getUnnamedSchema().addAnonymousType(workingType);
//        }
        return true;
    }

    private UserUnionType makeUnionType() throws SchemaException {
        UserUnionType union = new UserUnionType(getConfiguration());
        union.setBaseTypeReference(getBaseTypeReference());
        union.setDerivationMethod(getDerivationMethod());
        union.setTypeName(getTypeName(), getFingerprint());
        union.setFacetList(getLocalFacetList());
        union.convertFacetValues();
        union.setFinalProhibitions(finalProhibitions);
        union.setSystemId(getSystemId());
        union.setLineNumber(getLineNumber());
        union.setGeneratedId(getGeneratedId());
        union.setContainingDeclaration(getContainingDeclarationName(), containingDeclarationIsElement());
        //union.setExtendedTypes(extendedTypes);
        union.setRedefinitionLevel(getRedefinitionLevel());
        union.setSchemaDocumentURI(getSchemaDocumentURI());
        return union;
    }

    private UserListType makeListType() throws SchemaException {
        UserListType list = new UserListType(getConfiguration());
        list.setBaseTypeReference(getBaseTypeReference());
        list.setDerivationMethod(getDerivationMethod());
        list.setTypeName(getTypeName(), getFingerprint());
        list.setFacetList(getLocalFacetList());
        list.convertFacetValues();
        list.setFinalProhibitions(finalProhibitions);
        list.setSystemId(getSystemId());
        list.setLineNumber(getLineNumber());
        list.setGeneratedId(getGeneratedId());
        list.setContainingDeclaration(getContainingDeclarationName(), containingDeclarationIsElement());
        //list.setExtendedTypes(extendedTypes);
        list.setRedefinitionLevel(getRedefinitionLevel());
        list.setSchemaDocumentURI(getSchemaDocumentURI());
        list.setTokenSeparator(tokenSeparator);
        return list;
    }

    private boolean checkFacetsForListType(SchemaCompiler compiler) throws SchemaException {
        // Check that the facets are legal
        List<Facet> facets = getLocalFacetList();
        for (Facet facet : facets) {
            Class name = facet.getClass();
            if (name.equals(MaxExclusiveFacet.class) ||
                name.equals(MaxInclusiveFacet.class) ||
                name.equals(MinExclusiveFacet.class) ||
                name.equals(MinInclusiveFacet.class) ||
                name.equals(TotalDigitsFacet.class) ||
                name.equals(FractionDigitsFacet.class)) {
                compiler.error("The " +
                    name + " facet is not allowed on the list type " +
                    getDescription(), this);
                return false;
            }
        }
        return true;
    }

    private boolean checkFacetsForUnionType(SchemaCompiler compiler) throws SchemaException {
        // Check that the facets are legal
        List<Facet> facets = getLocalFacetList();
        for (Facet facet : facets) {
            Class<?> facetClass = facet.getClass();
            if (facetClass.equals(PatternFacet.class) || facetClass.equals(EnumerationFacet.class) || facetClass.equals(AssertionFacet.class)) {
                //OK
            } else {
                compiler.error("The " + facet.getName() + " facet is not allowed on the union type " +
                        getDescription(), this);
                return false;
            }
        }
        return true;
    }

    private void checkFacetsForRestrictedType(SimpleType base, SchemaCompiler compiler) throws SchemaException, ValidationException {
        // Check that the facets are legal
        List<Facet> facets = getLocalFacetList();
        for (Facet facet : facets) {
            facet.checkFacetRestriction(this, base, compiler);
        }
    }


    /**
     * Checks the validity of this SimpleType definition.
     *
     * @throws SchemaException when this SimpleType definition is invalid.
     */
    @Override
    public boolean validate(SchemaCompiler compiler) throws SchemaException {
        if (workingType != null) {
            return workingType.validate(compiler);
        }
        // otherwise we're on an error recovery path
        return false;
    }

    /**
     * Get the typed value corresponding to a given string value, assuming it is
     * valid against this type
     *
     * @param value    the string value
     * @param resolver a namespace resolver used to resolve prefixes in QNames and notations
     * @param rules    the conversion rules for the Configuration
     * @return an iterator over the atomic sequence comprising the typed value
     * @throws ValidationException if the supplied value is invalid against this simple type
     */

    @Override
    public AtomicSequence getTypedValue(UnicodeString value, NamespaceResolver resolver, ConversionRules rules)
            throws ValidationException {
        if (workingType != null) {
            return workingType.getTypedValue(value, resolver, rules);
        } else {
            throw new UnsupportedOperationException(getUnresolvedMessage());
        }
    }

    /**
     * Determine whether this is a built-in type or a user-defined type
     */

    @Override
    public boolean isBuiltInType() {
        return false;
    }

    /**
     * Test whether this type is namespace sensitive, that is, if a namespace context is needed
     * to translate between the lexical space and the value space. This is true for types derived
     * from, or containing, QNames and NOTATIONs
     *
     * @return true if the type is namespace-sensitive
     */

    @Override
    public boolean isNamespaceSensitive() {
        throw new UnsupportedOperationException(getUnresolvedMessage());
    }

    /**
     * Check whether a given input string is valid according to this SimpleType
     *
     * @param value      the input string to be checked
     * @param nsResolver a namespace resolver used to resolve namespace prefixes if the type
     *                   is namespace sensitive. The value supplied may be null; in this case any namespace-sensitive
     *                   content will throw an UnsupportedOperationException.
     * @param rules      the conversion rules for the configuration
     * @throws UnsupportedOperationException if the type is namespace-sensitive and no namespace
     *                                       resolver is supplied
     */

    @Override
    public ValidationFailure validateContent(/*@NotNull*/ UnicodeString value, NamespaceResolver nsResolver, /*@NotNull*/ ConversionRules rules) {
        if (workingType != null) {
            return workingType.validateContent(value, nsResolver, rules);
        } else if (getValidationStatus() == INVALID) {
            return new ValidationFailure("Cannot validate value, because simple type definition is invalid");
        } else {
            throw new UnsupportedOperationException(getUnresolvedMessage());
        }
    }

    /**
     * Serialize the schema component
     */

    @Override
    public void serialize(SchemaModelSerializer serializer) throws XPathException {
        if (workingType == null) {
            throw new UnsupportedOperationException(getUnresolvedMessage());
        } else {
            workingType.serialize(serializer);
        }
    }
    
    @Override
    protected void serializeVariety(SchemaModelSerializer serializer) {
        throw new UnsupportedOperationException(getUnresolvedMessage());
    }

    private String getUnresolvedMessage() {
        return "SimpleTypeDefinition " + this + " should have been resolved by now (" + getDescription() + ") #" +
                getFingerprint() + ": " + getNamePool().getClarkName(getFingerprint());
    }

}

// Copyright (c) 2004-2023 Saxonica Limited

