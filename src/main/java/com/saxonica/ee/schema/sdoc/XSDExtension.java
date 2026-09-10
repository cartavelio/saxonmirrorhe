package com.saxonica.ee.schema.sdoc;

import com.saxonica.ee.schema.*;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.trans.Err;
import net.sf.saxon.type.SchemaException;

/**
 * An xs:extension element in a schema document
 */

public class XSDExtension extends SchemaElement {

    /*@Nullable*/ private UserComplexType complexType = null;
    private boolean isSimpleContent;

    /**
     * Creates a new Extension element
     */

    public XSDExtension() {
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
        isSimpleContent = (getParent() instanceof XSDSimpleContent);

        complexType.setDerivationMethodName("extension");

        AttributeMap atts = attributes();
        String[] allowed = {"base", "id"};
        allowAttributes(atts, allowed);

        //-- base
        String base = atts.getValue("", "base");
        if (base != null) {
            int baseFingerprint;
            try {
                baseFingerprint = getFingerprint(base, USE_DEFAULT_NS);
            } catch (SchemaException err) {
                error(err.getMessage());
                return;
            }
            checkPermittedType(baseFingerprint, base, "a base type");
            TypeReference baseType = new TypeReference(baseFingerprint, complexType.getConfiguration(), this);
            complexType.setBaseTypeReference(baseType);
        } else {
            missingAttribute("base");
            return;
        }

        //-- @id
        processId();
    }

    /**
     * Validate this schema element, checking all document-level constraints
     *
     * @param compiler used to report errors and warnings
     * @throws SchemaException if any XML representation constraints are violated
     */

    @Override
    public void validate(SchemaCompiler compiler) throws SchemaException {

        checkContentModel(stateTerms, stateTargets, false);

        if (isSimpleContent) {
            for (NodeInfo child : children()) {
                int fp = child.getFingerprint();
                switch (fp) {
                    case StandardNames.XS_GROUP:
                    case StandardNames.XS_SEQUENCE:
                    case StandardNames.XS_CHOICE:
                    case StandardNames.XS_ALL:
                        String name = child.getLocalPart();
                        error((name.equals("all") ? "An" : "A") +
                                ' ' + Err.wrap(name, Err.ELEMENT) + " model group cannot appear in simple content");
                        return;
                    case StandardNames.XS_OPEN_CONTENT:
                        error("An <openContent> element cannot appear in simple content");
                        return;
                }
            }
        }
    }

    // Following FSM generated using:
    // Transform s4s11.scm generate-s4s-code.xsl type=extensionType

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

    private final static int[] state4terms = {StandardNames.XS_ATTRIBUTE, StandardNames.XS_GROUP,
            StandardNames.XS_CHOICE, StandardNames.XS_ATTRIBUTE_GROUP, StandardNames.XS_ALL,
            StandardNames.XS_ANY_ATTRIBUTE, StandardNames.XS_SEQUENCE, StandardNames.XS_ASSERT};
    private final static int[] state4targets = {1, 2, 2, 1, 2, 3, 2, 6};

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
     * Additional processing after all the children of this element have been validated.
     *
     * @param compiler used to report errors and warnings
     */

    @Override
    public void postValidate(SchemaCompiler compiler) throws SchemaException {

        if (complexType == null) {
            // error already reported
            return;
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
                    AttributeGroupReference attrGroupRef = ((XSDAttributeGroup) child).getAttributeGroupReference();
                    complexType.addAttributeGroupReference(attrGroupRef);
                    break;
                }

                case StandardNames.XS_GROUP: {
                    ModelGroupParticle modelGroup = ((XSDGroup) child).getGroupReference();
                    complexType.setParticle(modelGroup);
                    break;
                }

                case StandardNames.XS_SEQUENCE:
                case StandardNames.XS_CHOICE:
                case StandardNames.XS_ALL: {
                    ModelGroupParticle group = ((XSDCompositor) child).getModelGroupParticle();
                    complexType.setParticle(group);
                    break;
                }

                case StandardNames.XS_ASSERT: {
                    Assertion assertion = ((XSDAssert) child).getAssertion();
                    complexType.addAssertion(assertion);
                    break;
                }

                case StandardNames.XS_ANNOTATION: {
                    // no action
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

// Copyright (c) 2004-2022 Saxonica Limited
