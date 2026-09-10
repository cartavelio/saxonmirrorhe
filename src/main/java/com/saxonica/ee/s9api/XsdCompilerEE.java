package com.saxonica.ee.s9api;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.config.StandardSchemaResolver;
import com.saxonica.ee.schema.PreparedSchema;
import com.saxonica.ee.schema.SchemaCompiler;
import com.saxonica.ee.schema.SchemaModelLoader;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.ErrorReporter;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.SchemaURIResolver;
import net.sf.saxon.lib.StandardErrorReporter;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XsdCompiler;
import net.sf.saxon.s9api.XsdSchema;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpModifiers;
import net.sf.saxon.type.Schema;

import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.util.Arrays;

/**
 * An {@code XsdCompilerEE} is used to load schema documents, and to set options for the way in which they are loaded.
 */
@CSharpModifiers(code = {"internal"})
public class XsdCompilerEE extends XsdCompiler {

    private final Processor processor;
    private ErrorReporter errorReporter;
    private int xsdVersion;
    private SchemaURIResolver resolver;

    public XsdCompilerEE(Processor processor) {
        this.processor = processor;
        errorReporter = processor.getUnderlyingConfiguration().makeErrorReporter();
        if (errorReporter instanceof StandardErrorReporter) {
            ((StandardErrorReporter) errorReporter).setOutputErrorCodes(false);
        }
        xsdVersion = processor.getUnderlyingConfiguration().getXsdVersion();
        resolver = new StandardSchemaResolver((EnterpriseConfiguration)processor.getUnderlyingConfiguration());
    }


    /**
     * Set the version of XSD in use for this schema. The value must be "1.0" or "1.1". The default is taken
     * from the configuration property {@link Feature#XSD_VERSION} at the time the
     * {@code XsdSchemaCompiler} is instantiated. By default this is "1.1".
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

    /**
     * Set the ErrorReporter to be used while loading and validating schema documents
     *
     * @param reporter The error reporter to be used. This is notified of all errors detected during the
     *                 compilation. May be set to null to revert to the default ErrorReporter.
     */
    @Override
    public void setErrorReporter(ErrorReporter reporter) {
        this.errorReporter = reporter;
    }

    /**
     * Get the ErrorReporter being used while loading and validating schema documents
     *
     * @return reporter The error reporter in use. This is notified of all errors detected during the
     * compilation. Returns null if no user-supplied ErrorReporter has been set.
     */
    @Override
    public ErrorReporter getErrorReporter() {
        return errorReporter;
    }

    /**
     * Set the SchemaURIResolver to be used during schema loading. This SchemaURIResolver, despite its name,
     * is <b>not</b> used for resolving relative URIs against a base URI; it is used for dereferencing
     * an absolute URI (after resolution) to return a {@link Source} representing the
     * location where a schema document can be found.
     * <p>This SchemaURIResolver is used to dereference the URIs appearing in <code>xs:import</code>,
     * <code>xs:include</code>, and <code>xs:redefine</code> declarations.</p>
     *
     * @param resolver the SchemaURIResolver to be used during schema loading.
     */

    @Override
    public void setSchemaURIResolver(SchemaURIResolver resolver) {
        this.resolver = resolver;
    }

    /**
     * Get the SchemaURIResolver to be used during schema loading.
     *
     * @return the URIResolver used during stylesheet compilation. Returns null if no user-supplied
     *         URIResolver has been set.
     */

    @Override
    public SchemaURIResolver getSchemaURIResolver() {
        return resolver;
    }


    /**
     * Load schema documents from given Sources. The schema components derived from this schema
     * document are added to the cache of schema components maintained by this SchemaManager.
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
     * @throws SaxonApiException if the schema document is not valid, or if its contents are inconsistent
     *                                              with the schema components already held by this SchemaManager.
     */

    @Override
    public XsdSchema compile(Source... sources) throws SaxonApiException {
        try {
            final EnterpriseConfiguration config = (EnterpriseConfiguration) processor.getUnderlyingConfiguration();
            SchemaCompiler compiler = new SchemaCompiler(config);
            compiler.setLanguageVersion(xsdVersion);
            compiler.setErrorReporter(errorReporter);
            compiler.setSchemaURIResolver(resolver);
            PreparedSchema schema;
            if (sources.length == 0) {
                schema = new PreparedSchema(config); // an empty schema
            } else {
                schema = compiler.loadSources(Arrays.asList(sources));
            }
            return new XsdSchemaImpl(processor, schema);
        } catch (XPathException e) {
            throw new SaxonApiException(e);
        }
    }

    /**
     * Load a schema document from a given File or set of files. The schema components derived from this schema
     * document are retained by this SchemaManager as the current schema, and are also returned
     * by the method as an {@link XsdSchema} object.
     *
     * @param files the documents containing the schema. The getSystemId() method applied to this Source
     *             must return a base URI suitable for resolving <code>xs:include</code> and <code>xs:import</code>
     *             directives. The document may be either a schema document in source XSD format, or a compiled
     *             schema in Saxon-defined SCM format (as produced using the -export option)
     * @return the resulting schema.
     * @throws SaxonApiException if the schema document is not valid.
     */
    @Override
    public XsdSchema compile(File... files) throws SaxonApiException {
        StreamSource[] sources = new StreamSource[files.length];
        for (int i=0; i<files.length; i++) {
            sources[i] = new StreamSource(files[i]);
        }
        return compile(sources);
    }

    /**
     * Return an empty schema. This may be useful for validating instance documents when all
     * the actual schema definitions are linked from the instance document using {@code xsi:schemaLocation}
     * and {@code xsi:noNamespaceSchemaLocation} attributes
     *
     * @return an empty schema
     */

    public XsdSchema emptySchema() {
        return new XsdSchemaImpl(processor,
                                 new PreparedSchema((EnterpriseConfiguration) processor.getUnderlyingConfiguration()));
    }

    /**
     * Import a precompiled Schema Component Model from a given Source. The schema components derived from this schema
     * document are added to the cache of schema components maintained by this SchemaManager
     *
     * @param source the XML file containing the schema component model, as generated by a previous call on
     *               {@link XsdSchema#exportComponents}
     * @return the schema that results from importing the SCM file
     */


    public XsdSchema importComponents(Source source) throws SaxonApiException {
        try {
            SchemaModelLoader loader = new SchemaModelLoader(processor.getUnderlyingConfiguration());
            PreparedSchema ps = loader.load(source);
            return new XsdSchemaImpl(processor, ps);
        } catch (XPathException e) {
            throw new SaxonApiException(e);
        }
    }

    /**
     * Construct an {@link XsdSchema} by wrapping an internal schema object. This is intended primarily
     * for internal use
     * @param internalSchema the internal schema object
     */

    public XsdSchema wrapInternalSchema(Schema internalSchema) {
        if (!(internalSchema instanceof PreparedSchema)) {
            throw new IllegalArgumentException("The internalSchema must be an instance of PreparedSchema");
        }
        return new XsdSchemaImpl(processor, (PreparedSchema)internalSchema);
    }


}
// Copyright (c) 2018-2023 Saxonica Limited
