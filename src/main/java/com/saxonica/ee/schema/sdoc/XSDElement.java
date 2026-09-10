package com.saxonica.ee.schema.sdoc;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NodeSelector;
import net.sf.saxon.str.StringView;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.tree.util.Navigator;
import net.sf.saxon.type.Derivation;
import net.sf.saxon.type.SchemaException;

import java.util.StringTokenizer;

/**
 * An xs:element element in a schema document
 */

public class XSDElement extends SchemaElement {

    private boolean foundTypeReference = false;
    private boolean isReference = false;
    private StructuredQName elementName;

    /**
     * The element declaration for the element definition under construction.
     */
    private ElementDecl element = null;

    /**
     * The element particle - not created if this is a global element declaration
     */

    private ElementParticle elementParticle = null;

    /**
     * Creates a new XSDElement element
     */
    public XSDElement() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws SchemaException if any errors are found in the attributes
     */

    @Override
    protected void prepareAttributes() throws SchemaException {

        PreparedSchema schema = getSchema();

        AttributeMap atts = attributes();
        isReference = atts.getValue(NamespaceUri.NULL, "ref") != null;

        if (isTopLevel()) {
            String[] allowed = {"abstract", "block", "default", "final",
                    "fixed", "id", "name",
                    "nillable", "substitutionGroup", "type"};
            allowAttributes(atts, allowed);
            requireAttribute(atts, "name");
        } else {
            String[] allowed = {"block", "default", "fixed", "form",
                    "id", "maxOccurs", "minOccurs", "name",
                    "nillable", "ref", "targetNamespace", "type"};
            allowAttributes(atts, allowed);
        }

        // @form
        String form = atts.getValue(NamespaceUri.NULL, "form");
        if (form != null) {
            if (form.equals("qualified") || form.equals("unqualified")) {
                //element.setForm(attValue);
            } else {
                invalidAttributeValue("form", form, "Must be 'qualified' or 'unqualified'");
            }
        } else if (isTopLevel()) {
            form = "qualified";
        } else {
            form = getXSDSchema().getElementFormDefault();
            if (form == null) {
                form = "unqualified";
            }
        }

        // @targetNamespace
        String localTargetNamespace = atts.getValue(NamespaceUri.NULL, "targetNamespace");
        if (localTargetNamespace != null) {
            if (getXSDSchema().getSchemaCompiler().getLanguageVersion() == Configuration.XSD10) {
                error("The targetNamespace cannot be defined locally unless XSD 1.1 is enabled");
            }
            if (atts.getValue(NamespaceUri.NULL, "name") == null) {
                error("If there is no @name then there must be no @targetNamespace");
            }
            checkMutuallyExclusiveAttributes("targetNamespace", "form");
        }

        // @name
        String attValue = atts.getValue(NamespaceUri.NULL, "name");
        if (attValue != null) {
            element = new ElementDecl((EnterpriseConfiguration) getConfiguration(), isTopLevel(), this);
            element.setGeneratedId(Navigator.getPath(this));
            element.setNamespaceResolver(makeNamespaceContext());
            StructuredQName nc;
            try {
                if (localTargetNamespace != null || "qualified".equals(form)) {
                    nc = getComponentName(attValue, REQUIRE_TARGET_NS);
                } else {
                    nc = getComponentName(attValue, REQUIRE_NULL_NS);
                }
            } catch (SchemaException err) {
                // for error recovery, use a substitute name
                nc = StandardNames.SQ_XS_INVALID_NAME;
                error(err.getMessage());
            }
            elementName = nc;
            element.setElementName(nc);
            schema.addSubstitutionGroupMember(element, element);
            if (!isTopLevel()) {
                elementParticle = new ElementParticle((EnterpriseConfiguration) getConfiguration(), this);
                elementParticle.setTargetComponentName(nc);
                elementParticle.setReference(element);
                schema.addComponent(elementParticle);

                // check for common mistake of using <element name="x"/> instead of <element ref="x"/>

                if (atts.getValue(NamespaceUri.NULL, "type") == null && !hasChildNodes() && !isReference) {
                    boolean existsAsTopLevel = schema.getElementDecl(nc) != null;
                    if (!existsAsTopLevel) {
                        String localName = nc.getLocalPart();
                        XSDSchemaElement s = getXSDSchema();
                        for (NodeInfo ignored : s.children(NodeSelector.of(x -> x instanceof XSDElement &&
                                localName.equals(x.getAttributeValue(NamespaceUri.NULL, "name"))))) {
                            existsAsTopLevel = true;
                            break;
                        }
                    }
                    if (existsAsTopLevel) {
                        warning("Local element declaration matches the name of a global element, and allows any content. " +
                                "Did you mean to write ref='" + attValue + "' instead of name='" + attValue + "'? " +
                                "To suppress this warning, add type='xs:anyType'");
                    }
                }

            }
        } else {
            if (!isReference) {
                error("Either the @name or the @ref attribute must be supplied");
            }
        }

        // @ref
        attValue = atts.getValue(NamespaceUri.NULL, "ref");
        if (attValue != null) {
            StructuredQName nc;
            try {
                nc = getComponentName(attValue, USE_DEFAULT_NS);
            } catch (SchemaException err) {
                nc = StandardNames.SQ_XS_INVALID_NAME;
                error(err.getMessage());
            }
            elementName = nc;
            elementParticle = new ElementParticle((EnterpriseConfiguration) getConfiguration(), this);
            elementParticle.setTargetComponentName(nc);
            schema.addComponent(elementParticle);

            // report error if ref coexists with other incompatible attributes
            checkMutuallyExclusiveAttributes("ref", "name");
            checkMutuallyExclusiveAttributes("ref", "block");
            checkMutuallyExclusiveAttributes("ref", "default");
            checkMutuallyExclusiveAttributes("ref", "fixed");
            checkMutuallyExclusiveAttributes("ref", "form");
            checkMutuallyExclusiveAttributes("ref", "nillable");
            checkMutuallyExclusiveAttributes("ref", "type");
        }

        // error recovery code
        if (element == null && isTopLevel()) {
            element = new ElementDecl((EnterpriseConfiguration) getConfiguration(), isTopLevel(), this);
            StringBuilder buff = new StringBuilder(64);
            generateId(buff);
            element.setGeneratedId(buff.toString());
            element.setNamespaceResolver(getAllNamespaces());
            element.setElementName(
                    new StructuredQName("", NamespaceUri.NULL, "unnamed-error-element-at-line-" + getLineNumber()));
            schema.addSubstitutionGroupMember(element, element);
        }
        if (elementParticle == null && !isTopLevel()) {
            element = new ElementDecl((EnterpriseConfiguration) getConfiguration(), isTopLevel(), this);
            StringBuilder buff = new StringBuilder(64);
            generateId(buff);
            element.setGeneratedId(buff.toString());
            element.setNamespaceResolver(getAllNamespaces());
            StructuredQName nameCode = new StructuredQName("", NamespaceUri.NULL, "unnamed-error-element-at-line-" + getLineNumber());
            element.setElementName(nameCode);
            schema.addSubstitutionGroupMember(element, element);
            elementParticle = new ElementParticle((EnterpriseConfiguration) getConfiguration(), this);
            elementParticle.setTargetComponentName(nameCode);
        }


        // @id
        processId();

        if (element != null) {

            // @abstract
            attValue = atts.getValue(NamespaceUri.NULL, "abstract");
            if (attValue != null) {
                boolean isAbstract = parseBooleanAttribute("abstract", attValue);
                element.setAbstract(isAbstract);
            }

            // @block
            attValue = atts.getValue(NamespaceUri.NULL, "block");
            if (attValue == null) {
                if (element != null) {
                    element.setBlock(getXSDSchema().getBlockDefault());
                }
            } else {
                int f = parseFinalOrBlock("block",
                        attValue,
                        Derivation.DERIVATION_EXTENSION |
                                                  Derivation.DERIVATION_RESTRICTION |
                                                  Derivation.DERIVE_BY_SUBSTITUTION);
                element.setBlock(f);
            }

            // @default
            attValue = atts.getValue(NamespaceUri.NULL, "default");
            if (attValue != null) {
                element.setValueConstraint(new ValueConstraint(ValueConstraint.Variety.DEFAULT, StringView.tidy(attValue)));
            }

            // @final
            attValue = atts.getValue(NamespaceUri.NULL, "final");
            if (attValue == null) {
                if (element != null) {
                    element.setFinalProhibitions(getXSDSchema().getFinalDefault());
                }
            } else {
                int f = parseFinalOrBlock("final",
                        attValue,
                        Derivation.DERIVATION_EXTENSION |
                                                  Derivation.DERIVATION_RESTRICTION);
                element.setFinalProhibitions(f);
            }

            // @fixed
            attValue = atts.getValue(NamespaceUri.NULL, "fixed");
            if (attValue != null) {
                if (atts.getValue(NamespaceUri.NULL, "default") != null) {
                    mutuallyExclusiveAttributes("default", "fixed");
                } else {
                    element.setValueConstraint(new ValueConstraint(ValueConstraint.Variety.FIXED, StringView.tidy(attValue)));
                }
            }

            // @substitutionGroup
            attValue = atts.getValue(NamespaceUri.NULL, "substitutionGroup");
            if (attValue != null) {
                StringTokenizer st = new StringTokenizer(attValue, " \t\r\n");
                while (st.hasMoreTokens()) {
                    String head = st.nextToken();
                    try {
                        StructuredQName qName = getComponentName(head, USE_DEFAULT_NS);
                        element.addSubstitutionGroupHead(qName);
                    } catch (SchemaException err) {
                        error(err.getMessage());
                    }
                }
            }

            // @type
            attValue = atts.getValue(NamespaceUri.NULL, "type");
            if (attValue != null) {
                foundTypeReference = true;
                int fp;
                try {
                    fp = getFingerprint(attValue, USE_DEFAULT_NS);
                    checkPermittedType(fp, attValue, "an element type");
                    TypeReference ref = new TypeReference(fp, element.getConfiguration(), this);
                    element.setTypeReference(ref);
                } catch (SchemaException err) {
                    error(err.getMessage());
                }
            }

            // @nillable
            attValue = atts.getValue(NamespaceUri.NULL, "nillable");
            if (attValue != null) {
                boolean isNillable = parseBooleanAttribute("nillable", attValue);
                element.setNillable(isNillable);
            }
        }

        if (elementParticle != null) {
            /*
             * @minOccurs
             * if minOccurs is present the value is the int value of
             * of the attribute, otherwise minOccurs is 1.
             */
            attValue = atts.getValue(NamespaceUri.NULL, "minOccurs");
            int minOccurs = 1;
            if (attValue != null) {
                minOccurs = processMinOccurs(attValue);
                elementParticle.setMinOccurs(minOccurs);
            }

            /*
             * @maxOccurs
             * If maxOccurs is present, the value is either unbounded
             * or the int value of the attribute, otherwise maxOccurs
             * equals the minOccurs value.
             */
            attValue = atts.getValue(NamespaceUri.NULL, "maxOccurs");
            if (attValue != null) {
                int maxOccurs = processMaxOccurs(attValue);
                if (maxOccurs >= 0 && maxOccurs < minOccurs) {
                    error("maxOccurs is less than minOccurs");
                }
                elementParticle.setMaxOccurs(maxOccurs);
            } else {
                if (minOccurs > 0) {
                    elementParticle.setMaxOccurs(minOccurs);
                } else {
                    elementParticle.setMaxOccurs(1);
                }
                int maxOccurs = elementParticle.getMaxOccurs();
                if (maxOccurs != -1 && elementParticle.getMinOccurs() > maxOccurs) {
                    error("maxOccurs must not be less than minOccurs");
                }
            }
        }
    }

    /**
     * Return the element declaration schema component corresponding to this xs:element element
     *
     * @return the schema component representing the element declaration
     */
    public ElementDecl getElementDeclaration() {
        return element;
    }

    /**
     * Return the element particle schema component
     *
     * @return the schema component representing the element particle
     */

    /*@Nullable*/
    public ElementParticle getElementParticle() {
        return elementParticle;
    }


    /**
     * Validate this schema element, checking all document-level constraints
     *
     * @param compiler used for reporting errors
     * @throws SchemaException if any XML representation constraints are violated
     */

    @Override
    public void validate(SchemaCompiler compiler) throws SchemaException {

        String localTargetNamespace = getAttributeValue(NamespaceUri.NULL, "targetNamespace");
        if (localTargetNamespace != null &&
                !NamespaceUri.of(localTargetNamespace).equals(getXSDSchema().getTargetNamespace())) {
            checkLocalTargetNamespace("element");
        }

        checkContentModel(stateTerms, stateTargets, false);

        // Set the containing complex type of an element declaration, if any

        if (element != null) {
            AxisIterator ancIter = iterateAxis(AxisInfo.ANCESTOR);
            NodeInfo anc;
            while ((anc = ancIter.next()) != null) {
                if (anc instanceof XSDComplexType) {
                    element.setContainingComplexType(((XSDComplexType) anc).getComplexType());
                    break;
                } else if (anc instanceof XSDGroup) {
                    element.setContainingModelGroup(((XSDGroup) anc).getGroupDeclaration());
                    break;
                }
            }
        }

        XSDAlternative lastAlternative = null;
        for (NodeInfo child : children()) {
            String childName = child.getLocalPart();
            int childfp = child.getFingerprint();
            switch (childfp) {
                case StandardNames.XS_ANNOTATION: {
                    break;
                }
                case StandardNames.XS_COMPLEX_TYPE: {
                    if (isReference) {
                        mutuallyExclusiveElementAndAttribute("complexType", "ref");
                    }

                    if (foundTypeReference) {
                        mutuallyExclusiveElementAndAttribute("complexType", "type");
                    }
                    break;
                }
                case StandardNames.XS_SIMPLE_TYPE: {
                    if (isReference) {
                        mutuallyExclusiveElementAndAttribute("simpleType", "ref");
                    }

                    if (foundTypeReference) {
                        mutuallyExclusiveElementAndAttribute("simpleType", "type");
                    }
                    break;
                }
                case StandardNames.XS_ALTERNATIVE: {
                    if (isReference) {
                        mutuallyExclusiveElementAndAttribute("alternative", "ref");
                    }
                    if (compiler.getLanguageVersion() == Configuration.XSD10) {
                        error("Conditional Type Assignment requires XML Schema 1.1 to be enabled");
                        return;
                    }
                    if (lastAlternative != null) {
                        if (lastAlternative.getTypeAlternative().getCondition() == null) {
                            lastAlternative.error("The test attribute is required on all alternatives except the last");
                        }
                    }
                    lastAlternative = (XSDAlternative) child;
                    break;
                }
                case StandardNames.XS_KEY:
                case StandardNames.XS_KEYREF:
                case StandardNames.XS_UNIQUE: {
                    if (isReference) {
                        mutuallyExclusiveElementAndAttribute(childName, "ref");
                    }
                    break;
                }
                default: {
                    illegalElement(child);
                    break;
                }
            }
        }

    }

    // Following state machine generated using:
    // Transform -s:s4s11b.scm generate-s4s-code.xsl type=element

    private final static int[] state0terms = {StandardNames.XS_ANNOTATION, StandardNames.XS_SIMPLE_TYPE, StandardNames.XS_KEY, StandardNames.XS_COMPLEX_TYPE, StandardNames.XS_KEYREF, StandardNames.XS_UNIQUE, StandardNames.XS_ALTERNATIVE};
    private final static int[] state0targets = {1, 2, 3, 2, 3, 3, 4};

    private final static int[] state1terms = {StandardNames.XS_SIMPLE_TYPE, StandardNames.XS_KEY, StandardNames.XS_COMPLEX_TYPE, StandardNames.XS_KEYREF, StandardNames.XS_UNIQUE, StandardNames.XS_ALTERNATIVE};
    private final static int[] state1targets = {2, 3, 2, 3, 3, 4};

    private final static int[] state2terms = {StandardNames.XS_KEY, StandardNames.XS_KEYREF, StandardNames.XS_UNIQUE, StandardNames.XS_ALTERNATIVE};
    private final static int[] state2targets = {3, 3, 3, 4};

    private final static int[] state3terms = {StandardNames.XS_KEY, StandardNames.XS_KEYREF, StandardNames.XS_UNIQUE};
    private final static int[] state3targets = {3, 3, 3};

    private final static int[] state4terms = {StandardNames.XS_KEY, StandardNames.XS_KEYREF, StandardNames.XS_UNIQUE, StandardNames.XS_ALTERNATIVE};
    private final static int[] state4targets = {3, 3, 3, 4};

    private final static int[][] stateTerms = {
            state0terms, state1terms, state2terms, state3terms, state4terms};
    private final static int[][] stateTargets = {
            state0targets, state1targets, state2targets, state3targets, state4targets};

    @Override
    protected boolean isFinalState(int state) {
        return state == 0 || state == 1 || state == 2 || state == 3 || state == 4;
    }

    /**
     * Additional processing after all the children of this element have been validated.
     *
     * @param compiler used primarily for reporting errors
     */

    @Override
    public void postValidate(SchemaCompiler compiler) throws SchemaException {

        if (isReference) {
            return;
        }
        TypeAlternative lastAlternative = null;
        for (NodeInfo child : children()) {
            int childfp = child.getFingerprint();
            switch (childfp) {
                case StandardNames.XS_COMPLEX_TYPE: {
                    UserDefinedType schemaType = ((XSDComplexType) child).getComplexType();
                    TypeReference ref = new TypeReference(schemaType.getFingerprint(), element.getConfiguration(), child);
                    ref.setTarget(schemaType);
                    element.setTypeReference(ref);
                    schemaType.setContainingDeclaration(elementName, true);
                    break;
                }
                case StandardNames.XS_SIMPLE_TYPE: {
                    UserDefinedType schemaType = ((XSDSimpleType) child).getSimpleTypeDefinition();
                    TypeReference ref = new TypeReference(schemaType.getFingerprint(), element.getConfiguration(), child);
                    ref.setTarget(schemaType);
                    element.setTypeReference(ref);
                    schemaType.setContainingDeclaration(elementName, true);
                    ((UserSimpleType) schemaType).setContextComponent(element);
                    break;
                }
                case StandardNames.XS_ALTERNATIVE:
                    lastAlternative = ((XSDAlternative) child).getTypeAlternative();
                    element.addTypeAlternative(lastAlternative);
                    break;

                case StandardNames.XS_KEY:
                case StandardNames.XS_KEYREF:
                case StandardNames.XS_UNIQUE: {
                    IdentityConstraintReference constraint = ((XSDIdentityConstraint) child).getIdentityConstraint();
                    element.addIdentityConstraint(constraint);
                    break;
                }
            }
            if (lastAlternative != null && lastAlternative.getCondition() != null) {
                // If the last explicit alternative has a condition, add an implicit final alternative as the
                // default type
                EnterpriseConfiguration config = (EnterpriseConfiguration) getConfiguration();
                lastAlternative = new TypeAlternative(config, null, null);
                int typeRef = element.getTypeFingerprint();
                if (typeRef == -1) {
                    typeRef = StandardNames.XS_ANY_TYPE;
                }
                lastAlternative.setTypeReference(new TypeReference(typeRef, config, element));
                lastAlternative.setLocator(this);
                //element.addTypeAlternative(lastAlternative);
            }
        }

    }

}

// Copyright (c) 2004-2023 Saxonica Limited
