package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.fsa.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Operand;
import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.expr.instruct.Block;
import net.sf.saxon.expr.instruct.FixedAttribute;
import net.sf.saxon.expr.instruct.FixedElement;
import net.sf.saxon.lib.Logger;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.z.IntHashSet;
import net.sf.saxon.z.IntIterator;

import java.util.*;

import static net.sf.saxon.type.SchemaValidationStatus.INVALID;

/**
 * This class contains information obtained by compiling a {@link UserComplexType} in the context
 * of a particular schema. The same {@link UserComplexType} might have different compiled
 * representations in different schemas, because the compiled form takes into account
 * information such as substitution group membership that can vary from one schema
 * to another
 */

public class CompiledComplexType {

    private final UserComplexType type;
    private final PreparedSchema schema;

    private final static boolean debug = false;


    /**
     * The finite state machine for validating against this complex type
     */

    private FiniteStateMachine machine;

    /**
     * Flag to indicate that the validation machinery for the type has been built
     */

    private boolean built = false;

    /**
     * Table of context-determined types for element children
     */

    private Map<StructuredQName, SchemaType> contextDeterminedTypesForElements;

    /**
     * Table of context-determined types for attributes
     */

    private Map<StructuredQName, SimpleType> contextDeterminedTypesForAttributes;

    public CompiledComplexType(PreparedSchema schema, UserComplexType type) {
        this.schema = schema;
        this.type = type;
        machine = new FiniteStateMachine(type, false);
    }

    public CompiledComplexType(PreparedSchema schema, UserComplexType type, FiniteStateMachine fsm) {
        this.schema = schema;
        this.type = type;
        this.machine = fsm;
    }

    public EnterpriseConfiguration getConfiguration() {
        return schema.getConfiguration();
    }

    public UserComplexType getType() {
        return type;
    }

    public PreparedSchema getSchema() {
        return schema;
    }

    /**
     * Find an element particle within this complex type definition having a given element name
     * (identified by fingerprint), and return the schema type associated with that element particle.
     * If there is no such particle, return null. If the fingerprint matches an element wildcard,
     * return the type of the global element declaration with the given name if one exists, or AnyType
     * if none exists and lax validation is permitted by the wildcard.
     *
     * @param elementName        Identifies the name of the child element within this content model
     * @param schema
     * @param considerExtensions if true, and if the element particle is not defined within this type itself,
     *                           try to find it within the types derived by extension from this type, and/or within the open content
     *                           model permitted by the type. If there is one extension that defines the particle, return
     */

    //@Override
    public SchemaType getElementParticleType(int elementName, Schema schema, boolean considerExtensions) throws MissingComponentException {
        Particle p = type.getParticle();
        SchemaType child = p == null ? null : p.getElementParticleType(elementName, schema);
        if (child != null) {
            return child;
        } else if (considerExtensions) {
            Wildcard ocw = type.getOpenContentWildcard();
            if (ocw != null && ocw.matches(elementName, true, schema, type)) {
                return AnyType.getInstance();
            }
            for (UserComplexType ext : ((PreparedSchema)schema).getExtensionsOfType(type)) {
                // No need to consider further extensions, as iterateExtensionTypes() returns the transitive set
                SchemaType st = ext.getElementParticleType(elementName, schema, false);
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
     * @param schema
     * @param considerExtensions if true, types derived by extension from this type should be taken into account
     * @return the cardinality of the requested element within this complex type
     */

    //@Override
    public int getElementParticleCardinality(int elementName, Schema schema, boolean considerExtensions) throws MissingComponentException {
        int baseCardinality;
        Particle p = type.getParticle();
        if (p == null) {
            baseCardinality = StaticProperty.EMPTY;
        } else {
            baseCardinality = p.getElementParticleCardinality(elementName, schema);
        }
        // See if it matches open content
        Wildcard ocw = type.getOpenContentWildcard();
        if (ocw != null && ocw.matches(elementName, true, schema, type)) {
            return StaticProperty.ALLOWS_ZERO_OR_MORE;
        }
        // Now we need to look in all complex types derived from this type by extension. These might allow
        // (additional) occurrences of the sought element particle.
        if (considerExtensions) {
            for (UserComplexType ext : ((PreparedSchema)schema).getExtensionsOfType(type)) {
                if (baseCardinality == StaticProperty.ALLOWS_ZERO_OR_MORE) {
                    // quick exit
                    return baseCardinality;
                }
                // No need to consider extensions recursively, since iterateExtensionTypes() is already transitive
                int extCardinality = ext.getElementParticleCardinality(elementName, schema, false);
                baseCardinality = Cardinality.union(baseCardinality, extCardinality);
            }
        }
        return baseCardinality;
    }

    /**
     * Get a list of all the names of elements that can appear as children of an element having this
     * complex type, as integer fingerprints. If the list is unbounded (because of wildcards or the use
     * of xs:anyType), return null. The result includes elements defined in types that are defined by
     * extension from this type.
     *
     * @param schema
     * @param children        a set, initially empty, which on return will hold the names of all permitted
     *                        child elements; if the result contains the value XS_INVALID_NAME, this indicates that it is not possible to enumerate
     *                        all the children, typically because of wildcards. In this case the other contents of the set should
     *                        be ignored
     * @param ignoreWildcards if set to true, wildcards are ignored; only the elements that match explicit
     */

    //@Override
    public void gatherAllPermittedChildren(Schema schema, IntHashSet children, boolean ignoreWildcards) throws SchemaException {
        Particle p = type.getParticle();
        if (p == null) {
            return;
        }
        p.gatherAllPermittedElements(schema, children, ignoreWildcards);
        for (UserComplexType uct : ((PreparedSchema)schema).getExtensionsOfType(type)) {
            uct.gatherAllPermittedChildren(schema, children, ignoreWildcards);
        }
    }

    /**
     * Get a list of all the names of elements that can appear as descendants of an element having this
     * complex type, as integer fingerprints. If the list is unbounded (because of wildcards or the use
     * of xs:anyType), return null. The result includes elements defined in types that are defined by
     * extension from this type or other types processed
     *
     * @param schema
     * @param descendants an integer set, initially empty, which on return will hold the fingerprints of all permitted
     *                    descendant elements; if the result contains the value -1, this indicates that it is not possible to enumerate
     *                    all the descendants, typically because of wildcards. In this case the other contents of the set should
     *                    be ignored.
     */

    //@Override
    public void gatherAllPermittedDescendants(Schema schema, IntHashSet descendants) throws SchemaException {
        Particle p = type.getParticle();
        if (p == null) {
            return;
        }
        IntHashSet permittedChildren = new IntHashSet(16);
        gatherAllPermittedChildren(schema, permittedChildren, false);

        StructuredQName wildcardDummy = StandardNames.SQ_XS_INVALID_NAME;
        if (permittedChildren.contains(-1)) {
            descendants.add(-1);
            return;
        }
        IntIterator kids = permittedChildren.iterator();
        while (kids.hasNext()) {
            int c = kids.next();
            if (descendants.add(c)) {
                SchemaType childType = getElementParticleType(c, schema, true);
                if (childType == null) {
                    throw new AssertionError("Child particle " + c + " not found");
                }
                if (childType instanceof ComplexType) {
                    if (childType instanceof UserComplexType) {
                        ((UserComplexType) childType).gatherAllPermittedDescendants(schema, descendants);
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
     * @param schema
     * @param elementName the name of the required descendant element
     * @return the type of the descendant element; null if the element cannot appear as a descendant;
     * anyType if it can appear with several different types
     */

    //@Override
    public SchemaType getDescendantElementType(Schema schema, int elementName) throws SchemaException {
        Set<SchemaType> parentsConsidered = new HashSet<>();
        Set<SchemaType> foundTypes = new HashSet<>();
        getDescendantElementTypes(schema, elementName, parentsConsidered, foundTypes);
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

    private void getDescendantElementTypes(Schema schema,
                                           int elementName,
                                           Set<SchemaType> parentsConsidered,
                                           Set<SchemaType> foundTypes) throws SchemaException {
        Particle p = type.getParticle();
        if (p == null) {
            return;
        }

        // See if the requested element can appear as a child
        SchemaType targetType = getElementParticleType(elementName, schema, true);
        if (targetType != null) {
            foundTypes.add(targetType);
            if (targetType == AnyType.getInstance()) {
                return;
            }
        }

        IntHashSet permittedChildren = new IntHashSet(16);
        gatherAllPermittedChildren(schema, permittedChildren, false);

        if (permittedChildren.contains(-1)) {
            foundTypes.add(AnyType.getInstance());
            return;
        }
        IntIterator kids = permittedChildren.iterator();
        while (kids.hasNext()) {
            int c = kids.next();
            SchemaType childType = getElementParticleType(c, schema, true);
            if (childType == null) {
                throw new AssertionError("Child particle " + c + " not found");
            }
            if (!parentsConsidered.contains(childType)) {
                parentsConsidered.add(childType);
                if (childType instanceof ComplexType) {
                    if (childType instanceof UserComplexType) {
                        CompiledComplexType compiledChildType = getSchema().getCompiledComplexType((UserComplexType) childType);
                        compiledChildType.getDescendantElementTypes(schema, elementName, parentsConsidered, foundTypes);
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
     * @param schema
     * @param elementFingerprint the name of the required descendant element
     * @return the cardinality of the descendant element within this complex type
     */

    //@Override
    public int getDescendantElementCardinality(Schema schema, int elementFingerprint) throws SchemaException {
        int cardinality = StaticProperty.EMPTY;
        IntHashSet children = new IntHashSet();
        gatherAllPermittedChildren(schema, children, false);
        if (children.contains(-1)) {
            return StaticProperty.ALLOWS_ZERO_OR_MORE;
        }
        IntIterator kids = children.iterator();
        while (kids.hasNext()) {
            int child = kids.next();
            int childCard = getElementParticleCardinality(child, schema, true);
            if (child == elementFingerprint) {
                cardinality = Cardinality.sum(cardinality, childCard);
                if (Cardinality.allowsMany(cardinality)) {
                    // take an early bath
                    return cardinality;
                }
            }
            SchemaType childType = getElementParticleType(child, schema, true);
            if (childType != null && childType.isComplexType()) {
                IntHashSet childDescendants = new IntHashSet();
                ((ComplexType) childType).gatherAllPermittedDescendants(schema, childDescendants);
                if (childDescendants.contains(-1)) {
                    // The content model includes a wildcard
                    return StaticProperty.ALLOWS_ZERO_OR_MORE;
                }
                if (childDescendants.contains(elementFingerprint)) {
                    cardinality = Cardinality.sum(cardinality,
                                                  Cardinality.multiply(childCard, ((ComplexType) childType).getDescendantElementCardinality(schema, elementFingerprint)));
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
     * Find an attribute use within this complex type definition having a given attribute name
     * (identified by fingerprint), and return the schema type associated with that attribute.
     * If there is no such attribute use, return null. If the fingerprint matches an attribute wildcard,
     * return the type of the global attribute declaration with the given name if one exists, or AnySimpleType
     * if none exists and lax validation is permitted by the wildcard.
     *
     * @param attributeName Identifies the name of the required attribute use within this content model
     * @param schema
     */

    //@Override
    public SimpleType getAttributeUseType(StructuredQName attributeName, Schema schema) throws SchemaException {
        AttributeGroupDecl ag = type.getCombinedAttributeGroup();
        AttributeUse use = ag.getAttributeUse(attributeName);
        if (use != null && !use.isProhibited()) {
            return ((AttributeDecl) use.getTarget()).getType();
        }
        SimpleType attType = null;
        for (UserComplexType ext : ((PreparedSchema) schema).getExtensionsOfType(type)) {
            SimpleType st = ext.getAttributeUseType(attributeName, schema);
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
        if (wild.matches(attributeName, false, (PreparedSchema) schema, null)) {
            switch (wild.getProcessContents()) {
                case "skip":
                    return AnySimpleType.getInstance();
                case "strict": {
                    AttributeDecl decl = (AttributeDecl) schema.getAttributeDecl(attributeName);
                    if (decl != null) {
                        return decl.getType();
                    } else {
                        return null;
                    }
                }
                default: { // processContents() = lax
                    AttributeDecl decl = (AttributeDecl) schema.getAttributeDecl(attributeName);
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
     * @param schema
     * @return the cardinality associated with the attribute use identified by the fingerprint.
     * If the attribute is optional but either the attribute use or the corresponding attribute
     * declaration defines a default or fixed value, then it is treated as optional (zero-or-one),
     * because we don't know whether fixed/default attribute values are being reported to the
     * application or not: see bug 3072.
     */

    //@Override
    public int getAttributeUseCardinality(StructuredQName attributeName, Schema schema) throws SchemaException {
        AttributeGroupDecl ag = type.getCombinedAttributeGroup();
        AttributeUse use = ag.getAttributeUse(attributeName);
        if (use != null) {
            return use.getCardinality();
        }
        int card = 0;
        for (UserComplexType ext : ((PreparedSchema) schema).getExtensionsOfType(type)) {
            int c = ext.getAttributeUseCardinality(attributeName, schema);
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
        if (wild.matches(attributeName, false, (PreparedSchema) schema, null)) {
            return StaticProperty.ALLOWS_ZERO_OR_ONE;
        }
        return StaticProperty.EMPTY;
    }

    /**
     * Ask whether this type (or any known type derived from it by extension) allows the element
     * to have one or more attributes.
     *
     * @return true if attributes (other than the standard xsi: attributes) are allowed. The value
     * false indicates that only the standard attributes in the xsi namespace are permitted.
     */

    //@Override
    public boolean allowsAttributes() throws MissingComponentException {
        AttributeGroupDecl ag = type.getCombinedAttributeGroup();
        if (!ag.getAttributeUses().isEmpty()) {
            return true;
        }
        if (ag.getAttributeWildcard(null) != null) {
            return true;
        }
        for (UserComplexType ext : ((PreparedSchema) schema).getExtensionsOfType(type)) {
            if (ext.allowsAttributes(schema)) {
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

    //@Override
    public boolean containsElementWildcard() throws MissingComponentException {
        if (type.getParticle() != null && type.getParticle().containsElementWildcard()) {
            return true;
        }
        for (UserComplexType ext : ((PreparedSchema) schema).getExtensionsOfType(type)) {
            if (ext.containsElementWildcard(schema)) {
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

    public void makeContextDeterminedTypeMapForElements(PreparedSchema schema) throws SchemaException {
        if (contextDeterminedTypesForElements != null) {
            return;
        }
        contextDeterminedTypesForElements = new HashMap<>();
        gatherContextDeterminedTypesForElements(contextDeterminedTypesForElements, schema);
    }

    private void gatherContextDeterminedTypesForElements(Map<StructuredQName, SchemaType> map, PreparedSchema schema) throws SchemaException {
        // Gather from the base type first, so that the subtype wins if the type is redefined
        SchemaType base = type.getBaseType();
        if (base instanceof UserComplexType) {
            CompiledComplexType compiledBase = schema.getCompiledComplexType((UserComplexType) base);
            compiledBase.gatherContextDeterminedTypesForElements(map, schema);
        }
        Particle p = type.getParticle();
        if (p != null) {
            // This is a bit inefficient, but we only do it once.
            IntHashSet children = new IntHashSet();
            p.gatherAllPermittedElements(schema, children, true);
            IntIterator kids = children.iterator();
            while (kids.hasNext()) {
                int fp = kids.next();
                SchemaType type = getElementParticleType(fp, schema, false);
                map.put(getConfiguration().getNamePool().getStructuredQName(fp), type);
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
     * @throws net.sf.saxon.type.SchemaException if an error is encountered
     */

    public void makeContextDeterminedTypeMapForAttributes(PreparedSchema schema) throws SchemaException {
        if (contextDeterminedTypesForAttributes != null) {
            return;
        }
        contextDeterminedTypesForAttributes = new HashMap<>();
        gatherContextDeterminedTypesForAttributes(contextDeterminedTypesForAttributes, schema);
    }

    private void gatherContextDeterminedTypesForAttributes(Map<StructuredQName, SimpleType> map, PreparedSchema schema) throws SchemaException {
        // Gather from the base type first, so that the subtype wins if the type is redefined
        SchemaType base = type.getBaseType();
        if (base instanceof UserComplexType) {
            CompiledComplexType compiledBase = schema.getCompiledComplexType((UserComplexType)base);
            compiledBase.gatherContextDeterminedTypesForAttributes(map, schema);
        }
        for (AttributeUse use : type.getAttributeGroup().getAttributeUses()) {
            if (!use.isProhibited()) {
                StructuredQName fp = use.getTargetComponentName();
                SimpleType type = getAttributeUseType(fp, schema);
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
     * Analyze an XPath expression to see whether the expression is capable of delivering a value of this
     * type.
     *
     * @param expression the expression that delivers the content. If this is a Block, the sequence of
     *                   instructions within the block is checked by this method, and the individual instructions are
     *                   checked by a callback; in other cases the method calls expression.checkPermittedContents() which may pass
     *                   the analysis onto other expressions.
     * @param kind       the node kind whose content is being delivered: {@link Type#ELEMENT},
     *                   {@link Type#ATTRIBUTE}, or {@link Type#DOCUMENT}
     * @param schema
     * @throws net.sf.saxon.trans.XPathException if the expression will never deliver a value of the correct type (that is, of this type)
     */

    //@Override
    public void analyzeContentExpression(Expression expression, int kind, Schema schema) throws XPathException {
        Configuration config = getConfiguration();
        TypeHierarchy th = config.getTypeHierarchy();

        // If the complex type has a mandatory attribute, check that the content expression includes
        // an expression capable of delivering an attribute with the right name
        for (AttributeUse use : type.getAttributeGroup().getAttributeUses()) {
            if (use.isRequired()) {
                if (th.relationship(expression.getItemType(), NodeKindTest.ATTRIBUTE) == Affinity.DISJOINT) {
                    throw new XPathException("Type " + type.getDescription() + " defines a mandatory attribute " +
                                                     use.getTargetComponentName().getDisplayName() +
                                                     ", but there is no instruction to create such an attribute")
                            .withLocation(expression.getLocation()).asStaticError();
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
                        XPathException err = new XPathException("Type " + type.getDescription() + " defines a mandatory attribute " +
                                                                        use.getTargetComponentName().getDisplayName() +
                                                                        ", but there is no instruction to create such an attribute")
                                .withLocation(expression.getLocation())
                                .asStaticError();
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
                o.getChildExpression().checkPermittedContents(type, false);
            }
            // if all the children that can deliver elements have known element names, check that the
            // sequence of element names is valid against the content model of the parentType.
            // Keep checking until we find something that's dynamic

            if (getFiniteStateMachine() != null && !type.isAllContent()) {
                State state = getInitialState();
                for (Operand o : children) {
                    Expression child = o.getChildExpression();
                    if (child instanceof FixedElement) {
                        NodeName elementName = ((FixedElement) child).getFixedElementName();
                        int fingerprint = ((FixedElement) child).getFixedElementName().getFingerprint();
                        final Edge edge = state.getTransition(fingerprint, type, (PreparedSchema) schema);
                        if (edge == null) {
                            FiniteStateMachine machine = getFiniteStateMachine();
                            Wildcard openContentWildcard = machine.getOpenContentWildcard();
                            if (openContentWildcard != null &&
                                    openContentWildcard.matches(elementName, true, schema, type)) {
                                if (machine.isOpenContentInterleaved()) {
                                    // interleaved open content: leave the state unchanged
                                    continue;
                                } else if (state.isFinalState()) {
                                    // suffix wildcard: from now on we can only match the open content wildcard
                                    state = SuffixState.getInstance();
                                    continue;
                                }
                            }
                            if (state.isFinalState() && !((PreparedSchema) schema).getExtensionsOfType(type).isEmpty()) {
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
                            throw new XPathException(message, "XPTY0004").withLocation(child.getLocation());
                        }
                        try {
                            state = edge.makeTransition(state);
                        } catch (ValidationException e) {
                            throw new XPathException(e.getMessage(), "XPTY0004")
                                    .withLocation(expression.getLocation());
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
                        throw new XPathException("The generated content is empty: the schema requires at least one child element")
                                .withErrorCode("XPTY0004")
                                .withLocation(expression.getLocation());
                    } else {
                        throw new XPathException("The generated content is incomplete: the schema requires further child elements")
                                .withErrorCode("XPTY0004")
                                .withLocation(children[len - 1].getChildExpression().getLocation());
                    }
                }
            }
        } else {
            expression.checkPermittedContents(type, true);
        }
    }


    //////////////////////////////////////////////////////////////////////////////////////
    // FINITE STATE MACHINE
    //////////////////////////////////////////////////////////////////////////////////////

    /**
     * Set the finite state machine. Used when restoring a serialized component model
     *
     * @param machine the FSM for this complex type
     */

    public void setFiniteStateMachine(FiniteStateMachine machine) {
        this.machine = machine;
    }

    /**
     * Compile the finite-state machine for validating instances against this type
     *
     * @param compiler used for error reporting
     * @throws SchemaException to indicate an invalid schema
     */

    public void buildValidationMachinery(SchemaCompiler compiler) throws SchemaException {
        // Only compile a type once
        if (built || type.getValidationStatus() == INVALID) {
            return;
        }

        // Ensure the base type is compiled first
        SchemaType base = type.getBaseType();
        if (base instanceof UserComplexType) {
            compiler.getPreparedSchema().compileComplexType(compiler, (UserComplexType) base);
        }

        //System.err.println("Compiling type " + getDescription() + " (" + this + ")");

        if (debug) {
            Logger err = compiler.getConfiguration().getLogger();
            err.info("==== Compiling complex type " + type.getDescription());
            err.info("  simple content: " + type.isSimpleContent());
            err.info("  complex content: " + type.isComplexContent());
            err.info("  all content: " + type.isAllContent());
            err.info("  empty content: " + type.isEmptyContent());
            err.info("  emptiable: " + type.isEmptiable());
            err.info("  mixed content: " + type.isMixedContent());
            err.info("  base type: " + type.getBaseType().getDescription());
        }

        built = true;

        if (type.isSimpleContent() || type.isEmptyContent()) {
            machine.getInitialState().setFinalState(true);
        } else if (type.isAllContent()) {
            type.makeExtendedParticle(compiler);
        } else /* if (particle==null  || !(particle instanceof AllCompositor ))*/ {
            Particle extended = type.makeExtendedParticle(compiler);
            if (extended == null) {
                machine.getInitialState().setFinalState(true);
                machine.setOpenContentWildcard(type.getOpenContentWildcard(),
                                               type.getOpenContentMode() == OpenContentVariety.INTERLEAVE);
            } else {
                //makeAutomaton(compiler);
                compiler.scheduleAutomaton(this);
                if (type.getParticle() != null) {
                    type.getParticle().compile(compiler);
                }
            }
        }
        type.makeCombinedAttributeGroup(compiler);
        PreparedSchema schema = compiler.getPreparedSchema();
        makeContextDeterminedTypeMapForAttributes(schema);
        makeContextDeterminedTypeMapForElements(schema);
    }

    protected void makeAutomaton(SchemaCompiler compiler) throws SchemaException {
        //extended.rewrite(null);
        Particle extended = type.getParticle();
        NonDeterminizedState finalState = new NonDeterminizedState(machine);
        finalState.setFinalState(true);
        try {
            //                long startTime = System.currentTimeMillis();
            AutomatonState state = FiniteStateMachine.compileParticle(compiler, extended, finalState, type, machine);
            machine.setInitialState(state);
            //                System.err.println("Time to compile complex type " + getDescription() +
            //                        " - " + (System.currentTimeMillis() - startTime));
            if (debug) {
                Logger err = compiler.getConfiguration().getLogger();
                err.info("==== NFSA for complex type " + type.getDescription());
                machine.display(err);
            }
            //                startTime = System.currentTimeMillis();
            machine = FiniteStateMachine.determinize(compiler, machine);
            //                System.err.println("Time to determinize complex type " + getDescription() +
            //                        " - " + (System.currentTimeMillis() - startTime));

            if (debug) {
                Logger err = compiler.getConfiguration().getLogger();
                err.info("==== DFSA for complex type " + type.getDescription());
                machine.display(err);
            }

            machine.setOpenContentWildcard(type.getOpenContentWildcard(),
                                           type.getOpenContentMode() == OpenContentVariety.INTERLEAVE);
            //initialState = machine.getInitialState();
        } catch (SchemaException err) {
            compiler.error("Error in complex type " +
                                   type.getDescription() +
                                   ": " + err.getMessage(), type);
            throw err;
        }
    }

    /**
     * Recompile the complex type when new members are added to the substitution group of an element
     *
     * @param compiler user for error reporting
     * @throws net.sf.saxon.type.SchemaException if an error is encountered
     */

    public void recompile(SchemaCompiler compiler) throws SchemaException {
        if (debug) {
            getConfiguration().getLogger().info("Recompiling type " + type.getDescription());
        }
        built = false;
        buildValidationMachinery(compiler);
        // See bug 4554. Generation of the finite state machine is now deferred, so the expensive part
        // of the operation is only performed once, at the end
    }

    /**
     * Cancel validation and compilation: this happens when the complex type is redefined
     */

    public void cancelValidation() {
        //setValidationStatus(UNVALIDATED);
        built = false;
    }

    /**
     * Get the finite state machine used for validation against this type
     *
     * @return the finite state machine. May be null if this type does not have a complex content
     * model, or has an <code>&lt;all&gt;</code> model
     */

    public FiniteStateMachine getFiniteStateMachine() {
        return machine;
    }

    /**
     * Get the initial dynamic state of the finite state machine used for validating against
     * this type.
     *
     * @return the initial state of the machine. If this is null, then the complex
     * type has an empty content model.
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

}

