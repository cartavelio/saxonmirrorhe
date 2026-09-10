package com.saxonica.ee.schema.sdoc;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.PreparedSchema;
import com.saxonica.ee.schema.SchemaCompiler;
import com.saxonica.ee.schema.SingleNamespaceSchema;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.functions.ResolveURI;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.lib.SchemaURIResolver;
import net.sf.saxon.lib.StandardEntityResolver;
import net.sf.saxon.om.AttributeCollection;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaException;
import org.xml.sax.InputSource;
import org.xml.sax.SAXException;

import javax.xml.transform.Source;
import javax.xml.transform.sax.SAXSource;
import java.io.IOException;
import java.net.URISyntaxException;

/**
 * An xs:import element in an XML Schema document
 */

public class XSDImport extends AnnotationParent {

    private PreparedSchema importedSchema;

    public XSDImport() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws SchemaException if any errors are found in the attributes
     */

    public void prepareAttributes() throws SchemaException {

        AttributeCollection atts = getAttributeList();
        String[] allowed = {"id", "namespace", "schemaLocation"};
        allowAttributes(atts, allowed);

        //-- @id
        processId();

        //-- Get schemaLocation
        String schemaLocation = atts.getValue("", "schemaLocation");
        //-- Get namespace
        String namespace = atts.getValue("", "namespace");

        if ((schemaLocation == null) && (namespace == null)) {
            //-- A legal <import/> element...just return
            return;
        }

        XSDSchema thisSchemaDocument = getXSDSchema();
        //DocumentInfo thisRoot = thisSchemaDocument.getDocumentRoot();
        EnterpriseConfiguration config = (EnterpriseConfiguration) getConfiguration();

        SchemaCompiler manager = thisSchemaDocument.getSchemaCompiler();

        if (namespace == null) {
            namespace = "";
        }

        String documentBase = getBaseURI();

        Source source = null;
        if (schemaLocation != null) {
            try {
                source = SchemaReader.getSource(documentBase,
                        schemaLocation,
                        manager.getPipelineConfiguration(),
                        namespace);
            } catch (SchemaException e) {
                error("xs:import failed: " + e.getMessage());
                return;
            }
        }

        if (namespace.equals(thisSchemaDocument.getTargetNamespace())) {
            error("The namespace attribute in the xs:import element cannot be the same as the targetNamespace of the containing schema");
            return;
        }

        if (source != null) {
            // Are we currently reading this schema document?
            if (manager.isBeingRead(source.getSystemId())) {
                return;
            }

            // Have we already completed reading this document?

            XSDSchema existing = manager.getExistingSchemaDocument(source.getSystemId(), namespace);
            if (existing != null) {
                importedSchema = existing.getSchema();
                return;
            }
        } else {

            // Have we already completed reading this document?
            // See bug 2210 problem A. If the target namespace is known, then SchemaReader.getSource() returns null,
            // therefore the test on source.getSystemId() doesn't work, and we go on to issue a warning.

            try {
                String location = ResolveURI.makeAbsolute(schemaLocation, documentBase).toString();
                XSDSchema existing = manager.getExistingSchemaDocument(location, namespace);
                if (existing != null) {
                    importedSchema = existing.getSchema();
                    return;
                }
            } catch (URISyntaxException e) {
                // no action
            }
        }

        //-- Is another document for the same schema namespace already loaded?
        //PreparedSchema existingSchema = config.getSchema(namespace);
        int status = config.getNamespaceStatus(namespace);
        if (status == PreparedSchema.NAMESPACE_UNDER_CONSTRUCTION) {
            // This implies a cycle of imports. Alternatively, it means that a concurrent
            // process is loading a schema with this namespace...
            if (source == null
                    || (!manager.isBeingRead(source.getSystemId())
                        && !config.getBooleanProperty(Feature.MULTIPLE_SCHEMA_IMPORTS))) {
                if (schemaLocation != null) {
                    String message = "The schema document at " + schemaLocation +
                            " is not being read because schema components for this namespace are already being processed";
                    if (!namespace.equals(NamespaceConstant.XML)) {
                        message += ". To force the schema document to be read, set --multipleSchemaImports:on";
                    }
                    warning(message);
                }
                return;
            }
        } else if (status == PreparedSchema.NAMESPACE_SEALED) {
            return;
        } else if (status == PreparedSchema.NAMESPACE_KNOWN &&
                !config.getBooleanProperty(Feature.MULTIPLE_SCHEMA_IMPORTS)) {
            if (schemaLocation != null) {
                String message = "The schema document at " + schemaLocation +
                        " is not being read because schema components for this namespace are already available";
                if (!namespace.equals(NamespaceConstant.XML)) {
                    message += ". To force the schema document to be read, set --multipleSchemaImports:on";
                }
                warning(message);
            }
            return;
        }

        // For the XML namespace, use the built-in version if the schema location is known or if already loaded

        if (namespace.equals(NamespaceConstant.XML)) {
            if (schemaLocation == null ||
                    schemaLocation.equals("http://www.w3.org/2001/xml.xsd") ||
                    schemaLocation.equals("http://www.w3.org/2001/03/xml.xsd")) {
                // Use the built-in version, or none if already loaded
                if (status != PreparedSchema.NAMESPACE_KNOWN) {
                    importedSchema = XMLNamespaceSchema.buildSchema(config);
                    config.addSchema(importedSchema);
                }
                return;
            }
        }

        //-- if schemaLocation not provided, give up, unless it's the XSI or FN Namespace
        if (schemaLocation == null) {
            if (namespace.equals(NamespaceConstant.SCHEMA_INSTANCE)) {
                importedSchema = XSINamespaceSchema.buildSchema(config);
                config.addSchema(importedSchema);
                return;
            } else if (namespace.equals(NamespaceConstant.FN)) {
                if (!config.isSchemaAvailable(NamespaceConstant.FN)) {
                    try {
                        InputSource is = StandardEntityResolver.getInstance().resolveEntity(null, "http://www.w3.org/2005/xpath-functions.xsd");
                        if (config.isTiming()) {
                            config.getLogger().info("Loading schema from resources for: " + NamespaceConstant.FN);
                        }
                        source = new SAXSource(is);
                    } catch (SAXException e) {
                        throw new SchemaException(e);
                    } catch (IOException e) {
                        throw new SchemaException(e);
                    }
                }
            } else {
                PipelineConfiguration pipe = getSchemaNodeFactory().getPipelineConfiguration();
                SchemaURIResolver resolver = pipe.getSchemaURIResolver();
                if (resolver != null) {
                    try {
                        Source[] sources = resolver.resolve(namespace, getBaseURI(), new String[0]);
                        if (sources == null || sources.length == 0) {
                            warning("Cannot locate a schema document for namespace " + namespace);
                            // Error recovery
                            importedSchema = new SingleNamespaceSchema((EnterpriseConfiguration) getConfiguration(), namespace);
                            return;
                        } else if (sources.length > 1) {
                            error("SchemaURIResolver returned more than one schema document");
                        } else {
                            source = sources[0];
                        }
                    } catch (XPathException err) {
                        throw new SchemaException(err);
                    }
                }

            }
        }

        SchemaDocument importee;
        int errors;
        try {
            PipelineConfiguration pipe = getSchemaNodeFactory().getPipelineConfiguration();
            SchemaCompiler compiler = getSchemaNodeFactory().getSchemaCompiler();
            importee = SchemaReader.read(source, compiler, pipe, null);
            XSDSchema s = importee.getXSDSchema();
            errors = importee.getXSDSchema().getErrorCount();
            importedSchema = s.getSchema();
        } catch (SchemaException e) {
            Throwable cause = e.getException();
            if (cause instanceof java.io.IOException) {
                warning("Imported schema document " + schemaLocation + " cannot be located: " +
                        cause.getClass().getName() + " (" + cause.getMessage() + ")");
            } else {
                error("Failed to process imported schema document: " + e.getMessage());
            }
            // create a dummy imported schema for error recovery
            importedSchema = new SingleNamespaceSchema((EnterpriseConfiguration) getConfiguration(), namespace);
            errors = 0;
            return;
        }

        //int errors = (importee==null ? 1 : importee.getXSDSchema().getErrorCount());

        boolean valid = (errors == 0);

        // Don't validate the imported schema yet, because it may have references to components
        // not yet available. But do remove the surrogate schema that was registered at the start
        // of the import, or subsequent operations may fail

        config.removeSchema(namespace);

        if (!valid) {
            throw new SchemaException("Imported schema is invalid");
        }

        // check again that the target namespace differs (the check was only done
        // before if a "namespace" attribute was present

        String importedNamespace = importee.getTargetNamespace();
        if (thisSchemaDocument.getTargetNamespace().equals(importedNamespace)) {
            error("The targetNamespace of the imported schema must not be the same as " +
                    "the targetNamespace of the importing schema");
            return;
        }

        if (!namespace.equals(importedNamespace)) {
            error("The namespace attribute in xs:import differs from the targetNamespace of the imported schema");
            return;
        }

        //-- Add schema to list of imported schemas (if not already present)
        //importedSchema.setSchemaLocation(schemaLocation);
        config.addSchema(importedSchema);
    }

    /**
     * Get the imported schema. Returns null if the imported schema was found to be invalid or if
     * no import took place because the namespace was already present in the schema cache
     *
     * @return the imported schema
     */

    public PreparedSchema getImportedSchema() {
        return importedSchema;
    }

    /**
     * Get the target namespace. Return "" for the no-namespace
     *
     * @return the target namespace, using "" to represent the no-namespace
     */

    public String getTargetNamespace() {
        String uri = getAttributeList().getValue("", "namespace");
        if (uri == null) {
            return "";
        } else {
            return uri;
        }
    }

}

// Copyright (c) 2004-6 Saxonica Limited. All rights reserved