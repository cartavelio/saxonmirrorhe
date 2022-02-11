using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using net.sf.saxon;
using net.sf.saxon.s9api;

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
        private Configuration config;
        private Processor processor;
        private IQueryResolver moduleResolver;
        private IList<StaticError> errorList;
        private ErrorReporterToStaticError errorGatherer;
        private ErrorReporter errorReporter;
        private net.sf.saxon.s9api.XQueryCompiler compiler;

        // internal constructor: the public interface is a factory method
        // on the Processor object

        internal XQueryCompiler(Processor processor) {
            this.processor = processor;
            this.config = processor.Implementation;
            compiler = processor.JProcessor.newXQueryCompiler();
            compiler.setModuleURIResolver(new JDotNetStandardModuleURIResolver(processor.XmlResolver));
        }

        /// <summary>
        /// Create a collation based on a given <c>CompareInfo</c> and <c>CompareOptions</c>    
        /// </summary>
        /// <remarks>
        /// In the current and recent releases of Saxon, collations are always defined at the level of a <c>Configuration</c>.
        /// Declaring a collation here may therefore have wider effects than intended. It is recommended not to use
        /// this method, but to use <see cref="Processor.DeclareCollation(Uri, CompareInfo, CompareOptions)"/> instead.
        /// </remarks>
        /// <param name="uri">The collation URI to be used within the XPath expression to refer to this collation</param>
        /// <param name="compareInfo">The <c>CompareInfo</c>, which determines the language-specific
        /// collation rules to be used</param>
        /// <param name="options">Options to be used in performing comparisons, for example
        /// whether they are to be case-blind and/or accent-blind</param>
        /// <param name="isDefault">If true, this collation will be used as the default collation</param>
        public void DeclareCollation(Uri uri, CompareInfo compareInfo, CompareOptions options, Boolean isDefault) {
            JDotNetComparator comparator = new JDotNetComparator(uri.ToString(), compareInfo, options);
            config.registerCollation(uri.ToString(), comparator);
            if (isDefault) {
                compiler.declareDefaultCollation(uri.ToString());
            }
        }


        /// <summary>
        /// Declare a namespace for use by the query. This has the same
        /// status as a namespace appearing within the query prolog (though
        /// a declaration in the query prolog of the same prefix will take
        /// precedence).
        /// </summary>
        /// <param name="prefix">The namespace prefix to be declared. Use
        /// a zero-length string to declare the default namespace (that is, the
        /// default namespace for elements and types).</param>
        /// <param name="uri">The namespace URI. It is possible to specify
        /// a zero-length string to "undeclare" a namespace.</param>
        public void DeclareNamespace(String prefix, String uri) {
            compiler.declareNamespace(prefix, uri);
        }


        /// <summary>
        /// Get the <c>Processor</c> from which this <c>XQueryCompiler</c> was constructed
        /// </summary>

        public Processor Processor => processor;

        /// <summary>
        /// The required context item type for the expression. This is used for
        /// optimizing the expression at compile time, and to check at run-time
        /// that the value supplied for the context item is the correct type.
        /// </summary>

        public XdmItemType ContextItemType {
            get { return XdmItemType.MakeXdmItemType(compiler.getRequiredContextItemType().getUnderlyingItemType()); }
            set { compiler.setRequiredContextItemType(value.Unwrap()); }
        }

        /// <summary>
        /// The base URI of the query, which forms part of the static context
        /// of the query. This is used for resolving any relative URIs appearing
        /// within the query, for example in references to library modules, schema
        /// locations, or as an argument to the <c>doc()</c> function.
        /// </summary>


        public String BaseUri {
            get => compiler.getBaseURI().OriginalString;
            set => compiler.setBaseURI(new Uri(value));
        }


        /// <summary>
        /// Say that the query must be compiled to be schema-aware, even if it contains no
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
            get => compiler.isSchemaAware();
            set => compiler.setSchemaAware(value);
        }

        /// <summary>
        /// This property indicates whether XQuery Update syntax is accepted. The default
        /// value is false. This property must be set to true before compiling a query that
        /// uses update syntax.
        /// </summary>
        /// <remarks>
        /// <para>This propery must be set to true before any query can be compiled
        /// that uses updating syntax. This applies even if the query is not actually an updating
        /// query (for example, a copy-modify expression). XQuery Update syntax is accepted
        /// only by Saxon-EE. Non-updating queries are accepted regardless of the value of this
        /// property.</para>
        /// </remarks>


        public bool UpdatingEnabled {
            get => compiler.isUpdatingEnabled();
            set => compiler.setUpdatingEnabled(value);
        }

        /// <summary>
        /// This property indicates which version of XQuery language syntax is accepted. In this version
        /// of Saxon the version is always "3.1"; any attempt to set a different value is ignored.
        /// </summary>

        public string XQueryLanguageVersion {
            get => "3.1";
            set { }
        }


        /// <summary>
        /// A user-supplied <c>IQueryResolver</c> used to resolve location hints appearing in an
        /// <c>import module</c> declaration.
        /// </summary>
        /// <remarks>
        /// <para>In the absence of a user-supplied <c>QueryResolver</c>, an <c>import module</c> declaration
        /// is interpreted as follows. First, if the module URI identifies an already loaded module, that module
        /// is used and the location hints are ignored. Otherwise, each URI listed in the location hints is
        /// resolved using the <c>XmlResolver</c> registered with the <c>Processor</c>.</para>
        /// </remarks>

        public IQueryResolver QueryResolver {
            get => moduleResolver;
            set {
                moduleResolver = value;
                compiler.setModuleURIResolver(new DotNetModuleURIResolver(value));
            }
        }

        /// <summary>
        /// List of errors. The caller should supply an empty list before calling <c>Compile()</c>;
        /// the processor will then populate the list with error information obtained during
        /// the compilation. Each error will be included as an object of type <c>StaticError</c>.
        /// If no error list is supplied by the caller, error information will be written to
        /// the standard error stream.
        /// </summary>
        /// <remarks>
        /// By supplying a custom <c>List</c> with a user-written <c>add()</c> method, it is possible to
        /// intercept error conditions as they occur.
        /// </remarks>
        [Obsolete(
            "IList<StaticError> ErrorList is deprecated, please use the methods SetErrorList and GetErrorList which handle IList<XmlProcessingError>.")]
        public IList<StaticError> ErrorList {
            set {
                errorGatherer = new ErrorReporterToStaticError(value);
                compiler.setErrorReporter(errorGatherer);
            }
            get { return errorList; }
        }


        /// <summary>Set the <c>ErrorReporter</c> to be used when validating instance documents as a user defined IErrorReporter.
        ///  Requirements here is IErrorReport implementation with a user-written <c>report()</c> method, it is possible to
        /// intercept error conditions as they occur.
        /// If this property is used then the ErrorList property and SetErrorList method is overriden.</summary>
        /// <remarks>The <c>IErrorReporter</c> to be used</remarks>
        public IErrorReporter ErrorReporter {
            set {
                errorReporter = null;
                compiler.setErrorReporter(new ErrorReporterWrapper(value));
            }
        }

        /// <summary>
        /// List of errors. The caller may supply an empty list before calling <c>Compile</c>;
        /// the processor will then populate the list with error information obtained during
        /// the query compilation. Each error will be included as an object of type <c>XmlProcessingError</c>.
        /// If no error list is supplied by the caller, error information will be written to
        /// the standard error stream.
        /// </summary>
        /// <remarks>
        /// <para>By supplying a custom <c>List</c> with a user-written <c>add()</c> method, it is possible to
        /// intercept error conditions as they occur.</para>
        /// <para>Note that this error list is used only for errors detected during the compilation
        /// of the stylesheet. It is not used for errors detected when executing the stylesheet.</para>
        /// </remarks>
        /// <param name="value">Supplied list.</param>
        public void SetErrorList(IList<XmlProcessingError> value) {
            errorReporter = new ErrorReporter(value);
            compiler.setErrorReporter(errorReporter);
        }

        /// <summary>
        /// Get list of errors as <code>IList&lt;XmlProcessingError&gt;</code>
        /// </summary>
        public IList<XmlProcessingError> GetErrorList() {
            if (errorReporter == null) {
                return new List<XmlProcessingError>();
            }

            return errorReporter.ErrorList;
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
        /// <exception cref="StaticError">Throws a <c>StaticError</c> if errors were detected
        /// during static analysis of the query. Details of the errors will be added as <c>StaticError</c>
        /// objects to the <c>ErrorList</c> if supplied; otherwise they will be written to the standard
        /// error stream. The exception that is returned is merely a summary indicating the
        /// status.</exception>
        public XQueryExecutable Compile(Stream query) {
            try {
                net.sf.saxon.s9api.XQueryExecutable exec = compiler.compile(query);
                return new XQueryExecutable(exec);
            }
            catch (SaxonApiException e) {
                throw new StaticError(e);
            }
        }

        /// <summary>
        /// Compile a query supplied as a <c>String</c>.
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
        /// <exception cref="StaticError">Throws a <c>StaticError</c> if errors were detected
        /// during static analysis of the query. Details of the errors will be added as <c>StaticError</c>
        /// objects to the <c>ErrorList</c> if supplied; otherwise they will be written to the standard
        /// error stream. The exception that is returned is merely a summary indicating the
        /// status.</exception>        
        public XQueryExecutable Compile(String query) {
            try {
                net.sf.saxon.s9api.XQueryExecutable exec = compiler.compile(query);
                return new XQueryExecutable(exec);
            }
            catch (SaxonApiException e) {
                throw new StaticError(e);
            }
        }

        /// <summary>
        /// Escape hatch to the underlying Java implementation
        /// </summary>

        public net.sf.saxon.s9api.XQueryCompiler Implementation {
            get { return compiler; }
        }


        /// <summary>
        /// Request fast compilation. Fast compilation will generally be achieved at the expense of run-time performance
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

        public bool FastCompliation {
            set { compiler.setFastCompilation(value); }
            get { return compiler.isFastCompilation(); }
        }
    }
}