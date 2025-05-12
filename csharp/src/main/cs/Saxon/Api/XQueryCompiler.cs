using System;
using System.IO;
using Saxon.Api.Support;
using Saxon.Callbacks;

namespace Saxon.Api {
    /// <summary>
    /// The <c>XQueryCompiler</c> object allows XQuery queries to be compiled.
    /// </summary>
    /// <remarks>
    /// <para>To construct an <c>XQueryCompiler</c>, use the factory method
    /// <c>NewXQueryCompiler</c> on the <see cref="Processor"/> object.</para>
    /// <para>The <c>XQueryCompiler</c> holds information that represents the static context
    /// for the queries that it compiles. This information remains intact after performing
    /// a compilation. An <c>XQueryCompiler</c> may therefore be used repeatedly to compile multiple
    /// queries. Any changes made to the <c>XQueryCompiler</c> (that is, to the
    /// static context) do not affect queries that have already been compiled.</para>
    /// <para>An <c>XQueryCompiler</c> may be used concurrently in multiple threads, but
    /// it should not then be modified once initialized.</para>
    /// </remarks>
    [Serializable]
    public class XQueryCompiler {
        private XQueryResolver _moduleResolver;
        private ErrorReporter _errorReporter;

        // internal constructor: the public interface is a factory method
        // on the Processor object

        internal XQueryCompiler(Processor processor) {
            this.Processor = processor;
            Implementation = processor.JProcessor.newXQueryCompiler();
        }

        /// <summary>
        /// Declare a namespace for use by the query. This has the same
        /// status as a namespace declaration appearing within the query prolog (though
        /// a declaration of the same prefix in the query prolog will take
        /// precedence).
        /// </summary>
        /// <param name="prefix">The namespace prefix to be declared. Use
        /// a zero-length string to declare the default namespace (that is, the
        /// default namespace for elements and types).</param>
        /// <param name="uri">The namespace URI. It is possible to specify
        /// a zero-length string to "undeclare" a namespace.</param>
        public void DeclareNamespace(string prefix, string uri) {
            Implementation.declareNamespace(prefix, uri);
        }
        
        /// <summary>
        /// The name of the default collation used by queries compiled using this <c>XQueryCompiler</c>.
        /// This must be the name of a collation that is known to the <c>Processor</c>.
        /// </summary>

        public string DefaultCollationName {
            get => Implementation.getDefaultCollationName();
            set => Implementation.declareDefaultCollation(value);
        }

        /// <summary>
        /// Get the <c>Processor</c> from which this <c>XQueryCompiler</c> was constructed
        /// </summary>

        public Processor Processor { get; }

        /// <summary>
        /// The required context item type for the expression. This is used for
        /// optimizing the expression at compile time, and to check at run-time
        /// that the value supplied for the context item is the correct type.
        /// </summary>

        public XdmItemType ContextItemType {
            get => Implementation.getRequiredContextItemType();
            set => Implementation.setRequiredContextItemType(value);
        }

        /// <summary>
        /// The base URI of the query, which forms part of the static context
        /// of the query. This is used for resolving any relative URIs appearing
        /// within the query, for example in references to library modules, schema
        /// locations, or as an argument to the <c>doc()</c> function.
        /// </summary>

        public Uri BaseUri {
            get => Implementation.getBaseURI();
            set => Implementation.setBaseURI(value);
        }

        /// <summary>
        /// Property indicating that the query is to be compiled to be schema-aware, even if it contains no
        /// "import schema" declarations. Normally a query is treated as schema-aware
        /// only if it contains one or more "import schema" declarations. 
        /// </summary>
        /// <remarks>
        /// <para>If the query is not schema-aware, then all input documents must be untyped 
        /// (or <c>xs:anyType</c>), and validation of temporary nodes is disallowed
        /// (though validation of the final result tree is permitted). Setting the argument to true
        /// means that schema-aware code will be compiled regardless.</para>
        /// </remarks>

        public bool SchemaAware {
            get => Implementation.isSchemaAware();
            set => Implementation.setSchemaAware(value);
        }

        /// <summary>
        /// This property indicates whether XQuery Update syntax is accepted. The default
        /// value is false. This property must be set to true before compiling a query that
        /// uses update syntax.
        /// </summary>
        /// <remarks>
        /// <para>This property must be set to true before any query can be compiled
        /// that uses updating syntax. This applies even if the query is not actually an updating
        /// query (for example, a copy-modify expression). XQuery Update syntax is accepted
        /// only by Saxon-EE. Non-updating queries are accepted regardless of the value of this
        /// property.</para>
        /// </remarks>


        public bool UpdatingEnabled {
            get => Implementation.isUpdatingEnabled();
            set => Implementation.setUpdatingEnabled(value);
        }
        
        /// <summary>
        /// Property indicating that the query is to be compiled to enable streaming, which is
        /// necessary if the query is to be executed using the method <c>XQueryEvaluator.RunStreamed()</c>.
        /// In streaming mode, the source document is supplied as a stream, and no tree is built
        /// in memory. The default is false.
        /// </summary>
        /// <remarks>
        /// <para>When the value is set to <c>true</c>, this has the additional side-effect of
        /// setting the required context item type to <c>document-node()</c>.</para>
        /// </remarks>

        public bool StreamingEnabled {
            get => Implementation.isStreaming();
            set => Implementation.setStreaming(value);
        }

        /// <summary>
        /// This property indicates which version of XQuery language syntax is accepted. The values
        /// accepted are "3.1" and "4.0". The default is "3.1".
        /// </summary>
        /// <remarks>
        /// <para>XQuery 4.0 is work in progress, and the specification is far from stable.</para>
        /// </remarks>

        public string XQueryLanguageVersion {
            get => Implementation.getLanguageVersion();
            set => Implementation.setLanguageVersion(value);
        }

        /// <summary>
        /// A user-supplied <c>XQueryResolver</c> used to resolve the URIs appearing in an
        /// <c>import module</c> declaration.
        /// </summary>
        /// <remarks>
        /// <para>If an <c>import module</c> declaration identifies a module URI that has already been loaded,
        /// then the existing module is used (regardless of location hints and regardless of the <c>XQueryResolver</c>),
        /// unless the configuration option <c>XQUERY_MULTIPLE_MODULE_IMPORTS</c> is set. If this option is set, then an existing
        /// module will be reused if the location hint matches; otherwise a new module is loaded (which can
        /// potentially cause a failure if it results in duplicate variables or functions having the same name).
        /// </para>
        /// <para>The <c>XQueryResolver</c> can return multiple library modules; if it does so, they are all
        /// imported. If it returns an empty array, nothing is imported.</para>
        /// <para>If the <c>XQueryResolver</c> returns null (as distinct from returning an empty array),
        /// then the <c>import module</c> declaration is resolved as if there were no user-supplied
        /// <c>XQueryResolver</c>.</para>
        /// <para>If there is no user-supplied <c>XQueryResolver</c>, and if the module has not already been
        /// loaded, then an <c>import module</c> declaration is interpreted as follows. First, if there are no location hints,
        /// then a static error is reported. Otherwise, each URI listed in the location hints is
        /// resolved using the default resolution algorithm for the <c>Processor</c>, and this is expected
        /// to deliver an input stream containing an XQuery library module.</para>
        /// <para>The interface changed in Saxon 11.1 so that the <c>XQueryResolver</c> now returns one or more
        /// <c>TextResource</c> objects. The previous class <c>XQueryModuleSource</c> no longer exists.
        /// </para>
        /// </remarks>

        public XQueryResolver XQueryResolver {
            get => this._moduleResolver;
            set {
                this._moduleResolver = value;
                Implementation.setModuleURIResolver(
                    new QueryResolverWrapper(value, this.Processor));
            }
        }

        /// <summary>Set the <c>ErrorReporter</c> to be used when validating instance documents as a user defined IErrorReporter.
        ///  Requirements here is IErrorReport implementation with a user-written <c>report()</c> method, it is possible to
        /// intercept error conditions as they occur.
        /// If this property is used then the ErrorList property and SetErrorList method is overriden.</summary>
        /// <remarks>The <c>IErrorReporter</c> to be used</remarks>
        public ErrorReporter ErrorReporter {
            get => this._errorReporter;
            set {
                this._errorReporter = null;
                Implementation.setErrorReporter(new ErrorReporterWrapper(value));
            }
        }

        /// <summary>
        /// Compile a query supplied as a <c>Stream</c>.
        /// </summary>
        /// <remarks>
        /// <para>The XQuery processor attempts to deduce the encoding of the query
        /// by looking for a byte-order-mark, or if none is present, by looking
        /// for the encoding declaration in the XQuery version declaration.
        /// For this to work, the stream must have the <c>CanSeek</c> property.
        /// If no encoding information is present, UTF-8 is assumed.</para>
        /// <para>The base URI of the query is set to the value of the <c>BaseUri</c>
        /// property. If this has not been set, then the base URI will be undefined, which
        /// means that any use of an expression that depends on the base URI will cause
        /// an error.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// XQueryExecutable q = compiler.Compile(new FileStream("input.xq", FileMode.Open, FileAccess.Read));
        /// </code>
        /// </example>
        /// <param name="query">A stream containing the source text of the query</param>
        /// <returns>An <c>XQueryExecutable</c> which represents the compiled query object.
        /// The <c>XQueryExecutable</c> may be run as many times as required, in the same or a different
        /// thread. The <c>XQueryExecutable</c> is not affected by any changes made to the <c>XQueryCompiler</c>
        /// once it has been compiled.</returns>
        /// <exception cref="SaxonApiException">Throws a <c>SaxonApiException</c> if errors were detected
        /// during static analysis of the query. Details of the errors will be reported to the
        /// <c>ErrorReporter</c> if supplied; otherwise they will be written to the standard
        /// error stream. The exception that is returned from this method is merely a summary indicating the
        /// status, and is not useful for diagnosing the actual error.</exception>
        public XQueryExecutable Compile(Stream query) {
            Saxon.Hej.s9api.XQueryExecutable exec = Implementation.compile(query);
            return new XQueryExecutable(Processor, exec);
        }

        /// <summary>
        /// Compile a query supplied as a <c>string</c>.
        /// </summary>
        /// <remarks>
        /// Using this method the query processor is provided with a string of Unicode
        /// characters, so no decoding is necessary. Any encoding information present in the
        /// version declaration is therefore ignored.
        /// </remarks>
        /// <example>
        /// <code>
        /// XQueryExecutable q = compiler.Compile("distinct-values(//*/node-name()");
        /// </code>
        /// </example>
        /// <param name="query">A string containing the source text of the query</param>
        /// <returns>An <c>XQueryExecutable</c> which represents the compiled query object.
        /// The <c>XQueryExecutable</c> may be run as many times as required, in the same or a different
        /// thread. The <c>XQueryExecutable</c> is not affected by any changes made to the <c>XQueryCompiler</c>
        /// once it has been compiled.</returns>
        /// <exception cref="SaxonApiException">Throws a <c>SaxonApiException</c> if errors were detected
        /// during static analysis of the query. Details of the errors will be reported to the
        /// <c>ErrorReporter</c> if supplied; otherwise they will be written to the standard
        /// error stream. The exception that is returned from this method is merely a summary indicating the
        /// status, and is not useful for diagnosing the actual error.</exception>
        public XQueryExecutable Compile(string query) {
            Saxon.Hej.s9api.XQueryExecutable exec = Implementation.compile(query);
            return new XQueryExecutable(Processor, exec);
        }
        
        /// <summary>
        /// Compile a library module supplied as a stream. The code generated by compiling the library is available
        /// for importing by all subsequent compilations using the same XQueryCompiler; it is identified by an
        /// "import module" declaration that specifies the module URI of the library module. No module location
        /// hint is required, and if one is present, it is ignored.
        /// </summary>
        /// <remarks>
        /// <para>The base URI of the query should be supplied by setting the <c>BaseURI</c>
        /// property of the <c>QueryCompiler</c>.</para>
        /// </remarks>
        /// <param name="query">A stream delivering the text of the query</param>
        /// <exception>SaxonApiException if the query compilation fails with a static error</exception>
        public void CompileLibrary(Stream query) {
            Implementation.compileLibrary(query);
        }

        /// <summary>
        /// Compile a library module supplied as a string. The code generated by compiling the library is available
        /// for importing by all subsequent compilations using the same XQueryCompiler; it is identified by an
        /// "import module" declaration that specifies the module URI of the library module. No module location
        /// hint is required, and if one is present, it is ignored.
        /// </summary>
        /// <remarks>
        /// <para>The base URI of the query should be supplied by setting the <c>BaseURI</c>
        /// property of the <c>QueryCompiler</c>.</para>
        /// </remarks>
        /// <param name="query">The text of the query</param>
        /// <exception>SaxonApiException if the query compilation fails with a static error</exception>
        public void CompileLibrary(string query) {
            Implementation.compileLibrary(query);
        }
        
        /// <summary>
        /// Escape hatch to the underlying Java implementation. The result gives access to internal
        /// methods that may vary from release to release.
        /// </summary>

        private Saxon.Hej.s9api.XQueryCompiler Implementation { get; }


        /// <summary>
        /// Property indicating that fast compilation is preferred.
        /// Fast compilation will generally be achieved at the expense of run-time performance
        /// and quality of diagnostics. Fast compilation is a good trade-off if (a) the expression is known to be correct,
        /// and (b) once compiled, the expression is only executed once against a document of modest size.
        /// </summary>
        /// <remarks>
        /// <para><i>The current implementation is equivalent to switching off all optimizations. Setting this option, however,
        /// indicates an intent rather than a mechanism, and the implementation details may change in future to reflect
        /// the intent.</i></para>
        /// <para>Set to true to request fast compilation; set to false to revert to the optimization options
        /// defined in the Configuration.</para>
        /// </remarks>

        public bool FastCompilation {
            set => Implementation.setFastCompilation(value);
            get => Implementation.isFastCompilation();
        }
        
        /// <summary>
        /// Property indicating that the query is to be compiled with tracing enabled.
        /// </summary>
        /// <remarks>
        /// <para>This allows tracing to be switched on at run-time, but it has no effect unless
        /// run-time tracing is activated.</para>
        /// </remarks>

        public bool CompileWithTracing {
            get => Implementation.isCompileWithTracing();
            set => Implementation.setCompileWithTracing(value);
        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited