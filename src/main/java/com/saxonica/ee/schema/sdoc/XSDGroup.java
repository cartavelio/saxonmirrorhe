package com.saxonica.ee.schema.sdoc;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.*;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.transpile.CSharp;
import net.sf.saxon.type.MissingComponentException;
import net.sf.saxon.type.SchemaException;

import java.util.List;

/**
 * An xs:group element in an XML Schema document
 */

public class XSDGroup extends SchemaElement {

    /**
     * The Group declaration for the Group we are constructing. Null if this is a reference.
     */
    private ModelGroupDefinition groupDeclaration = null;

    /**
     * The Group reference for the group we are referencing. Null if this is a declaration.
     */
    private ModelGroupParticle modelGroupParticle = null;

    /**
     * Flag to say that a minOccurs attribute has been found
     */

    private boolean foundMinOccurs = false;

    /**
     * Flag to say that a minOccurs attribute has been found
     */

    private boolean foundMaxOccurs = false;

    /**
     * Flag to indicate that this group is a redefinition
     */

    private boolean isRedefinition;

    /**
     * Creates a new Group element
     */

    public XSDGroup() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws SchemaException if any errors are found in the attributes
     */

    @Override
    protected void prepareAttributes() throws SchemaException {

        PreparedSchema schema = getSchema();
        isRedefinition = (getParent() instanceof XSDRedefine);

        AttributeMap atts = attributes();
        if (isTopLevel()) {
            groupDeclaration = new ModelGroupDefinition((EnterpriseConfiguration) getConfiguration(), this);
            groupDeclaration.setSchemaDocumentURI(getParent().getSystemId());
            String[] allowed = {"id", "name"};
            allowAttributes(atts, allowed);
            requireAttribute(atts, "name");
        } else {
            modelGroupParticle = new ModelGroupParticle((EnterpriseConfiguration) getConfiguration(), this);
            schema.addComponent(modelGroupParticle);
            String[] allowed = {"id", "maxOccurs", "minOccurs", "ref"};
            allowAttributes(atts, allowed);
            requireAttribute(atts, "ref");
        }

        String attValue;

        // @name
        String name = atts.getValue(NamespaceUri.NULL, "name");
        if (groupDeclaration != null) {
            if (name != null) {
                StructuredQName qName;
                try {
                    qName = getComponentName(name, REQUIRE_TARGET_NS);
                } catch (SchemaException err) {
                    error(err.getMessage());
                    qName = StandardNames.SQ_XS_INVALID_NAME;
                }
                groupDeclaration.setModelGroupName(qName);
            } else if (name == null) {
                groupDeclaration.setModelGroupName(StandardNames.SQ_XS_INVALID_NAME);
            }
        }

        /*
         * @maxOccurs
         * If maxOccurs is present, the value is either unbounded
         * or the int value of the attribute, otherwise maxOccurs
         * equals the minOccurs value.
         */
        attValue = atts.getValue(NamespaceUri.NULL, "maxOccurs");
        if (attValue != null && modelGroupParticle != null) {
            foundMaxOccurs = true;
            if (groupDeclaration != null) {
                mutuallyExclusiveAttributes("name", "maxOccurs");
            } else {
                int max = processMaxOccurs(attValue);
                modelGroupParticle.setMaxOccurs(max);
            }
        }
        //-- minOccurs
        attValue = atts.getValue(NamespaceUri.NULL, "minOccurs");
        if (attValue != null && modelGroupParticle != null) {
            foundMinOccurs = true;
            if (groupDeclaration != null) {
                mutuallyExclusiveAttributes("name", "minOccurs");
            } else {
                int minOccurs = processMinOccurs(attValue);
                modelGroupParticle.setMinOccurs(minOccurs);
            }
        }

        if (modelGroupParticle != null) {
            int maxOccurs = modelGroupParticle.getMaxOccurs();
            if (maxOccurs != -1 && modelGroupParticle.getMinOccurs() > maxOccurs) {
                error("maxOccurs must not be less than minOccurs");
            }
        }

        //-- @ref
        attValue = atts.getValue(NamespaceUri.NULL, "ref");
        if (attValue != null && modelGroupParticle != null) {
            if (groupDeclaration != null) {
                mutuallyExclusiveAttributes("name", "ref");
            } else {
                StructuredQName fp;
                try {
                    fp = getComponentName(attValue, USE_DEFAULT_NS);
                } catch (SchemaException err) {
                    error(err.getMessage());
                    return;
                }
                modelGroupParticle.setTargetComponentName(fp);
            }
        }

        //-- @id
        processId();

    }

    /**
     * Returns the Group declaration under construction.
     *
     * @return the constructed Group
     */

    public ModelGroupDefinition getGroupDeclaration() {
        return groupDeclaration;
    }

    /**
     * Returns the Group reference under construction.
     *
     * @return the constructed Group reference
     */

    /*@Nullable*/
    public ModelGroupParticle getGroupReference() {
        return modelGroupParticle;
    }

    /**
     * Validate this schema element, checking all document-level constraints
     *
     * @param compiler used for reporting errors and warnings
     * @throws SchemaException if any XML representation constraints are violated
     */

    @Override
    public void validate(SchemaCompiler compiler) throws SchemaException {

        boolean foundAnnotation = false;
        NodeInfo foundCompositor = null;
        for (NodeInfo child : children()) {
            int fp = child.getFingerprint();
            switch (fp) {
                case StandardNames.XS_ANNOTATION:
                    if (foundAnnotation) {
                        duplicateElement("annotation");
                    }
                    if (foundCompositor != null) {
                        mustBeFirstElement("annotation");
                    }
                    foundAnnotation = true;
                    break;

                case StandardNames.XS_ALL:
                    if (foundMinOccurs) {
                        mutuallyExclusiveElementAndAttribute("all", "minOccurs");
                    }
                    if (foundMaxOccurs) {
                        mutuallyExclusiveElementAndAttribute("all", "maxOccurs");
                    }
                    CSharp.emitCode("goto case Saxon.Hej.om.StandardNames.XS_SEQUENCE;");
                    // fall through
                case StandardNames.XS_SEQUENCE:
                case StandardNames.XS_CHOICE:
                    if (foundCompositor != null) {
                        // only one particle allowed
                        mutuallyExclusive(child.getDisplayName(), foundCompositor.getDisplayName());
                    }
                    if (modelGroupParticle != null) {
                        // no particles allowed in a group reference
                        mutuallyExclusiveElementAndAttribute(child.getDisplayName(), "ref");
                    }
                    foundCompositor = child;
                    break;
                default:
                    illegalElement(child);
                    break;
            }
        }

    }

    /**
     * Additional processing after all the children of this element have been validated.
     *
     * @param compiler used for reporting errors and warnings
     */

    @Override
    public void postValidate(SchemaCompiler compiler) throws SchemaException {

        if (groupDeclaration == null) {
            return;
        }

        for (NodeInfo child : children()) {
            int fp = child.getFingerprint();

            switch (fp) {
                case StandardNames.XS_ALL:
                case StandardNames.XS_SEQUENCE:
                case StandardNames.XS_CHOICE:
                    ModelGroupParticle compositor = ((XSDCompositor) child).getModelGroupParticle();
                    groupDeclaration.addParticle(compositor);
                    break;
            }
        }
        if (isRedefinition) {
            PreparedSchema externalSchema = ((XSDRedefine) getParent()).getExternalSchema();
            PreparedSchema redefinedSchema = ((XSDRedefine) getParent()).getRedefinedSchema();

            if (externalSchema == null || redefinedSchema == null) {
                return;     // error already reported
            }
            ModelGroupDefinition originalGroup = externalSchema.getGroup(groupDeclaration.getModelGroupName());
            if (originalGroup == null) {
                error("Group " + Err.wrap(groupDeclaration.getName()) +
                        " is not defined in the schema being redefined");
                return;
            }
            if (!((XSDRedefine) getParent()).getExternalSchemaDocumentURI().equals(originalGroup.getSchemaDocumentURI())) {
                // Test added in Saxon 9.3
                compiler.warning("The redefined model group was found, but not in the schema document referenced by " +
                        "the schemaLocation attribute of the containing <xs:redefine> element. This is not allowed by the XSD " +
                        "specification. However, Saxon does not currently enforce this rule.", SaxonErrorCode.SXSD1016, this);
            }
            groupDeclaration.setRedefinitionLevel(originalGroup.getRedefinitionLevel() + 1);
            boolean found = patchUpSelfReferences(
                    groupDeclaration.getParticles(),
                    groupDeclaration.getModelGroupName(),
                    originalGroup,
                    false);
            if (!found) {
                groupDeclaration.setRedefinedModelGroupDefinition(originalGroup);
            }

            redefinedSchema.addGroup(groupDeclaration);
        }
    }

    private boolean patchUpSelfReferences(List<Particle> particles, StructuredQName qName, ModelGroupDefinition originalGroup, boolean found)
            throws SchemaException {

        PreparedSchema redefinedSchema = ((XSDRedefine) getParent()).getRedefinedSchema();
        boolean f = found;
        for (Particle p : particles) {
            if (p instanceof ModelGroupParticle && qName.equals(p.getTargetComponentName())) {
                if (f) {
                    error("Redefined group " +
                            Err.wrap(groupDeclaration.getName()) +
                            " contains more than one reference to itself");
                }
                f = true;
                ModelGroupParticle gref = (ModelGroupParticle) p;
                if (gref.getMinOccurs() != 1) {
                    error("Redefined group " +
                            Err.wrap(groupDeclaration.getName()) +
                            " must have minOccurs=1");
                }
                if (gref.getMaxOccurs() != 1) {
                    error("Redefined group " +
                            Err.wrap(groupDeclaration.getName()) +
                            " must have maxOccurs=1");
                }
                // Change the fingerprint of both the group reference, and the group that
                // it refers to, so that all references other than this one will identify
                // the redefined group

                NamespaceUri uri = getXSDSchemaElement().getTargetNamespace();
                String localName = gref.getTargetComponentName().getLocalPart();
                String newLocalName = localName + '_' + gref.hashCode();
                StructuredQName newNameCode = new StructuredQName("", uri, newLocalName);
                gref.setTargetComponentName(newNameCode);
                originalGroup.setModelGroupName(newNameCode);
                redefinedSchema.addGroup(originalGroup);
            } else if (p instanceof ModelGroupParticle) {
                // Carry on searching recursively
                ModelGroup group;
                try {
                    group = ((ModelGroupParticle) p).getGroup();
                } catch (MissingComponentException e) {
                    // skip this group reference. It's not clear we should be following group references
                    // here anyway; and it's too early to do so because we haven't fixed them all up yet.
                    continue;
                }
                f = patchUpSelfReferences(group.getParticles(), qName, originalGroup, f);
            }
        }
        return f;
    }

}

// Copyright (c) 2004-2023 Saxonica Limited