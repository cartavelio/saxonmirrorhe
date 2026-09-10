package com.saxonica.ee.schema.sdoc;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.type.SchemaException;

/**
 * An xs:unique, xs:key, or xs:keyref element in an XML Schema document
 */

public class XSDIdentityConstraint extends SchemaElement {

    /**
     * Flag indicating whether this is a reference to a component defined elsewhere
     */
    private boolean isRef = false;

    /**
     * The fingerprint of the name of this component (or reference)
     */
    private StructuredQName constraintName;

    /**
     * The IdentityConstraint we are unmarshalling
     */
    private IdentityConstraint identityConstraint = null;

    /**
     * Creates a new IdentityConstraint element
     */
    public XSDIdentityConstraint() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws SchemaException if any errors are found in the attributes
     */

    @Override
    protected void prepareAttributes() throws SchemaException {

        AttributeMap atts = attributes();
        if (getFingerprint() == StandardNames.XS_KEYREF) {
            String[] allowed = {"id", "name", "ref", "refer"};
            allowAttributes(atts, allowed);
        } else {
            String[] allowed = {"id", "name", "ref"};
            allowAttributes(atts, allowed);
        }

        //-- @id
        processId();

        checkMutuallyExclusiveAttributes("ref", "name");
        checkMutuallyExclusiveAttributes("ref", "refer");

        String refAtt = atts.getValue("", "ref");
        StructuredQName nameCode;

        if (refAtt != null) {
            if (getXSDSchema().getSchemaCompiler().getLanguageVersion() == Configuration.XSD10) {
                error("The @ref attribute cannot be used on an integrity constraint unless XSD 1.1 is enabled");
            }
            nameCode = getComponentName(refAtt, USE_DEFAULT_NS);
            isRef = true;

        } else {

            String name = atts.getValue("", "name");
            if (name == null) {
                missingAttribute("name");
                name = "saxon-error-" + getLocalPart() + "-name"; // for error recovery
            }
            nameCode = getComponentName(name, REQUIRE_TARGET_NS);

            //-- keyref
            if (getFingerprint() == StandardNames.XS_KEYREF) {
                String referAtt = atts.getValue("", "refer");
                if (referAtt == null) {
                    missingAttribute("refer");
                    return;
                }
                StructuredQName referFingerprint;
                try {
                    referFingerprint = getComponentName(referAtt, USE_DEFAULT_NS);
                } catch (SchemaException err) {
                    referFingerprint = StandardNames.SQ_XS_INVALID_NAME;
                    error(err.getMessage());
                }
                identityConstraint = new KeyRef((EnterpriseConfiguration)getConfiguration(), referFingerprint);
            }
            //-- unique
            else if (getFingerprint() == StandardNames.XS_UNIQUE) {
                identityConstraint = new Unique();
            }
            //-- key
            else {
                identityConstraint = new Key();
            }

            PreparedSchema schema = getSchema();
            //identityConstraint.setSchema(schema);
            identityConstraint.setConfiguration((EnterpriseConfiguration) getConfiguration());
            identityConstraint.setLocator(this);
            identityConstraint.setConstraintName(nameCode);

            if (schema.getIdentityConstraint(nameCode) != null) {
                duplicate("identity constraint", name);
            }

            schema.addIdentityConstraint(identityConstraint);
            
        }

        constraintName = nameCode;

    }

    /**
     * Ask whether this is a reference
     *
     * @return true if this is a reference (@ref attribute present)
     */

    public boolean isReference() {
        return isRef;
    }

    /**
     * Returns the IdentityConstraint created.
     *
     * @return the IdentityConstraint created (returns null if this is a reference)
     */
    /*@Nullable*/
    public IdentityConstraintReference getIdentityConstraint() {
        if (constraintName == null) {
            return null;
            // already reported
        }
        IdentityConstraintReference ref =
                new IdentityConstraintReference(constraintName, getFingerprint(), (EnterpriseConfiguration) getConfiguration(), this);
        if (isRef) {
            return ref;
        } else {
            if (identityConstraint == null) {
                return null;    // error already reported
            }
            ref.setTarget(identityConstraint);
        }
        return ref;
    }


    /**
     * Validate this schema element, checking all document-level constraints
     *
     * @param compiler the schema compiler
     * @throws SchemaException if any XML representation constraints are violated
     */

    @Override
    public void validate(SchemaCompiler compiler) throws SchemaException {

        boolean foundAnnotation = false;
        boolean foundSelector = false;
        boolean foundField = false;

        for (NodeInfo child : children()) {
            switch (child.getFingerprint()) {
                case StandardNames.XS_ANNOTATION: {
                    if (foundAnnotation) {
                        duplicateElement("annotation");
                    }
                    if (foundSelector || foundField) {
                        mustBeFirstElement("annotation");
                    }
                    foundAnnotation = true;
                    break;
                }
                case StandardNames.XS_SELECTOR: {
                    if (foundField) {
                        mustPrecede("selector", "field");
                    }
                    if (foundSelector) {
                        duplicateElement("selector");
                    }
                    foundSelector = true;
                    break;
                }
                case StandardNames.XS_FIELD: {
                    foundField = true;
                    break;
                }
                default: {
                    illegalElement(child);
                    break;
                }
            }
        }

        if (isRef) {
            if (foundSelector) {
                mutuallyExclusiveElementAndAttribute("selector", "ref");
            }
            if (foundField) {
                mutuallyExclusiveElementAndAttribute("field", "ref");
            }
        } else {
            if (!foundSelector) {
                missingChildElement("selector");
            }
            if (!foundField) {
                missingChildElement("field");
            }
        }
    }

    /**
     * Process the child elements after validation
     *
     * @param compiler the schema compiler
     */
    @Override
    public void postValidate(SchemaCompiler compiler)
            throws SchemaException {
        if (identityConstraint == null) {
            // means an error was already reported
            return;
        }
        for (NodeInfo child : children()) {
            switch (child.getFingerprint()) {
                case StandardNames.XS_ANNOTATION: {
                    break;
                }
                case StandardNames.XS_SELECTOR: {
                    IdentitySelector selector = ((XSDFieldOrSelector) child).getIdentitySelector();
                    identityConstraint.setSelector(selector);
                    break;
                }
                case StandardNames.XS_FIELD: {
                    IdentityField field = ((XSDFieldOrSelector) child).getIdentityField();
                    identityConstraint.addField(field);
                    if (child.getAttributeValue(NamespaceConstant.SAXON, "order") != null) {
                        if (getFingerprint() != StandardNames.XS_UNIQUE) {
                            error("saxon:order is allowed only on a field of xs:unique");
                        }
                        identityConstraint.setOrdered(true);
                    }
                    break;
                }
            }
        }
    }

}

// Copyright (c) 2004-2022 Saxonica Limited