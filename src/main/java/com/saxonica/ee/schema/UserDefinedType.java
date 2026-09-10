package com.saxonica.ee.schema;

import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.om.DocumentKey;
import net.sf.saxon.om.NamePool;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.type.*;

import java.util.Collections;
import java.util.HashSet;
import java.util.Set;

/**
 * This class represents a user-defined simple type or complex type as defined in XML Schema.
 */

public abstract class UserDefinedType extends SchemaStructure
        implements UserSchemaComponent, SchemaType {

    /**
     * Flags used to implement the "final" and "block" attributes
     */
    protected int finalProhibitions = 0;

    /**
     * The base type from which this type is derived
     */

    private TypeReference baseTypeReference;

    /**
     * The name of the element or attribute declaration containing
     * this anonymous type definition
     */

    private StructuredQName containingDeclarationName;

    /**
     * Flag to indicate whether the containing declaration of an anonymous type
     * is an element or attribute
     */

    private boolean containingDeclarationIsElementNode = true;

    /**
     * The derivation method, for example SchemaType.DERIVATION_RESTRICTION
     */
    private int derivation = Derivation.DERIVATION_RESTRICTION;

    /**
     * The fingerprint of this type in the Saxon NamePool
     */

    private int fingerprint = -1;

    /**
     * The name of the type as a structured QName
     */

    private StructuredQName qName = null;

    /**
     * The set of all the types that are derived by extension from this type
     */

    protected Set<UserComplexType> extendedTypes;

    /**
     * Default constructor. For internal use only.
     */
    public UserDefinedType() {
    }

    /**
     * Get the local name of this type if it has one. Returns a conventional
     * system-allocated name if the type is anonymous.
     *
     * @return the local name of the type component. Never null.
     */

    /*@NotNull*/
    @Override
    public String getName() {
        return getTypeName().getLocalPart();
    }

    /**
     * Get the namespace URI of the name of this type if it has one. Returns a conventional
     * system-allocated namespace if the type is anonymous.
     *
     * @return the target namespace of the type component. Never null.
     */

    /*@NotNull*/
    @Override
    public String getTargetNamespace() {
        return getTypeName().getURI();
    }

    /**
     * Get the fingerprint of the name of this type
     *
     * @return the fingerprint. Returns an invented fingerprint for an anonymous type.
     */

    @Override
    public int getFingerprint() {
        return fingerprint;
    }

    /**
     * Get the name of the type as a StructuredQName
     *
     * @return a StructuredQName identifying the type.  In the case of an anonymous type, an internally-generated
     * name is returned
     */
    public StructuredQName getTypeName() {
        return qName;
    }

    @Override
    public StructuredQName getStructuredQName() {
        return qName;
    }

    /**
     * Get the name of this type as an EQName, that is, a string in the format Q{uri}local.
     *
     * @return an EQName identifying the type. In the case of an anonymous type, an internally-generated
     * name is returned
     */
    @Override
    public String getEQName() {
        return null;
    }

    /**
     * Test whether this is an anonymous type
     *
     * @return true if this SchemaType is an anonymous type
     */

    @Override
    public boolean isAnonymousType() {
        return fingerprint == -1 || getTargetNamespace().equals(NamespaceConstant.ANONYMOUS);
    }

    /**
     * Set the nameCode of the name of this type. For internal use only.
     *
     * @param fingerprint the nameCode allocated in the namepool for this schema
     */


    public void setTypeName(StructuredQName name, int fingerprint) {
        this.qName = name;
        this.fingerprint = fingerprint;

    }

    /**
     * Get the reference to the base type. The result is never null, except in the case of the type AnyType,
     * which is the root of the type hierarchy.
     *
     * @return a reference to the base type
     */

    public TypeReference getBaseTypeReference() {
        return baseTypeReference;
    }

    /**
     * Get the fingerprint of the name of the base type of this type
     *
     * @return the fingerprint. Return an invented fingerprint for an anonymous type.
     */
    public int getBaseTypeFingerprint() {
        if (baseTypeReference != null) {
            return baseTypeReference.getTargetFingerprint();
        }
        return -1;
    }

    /**
     * Get the display name of the type: that is, a lexical QName with an arbitrary prefix
     *
     * @return a lexical QName identifying the type
     */

    @Override
    public String getDisplayName() {
        return getTypeName().getDisplayName();
    }


    /**
     * Test whether this SchemaType is a complex type
     *
     * @return true if this SchemaType is a complex type
     */

    @Override
    public final boolean isComplexType() {
        return !isSimpleType();
    }

    /**
     * Returns the value of the 'block' attribute for this type, as a bit-signnificant
     * integer with fields such as {@link Derivation#DERIVATION_LIST} and {@link Derivation#DERIVATION_EXTENSION}
     *
     * @return the value of the 'block' attribute for this type
     */

    @Override
    public int getBlock() {
        return 0;
    }

    /**
     * Get the NamePool in which the name of this type is defined
     *
     * @return the name pool
     */

    public NamePool getNamePool() {
        return getConfiguration().getNamePool();
    }

    /**
     * Returns the base type that this type inherits from.
     * If this type is a Simpletype that is a built in primitive type then null is returned.
     *
     * @return the base type.
     * @throws IllegalStateException
     *          if this type has no known base type. This can only arise during
     *          schema construction, since a type with an unknown base type cannot be used for validation
     */

    /*@NotNull*/
    @Override
    public SchemaType getBaseType() {
        if (baseTypeReference == null) {
            if (isSimpleType()) {
                return AnySimpleType.getInstance();
            } else {
                return AnyType.getInstance();
            }
        }
        try {
            return (SchemaType) baseTypeReference.getTarget();
        } catch (MissingComponentException e) {
            throw new IllegalStateException(e);
        }
    }

    /**
     * Sets the base type for this datatype. For internal use only.
     *
     * @param baseType the base type which this type inherits from
     */

    public void setBaseTypeReference(TypeReference baseType) {
        this.baseTypeReference = baseType;
    }

    /**
     * Gets the integer code of the derivation method used to derive this type from its
     * parent. Returns zero for primitive types.
     *
     * @return a numeric code representing the derivation method, for example {@link Derivation#DERIVATION_RESTRICTION}
     */

    @Override
    public int getDerivationMethod() {
        return derivation;
    }

    /**
     * Determines whether derivation (of a particular kind)
     * from this type is allowed, based on the "final" property
     *
     * @param derivation the kind of derivation, for example {@link Derivation#DERIVATION_LIST}
     * @return true if this kind of derivation is allowed
     */

    @Override
    public boolean allowsDerivation(int derivation) {
        return (finalProhibitions & derivation) == 0;
    }

    /**
     * Get the types of derivation that are not permitted, by virtue of the "final" property.
     *
     * @return the types of derivation that are not permitted, as a bit-significant integer
     *         containing bits such as {@link net.sf.saxon.type.Derivation#DERIVATION_EXTENSION}
     */
    @Override
    public int getFinalProhibitions() {
        return finalProhibitions;
    }

    /**
     * Set the derivation method name. For internal use only.
     *
     * @param method the derivation method, for example "restriction"
     * @throws net.sf.saxon.type.SchemaException
     *          for an unknown derivation method
     */

    public void setDerivationMethodName(String method) throws SchemaException {
        switch (method) {
            case "restriction":
                derivation = Derivation.DERIVATION_RESTRICTION;
                break;
            case "union":
                derivation = Derivation.DERIVATION_UNION;
                break;
            case "list":
                derivation = Derivation.DERIVATION_LIST;
                break;
            case "extension":
                derivation = Derivation.DERIVATION_EXTENSION;
                break;
            default:
                throw new SchemaException("Unknown derivation method: " + method);
        }
    }

    /**
     * Sets the derivation method code (without validating it)
     *
     * @param method the derivation method as an integer code, for example {@link Derivation#DERIVATION_UNION}
     */
    public void setDerivationMethod(int method) {
        derivation = method;
    }

    /**
     * Sets the value of the 'final' property, indicating which
     * types of derivation are not allowed. For internal use only.
     *
     * @param finalValue the value of the final property, as a bit-significant integer
     */

    public void setFinalProhibitions(int finalValue) {
        finalProhibitions = finalValue;
    }

    /**
     * Check that this type is validly derived from a given type
     *
     * @param type  the type from which this type is derived
     * @param block the derivations that are blocked by the relevant element declaration
     * @throws net.sf.saxon.type.SchemaException
     *          if the derivation is not allowed
     */

    @Override
    public void checkTypeDerivationIsOK(SchemaType type, int block) throws SchemaException {
        UserComplexType.checkTypeDerivation((ComplexType) this, type, block);
    }


    /**
     * Set the name of the containing declaration (for diagnostics)
     *
     * @param declarationName The name of the element or attribute declaration "owning" this anonymous
     *                    type definition
     * @param isElement   True if the owning declaration is an element declaration, false if it is an
     *                    attribute declaration
     */

    public void setContainingDeclaration(StructuredQName declarationName, boolean isElement) {
        containingDeclarationName = declarationName;
        containingDeclarationIsElementNode = isElement;
    }

    /**
     * Get a description of this type for use in diagnostics. In the case of a named type, this is the
     * EQName. In the case of a type known to be defined immediately within an element
     * or attribute declaration, it is a phrase that identifies the containing declaration. In other cases,
     * it is a phrase of the form "defined at line L of URI". The description is designed to be inserted
     * in a context such as "the type X is ..."
     */

    @Override
    public String getDescription() {
        int fp = getFingerprint();
        NamePool pool = getNamePool();
        if (fp != -1 && !getTypeName().hasURI(NamespaceConstant.ANONYMOUS)) {
            return getTypeName().getEQName();
        }
        if (containingDeclarationName != null) {
            return "of " +
                    (containingDeclarationIsElementNode ? "element " : "attribute ") +
                    containingDeclarationName.getDisplayName();
        }
        String sysId = getSystemId();
        if (sysId == null) {
            return pool.getEQName(fp);
        }
        int lastSlash = sysId.lastIndexOf('/');
        String module = lastSlash < 0 ? sysId : sysId.substring(lastSlash + 1);
        return "at " + module + "#" + getLineNumber();
    }

    public String toString() {
        return (isComplexType() ? "ComplexType " : "SimpleType " ) + getDescription();
    }

    /**
     * Get the name of the containing element or attribute declaration, if any (and if known).
     *
     * @return the name of the containing declaration.
     */

    public StructuredQName getContainingDeclarationName() {
        return containingDeclarationName;
    }

    /**
     * Determine whether the containing declaration is an element declaration or an attribute declaration
     *
     * @return true if the containin declaration is an element declaration, false if it is an attribute
     */

    public boolean containingDeclarationIsElement() {
        return containingDeclarationIsElementNode;
    }

    /**
     * Test whether this is the same type as another type. They are considered to be the same type
     * if they are derived from the same type definition in the original XML representation (which
     * can happen when there are multiple includes of the same file)
     */

    @Override
    public boolean isSameType(SchemaType other) {
        if (this == other) {
            return true;
        }
        if (other instanceof UserDefinedType) {
            UserDefinedType so = (UserDefinedType) other;
            return getFingerprint() == other.getFingerprint() && hasSameLocation(so);
        }
        return getFingerprint() == other.getFingerprint();

    }

    /**
     * Compare two URIs, ignoring the "file:///" versus "file:/" difference
     *
     * @param s1 the first URI
     * @param s2 the second URI
     * @return true if they are equivalent under these rules
     */

    public static boolean isSameURI(String s1, /*@Nullable*/ String s2) {
        if (s1 == null || s2 == null) {
            return false;
        }
        if (s1.equals(s2)) {
            return true;
        }
        s1 = DocumentKey.normalizeURI(s1);
        s2 = DocumentKey.normalizeURI(s2);
        return s1.equals(s2);
    }

    /**
     * Register another type as an extension of this type
     *
     * @param derived the type that is derived from this one by extension
     */

    protected void registerExtensionType(UserComplexType derived) {
        if (extendedTypes == null) {
            extendedTypes = new HashSet<UserComplexType>(5);
        }
        extendedTypes.add(derived);
    }

    /**
     * Get all the types defined as extensions of this type, transitively
     *
     * @return the set of complex types derived by extension from this type, either directly
     *         or indirectly
     */

    public Set<UserComplexType> getExtensionTypes() {
        if (extendedTypes == null) {
            return Collections.emptySet();
        } else {
            return extendedTypes;
        }
    }

    /**
     * Set the set of types derived by extension from this one
     *
     * @param types the set of extended types
     */

    public void setExtendedTypes(Set<UserComplexType> types) {
        extendedTypes = types;
    }
}

// Copyright (c) 2004-2022 Saxonica Limited