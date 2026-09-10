package com.saxonica.ee.s9api;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.lib.*;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpModifiers;
import net.sf.saxon.type.SchemaException;

import javax.xml.transform.ErrorListener;
import javax.xml.transform.Source;

/**
 * The SchemaManager is used to load schema documents, and to set options for the way in which they are loaded.
 * At present all the resulting schema components are held in a single pool owned by the Processor object.
 */
@CSharpModifiers(code = {"internal"})
public class SchemaManagerImpl extends SchemaManager {

    private final Processor processor;
    private ErrorReporter errorReporter;

    public SchemaManagerImpl(Processor processor) {
        this.processor = processor;
        errorReporter = processor.getUnderlyingConfiguration().makeErrorReporter();
        if (errorReporter instanceof StandardErrorReporter) {
            ((StandardErrorReporter) errorReporter).setOutputErrorCodes(false);
        }
    }


    /**
     * Set the version of XSD in use. The value must be "1.0" or "1.1". The default is currently "1.0",
     * but this may change in a future release.
     *
     * @param version the version of the XSD specification/language: either "1.0" or "1.1".
     */

    @Override
    public void setXsdVersion(String version) {
        if (version.equals("1.0") || version.equals("1.1")) {
            processor.setConfigurationProperty(Feature.XSD_VERSION, version);
        } else {
            throw new IllegalArgumentException("XsdVersion");
        }
    }

    /**
     * Get the version of XSD in use. The value will be "1.0" or "1.1"
     *
     * @return the version of XSD in use.
     */

    @Override
    public String getXsdVersion() {
        return processor.getUnderlyingConfiguration().getXsdVersion() == Configuration.XSD10 ? "1.0" : "1.1";
    }

    //#if CSHARP==false
    /**
     * Set the ErrorListener to be used while loading and validating schema documents
     *
     * @param listener The error listener to be used. This is notified of all errors detected during the
     *                 compilation. May be set to null to revert to the default ErrorListener.
     */

    @Override
    public void setErrorListener(/*@Nullable*/ ErrorListener listener) {
        this.errorReporter = new ErrorReporterToListener(listener);
    }

    /**
     * Get the ErrorListener being used while loading and validating schema documents
     *
     * @return listener The error listener in use. This is notified of all errors detected during the
     *         compilation. Returns null if no user-supplied ErrorListener has been set.
     */

    /*@Nullable*/
    @Override
    public ErrorListener getErrorListener() {
        if (errorReporter instanceof ErrorReporterToListener) {
            return ((ErrorReporterToListener)errorReporter).getErrorListener();
        } else {
            return null;
        }
    }
    //#endif

    @Override
    public void setErrorReporter(ErrorReporter reporter) {
        this.errorReporter = reporter;
    }

    @Override
    public ErrorReporter getErrorReporter() {
        return errorReporter;
    }


    /**
     * Set the SchemaURIResolver to be used during schema loading. This SchemaURIResolver, despite its name,
     * is <b>not</b> used for resolving relative URIs against a base URI; it is used for dereferencing
     * an absolute URI (after resolution) to return a {@link javax.xml.transform.Source} representing the
     * location where a schema document can be found.
     * <p>This SchemaURIResolver is used to dereference the URIs appearing in <code>xs:import</code>,
     * <code>xs:include</code>, and <code>xs:redefine</code> declarations.</p>
     *
     * @param resolver the SchemaURIResolver to be used during schema loading.
     */

    @Override
    public void setSchemaURIResolver(SchemaURIResolver resolver) {
        ((EnterpriseConfiguration)processor.getUnderlyingConfiguration()).setSchemaURIResolver(resolver);
    }

    /**
     * Get the SchemaURIResolver to be used during schema loading.
     *
     * @return the URIResolver used during stylesheet compilation. Returns null if no user-supplied
     *         URIResolver has been set.
     */

    @Override
    public SchemaURIResolver getSchemaURIResolver() {
        return ((EnterpriseConfiguration)processor.getUnderlyingConfiguration()).getSchemaURIResolver();
    }

    /**
     * Load a schema document from a given Source. The schema components derived from this schema
     * document are added to the cache of schema components maintained by this SchemaManager
     *
     * @param source the document containing the schema. The getSystemId() method applied to this Source
     *               must return a base URI suitable for resolving <code>xs:include</code> and <code>xs:import</code>
     *               directives. The document may be either a schema document in source XSD format, or a compiled
     *               schema in Saxon-defined SCM format (as produced using the -export option)
     * @throws net.sf.saxon.s9api.SaxonApiException if the schema document is not valid, or if its contents are inconsistent
     *                           with the schema components already held by this SchemaManager.
     */

    @Override
    public void load(Source source) throws SaxonApiException {
        try {
            processor.getUnderlyingConfiguration().addSchemaSource(source, errorReporter);
        } catch (SchemaException e) {
            throw new SaxonApiException(e);
        }
    }

    /**
     * Import a precompiled Schema Component Model from a given Source. The schema components derived from this schema
     * document are added to the cache of schema components maintained by this SchemaManager
     *
     * @param source the XML file containing the schema component model, as generated by a previous call on
     *               {@link #exportComponents}
     * @throws net.sf.saxon.s9api.SaxonApiException if a failure occurs loading the schema from the supplied source
     */

    @Override
    public void importComponents(Source source) throws SaxonApiException {
        try {
            processor.getUnderlyingConfiguration().importComponents(source);
        } catch (XPathException err) {
            throw new SaxonApiException(err);
        }
    }

    /**
     * Export a precompiled Schema Component Model containing all the components (except built-in components)
     * that have been loaded into this Processor.
     *
     * @param destination the destination to recieve the precompiled Schema Component Model in the form of an
     *                    XML document
     * @throws net.sf.saxon.s9api.SaxonApiException if a failure occurs writing the schema components to the supplied destination
     */

    @Override
    public void exportComponents(Destination destination) throws SaxonApiException {
        try {
            Configuration config = processor.getUnderlyingConfiguration();
            if (config.getConfigurationProperty(Feature.ALLOW_UNRESOLVED_SCHEMA_COMPONENTS)) {
                throw new SaxonApiException("Exporting to an SCM file is not possible when unresolved references to schema components have been permitted");
            }
            Receiver out = destination.getReceiver(config.makePipelineConfiguration(), config.obtainDefaultSerializationProperties());
            config.exportComponents(out);
        } catch (XPathException e) {
            throw new SaxonApiException(e);
        }
    }


    /**
     * Create a SchemaValidator which can be used to validate instance documents against the schema held by this
     * SchemaManager
     *
     * @return a new SchemaValidator
     */

    @Override
    public SchemaValidator newSchemaValidator() {
        return new SchemaValidatorImpl(processor);
    }


}
// Copyright (c) 2018-2020 Saxonica Limited
