package com.saxonica.ee.s9api;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.PreparedSchema;
import com.saxonica.ee.schema.SchemaCompiler;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.lib.*;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpModifiers;

import javax.xml.transform.ErrorListener;
import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import java.util.Arrays;

/**
 * A SchemaManager is used to load schema documents, and to set options for the way in which they are loaded.
 */
@CSharpModifiers(code = {"internal"})
public class SchemaManagerImpl extends SchemaManager {

    private final Processor processor;
    private ErrorReporter errorReporter;
    private PreparedSchema schema;
    private int xsdVersion;

    public SchemaManagerImpl(Processor processor) {
        this.processor = processor;
        errorReporter = processor.getUnderlyingConfiguration().makeErrorReporter();
        if (errorReporter instanceof StandardErrorReporter) {
            ((StandardErrorReporter) errorReporter).setOutputErrorCodes(false);
        }
        xsdVersion = processor.getUnderlyingConfiguration().getXsdVersion();
    }


    /**
     * Set the version of XSD in use for this schema. The value must be "1.0" or "1.1". The default is currently "1.0",
     * but this may change in a future release.
     *
     * @param version the version of the XSD specification/language: either "1.0" or "1.1".
     */

    @Override
    public void setXsdVersion(String version) {
        if (version.equals("1.0") || version.equals("1.1")) {
            this.xsdVersion = version.equals("1.0") ? Configuration.XSD10 : Configuration.XSD11;
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
        return xsdVersion == Configuration.XSD10 ? "1.0" : "1.1";
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
     * Load schema documents from given Sources. The schema components derived from this schema
     * document are added to the collection of schema components maintained by this SchemaManager.
     *
     * <p>If the source is a {@link StreamSource} containing a systemId but no {@link java.io.InputStream}
     * or {@link java.io.Reader}, then an attempt is made to resolve the systemId using the resource
     * resolver registered with the {@code Configuration}. It is NOT resolved using the {@link SchemaURIResolver} (this
     * is used only for included and imported modules).</p>
     *
     * @param sources the documents containing the schema. The getSystemId() method applied to each Source
     *               must return a base URI suitable for resolving <code>xs:include</code> and <code>xs:import</code>
     *               directives. The document may be either a schema document in source XSD format, or a compiled
     *               schema in Saxon-defined SCM format (as produced using the -export option)
     * @return the schema resulting from this and previous calls on {@link #load(Source...)}
     * @throws net.sf.saxon.s9api.SaxonApiException if the schema document is not valid, or if its contents are inconsistent
     *                                              with the schema components already held by this SchemaManager.
     */

    @Override
    public synchronized XsdSchema load(Source... sources) throws SaxonApiException {
        try {
            final EnterpriseConfiguration config = (EnterpriseConfiguration) processor.getUnderlyingConfiguration();
            SchemaCompiler compiler = new SchemaCompiler(config);
            compiler.setLanguageVersion(xsdVersion);
            PreparedSchema ps = compiler.loadSources(Arrays.asList(sources));
            if (schema != null) {
                schema.copyTo(ps, compiler);
            }
            schema = ps;
            return new XsdSchemaImpl(processor, schema);
        } catch (XPathException e) {
            throw new SaxonApiException(e);
        }
    }
    /**
     * Import a precompiled Schema Component Model from a given Source. The schema components derived from this schema
     * document are added to the collection of schema components maintained by this SchemaManager.
     *
     * @param source the XML file containing the schema component model, as generated by a previous call on
     *               {@link #exportComponents}
     * @throws net.sf.saxon.s9api.SaxonApiException if a failure occurs loading the schema from the supplied source
     */

    @Override
    public synchronized XsdSchema importComponents(Source source) throws SaxonApiException {
        throw new NotImplementedException();
//        try {
//            processor.getUnderlyingConfiguration().importComponents(source);    // TODO: make it local
//        } catch (XPathException err) {
//            throw new SaxonApiException(err);
//        }
        //return new XsdSchemaImpl(schema);
    }

    public void clear() {
        schema = null;
    }

    public XsdSchema getXsdSchema() {
        return new XsdSchemaImpl(processor, schema);
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
            schema.exportComponents(out);
        } catch (XPathException e) {
            throw new SaxonApiException(e);
        }
    }


    /**
     * Create a SchemaValidator which can be used to validate instance documents against the schema held by this
     * SchemaManager
     *
     * @return a new SchemaValidator
     * @throws IllegalStateException if no schema has been loaded
     */

    @Override
    public SchemaValidator newSchemaValidator() throws IllegalStateException {
        PreparedSchema ps = schema;
        if (ps == null) {
            throw new IllegalStateException("No schema has been loaded");
        }
        return new SchemaValidatorImpl(processor, new XsdSchemaImpl(processor, ps));
    }


}
// Copyright (c) 2018-2023 Saxonica Limited
