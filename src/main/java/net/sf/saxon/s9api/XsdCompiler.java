////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.s9api;

import net.sf.saxon.lib.ErrorReporter;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.SchemaURIResolver;
import net.sf.saxon.transpile.CSharpModifiers;

import javax.xml.transform.Source;
import java.io.File;

/**
 * The {@code XsdCompiler} is used to load schema documents, and to set options for the way in
 * which they are loaded.
 *
 * <p><i>The {@code XsdCompiler} is new in Saxon 13; it replaces the class {@link SchemaManager}
 * which is retained (but deprecated) to provide a degree of backwards compatibility.</i></p>
 *
 * <p>A schema, represented by an {@link XsdSchema} object, is a collection of schema components,
 * which may come from one schema document or from a collection of schema documents.
 * An {@link XsdCompiler} is used to create {@link XsdSchema} objects from source
 * schema documents.</p>
 *
 * <p>Schema processing requires Saxon-EE. The class {@code XsdCompiler} exists in Saxon-HE
 * and Saxon-PE, but with no useful functionality (most methods throw an {@link UnsupportedOperationException}).
 * In Saxon-EE the subclass {@code XsdSchemaCompilerEE} is instantiated to do the heavy lifting, but this
 * is not normally exposed to users.</p>
 *
 * <p>An {@code XsdCompiler} is created using the factory method {@link Processor#newXsdCompiler}. The normal pattern
 * of usage is to set options, then call {@link #compile(File...)} or {@link #compile(Source...)} to compile
 * one or more schema documents into a schema. The schema can be used to validate instance documents using
 * the method {@link XsdSchema#getValidator()}, and it can be added to the static context of XQuery queries
 * and XSLT stylesheets using the methods {@link XQueryCompiler#useSchema(XsdSchema)} and
 * {@link XsltCompiler#useSchema(XsdSchema)}.</p>
 *
 * @since 13.0
 */

@CSharpModifiers(code = {"abstract", "internal"})
public class XsdCompiler {

    public XsdCompiler() {
    }

    /**
     * Set the version of XSD in use for this schema. The value must be "1.0" or "1.1". The default is taken
     * from the configuration property {@link Feature#XSD_VERSION} at the time the
     * {@code XsdSchemaCompiler} is instantiated. By default this is "1.1".
     *
     * @param version the version of the XSD specification/language: either "1.0" or "1.1".
     */

    public void setXsdVersion(String version) {
        throw new UnsupportedOperationException();
    }

    /**
     * Get the version of XSD in use for this schema. The value will be "1.0" or "1.1"
     *
     * @return the version of XSD in use.
     */

    public String getXsdVersion() {
        throw new UnsupportedOperationException();
    };

    /**
     * Set the ErrorReporter to be used while loading and validating schema documents
     *
     * @param reporter The error reporter to be used. This is notified of all errors detected during the
     *                 compilation. May be set to null to revert to the default ErrorReporter.
     */
    public void setErrorReporter(ErrorReporter reporter) {
        throw new UnsupportedOperationException();
    }

    /**
     * Get the ErrorReporter being used while loading and validating schema documents
     *
     * @return reporter The error reporter in use. This is notified of all errors detected during the
     * compilation. Returns null if no user-supplied ErrorReporter has been set.
     */

    public ErrorReporter getErrorReporter() {
        throw new UnsupportedOperationException();
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

    public void setSchemaURIResolver(SchemaURIResolver resolver) {
        throw new UnsupportedOperationException();
    }

    /**
     * Get the SchemaURIResolver to be used during schema loading.
     *
     * @return the URIResolver used during stylesheet compilation. Returns null if no user-supplied
     *         URIResolver has been set.
     */

    public SchemaURIResolver getSchemaURIResolver() {
        throw new UnsupportedOperationException();
    }

    /**
     * Load a schema document from a given Source. The schema components derived from this schema
     * document are retained by this SchemaManager as the current schema, and are also returned
     * by the method as an {@link XsdSchema} object.
     *
     * <p>This method can only be called once on a given {@code SchemaManager} object.</p>
     *
     * @param source the documents containing the schema. The getSystemId() method applied to each Source
     *               must return a base URI suitable for resolving <code>xs:include</code> and <code>xs:import</code>
     *               directives. The document may be either a schema document in source XSD format, or a compiled
     *               schema in Saxon-defined SCM format.
     * @return the resulting schema.
     * @throws SaxonApiException if any of the schema documents is not valid, or if there are conflicts
     * between the schema documents.
     */

    public XsdSchema compile(Source... source) throws SaxonApiException {
        throw new UnsupportedOperationException();
    }

    /**
     * Load a schema document from a given File or set of files. The schema components derived from this schema
     * document are retained by this SchemaManager as the current schema, and are also returned
     * by the method as an {@link XsdSchema} object.
     *
     * @param file the document containing the schema. The getSystemId() method applied to this Source
     *               must return a base URI suitable for resolving <code>xs:include</code> and <code>xs:import</code>
     *               directives. The document may be either a schema document in source XSD format, or a compiled
     *               schema in Saxon-defined SCM format (as produced using the -export option)
     * @return the resulting schema.
     * @throws SaxonApiException if the schema document is not valid.
     * @since 12.0
     */

    public XsdSchema compile(File... file) throws SaxonApiException {
        throw new UnsupportedOperationException();
    }

    /**
     * Return an empty schema. This may be useful for validating instance documents when all
     * the actual schema definitions are linked from the instance document using {@code xsi:schemaLocation}
     * and {@code xsi:noNamespaceSchemaLocation} attributes
     * @return an empty schema
     */

    public XsdSchema emptySchema() {
        throw new UnsupportedOperationException();
    }

    /**
     * Import a precompiled Schema Component Model from a given Source. The schema components derived from this schema
     * document are added to the cache of schema components maintained by this SchemaManager
     *
     * @param source the XML file containing the schema component model, as generated by a previous call on
     *               {@link Configuration#exportComponents}
     * @return
     */


    public XsdSchema importComponents(Source source) throws SaxonApiException {
        throw new UnsupportedOperationException();
    }

}

