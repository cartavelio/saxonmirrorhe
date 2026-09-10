package com.saxonica.ee.schema.sdoc;

//-- imported classes and packages

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.*;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.type.SchemaException;

import java.util.List;

/**
 * An xs:attributeGroup element in an XML Schema document
 */

public class XSDAttributeGroup extends SchemaElement {

    /**
     * Flag indicating that this attribute group is a redefinition, that is,
     * it is contained in a <redefine> element
     */

    private boolean isRedefinition;

    /**
     * The AttributeGroup declaration we are constructing, if this is a named attribute group
     */
    private AttributeGroupDecl attributeGroupDecl = null;

    /**
     * The AttributeGroup declaration we are constructing, if this is an attribute group reference
     */
    private AttributeGroupReference attributeGroupReference = null;


    private boolean isRef = false;

    /**
     * Creates a new AttributeGroup element
     */

    public XSDAttributeGroup() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws SchemaException if any errors are found in the attributes
     */

    @Override
    protected void prepareAttributes() throws SchemaException {

        isRedefinition = (getParent() instanceof XSDRedefine);

        AttributeMap atts = attributes();
        if (isTopLevel()) {
            String[] allowed = {"id", "name"};
            allowAttributes(atts, allowed);
            requireAttribute(atts, "name");

            String qname = atts.getValue(NamespaceUri.NULL, "name");
            if (qname == null) {
                // error already reported
                return;
            }
            StructuredQName nc;
            try {
                nc = getComponentName(qname, REQUIRE_TARGET_NS);
            } catch (SchemaException err) {
                error(err.getMessage());
                return;
            }
            attributeGroupDecl = new AttributeGroupDecl((EnterpriseConfiguration) getConfiguration());
            attributeGroupDecl.setLocator(this);
            attributeGroupDecl.setAttributeGroupName(nc);
            attributeGroupDecl.setSchemaDocumentURI(getParent().getSystemId());
        } else {
            String[] allowed = {"id", "ref"};
            allowAttributes(atts, allowed);
            requireAttribute(atts, "ref");
            String ref = atts.getValue(NamespaceUri.NULL, "ref");
            if (ref == null) {
                return;
            }
            StructuredQName fp;
            try {
                fp = getComponentName(ref, USE_DEFAULT_NS);
            } catch (SchemaException err) {
                error(err.getMessage());
                return;
            }

            isRef = true;
            attributeGroupReference = new AttributeGroupReference((EnterpriseConfiguration) getConfiguration(), fp);
            attributeGroupReference.setLocator(this);
        }


        //-- @id
        processId();
    }

    /**
     * Is this an attribute group declaration or a reference?
     *
     * @return true if it is a reference
     */

    public boolean isReference() {
        return isRef;
    }

    /**
     * Returns the AttributeGroupDecl created by this AttributeGroup declaration.
     *
     * @return the AttributeGroupDecl corresponding to this xs:attribute-group element if it is
     *         a global attribute group declaration; if it is an attribute group reference, return null.
     */
    public AttributeGroupDecl getAttributeGroupDecl() {
        return attributeGroupDecl;
    }

    /**
     * Return the AttributeGroupReference, if this is a reference
     *
     * @return the AttributeGroupReference, or null if this is not a reference
     */

    /*@Nullable*/
    public AttributeGroupReference getAttributeGroupReference() {
        return attributeGroupReference;
    }

    /**
     * Validate this schema element, checking all document-level constraints
     *
     * @param compiler the schema compiler
     * @throws SchemaException if any XML representation constraints are violated
     */

    @Override
    public void validate(SchemaCompiler compiler) throws SchemaException {
        if (isRef) {
            checkOnlyChildIsAnnotation();
        } else {
            checkContentModel(stateTerms, stateTargets, false);
        }
    }

    // Following FSM generated using:
    // Transform s4s11.scm generate-s4s-code.xsl type=namedAttributeGroup

    private final static int[] state0terms =
            {StandardNames.XS_ANY_ATTRIBUTE, StandardNames.XS_ATTRIBUTE, StandardNames.XS_ATTRIBUTE_GROUP,
                    StandardNames.XS_ANNOTATION};
    private final static int[] state0targets = {1, 2, 2, 3};

    private final static int[] state1terms = {};
    private final static int[] state1targets = {};

    private final static int[] state2terms =
            {StandardNames.XS_ANY_ATTRIBUTE, StandardNames.XS_ATTRIBUTE, StandardNames.XS_ATTRIBUTE_GROUP};
    private final static int[] state2targets = {1, 2, 2};

    private final static int[] state3terms =
            {StandardNames.XS_ANY_ATTRIBUTE, StandardNames.XS_ATTRIBUTE, StandardNames.XS_ATTRIBUTE_GROUP};
    private final static int[] state3targets = {1, 2, 2};

    private final static int[][] stateTerms = {
            state0terms, state1terms, state2terms, state3terms};
    private final static int[][] stateTargets = {
            state0targets, state1targets, state2targets, state3targets};

    @Override
    protected boolean isFinalState(int state) {
        return true;
    }

    /**
     * Additional processing after all the children of this element have been validated.
     *
     * @param compiler the schema compiler
     */

    @Override
    public void postValidate(SchemaCompiler compiler) throws SchemaException {

        for (NodeInfo child : children()) {
            String name = child.getLocalPart();

            //-- <anyAttribute>
            if ("anyAttribute".equals(name)) {
                AttributeWildcard wildcard = ((XSDAnyAttribute) child).getWildcard();
                if (attributeGroupDecl != null) {
                    attributeGroupDecl.setAnyAttribute(wildcard);
                }
            }

            //-- attribute declarations
            else if ("attribute".equals(name)) {
                AttributeUse attributeUse = ((XSDAttribute) child).getAttributeUse();
                if (attributeGroupDecl != null && attributeUse != null) {
                    attributeUse.setContainingAttributeGroupDecl(attributeGroupDecl);
                    if (attributeUse.isProhibited()) {
                        compiler.warning("Specifying use='prohibited' within an attribute group has no effect", SaxonErrorCode.SXSD1015, this);
                        // See Microsoft tests, attZ015, and test suite bug 4043
                    } else {
                        attributeGroupDecl.addAttributeUse(attributeUse);
                    }
                }
            }
            //-- attribute group references
            else if ("attributeGroup".equals(name)) {
                AttributeGroupReference obj = ((XSDAttributeGroup) child).getAttributeGroupReference();
                if (attributeGroupDecl != null) {
                    attributeGroupDecl.addAttributeGroupReference(obj);
                }
            }

            //-- annotation
            else if ("annotation".equals(name)) {

            }

        }
        if (isRedefinition) {
            SingleNamespaceSchema externalSchema = ((XSDRedefine) getParent()).getExternalSchema();
            SingleNamespaceSchema redefinedSchema = ((XSDRedefine) getParent()).getRedefinedSchema();
            if (externalSchema == null || redefinedSchema == null) {
                return;     // error already reported
            }
            EnterpriseConfiguration config = getSchema().getConfiguration();
            if (config.isSealedNamespace(redefinedSchema.getTargetNamespace())) {
                error("It is not possible to redefine schema components in namespace " +
                        redefinedSchema.getTargetNamespace() +
                        " because components in this namespace have already been used " +
                        " for validating instance documents, or for compiling queries or stylesheets");
                return;
            }
            if (isRef) {
                error("An attribute group redefinition must not be a reference");
            }
            AttributeGroupDecl found = patchUpSelfReferences(attributeGroupDecl.getLocalAttributeGroupReferences(),
                    attributeGroupDecl.getAttributeGroupName(),
                    null, compiler);
            if (found != null) {
                // If a self-reference was found, then no further checking is needed
                attributeGroupDecl.setRedefinitionLevel(found.getRedefinitionLevel() + 1);
                checkImmediateRedefinition(found, compiler);
            } else {
                // If no self-reference was found, then the name of the attribute group
                // must be the same as the name of an attribute group in the redefined schema,
                // and this definition must be a valid restriction of the original definition.
                // The latter condition is checked during validation.
                AttributeGroupDecl d = externalSchema.getAttributeGroup(attributeGroupDecl.getAttributeGroupName());
                if (d == null) {
                    error("Attribute group " + Err.wrap(attributeGroupDecl.getDisplayName()) +
                            " is not defined in the schema being redefined");
                    return;
                }
                attributeGroupDecl.setRedefinedAttributeGroupDecl(d);
                attributeGroupDecl.setRedefinitionLevel(d.getRedefinitionLevel() + 1);
            }
            redefinedSchema.addAttributeGroup(attributeGroupDecl);
        }
    }

    private AttributeGroupDecl patchUpSelfReferences(
            List<AttributeGroupReference> references, StructuredQName name, AttributeGroupDecl found, SchemaCompiler compiler)
            throws SchemaException {
        PreparedSchema containingSchema = getSchema();
        PreparedSchema redefinedSchema = ((XSDRedefine) getParent()).getRedefinedSchema();
        PreparedSchema externalSchema = ((XSDRedefine) getParent()).getExternalSchema();
        AttributeGroupDecl f = found;
        for (AttributeGroupReference ref : references) {
            if (ref.getTargetComponentName().equals(name)) {
                if (f != null) {
                    error("Redefined attribute group " +
                            Err.wrap(name.getDisplayName()) +
                            " contains more than one reference to itself");
                }

                // Change the fingerprint of both the attribute group reference, and the
                // attribute group that it refers to, so that all references
                // other than this one will identify the redefined group

                NamespaceUri uri = getXSDSchema().getTargetNamespace();
                String localName = ref.getTargetComponentName().getLocalPart();
                String newLocalName = localName + '_' + ref.hashCode();
                StructuredQName newNameCode = new StructuredQName("", uri, newLocalName);
                f = externalSchema.getAttributeGroup(name);
                if (f == null) {
                    error("Attribute group " +
                            Err.wrap(name.getDisplayName()) +
                            " is not defined in the schema being redefined");
                    return null;
                }
                checkImmediateRedefinition(f, compiler);
                ref.setTargetComponentName(newNameCode);
                f.setAttributeGroupName(newNameCode);
                redefinedSchema.addAttributeGroup(f);

                // Carry on searching recursively
                f = patchUpSelfReferences(f.getLocalAttributeGroupReferences(), name, f, compiler);
            }
        }
        return f;
    }

    /**
     * Check that the redefined attribute group was defined immediately in the schema document being redefined.
     * Saxon (since 9.3) reports a warning if this condition is violated, although it is definitely an error
     * according to the specification (Schema Representation Constraint, Individual Component Redefinition).
     * Releases prior to 9.3 reported neither an error or a warning.
     *
     * @param f        the attribute group being redefined
     * @param compiler used for error reporting
     */

    private void checkImmediateRedefinition(AttributeGroupDecl f, SchemaCompiler compiler) {
        if (!((XSDRedefine) getParent()).getExternalSchemaDocumentURI().equals(f.getSchemaDocumentURI())) {
            // Test added in Saxon 9.3
            compiler.warning("The redefined attribute group was found, but not in the schema document referenced by " +
                    "the schemaLocation attribute of the containing <xs:redefine> element. This is not allowed by the XSD " +
                    "specification. However, Saxon does not currently enforce this rule.", SaxonErrorCode.SXSD1016, this);
        }
    }


}

// Copyright (c) 2011-2022 Saxonica Limited