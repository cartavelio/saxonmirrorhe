package com.saxonica.ee.schema.sdoc;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.PreparedSchema;
import com.saxonica.ee.schema.SchemaCompiler;
import com.saxonica.ee.schema.SingleNamespaceSchema;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.functions.ResolveURI;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.type.SchemaException;

import javax.xml.transform.Source;
import java.net.URISyntaxException;

/**
 * An xs:include element in an XML Schema document
 */

public class XSDInclude extends AnnotationParent {

    private PreparedSchema includedSchema;

    /**
     * Create an XSDInclude object
     */

    public XSDInclude() {
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

        String schemaLocation = atts.getValue(NamespaceUri.NULL, "schemaLocation");
        if (schemaLocation == null) {
            missingAttribute("schemaLocation");
            return;
        }

        SchemaCompiler compiler = getXSDSchema().getSchemaCompiler();
        String documentBase = getBaseURI();
        try {
            String location = ResolveURI.makeAbsolute(schemaLocation, documentBase).toString();
            XSDSchemaElement existing = compiler.getExistingSchemaDocument(location, getXSDSchema().getTargetNamespace());
            if (existing != null) {
                includedSchema = existing.getSchema();
                return;
            }
        } catch (URISyntaxException e) {
            // no action
        }


        Source source = null;
        try {
            source = SchemaReader.getSource(getBaseURI(), schemaLocation, compiler, null);
        } catch (SchemaException e) {
            handleSchemaException(schemaLocation, e);
        }
        if (source == null) {
            warning("Failed to locate included schema document " + schemaLocation);
            includedSchema = new SingleNamespaceSchema((EnterpriseConfiguration) getConfiguration(), NamespaceUri.NULL);
        } else {

            //-- Has this schema location been processed?

            if (compiler.isBeingRead(source.getSystemId())) {
                warning("Module " + source.getSystemId() + " includes itself recursively");
                return;
            }

            try {
                PipelineConfiguration pipe = getSchemaNodeFactory().getPipelineConfiguration();
                SchemaDocument inc = SchemaReader.read(source, compiler, pipe, this);
                XSDSchemaElement includedXSDSchema = inc.getXSDSchema();
                includedXSDSchema.setReferrer(this);
                includedSchema = includedXSDSchema.getSchema();
                compiler.markAsRead(source.getSystemId(), includedXSDSchema);
            } catch (SchemaException e) {
                handleSchemaException(schemaLocation, e);
            }
        }

    }

    private void handleSchemaException(String schemaLocation, SchemaException e) throws SchemaException {
        Throwable cause = e.getException();
        if (isFileNotFoundException(cause)) {
            // It is not an error if the schemaLocation cannot be resolved; this is equivalent
            // to including an empty schema document. But we give a warning, because otherwise the
            // subsequent errors are likely to be obscure
            warning("Included schema document " + schemaLocation + " cannot be located: " +
                            cause.getClass().getName() + " (" + cause.getMessage() + ")");
            includedSchema = new SingleNamespaceSchema((EnterpriseConfiguration) getConfiguration(), NamespaceUri.NULL);
        } else {
            error("Failed to process included schema document " + schemaLocation);
            throw e;
        }

    }

    @CSharpReplaceBody(code="return cause.Message.StartsWith(\"Unable to retrieve\");")
    public static boolean isFileNotFoundException(Throwable cause) {
        return cause instanceof java.io.IOException;
    }

    /**
     * Get the schema (= collection of schema components) that was included by this
     * xs:include instruction
     *
     * @return the components that were included
     */

    public PreparedSchema getIncludedSchema() {
        return includedSchema;
    }

}

// Copyright (c) 2011-2023 Saxonica Limited
