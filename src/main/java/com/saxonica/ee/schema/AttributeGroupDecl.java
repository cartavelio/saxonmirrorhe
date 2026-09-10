package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.CallableDelegate;
import net.sf.saxon.expr.sort.SimpleTypeComparison;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;
import net.sf.saxon.z.IntHashMap;
import net.sf.saxon.z.IntToIntArrayMap;
import net.sf.saxon.z.IntToIntHashMap;
import net.sf.saxon.z.IntToIntMap;

import java.util.*;

import static net.sf.saxon.type.SchemaValidationStatus.INVALID;
import static net.sf.saxon.type.SchemaValidationStatus.UNVALIDATED;
import static net.sf.saxon.type.SchemaValidationStatus.VALIDATED;
import static net.sf.saxon.type.SchemaValidationStatus.VALIDATING;

/**
 * An XML Schema Attribute Group Declaration. This represents a named attribute group,
 * but not an attribute group reference. As well as attribute groups explicitly written
 * in the schema using xsl:attributeGroup, this class is also used to represent any
 * collection of attributes defined using xsl:attribute elements as part of a complex
 * type definition.
 * <p>In contrast with the Attribute Group Definition schema component as defined in the
 * W3C specification, the model retains explicit references to subsidiary attribute group
 * definitions. However, the method {@link #getAttributeUses()} performs the transitive
 * closure of these references, returning all the attributes at the leaves of the tree.</p>
 */

public final class AttributeGroupDecl extends SchemaStructure
        implements UserSchemaComponent, SerializableSchemaComponent {

    /**
     * The collection of attributes for this AttributeGroup.
     * Each object in this list is an AttributeUse
     */

    private final List<AttributeUse> attributes;

    /**
     * The collection of AttributeGroup References for this
     * AttributeGroup. Each object in this list is an AttributeGroupReference
     */

    private final List<AttributeGroupReference> references;

    /**
     * The anyAttribute wildcard, if any
     */

    private AttributeWildcard anyAttribute = null;

    /**
     * The name this attribute group
     */

    private StructuredQName attributeGroupName;

    /**
     * A HashMap containing all the attributes in this attribute group,
     * expanded recursively. The key is the integer fingerprint of the
     * attribute name, the value is the attribute use. The map
     * is set up when first requested.
     */

    private IntHashMap<AttributeUse> declaredAttributes = null;

    /**
     * A map from the fingerprints of the attributes in this attribute group
     * to consecutive integer positions in a counter array; the actual counter
     * array is allocated at run-time during instance validation, but the mapping
     * of attributes to positions in this array is fixed at compile time
     */

    private IntToIntMap counterMap;

    /**
     * If this attribute group is a redefinition and does NOT contain a self-reference,
     * then during validation we must check that this is a valid restriction. This item holds
     * the attribute group being redefined
     */

    private AttributeGroupDecl redefinedAttributeGroup;

    /**
     * Creates a new AttributeGroup definition. For internal use only.
     *
     * @param config the EnterpriseConfiguration that this AttributeGroup
     *               belongs to.
     */
    public AttributeGroupDecl(EnterpriseConfiguration config) {
        setConfiguration(config);
        attributes = new ArrayList<>(10);
        references = new ArrayList<>(10);
    }

    /**
     * Get the name of this attribute group, as a qualified name
     * @return the expanded name of this attribute group
     */

    public StructuredQName getAttributeGroupName() {
        return attributeGroupName;
    }

    /**
     * Returns the local part of the name of this named AttributeGroup.
     *
     * @return the local name of this named AttributeGroup.
     */

    public String getName() {
        return attributeGroupName.getLocalPart();
    }

    /**
     * Returns the namespace URI of the name of this named AttributeGroup.
     *
     * @return the namespace URI of this named AttributeGroup: null if the attribute group is in no namespace
     */

    public NamespaceUri getTargetNamespace() {
        return attributeGroupName.getNamespaceUri();
    }


    /**
     * Add the given attribute use to this AttributeGroup. Doesn't
     * throw an error if it's a duplicate: this is left until validation time.
     * For internal use only.
     *
     * @param use the AttributeUse to add
     */

    public void addAttributeUse(AttributeUse use) {
        if (use == null) {
            return;
        }
        attributes.add(use);
    }

    /**
     * Adds the given AttributeGroupReference to this AttributeGroup.
     * For internal use only.
     *
     * @param attrGroup the AttributeGroupReference to add
     */
    public void addAttributeGroupReference(AttributeGroupReference attrGroup) {
        if (attrGroup == null) {
            return;
        }
        references.add(attrGroup);
    }

    /**
     * Set the attribute group that this attribute group redefines. This is called only for
     * an attribute group that redefined another one, *without* a self-reference
     *
     * @param other the attribute group declaration that this attribute group redefines
     */

    public void setRedefinedAttributeGroupDecl(AttributeGroupDecl other) {
        redefinedAttributeGroup = other;
    }

    /**
     * Returns the attributes defined locally in this attribute group.
     * (not those of the nested groups)
     *
     * @return the attribute uses defined locally in this attribute group.
     */
    public List<AttributeUse> getLocalAttributes() {
        return attributes;
    }

    /**
     * Returns the AttributeGroup references contained locally in this attribute group.
     * (not those of the nested groups)
     *
     * @return the locally-defined attribute groups.
     */
    public List<AttributeGroupReference> getLocalAttributeGroupReferences() {
        return references;
    }

    /**
     * Check for cycles in the attribute group.
     *
     * @param references The attribute groups that contain direct or indirect references
     *                   to this attribute group
     * @param compiler   used to report errors and warnings
     * @throws SchemaException if a circularity is detected.
     */

    @Override
    public void lookForCycles(Stack<SchemaComponent> references, SchemaCompiler compiler) throws SchemaException {
        if (compiler.getLanguageVersion() == Configuration.XSD11) {
            // circular attribute groups are not an error in XSD 1.1
            return;
        }
        if (references.contains(this)) {
            throw new SchemaException("The definition of attribute group " +
                    Err.wrap(getAttributeGroupName().getDisplayName()) +
                    " is circular", this);
        }
        references.push(this);
        for (AttributeGroupReference group : getLocalAttributeGroupReferences()) {
            group.lookForCycles(references, compiler);
        }
        references.pop();
    }


    /**
     * Get the local attribute wildcard used in this attribute group
     *
     * @return the wildcard used in this complexType if there is one; otherwise
     *         return null.
     */

    public AttributeWildcard getLocalAttributeWildcard() {
        return anyAttribute;
    }

    /**
     * Get the complete attribute wildcard used in this attribute group.
     * This takes into account any attribute wildcards used in contained attribute
     * groups. The rules are given in Schema Part 1 section 3.4.2. The returned wildcard
     * is the intersection of the attribute wildcards in the various attribute groups: that is,
     * a wildcard that allows only what each one of these wildcards allows.
     *
     * @return the combined wildcard used in this attribute group. This can be null. It can also
     *         be "inexpressible". This results in an AttributeWildcard marked as being inexpressible,
     *         an error condition that the caller is left to deal with.
     * @throws MissingComponentException
     *          if a required component contains a reference
     *          to another component that is not present in the schema
     */
    public AttributeWildcard getAttributeWildcard() {
        return getAttributeWildcard(new Stack<>());
    }

    AttributeWildcard getAttributeWildcard(Stack<AttributeGroupDecl> stack) {
        AttributeWildcard awc = getLocalAttributeWildcard();
        Wildcard wat = awc == null ? null : awc.getWildcard();
        if (stack.contains(this)) {
            return null;
        }
        stack.push(this);
        for (AttributeGroupReference ref : getLocalAttributeGroupReferences()) {
            AttributeWildcard gawc = ref.getAttributeWildcard(stack);
            Wildcard gat = gawc == null ? null : gawc.getWildcard();
            if (gat != null) {
                if (wat == null) {
                    wat = gat;
                } else {
                    wat = Wildcard.makeIntersection(wat, gat, getConfiguration().getNamePool());
                }
            }
        }
        stack.pop();
        return wat == null ? null : new AttributeWildcard(wat);
    }

    /**
     * Returns a list of all the attribute uses of this
     * attribute group. The list includes attributes from
     * all attribute groups references contained in this AttributeGroup.
     *
     * @return all the attributes of this
     *         attribute group, expanded recursively. The list does
     *         not include attribute wildcards.
     */

    public List<AttributeUse> getAttributeUses() throws MissingComponentException {
        List<AttributeUse> list = new ArrayList<>(20);
        gatherAttributeUses(list);
        return list;
    }

    /**
     * Append all attribute declarations contained directly or indirectly in this
     * attribute group to a supplied List. The AttributeGroupDecl must have been
     * validated before this method is called.
     *
     * @param list the list to which the contained attribute declarations should be appended
     */

    private void gatherAttributeUses(List<AttributeUse> list) throws MissingComponentException {
        gatherAttributeUses(list, new Stack<>());
    }

    private void gatherAttributeUses(List<AttributeUse> list, Stack<AttributeGroupDecl> stack) throws MissingComponentException {
        list.addAll(attributes);
        for (AttributeGroupReference reference : references) {
            AttributeGroupDecl decl = (AttributeGroupDecl) reference.getTarget();
            stack.push(this);
            if (!stack.contains(decl)) {
                decl.gatherAttributeUses(list, stack);
            }
            stack.pop();
        }
    }

    /**
     * Get the counter map used to map attribute names to positions in an array of
     * counters used during instance validation
     *
     * @return the counter map. This is available at any time after validate() is called.
     *         The counter map maps the fingerprints of the attributes defined in this attribute group
     *         to a sequence of consecutive integers that can be used to index into an array of counters.
     */

    public IntToIntMap getCounterMap() {
        return counterMap;
    }

    /**
     * Return the name of this AttributeGroup, for display purposes
     *
     * @return the name of this AttributeGroup, or null, if
     *         no name was defined.
     */

    public String getDisplayName() {
        return attributeGroupName.getDisplayName();
    }

    /**
     * Sets the wildcard (anyAttribute) of this attribute Group. It is the caller's responsibility
     * to ensure that only one attribute wildcard is defined in an attribute group.
     *
     * @param wildcard the wildcard defining the xs:anyAttribute content of this attribute group,
     *                 if any.
     */

    public void setAnyAttribute(AttributeWildcard wildcard) {
        anyAttribute = wildcard;
    }

    /**
     * Check references from this component to other components
     *
     * @param compiler used for reporting errors and warnings
     * @return true if all is well, false if errors found
     */

    @Override
    public boolean fixup(SchemaCompiler compiler) throws SchemaException {
        if (getFixupStatus() == UNVALIDATED) {
            setFixupStatus(VALIDATING);
            for (AttributeUse use : attributes) {
                AttributeDecl decl = (AttributeDecl) PreparedSchema.validateReference(use, compiler, false);
                if (decl == null) {
                    setFixupStatus(INVALID);
                    continue;
                }
                boolean b = decl.fixup(compiler);
                if (!b) {
                    setFixupStatus(INVALID);
                    //continue;
                }
            }
            for (AttributeGroupReference ref : references) {
                AttributeGroupDecl decl = (AttributeGroupDecl) PreparedSchema.validateReference(ref, compiler, false);
                if (decl == null) {
                    ref.setFixupStatus(INVALID);
                    setFixupStatus(INVALID);
                    continue;
                }
                boolean b = decl.fixup(compiler);
                if (!b) {
                    setFixupStatus(INVALID);
                    //continue;
                }
            }
            if (getFixupStatus() == INVALID) {
                return false;
            }
            lookForCycles(new Stack<SchemaComponent>(), compiler);
        }
        setFixupStatus(VALIDATED);
        return true;
    }


    /**
     * Check the validity of this AttributeGroup declaration. For internal use only.
     * Any errors found are reported to the error listener.
     *
     * @param compiler used for reporting errors and warnings
     * @return true if the declaration is valid, false if not
     * @throws SchemaException if this Attribute declaration
     *                         is invalid and the error listener chooses to treat the error
     *                         as fatal.
     */

    @Override
    public boolean validate(SchemaCompiler compiler) throws SchemaException {

        boolean result = true;

        if (getFixupStatus() == UNVALIDATED) {
            result = fixup(compiler);
        }

        if (getFixupStatus() == INVALID) {
            setValidationStatus(INVALID);
            return false;
        }

        // check for circularities and for previous validation
        switch (getValidationStatus()) {
            case VALIDATED:
            case INVALID:
                return true;
            case VALIDATING:
                compiler.error("The definition of attribute group " +
                        Err.wrap(getAttributeGroupName().getDisplayName()) +
                        " is circular", this);
                return false;
            case UNVALIDATED:
                break;
        }
        setValidationStatus(VALIDATING);


        for (AttributeUse att : attributes) {
            result &= att.validate(compiler);
        }

        for (AttributeGroupReference ref : references) {
            result &= ref.validate(compiler);
        }

        if (!result) {
            setValidationStatus(INVALID);
            return false;
        }

        AttributeWildcard wild = getAttributeWildcard();
        if (wild != null) {
            result = wild.validate(compiler);
        }

        // Check that there are no duplicated attributes, and allocate the counter map
        if (result) {
            String foundID = null;
            List<AttributeUse> attributeList = new ArrayList<>();
            gatherAttributeUses(attributeList);
            Set<StructuredQName> fingerprintSet = new HashSet<>(attributeList.size() * 2);
            final TypeHierarchy th = compiler.getConfiguration().getTypeHierarchy();
            for (AttributeUse att : attributeList) {
                result &= att.validate(compiler);
                if (result) {
                    AttributeDecl decl = (AttributeDecl) att.getTarget();
                    SimpleType stype = decl.getSimpleType();
                    if (stype.isAtomicType() && !(stype.getFingerprint() == StandardNames.XS_ANY_SIMPLE_TYPE) &&
                            th.isSubType((AtomicType) stype, BuiltInAtomicType.ID)) {
                        if (foundID != null && compiler.getLanguageVersion() == Configuration.XSD10) {
                            compiler.error("Attribute group contains more than one ID attribute (" +
                                    foundID + ", " + att.getDisplayName() + ')', this);
                            result = false;
                        }
                        foundID = att.getDisplayName();
                        // The spec (3.6.6 para 3) says "derived from ID", we take it to mean "derived by restriction"
                    }
                    StructuredQName key = att.getTargetComponentName();
                    if (fingerprintSet.contains(key)) {
                        compiler.error("Attribute " + Err.wrap(key.getDisplayName(), Err.ATTRIBUTE) +
                                " appears more than once in attribute group", this);
                        result = false;
                    }
                    fingerprintSet.add(key);
//                    counterMap.put(key, counterIndex++);
                }
            }
            buildCounterMap();
        }

        // If the attribute group is a redefinition, and does not contain a self reference,
        // then it must be a valid restriction of the old definition.
        if (redefinedAttributeGroup != null) {
            result &= isValidRestriction(redefinedAttributeGroup, compiler);
        }

        // Force computation of the set of declared attributes (expand the referenced attribute groups recursively)
        getDeclaredAttributes();

        setValidationStatus(VALIDATED);
        return result;

    }

    /**
     * Build the counter map
     */

    public void buildCounterMap() throws MissingComponentException {
        List<AttributeUse> attributeList = new ArrayList<>();
        gatherAttributeUses(attributeList);
        int size = attributeList.size();
        if (size < 8) {
            counterMap = new IntToIntArrayMap(size + 2);
        } else {
            counterMap = new IntToIntHashMap(size + 2);
        }

        int counterIndex = 0;
        for (AttributeUse att : attributeList) {
            int key = att.getAttributeDeclaration().getFingerprint();
            counterMap.put(key, counterIndex++);
        }
    }

    /**
     * Test whether this attribute group is a valid restriction of another attribute group.
     * If it is not a valid restriction, an error is reported to the error listener and the
     * method returns false.
     *
     * @param base     the other attribute group
     * @param compiler user for reporting errors and warnings
     * @return true if this is a valid restriction, false if not
     * @throws SchemaException if an error is detected and the error listener
     *                         decides to treat the error as fatal.
     */

    public boolean isValidRestriction(AttributeGroupDecl base, SchemaCompiler compiler)
            throws SchemaException {
        boolean result = true;
        for (AttributeUse att : getAttributeUses()) {
            if (att.isProhibited()) {
                // prohibited attribute uses are treated as if they didn't exist
                continue;
            }
            StructuredQName targetName = att.getTargetComponentName();
            //int fp = targetName.getFingerprint();
            AttributeUse baseAtt = base.getAttributeUse(targetName);
            if (baseAtt == null || baseAtt.isProhibited()) {
                AttributeWildcard baseWildCard = base.getAttributeWildcard();
                if (baseWildCard == null) {
                    compiler.error("Invalid restriction: attribute " + Err.wrap(att.getDisplayName(), Err.ATTRIBUTE) +
                            " is " + (baseAtt == null ? "not defined" : "prohibited") + " in the base type", this);
                    result = false;
                } else {
                    if (!baseWildCard.getWildcard().matches(att.getTargetComponentName(), false, compiler.getPreparedSchema(), null)) {
                        compiler.error("Invalid restriction: attribute " + Err.wrap(att.getDisplayName(), Err.ATTRIBUTE) +
                                " is " + (baseAtt == null ? "not defined" : "prohibited") + " in the base type", this);
                        result = false;
                    }
                }
            } else {
                if (baseAtt.isRequired() && !att.isRequired()) {
                    compiler.error("Invalid restriction: attribute " + Err.wrap(att.getDisplayName(), Err.ATTRIBUTE) +
                            " is optional but the base attribute is required", this);
                    result = false;
                }
                if (baseAtt.isInheritable() != att.isInheritable()) {
                    if (att.isInheritable()) {
                        compiler.error("Invalid restriction: attribute " + Err.wrap(att.getDisplayName(), Err.ATTRIBUTE) +
                                " is inheritable, but the corresponding attribute in the base type is not", this);
                    } else {
                        compiler.error("Invalid restriction: attribute " + Err.wrap(att.getDisplayName(), Err.ATTRIBUTE) +
                                " is not inheritable, but the corresponding attribute in the base type is", this);
                    }
                    result = false;
                }
                AttributeDecl decl = (AttributeDecl) att.getTarget();
                AttributeDecl baseDecl = (AttributeDecl) baseAtt.getTarget();
                String reason = UserSimpleType.isTypeDerivationOK(decl.getSimpleType(), baseDecl.getSimpleType(), 0);
                if (reason != null) {
                    compiler.error("Invalid restriction: attribute " + Err.wrap(att.getDisplayName(), Err.ATTRIBUTE) +
                            " is inconsistent with the base type. " + reason, this);
                    result = false;
                }
                AtomicSequence baseFixed = baseAtt.getFixedValue();
                if (baseFixed == null) {
                    baseFixed = baseDecl.getFixedValue();
                }
                if (baseFixed != null) {
                    if (att.getUnderlyingDefaultValue() != null) {
                        compiler.error("Invalid restriction: attribute " + Err.wrap(att.getDisplayName(), Err.ATTRIBUTE) +
                                " has a default value but the base attribute is fixed", this);
                        result = false;
                    }
                    AtomicSequence attFixed = att.getUnderlyingFixedValue();
                    if (attFixed != null &&
                            !SimpleTypeComparison.getInstance().equal(attFixed, baseFixed)) {
                        compiler.error("Invalid restriction: attribute " + Err.wrap(att.getDisplayName(), Err.ATTRIBUTE) +
                                " has a fixed value different from that of the base attribute", this);
                        result = false;
                    } else if (attFixed == null) {
                        compiler.error("Invalid restriction: attribute " + Err.wrap(att.getDisplayName(), Err.ATTRIBUTE) +
                                " must have the same fixed value as the base attribute", this);
                        result = false;
                    }
                }
            }

        }
        for (AttributeUse baseAtt : base.getAttributeUses()) {
            if (baseAtt.isRequired()) {
                AttributeUse thisAtt = getAttributeUse(baseAtt.getTargetComponentName());
                if (thisAtt == null) {
                    compiler.error("Invalid restriction: base type has a required attribute " +
                            Err.wrap(baseAtt.getDisplayName(), Err.ATTRIBUTE), this);
                    result = false;
                }
            }
        }
        AttributeWildcard wild = getAttributeWildcard();
        if (wild != null) {
            AttributeWildcard baseWild = base.getAttributeWildcard();
            if (baseWild == null) {
                compiler.error("Invalid restriction: this type has an <xs:anyAttribute> " +
                        "but the base type does not", this);
                return false;
            }
            if (!wild.getWildcard().isSubset(baseWild.getWildcard(), compiler.getNamePool())) {
                compiler.error("Invalid restriction: this type has an <anyAttribute> wildcard which allows" +
                        " attribute names that are not permitted by the <anyAttribute> on the base type", this);
                return false;
            }
            if (baseWild.getWildcard().compareStrength(wild.getWildcard()) > 0) {
                compiler.error("Invalid <anyAttribute> restriction: " +
                        "this type has a weaker processContents than the base type", this);
                return false;
            }
        }
        return result;
    }

    /**
     * Test whether this is the same attribute group as another. They are considered to be the same component
     * if they are derived from the same type definition in the original XML representation (which
     * can happen when there are multiple includes of the same file)
     *
     * @param other the attribute group declaration that we are comparing with
     * @return true if the two attribute group declarations are identical
     */

    public boolean isSameDeclaration(AttributeGroupDecl other) {
        return this == other ||
                (getAttributeGroupName().equals(other.getAttributeGroupName()) && hasSameLocation(other));

    }

    /**
     * Set the name of this attribute group. For internal use only.
     *
     * @param name the name of this attribute group
     */

    public void setAttributeGroupName(StructuredQName name) {
        this.attributeGroupName = name;
    }

    /**
     * Get a HashMap containing all the attributes in this attribute group,
     * expanded recursively. The key is the integer fingerprint identifying
     * the name of the attribute. The value is an AttributeUse object identifying
     * the attribute declaration.
     *
     * @return a map defining the attributes in the attribute group
     */

    public IntHashMap<AttributeUse> getDeclaredAttributes() throws MissingComponentException {
        if (declaredAttributes == null) {
            IntHashMap<AttributeUse> map = new IntHashMap<>(10);
            for (AttributeUse att : getAttributeUses()) {
                map.put(att.getAttributeDeclaration().getFingerprint(), att);
            }
            declaredAttributes = map;
        }
        return declaredAttributes;
    }

    /**
     * Get the attribute use with a specific name, if there
     * is one. Otherwise return null.
     *
     * @param attributeName the expanded name of the required attribute in the name pool.
     * @return the attribute with a given name, from among the transitively-expanded
     *         definition of this attribute group.
     */

    /*@Nullable*/
    public final AttributeUse getAttributeUse(StructuredQName attributeName) throws MissingComponentException {
        for (AttributeUse att : getAttributeUses()) {
            if (att.getTargetComponentName().equals(attributeName)) {
                return att;
            }
        }
        return null;
    }

    /**
     * Serialize the schema component
     */

    @Override
    public void serialize(SchemaModelSerializer serializer) throws XPathException {
        String id = serializer.getId(this, true);
        serializer.startElement("attributeGroup");
        serializer.emitAttribute("id", id);
        serializer.emitAttribute("name", getName());
        if (getTargetNamespace() != null && !getTargetNamespace().isEmpty()) {
            serializer.emitAttribute("targetNamespace", getTargetNamespace().toString());
        }
        serializeContents(serializer);
        serializer.endElement();
    }

    /**
     * Serialize the list of attribute users and wildcard
     *
     * @param serializer the object to which the information is to be sent
     * @throws net.sf.saxon.trans.XPathException
     *          if a failure occurs
     */

    public void serializeContents(SchemaModelSerializer serializer) throws XPathException {
        for (AttributeUse att : getAttributeUses()) {
            att.serialize(serializer);
        }
        if (getAttributeWildcard(null) != null) {
            getAttributeWildcard(null).serialize(serializer);
        }
    }

    /**
     * Elaborate the schema component: after reloading a serialized schema component model,
     * this expands the component with derived information needed during validation episodes.
     * The model is assumed to be valid.
     *
     * @param compiler the schema compiler
     */

    @Override
    public void elaborate(SchemaCompiler compiler) throws SchemaException {
        buildCounterMap();
    }

    @Override
    public FunctionItem getComponentAsFunction() {
        AttributeGroupDecl thisDecl = this;
        CallableDelegate callable = new CallableDelegate((context, arguments) -> {
                String key = arguments[0].head().getStringValue();
                switch (key) {
                    case "class":
                        return StringValue.bmp("Attribute Group Definition");
                    case "implementation":
                        return new ObjectValue<>(thisDecl);
                    case "name":
                        return new StringValue(getName(), BuiltInAtomicType.NCNAME);
                    case "target namespace":
                        return new AnyURIValue(getTargetNamespace().toUnicodeString());
                    case "attribute uses":
                        List<Item> uses = new ArrayList<>();
                        for (AttributeUse use : getAttributeUses()) {
                            uses.add(use.getComponentAsFunction());
                        }
                        return SequenceExtent.makeSequenceExtent(uses);
                    case "attribute wildcard":
                        if (getLocalAttributeWildcard() != null) {
                            return getLocalAttributeWildcard().getComponentAsFunction();
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

// Copyright (c) 2004-2023 Saxonica Limited