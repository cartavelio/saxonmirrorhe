package com.saxonica.ee.schema.sdoc;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.*;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.type.ComplexType;
import net.sf.saxon.type.SchemaException;

/**
 * An xs:restriction element that is a child of xs:complexContent within an XML Schema document
 */
public class XSDComplexContentRestriction extends SchemaElement {

    /**
     * The complexType we are constructing
     */
    private UserComplexType complexType = null;


    /**
     * Creates a new Restriction element
     */
    public XSDComplexContentRestriction() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws SchemaException if any errors are found in the attributes
     */

    @Override
    protected void prepareAttributes() throws SchemaException {

        try {
            complexType = getContainingComplexType();
        } catch (SchemaException e) {
            e.setLocator(this);
            error(e);
            return;
        }
        complexType.setDerivationMethodName("restriction");
        complexType.setRestriction(true);

        AttributeMap atts = attributes();
        String[] allowed = {"base", "id"};
        allowAttributes(atts, allowed);

        //-- @id
        processId();

        //-- @base
        String base = atts.getValue(NamespaceUri.NULL, "base");
        if (base != null) {
            int baseFingerprint;
            try {
                baseFingerprint = getFingerprint(base, USE_DEFAULT_NS);
            } catch (SchemaException err) {
                error(err.getMessage());
                return;
            }
            if (baseFingerprint == StandardNames.XS_DAY_TIME_DURATION ||
                    baseFingerprint == StandardNames.XS_YEAR_MONTH_DURATION) {
                warning("Type " + base + " is recognized by Saxon but is not interoperable");
            } else if (baseFingerprint == StandardNames.XS_ANY_ATOMIC_TYPE ||
                    baseFingerprint == StandardNames.XS_UNTYPED) {
                error("Type " + base + " cannot be used as a base type");
            }
            TypeReference baseType = new TypeReference(baseFingerprint, (EnterpriseConfiguration) getConfiguration(), this);
            complexType.setBaseTypeReference(baseType);
        }
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
    }

    // Following FSM generated using
    // Transform s4s11.scm generate-s4s-code.xsl type=complexRestrictionType

    private final static int[] state0terms = {StandardNames.XS_CHOICE, StandardNames.XS_SEQUENCE,
            StandardNames.XS_ASSERT, StandardNames.XS_OPEN_CONTENT, StandardNames.XS_ANNOTATION,
            StandardNames.XS_ATTRIBUTE_GROUP, StandardNames.XS_ANY_ATTRIBUTE, StandardNames.XS_ALL,
            StandardNames.XS_ATTRIBUTE, StandardNames.XS_GROUP};
    private final static int[] state0targets = {2, 2, 6, 4, 5, 1, 3, 2, 1, 2};

    private final static int[] state1terms = {StandardNames.XS_ATTRIBUTE, StandardNames.XS_ATTRIBUTE_GROUP,
            StandardNames.XS_ANY_ATTRIBUTE, StandardNames.XS_ASSERT};
    private final static int[] state1targets = {1, 1, 3, 6};

    private final static int[] state2terms = {StandardNames.XS_ATTRIBUTE, StandardNames.XS_ATTRIBUTE_GROUP,
            StandardNames.XS_ANY_ATTRIBUTE, StandardNames.XS_ASSERT};
    private final static int[] state2targets = {1, 1, 3, 6};

    private final static int[] state3terms = {StandardNames.XS_ASSERT};
    private final static int[] state3targets = {6};

    private final static int[] state4terms = {StandardNames.XS_GROUP, StandardNames.XS_CHOICE,
            StandardNames.XS_ALL, StandardNames.XS_SEQUENCE};
    private final static int[] state4targets = {2, 2, 2, 2};

    private final static int[] state5terms = {StandardNames.XS_CHOICE, StandardNames.XS_SEQUENCE,
            StandardNames.XS_ASSERT, StandardNames.XS_OPEN_CONTENT, StandardNames.XS_ATTRIBUTE_GROUP,
            StandardNames.XS_ANY_ATTRIBUTE, StandardNames.XS_ALL, StandardNames.XS_ATTRIBUTE, StandardNames.XS_GROUP};
    private final static int[] state5targets = {2, 2, 6, 4, 1, 3, 2, 1, 2};

    private final static int[] state6terms = {StandardNames.XS_ASSERT};
    private final static int[] state6targets = {6};

    private final static int[][] stateTerms = {
            state0terms, state1terms, state2terms, state3terms, state4terms, state5terms, state6terms};
    private final static int[][] stateTargets = {
            state0targets, state1targets, state2targets, state3targets, state4targets, state5targets, state6targets};

    @Override
    protected boolean isFinalState(int state) {
        return true;
    }

    /**
     * Get the complex type that is the result of the restriction
     *
     * @return the type formed by restriction
     */

    /*@Nullable*/
    public ComplexType getComplexType() {
        return complexType;
    }

    /**
     * Process child elements after validation
     *
     * @param compiler used for error reporting
     */
    @Override
    public void postValidate(SchemaCompiler compiler)
            throws SchemaException {

        if (complexType == null) {
            return; // error already reported
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
                case StandardNames.XS_SEQUENCE:
                case StandardNames.XS_CHOICE:
                case StandardNames.XS_ALL: {
                    complexType.setParticle(((XSDCompositor) child).getModelGroupParticle());
                    break;
                }
                case StandardNames.XS_GROUP: {
                    ModelGroupParticle modelGroup = ((XSDGroup) child).getGroupReference();
                    complexType.setParticle(modelGroup);
                    break;
                }
                case StandardNames.XS_ASSERT: {
                    Assertion assertion = ((XSDAssert) child).getAssertion();
                    complexType.addAssertion(assertion);
                    break;
                }
                case StandardNames.XS_OPEN_CONTENT: {
                    ElementWildcard ew = ((XSDOpenContent) child).getElementWildcard();
                    Wildcard openContentWildcard = (ew == null ? null : ew.getWildcard());
                    String openContentMode = ((XSDOpenContent) child).getMode();
                    complexType.setOpenContentWildcard(openContentWildcard, openContentMode, true);
                    break;
                }
            }
        }

    }


}

// Copyright (c) 2004-2023 Saxonica Limited
