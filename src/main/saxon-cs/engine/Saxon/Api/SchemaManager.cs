using System;
using System.Impl.Helpers;
using System.IO;
using System.Xml;
using Saxon.Ejavax.xml.transform.stream;
using Saxon.Hej;
using Saxon.Hej.pull;
using Saxon.Hej.type;
using Saxon.Api.Support;



namespace Saxon.Api {
    /// <summary>
    /// A <c>SchemaManager</c> is responsible for compiling schemas and
    /// maintaining a cache of compiled schemas that can be used for validating
    /// instance documents.
    /// </summary>
    /// <remarks>
    /// <para>To obtain a <c>SchemaManager</c>, use the 
    /// <c>SchemaManager</c> property of the <c>Processor</c> object.</para>
    /// <para>In a schema-aware <c>Processor</c> there is exactly one
    /// <c>SchemaManager</c> (in a non-schema-aware <c>Processor</c> there is none).</para>
    /// <para>The cache of compiled schema definitions can include only one schema
    /// component (for example a type, or an element declaration) with any given name.
    /// An attempt to compile two different schemas in the same namespace will usually
    /// therefore fail.</para>
    /// <para>As soon as a type definition or element declaration is used for the first
    /// time in a validation episode, it is marked as being "sealed": this prevents subsequent
    /// modifications to the component. Examples of modifications that are thereby disallowed
    /// include adding to the substitution group of an existing element declaration, adding subtypes
    /// to an existing type, or redefining components using <c>&lt;xs:redefine&gt;</c>.</para>
    /// </remarks>

    [Serializable]
    public class SchemaManager
    {
        private Saxon.Hej.s9api.SchemaManager s9schemaManager;
        private Configuration config;
        private Processor processor;
        
        // internal constructor

        internal SchemaManager(Processor processor)
        {
            this.processor = processor;
            this.s9schemaManager = processor.JProcessor.getSchemaManager();
            this.config = processor.Implementation;
        }

        /// <summary>
        /// The version of the W3C XML Schema Specification handled by this <c>SchemaManager</c>.
        /// </summary>
        /// <remarks>
        /// <para>The value must be "1.0" (indicating XML Schema 1.0) or "1.1" (indicating XML Schema 1.1).
        /// The default is "1.0". New constructs defined in XSD 1.1 are rejected unless this property
        /// is set to "1.1" before compiling the schema.
        /// </para>
        /// </remarks>
        /// 
        public string XsdVersion {
            get => s9schemaManager.getXsdVersion();
            set => s9schemaManager.setXsdVersion(value);
        }
        
        /// <summary>
        /// The <c>SchemaResolver</c> is a user-supplied callback used for resolving references to
        /// schema documents. It applies to references from one schema document to another
        /// appearing in <c>xs:import</c>, <c>xs:include</c>, and <c>xs:redefine</c>; to
        /// references from an instance document to a schema in <c>xsi:schemaLocation</c> and
        /// <c>xsi:noNamespaceSchemaLocation</c>, to <c>xsl:import-schema</c> in XSLT, and to
        /// the <c>import schema</c> declaration in XQuery.
        /// </summary>

        public SchemaResolver SchemaResolver {
            set {
                DelegatingSchemaResolver dsr = new DelegatingSchemaResolver(value);
                dsr.setConfiguration(processor.Implementation);
                s9schemaManager.setSchemaURIResolver(dsr);
            }
        }

        /// <summary>Set the <c>ErrorReporter</c> to be used when validating instance documents as a user defined ErrorReporter.
        /// If this property is used then the ErrorList property and SetErrorList method is overriden </summary>
        /// <remarks>The <c>IErrorReporter</c> to be used</remarks>
        public ErrorReporter ErrorReporter {
            set => s9schemaManager.setErrorReporter(new ErrorReporterWrapper(value));
        }
        
        /// <summary>
        /// Compile a schema supplied as a <c>Stream</c>. The resulting schema components are added
        /// to the cache. The base URI of the schema document is taken as the current working directory.
        /// </summary>
        /// <param name="input">A stream containing the source text of the schema. This method
        /// will consume the supplied stream. It is the caller's responsibility to close the stream
        /// after use.</param>

        public void Compile(Stream input) {
            Compile (input, new Uri(Environment.CurrentDirectory));
        }

        /// <summary>
        /// Compile a schema supplied as a <c>Stream</c>. The resulting schema components are added
        /// to the cache.
        /// </summary>
        /// <param name="input">A stream containing the source text of the schema. This method
        /// will consume the supplied stream. It is the caller's responsibility to close the stream
        /// after use.</param>
        /// <param name="baseUri">The base URI of the schema document, for resolving any references to other
        /// schema documents</param>        

        public void Compile(Stream input, Uri baseUri) {
            StreamSource source = new (input, baseUri.ToString());
            s9schemaManager.load(source);
        }
        
        /// <summary>
        /// Compile a schema supplied as a <c>TextReader</c>. The resulting schema components are added
        /// to the cache. The base URI of the schema document is taken as the current working directory.
        /// </summary>
        /// <param name="input">A <c>TextReader</c> that supplies the source text of the schema. This method
        /// will consume the supplied stream. It is the caller's responsibility to close the reader
        /// after use.</param>

        public void Compile(TextReader input) {
            Compile (input, new Uri(Environment.CurrentDirectory));
        }
        
        /// <summary>
        /// Compile a schema supplied as a <c>TextReader</c>. The resulting schema components are added
        /// to the cache.
        /// </summary>
        /// <param name="input">A <c>TextReader</c> that supplies the source text of the schema. This method
        /// will consume the supplied stream. It is the caller's responsibility to close the reader
        /// after use.</param>
        /// <param name="baseUri">The base URI of the schema document, for resolving any references to other
        /// schema documents</param>        

        public void Compile(TextReader input, Uri baseUri) {
            StreamSource source = new (input, baseUri.ToString());
            s9schemaManager.load(source);
        }

        /// <summary>
        /// Compile a schema, retrieving the source using a URI. The resulting schema components are added
        /// to the cache.
        /// </summary>
        /// <remarks>
        /// The document located via the URI is parsed using the <c>System.Xml</c> parser.
        /// </remarks>
        /// <param name="uri">The URI identifying the location where the schema document can be
        /// found</param>

        public void Compile(Uri uri)
        {
            StreamSource source = new (uri.ToString());
            s9schemaManager.load(source);
        }

        /// <summary>
        /// Compile a schema, delivered using an <c>XmlReader</c>. The resulting schema components are added
        /// to the cache.
        /// </summary>
        /// <remarks>
        /// The <c>XmlReader</c> is responsible for parsing the document; this method builds a tree
        /// representation of the document (in an internal Saxon format) and compiles it.
        /// The <c>XmlReader</c> is used as supplied; it is the caller's responsibility to ensure that
        /// its settings are appropriate for parsing a schema document (for example, that entity references
        /// are expanded and whitespace is retained).
        /// </remarks>
        /// <param name="reader">The <c>XmlReader</c> (that is, the XML parser) used to supply the source schema document</param>

        public void Compile(XmlReader reader)
        {
            DotNetPullProvider source = new (reader);
            s9schemaManager.load(new PullSource(source));
        }

        /// <summary>
        /// Compile a schema document, located at an <c>XdmNode</c>. This may be a document node whose
        /// child is an <c>xs:schema</c> element, or it may be
        /// the <c>xs:schema</c> element itself. The resulting schema components are added
        /// to the cache.
        /// </summary>
        /// <param name="node">The document node or the outermost element node of a schema document.</param>

        public void Compile(XdmNode node)
        {
            s9schemaManager.load(node.Implementation.asActiveSource());
        }

        /// <summary>
        /// Import schema components from an SCM file, produced using a previous call on
        /// <c>ExportComponents</c>.
        /// </summary>
        /// <param name="schemaComponentModel">The XML representation of the SCM.</param>
        public void ImportComponents(XdmNode schemaComponentModel) {
            s9schemaManager.importComponents(schemaComponentModel.Implementation.asActiveSource());
        }
        
        /// <summary>
        /// Export schema components to an SCM file, available using a subsequent call on
        /// <c>ImportComponents</c>.
        /// </summary>
        /// <remarks>
        /// The returned SCM tree will typically be serialized to a file, so that it can be reused
        /// whenever a schema is required. Most Saxon APIs that accept a source XSD schema will also
        /// accept a lexical SCM file in its place.
        /// </remarks>
        /// <returns>The XML representation of the SCM.</returns>
        public XdmNode ExportComponents() {
            XdmDestination destination = new XdmDestination();
            s9schemaManager.exportComponents(destination.UnderlyingDestination);
            return destination.XdmNode;
        }

        /// <summary>
        /// Create a new <c>SchemaValidator</c>, which may be used for validating instance
        /// documents.
        /// </summary>
        /// <remarks>
        /// <para>The <c>SchemaValidator</c> uses the cache of schema components held by the
        /// <c>SchemaManager</c>. It may also add new components to this cache (for example,
        /// when the instance document references a schema using <c>xsi:schemaLocation</c>).
        /// It is also affected by changes to the schema cache that occur after the 
        /// <c>SchemaValidator</c> is created.</para>
        /// <para>When schema components are used for validating instance documents (or for compiling
        /// schema-aware queries and stylesheets) they are <i>sealed</i> to prevent subsequent modification.
        /// The modifications disallowed once a component is sealed include adding to the substitution group
        /// of an element declaration, adding subtypes derived by extension to an existing complex type, and
        /// use of <c>&lt;xs:redefine&gt;</c>.</para>
        /// </remarks>

        public SchemaValidator NewSchemaValidator() {
            return new (s9schemaManager.newSchemaValidator(), processor);
        }

        /// <summary>
        /// Factory method to get an <c>AtomicType</c> object representing the atomic type with a given <c>QName</c>.
        /// </summary>
        /// <remarks>
        /// It is undefined whether two calls on this method supplying the same <c>QName</c> will return the same
        /// <c>XdmAtomicType</c> object instance.
        /// </remarks>
        /// <param name="qname">The <c>QName</c> of the required type</param>
        /// <returns>An <c>AtomicType</c> object representing this type if it is present in this schema (and is an
        /// atomic type); otherwise, null. </returns>

        public XdmAtomicType GetAtomicType(QName qname)
        {
            SchemaType type = config.getSchemaType(qname.GetStructuredQName());
            if (type is BuiltInAtomicType)
            {
                return XdmAtomicType.BuiltInAtomicType(qname);
            }
            // else if (type is AtomicType)
            // {
            //     ItemTypeFactory factory = new ItemTypeFactory(processor.JProcessor);
            //     return (XdmAtomicType)factory.getAtomicType(qname);
            // }
            else
            {
                return null;
            }
        }
        
        
    }
}

// Copyright (c) 2021 Saxonica Limited