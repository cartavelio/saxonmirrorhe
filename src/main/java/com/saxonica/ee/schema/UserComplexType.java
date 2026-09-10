package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.fsa.*;
import com.saxonica.ee.schema.sdoc.SimpleTypeDefinition;
import com.saxonica.ee.stream.Sweep;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.Block;
import net.sf.saxon.expr.instruct.FixedAttribute;
import net.sf.saxon.expr.instruct.FixedElement;
import net.sf.saxon.expr.sort.SimpleTypeComparison;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.lib.Logger;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;
import net.sf.saxon.z.IntHashMap;
import net.sf.saxon.z.IntHashSet;
import net.sf.saxon.z.IntIterator;

import javax.xml.transform.SourceLocator;
import java.util.*;

import static net.sf.saxon.type.SchemaValidationStatus.INVALID;
import static net.sf.saxon.type.SchemaValidationStatus.UNVALIDATED;
import static net.sf.saxon.type.SchemaValidationStatus.VALIDATED;
import static net.sf.saxon.type.SchemaValidationStatus.VALIDATING;

/**
 * A user-defined complex type (that is, any complex type other than xs:anyType)
 */

public class UserComplexType extends UserDefinedType
        implements ComplexType, UserSchemaComponent, SerializableSchemaComponent {

    /**
     * Diagnostic flag set by saxon:debug="true" on the complexType element
     */

    private boolean debug = false;

    /**
     * The {variety} property: one of {@link ComplexVariety#EMPTY}, {@link ComplexVariety#MIXED}, {@link ComplexVariety#SIMPLE}, or
     * {@link ComplexVariety#ELEMENT_ONLY}
     */
    private ComplexVariety variety;

    /**
     * The abstract flag for this UserComplexType
     */
    private boolean _abstract = false;

    /**
     * Boolean indicating whether this complex type's content model uses xs:all
     */

    private boolean allContent = false;

    /**
     * The attribute declarations for this UserComplexType. Includes attributes, attribute
     * groups, and attribute wildcards. If this complex type is derived by extension,
     * it does not include attributes from the base type.
     */
    private AttributeGroupDecl attributes = null;

    /**
     * The attribute declarations for this UserComplexType. Includes attributes, attribute
     * groups, and attribute wildcards. If this complex type is derived by extension,
     * it includes all the attributes from the base type as well as those locally
     * defined.
     */
    private AttributeGroupDecl extendedAttributes = null;

    /**
     * The value of the 'block' property for this UserComplexType. This is a bit-significant
     * value, made up of constants such as DERIVE_BY_EXTENSION and DERIVE_BY_RESTRICTION
     */
    private int block = 0;

    /**
     * For a complex type with simple content, the simple type of the content.
     * Otherwise null. Note that this is not the same as the base type: a complex
     * type with simple content may be derived from another complex type with simple
     * content, in which case that complex type is the base type; the simpleContentType
     * is always the type of the content itself.
     */

    private TypeReference simpleContentTypeRef = null;

    /**
     * The content model for this UserComplexType, as defined in the schema
     */

    private Particle particle;

    /**
     * The content model for this UserComplexType, extended with the content model
     * of the type that it extends (if any)
     */

    private Particle extendedParticle = null;

    /**
     * a flag set to true if this complexType is a restriction
     */
    private boolean restricted = false;

    /**
     * The finite state machine for validating against this complex type
     */

    private FiniteStateMachine machine;

    /**
     * Flag to indicate that the type has been compiled
     */

    private boolean compiled = false;

    /**
     * The set of all the assertions that are defined on this complex type
     */

    private Set<Assertion> assertions;

    /**
     * The wildcard used to define permitted open content, if any
     */

    private Wildcard openContentWildcard;

    /**
     * The open content mode: {@link OpenContentVariety#NONE}, {@link OpenContentVariety#ABSENT},
     * {@link OpenContentVariety#INTERLEAVE}, or {@link OpenContentVariety#SUFFIX}
     */

    private OpenContentVariety openContentMode = OpenContentVariety.ABSENT;

    /**
     * A boolean indicating whether open content applies even if the content model would otherwise be empty
     */

    private boolean openContentAppliesToEmpty;

    /**
     * Table of context-determined types for element children
     */

    private Map<StructuredQName, SchemaType> contextDeterminedTypesForElements;

    /**
     * Table of context-determined types for attributes
     */

    private Map<StructuredQName, SimpleType> contextDeterminedTypesForAttributes;

    /**
     * Version of XML Schema specification in use
     */

    private int xsdVersion = Configuration.XSD10;

    /**
     * Creates a new user-defined Complextype
     *
     * @param config  the Configuration to which this user-defined complex type belongs
     * @param locator the location of this complex type within a schema document
     */

    public UserComplexType(EnterpriseConfiguration config, SourceLocator locator) {
        setConfiguration(config);
        setLocator(locator);
        attributes = new AttributeGroupDecl(config);
        attributes.setLocator(locator);
        machine = new FiniteStateMachine(this, false);
    }

    /**
     * Get the variety of this complex type. This will be one of the values
     * {@link ComplexVariety#EMPTY}, {@link ComplexVariety#MIXED}, {@link ComplexVariety#SIMPLE}, or
     * {@link ComplexVariety#ELEMENT_ONLY}
     * @return the variety of this complex type
     */

    @Override
    public ComplexVariety getVariety() {
        return variety;
    }

    /**
     * Get the name of this type as an EQName, that is, a string in the format Q{uri}local.
     *
     * @return an EQName identifying the type. In the case of an anonymous type, an internally-generated
     *         name is returned
     */
    @Override
    public String getEQName() {
        return "Q{" + getTargetNamespace() + "}" + getName();
    }

    /**
     * Set Saxon debugging on or off
     *
     * @param debug true to switch debugging on; false to switch it off
     */

    public void setSaxonDebug(boolean debug) {
        this.debug = debug;
    }

    /**
     * Get the XML Schema language version
     *
     * @return {@link Configuration#XSD10} or {@link Configuration#XSD11}
     */

    public int getLanguageVersion() {
        return xsdVersion;
    }

    /**
     * Set the finite state machine. Used when restoring a serialized component model
     *
     * @param machine the FSM for this complex type
     */

    public void setFiniteStateMachine(FiniteStateMachine machine) {
        this.machine = machine;
    }

    /**
     * Adds the given AttributeUse to this UserComplexType. Doesn't throw
     * an error if it's a duplicate: this is left to be checked at validation time
     *
     * @param use the AttributeUse to add to this UserComplexType
     */

    public void addAttributeUse(AttributeUse use) {
        attributes.addAttributeUse(use);
    }

    /**
     * Set the combined attribute group to an empty set of attributes.
     * Used when rebuilding a complex type from a saved SCM file
     */

    public void setEmptyCombinedAttributeGroup() {
        extendedAttributes = new AttributeGroupDecl(getConfiguration());
    }

    /**
     * Add an attribute use to the final (combined) content model for this complex type.
     * This is used when rebuilding a complex type from a saved SCM file; the difference
     * is that in this case, all required attributes are added explicitly, and none
     * are inherited from the base type.
     *
     * @param use the AttributeUse to add to this UserComplexType
     */

    public void addNonInheritedAttributeUse(AttributeUse use) {
        attributes.addAttributeUse(use);
        extendedAttributes.addAttributeUse(use);
    }

    /**
     * Adds the given AttributeGroupReference to this UserComplexType
     *
     * @param attrGroupRef the AttributeGroupReference to add to this
     *                     UserComplexType
     */

    public void addAttributeGroupReference(AttributeGroupReference attrGroupRef) {
        attributes.addAttributeGroupReference(attrGroupRef);
    }

    /**
     * Returns the attribute group containing details of all the
     * attributes defined for this complex type. This method does not include
     * the attributes defined on the base type: for that, use {@link #getCombinedAttributeGroup()}
     *
     * @return an AttributeGroup holding the attribute details
     */

    public AttributeGroupDecl getAttributeGroup() {
        return attributes;
    }

    /**
     * Returns the attribute group containing details of all the
     * attributes defined for this complex type. This method also includes
     * the attributes defined on the base type, and excludes any prohibited attributes
     *
     * @return an AttributeGroup holding the attribute details
     */

    public AttributeGroupDecl getCombinedAttributeGroup() {
        if (extendedAttributes == null) {
            throw new IllegalStateException("Extended attribute group for complex type " + getDescription() + " has not been computed");
        }
        return extendedAttributes;
    }

    /**
     * Determine the list of attribute uses and attribute wildcards allowed on this
     * complex type, by combining those defined on this type with those defined on the base type
     *
     * @param compiler used for error reporting. May be null if the schema is known to be valid.
     * @return true if successful, false if the attribute groups are inconsistent with each other
     * @throws SchemaException if the error listener throws an exception
     */

    private boolean makeCombinedAttributeGroup(SchemaCompiler compiler) throws SchemaException {
        if (extendedAttributes != null) {
            return true;
        } else if (getDerivationMethod() == Derivation.DERIVATION_EXTENSION) {
            boolean result;
            if (compiler != null) {
                result = attributes.validate(compiler);
                if (!result) {
                    return false;
                }
            }
            SchemaType base = getBaseType();
            if (base.isSimpleType()) {
                // if derived by extension from a simple type, there are no
                // attributes on the base type
                extendedAttributes = attributes;
                return true;
            }

            if (base instanceof UserComplexType) {
                result = ((UserComplexType) base).makeCombinedAttributeGroup(compiler);
                if (!result) {
                    return false;
                }
                extendedAttributes = new AttributeGroupDecl(getConfiguration());
                extendedAttributes.setLocator(this);

                // form the union of the attribute uses on this type and those on the base type
                for (AttributeUse att : getAttributeGroup().getAttributeUses()) {
                    extendedAttributes.addAttributeUse(att);
                }
                AttributeGroupDecl baseAtts = ((UserComplexType) base).getCombinedAttributeGroup();
                for (AttributeUse att : baseAtts.getAttributeUses()) {
                    extendedAttributes.addAttributeUse(att);
                }

                // combine the attribute wildcards on this type and the base type
                AttributeWildcard baseWild = baseAtts.getAttributeWildcard(compiler);
                AttributeWildcard thisWild = attributes.getAttributeWildcard(compiler);
                if (baseWild != null) {
                    if (thisWild != null) {
                        Wildcard union = Wildcard.makeUnion(
                                thisWild.getWildcard(), baseWild.getWildcard(), getNamePool());
                        AttributeWildcard newawc = union.isInexpressible() ? thisWild : new AttributeWildcard(union);
                        extendedAttributes.setAnyAttribute(newawc);
                    } else {
                        extendedAttributes.setAnyAttribute(baseWild);
                    }
                } else {
                    extendedAttributes.setAnyAttribute(thisWild);
                }
            } else {
                // the base type must be an xs:AnyType, which has no attributes
                extendedAttributes = attributes;
            }
            return true;
        } else {
            // Derived by restriction
            boolean result;
            if (compiler != null) {
                result = attributes.validate(compiler);
                if (!result) {
                    return false;
                }
            }
            SchemaType base = getBaseType();
            extendedAttributes = new AttributeGroupDecl(getConfiguration());
            extendedAttributes.setLocator(this);
            IntHashMap<AttributeUse> map = new IntHashMap<>(20);
            if (base instanceof UserComplexType) {
                ((UserComplexType) base).makeCombinedAttributeGroup(compiler);
                AttributeGroupDecl baseAtts = ((UserComplexType) base).getCombinedAttributeGroup();
                for (AttributeUse att : baseAtts.getAttributeUses()) {
                    if (!att.isProhibited()) {
                        map.put(att.getAttributeDeclaration().getFingerprint(), att);
                    }
                }

                for (AttributeUse att : getAttributeGroup().getAttributeUses()) {
                    int key = att.getAttributeDeclaration().getFingerprint();
                    AttributeUse old = map.get(key);
                    if (old != null) {
                        // we do the validation separately, we can just overwrite the attribute here;
                        // except for the case where use="prohibited", in which case we remove the
                        // attribute entirely
                        if (att.isProhibited()) {
                            map.remove(key);
                        } else {
                            map.put(key, att);
                        }
                    } else {
                        map.put(key, att);
                    }
                }
                Iterator<AttributeUse> iter2 = map.valueIterator();
                while (iter2.hasNext()) {
                    extendedAttributes.addAttributeUse(iter2.next());
                }
                AttributeWildcard wat = getAttributeGroup().getAttributeWildcard(compiler);
                if (wat != null) {
                    extendedAttributes.setAnyAttribute(wat);
                }
            } else {
                // base type must be an xs:AnyType, which has no attributes
                extendedAttributes = attributes;
            }
            return true;
        }
    }

    /**
     * Returns the value of the 'block' attribute for this element
     *
     * @return the value of the 'block' attribute for this element
     */
    @Override
    public int getBlock() {
        return block;
    }

    /**
     * Returns true if this UserComplexType has been marked as Abstract.
     *
     * @return true if this UserComplexType is "abstract".
     */

    @Override
    public boolean isAbstract() {
        return _abstract;
    }

    /**
     * Returns true if this complexType has complex content: that is, if its {content type}
     * is anything other than a simple type definition.
     *
     * @return true if this complexType has complex content
     */
    @Override
    public boolean isComplexContent() {
        return variety != ComplexVariety.SIMPLE;
    }

    /**
     * Returns true if this complexType has simple content; that is, if its {content type}
     * is a simple type definition and its {variety} is simple.
     *
     * @return true if this complexType has simple content
     */

    @Override
    public boolean isSimpleContent() {
        return variety == ComplexVariety.SIMPLE;
    }

    /**
     * Test whether this complex type has "all" content, that is, a content model
     * using an xs:all compositor
     */

    @Override
    public boolean isAllContent() {
        return allContent;
    }

    /**
     * Compute whether this complex type has a content model defined by an xs:all compositor
     * (after simplification to remove pointless particles)
     *
     * @return true if the content model of the complex type is "all"
     * @throws net.sf.saxon.type.SchemaException
     *          if an error is found in the schema
     */

    public boolean computeIsAllContent() throws SchemaException {
        if (isSimpleContent()) {
            return false;
        }

        if (particle instanceof ModelGroupParticle && ((ModelGroupParticle) particle).getGroup().containsAll(true)) {
            return allContent = true;
        }

        SchemaType base;
        if (particle == null &&
                getDerivationMethod() == Derivation.DERIVATION_EXTENSION &&
                (base = getBaseType()) instanceof ComplexType &&
                ((ComplexType) base).isAllContent()) {
            return allContent = true;
        }
        return allContent = false;
    }

    /**
     * For a complex type with "all" content, get the "all" model group. This method is always applied
     * to the simplified content model (after removal of pointless particles)
     *
     * @return the "all" particle if there is one, or null otherwise.
     */

    public ModelGroupParticle getAllCompositor() throws MissingComponentException {
        SchemaType base;
        if (extendedParticle instanceof ModelGroupParticle) {
            ModelGroup group = ((ModelGroupParticle) extendedParticle).getGroup();
            if (group instanceof AllModelGroup) {
                return (ModelGroupParticle) extendedParticle;
            }
            if (group.containsAll(true)) {
                ModelGroupParticle gpref = (ModelGroupParticle) extendedParticle;
                while (true) {
                    Particle part;
                    Iterator<Particle> iter = group.getParticles().iterator();
                    if (iter.hasNext()) {
                        part = iter.next();
                    } else {
                        throw new IllegalStateException("Empty group");
                    }
                    if (part instanceof ModelGroupParticle) {
                        if (part == gpref) {
                            throw new IllegalStateException("Circular group");
                        }
                        gpref = (ModelGroupParticle) part;
                        group = gpref.getGroup();
                        if (group instanceof AllModelGroup) {
                            return gpref;
                        }
                    } else {
                        throw new IllegalStateException("Failure finding xs:all model group");
                    }
                }
            }
            return null;
        } else if (extendedParticle == null &&
                getDerivationMethod() == Derivation.DERIVATION_EXTENSION &&
                (base = getBaseType()) instanceof ComplexType &&
                ((ComplexType) base).isAllContent()) {
            return ((UserComplexType) base).getAllCompositor();
        } else {
            return null;
        }
    }

    /**
     * For a complex type with simple content, set a reference to the simple type of the content
     *
     * @param ref a reference to a simple type
     */

    public void setSimpleContentTypeReference(TypeReference ref) {
        simpleContentTypeRef = ref;
    }

    /**
     * Get the reference to the simple content type
     *
     * @return a reference to a simple type, or null if the type is not a complex type with simple content
     */

    public TypeReference getSimpleContentTypeReference() {
        if (simpleContentTypeRef == null) {
            if (isSimpleContent()) {
                SchemaType base = getBaseType();
                if (base.isSimpleType()) {
                    // the simple content type is the same as the base type: share the reference
                    simpleContentTypeRef = getBaseTypeReference();
                } else if (base instanceof UserComplexType) {
                    simpleContentTypeRef = ((UserComplexType) base).getSimpleContentTypeReference();
                }
            }
        }
        return simpleContentTypeRef;
    }

    /**
     * For a complex type with simple content, return the simple type of the content.
     * Otherwise, return null.
     */

    /*@Nullable*/
    @Override
    public SimpleType getSimpleContentType() throws MissingComponentException {
        SimpleType simpleContentType;
        TypeReference ref = getSimpleContentTypeReference();
        if (ref == null) {
            return null;
        }
        simpleContentType = (SimpleType) ref.getTarget();
        if (simpleContentType instanceof SimpleTypeDefinition) {
            simpleContentType = ((SimpleTypeDefinition) simpleContentType).getWorkingType();
        }
        return simpleContentType;
    }

    /**
     * Ask whether this complexType is a restriction
     *
     * @return true if this complexType is a restriction
     */
    @Override
    public boolean isRestricted() {
        return restricted;
    }

    /**
     * Ask whether the contentType of this complexType is empty
     */

    @Override
    public boolean isEmptyContent() {
        return variety == ComplexVariety.EMPTY;
    }

    /**
     * Ask whether this complex type allows mixed content
     *
     * @return true, if and only if this type allows mixed content
     */

    @Override
    public boolean isMixedContent() {
        return variety == ComplexVariety.MIXED;
    }

    /**
     * Ask whether this SchemaType is a simple type
     *
     * @return true if this SchemaType is a simple type
     */

    @Override
    public boolean isSimpleType() {
        return false;
    }

    /**
     * Ask whether this SchemaType is an atomic type
     *
     * @return true if this SchemaType is an atomic type
     */

    @Override
    public boolean isAtomicType() {
        return false;
    }

    /**
     * Ask whether this type is an ID type. This is defined to be any simple type
     * who typed value may contain atomic values of type xs:ID: that is, it includes types derived
     * from ID by restriction, list, or union. Note that for a node to be treated
     * as an ID, its typed value must be a *single* atomic value of type ID; the type of the
     * node, however, can still allow a list.
     */

    @Override
    public boolean isIdType() throws MissingComponentException {
        //noinspection ConstantConditions
        return isSimpleContent() && getSimpleContentType().isIdType();
    }

    /**
     * Ask whether this type is an IDREF or IDREFS type. This is defined to be any simple type
     * who typed value may contain atomic values of type xs:IDREF: that is, it includes types derived
     * from IDREF or IDREFS by restriction, list, or union
     */

    @Override
    public boolean isIdRefType() throws MissingComponentException {
        //noinspection ConstantConditions
        return isSimpleContent() && getSimpleContentType().isIdRefType();
    }

    /**
     * Get the open content wildcard for this complex type.
     *
     * @return the open content wildcard, or null if open content is not allowed for this complex type.
     */

    public Wildcard getOpenContentWildcard() {
        return openContentWildcard;
    }

    /**
     * Ask whether open content permitted on this complex type is interleaved or suffixed.
     *
     * @return the open content mode: {@link OpenContentVariety#NONE}, {@link OpenContentVariety#ABSENT},
     *         {@link OpenContentVariety#INTERLEAVE}, or {@link OpenContentVariety#SUFFIX}
     */

    public OpenContentVariety getOpenContentMode() {
        return openContentMode;
    }

    public String getOpenContentModeString() {
        switch (openContentMode) {
            case ABSENT:
                return "absent";
            case NONE:
                return "none";
            case INTERLEAVE:
                return "interleave";
            case SUFFIX:
                return "suffix";
        }
        return null;
    }

    /**
     * Sets whether or not this UserComplexType should be abstract.
     *
     * @param isAbstract a boolean that when true makes this UserComplexType
     *                   abstract.
     */

    public void setAbstract(boolean isAbstract) {
        this._abstract = isAbstract;
    }

    /**
     * Sets the wildcard (anyAttribute) of the complexType. It is the caller's
     * responsibility to check that there can be at most one attribute wildcard
     * in a complex type definition.
     *
     * @param wildcard the attribute wildcard
     */

    public void setAnyAttribute(AttributeWildcard wildcard) {
        attributes.setAnyAttribute(wildcard);
    }

    /**
     * Sets the wildcard (anyAttribute) of the complexType on both the locally-declared
     * and the combined attribute groups. Used when reloading the complex type from an SCM file.
     *
     * @param wildcard the attribute wildcard
     */

    public void setNonInheritedAnyAttribute(AttributeWildcard wildcard) {
        attributes.setAnyAttribute(wildcard);
        extendedAttributes.setAnyAttribute(wildcard);
    }

    /**
     * Sets the value of the 'block' attribute for this UserComplexType.
     *
     * @param block the value of the block attribute for this
     *              UserComplexType definition, as a bit-significant integer.
     */

    public void setBlock(int block) {
        this.block = block;
    }

    /**
     * Set the variety of this complex type
     *
     * @param variety the variety: one of of the values
     *                {@link ComplexVariety#EMPTY}, {@link ComplexVariety#MIXED}, {@link ComplexVariety#SIMPLE}, or
     *                {@link ComplexVariety#ELEMENT_ONLY}
     */

    public void setVariety(ComplexVariety variety) {
        this.variety = variety;
    }

    /**
     * Set whether or not this complexType is a restriction
     *
     * @param restricted true if this complexType is a restriction
     */
    public void setRestriction(boolean restricted) {
        this.restricted = restricted;
    }

    /**
     * Set the particle representing the content model of this complex type
     *
     * @param particle the Particle to add
     */

    public void setParticle(Particle particle) {
        this.particle = particle;
    }

    /**
     * Set the open content details for this complex type
     *
     * @param wildcard       the open content wildcard, defined either by the <code>&lt;openContent&gt;</code> or <code>&lt;defaultOpenContent&gt;</code> elements
     * @param mode           the open content mode. This must be one of "interleave" or "suffix"
     * @param appliesToEmpty set to true if this open content applies even when the type would otherwise have an empty
     *                       content model
     */

    public void setOpenContentWildcard(Wildcard wildcard, String mode, boolean appliesToEmpty) {
        openContentWildcard = wildcard;
        if ("interleave".equals(mode)) {
            openContentMode = OpenContentVariety.INTERLEAVE;
        } else if ("suffix".equals(mode)) {
            openContentMode = OpenContentVariety.SUFFIX;
        } else if ("none".equals(mode)) {
            openContentMode = OpenContentVariety.NONE;
        } else {
            throw new IllegalArgumentException("Invalid value of open content mode - " + mode);
        }
        openContentAppliesToEmpty = appliesToEmpty;
    }

    /**
     * Add an assertion to this complex type
     *
     * @param assertion the assertion to be added
     */

    public void addAssertion(Assertion assertion) {
        if (assertions == null) {
            assertions = new HashSet<>(4);
        }
        assertions.add(assertion);
    }

    /**
     * Test whether there are any assertions defined on this complex type
     *
     * @return true if there are any assertions
     */

    @Override
    public boolean hasAssertions() {
        return assertions != null && !assertions.isEmpty();
    }

    /**
     * Get the assertions defined on this complex type
     *
     * @return the set of asserts defined on this type, including those inherited from the base type
     */

    public Set<Assertion> getAssertions() {
        if (assertions == null) {
            return Collections.emptySet();
        } else {
            return assertions;
        }
    }

    /**
     * Get the assertions defined on this complex type that have a given streaming sweep
     * @param sweep the sweep of the assertions to be selected
     * @return the set of asserts defined on this type, including those inherited from the base type, where
     * the sweep of the assertion is the requested sweep
     */

    public Set<Assertion> getAssertions(Sweep sweep) {
        if (assertions == null) {
            return Collections.emptySet();
        } else {
            HashSet<Assertion> filtered = new HashSet<>(assertions.size());
            for (Assertion a : assertions) {
                if (a.getSweep() == sweep) {
                    filtered.add(a);
                }
            }
            return filtered;
        }
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
    public void lookForCycles(Stack<SchemaComponent> references, SchemaCompiler compiler) throws SchemaException, MissingComponentException {
        for (Object reference : references) {
            if (((SchemaType) reference).isSameType(this)) {
                compiler.error("The definition of the complex type " + getDescription() +
                        " is circular", this);
                throw new SchemaException("Circular definition found");
            }
        }
        references.push(this);
        getBaseTypeReference().tryToResolve(compiler, false);
        if (getBaseTypeReference().isResolved()) {
            SchemaType base = (SchemaType) getBaseTypeReference().getTarget();
            if (base instanceof UserSchemaComponent) {
                ((UserSchemaComponent) base).lookForCycles(references, compiler);
            }
        }
        references.pop();
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
            if (attributes != null) {
                boolean ok = attributes.fixup(compiler);
                if (!ok) {
                    setFixupStatus(INVALID);
                    return false;
                }
            }
            if (simpleContentTypeRef != null) {
                SchemaType type = (SchemaType) PreparedSchema.validateReference(simpleContentTypeRef, compiler, false);
                if (type == null) {
                    setFixupStatus(INVALID);
                    return false;
                }
                if (type instanceof UserSchemaComponent) {
                    boolean ok = ((UserSchemaComponent) type).fixup(compiler);
                    if (!ok) {
                        setFixupStatus(INVALID);
                        return false;
                    }
                }
            }
            if (getBaseTypeReference() != null) {
                SchemaType type = (SchemaType) PreparedSchema.validateReference(getBaseTypeReference(), compiler, true);
                if (type == null) {
                    setFixupStatus(INVALID);
                    return false;
                }
                if (type instanceof UserSchemaComponent) {
                    ((UserSchemaComponent) type).fixup(compiler);
                }
            }
            if (particle != null) {
                particle.fixup(compiler);
            }
        }
        setFixupStatus(VALIDATED);
        return true;
    }

    /**
     * Checks the validity of this UserComplexType definition.
     */

    @Override
    public boolean validate(SchemaCompiler compiler) throws SchemaException {
        boolean result = true;

        if (getFixupStatus() == INVALID) {
            return false;
        }

        // check for previous validation
        if (!isValidationNeeded()) {
            return true;
        }
        setValidationStatus(VALIDATING);
        xsdVersion = compiler.getLanguageVersion();

        //-- check content model
        if (particle != null) {
            result = particle.validate(compiler);
            if (!result) {
                setValidationStatus(INVALID);
                return false;
            }
        }

        //-- make sure the base type is known and validated
        if (getBaseTypeReference() != null) {
            PreparedSchema.validateReference(getBaseTypeReference(), compiler, true);
            if (!getBaseTypeReference().isResolved()) {
                setValidationStatus(INVALID);
                return false;
            }
        }

        SchemaType base = getBaseType();
        if (base instanceof UserComplexType) {
            lookForCycles(new Stack<SchemaComponent>(), compiler);
            result = ((UserComplexType) base).validate(compiler);
            // Copy assertions from the base type to this type
            for (Assertion a : ((UserComplexType) base).getAssertions()) {
                addAssertion(a);
            }
        }

        // if it's simple content, validate the simple content type
        if (result & isSimpleContent()) {
            result &= validateSimpleContent(compiler, base);
        }

        // Make and validate the extended attribute group
        if (result) {
            result = makeCombinedAttributeGroup(compiler);
        }
        if (result) {
            result = extendedAttributes.validate(compiler);
        }

        // DERIVATION BY EXTENSION

        if (getDerivationMethod() == Derivation.DERIVATION_EXTENSION) {
            result &= validateExtension(compiler, base);
        }

        // DERIVATION BY RESTRICTION

        if (getDerivationMethod() == Derivation.DERIVATION_RESTRICTION) {
            result &= validateRestriction(compiler, base);
        }

        // compute whether this complex type has an "all" content model

        allContent = computeIsAllContent();

        // try compiling the finite state machine for this complex type
        if (result) {
            compile(compiler);
        }

        setValidationStatus(result ? VALIDATED : INVALID);

        return result;

    }

    /**
     * Validate a type that has simple content
     *
     * @param compiler the schema compiler
     * @param base     the base type from which this type is derived
     * @return true if validation succeeds
     * @throws SchemaException if the type is found to be invalid
     */

    private boolean validateSimpleContent(SchemaCompiler compiler, SchemaType base) throws SchemaException {
        if (simpleContentTypeRef == null) {
            if (getValidationStatus() == INVALID) {
                setValidationStatus(INVALID);
                return false;
            } else if (base.isSimpleType()) {
                // the simple content type is the same as the base type: share the reference
                simpleContentTypeRef = getBaseTypeReference();
            } else if (base.isComplexType() && ((ComplexType) base).isSimpleContent()) {
                simpleContentTypeRef = ((UserComplexType) base).getSimpleContentTypeReference();
            } else {
                String err = "The complex type " + getDescription();
                err += " has simple content but its base type has complex content";
                compiler.error(err, this);
                setValidationStatus(INVALID);
                return false;
            }
        }
        SimpleType content = (SimpleType) PreparedSchema.validateReference(getSimpleContentTypeReference(), compiler, false);
//        if (content.isIdType()) {
//            compiler.warning("A complex type with simple xs:ID content does not impose any uniqueness constraints", this);
//        }
//        if (content.isIdRefType()) {
//            compiler.warning("A complex type with simple xs:IDREF content does not impose any referential constraints", this);
//        }
        if (content instanceof SimpleTypeDefinition) {
            SchemaType type = compiler.getPreparedSchema().getSchemaType(content.getStructuredQName());
            if (type != null) {
                if (type.getValidationStatus() == INVALID) {
                    setValidationStatus(INVALID);
                    return false;
                } else if (type.isSimpleType()) {
                    if (type instanceof SimpleTypeDefinition) {
                        type = ((SimpleTypeDefinition) type).getWorkingType();
                    }
                    getSimpleContentTypeReference().setTarget(type);
                } else {
                    String err = "The complex type " + getDescription();
                    err += " has simple content that references a complex type";
                    compiler.error(err, this);
                    setValidationStatus(INVALID);
                    return false;
                }
            } else {
                // we've already processed the SimpleType
            }
            if (getSimpleContentType() instanceof UserSimpleType) {
                return ((UserSimpleType) getSimpleContentType()).validate(compiler);
            }
        }
        return true;
    }

    /**
     * Validate a type that is derived by extension. This method also incorporates the mapping rules that
     * construct the properties of the extended type from the raw information in the XML representation
     * together with the properties of the base type from which this type is derived.
     *
     * @param compiler the schema compiler
     * @param base     the base type from which this type is derived by extension
     * @return true if validation is successful
     * @throws SchemaException if a schema error is found
     */

    private boolean validateExtension(SchemaCompiler compiler, SchemaType base) throws SchemaException {

        // If the base type already has an extension identical to this one, then we can make a quick
        // exit. This can happen when the same schema is processed more than once.

        if (base instanceof UserDefinedType) {
            Set<UserComplexType> existingExtensions = ((UserDefinedType) base).getExtensionTypes();
            for (UserComplexType ex : existingExtensions) {
                if (ex.getTypeName().equals(getTypeName()) && ex.isSameType(this)) {
                    return true;
                }
            }
        }

//        SchemaType existing = getConfiguration().getSchemaType(getStructuredQName());
//        if (existing != null && existing.isSameType(this)) {
//            return true;
//        }

        // Don't allow derivation by extension from a type that has already been used for validation
        // or for compiling schemas or stylesheets

        final EnterpriseConfiguration config = getConfiguration();
        if (config.isSealedNamespace(base.getTargetNamespace())) {
            String err = "It is not possible to extend type " + base.getDescription() +
                    " because that type has already been used for validating instance documents" +
                    " or for compiling queries or stylesheets";
            compiler.error(err, this);
            setValidationStatus(INVALID);
            return false;
        }

        // See Schema Part 1 section 3.4.6,
        // Schema Component Constraint: Derivation Valid (Extension)

        // Rules 1.1, 2.2
        if (!base.allowsDerivation(Derivation.DERIVATION_EXTENSION)) {
            String err = "The complex type " + getDescription() +
                    " is derived by extension, but the base type prohibits extension";
            compiler.error(err, this);
            setValidationStatus(INVALID);
            return false;
        }

        // Calculate whether the contentType is Empty

        if (variety == ComplexVariety.ELEMENT_ONLY && isLocallyEmpty() && isEmptyContent(base)) {
            variety = ComplexVariety.EMPTY;
            particle = null;
        }

        if (isComplexContent() && base.isSimpleType()) {
            String err = "The type " + getDescription() + " has complex content so it " +
                    "cannot be based on the simple type " + base.getDescription();
            compiler.error(err, this);
            setValidationStatus(INVALID);
            return false;
        }

        // Disallow xs:all unless the base type is empty (XSD 1.0 only)

        if (compiler.getLanguageVersion() == Configuration.XSD10) {
            if (isAllParticle(particle) && !isEmptyContent(base)) {
                String err = "The type " + getDescription() + " is defined with xs:all so (in XSD 1.0) it " +
                        "cannot be derived by extension from a non-empty type";
                compiler.error(err, this);
                setValidationStatus(INVALID);
                return false;
            }

            if (!isLocallyEmpty() && base instanceof UserComplexType
                    && isAllParticle(((UserComplexType) base).particle)) {
                String err = "In XSD 1.0, a type cannot be derived by extension from a " +
                        "type defined using xs:all unless the extension is empty";
                compiler.error(err, this);
                setValidationStatus(INVALID);
                return false;
            }
        } else {
            // XSD 1.1
            if (isAllParticle(particle) && base instanceof UserComplexType) {
                if (isAllParticle(((UserComplexType) base).particle)) {
                    // As a special case (see bug 6202, test case all308), and for no good reason,
                    // extension from an empty all group with mixed content is not permitted. It's
                    // a bug in the spec, but we implement it warts and all
                    AllModelGroup amg = (AllModelGroup) ((ModelGroupParticle) ((UserComplexType) base).particle).getGroup();
                    if (amg.getSimplifiedContentModel().isEmpty() && ((UserComplexType) base).isMixedContent()) {
                        String err = "The type " + getDescription() + " defined with xs:all cannot " +
                                "be derived by extension from a base type that is defined using xs:all with mixed " +
                                "content but no element content. This is because the base type " +
                                "is treated as if the compositor were xs:sequence rather than xs:all";
                        compiler.error(err, this);
                        setValidationStatus(INVALID);
                        return false;
                    }
                    if (particle.getMinOccurs() != ((UserComplexType) base).particle.getMinOccurs()) {
                        String err = "The type " + getDescription() + " defined with xs:all cannot " +
                                "be derived by extension from a base type that is defined using xs:all because " +
                                "the two xs:all particles have different minOccurs values";
                        compiler.error(err, this);
                        setValidationStatus(INVALID);
                        return false;
                    }
                    Particle p = makeExtendedParticle(compiler);
                    boolean validParticle = p.validate(compiler);
                    if (!validParticle) {
                        setValidationStatus(INVALID);
                        return false;
                    }
                } else if (((UserComplexType) base).isEmptyContent()) {
                    // OK, no action
                } else {
                    String err = "The type " + getDescription() + " is defined with xs:all so it " +
                            "cannot be derived by extension unless the base type is empty or is itself defined with xs:all";
                    compiler.error(err, this);
                    setValidationStatus(INVALID);
                    return false;
                }
            }
            if (!isLocallyEmpty() && !isAllParticle(particle) && base instanceof UserComplexType
                    && isAllParticle(((UserComplexType) base).particle)) {
                String err = "A type cannot be derived by extension from a " +
                        "type defined using xs:all unless the extension is empty " +
                        "or the extension is itself defined using xs:all";
                compiler.error(err, this);
                setValidationStatus(INVALID);
                return false;
            }
        }

        if (isComplexContent() && base instanceof UserComplexType &&
                ((UserComplexType) base).isSimpleContent()) {
            if (isLocallyEmpty() && compiler.getLanguageVersion() == Configuration.XSD10) {
                // this is a special case: see test particlesZ031. Although the user wrote
                // <complexContent>, extending a complex type with simple content with a null
                // extension appears (in XML Schema 1.0) to be a legitimate way of creating a complex type with
                // *simple* content. In 1.1 it violates Derivation Valid (Extension) - see bug 5943
                setVariety(ComplexVariety.SIMPLE);
                setValidationStatus(UNVALIDATED);
                return validate(compiler);
            }
            String err = "The type " + getDescription() + " has complex content so it " +
                    "cannot be derived by extension from the complex type " +
                    base.getDescription() + " which has simple content";
            compiler.error(err, this);
            setValidationStatus(INVALID);
            return false;
        }

        if (isSimpleContent()) {
            if (base.isSimpleType()) {
                // OK
            } else if (base instanceof AnyType) {
                String err = "The type " + getDescription() + " has simple content so it " +
                        "cannot be derived by extension from xs:anyType";
                compiler.error(err, this);
                setValidationStatus(INVALID);
                return false;
            } else if (!((ComplexType) base).isSimpleContent()) {
                String err = "The type " + getDescription() + " has simple content so it " +
                        "cannot be derived by extension from the type " + base.getDescription() +
                        " which has complex content";
                compiler.error(err, this);
                setValidationStatus(INVALID);
                return false;
            }
        }

        if (base.isComplexType()) {
            ComplexType cbase = (ComplexType) base;
            if (isMixedContent()) {
                if (base instanceof AnyType || cbase.isMixedContent() ||
                        cbase.isEmptyContent()) {
                    // OK
                } else {
                    String err = "The type " + getDescription() + " has mixed content, " +
                            "so its base type must have mixed or empty content";
                    compiler.error(err, this);
                    setValidationStatus(INVALID);
                    return false;
                }
            } else if (particle != null) {
                // implies element-only content
                if (base instanceof ComplexType && cbase.isMixedContent()) {
                    String err = "The type " + getDescription() + " has element-only content, " +
                            "so it cannot be derived by extension from a type with mixed content";
                    compiler.error(err, this);
                    setValidationStatus(INVALID);
                    return false;
                }
            } else if (isLocallyEmpty()) {
                if (base instanceof ComplexType && cbase.isMixedContent()) {
                    setVariety(ComplexVariety.MIXED);
                    // Note: if the mixed attribute is absent, we override it. If the attribute
                    // was explicitly set to "false", this is apparently not an error.
                }
            }

            // Default openContent to the openContent of the base type

            if (base instanceof UserComplexType) {
                if (openContentMode == OpenContentVariety.NONE) {

                }
                if (openContentWildcard == null /*&& openContentMode != OPEN_CONTENT_NONE*/) {
                    openContentWildcard = ((UserComplexType) base).getOpenContentWildcard();
                    openContentMode = ((UserComplexType) base).getOpenContentMode();
                } else {
                    // Check that openContent for the extension is consistent with the base type
                    // See 3.4.6.3 Derivation Valid (Extension) rule 1.4.3.2.2.3
                    Wildcard baseOpenWildcard = ((UserComplexType) base).getOpenContentWildcard();
                    boolean baseOpenWildcardInterleaved =
                            ((UserComplexType) base).getOpenContentMode() == OpenContentVariety.INTERLEAVE;
                    if (baseOpenWildcard != null) {
                        if (openContentWildcard != null && baseOpenWildcardInterleaved &&
                                openContentMode != OpenContentVariety.INTERLEAVE) {
                            String err = "Invalid extension. The type " + getDescription() +
                                    " has interleaved open content," +
                                    " but its base type has suffix open content";
                            compiler.error(err, this);
                            setValidationStatus(INVALID);
                            return false;
                        } else if (openContentWildcard == null) {
                            String err = "Invalid extension. The base type of type " + getDescription() +
                                    " allows open content, but this type does not";
                            compiler.error(err, this);
                            setValidationStatus(INVALID);
                            return false;
                        }
                        // clause 1.4.3.2.2.4
                        if (openContentWildcard != null) {
                            openContentWildcard = Wildcard.makeUnion(openContentWildcard, baseOpenWildcard, getNamePool());
//                            if (!baseOpenWildcard.isSubset(openContentWildcard, getNamePool())) {
//                                String err = "Invalid extension. The open content wildcard of the base type of type " + getDescription() +
//                                    " is not a subset of the open content wildcard of the extended type";
//                                compiler.error(err, this);
//                                setValidationStatus(INVALID);
//                                return false;
//                            }
                        }
                    }
                }
            }

            if (isComplexContent()) {
                // Check the extended particle is OK (e.g. Element Declarations Consistent)
                Particle p = makeExtendedParticle(compiler);
                boolean ok = p == null || p.validate(compiler);
                if (!ok) {
                    setValidationStatus(INVALID);
                    return false;
                }
            }

            // For any attribute allowed in the content model of this type which was not allowed by
            // the base type, check that there isn't a type further up the tree that allows this attribute
            // with an incompatible type.

            // Schema Part 1, Schema Component Constraint: Derivation Valid (Extension)
            // See #cos-ct-extends clause 1.5 (as refined in XML Schema 1.1)

            for (AttributeUse au : attributes.getAttributeUses()) {
                StructuredQName fp = au.getTargetComponentName();
                if (base instanceof UserComplexType) {
                    SchemaType baseComplexType = base;
                    while (baseComplexType instanceof UserComplexType) {
                        int derivation = baseComplexType.getDerivationMethod();
                        SchemaType g = baseComplexType.getBaseType();
                        if (g instanceof UserComplexType && derivation == Derivation.DERIVATION_RESTRICTION) {
                            AttributeUse baseAttributeUse =
                                    ((UserComplexType) g).getCombinedAttributeGroup().getAttributeUse(fp);
                            if (baseAttributeUse != null) {
                                SchemaType original = baseAttributeUse.getAttributeDeclaration().getType();
                                if (original != null) {
                                    SchemaType derived = au.getAttributeDeclaration().getType();
                                    try {
                                        derived.checkTypeDerivationIsOK(original, 0);
                                    } catch (SchemaException se) {
                                        String err = "The type " + getDescription() +
                                                " is derived by extension, and reintroduces" +
                                                " attribute " + fp.getStructuredQName().getEQName() +
                                                " which was defined in the content model of the ancestor type " +
                                                g.getDescription() +
                                                " with an incompatible type. " + se.getMessage();
                                        compiler.error(err, this);
                                        setValidationStatus(INVALID);
                                        return false;
                                    }
                                }
                            }
                        }
                        baseComplexType = g;
                    }
                }
            }

            // For any element allowed in the content model of this type which was not allowed by
            // the base type, check that there isn't a type further up the tree that allows this element
            // with an incompatible type.

            // Schema Part 1, Schema Component Constraint: Derivation Valid (Extension)
            // See #cos-ct-extends clause 1.5 (as refined in XML Schema 1.1)

            if (particle != null) {
                IntHashSet extensionElements = new IntHashSet(10);
                particle.gatherAllPermittedElements(extensionElements, true);
                IntIterator extensionElementIter = extensionElements.iterator();
                while (extensionElementIter.hasNext()) {
                    int fp = extensionElementIter.next();
                    if (base instanceof UserComplexType) {
                        SchemaType particleBaseType = base;
                        while (particleBaseType instanceof UserComplexType) {
                            int derivation = particleBaseType.getDerivationMethod();
                            SchemaType g = particleBaseType.getBaseType();
                            if (g instanceof UserComplexType && derivation == Derivation.DERIVATION_RESTRICTION) {
                                SchemaType original = ((UserComplexType) g).getElementParticleType(fp, false);
                                if (original != null) {
                                    SchemaType derived = particle.getElementParticleType(fp);
                                    try {
                                        derived.checkTypeDerivationIsOK(original, 0);
                                    } catch (SchemaException se) {
                                        String err = "The type " + getDescription() +
                                            " is derived by extension, and reintroduces" +
                                            " element " + getNamePool().getClarkName(fp) +
                                            " which was defined in the content model of the ancestor type " +
                                            g.getDescription() +
                                            " with an incompatible type. " + se.getMessage();
                                        compiler.error(err, this);
                                        setValidationStatus(INVALID);
                                        return false;
                                    }
                                }
                            }
                            particleBaseType = g;
                        }
                    }
                }
            }
        }

        // Register this extension with the types it is extending, so that they know all the possible
        // elements and attributes that can appear in the content of that type.

        SchemaType baseType = base;
        while (true) {
            if (baseType instanceof UserDefinedType) {
                if (baseType instanceof SimpleTypeDefinition) {
                    baseType = ((SimpleTypeDefinition) baseType).getWorkingType();
                }
                ((UserDefinedType) baseType).registerExtensionType(this);
                baseType = baseType.getBaseType();
            } else if (baseType instanceof SimpleType && ((SimpleType) baseType).isBuiltInType()) {
                // A user-defined complex type with simple content can be derived by extension from
                // (and therefore be substitutable for) a built-in simple type
                compiler.getPreparedSchema().registerExtensionOfBuiltInType(baseType.getFingerprint(), this);
                baseType = baseType.getBaseType();
            } else {
                break;
            }
        }
        return true;
    }

    /**
     * Validate a type that is derived by restriction
     *
     * @param compiler the schema compiler
     * @param base     the base type from which this is derived by restriction
     * @return true if validation succeeds false if it fails
     * @throws SchemaException if there are errors and the ErrorListener decides they are fatal
     */

    private boolean validateRestriction(SchemaCompiler compiler, SchemaType base) throws SchemaException {

        if (!base.allowsDerivation(Derivation.DERIVATION_RESTRICTION)) {
            String err = "The complex type " + getDescription() +
                    " is derived by restriction, but the base type prohibits restriction";
            compiler.error(err, this);
            setValidationStatus(INVALID);
            return false;
        }

        // Calculate whether the contentType of this UserComplexType is empty

        if (variety == ComplexVariety.ELEMENT_ONLY &&
                (openContentWildcard == null || !openContentAppliesToEmpty) &&
                isLocallyEmpty()) {
            variety = ComplexVariety.EMPTY;
            particle = null;
        }

        if (base.isSimpleType()) {

            // It is an error to derive a complex type by restriction from a simple type.
            String err = "The complex type " + getDescription();
            err += " is defined as a restriction of the simple type ";
            err += base.getDescription();
            compiler.error(err, this);
            setValidationStatus(INVALID);
            return false;

        } else if (base.isComplexType()) {

            // we are now sure that the base is a ComplexType
            // if this type has simple content then the base must either have compatible simple content, or must
            // be a mixed type and must be emptiable
            ComplexType cbase = (ComplexType) base;
            boolean OK = false;
            if (isSimpleContent()) {
                // rule 5.2.2.1
                if (cbase.isSimpleContent()) {
                    SimpleType thisContent = getSimpleContentType();
                    SimpleType baseContent = cbase.getSimpleContentType();
                    String reason;
                    if (thisContent == null || baseContent == null) {
                        reason = "Cannot establish the simple content type";
                        // probably a consequence of a previous error: see ctK002.xsd
                    } else {
                        reason = UserSimpleType.isTypeDerivationOK(thisContent, baseContent, 0);
                    }
                    if (reason == null) {
                        OK = true;
                    } else {
                        String err = "The complex type " + getDescription() +
                                " has simple content of a type that is not validly derived from that " +
                                "of the base type. " + reason;
                        compiler.error(err, this);
                        setValidationStatus(INVALID);
                        return false;
                    }
                }
                // rule 5.2.2.2 (first half)
                if (!OK) {
                    if (!cbase.isMixedContent()) {
                        String err = "The complex type " + getDescription();
                        err += " has simple content so its base type, if complex, must have mixed content";
                        compiler.error(err, this);
                        setValidationStatus(INVALID);
                        return false;
                    }
                    // rule 5.2.2.2 (second half)
                    if (!((ComplexType) base).isEmptiable()) {
                        String err = "The complex type " + getDescription() +
                                " has simple content so its base type, if complex, must allow empty content";
                        compiler.error(err, this);
                        setValidationStatus(INVALID);
                        return false;
                    }
                    // Section 3.4.3, Schema Representation Constraint, clause 2.2
                    @SuppressWarnings({"ConstantConditions"})
                    SchemaType simpleContentBase = getSimpleContentType().getBaseType();
                    if (simpleContentBase == null || simpleContentBase instanceof AnySimpleType) {
                        String err = "The complex type " + getDescription() +
                                " has simple content and its base type is a mixed complex type," +
                                " so it must specify a simpleType";
                        compiler.error(err, this);
                        setValidationStatus(INVALID);
                        return false;
                    }
                }

            } else {
                // this type has complex content
                if (cbase.isSimpleContent()) {
                    String err = "The complex type " + getDescription() +
                            " has complex content, but is derived by restriction from a base type that has simple content";
                    compiler.error(err, this);
                    setValidationStatus(INVALID);
                    return false;
                }
            }

            // if this type is mixed, the base type must be mixed
            if (isMixedContent() && !((ComplexType) base).isMixedContent()) {
                String err = "The complex type " + getDescription() +
                        " has mixed content so its base type must also have mixed content";
                compiler.error(err, this);
                setValidationStatus(INVALID);
                return false;
            }

            // if the particle or any contained particle is pointless, replace it by its contents

            if (particle != null) {
                if (particle.isPointless(null)) {
                    ModelGroup group = ((ModelGroupParticle) particle).getGroup();
                    List<Particle> contentModel = group.getSimplifiedContentModel();
                    if (variety == ComplexVariety.ELEMENT_ONLY && contentModel.isEmpty() &&
                            !(group instanceof ChoiceModelGroup) /* bug 2388 */ &&
                            (openContentWildcard == null || !openContentAppliesToEmpty)) {
                        // NOTE: seems redundant, we have done this check already
                        particle = null;
                        variety = ComplexVariety.EMPTY;
                    } else if (contentModel.size() == 1 && !(group instanceof AllModelGroup)) {
                        // Cannot reduce an All model group, because it affects the rules for derivation by extension
                        // (See Saxonica test All311)
                        particle = contentModel.get(0);
                    } else {
                        // This was redundant, it creates an object and then ignores it
//                        SequenceModelGroup seq = new SequenceModelGroup();
//                        seq.setContentModel(contentModel);
                    }
                } else if (particle instanceof ModelGroupParticle && particle.getMinOccurs() == 1 && particle.getMaxOccurs() == 1) {
                    ((ModelGroupParticle) particle).getGroup().getSimplifiedContentModel();
                }
            }

            // check that this is a valid restriction of the base type

            return isValidRestriction(base, compiler);
        }
        return true;
    }

    /**
     * Ask whether a given particle is an xs:all model group particle
     *
     * @param particle the particle to be tested
     * @return true if it has an xs:all content model
     */

    private static boolean isAllParticle(Particle particle) throws MissingComponentException {
        return particle instanceof ModelGroupParticle &&
                ((ModelGroupParticle) particle).getGroup() instanceof AllModelGroup;
    }

    /**
     * Determine whether the content model (of this type, ignoring the base type, and ignoring any open content)
     * is empty
     *
     * @return true if the content model is locally empty
     */

    private boolean isLocallyEmpty() throws MissingComponentException {
        if (isSimpleContent()) {
            return false;
        }
        if (particle == null || particle.getMaxOccurs() == 0) {
            return true;
        }
        if (particle instanceof ModelGroupParticle) {
            ModelGroup group = ((ModelGroupParticle) particle).getGroup();
            return group.isEmpty() &&
                    (group instanceof AllModelGroup || group instanceof SequenceModelGroup ||
                            (group instanceof ChoiceModelGroup && particle.getMinOccurs() == 0));
        }
        return false;
    }

    private boolean isEmptyContent(SchemaType type) {
        return (type instanceof ComplexType) &&
                ((ComplexType) type).isEmptyContent();
    }

    /**
     * Determine whether this type is a valid restriction of another type
     *
     * @param base     the specified base type
     * @param compiler used for error reporting
     * @return true if this type is a valid restriction of the specified base type
     * @throws SchemaException if an error occurs
     */

    public boolean isValidRestriction(SchemaType base, SchemaCompiler compiler) throws SchemaException {
        if (base instanceof AnyType) {
            return true;
        }
        if (this == base) {
            return true;
        }
        if (getValidationStatus() == INVALID || base.getValidationStatus() == INVALID) {
            // the error will already have been reported, no point checking any further
            return false;
        }
        ComplexType cbase = (ComplexType) base;
        makeCombinedAttributeGroup(compiler);
        if (cbase instanceof UserComplexType) {
            final UserComplexType ucbase = (UserComplexType) cbase;
            ucbase.makeCombinedAttributeGroup(compiler);
            if (!ucbase.compiled) {
                ucbase.compile(compiler);
            }
        }

        if (cbase instanceof UserComplexType) {
            if (!getCombinedAttributeGroup().isValidRestriction(
                    ((UserComplexType) cbase).getCombinedAttributeGroup(), compiler)) {
                compiler.error("The attributes of complex type " + getDescription() +
                        " are not a restricted subset of the attributes of the type " + base.getDescription(), this);
                return false;
            }
        }

        if (!compiled) {
            compile(compiler);
        }
        String subsumeError = isSubsumedBy(cbase, compiler);
        if (subsumeError != null) {
            if (subsumeError.startsWith("?")) {
                compiler.error("Saxon is unable to determine whether or not the" +
                        " content model of the complex type " + getDescription() +
                        " is a valid restriction of the content model of the type " + base.getDescription() + ". "
                        + subsumeError.substring(1), this);
            } else {
                compiler.error("The content model of the complex type " + getDescription() +
                        " is not a valid restriction of the content model of the type " + base.getDescription()
                        + ". " + subsumeError, this);
            }
            return false;
        }
        return true;
    }

    /**
     * Compile the finite-state machine for validating instances against this type
     *
     * @param compiler used for error reporting
     * @throws SchemaException to indicate an invalid schema
     */

    public void compile(SchemaCompiler compiler) throws SchemaException {
        // Only compile a type once
        if (compiled || getValidationStatus() == INVALID) {
            return;
        }

        //System.err.println("Compiling type " + getDescription() + " (" + this + ")");

        if (debug) {
            Logger err = compiler.getConfiguration().getLogger();
            err.info("==== Compiling complex type " + getDescription());
            err.info("  simple content: " + isSimpleContent());
            err.info("  complex content: " + isComplexContent());
            err.info("  all content: " + isAllContent());
            err.info("  empty content: " + isEmptyContent());
            err.info("  emptiable: " + isEmptiable());
            err.info("  mixed content: " + isMixedContent());
            err.info("  base type: " + getBaseType().getDescription());
        }
        compiled = true;

        if (isSimpleContent() || isEmptyContent()) {
            machine.getInitialState().setFinalState(true);
        } else if (isAllContent()) {
            makeExtendedParticle(compiler);
        } else /* if (particle==null  || !(particle instanceof AllCompositor ))*/ {
            Particle extended = makeExtendedParticle(compiler);
            if (extended == null) {
                machine.getInitialState().setFinalState(true);
                machine.setOpenContentWildcard(openContentWildcard, openContentMode == OpenContentVariety.INTERLEAVE);
            } else {
                //makeAutomaton(compiler);
                compiler.scheduleAutomaton(this);
                if (particle != null) {
                    particle.compile(compiler);
                }
            }
        }
        makeCombinedAttributeGroup(compiler);
        makeContextDeterminedTypeMapForAttributes();
        makeContextDeterminedTypeMapForElements();
    }

    protected void makeAutomaton(SchemaCompiler compiler) throws SchemaException {
        //extended.rewrite(null);
        Particle extended = extendedParticle;
        NonDeterminizedState finalState = new NonDeterminizedState(machine);
        finalState.setFinalState(true);
        try {
            //                long startTime = System.currentTimeMillis();
            AutomatonState state = FiniteStateMachine.compileParticle(compiler, extended, finalState, this, machine);
            machine.setInitialState(state);
            //                System.err.println("Time to compile complex type " + getDescription() +
            //                        " - " + (System.currentTimeMillis() - startTime));
            if (debug) {
                Logger err = compiler.getConfiguration().getLogger();
                err.info("==== NFSA for complex type " + getDescription());
                machine.display(err);
            }
            //                startTime = System.currentTimeMillis();
            machine = FiniteStateMachine.determinize(compiler, machine);
            //                System.err.println("Time to determinize complex type " + getDescription() +
            //                        " - " + (System.currentTimeMillis() - startTime));

            if (debug) {
                Logger err = compiler.getConfiguration().getLogger();
                err.info("==== DFSA for complex type " + getDescription());
                machine.display(err);
            }

            machine.setOpenContentWildcard(openContentWildcard, openContentMode == OpenContentVariety.INTERLEAVE);
            //initialState = machine.getInitialState();
        } catch (SchemaException err) {
            compiler.error("Error in complex type " +
                    getDescription() +
                    ": " + err.getMessage(), this);
            throw err;
        }
    }

    /**
     * Recompile the complex type when new members are added to the substitution group of an element
     *
     * @param compiler user for error reporting
     * @throws net.sf.saxon.type.SchemaException
     *          if an error is encountered
     */

    public void recompile(SchemaCompiler compiler) throws SchemaException {
        if (debug) {
            getConfiguration().getLogger().info("Recompiling type " + getDescription());
        }
        compiled = false;
        compile(compiler);
        // See bug 4554. Generation of the finite state machine is now deferred, so the expensive part
        // of the operation is only performed once, at the end
    }

    /**
     * Cancel validation and compilation: this happens when the complex type is redefined
     */

    public void cancelValidation() {
        setValidationStatus(UNVALIDATED);
        compiled = false;
    }

    /**
     * Get the finite state machine used for validation against this type
     *
     * @return the finite state machine. May be null if this type does not have a complex content
     *         model, or has an <code>&lt;all&gt;</code> model
     */

    public FiniteStateMachine getFiniteStateMachine() {
        return machine;
    }

    /**
     * Get the initial dynamic state of the finite state machine used for validating against
     * this type.
     *
     * @return the initial state of the machine. If this is null, then the complex
     *         type has an empty content model.
     */

    public State getInitialState() {
        AutomatonState as = machine.getInitialState();
        if (as.requiresCounter()) {
            // This doesn't actually happen
            return new CountingState(as, 0);
        } else {
            return as;
        }
    }

    /**
     * Get the particle corresponding to this UserComplexType, combining it with the
     * content model of the base type if this type is derived by extension. The resulting particle,
     * as well as concatenating with the base content model if the type is derived by extension,
     * will be simplified in that pointless particles will be removed and group references
     * will be resolved (unless they have a significant minOccurs or maxOccurs).
     *
     * @param compiler used for error reporting. May be null if the schema has already been fully validated.
     * @return The particle that combines the locally-defined content model with the content model
     *         of the type that this type is extending.
     * @throws SchemaException if an error occurs
     */

    public Particle makeExtendedParticle(SchemaCompiler compiler) throws SchemaException {
        // This is a memo function, the result is saved the first time
        if (extendedParticle != null) {
            return extendedParticle;
        }
        if (compiler != null) {
            lookForCycles(new Stack<SchemaComponent>(), compiler);
        }
        if (getDerivationMethod() == Derivation.DERIVATION_EXTENSION) {
            // For derivation by extension, construct a new Sequence compositor
            // combining the particles from the base type and this type
            ComplexType cbase = (ComplexType) getBaseType();
            if (isAllContent() || cbase.isAllContent()) {
                if (cbase.isEmptyContent()) {
                    extendedParticle = particle;
                } else if (isLocallyEmpty()) {
                    extendedParticle = ((UserComplexType) cbase).getParticle();
                } else {
                    // they must both have All content
                    ModelGroup all = new AllModelGroup();
                    all.setLocator(this);
                    ModelGroupParticle base = (ModelGroupParticle) ((UserComplexType) cbase).makeExtendedParticle(compiler);
                    for (Particle p : base.getGroup().getParticles()) {
                        all.addParticle(p);
                    }
                    for (Particle p : ((ModelGroupParticle) particle).getGroup().getParticles()) {
                        all.addParticle(p);
                    }
                    all.elaborate(compiler);
                    ModelGroupParticle mgp = new ModelGroupParticle(getConfiguration(), this);
                    mgp.setTarget(all);
                    mgp.setLocator(this);
                    extendedParticle = mgp;
                    extendedParticle.setMinOccurs(particle.getMinOccurs());
                    variety = variety == ComplexVariety.MIXED ? ComplexVariety.MIXED : ComplexVariety.ELEMENT_ONLY;
                }
                return extendedParticle;

            } else if (cbase instanceof UserComplexType) {
                ModelGroup seq = new SequenceModelGroup();
                Particle base = ((UserComplexType) cbase).makeExtendedParticle(compiler);
                if (base != null) {
                    seq.addParticle(base);
                }
                if (particle != null) {
                    seq.addParticle(particle);
                }
                if (seq.isPointless(null)) {
                    List<Particle> contentModel = seq.getSimplifiedContentModel();
                    if (contentModel.isEmpty()) {
                        extendedParticle = null;
                        if (variety == ComplexVariety.ELEMENT_ONLY) {
                            variety = ComplexVariety.EMPTY;
                        }
                    } else if (contentModel.size() == 1) {
                        extendedParticle = (Particle) contentModel.get(0);
                    } else {
                        ModelGroupParticle mgp = new ModelGroupParticle(getConfiguration(), this);
                        mgp.setTarget(seq);
                        extendedParticle = mgp;
                    }
                } else {
                    ModelGroupParticle mgp = new ModelGroupParticle(getConfiguration(), this);
                    mgp.setTarget(seq);
                    extendedParticle = mgp;
                }
                if (extendedParticle != null) {
                    extendedParticle.markVulnerableSubParticles();
                }
                return extendedParticle;
            } else if (cbase instanceof AnyType) {
                // it's possible to derive a complex type by extension from xs:anyType. This is a pretty
                // weird thing to do, and it doesn't make sense unless the extension is empty.
                if (isLocallyEmpty()) {
                    // this is equivalent to a content model of xs:any namespace="##any"
                    ElementWildcard any = new ElementWildcard(getConfiguration());
                    any.setMinOccurs(0);
                    any.setMaxOccurs(-1);
                    any.getWildcard().setProcessContents("lax");
                    extendedParticle = any;
                    return extendedParticle;
                } else {
                    setValidationStatus(INVALID);
                    String err = "xs:anyType cannot be extended, because any non-empty extension creates an ambiguity";
                    if (compiler == null) {
                        throw new IllegalStateException(err);
                    } else {
                        compiler.error(err, this);
                    }
                    extendedParticle = particle;
                    return particle;
                }
            } else {
                throw new IllegalStateException("Unknown implementation of ComplexType: " +
                        (cbase == null ? "null" : cbase.getClass()));
            }
        } else if (particle == null) {
            extendedParticle = null;
            return extendedParticle;
        } else {
            Particle p = particle;
            if (p instanceof ModelGroupParticle && p.getMinOccurs() == 1 && p.getMaxOccurs() == 1) {
                ModelGroup group = ((ModelGroupParticle) p).getGroup();
                if (group.getSimplifiedContentModel().size() == 1 && !(group instanceof AllModelGroup)) {
                    // A singleton *all* model group cannot be simplified because this would affect the
                    // rules for derivation by extension.
                    p = group.getSimplifiedContentModel().get(0);
                }
            }
            if (p.isPointless(null) && (p instanceof ModelGroupParticle)) {
                List<Particle> contentModel = ((ModelGroupParticle) p).getGroup().getSimplifiedContentModel();
                if (variety == ComplexVariety.ELEMENT_ONLY && contentModel.isEmpty() &&
                    !(((ModelGroupParticle) p).getGroup() instanceof ChoiceModelGroup) /* bug 2388 */ &&
                        (openContentWildcard == null || !openContentAppliesToEmpty)) {
                    extendedParticle = null;
                    variety = ComplexVariety.EMPTY;
                } else if (contentModel.size() == 1) {
                    extendedParticle = contentModel.get(0);
                } else {
                    extendedParticle = p;
                }
            } else {
                extendedParticle = p;
            }
            if (extendedParticle != null) {
                extendedParticle.markVulnerableSubParticles();
            }
            return extendedParticle;

        }
    }

    /**
     * Get the (extended) particle representing the content model of this complex type.
     * This must be called only on a UserComplexType that has been validated.
     *
     * @return the extended particle (the particle defined on this type, appended where necessary
     *         onto the particle of the type from which it is derived by extension)
     */

    public Particle getParticle() {
        return extendedParticle;
    }

    /**
     * Test whether the content model of this complexType allows empty content
     */

    @Override
    public boolean isEmptiable() throws MissingComponentException {
        Particle p = getParticle();
        return p == null || p.isEmptiable();
    }

    /**
     * Find an element particle within this complex type definition having a given element name
     * (identified by fingerprint), and return the schema type associated with that element particle.
     * If there is no such particle, return null. If the fingerprint matches an element wildcard,
     * return the type of the global element declaration with the given name if one exists, or AnyType
     * if none exists and lax validation is permitted by the wildcard.
     * @param elementName        Identifies the name of the child element within this content model
     * @param considerExtensions if true, and if the element particle is not defined within this type itself,
     *                           try to find it within the types derived by extension from this type, and/or within the open content
     *                           model permitted by the type. If there is one extension that defines the particle, return
     */

    @Override
    public SchemaType getElementParticleType(int elementName, boolean considerExtensions) throws MissingComponentException {
        Particle p = getParticle();
        SchemaType child = p == null ? null : p.getElementParticleType(elementName);
        if (child != null) {
            return child;
        } else if (considerExtensions) {
            if (openContentWildcard != null &&
                    openContentWildcard.matches(elementName, true, getConfiguration(), this)) {
                return AnyType.getInstance();
            }
            for (UserComplexType ext : getExtensionTypes()) {
                // No need to consider further extensions, as iterateExtensionTypes() returns the transitive set
                SchemaType st = ext.getElementParticleType(elementName, false);
                if (st != null) {
                    if (child == null) {
                        child = st;
                    } else if (child == st) {
                        // no action
                    } else {
                        return AnyType.getInstance();
                    }
                }
            }
            return child;
        } else {
            return null;
        }
    }

    /**
     * Find an element particle within this complex type definition having a given element name
     * (identified by fingerprint), and return the cardinality associated with that element particle,
     * that is, the number of times the element can occur within this complex type. The value is one of
     * {@link StaticProperty#EXACTLY_ONE}, {@link StaticProperty#ALLOWS_ZERO_OR_ONE},
     * {@link StaticProperty#ALLOWS_ZERO_OR_MORE}, {@link StaticProperty#ALLOWS_ONE_OR_MORE},
     * If there is no such particle, return {@link StaticProperty#EMPTY}. The method also searches
     * for wildcard particles, element particles in extension types, and wildcards defined as open content.
     *
     * @param elementName        Identifies the name of the child element within this content model
     * @param considerExtensions if true, types derived by extension from this type should be taken into account
     * @return the cardinality of the requested element within this complex type
     */

    @Override
    public int getElementParticleCardinality(int elementName, boolean considerExtensions) throws MissingComponentException {
        int baseCardinality;
        Particle p = getParticle();
        if (p == null) {
            baseCardinality = StaticProperty.EMPTY;
        } else {
            baseCardinality = p.getElementParticleCardinality(elementName);
        }
        // See if it matches open content
        if (openContentWildcard != null &&
                openContentWildcard.matches(elementName, true, getConfiguration(), this)) {
            return StaticProperty.ALLOWS_ZERO_OR_MORE;
        }
        // Now we need to look in all complex types derived from this type by extension. These might allow
        // (additional) occurrences of the sought element particle.
        if (considerExtensions) {
            for (UserComplexType ext : getExtensionTypes()) {
                if (baseCardinality == StaticProperty.ALLOWS_ZERO_OR_MORE) {
                    // quick exit
                    return baseCardinality;
                }
                // No need to consider extensions recursively, since iterateExtensionTypes() is already transitive
                int extCardinality = ext.getElementParticleCardinality(elementName, false);
                baseCardinality = Cardinality.union(baseCardinality, extCardinality);
            }
        }
        return baseCardinality;
    }

    /**
     * Find an attribute use within this complex type definition having a given attribute name
     * (identified by fingerprint), and return the schema type associated with that attribute.
     * If there is no such attribute use, return null. If the fingerprint matches an attribute wildcard,
     * return the type of the global attribute declaration with the given name if one exists, or AnySimpleType
     * if none exists and lax validation is permitted by the wildcard.
     *
     * @param attributeName Identifies the name of the required attribute use within this content model
     */

    @Override
    public SimpleType getAttributeUseType(StructuredQName attributeName) throws SchemaException {
        AttributeGroupDecl ag = getCombinedAttributeGroup();
        AttributeUse use = ag.getAttributeUse(attributeName);
        if (use != null && !use.isProhibited()) {
            return ((AttributeDecl) use.getTarget()).getType();
        }
        SimpleType attType = null;
        for (UserComplexType ext : getExtensionTypes()) {
            SimpleType st = ext.getAttributeUseType(attributeName);
            if (st != null) {
                if (attType == null) {
                    attType = st;
                } else if (attType == st) {
                    // no action
                } else {
                    return AnySimpleType.getInstance();
                }
            }
        }
        if (attType != null) {
            return attType;
        }

        AttributeWildcard awild = ag.getAttributeWildcard(null);
        if (awild == null) {
            return null;
        }
        Wildcard wild = awild.getWildcard();
        if (wild.matches(attributeName, false, getConfiguration(), null)) {
            switch (wild.getProcessContents()) {
                case "skip":
                    return AnySimpleType.getInstance();
                case "strict": {
                    AttributeDecl decl = getConfiguration().getSuperSchema().getAttributeDecl(attributeName);
                    if (decl != null) {
                        return decl.getType();
                    } else {
                        return null;
                    }
                }
                default: { // processContents() = lax
                    AttributeDecl decl = getConfiguration().getSuperSchema().getAttributeDecl(attributeName);
                    if (decl != null) {
                        return decl.getType();
                    } else {
                        return AnySimpleType.getInstance();
                    }
                }
            }
        }
        return null;
    }

    /**
     * Find an attribute use within this complex type definition having a given attribute name
     * (identified by fingerprint), and return the cardinality associated with that attribute,
     * which will always be 0, 1, or 0-or-1.
     * If there is no such attribute use, return 0. If the fingerprint matches an attribute wildcard,
     * return 0-or-1.
     * <p>If there are types derived from this type by extension, search those too.</p>
     *
     * @param attributeName Identifies the name of the required attribute within this content model
     * @return the cardinality associated with the attribute use identified by the fingerprint.
     * If the attribute is optional but either the attribute use or the corresponding attribute
     * declaration defines a default or fixed value, then it is treated as optional (zero-or-one),
     * because we don't know whether fixed/default attribute values are being reported to the
     * application or not: see bug 3072.
     */

    @Override
    public int getAttributeUseCardinality(StructuredQName attributeName) throws SchemaException {
        AttributeGroupDecl ag = getCombinedAttributeGroup();
        AttributeUse use = ag.getAttributeUse(attributeName);
        if (use != null) {
            return use.getCardinality();
        }
        int card = 0;
        for (UserComplexType ext : getExtensionTypes()) {
            int c = ext.getAttributeUseCardinality(attributeName);
            card |= c;
        }
        if (card != 0) {
            return card;
        }
        AttributeWildcard awild = ag.getAttributeWildcard(null);
        if (awild == null) {
            return StaticProperty.EMPTY;
        }
        Wildcard wild = awild.getWildcard();
        if (wild.matches(attributeName, false, getConfiguration(), null)) {
            return StaticProperty.ALLOWS_ZERO_OR_ONE;
        }
        return StaticProperty.EMPTY;
    }

    /**
     * Ask whether this type (or any known type derived from it by extension) allows the element
     * to have one or more attributes.
     *
     * @return true if attributes (other than the standard xsi: attributes) are allowed. The value
     *         false indicates that only the standard attributes in the xsi namespace are permitted.
     */

    @Override
    public boolean allowsAttributes() throws MissingComponentException {
        AttributeGroupDecl ag = getCombinedAttributeGroup();
        if (!ag.getAttributeUses().isEmpty()) {
            return true;
        }
        if (ag.getAttributeWildcard(null) != null) {
            return true;
        }
        for (UserComplexType ext : getExtensionTypes()) {
            if (ext.allowsAttributes()) {
                return true;
            }
        }
        return false;
    }

    /**
     * Ask whether this type (or any known type derived from it by extension) allows the element
     * to have children that match a wildcard
     *
     * @return true if the content model of this type, or its extensions, contains an element wildcard
     */

    @Override
    public boolean containsElementWildcard() throws MissingComponentException {
        if (extendedParticle != null && extendedParticle.containsElementWildcard()) {
            return true;
        }
        for (UserComplexType ext : getExtensionTypes()) {
            if (ext.containsElementWildcard()) {
                return true;
            }
        }
        return false;
    }

    /**
     * Build the context-determined type map for elements
     *
     * @throws SchemaException if an error is encountered
     */

    public void makeContextDeterminedTypeMapForElements() throws SchemaException {
        if (contextDeterminedTypesForElements != null) {
            return;
        }
        contextDeterminedTypesForElements = new HashMap<>();
        gatherContextDeterminedTypesForElements(contextDeterminedTypesForElements);
    }

    private void gatherContextDeterminedTypesForElements(Map<StructuredQName, SchemaType> map) throws SchemaException {
        // Gather from the base type first, so that the subtype wins if the type is redefined
        SchemaType base = getBaseType();
        if (base instanceof UserComplexType) {
            ((UserComplexType) base).gatherContextDeterminedTypesForElements(map);
        }
        Particle p = getParticle();
        if (p != null) {
            // This is a bit inefficient, but we only do it once.
            IntHashSet children = new IntHashSet();
            p.gatherAllPermittedElements(children, true);
            IntIterator kids = children.iterator();
            while (kids.hasNext()) {
                int fp = kids.next();
                SchemaType type = getElementParticleType(fp, false);
                map.put(getNamePool().getStructuredQName(fp), type);
            }
        }
    }

    /**
     * Get the context-determined type for a given child element. Note that this does not consider
     * types derived from this one by extension.
     *
     * @param fp the child element name
     * @return the context-determined type for this element, if there is one, or null otherwise
     */

    public SchemaType getContextDeterminedTypeForElement(StructuredQName fp) {
        if (contextDeterminedTypesForElements == null) {
            return null;
        }
        return contextDeterminedTypesForElements.get(fp);
    }

    /**
     * Build the context-determined type map for attributes
     *
     * @throws net.sf.saxon.type.SchemaException
     *          if an error is encountered
     */

    public void makeContextDeterminedTypeMapForAttributes() throws SchemaException {
        if (contextDeterminedTypesForAttributes != null) {
            return;
        }
        contextDeterminedTypesForAttributes = new HashMap<>();
        gatherContextDeterminedTypesForAttributes(contextDeterminedTypesForAttributes);
    }

    private void gatherContextDeterminedTypesForAttributes(Map<StructuredQName, SimpleType> map) throws SchemaException {
        // Gather from the base type first, so that the subtype wins if the type is redefined
        SchemaType base = getBaseType();
        if (base instanceof UserComplexType) {
            ((UserComplexType) base).gatherContextDeterminedTypesForAttributes(map);
        }
        for (AttributeUse use : getAttributeGroup().getAttributeUses()) {
            if (!use.isProhibited()) {
                StructuredQName fp = use.getTargetComponentName();
                SimpleType type = getAttributeUseType(fp);
                map.put(fp, type);
            }
        }
    }

    /**
     * Get the context-determined type for a given attribute. Note that this does not consider
     * types derived from this one by extension.
     *
     * @param fp the attribute name
     * @return the context-determined type for this attribute, if there is one, or null otherwise
     */

    public SimpleType getContextDeterminedTypeForAttribute(StructuredQName fp) {
        if (contextDeterminedTypesForAttributes == null) {
            return null;
        }
        return contextDeterminedTypesForAttributes.get(fp);
    }

    /**
     * Get a list of all the names of elements that can appear as children of an element having this
     * complex type, as integer fingerprints. If the list is unbounded (because of wildcards or the use
     * of xs:anyType), return null. The result includes elements defined in types that are defined by
     * extension from this type.
     * @param children        a set, initially empty, which on return will hold the names of all permitted
     *                        child elements; if the result contains the value XS_INVALID_NAME, this indicates that it is not possible to enumerate
     *                        all the children, typically because of wildcards. In this case the other contents of the set should
     *                         be ignored
     * @param ignoreWildcards if set to true, wildcards are ignored; only the elements that match explicit
     */

    @Override
    public void gatherAllPermittedChildren(IntHashSet children, boolean ignoreWildcards) throws SchemaException {
        Particle p = getParticle();
        if (p == null) {
            return;
        }
        p.gatherAllPermittedElements(children, ignoreWildcards);
        if (extendedTypes != null) {
            for (Object extendedType : extendedTypes) {
                UserComplexType uct = (UserComplexType) extendedType;
                uct.gatherAllPermittedChildren(children, ignoreWildcards);
            }
        }
    }

    /**
     * Get a list of all the names of elements that can appear as descendants of an element having this
     * complex type, as integer fingerprints. If the list is unbounded (because of wildcards or the use
     * of xs:anyType), return null. The result includes elements defined in types that are defined by
     * extension from this type or other types processed
     *
     * @param descendants an integer set, initially empty, which on return will hold the fingerprints of all permitted
     *                    descendant elements; if the result contains the value -1, this indicates that it is not possible to enumerate
     *                    all the descendants, typically because of wildcards. In this case the other contents of the set should
     *                    be ignored.
     */

    @Override
    public void gatherAllPermittedDescendants(IntHashSet descendants) throws SchemaException {
        Particle p = getParticle();
        if (p == null) {
            return;
        }
        IntHashSet permittedChildren = new IntHashSet(16);
        gatherAllPermittedChildren(permittedChildren, false);

        StructuredQName wildcardDummy = StandardNames.SQ_XS_INVALID_NAME;
        if (permittedChildren.contains(-1)) {
            descendants.add(-1);
            return;
        }
        IntIterator kids = permittedChildren.iterator();
        while (kids.hasNext()) {
            int c = kids.next();
            if (descendants.add(c)) {
                SchemaType childType = getElementParticleType(c, true);
                if (childType == null) {
                    throw new AssertionError("Child particle " + c + " not found");
                }
                if (childType instanceof ComplexType) {
                    if (childType instanceof UserComplexType) {
                        ((UserComplexType) childType).gatherAllPermittedDescendants(descendants);
                    } else {
                        descendants.add(-1);
                        return;
                    }
                }
            }
        }
    }

    /**
     * Assuming an element is a permitted descendant in the content model of this type, determine
     * the type of the element when it appears as a descendant. If it appears with more than one type,
     * return xs:anyType.
     *
     * @param elementName the name of the required descendant element
     * @return the type of the descendant element; null if the element cannot appear as a descendant;
     *         anyType if it can appear with several different types
     */

    @Override
    public SchemaType getDescendantElementType(int elementName) throws SchemaException {
        Set<SchemaType> parentsConsidered = new HashSet<>();
        Set<SchemaType> foundTypes = new HashSet<>();
        getDescendantElementTypes(elementName, parentsConsidered, foundTypes);
        if (foundTypes.isEmpty()) {
            return null;
        }
        if (foundTypes.size() == 1) {
            Iterator<SchemaType> iter = foundTypes.iterator();
            if (iter.hasNext()) {
                return iter.next();
            }
        }
        return AnyType.getInstance();
    }

    private void getDescendantElementTypes(int elementName,
                                           Set<SchemaType> parentsConsidered,
                                           Set<SchemaType> foundTypes) throws SchemaException {
        Particle p = getParticle();
        if (p == null) {
            return;
        }

        // See if the requested element can appear as a child
        SchemaType targetType = getElementParticleType(elementName, true);
        if (targetType != null) {
            foundTypes.add(targetType);
            if (targetType == AnyType.getInstance()) {
                return;
            }
        }

        IntHashSet permittedChildren = new IntHashSet(16);
        gatherAllPermittedChildren(permittedChildren, false);

        if (permittedChildren.contains(-1)) {
            foundTypes.add(AnyType.getInstance());
            return;
        }
        IntIterator kids = permittedChildren.iterator();
        while (kids.hasNext()) {
            int c = kids.next();
            SchemaType childType = getElementParticleType(c, true);
            if (childType == null) {
                throw new AssertionError("Child particle " + c + " not found");
            }
            if (!parentsConsidered.contains(childType)) {
                parentsConsidered.add(childType);
                if (childType instanceof ComplexType) {
                    if (childType instanceof UserComplexType) {
                        ((UserComplexType) childType).getDescendantElementTypes(elementName, parentsConsidered, foundTypes);
                    } else {
                        foundTypes.add(childType);
                    }
                }
            }
        }
    }

    /**
     * Assuming an element is a permitted descendant in the content model of this type, determine
     * the cardinality of the element when it appears as a descendant.
     *
     * @param elementFingerprint the name of the required descendant element
     * @return the cardinality of the descendant element within this complex type
     */

    @Override
    public int getDescendantElementCardinality(int elementFingerprint) throws SchemaException {
        int cardinality = StaticProperty.EMPTY;
        IntHashSet children = new IntHashSet();
        gatherAllPermittedChildren(children, false);
        if (children.contains(-1)) {
            return StaticProperty.ALLOWS_ZERO_OR_MORE;
        }
        IntIterator kids = children.iterator();
        while (kids.hasNext()) {
            int child = kids.next();
            int childCard = getElementParticleCardinality(child, true);
            if (child == elementFingerprint) {
                cardinality = Cardinality.sum(cardinality, childCard);
                if (Cardinality.allowsMany(cardinality)) {
                    // take an early bath
                    return cardinality;
                }
            }
            SchemaType childType = getElementParticleType(child, true);
            if (childType != null && childType.isComplexType()) {
                IntHashSet childDescendants = new IntHashSet();
                ((ComplexType) childType).gatherAllPermittedDescendants(childDescendants);
                if (childDescendants.contains(-1)) {
                    // The content model includes a wildcard
                    return StaticProperty.ALLOWS_ZERO_OR_MORE;
                }
                if (childDescendants.contains(elementFingerprint)) {
                    cardinality = Cardinality.sum(cardinality,
                        Cardinality.multiply(childCard, ((ComplexType) childType).getDescendantElementCardinality(elementFingerprint)));
                    if (cardinality == StaticProperty.ALLOWS_ZERO_OR_MORE) {
                        // take an early bath
                        return cardinality;
                    }
                }
            }
        }
        return cardinality;
    }

    /**
     * Test whether this complex type is subsumed by another complex type. The algorithm
     * used is as published by Thompson and Tobin, XML Europe 2003.
     *
     * @param base     the other type (the type that this is derived from by restriction, validly or otherwise)
     * @param compiler used for error reporting
     * @return null indicating that this type does indeed subsume the other; or a string indicating
     *         why it doesn't. If the string starts with "?", Saxon isn't able to prove subsumption one way
     *         or the other.
     * @throws net.sf.saxon.type.SchemaException
     *          if an error is encountered
     */

    /*@Nullable*/
    public String isSubsumedBy(ComplexType base, SchemaCompiler compiler) throws SchemaException {
        if (base instanceof AnyType) {
            return null;
        } else if (base instanceof Untyped) {
            return "Untyped does not subsume any other type";
        } else {
            UserComplexType cbase = (UserComplexType) base;
            Wildcard bw = cbase.getOpenContentWildcard();
            Wildcard rw = this.getOpenContentWildcard();
            if (bw == null) {
                if (rw != null) {
                    // Restricted type has open content; base type does not. This is unlikely
                    // to be a valid restriction. But for the sake of test open022, we handle one
                    // special case: where the base type model is a repeating wildcard that subsumes
                    // the open content wildcard of the derived type
                    Particle bp = cbase.getParticle();
                    if (bp instanceof ElementWildcard &&
                            bp.getMinOccurs() == 0 &&
                            bp.getMaxOccurs() == -1 &&
                            (this.isEmptyContent() || ((ModelGroup) this.getParticle().getTarget()).getContentModelSize() == 0) &&
                            rw.isSubset(((ElementWildcard) bp).getWildcard(), getNamePool())) {
                        return null;
                    }
                    return "Restricted type has open content; base type does not";
                }
            } else {
                if (rw != null) {
                    if (!rw.isSubset(bw, compiler.getNamePool())) {
                        return "Restricted type has an open content wildcard that matches names which are " +
                                "not matched by the open content of the base type";
                    }
                    if (bw.compareStrength(rw) > 0) {
                        return "The processContents on the open content wildcard of the restricted type" +
                                " is weaker than that on the open content wildcard of the base type";
                    }
                    if (!isLocallyEmpty()) {
                        boolean bInterleaved = cbase.getOpenContentMode() == OpenContentVariety.INTERLEAVE;
                        boolean rInterleaved = this.getOpenContentMode() == OpenContentVariety.INTERLEAVE;
                        if (rInterleaved && !bInterleaved) {
                            return "Restricted type allows interleaved open content, but " +
                                    "base type only allows suffixed open content";
                        }
                    }
                }
            }
            return cbase.subsumes(this, compiler);
        }
    }

    /**
     * Test whether this complex type subsumes another complex type. The algorithm
     * used is as published by Thompson and Tobin, XML Europe 2003.
     *
     * @param sub      the other type (the type that is derived by restriction, validly or otherwise)
     * @param compiler used for error reporting
     * @return null indicating that this type does indeed subsume the other; or a string indicating
     *         why it doesn't. If the string starts with "?", Saxon isn't able to prove subsumption
     *         one way or the other.
     */

    private String subsumes(UserComplexType sub, SchemaCompiler compiler) throws SchemaException {

        computeIsAllContent();
        if (!sub.getAssertions().containsAll(getAssertions())) {
            return "The assertions defined on complex type " + getDescription() +
                    " are not a subset of the assertions defined on the type " + sub.getDescription();
        }
        sub.computeIsAllContent();
        if (isAllContent()) {
            return AllModelGroup.allSubsumes(this, sub, compiler);
        } else if (sub.isAllContent()) {
            return subsumesAll(sub, compiler);
        } else if (sub.isEmptyContent()) {
            if (isEmptiable()) {
                return null;
            } else {
                return "Derived type has empty content model, but base type does not allow empty content";
            }
        } else {
            compiler.scheduleAutomatonCheck(this, sub);
            return null;
            //return FiniteStateMachine.subsumesMachine(machine, sub.machine, compiler);
        }
    }

    /**
     * Determine whether one element term matches another. This is a simplified version of the procedure
     * in Edge.matches(), used only for xs:all content models
     *
     * @param base     the element declaration used in the base type
     * @param sub      the element declaration used in the derived type
     * @param compiler used for reporting warnings
     * @return null if the edges match, otherwise a string explaining why there is no match.
     */

    protected static String elementParticleMatches(ElementDecl base, ElementDecl sub, SchemaCompiler compiler) throws MissingComponentException {
        // extracted from AutomatonState.matches()

        SchemaType baseType = base.getType();
        SchemaType subType = sub.getType();

        // (a) If base is not nillable, then sub must not be nillable

        if (sub.isNillable() && !base.isNillable()) {
            return "Element " + eName(sub) + " in derived type is nillable but element "
                    + eName(base) + " in base type is not";
        }

        // (b) If base has a fixed value constraint, then sub must have a fixed value constraint with
        // the same value

        if (base.getFixedValue() != null) {
            if (sub.getFixedValue() == null) {
                return "Element " + eName(base) + " in base type has a fixed value, but element " + eName(sub) +
                        " in derived type does not";
            }
            if (!SimpleTypeComparison.getInstance().equal(sub.getFixedValue(), base.getFixedValue())) {
                return "Element " + eName(sub) + " in derived type has a different fixed value from element "
                        + eName(base) + " in base type";
            }
        }

        // (c) DE's type definition must be the same as BE's, or be validly derived from it
        boolean elementTypeOK = true;
        String reason = null;
        if (subType.isSimpleType() /*&& baseType.isSimpleType()*/) {
            reason = UserSimpleType.isTypeDerivationOK((SimpleType) subType, baseType, 0);
            elementTypeOK = reason == null;
        } else {
            while (!baseType.isSameType(subType)) {
                if (subType.getDerivationMethod() != Derivation.DERIVATION_RESTRICTION) {
                    elementTypeOK = false;
                    break;
                }
                subType = subType.getBaseType();
                if (subType == null) {
                    elementTypeOK = false;
                    break;
                }
            }
        }
        if (!elementTypeOK) {
            return "The type of element " + eName(sub) + " in the restricted content model is not validly derived from the " +
                    "type of the corresponding element " + eName(sub) + " in the base content model" +
                    (reason == null ? "" : ". " + reason);
        }

        // (d) If B has a fixed value constraint, D must have a fixed value constraint with the same value
        if (base.getFixedValue() != null &&
                !SimpleTypeComparison.getInstance().equal(base.getFixedValue(), sub.getFixedValue())) {
            return "The fixed value of element " + eName(sub) + " in the base content model is not reflected in the corresponding " +
                    "element " + eName(sub) + " of the restricted content model";
        }

        // (e) DE's identity constraints must be a superset of BE's identity constraints. Currently
        // we only check that there are at least as many constraints of each type, and if there's at least
        // one then we give a warning that we don't know how to check them for consistency
        HashSet<IdentityConstraint> baseUniqueSet = new HashSet<>(4);
        HashSet<IdentityConstraint> baseKeySet = new HashSet<>(4);
        HashSet<IdentityConstraint> baseKeyRefSet = new HashSet<>(4);
        for (IdentityConstraintReference icr : base.getIdentityConstraints()) {
            IdentityConstraint ic = icr.getTarget();
            if (ic instanceof Unique) {
                baseUniqueSet.add(ic);
            } else if (ic instanceof Key) {
                baseKeySet.add(ic);
            } else if (ic instanceof KeyRef) {
                baseKeyRefSet.add(ic);
            }
        }
        HashSet<IdentityConstraint> subUniqueSet = new HashSet<>(4);
        HashSet<IdentityConstraint> subKeySet = new HashSet<>(4);
        HashSet<IdentityConstraint> subKeyRefSet = new HashSet<>(4);
        for (IdentityConstraintReference icr : sub.getIdentityConstraints()) {
            IdentityConstraint ic = icr.getTarget();
            if (ic instanceof Unique) {
                subUniqueSet.add(ic);
            } else if (ic instanceof Key) {
                subKeySet.add(ic);
            } else if (ic instanceof KeyRef) {
                subKeyRefSet.add(ic);
            }
        }
        if (baseUniqueSet.size() > subUniqueSet.size()) {
            return "Element " + eName(base) + "in the base type has an xs:unique constraint that " +
                    "is not present in the restricted type";
        }
        if (baseKeySet.size() > subKeySet.size()) {
            return "Element " + eName(sub) + " in the base type has an xs:key constraint that is not present " +
                    "in the restricted type";
        }
        if (baseKeyRefSet.size() > subKeyRefSet.size()) {
            return "Element " + eName(sub) + " in the base type has an xs:keyref constraint that is not present " +
                    "in the restricted type";
        }
        if (baseUniqueSet.size() + baseKeySet.size() + baseKeyRefSet.size() > 0) {
            compiler.warning("Saxon is not able to verify that the identity constraints in element " +
                    eName(sub) + " are compatible with those in the the base type",
                    sub);
            // TODO: we could actually compare the constraints, but it's not 100% clear what the rules are
        }

        // (g) Check that the disallowed substitutions (block) of B are a superset of those
        // of D
        if ((base.getBlock() & ~sub.getBlock()) != 0) {
            return "The disallowed substitutions of element " + eName(sub) + "in the restricted content model (@block) must be " +
                    "a superset of those in the base content model";
        }
        return null;
    }

    private static String eName(ElementDecl e) {
        return Err.wrap(e.getDisplayName(), Err.ELEMENT);
    }

    /**
     * Test whether the particle of this complex type subsumes a complex type defined using xs:all
     *
     * @param sub      the derived type
     * @param compiler used for error reporting
     * @return null if this type validly subsumes the derived type; otherwise, a string that can be used
     *         as an error message to explain why the derivation is not valid
     */

    /*@Nullable*/
    public String subsumesAll(UserComplexType sub, SchemaCompiler compiler) throws SchemaException {
        // We've already handled the case where the base type itself uses xs:all. The only other cases
        // that can succeed is where the base type uses an xs:any wildcard, or where the derived xs:all type
        // is a singleton
        AllModelGroup subAll = (AllModelGroup) sub.getAllCompositor().getGroup();
        if (subAll.getNumberOfElementParticles() == 1) {
            ModelGroup seq = new SequenceModelGroup();
            seq.addParticle(subAll.getSimplifiedContentModel().get(0));
            Particle temp = sub.getParticle();
            sub.setParticle(subAll.getSimplifiedContentModel().get(0));
            String message = subsumes(sub, compiler);
            sub.setParticle(temp);
            return message;
        }
        Particle base = makeExtendedParticle(compiler);
        EnterpriseConfiguration config = getConfiguration();
        if (base instanceof ElementWildcard) {
            int min = 0;
            int max = 0;
            for (Particle p : subAll.getParticles()) {
                min += p.getMinOccurs();
                max += p.getMaxOccurs();
                if (!((ElementWildcard) base).getWildcard().matches(p.getTargetComponentName(), true, config, this)) {
                    NamespaceUri uri = p.getTargetComponentName().getURI();
                    return "The base type does not allow elements in " +
                            (uri.isEmpty() ? "no namespace" : "namespace " + uri);
                }
            }
            if (min < base.getMinOccurs()) {
                return "The derived type allows fewer elements to appear than the base type requires in its @minOccurs";
            }
            if (base.getMaxOccurs() >= 0 && max > base.getMaxOccurs()) {
                return "The derived type allows more elements to appear than the base type permits in its @maxOccurs";
            }
            return null;
        } else {
            return "A type using xs:all cannot be validly derived from this base type";
        }


    }

    /**
     * Analyze an expression to see whether the expression is capable of delivering a value of this
     * type.
     *
     * @param expression the expression that delivers the content. If this is a Block, the sequence of
     *                   instructions within the block is checked by this method, and the individual instructions are
     *                   checked by a callback; in other cases the method calls expression.checkPermittedContents() which may pass
     *                   the analysis onto other expressions.
     * @param kind       the node kind whose content is being delivered: {@link Type#ELEMENT},
     *                   {@link Type#ATTRIBUTE}, or {@link Type#DOCUMENT}
     * @throws net.sf.saxon.trans.XPathException
     *          if the expression will never deliver a value of the correct type (that is, of this type)
     */

    @Override
    public void analyzeContentExpression(Expression expression, int kind) throws XPathException {
        Configuration config = getConfiguration();
        TypeHierarchy th = config.getTypeHierarchy();

        // If the complex type has a mandatory attribute, check that the content expression includes
        // an expression capable of delivering an attribute with the right name
        for (AttributeUse use : getAttributeGroup().getAttributeUses()) {
            if (use.isRequired()) {
                if (th.relationship(expression.getItemType(), NodeKindTest.ATTRIBUTE) == Affinity.DISJOINT) {
                    XPathException err = new XPathException("Type " + getDescription() + " defines a mandatory attribute " +
                            use.getTargetComponentName().getDisplayName() +
                            ", but there is no instruction to create such an attribute");
                    err.setLocation(expression.getLocation());
                    err.setIsStaticError(true);
                    throw err;
                }

                if (expression instanceof Block) {
                    Operand[] children = ((Block) expression).getOperanda();
                    boolean foundPossible = false;
                    if (children != null) {
                        for (Operand o : children) {
                            Expression child = o.getChildExpression();
                            if (child instanceof FixedAttribute) {
                                if (((FixedAttribute) child).getAttributeFingerprint() ==
                                            use.getAttributeDeclaration().getFingerprint()) {
                                    foundPossible = true;
                                    break;
                                }
                            } else if (th.relationship(child.getItemType(), NodeKindTest.ATTRIBUTE) != Affinity.DISJOINT) {
                                foundPossible = true;
                                break;
                            }
                        }
                    }
                    if (!foundPossible) {
                        XPathException err = new XPathException("Type " + getDescription() + " defines a mandatory attribute " +
                            use.getTargetComponentName().getDisplayName() +
                                ", but there is no instruction to create such an attribute");
                        err.setLocation(expression.getLocation());
                        err.setIsStaticError(true);
                        throw err;
                    }

                }
            }
        }

        // For a Block, check whether the sequence of elements generated is consistent with the content model
        if (expression instanceof Block) {
            Operand[] children = ((Block) expression).getOperanda();
            if (children == null) {
                return;
            }
            for (Operand o : children) {
                o.getChildExpression().checkPermittedContents(this, false);
            }
            // if all the children that can deliver elements have known element names, check that the
            // sequence of element names is valid against the content model of the parentType.
            // Keep checking until we find something that's dynamic

            if (getFiniteStateMachine() != null && !isAllContent()) {
                State state = getInitialState();
                for (Operand o : children) {
                    Expression child = o.getChildExpression();
                    if (child instanceof FixedElement) {
                        NodeName elementName = ((FixedElement) child).getFixedElementName();
                        int fingerprint = ((FixedElement) child).getFixedElementName().getFingerprint();
                        final Edge edge = state.getTransition(fingerprint, this);
                        if (edge == null) {
                            FiniteStateMachine machine = getFiniteStateMachine();
                            Wildcard openContentWildcard = machine.getOpenContentWildcard();
                            if (openContentWildcard != null &&
                                    openContentWildcard.matches(elementName, true, getConfiguration(), this)) {
                                if (machine.isOpenContentInterleaved()) {
                                    // interleaved open content: leave the state unchanged
                                    continue;
                                } else if (state.isFinalState()) {
                                    // suffix wildcard: from now on we can only match the open content wildcard
                                    state = SuffixState.getInstance();
                                    continue;
                                }
                            }
                            if (state.isFinalState() && !getExtensionTypes().isEmpty()) {
                                // Doing shallow (compile time) validation, we need to allow for the fact that the validation
                                // might actually be against a type derived by extension. Test qischema064
                                break;
                            }
                            CharSequence sb = state.listAllowedElements();
                            String message;
                            if (state instanceof SuffixState) {
                                message = "The content model does not allow element " +
                                        Err.wrap(elementName.getDisplayName(), Err.ELEMENT) +
                                        " to appear after an element that matches the suffix open content wildcard. ";
                            } else {
                                message = "The content model does not allow element " +
                                        Err.wrap(elementName.getDisplayName(), Err.ELEMENT) +
                                        " to appear here" +
                                        (openContentWildcard == null ? "" : " (it does not match the open content wildcard)") +
                                        ". " + sb;
                            }
                            XPathException ve = new XPathException(message);
                            ve.setErrorCode("XPTY0004");
                            ve.setLocation(child.getLocation());
                            throw ve;
                        }
                        try {
                            state = edge.makeTransition(state);
                        } catch (ValidationException e) {
                            XPathException ve = new XPathException(e.getMessage());
                            ve.setErrorCode("XPTY0004");
                            ve.setLocation(expression.getLocation());
                            throw ve;
                        }

                    } else {
                        ItemType t = child.getItemType();
                        if (th.relationship(t, NodeKindTest.ELEMENT) != Affinity.DISJOINT) {
                            // instruction might or might not produce an element. Abandon static checking.
                            return;
                        }
                    }
                }
                if (!state.isFinalState()) {
                    int len = children.length;
                    if (len == 0) {
                        XPathException ve = new XPathException("The generated content is empty: the schema requires at least one child element");
                        ve.setErrorCode("XPTY0004");
                        ve.setLocation(expression.getLocation());
                        throw ve;
                    } else {
                        XPathException ve = new XPathException("The generated content is incomplete: the schema requires further child elements");
                        ve.setErrorCode("XPTY0004");
                        ve.setLocation(children[len - 1].getChildExpression().getLocation());
                        throw ve;
                    }
                }
            }
        } else {
            expression.checkPermittedContents(this, true);
        }
    }

    /**
     * Get the typed value of a node that is annotated with this schema type.
     *
     * @param node the node whose typed value is required
     * @return the typed value. If requireSingleton is set to true, the result will always be an
     *         AtomicValue. In other cases it may be a Value representing a sequence whose items are atomic
     *         values.
     * @since 8.5
     */

    @Override
    public AtomicSequence atomize(NodeInfo node) throws XPathException {
        if (isSimpleContent()) {
            if (node.isNilled()) {
                return EmptyAtomicSequence.getInstance();
            }
            try {
                return getSimpleContentType().atomize(node);
            } catch (MissingComponentException e) {
                throw new XPathException("Can't get typed value; node is annotated with an incomplete type definition");
            }
        } else if (isMixedContent()) {
            if (node.isNilled()) {
                return EmptyAtomicSequence.getInstance();
            }
            return StringValue.makeUntypedAtomic(node.getUnicodeStringValue());
        } else if (isEmptyContent()) {
            return EmptyAtomicSequence.getInstance();
        } else {
            XPathException err = new XPathException("Cannot get the typed value of an element (" + node.getDisplayName() + ") with element-only content");
            err.setErrorCode("FOTY0012");
            err.setIsTypeError(true);
            throw err;
        }
    }

    /**
     * Check that a type is validly derived from a given type. This method is used only for complex types.
     *
     * @param derived the derived type
     * @param base    the type from which this type is derived
     * @param block   the derivations that are blocked by the relevant element declaration
     * @throws net.sf.saxon.type.SchemaException
     *          if the derivation is not allowed
     */

    public static void checkTypeDerivation(ComplexType derived, SchemaType base, int block) throws SchemaException {
        boolean sameType = derived.isSameType(base);

        // clause 1

        if (!sameType && ((derived.getDerivationMethod() & block) != 0)) {
            throw new SchemaException("Derivation of the requested type " +
                    derived.getDescription() +
                    " is blocked either by the base type " +
                    derived.getDescription() + " or by the element declaration");
        }

        // clause 2.1

        if (sameType) {
            return;
        }

        // clause 2.2

        SchemaType baseTypeOfD = derived.getBaseType();
        if (baseTypeOfD == null) {
            baseTypeOfD = AnyType.getInstance();
        }
        if (base.isSameType(baseTypeOfD)) {
            return;
        }

        // clause 2.3.1

        if (baseTypeOfD == AnyType.getInstance()) {
            throw new SchemaException("The requested type " +
                    derived.getDescription() +
                    " is not derived from the declared type " +
                    base.getDescription());
        }

        // clause 2.3.2

        if (baseTypeOfD.isComplexType()) {
            // clause 2.3.2.1
            try {
                checkTypeDerivation((ComplexType) baseTypeOfD, base, block);
            } catch (SchemaException err) {
                throw new SchemaException("The requested type " +
                        derived.getDescription() +
                        " is not validly derived from the declared type " +
                        base.getDescription());
            }
        } else {
            // clause 2.3.2.2
            String err = UserSimpleType.isTypeDerivationOK((SimpleType) baseTypeOfD, base, block);
            if (err != null) {
                throw new SchemaException("The requested type " +
                        derived.getDescription() +
                        " is not validly derived from the declared type " +
                        base.getDescription() + ". " + err);
            }
        }
    }

    /**
     * Elaborate the schema component: after reloading a serialized schema component model,
     * this expands the component with derived information needed during validation episodes.
     * The model is assumed to be valid.
     *
     * @param compiler the schema compiler
     * @throws SchemaException if the schema is found to be invalid
     */

    @Override
    public void elaborate(SchemaCompiler compiler) throws SchemaException {
        if (getValidationStatus() != VALIDATED) {
            // The base type must be elaborated first, to ensure that the context-determined type
            // map is built correctly. See test wild068.
            SchemaType base = getBaseType();
            if (base instanceof UserComplexType) {
                ((UserComplexType) base).elaborate(compiler);
            }
            xsdVersion = compiler.getLanguageVersion();
            extendedParticle = particle;
            if (particle != null) {
                particle.elaborate(compiler);
            }
            makeCombinedAttributeGroup(null);
            computeIsAllContent();
            extendedAttributes.buildCounterMap();
            for (AttributeUse att : extendedAttributes.getAttributeUses()) {
                att.elaborate(compiler);
            }
            makeContextDeterminedTypeMapForAttributes();
            makeContextDeterminedTypeMapForElements();
            setValidationStatus(VALIDATED);
        }
    }


    /**
     * Serialize the schema component
     */
    @Override
    public void serialize(SchemaModelSerializer serializer) throws XPathException {
        String id = serializer.getId(this, true);
        serializer.startElement("complexType");
        serializer.emitAttribute("id", id);
        if (!isAnonymousType()) {
            serializer.emitAttribute("name", getName());
            if (!getTargetNamespace().isEmpty()) {
                serializer.emitAttribute("targetNamespace", getTargetNamespace().toString());
            }
        }
        serializer.emitAttribute("base", serializer.getTypeLink(getBaseType()));
        if (finalProhibitions != 0) {
            String ex = "";
            if ((finalProhibitions & Derivation.DERIVATION_EXTENSION) != 0) {
                ex = (ex.isEmpty() ? "" : " ") + "extension";
            }
            if ((finalProhibitions & Derivation.DERIVATION_RESTRICTION) != 0) {
                ex += (ex.isEmpty() ? "" : " ") + "restriction";
            }
            serializer.emitAttribute("final", ex);
        }
        if (block != 0) {
            String dis = "";
            if ((block & Derivation.DERIVATION_EXTENSION) != 0) {
                dis = "extension";
            }
            if ((block & Derivation.DERIVATION_RESTRICTION) != 0) {
                dis += (dis.isEmpty() ? "" : " ") + "restriction";
            }
            serializer.emitAttribute("block", dis);
        }
        serializer.emitAttribute("derivationMethod",
                getDerivationMethod() == Derivation.DERIVATION_EXTENSION ? "extension" : "restriction");
        serializer.emitAttribute("abstract", isAbstract() ? "true" : "false");
        String varietyName;
        switch (variety) {
            case EMPTY:
                varietyName = "empty";
                break;
            case MIXED:
                varietyName = "mixed";
                break;
            case SIMPLE:
                varietyName = "simple";
                break;
            case ELEMENT_ONLY:
            default:
                varietyName = "element-only";
                break;
        }
        serializer.emitAttribute("variety", varietyName);
        if (isSimpleContent()) {
            SimpleType st = getSimpleContentType();
            serializer.emitAttribute("simpleType", serializer.getTypeLink(st));
        }
        if (openContentWildcard != null &&
                openContentMode != OpenContentVariety.ABSENT && openContentMode != OpenContentVariety.NONE) {
            String modeName;
            switch (openContentMode) {
                default:
                    throw new IllegalStateException("openContent mode must be suffix or interleave");
                case INTERLEAVE:
                    modeName = "interleave";
                    break;
                case SUFFIX:
                    modeName = "suffix";
                    break;
            }

            String openContentId = serializer.getId(openContentWildcard, false);
            serializer.startElement("openContent");
            serializer.emitAttribute("mode", modeName);
            serializer.emitAttribute("wildcard", openContentId);
            serializer.endElement();
        }
        getCombinedAttributeGroup().serializeContents(serializer);
        if (isComplexContent()) {
            Particle particle = getParticle();
            if (particle != null) {
                particle.serializeParticle(serializer);
            }
            if (machine.getInitialState() != null) {
                machine.serialize(serializer);
            }
        }
        if (hasAssertions()) {
            for (Assertion assertion : getAssertions()) {
                assertion.serialize(serializer);
            }
        }
        serializer.endElement();
    }

    @Override
    public Function getComponentAsFunction() {
        return getComponentAsFunction(this);
    }

    public static Function getComponentAsFunction(final ComplexType type) {
        CallableDelegate callable = new CallableDelegate((context, arguments) -> {
            String key = arguments[0].head().getStringValue();
            switch (key) {
                case "class":
                    return StringValue.bmp("Complex Type Definition");
                case "implementation":
                    return new ObjectValue<>(type);
                case "name":
                    if (type.isAnonymousType()) {
                        return EmptySequence.getInstance();
                    } else {
                        return new StringValue(type.getName(), BuiltInAtomicType.NCNAME);
                    }
                case "target namespace":
                    if (type.isAnonymousType()) {
                        return EmptySequence.getInstance();
                    } else {
                        return new AnyURIValue((type.getTargetNamespace().toUnicodeString()));
                    }
                case "base type definition":
                    return type.getBaseType().getComponentAsFunction();
                case "final": {
                    List<AtomicValue> list = new ArrayList<>();
                    int f = type.getFinalProhibitions();
                    if ((f & Derivation.DERIVATION_RESTRICTION) != 0) {
                        list.add(StringValue.bmp("restriction"));
                    }
                    if ((f & Derivation.DERIVATION_EXTENSION) != 0) {
                        list.add(StringValue.bmp("extension"));
                    }
                    return new AtomicArray(list);
                }
                case "context":
                    if (type instanceof UserComplexType && !type.isAnonymousType()) {
                        return EmptySequence.getInstance();
                    } else {
                        // TODO
                        return EmptySequence.getInstance();
                    }
                case "derivation method":
                    return new StringValue(
                            type.getDerivationMethod() == Derivation.DERIVATION_RESTRICTION ? "restriction" : "extension");
                case "abstract":
                    return BooleanValue.get(type.isAbstract());
                case "attribute uses":
                    if (type instanceof UserComplexType) {
                        List<Item> list = new ArrayList<>();
                        for (AttributeUse use : ((UserComplexType) type).getAttributeGroup().getLocalAttributes()) {
                            list.add(use.getComponentAsFunction());
                        }
                        return SequenceExtent.makeSequenceExtent(list);
                    } else {
                        return EmptySequence.getInstance();
                    }
                case "attribute wildcard":
                    if (type instanceof UserComplexType) {
                        AttributeWildcard wild = ((UserComplexType) type).getAttributeGroup().getLocalAttributeWildcard();
                        return wild.getComponentAsFunction();
                    } else {
                        // TODO: AnyType has a wildcard?
                        return EmptySequence.getInstance();
                    }
                case "content type":
                    CallableDelegate content = new CallableDelegate((context2, arguments2) -> {
                            String key2 = arguments2[0].head().getStringValue();
                            switch (key2) {
                                case "class":
                                    return StringValue.bmp("Content Type");
                                case "variety":
                                    String variety;
                                    switch (type.getVariety()) {
                                        case ELEMENT_ONLY:
                                            variety = "element-only";
                                            break;
                                        case EMPTY:
                                            variety = "empty";
                                            break;
                                        case MIXED:
                                            variety = "mixed";
                                            break;
                                        case SIMPLE:
                                        default:
                                            variety = "simple";
                                            break;
                                    }
                                    return StringValue.bmp(variety);
                                case "particle":
                                    if (type.getVariety() == ComplexVariety.MIXED || type.getVariety() == ComplexVariety.ELEMENT_ONLY) {
                                        return ((UserComplexType) type).getParticle().getComponentAsFunction();
                                        // TODO: what about anyType, etc?
                                    } else {
                                        return EmptySequence.getInstance();
                                    }

                                case "open content":
                                    if (type instanceof UserComplexType) {
                                        final UserComplexType ctype = (UserComplexType) type;
                                        final String mode = ctype.getOpenContentModeString();
                                        if ("interleave".equals(mode) || "suffix".equals(mode)) {
                                            CallableDelegate open = new CallableDelegate((context3, arguments3) -> {
                                                String key3 = arguments[0].head().getStringValue();
                                                switch (key3) {
                                                    case "class":
                                                        return StringValue.bmp("Open Content");
                                                    case "mode":
                                                        return new StringValue(mode);
                                                    case "wildcard":
                                                        return ctype.getOpenContentWildcard().getComponentAsFunction();
                                                    default:
                                                        return EmptySequence.getInstance();
                                                }
                                            });
                                            return new CallableFunction(1, open, SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
                                        } else {
                                            return EmptySequence.getInstance();
                                        }
                                    } else {
                                        return EmptySequence.getInstance();
                                    }
                                case "simple type definition":
                                    if (type.isSimpleContent()) {
                                        return type.getSimpleContentType().getComponentAsFunction();
                                    } else {
                                        return EmptySequence.getInstance();
                                    }
                                default:
                                    return EmptySequence.getInstance();
                            }

                    });
                    return new CallableFunction(1, content, SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
                case "prohibited substitutions": {
                    List<AtomicValue> list = new ArrayList<>();
                    int s = type.getBlock();
                    if ((s & Derivation.DERIVATION_EXTENSION) != 0) {
                        list.add(StringValue.bmp("extension"));
                    }
                    if ((s & Derivation.DERIVATION_RESTRICTION) != 0) {
                        list.add(StringValue.bmp("restriction"));
                    }
                    return new AtomicArray(list);
                }
                case "assertions":
                    if (type instanceof UserComplexType) {
                        List<Item> list = new ArrayList<>();
                        for (Assertion assertion : ((UserComplexType) type).getAssertions()) {
                            list.add(assertion.getComponentAsFunction());
                        }
                        return SequenceExtent.makeSequenceExtent(list);
                    } else {
                        return EmptySequence.getInstance();
                    }
                default:
                    return EmptySequence.getInstance();
            }
        });
        return new CallableFunction(1, callable, SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
    }
}

// Copyright (c) 2004-2022 Saxonica Limited
