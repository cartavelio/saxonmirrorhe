package com.saxonica.ee.schema.sdoc;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.PreparedSchema;
import com.saxonica.ee.schema.SchemaCompiler;
import com.saxonica.ee.schema.SingleNamespaceSchema;
import net.sf.saxon.Version;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.functions.ResolveURI;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.lib.SchemaURIResolver;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaException;

import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import java.io.InputStream;
import java.net.URISyntaxException;
import java.util.ArrayList;
import java.util.List;

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

    @Override
    protected void prepareAttributes() throws SchemaException {

        AttributeMap atts = attributes();
        String[] allowed = {"id", "namespace", "schemaLocation"};
        allowAttributes(atts, allowed);

        //-- @id
        processId();

        //-- Get schemaLocation
        String schemaLocation = atts.getValue(NamespaceUri.NULL, "schemaLocation");
        //-- Get namespace
        String namespace = atts.getValue(NamespaceUri.NULL, "namespace");

        if ((schemaLocation == null) && (namespace == null)) {
            //-- A legal <import/> element...just return
            return;
        }

        NamespaceUri targetNamespace = NamespaceUri.of(namespace);

        XSDSchema thisSchemaDocument = getXSDSchema();
        //DocumentInfo thisRoot = thisSchemaDocument.getDocumentRoot();
        EnterpriseConfiguration config = (EnterpriseConfiguration) getConfiguration();

        SchemaCompiler compiler = thisSchemaDocument.getSchemaCompiler();

        String documentBase = getBaseURI();

        // Have we already completed reading this document?

        if (schemaLocation != null) {
            try {
                String location = ResolveURI.makeAbsolute(schemaLocation, documentBase).toString();
                XSDSchema existing = compiler.getExistingSchemaDocument(location, targetNamespace);
                if (existing != null) {
                    importedSchema = existing.getSchema();
                    return;
                }
            } catch (URISyntaxException e) {
                // no action
            }
        }

        // For the XML namespace, use the built-in version if the schema location is known or if already loaded

        if (targetNamespace.equals(NamespaceUri.XML)) {
            if (schemaLocation == null ||
                    schemaLocation.equals("http://www.w3.org/2001/xml.xsd") ||
                    schemaLocation.equals("http://www.w3.org/2001/03/xml.xsd")) {
                // Use the built-in version, or none if already loaded
                if (config.getNamespaceStatus(targetNamespace) != PreparedSchema.NAMESPACE_KNOWN) {
                    importedSchema = XMLNamespaceSchema.buildSchema(config);
                    config.addSchema(importedSchema);
                }
                return;
            }
        }

        if (targetNamespace.equals(thisSchemaDocument.getTargetNamespace())) {
            error("The namespace attribute in the xs:import element cannot be the same as the targetNamespace of the containing schema");
            return;
        }

        Source source = null;
        if (schemaLocation != null) {
            try {
                source = SchemaReader.getSource(documentBase,
                        schemaLocation,
                        compiler,
                        targetNamespace);
            } catch (SchemaException e) {
                processSchemaException(schemaLocation, namespace, e);
                return;
            }
        }

        if (source != null) {
            // Are we currently reading this schema document?
            if (compiler.isBeingRead(source.getSystemId())) {
                return;
            }

        }

        // Is another document for the same schema namespace already loaded?
        int status = config.getNamespaceStatus(targetNamespace);
        if (status == PreparedSchema.NAMESPACE_UNDER_CONSTRUCTION) {
            // This implies a cycle of imports. Alternatively, it means that a concurrent
            // process is loading a schema with this namespace...
            if (source == null
                    || (!compiler.isBeingRead(source.getSystemId())
                                && !config.getBooleanProperty(Feature.MULTIPLE_SCHEMA_IMPORTS))) {
                if (schemaLocation != null) {
                    String message = "The schema document at " + schemaLocation +
                            " is not being read because schema components for this namespace are already being processed";
                    if (!NamespaceConstant.XML.equals(namespace)) {
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

        if (source == null && schemaLocation != null) {
            // This path is used on SaxonCS
            warning("Failed to locate imported schema document: " + schemaLocation);
            // create a dummy imported schema for error recovery
            importedSchema = new SingleNamespaceSchema((EnterpriseConfiguration) getConfiguration(), targetNamespace);
            return;
        }

        // If schemaLocation not provided, give up, unless it's the XSI or FN Namespace
        if (schemaLocation == null) {
            if (namespace.equals(NamespaceConstant.SCHEMA_INSTANCE)) {
                importedSchema = XSINamespaceSchema.buildSchema(config);
                config.addSchema(importedSchema);
                return;
            } else if (targetNamespace.equals(NamespaceUri.FN)) {
                if (!config.isSchemaAvailable(NamespaceUri.FN)) {
                    List<String> messages = new ArrayList<>();
                    InputStream in = Version.platform.locateResource("xpath-functions.xsd", messages);
                    if (config.isTiming()) {
                        config.getLogger().info("Loading schema from resources for: " + NamespaceConstant.FN);
                    }
                    source = new StreamSource(in, "http://www.w3.org/2005/xpath-functions.scm");
                }
            } else {
                SchemaURIResolver resolver = compiler.getSchemaURIResolver();
                if (resolver != null) {
                    try {
                        Source[] sources = resolver.resolve(namespace, getBaseURI(), new String[0]);
                        if (sources == null || sources.length == 0) {
                            warning("Cannot locate a schema document for namespace " + namespace);
                            // Error recovery
                            importedSchema = new SingleNamespaceSchema((EnterpriseConfiguration) getConfiguration(), targetNamespace);
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

        SchemaDocument importee = null;
        int errors;
        try {
            PipelineConfiguration pipe = getSchemaNodeFactory().getPipelineConfiguration();
            importee = SchemaReader.read(source, compiler, pipe, null);
            XSDSchema s = importee.getXSDSchema();
            errors = importee.getXSDSchema().getErrorCount();
            importedSchema = s.getSchema();
        } catch (SchemaException e) {
            processSchemaException(schemaLocation, namespace, e);
            return;
        }

        boolean valid = (errors == 0);

        // Don't validate the imported schema yet, because it may have references to components
        // not yet available. But do remove the surrogate schema that was registered at the start
        // of the import, or subsequent operations may fail

        config.removeSchema(targetNamespace);

        if (!valid) {
            throw new SchemaException("Imported schema is invalid");
        }

        // check again that the target namespace differs (the check was only done
        // before if a "namespace" attribute was present

        NamespaceUri importedNamespace = importee.getTargetNamespace();
        if (thisSchemaDocument.getTargetNamespace().equals(importedNamespace)) {
            error("The targetNamespace of the imported schema must not be the same as " +
                    "the targetNamespace of the importing schema");
            return;
        }

        if (!targetNamespace.equals(importedNamespace)) {
            error("The namespace attribute in xs:import differs from the targetNamespace of the imported schema");
            return;
        }

        //  Add schema to list of imported schemas (if not already present)
        config.addSchema(importedSchema);
    }

    private void processSchemaException(String schemaLocation, String namespace, SchemaException e)
    throws SchemaException {
        Throwable cause = e.getException();
        if (XSDInclude.isFileNotFoundException(cause)) {
            warning("Imported schema document " + schemaLocation + " cannot be located: " +
                            cause.getClass().getName() + " (" + cause.getMessage() + ")");
        } else {
            error("Failed to process imported schema document: " + e.getMessage());
        }
        // create a dummy imported schema for error recovery
        importedSchema = new SingleNamespaceSchema((EnterpriseConfiguration) getConfiguration(), getTargetNamespace());
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

    public NamespaceUri getTargetNamespace() {
        String uri = attributes().getValue(NamespaceUri.NULL, "namespace");
        if  (uri == null) {
                return NamespaceUri.NULL;
            } else{
                return NamespaceUri.of(uri);
        }
    }

}

// Copyright (c) 2004-2023 Saxonica Limited