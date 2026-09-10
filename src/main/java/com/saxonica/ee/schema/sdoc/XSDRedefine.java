package com.saxonica.ee.schema.sdoc;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.SchemaCompiler;
import com.saxonica.ee.schema.SingleNamespaceSchema;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.type.SchemaException;

import javax.xml.transform.Source;
import java.util.HashSet;

/**
 * An xs:redefine element in an XML Schema Document
 */
public class XSDRedefine extends SchemaElement {

    private SingleNamespaceSchema externalSchema;      // the external schema before redefinition of components
    private SingleNamespaceSchema redefinedSchema;     // the external schema after redefinition of components
    private String externalSchemaDocumentURI;


    public XSDRedefine() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws SchemaException if any errors are found in the attributes
     */

    @Override
    protected void prepareAttributes() throws SchemaException {

        AttributeMap atts = attributes();
        String[] allowed = {"id", "schemaLocation"};
        allowAttributes(atts, allowed);

        //-- @id
        processId();

        //-- @schemaLocation
        String schemaLocation = atts.getValue(NamespaceUri.NULL, "schemaLocation");
        if (schemaLocation == null) {
            missingAttribute("schemaLocation");
            throw new SchemaException("Processing abandoned");
        }

        String documentBase = getBaseURI();
        PipelineConfiguration pipe = getSchemaNodeFactory().getPipelineConfiguration();
        SchemaCompiler compiler = getXSDSchema().getSchemaCompiler();
        
        Source source = null;
        try {
            source = SchemaReader.getSource(documentBase, schemaLocation, compiler, null);
        } catch (SchemaException e) {
            handleSchemaException(schemaLocation, e);
        }
        if (source == null) {
            warning("Failed to locate redefined schema document " + schemaLocation);
            externalSchema = new SingleNamespaceSchema((EnterpriseConfiguration) getConfiguration(), NamespaceUri.NULL);
        }

        //-- Have we already imported this XML Schema file? We can't handle
        //   this at present.

        if (source != null) {
            if (compiler.isBeingRead(source.getSystemId())) {
                error("The schema document " + source.getSystemId() + " includes or redefines itself recursively");
                return;
            }
            if (compiler.getExistingSchemaDocument(schemaLocation, getXSDSchema().getTargetNamespace()) != null) {
                error("Saxon can't handle redefinition of a schema document that has already been loaded by a different route");
                return;
            }

            try {
                SchemaDocument schemaDoc = SchemaReader.read(source, compiler, this);
                XSDSchemaElement redefined = schemaDoc.getXSDSchema();
                externalSchema = redefined.getSchema();
                externalSchemaDocumentURI = redefined.getSystemId();
            } catch (SchemaException e) {
                handleSchemaException(schemaLocation, e);
            }
        }
    }

    private void handleSchemaException(String schemaLocation, SchemaException e) throws SchemaException {
        Throwable cause = e.getException();
        if (XSDInclude.isFileNotFoundException(cause)) {
            warning("Failed to locate redefined schema document " + schemaLocation);
            externalSchema = new SingleNamespaceSchema((EnterpriseConfiguration) getConfiguration(), NamespaceUri.NULL);
            // We will get a hard error if any components are actually redefined
        } else {
            // the error has already been reported
            throw e;
        }
    }

    /**
     * Get the referenced schema after redefinition of redefined components
     *
     * @return the post-redefinition schema
     */

    public SingleNamespaceSchema getRedefinedSchema() {
        return redefinedSchema;
    }

    /**
     * Get the referenced schema before redefinition of redefined components
     *
     * @return the pre-redefinition schema
     */

    public SingleNamespaceSchema getExternalSchema() {
        return externalSchema;
    }

    /**
     * Get the document URI of the referenced schema document
     *
     * @return the URI of the pre-redefinition schema
     */

    public String getExternalSchemaDocumentURI() {
        return externalSchemaDocumentURI;
    }


    /**
     * Validate this schema element, checking all document-level constraints
     *
     * @param compiler the schema compiler
     * @throws SchemaException if any XML representation constraints are violated
     */

    @Override
    public void validate(SchemaCompiler compiler) throws SchemaException {
        if (externalSchema == null) {
            return;     // previous error
        }
        HashSet<String> redefinedTypes = new HashSet<>();
        HashSet<String> redefinedAttributeGroups = new HashSet<>();
        HashSet<String> redefinedModelGroups = new HashSet<>();

        for (NodeInfo child : children()) {
            int fp = child.getFingerprint();
            switch (fp) {
                case StandardNames.XS_ANNOTATION: {
                    break;
                }
                case StandardNames.XS_ATTRIBUTE_GROUP: {
                    String attributeGroupName = child.getAttributeValue(NamespaceUri.NULL, "name");
                    if (attributeGroupName != null && !redefinedAttributeGroups.add(attributeGroupName)) {
                        error("Attempting to redefine the same attribute group more than once");
                    }
                    break;
                }
                case StandardNames.XS_COMPLEX_TYPE:
                case StandardNames.XS_SIMPLE_TYPE: {
                    String typeName = child.getAttributeValue(NamespaceUri.NULL, "name");
                    if (typeName != null && !redefinedTypes.add(typeName)) {
                        error("Attempting to redefine the same type more than once");
                    }
                    break;
                }
                case StandardNames.XS_GROUP: {
                    String modelGroupName = child.getAttributeValue(NamespaceUri.NULL, "name");
                    if (modelGroupName != null && !redefinedModelGroups.add(modelGroupName)) {
                        error("Attempting to redefine the same named model group more than once");
                    }
                    break;
                }
                default: {
                    illegalElement(child);
                    break;
                }
            }
        }
        redefinedSchema = new SingleNamespaceSchema(
                externalSchema.getConfiguration(), externalSchema.getTargetNamespace());
        externalSchema.copyTo(redefinedSchema, compiler);
    }

    /**
     * Process child elements after validation
     *
     * @param compiler the schema compiler
     */

    @Override
    public void postValidate(SchemaCompiler compiler) throws SchemaException {
        if (redefinedSchema != null) {
            redefinedSchema.copyTo(getSchema(), compiler);
        }
    }

}

// Copyright (c) 2004-2023 Saxonica Limited

