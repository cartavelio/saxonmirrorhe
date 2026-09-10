package com.saxonica.ee.schema.sdoc;

//  imported classes and packages

import com.saxonica.ee.schema.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.type.*;
import net.sf.saxon.value.Whitespace;

import java.util.Optional;

/**
 * An xs:complexType element in an XML Schema document
 */

public class XSDComplexType extends SchemaElement {

    /**
     * The value of the mixed attribute: true, false, or null (unspecified)
     */

    Optional<Boolean> mixed = Optional.empty();

    /**
     * Flag indicating whether default attributes (as defined in the xs:schema element) apply to
     * this complex type
     */

    boolean defaultAttributesApply = true;

    /**
     * Flag indicating that this complexType is a child of a <redefine> element
     */
    private boolean isRedefinition;

    /**
     * The Attribute reference for the Attribute we are constructing
     */
    private UserComplexType complexType = null;

    /**
     * Creates a new ComplexType element
     */
    public XSDComplexType() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws SchemaException if any errors are found in the attributes
     */

    @Override
    protected void prepareAttributes() throws SchemaException {

        PreparedSchema schema = getSchema();
        complexType = new UserComplexType(schema.getConfiguration(), this);
        complexType.setSchemaDocumentURI(getParent().getSystemId());
        isRedefinition = (getParent() instanceof XSDRedefine);

        AttributeMap atts = attributes();
        if (isTopLevel()) {
            String[] allowed = {"abstract", "block", "defaultAttributesApply", "final", "id", "mixed", "name"};
            allowAttributes(atts, allowed);
            requireAttribute(atts, "name");
        } else {
            String[] allowed = {"defaultAttributesApply", "id", "mixed"};
            allowAttributes(atts, allowed);
        }

        String debug = atts.getValue(NamespaceUri.SAXON, "debug");
        if (debug != null) {
            debug = Whitespace.trim(debug);
            if ("true".equals(debug) || "1".equals(debug)) {
                complexType.setSaxonDebug(true);
            }
        }

        //  @id
        processId();

        //  @name
        String attValue = Whitespace.trim(atts.getValue(NamespaceUri.NULL, "name"));

        // allocate fingerprint
        if (attValue != null) {
            StructuredQName typeName;
            try {
                typeName = getComponentName(attValue, REQUIRE_TARGET_NS);
            } catch (SchemaException err) {
                // for error recovery, use a substitute name
                typeName = StandardNames.SQ_XS_INVALID_NAME;
                attValue = "invalid-complex-type-definition-" + hashCode();
                error(err.getMessage());
            }

            int fp = getNamePool().allocateFingerprint(typeName.getNamespaceUri(), typeName.getLocalPart());
            complexType.setTypeName(typeName, fp);

            if (!isRedefinition) {
                if (schema.getSchemaType(complexType.getTypeName()) != null && fp > 1023) {
                    // don't report duplicates for built-in types: this might be the schema for schemas
                    duplicate("type", typeName);
                }
            }
        } else {
            // allocate a fingerprint for this anonymous type.
            NodeName nc = allocateAnonymousTypeCode();
            complexType.setTypeName(nc.getStructuredQName(), nc.getFingerprint());
            schema.addAnonymousType(complexType);
        }

        //  @mixed
        String mixedAtt = Whitespace.trim(atts.getValue(NamespaceUri.NULL, "mixed"));
        if (mixedAtt != null) {
            mixed = Optional.of(parseBooleanAttribute("mixed", mixedAtt));
        }


        //  @abstract
        attValue = atts.getValue(NamespaceUri.NULL, "abstract");
        if (attValue != null) {
            boolean isAbstract = parseBooleanAttribute("abstract", attValue);
            complexType.setAbstract(isAbstract);
        }

        //  @block
        String blockVal = atts.getValue(NamespaceUri.NULL, "block");
        if (blockVal == null) {
            complexType.setFinalProhibitions(getXSDSchema().getBlockDefault());
        } else {
            int f = parseFinalOrBlock("block",
                    blockVal,
                    Derivation.DERIVATION_EXTENSION |
                                              Derivation.DERIVATION_RESTRICTION);
            complexType.setBlock(f);
        }

        //  @final
        String finalVal = atts.getValue(NamespaceUri.NULL, "final");
        if (finalVal == null) {
            complexType.setFinalProhibitions(getXSDSchema().getFinalDefault());
        } else {
            int f = parseFinalOrBlock("final",
                    finalVal,
                    Derivation.DERIVATION_EXTENSION |
                                              Derivation.DERIVATION_RESTRICTION);
            complexType.setFinalProhibitions(f);
        }

        //  @defaultAttributesApply
        String applyAtt = Whitespace.trim(atts.getValue(NamespaceUri.NULL, "defaultAttributesApply"));
        if (applyAtt != null) {
            if (applyAtt.equals("true") || applyAtt.equals("1")) {
                defaultAttributesApply = true;
            } else if (applyAtt.equals("false") || applyAtt.equals("0")) {
                defaultAttributesApply = false;
            } else {
                invalidAttributeValue("defaultAttributesApply", applyAtt, "Must be true or false");
            }
        }

    }

    /**
     * Get the UserComplexType represented by this complexType element in the schema document
     *
     * @return the corresponding UserComplexType object
     */
    /*@Nullable*/
    public UserComplexType getComplexType() {
        return complexType;
    }

    /**
     * Validate this schema element, checking all document-level constraints
     *
     * @param compiler used for error reporting
     * @throws SchemaException if any XML representation constraints are violated
     */

    @Override
    public void validate(SchemaCompiler compiler) throws SchemaException {

        checkContentModel(stateTerms, stateTargets, false);

        boolean foundSimpleContent = false;
        boolean foundComplexContent = false;

        // Set the variety provisionally
        boolean thisMixed = mixed.isPresent() && mixed.get();
        complexType.setVariety(thisMixed ? ComplexVariety.MIXED : ComplexVariety.ELEMENT_ONLY);

        for (NodeInfo child : children()) {
            int fp = child.getFingerprint();
            if (fp == StandardNames.XS_SIMPLE_CONTENT) {
                foundSimpleContent = true;
                complexType.setVariety(ComplexVariety.SIMPLE);
                if (thisMixed && compiler.getLanguageVersion() == Configuration.XSD11) {
                    // XSD 1.1 bans mixed="true" when simpleContent is used
                    error("The <complexType> must not specify mixed='true' when <simpleContent> is present");
                }
            } else if (fp == StandardNames.XS_COMPLEX_CONTENT) {
                foundComplexContent = true;
                Optional<Boolean> ccMixed = ((XSDComplexContent) child).getMixedAttribute();
                if (compiler.getLanguageVersion() == Configuration.XSD11) {
                    // XSD 1.1 bans inconsistent values of the mixed attribute between complexType and complexContent
                    if (mixed.isPresent() && ccMixed.isPresent() && !mixed.equals(ccMixed)) {
                        error("If mixed is specified on both <complexType> and <complexContent>, they must agree");
                    }
                }
                boolean eitherMixed = thisMixed || (ccMixed.isPresent() && ccMixed.get());
                complexType.setVariety(eitherMixed ? ComplexVariety.MIXED : ComplexVariety.ELEMENT_ONLY);
            } else if (fp == StandardNames.XS_ASSERT) {
                if (compiler.getLanguageVersion() == Configuration.XSD10) {
                    error("Assertions require XSD 1.1 to be enabled");
                }
            } else if (fp == StandardNames.XS_OPEN_CONTENT) {
                if (compiler.getLanguageVersion() == Configuration.XSD10) {
                    error("Open content requires XSD 1.1 to be enabled");
                }
            }
        }
        if (isRedefinition && !(foundSimpleContent || foundComplexContent)) {
            error("A redefined complex type must be derived by extension or restriction");
            //return;
        }
    }

    // Following FSM generated using:
    // Transform s4s11.scm generate-s4s-code.xsl type=topLevelComplexType

    private final static int[] state0terms = {StandardNames.XS_CHOICE, StandardNames.XS_SEQUENCE, StandardNames.XS_ASSERT, StandardNames.XS_OPEN_CONTENT, StandardNames.XS_ANNOTATION, StandardNames.XS_ATTRIBUTE_GROUP, StandardNames.XS_ANY_ATTRIBUTE, StandardNames.XS_SIMPLE_CONTENT, StandardNames.XS_ALL, StandardNames.XS_ATTRIBUTE, StandardNames.XS_GROUP, StandardNames.XS_COMPLEX_CONTENT};
    private final static int[] state0targets = {3, 3, 7, 5, 6, 1, 4, 2, 3, 1, 3, 2};

    private final static int[] state1terms = {StandardNames.XS_ATTRIBUTE, StandardNames.XS_ATTRIBUTE_GROUP, StandardNames.XS_ANY_ATTRIBUTE, StandardNames.XS_ASSERT};
    private final static int[] state1targets = {1, 1, 4, 7};

    private final static int[] state2terms = {};
    private final static int[] state2targets = {};

    private final static int[] state3terms = {StandardNames.XS_ATTRIBUTE, StandardNames.XS_ATTRIBUTE_GROUP, StandardNames.XS_ANY_ATTRIBUTE, StandardNames.XS_ASSERT};
    private final static int[] state3targets = {1, 1, 4, 7};

    private final static int[] state4terms = {StandardNames.XS_ASSERT};
    private final static int[] state4targets = {7};

    private final static int[] state5terms = {StandardNames.XS_ATTRIBUTE, StandardNames.XS_GROUP, StandardNames.XS_CHOICE, StandardNames.XS_ATTRIBUTE_GROUP, StandardNames.XS_ALL, StandardNames.XS_ANY_ATTRIBUTE, StandardNames.XS_SEQUENCE, StandardNames.XS_ASSERT};
    private final static int[] state5targets = {1, 3, 3, 1, 3, 4, 3, 7};

    private final static int[] state6terms = {StandardNames.XS_CHOICE, StandardNames.XS_SEQUENCE, StandardNames.XS_ASSERT, StandardNames.XS_OPEN_CONTENT, StandardNames.XS_ATTRIBUTE_GROUP, StandardNames.XS_ANY_ATTRIBUTE, StandardNames.XS_SIMPLE_CONTENT, StandardNames.XS_ALL, StandardNames.XS_ATTRIBUTE, StandardNames.XS_GROUP, StandardNames.XS_COMPLEX_CONTENT};
    private final static int[] state6targets = {3, 3, 7, 5, 1, 4, 2, 3, 1, 3, 2};

    private final static int[] state7terms = {StandardNames.XS_ASSERT};
    private final static int[] state7targets = {7};

    private final static int[][] stateTerms = {
            state0terms, state1terms, state2terms, state3terms, state4terms, state5terms, state6terms, state7terms};
    private final static int[][] stateTargets = {
            state0targets, state1targets, state2targets, state3targets, state4targets, state5targets, state6targets, state7targets};

    @Override
    protected boolean isFinalState(int state) {
        return true;
    }


    /**
     * Hook to allow additional validation of a parent element immediately after its
     * children have been validated.
     *
     * @param compiler used for error reporting
     */

    @Override
    public void postValidate(SchemaCompiler compiler) throws SchemaException {
        if (isRedefinition) {
            PreparedSchema externalSchema = ((XSDRedefine) getParent()).getExternalSchema();
            PreparedSchema redefinedSchema = ((XSDRedefine) getParent()).getRedefinedSchema();
            if (complexType.getBaseTypeFingerprint() != complexType.getFingerprint()) {
                error("A redefined complex type must be based on the type with the same name");
            }
            if (externalSchema == null) {
                return; // error already reported
            }
            UserDefinedType oldType = (UserDefinedType) externalSchema.getSchemaType(complexType.getTypeName());
            if (oldType == null) {
                error("Cannot redefine complex type " + Err.wrap(complexType.getName()) +
                        " because it is not present in the redefined schema");
                return;
            } else if (oldType instanceof SimpleType) {
                error("Cannot redefine a simple type " + Err.wrap(complexType.getName()) +
                        "as a complex type");
            }
            if (!((XSDRedefine) getParent()).getExternalSchemaDocumentURI().equals(oldType.getSchemaDocumentURI())) {
                // Test added in Saxon 9.3
                compiler.warning("The redefined complex type was found, but not in the schema document referenced by " +
                        "the schemaLocation attribute of the containing <xs:redefine> element. This is not allowed by the XSD " +
                        "specification. However, Saxon does not currently enforce this rule.", SaxonErrorCode.SXSD1016, this);
            }
            complexType.setRedefinitionLevel(oldType.getRedefinitionLevel() + 1);
            // rename the type being redefined
            NamespaceUri uri = getXSDSchema().getTargetNamespace();
            NamePool pool = getNamePool();
            String localName = pool.getLocalName(complexType.getFingerprint());
            String newLocalName = localName + '_' + complexType.hashCode();
            String prefix = complexType.getTypeName().getPrefix();
            int newFingerprint = pool.allocateFingerprint(uri, newLocalName);
            oldType.setTypeName(new StructuredQName(prefix, uri, newLocalName), newFingerprint);
            TypeReference baseType = new TypeReference(newFingerprint, complexType.getConfiguration(), this);
            complexType.setBaseTypeReference(baseType);
            redefinedSchema.addType(oldType);
            redefinedSchema.addType(complexType);
            // If this is a complex type with simple content, then the base type of the simple type has
            // been temporarily set to reference the base complex type. Because this has been renamed, we
            // need to set the pointer to the new name.
            if (complexType.isSimpleContent()) {
                TypeReference simpleTypeRef = complexType.getSimpleContentTypeReference();
                SimpleType simpleType = (SimpleType) simpleTypeRef.getTarget();
                if (simpleType instanceof SimpleTypeDefinition) {
                    ((SimpleTypeDefinition) simpleType).setBaseTypeReference(baseType);
                    redefinedSchema.addType(simpleType);
                }
            }
        } else {
            if (complexType.getBaseTypeReference() == null) {
                TypeReference ref = new TypeReference(StandardNames.XS_ANY_TYPE, complexType.getConfiguration(), this);
                ref.setTarget(AnyType.getInstance());
                complexType.setBaseTypeReference(ref);
            }
            if (complexType.getBaseTypeFingerprint() == complexType.getFingerprint() &&
                    complexType.getFingerprint() != StandardNames.XS_ANY_TYPE) {
                // don't report an error for xs:anyType, we must be processing the schema for schemas
                error("A complex type must not be based on itself");
            }
        }
        assembleParticles();
    }

    /**
     * Assemble the particles that make up this complex type.
     *
     * @throws net.sf.saxon.type.SchemaException
     *          if an error in the schema is found
     */
    private void assembleParticles() throws SchemaException {

        Wildcard openContentWildcard = null;
        String openContentMode = null;

        AttributeGroupReference defaultAttributes;
        XSDSchemaElement schemaDefiningDefaults;
        if (getParent() instanceof XSDOverride) {
            SchemaDocument original = ((XSDOverride) getParent()).getOriginalSchemaDocument();
            schemaDefiningDefaults = original.getXSDSchemaElement();
        } else {
            schemaDefiningDefaults = getXSDSchema();
        }
        defaultAttributes = schemaDefiningDefaults.getDefaultAttributeGroupReference();
        if (defaultAttributes != null && defaultAttributesApply) {
            complexType.addAttributeGroupReference(defaultAttributes);
        }

        for (NodeInfo child : children()) {
            switch (child.getFingerprint()) {
                case StandardNames.XS_ANY_ATTRIBUTE: {
                    AttributeWildcard wildcard = ((XSDAnyAttribute) child).getWildcard();
                    complexType.setAnyAttribute(wildcard);
                    break;
                }

                case StandardNames.XS_ATTRIBUTE: {
                    AttributeUse attrDecl = ((XSDAttribute) child).getAttributeUse();
                    complexType.addAttributeUse(attrDecl);
                    break;
                }

                case StandardNames.XS_ATTRIBUTE_GROUP: {
                    AttributeGroupReference attrGroupRef =
                            ((XSDAttributeGroup) child).getAttributeGroupReference();
                    complexType.addAttributeGroupReference(attrGroupRef);
                    break;
                }

                case StandardNames.XS_ELEMENT: {
                    ElementParticle element = ((XSDElement) child).getElementParticle();
                    complexType.setParticle(element);
                    break;
                }

                case StandardNames.XS_GROUP: {
                    ModelGroupParticle modelGroup = ((XSDGroup) child).getGroupReference();
                    complexType.setParticle(modelGroup);
                    break;
                }

                case StandardNames.XS_SIMPLE_CONTENT: {
                    // no action needed
                    break;
                }

                case StandardNames.XS_COMPLEX_CONTENT: {
                    openContentWildcard = complexType.getOpenContentWildcard();
                    openContentMode = complexType.getOpenContentModeString();
                    break;
                }

                case StandardNames.XS_SEQUENCE:
                case StandardNames.XS_CHOICE:
                case StandardNames.XS_ALL: {
                    complexType.setParticle(((XSDCompositor) child).getModelGroupParticle());
                    break;
                }

                case StandardNames.XS_ASSERT: {
                    complexType.addAssertion(((XSDAssert) child).getAssertion());
                    break;
                }

                case StandardNames.XS_ANNOTATION: {
                    // no action
                    break;
                }

                case StandardNames.XS_OPEN_CONTENT: {
                    ElementWildcard ew = ((XSDOpenContent) child).getElementWildcard();
                    openContentWildcard = (ew == null ? null : ew.getWildcard());
                    openContentMode = ((XSDOpenContent) child).getMode();
                    break;
                }
            }
        }


        if (openContentWildcard != null) {
            complexType.setOpenContentWildcard(openContentWildcard, openContentMode, true);
        } else if (schemaDefiningDefaults.getDefaultOpenContentWildcard() != null && !"none".equals(openContentMode)) {
            complexType.setOpenContentWildcard(
                    schemaDefiningDefaults.getDefaultOpenContentWildcard(),
                    schemaDefiningDefaults.getDefaultOpenContentMode(),
                    schemaDefiningDefaults.defaultOpenContentAppliesToEmpty());
        }

    }

}

// Copyright (c) 2004-2023 Saxonica Limited
