package com.saxonica.ee.schema.sdoc;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.PreparedSchema;
import com.saxonica.ee.schema.SchemaCompiler;
import com.saxonica.ee.schema.SingleNamespaceSchema;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.type.SchemaException;

import javax.xml.transform.Source;

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

        String schemaLocation = atts.getValue("", "schemaLocation");
        if (schemaLocation == null) {
            missingAttribute("schemaLocation");
            return;
        }

        SchemaCompiler manager = getXSDSchema().getSchemaCompiler();

        Source source = null;
        try {
            source = SchemaReader.getSource(getBaseURI(), schemaLocation, manager.getPipelineConfiguration(), null);
        } catch (SchemaException e) {
            handleSchemaException(schemaLocation, e);
        }
        if (source != null) {
            XSDSchema existing = manager.getExistingSchemaDocument(source.getSystemId(), getXSDSchema().getTargetNamespace());
            if (existing != null) {
                includedSchema = existing.getSchema();
                return;
            }

            //-- Has this schema location been processed?

            if (manager.isBeingRead(source.getSystemId())) {
                warning("Module " + source.getSystemId() + " includes itself recursively");
                return;
            }

            try {
                PipelineConfiguration pipe = getSchemaNodeFactory().getPipelineConfiguration();
                SchemaCompiler compiler = getSchemaNodeFactory().getSchemaCompiler();
                SchemaDocument inc = SchemaReader.read(source, compiler, pipe, this);
                XSDSchema includedXSDSchema = inc.getXSDSchema();
                includedXSDSchema.setReferrer(this);
                includedSchema = includedXSDSchema.getSchema();
                manager.markAsRead(source.getSystemId(), includedXSDSchema);
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
            includedSchema = new SingleNamespaceSchema((EnterpriseConfiguration) getConfiguration(), "");
        } else {
            error("Failed to process included schema document " + schemaLocation);
            throw e;
        }

    }

    @CSharpReplaceBody(code="return cause.InnerException is System.IO.FileNotFoundException || cause.InnerException is System.Net.Http.HttpRequestException;")
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

// Copyright (c) 2011-2020 Saxonica Limited
