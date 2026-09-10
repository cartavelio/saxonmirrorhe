
using Saxon.Eej.ee.s9api;
using Saxon.Eej.ee.schema;
using Saxon.Ejavax.xml.transform;
using Saxon.Ejavax.xml.transform.stream;

namespace Saxon.Api.Support {
    internal class XsdCompilerEE : Saxon.Api.XsdCompiler {

        private readonly Saxon.Hej.s9api.XsdCompiler xsdCompilerJ;
        private readonly Saxon.Api.Processor processor;
        private ErrorReporter _errorReporter;

        internal XsdCompilerEE( Saxon.Api.Processor processor, Saxon.Hej.s9api.XsdCompiler xsdCompilerJ) {
            this.processor = processor;
            this.xsdCompilerJ = xsdCompilerJ;
        }

        /// <summary>
        /// The version of the W3C XML Schema Specification handled by this <c>XsdCompiler</c>.
        /// </summary>
        /// <remarks>
        /// <para>The value must be "1.0" (indicating XML Schema 1.0) or "1.1" (indicating XML Schema 1.1).
        /// The default is "1.0". New constructs defined in XSD 1.1 are rejected unless this property
        /// is set to "1.1" before compiling the schema.
        /// </para>
        /// </remarks>
        /// 
        public override string XsdVersion {
            get => xsdCompilerJ.getXsdVersion();
            set => xsdCompilerJ.setXsdVersion(value);
        }

        /// <summary>The <c>ErrorReporter</c> to be used when compiling schema documents.</summary>

        /// <summary>
        /// The <code>ErrorReporter</code> to be used at run-time to notify any dynamic
        /// errors that occur.
        /// </summary>

        public override ErrorReporter ErrorReporter {
            get => _errorReporter;
            set {
                _errorReporter = value;
                xsdCompilerJ.setErrorReporter(new ErrorReporterWrapper(value));
            }
        }

        /// <summary>
        /// The <c>SchemaResolver</c> is a user-supplied callback used for resolving references to
        /// schema documents. It applies to references from one schema document to another
        /// appearing in <c>xs:import</c>, <c>xs:include</c>, <c>xs:override</c>, and <c>xs:redefine</c>; to
        /// references from an instance document to a schema in <c>xsi:schemaLocation</c> and
        /// <c>xsi:noNamespaceSchemaLocation</c>, to <c>xsl:import-schema</c> in XSLT, and to
        /// the <c>import schema</c> declaration in XQuery.
        /// </summary>
        /// <remarks>
        /// <para>If no <c>SchemaResolver</c> is supplied, or if the supplied
        /// <c>SchemaResolver</c> returns null, then the system attempts to locate
        /// a schema using the common resource resolver associated with the Saxon <c>Configuration</c>.
        /// It first attempts to resolve the location hint (if available), then the target namespace
        /// (if present and non-empty). If both attempts fail, the system then attempts to use
        /// standard URI resolution mechanisms to resolve the location hint (if available); it does
        /// not attempt to resolve the target namespace.
        /// </para>
        /// </remarks>

        public override SchemaResolver SchemaResolver {
            set {
                DelegatingSchemaResolver dsr = new DelegatingSchemaResolver(value, processor.Implementation);
                xsdCompilerJ.setSchemaURIResolver(dsr);
            }
        }

        ///<summary>
        /// Load a schema document from a given SchemaSource, or from a set of SchemaSources.
        /// The schema components are  returned
        /// by the method as an <c>XsdSchema</c> object.
        ///</summary>
        public override XsdSchema Compile(params XsdCompiler.SchemaSource[] sources) {
            Source[] ss = new Source[sources.Length];
            for (int i = 0; i < sources.Length; i++) {
                ss[i] = new StreamSource(sources[i].Stream, sources[i].Uri.AbsoluteUri);
            }

            return new XsdSchemaEE(processor, xsdCompilerJ.compile(ss));
        }

        /**
         * Combine a set of schemas into one
         * @param schemas the input schemas. These are not modified.
         * @return the combined schema
         * @throws SaxonApiException if the schemas are not compatible, for example if they contain
         * duplicate names.
         * @since 13.0
         */
        public override XsdSchema Combine(params XsdSchema[] schemas) {
            Saxon.Hej.s9api.XsdSchema[] jSchemas = new Hej.s9api.XsdSchema[schemas.Length];
            for (int i = 0; i < schemas.Length; i++) {
                jSchemas[i] = new Saxon.Eej.ee.s9api.XsdSchemaImpl(
                    processor.JProcessor,
                    (PreparedSchema)((Saxon.Api.Support.XsdSchemaEE)schemas[i]).UnderlyingSchema);
            }
            return new XsdSchemaEE(processor, xsdCompilerJ.combine(jSchemas));
        }

        /**
         * Return an empty schema. This may be useful for validating instance documents when all
         * the actual schema definitions are linked from the instance document using {@code xsi:schemaLocation}
         * and {@code xsi:noNamespaceSchemaLocation} attributes
         * @return an empty schema
         */
        public override XsdSchema EmptySchema() {
            return new XsdSchemaEE(processor, xsdCompilerJ.emptySchema());
        }

        /**
         * Import a precompiled Schema Component Model from a given Source. The schema components derived from this schema
         * document are added to the cache of schema components maintained by this SchemaManager
         *
         * @param source the XML file containing the schema component model, as generated by a previous call on
         *               {@link XsdSchema#exportComponents}
         * @return the schema produced by processing the supplied schema component model export file
         */
        public override XsdSchema ImportComponents(SchemaSource source) {
            Saxon.Hej.s9api.XsdSchema s = xsdCompilerJ.importComponents(
                new StreamSource(source.Stream, source.Uri.ToString()));
            return new XsdSchemaEE(processor, s);
        }
    }
}