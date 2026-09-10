package com.saxonica.ee.schema.sdoc;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.PreparedSchema;
import com.saxonica.ee.schema.SchemaCompiler;
import com.saxonica.ee.schema.TypeReference;
import com.saxonica.ee.schema.UserSimpleType;
import net.sf.saxon.om.*;
import net.sf.saxon.type.Derivation;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.SimpleType;

/**
 * An xs:simple-type element in an XML Schema document
 */

public class XSDSimpleType extends SchemaElement {

    /**
     * Flag indicating that this simpleType is a redefinition, that is, the
     * parent of the <simpleType> element is a <redefine> element
     */

    private boolean isRedefinition;

    /**
     * The SimpleTypeDefinition we are constructing
     */
    private SimpleTypeDefinition simpleTypeDef = null;

    boolean foundRestriction = false;

    /**
     * Creates a new SimpleType element
     */
    public XSDSimpleType() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws SchemaException if any errors are found in the attributes
     */

    @Override
    protected void prepareAttributes() throws SchemaException {

        PreparedSchema schema = getSchema();
        isRedefinition = getParent() instanceof XSDRedefine;

        AttributeMap atts = attributes();
        if (isTopLevel()) {
            String[] allowed = {"final", "id", "name"};
            allowAttributes(atts, allowed);
            requireAttribute(atts, "name");
        } else {
            String[] allowed = {"id"};
            allowAttributes(atts, allowed);
        }

        String name = atts.getValue("", "name");

        simpleTypeDef = new SimpleTypeDefinition((EnterpriseConfiguration) getConfiguration());
        simpleTypeDef.setLocator(this);
        simpleTypeDef.setSchemaDocumentURI(getParent().getSystemId());

        //-- @id
        processId();

        //-- get the name of the type, and register the fingerprint
        if (name != null) {
            StructuredQName typeName = null;
            try {
                typeName = getComponentName(name, REQUIRE_TARGET_NS);
            } catch (SchemaException err) {
                error(err.getMessage());
                typeName = StandardNames.SQ_XS_INVALID_NAME;
            }
            int fp = getNamePool().allocateFingerprint(typeName.getURI(), typeName.getLocalPart());
            simpleTypeDef.setTypeName(typeName, fp);
            if (!isRedefinition) {
                if (schema.getSchemaType(simpleTypeDef.getTypeName()) != null && fp > 1023) {
                    // don't report duplicates for built-in types: this might be the schema for schemas
                    duplicate("type", name);
                }
            }
        } else {
            // Anonymous type: give it a system-allocated name
            NodeName nn = allocateAnonymousTypeCode();
            simpleTypeDef.setTypeName(nn.getStructuredQName(), nn.getFingerprint());
            schema.addType(simpleTypeDef);
        }

        //-- @final
        String finalVal = atts.getValue("", "final");
        if (finalVal == null) {
            simpleTypeDef.setFinalProhibitions(getXSDSchema().getFinalDefault());
        } else {
            int f = parseFinalOrBlock("final",
                    finalVal,
                    Derivation.DERIVATION_LIST |
                                              Derivation.DERIVATION_UNION |
                                              Derivation.DERIVATION_RESTRICTION |
                                              Derivation.DERIVATION_EXTENSION);
            // The inclusion of EXTENSION here assumes the resolution of specification bug 2074
            simpleTypeDef.setFinalProhibitions(f);
        }

    }

    /**
     * Returns the SimpleType created
     */

    /*@Nullable*/
    public SimpleTypeDefinition getSimpleTypeDefinition() throws SchemaException {
        return simpleTypeDef;
    }

    /**
     * Validate this schema element, checking all document-level constraints
     *
     * @param compiler the schema compiler
     * @throws SchemaException if any XML representation constraints are violated
     */

    @Override
    public void validate(SchemaCompiler compiler) throws SchemaException {

        checkContentModel(stateTerms, stateTargets, false);

        for (NodeInfo child : children()) {
            switch (child.getFingerprint()) {
                case StandardNames.XS_RESTRICTION:
                    foundRestriction = true;
                    simpleTypeDef.setDerivationMethod(Derivation.DERIVATION_RESTRICTION);
                    break;

                case StandardNames.XS_LIST:
                    simpleTypeDef.setDerivationMethod(Derivation.DERIVATION_LIST);
                    break;

                case StandardNames.XS_UNION:
                    simpleTypeDef.setDerivationMethod(Derivation.DERIVATION_UNION);
                    break;
            }
        }
    }

    private final static int[] state0terms =
            {StandardNames.XS_LIST, StandardNames.XS_UNION, StandardNames.XS_RESTRICTION,
                    StandardNames.XS_ANNOTATION};
    private final static int[] state0targets = {1, 1, 1, 2};

    private final static int[] state1terms = {};
    private final static int[] state1targets = {};

    private final static int[] state2terms =
            {StandardNames.XS_LIST, StandardNames.XS_UNION, StandardNames.XS_RESTRICTION};
    private final static int[] state2targets = {1, 1, 1};

    private final static int[][] stateTerms = {
            state0terms, state1terms, state2terms};
    private final static int[][] stateTargets = {
            state0targets, state1targets, state2targets};

    @Override
    protected boolean isFinalState(int state) {
        return state == 1;
    }

    /**
     * Hook to allow additional validation of a parent element immediately after its
     * children have been validated.
     *
     * @param compiler the schema compiler
     */

    @Override
    public void postValidate(SchemaCompiler compiler) throws SchemaException {
        if (isRedefinition) {
            PreparedSchema externalSchema = ((XSDRedefine) getParent()).getExternalSchema();
            PreparedSchema redefinedSchema = ((XSDRedefine) getParent()).getRedefinedSchema();
            if (externalSchema == null || redefinedSchema == null) {
                return; // assume error already reported
            }
            if (!foundRestriction) {
                error("A redefined simple type must be a restriction");
                return;
            }
            if (simpleTypeDef.getBaseTypeFingerprint() != simpleTypeDef.getFingerprint()) {
                error("A redefined simple type must be based on the type with the same name");
                return;
            }
            SimpleType oldSType = externalSchema.getSimpleType(simpleTypeDef.getTypeName());
            if (oldSType == null) {
                error("Cannot redefine the simple type " + simpleTypeDef.getDescription() +
                        " because it is not present in the redefined schema");
                return;
            }
            if (!(oldSType instanceof UserSimpleType)) {
                error("Cannot redefine a built-in simple type (" + simpleTypeDef.getDescription() + ')');
                return;
            }
            if (!((XSDRedefine) getParent()).getExternalSchemaDocumentURI().equals(((UserSimpleType) oldSType).getSchemaDocumentURI())) {
                // Test added in Saxon 9.3
                compiler.warning("The redefined simple type was found, but not in the schema document referenced by " +
                        "the schemaLocation attribute of the containing <xs:redefine> element. This is not allowed by the XSD " +
                        "specification. However, Saxon does not currently enforce this rule.", this);
            }
            simpleTypeDef.setRedefinitionLevel(oldSType.getRedefinitionLevel() + 1);
            UserSimpleType oldType = (UserSimpleType) oldSType;
            // rename the type being redefined
            String uri = getXSDSchema().getTargetNamespace();
            NamePool pool = getNamePool();
            String localName = simpleTypeDef.getName();
            String prefix = simpleTypeDef.getTypeName().getPrefix();
            String newLocalName = localName + '_' + simpleTypeDef.hashCode();
            int newFingerprint = pool.allocateFingerprint(uri, newLocalName);
            oldType.setTypeName(new StructuredQName(prefix, uri, newLocalName), newFingerprint);
            TypeReference ref = new TypeReference(newFingerprint, simpleTypeDef.getConfiguration(), this);
            simpleTypeDef.setBaseTypeReference(ref);
            redefinedSchema.addType(oldType);
            redefinedSchema.addType(simpleTypeDef);
        } else {
            int fp = simpleTypeDef.getFingerprint();
            if (fp != -1 && fp == simpleTypeDef.getBaseTypeFingerprint()) {
                error("A simple type must not be based on itself");
                //return;
            }
            if (getParent() instanceof XSDList || getParent() instanceof XSDUnion) {
                simpleTypeDef.setContextComponent(((XSDSimpleType) getParent().getParent()).getSimpleTypeDefinition());
            }
        }
    }

}

// Copyright (c) 2004-2022 Saxonica Limited
