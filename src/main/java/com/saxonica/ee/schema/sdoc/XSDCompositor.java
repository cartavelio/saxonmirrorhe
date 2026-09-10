package com.saxonica.ee.schema.sdoc;

import com.saxonica.ee.schema.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.trans.Err;
import net.sf.saxon.tree.linked.NodeImpl;
import net.sf.saxon.type.SchemaException;

/**
 * An xs:sequence, xs:choice, or xs:all element in an XML Schema document
 */

public class XSDCompositor extends SchemaElement {

    /**
     * The ModelGroup object we are constructing
     */
    private ModelGroup modelGroup = null;

    /**
     * The ModelGroupParticle that refers to this ModelGroup
     */
    private ModelGroupParticle particle = null;

    /**
     * Creates a new Compositor element
     */
    public XSDCompositor() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws SchemaException if any errors are found in the attributes
     */

    @Override
    protected void prepareAttributes() throws SchemaException {

        AttributeMap atts = attributes();
        String[] allowed = {"id", "maxOccurs", "minOccurs"};
        allowAttributes(atts, allowed);

        String attValue;

        int fp = getFingerprint();
        switch (fp) {
            case StandardNames.XS_SEQUENCE:
                modelGroup = new SequenceModelGroup();
                break;
            case StandardNames.XS_CHOICE:
                modelGroup = new ChoiceModelGroup();
                break;
            case StandardNames.XS_ALL:
                modelGroup = new AllModelGroup();
                break;
            default:
                error("Invalid compositor element name: " + Err.wrap(getLocalPart(), Err.ELEMENT) + "(fp = " + fp + ")");
                return;
        }
        modelGroup.setLocator(this);
        modelGroup.setConfiguration(getSchema().getConfiguration());
        particle = new ModelGroupParticle(getSchema().getConfiguration(), this);
        particle.setTarget(modelGroup);

        NodeImpl parent = getParent();

        /*
         * @maxOccurs
         * If maxOccurs is present, the value is either unbounded
         * or the int value of the attribute, otherwise maxOccurs
         * equals the minOccurs value.
         */
        attValue = atts.getValue("", "maxOccurs");

        if (attValue != null) {
            if (parent.getFingerprint() == StandardNames.XS_GROUP) {
                error("Cannot specify 'maxOccurs' on a top-level '" + getLocalPart() + "' compositor within a named group");
            }
            int max = processMaxOccurs(attValue);
            particle.setMaxOccurs(max);
        }

        // @minOccurs
        attValue = atts.getValue("", "minOccurs");
        if (attValue != null) {
            if (parent.getFingerprint() == StandardNames.XS_GROUP) {
                error("Cannot specify 'minOccurs' on a top-level '" + getLocalPart() + "' compositor within a named group");
            }
            int minOccurs = processMinOccurs(attValue);
            particle.setMinOccurs(minOccurs);
        }

        int maxOccurs = particle.getMaxOccurs();
        if (maxOccurs != -1 && particle.getMinOccurs() > maxOccurs) {
            error("maxOccurs must not be less than minOccurs");
        }

        if (modelGroup instanceof AllModelGroup) {
            int max = particle.getMaxOccurs();
            if (getXSDSchema().getSchemaCompiler().getLanguageVersion() == Configuration.XSD10) {
                if (max != 1) {
                    invalidAttributeValue("maxOccurs", (max == -1 ? "unbounded" : max + ""), "Must be \"1\"");
                    return;
                }
            } else {
                if (max < 0 || max > 1) {
                    invalidAttributeValue("maxOccurs", (max == -1 ? "unbounded" : max + ""), "Must be \"0\" or \"1\"");
                    return;
                }
            }
            if (particle.getMinOccurs() > 1) {
                invalidAttributeValue("minOccurs", particle.getMinOccurs() + "", "Must be \"0\" or \"1\"");
                return;
            }
        }

        //-- @id
        processId();
    }


    /**
     * Returns the constructed Compositor.
     *
     * @return the Compositor that has been constructed
     */

    public ModelGroup getModelGroup() {
        return modelGroup;
    }

    /**
     * Returns the particle referring to the constructed ModelGroup.
     *
     * @return the particle referring to the constructed ModelGroup.
     */

    /*@Nullable*/
    public ModelGroupParticle getModelGroupParticle() {
        return particle;
    }


    /**
     * Validate this schema element, checking all document-level constraints
     *
     * @param compiler used for error reporting
     * @throws SchemaException if any XML representation constraints are violated
     */

    @Override
    public void validate(SchemaCompiler compiler) throws SchemaException {
        boolean foundElement = false;
        boolean foundCompositor = false;
        boolean foundGroup = false;
        boolean foundAnnotation = false;
        for (NodeInfo child : children()) {
            switch (child.getFingerprint()) {
                case StandardNames.XS_ANNOTATION:
                    if (foundElement || foundCompositor || foundGroup) {
                        mustBeFirstElement("annotation");
                    }
                    if (foundAnnotation) {
                        duplicateElement("annotation");
                    }
                    foundAnnotation = true;
                    break;

                case StandardNames.XS_ELEMENT:
                    foundElement = true;
                    break;

                case StandardNames.XS_GROUP:
                    if (modelGroup instanceof AllModelGroup && compiler.getLanguageVersion() == Configuration.XSD10) {
                        illegalElement(child);
                    }
                    foundGroup = true;
                    break;

                case StandardNames.XS_SEQUENCE:
                case StandardNames.XS_CHOICE:
                    if (modelGroup instanceof AllModelGroup) {
                        illegalElement(child);
                    }
                    foundCompositor = true;
                    break;

                case StandardNames.XS_ANY:
                    if (modelGroup instanceof AllModelGroup && compiler.getLanguageVersion() == Configuration.XSD10) {
                        illegalElement(child);
                    }
                    break;

                default:
                    illegalElement(child);
                    break;
            }
        }
    }

    /**
     * Process child elements after validation
     *
     * @param compiler used for error reporting
     */

    @Override
    public void postValidate(SchemaCompiler compiler)
            throws SchemaException {

        for (NodeInfo child : children()) {
            switch (child.getFingerprint()) {

                case StandardNames.XS_ANY:
                    ElementWildcard wildcard = ((XSDAny) child).getElementWildcard();
                    modelGroup.addParticle(wildcard);
                    break;

                case StandardNames.XS_ANNOTATION:
                    break;

                case StandardNames.XS_ELEMENT:
                    ElementParticle element = ((XSDElement) child).getElementParticle();
                    if (modelGroup instanceof AllModelGroup && compiler.getLanguageVersion() == Configuration.XSD10) {
                        if (element.getMinOccurs() > 1) {
                            error("Within <xs:all>, an <xs:element> must have @minOccurs equal to 0 or 1");
                        }
                        if (element.getMaxOccurs() > 1 || element.getMaxOccurs() == -1) {
                            error("Within <xs:all>, an <xs:element> must have @maxOccurs equal to 0 or 1");
                        }
                    }
                    modelGroup.addParticle(element);
                    break;

                case StandardNames.XS_GROUP:
                    ModelGroupParticle modelGroupParticle = ((XSDGroup) child).getGroupReference();
                    this.modelGroup.addParticle(modelGroupParticle);
                    break;

                case StandardNames.XS_SEQUENCE:
                case StandardNames.XS_CHOICE:
                case StandardNames.XS_ALL:
                    ModelGroupParticle p = ((XSDCompositor) child).getModelGroupParticle();
                    this.modelGroup.addParticle(p);
                    break;
            }

        }
    }

}

// Copyright (c) 2004-2022 Saxonica Limited