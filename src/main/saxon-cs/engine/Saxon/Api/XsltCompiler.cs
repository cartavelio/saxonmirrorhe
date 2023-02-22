using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Impl.Helpers;
using System.IO;
using System.Xml;
using javax.xml.transform;
using javax.xml.transform.stream;
using net.sf.saxon;
using net.sf.saxon.@event;
using net.sf.saxon.om;
using net.sf.saxon.pull;
using net.sf.saxon.s9api;
using net.sf.saxon.style;
using net.sf.saxon.trans;
using net.sf.saxon.trans.packages;
using net.sf.saxon.tree.util;
using Saxon.Impl.Helpers;
using JXsltCompiler = net.sf.saxon.s9api.XsltCompiler;
using System = java.lang.System;

namespace Saxon.Api {
    /// <summary>
    /// An <c>XsltCompiler</c> object allows XSLT 3.0 stylesheets to be compiled.
    /// The compiler holds information that represents the static context
    /// for the compilation.
    /// </summary>
    /// <remarks>
    /// <para>To construct an <c>XsltCompiler</c>, use the factory method
    /// <c>NewXsltCompiler</c> on the <see cref="Processor"/> object.</para>
    /// <para>An <c>XsltCompiler</c> may be used repeatedly to compile multiple
    /// queries. Any changes made to the <c>XsltCompiler</c> (that is, to the
    /// static context) do not affect queries that have already been compiled.
    /// An <c>XsltCompiler</c> may be used concurrently in multiple threads, but
    /// it should not then be modified once initialized.</para>
    /// </remarks>
    [Serializable]
    public abstract class XsltCompiler {
        private Processor processor;
        private Uri baseUri;
        private ErrorReporterToStaticError errorGatherer;
        private ErrorReporter errorReporter;
        private IDictionary<QName, XdmValue> variableList = new Dictionary<QName, XdmValue>();
        private JXsltCompiler jXsltCompiler;
        private XmlResolver xmlResolver = null;

        // internal constructor: the public interface is a factory method
        // on the Processor object

        internal XsltCompiler(Processor processor) {
            this.processor = processor;
            jXsltCompiler = processor.JProcessor.newXsltCompiler();
            errorGatherer = new ErrorReporterToStaticError(new List<StaticError>());
            errorReporter = new ErrorReporter(new List<XmlProcessingError>());
            //xsltCompiler.setErrorReporter(errorReporter);
            jXsltCompiler.setURIResolver(processor.Implementation.getURIResolver());
        }

        /// <summary>
        /// The base URI of the stylesheet, which forms part of the static context
        /// of the stylesheet. This is used for resolving any relative URIs appearing
        /// within the stylesheet, for example in <c>xsl:include</c> and <c>xsl:import</c>
        /// declarations, in schema locations defined to <c>xsl:import-schema</c>, 
        /// or as an argument to the <c>document()</c> or <c>doc()</c> function.
        /// </summary>
        /// <remarks>
        /// This base URI is used only if the input supplied to the <c>Compile</c> method
        /// does not provide its own base URI. It is therefore used on the version of the
        /// method that supplies input from a <c>Stream</c>. On the version that supplies
        /// input from an <c>XmlReader</c>, this base URI is used only if the <c>XmlReader</c>
        /// does not have its own base URI.
        /// </remarks>


        public Uri BaseUri {
            get => baseUri;
            set => baseUri = value;
        }

        /// <summary>
        /// Create a collation based on a given <c>CompareInfo</c> and <c>CompareOptions</c>.    
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
        [Obsolete("Declare collations globally at the Processor level.")]
        public void DeclareCollation(Uri uri, CompareInfo compareInfo, CompareOptions options, Boolean isDefault) {
            JDotNetComparator comparator = new JDotNetComparator(uri.ToString(), compareInfo, options);
            processor.JProcessor.getUnderlyingConfiguration().registerCollation(uri.ToString(), comparator);
        }

        /// <summary>
        /// The name of the default collation used by stylesheets compiled using this <c>XsltCompiler</c>.
        /// This must be the name of a collation that is known to the <c>Processor</c>.
        /// </summary>

        public String DefaultCollationName {
            get => jXsltCompiler.getDefaultCollation();
            set => jXsltCompiler.declareDefaultCollation(value);
        }

        /// <summary>
        /// The <c>Processor</c> from which this <c>XsltCompiler</c> was constructed
        /// </summary>

        public Processor Processor => processor;

        /// <summary>
        /// An <c>XmlResolver</c>, which will be used to resolve URI references while compiling
        /// a stylesheet.
        /// </summary>
        /// <remarks>
        /// If no <c>XmlResolver</c> is set for the <c>XsltCompiler</c>, the <c>XmlResolver</c>
        /// that is used is the one that was set on the <c>Processor</c> at the time <c>NewXsltCompiler</c>
        /// was called.
        /// </remarks>

        public XmlResolver XmlResolver {
            get {
                if (xmlResolver == null) {
                    javax.xml.transform.URIResolver resolver =
                        jXsltCompiler.getUnderlyingCompilerInfo().getURIResolver();
                    if (resolver is DotNetURIResolver) {
                        return ((DotNetURIResolver) resolver).getXmlResolver();
                    }
                    else {
                        xmlResolver = new XmlUrlResolver();
                        return xmlResolver;
                    }
                }
                else {
                    return xmlResolver;
                }
            }
            set {
                xmlResolver = value;
                jXsltCompiler.getUnderlyingCompilerInfo().setURIResolver(new DotNetURIResolver(xmlResolver));
            }
        }

        /// <summary>
        /// The <c>SchemaAware</c> property determines whether the stylesheet is schema-aware. By default, a stylesheet
        /// is schema-aware if it contains one or more <code>xsl:import-schema</code> declarations. This option allows
        /// a stylesheet to be marked as schema-aware even if it does not contain such a declaration.
        /// </summary>
        /// <remarks>
        /// <para>If the stylesheet is not schema-aware, then schema-validated input documents will be rejected.</para>
        /// <para>The reason for this option is that it is expensive to generate code that can handle typed input
        /// documents when they will never arise in practice.</para>
        /// <para>The initial setting of this property is false, regardless of whether or not the <c>Processor</c>
        /// is schema-aware. Setting this property to true if the processor is not schema-aware will cause an Exception.</para>
        /// </remarks>

        public bool SchemaAware {
            get => jXsltCompiler.isSchemaAware();
            set => jXsltCompiler.setSchemaAware(value);
        }


        /// <summary>
        /// Indicates whether or not assertions (<c>xsl:assert</c> instructions) are enabled at compile time. 
        /// </summary>
        /// <remarks>By default assertions are disabled at compile time. If assertions are enabled at compile time, then by
        /// default they will also be enabled at run time; but they can be disabled at run time by
        /// specific request. At compile time, assertions can be enabled for some packages and
        /// disabled for others; at run time, they can only be enabled or disabled globally.</remarks>
        /// <returns>true if assertions are enabled at compile time</returns>


        public bool AssertionsEnabled {
            get => jXsltCompiler.isAssertionsEnabled();
            set => jXsltCompiler.setAssertionsEnabled(value);
        }

        /// <summary>
        /// The <c>XsltLanguageVersion</c> property determines the version of the XSLT language specification
        /// implemented by the compiler. In this Saxon release the value is always "3.0".
        /// </summary>
        /// <remarks>
        /// <para>Getting this property always returns "3.0".</para>
        /// <para>Setting this property has no effect.</para>
        /// </remarks>

        public string XsltLanguageVersion {
            get => "3.0";
            set {
                if (value == null) throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// This property determines whether bytecode is to be generated in the compiled stylesheet.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Bytecode generation is not available in Saxon-CS, attempting to set this property to true has no effect.
        /// </para>
        /// </remarks>
        /// <returns>true if bytecode is to be generated, false if not</returns>

        public bool ByteCodeEnabled {
            get => false;
            set { }
        }


        /// <summary>
        /// Property to say whether just-in-time compilation of template rules should be used.
        /// jit true if just-in-time compilation is to be enabled. With this option enabled,
        /// static analysis of a template rule is deferred until the first time that the
        /// template is matched.This can improve performance when many template
        /// rules are rarely used during the course of a particular transformation; however,
        /// it means that static errors in the stylesheet will not necessarily cause the
        /// <code>Compile(Source)</code> method to throw an exception (errors in code that is
        ///  actually executed will still be notified to the registered <code>ErrorList</code>
        ///  or <code>ErrorList</code>, but this may happen after the {
        /// <c>Compile(Source)</c>
        ///  method returns). This option is enabled by default in Saxon - EE, and is not available
        ///  in Saxon - HE or Saxon-PE.
        ///  <p><b> Recommendation:</b> disable this option unless you are confident that the
        ///  stylesheet you are compiling is error - free.</p>
        /// </summary>
        public bool JustInTimeCompilation {
            get => jXsltCompiler.isJustInTimeCompilation();
            set => jXsltCompiler.setJustInTimeCompilation(value);
        }


        /// <summary>
        /// List of errors. The caller should supply an empty list before calling Compile;
        /// the processor will then populate the list with error information obtained during
        /// the compilation. Each error will be included as an object of type <c>StaticError</c>.
        /// If no error list is supplied by the caller, error information will be written to
        /// an error list allocated by the system, which can be obtained as the value of this property.
        /// </summary>
        /// <remarks>
        /// By supplying a custom <c>List</c> with a user-written <c>add()</c> method, it is possible to
        /// intercept error conditions as they occur.
        /// </remarks>
        [Obsolete(
            "IList<StaticError> ErrorList is deprecated, please use the methods SetErrorList and GetErrorList which hanldes IList<XmlProcessingError>.")]
        public IList<StaticError> ErrorList {
            set {
                errorGatherer = new ErrorReporterToStaticError(value);
                jXsltCompiler.setErrorReporter(errorGatherer);
            }
            get => errorGatherer.ErrorList;
        }

        /// <summary>Set the <c>ErrorReporter</c> to be used when validating instance documents as a user defined IErrorReporter.
        /// If this property is used then the ErrorList property and SetErrorList method is overriden </summary>
        /// <remarks>The <c>IErrorReporter</c> to be used</remarks>
        public IErrorReporter ErrorReporter {
            set {
                errorReporter = null;
                jXsltCompiler.setErrorReporter(new ErrorReporterWrapper(value));
            }
        }


        /// <summary>
        /// List of errors. The caller may supply an empty list before calling <c>Compile</c>;
        /// the processor will then populate the list with error information obtained during
        /// the XSLT compilation. Each error will be included as an object of type <c>XmlProcessingError</c>.
        /// If no error list is supplied by the caller, error information will be written to
        /// the standard error stream.
        /// </summary>
        /// <remarks>
        /// <para>By supplying a custom <c>List</c> or IErrorReport implementation with a user-written <c>report()</c> method, it is possible to
        /// intercept error conditions as they occur.</para>
        /// <para>Note that this error list is used only for errors detected during the compilation
        /// of the stylesheet. It is not used for errors detected when executing the stylesheet.</para>
        /// </remarks>
        /// <param name="value">Supplied list.</param>
        public void SetErrorList(IList<XmlProcessingError> value) {
            errorReporter = new ErrorReporter(value);
            jXsltCompiler.setErrorReporter(errorReporter);
        }

        /// <summary>
        /// Get list of errors as <code>IList&lt;XmlProcessingError&gt;</code>
        /// </summary>
        public IList<XmlProcessingError> GetErrorList() {
            return errorReporter.ErrorList;
        }


        /// <summary>
        /// Compile a stylesheet supplied as a Stream.
        /// </summary>
        /// <example>
        /// <code>
        /// Stream source = new FileStream("input.xsl", FileMode.Open, FileAccess.Read);
        /// XsltExecutable q = compiler.Compile(source);
        /// source.Close();
        /// </code>
        /// </example>
        /// <param name="input">A stream containing the source text of the stylesheet</param>
        /// <returns>An <c>XsltExecutable</c> which represents the compiled stylesheet object.
        /// The <c>XsltExecutable</c> may be loaded as many times as required, in the same or a different
        /// thread. The <c>XsltExecutable</c> is not affected by any changes made to the <c>XsltCompiler</c>
        /// once it has been compiled.</returns>
        /// <remarks>
        /// <para>If the stylesheet contains any <c>xsl:include</c> or <c>xsl:import</c> declarations,
        /// then the <c>BaseUri</c> property must be set to allow these to be resolved.</para>
        /// <para>The stylesheet is contained in the part of the input stream between its current
        /// position and the end of the stream. It is the caller's responsibility to close the input 
        /// stream after use. If the compilation succeeded, then on exit the stream will be 
        /// exhausted; if compilation failed, the current position of the stream on exit is
        /// undefined.</para>
        /// </remarks>
        public XsltExecutable Compile(Stream input) {
            try {
                StreamSource ss = new StreamSource(input);
                if (baseUri != null) {
                    ss.setSystemId(Uri.EscapeUriString(baseUri.ToString()));
                }

                XsltExecutable executable = new XsltExecutable(jXsltCompiler.compile(ss));
                executable.InternalProcessor = processor;
                return executable;
            }
            catch (SaxonApiException err) {
                throw new StaticError(err);
            }
        }

        // internal method: Compile a stylesheet supplied as a Stream.
        // For example:
        // <code>
        // Stream source = new FileStream("input.xsl", FileMode.Open, FileAccess.Read);
        // XsltExecutable q = compiler.Compile(source);
        // source.Close();
        // </code>
        // <param name="input">A stream containing the source text of the stylesheet</param>
        // <param name="theBaseUri">Specify the base URI of the stream</param>
        // <param name="closeStream">Flag to indicate if the stream should be closed in the method</param>
        // <returns>An <c>XsltExecutable</c> which represents the compiled stylesheet object.
        // The XsltExecutable may be loaded as many times as required, in the same or a different
        // thread. The <c>XsltExecutable</c> is not affected by any changes made to the <c>XsltCompiler</c>
        // once it has been compiled.</returns>
        // <para>If the stylesheet contains any <c>xsl:include</c> or <c>xsl:import</c> declarations,
        // then the <c>BaseUri</c> property must be set to allow these to be resolved.</para>
        // <para>The stylesheet is contained in the part of the input stream between its current
        // position and the end of the stream. It is the caller's responsibility to close the input 
        // stream after use. If the compilation succeeded, then on exit the stream will be 
        // exhausted; if compilation failed, the current position of the stream on exit is
        // undefined.</para>

        internal XsltExecutable Compile(Stream input, String theBaseUri, bool closeStream) {
            //See bug 2306
            try {
                StreamSource ss = new StreamSource(input);
                if (theBaseUri != null) {
                    ss.setSystemId(Uri.EscapeUriString(theBaseUri.ToString()));
                }
                else {
                    if (baseUri != null) {
                        ss.setSystemId(Uri.EscapeUriString(baseUri.ToString()));
                    }
                }

                net.sf.saxon.s9api.XsltExecutable jexecutable = jXsltCompiler.compile(ss);

                if (closeStream) {
                    input.Close();
                }

                XsltExecutable executable = new XsltExecutable(jexecutable);
                executable.InternalProcessor = processor;
                return executable;
            }
            catch (SaxonApiException err) {
                throw new StaticError(err);
            }
        }


        /// <summary>Compile a library package.</summary>
        /// <remarks>
        /// <para>The source argument identifies an XML file containing an <c>xsl:package</c> element. Any packages
        /// on which this package depends must have been made available to the <c>XsltCompiler</c>
        /// by importing them using <see cref="ImportPackage"/>.</para>
        /// </remarks>
        /// <param name='input'>source identifies an XML document holding the the XSLT package to be compiled</param>
        /// <returns>The <c>XsltPackage</c> that results from the compilation. Note that this package
        /// is not automatically imported to this <c>XsltCompiler</c>; if the package is required
        /// for use in subsequent compilations then it must be explicitly imported.</returns>
        public XsltPackage CompilePackage(Stream input) {
            try {
                StreamSource ss = new StreamSource(input);
                if (baseUri != null) {
                    ss.setSystemId(Uri.EscapeUriString(baseUri.ToString()));
                }

                return new XsltPackage(processor, jXsltCompiler.compilePackage(ss));
            }
            catch (SaxonApiException err) {
                throw new StaticError(err);
            }
        }

        /// <summary>Import a library package. Calling this method makes the supplied package available for reference
        /// in the <code>xsl:use-package</code> declaration of subsequent compilations performed using this
        /// <code>XsltCompiler</code>.</summary>
        /// <param name='thePackage'> the package to be imported</param>
        /// <remarks>since 9.6</remarks>
        public void ImportPackage(XsltPackage thePackage) {
            if (thePackage.Processor != this.processor) {
                throw new StaticError(
                    new TransformerException(
                        "The imported package and the XsltCompiler must belong to the same Processor"));
            }

            GetUnderlyingCompilerInfo().getPackageLibrary().addPackage(thePackage.getUnderlyingPreparedPackage());
        }


        /// <summary>Import a library package. Calling this method makes the supplied package available for reference
        /// in the <code>xsl:use-package</code> declaration of subsequent compilations performed using this
        /// <code>XsltCompiler</code>.</summary>
        /// <param name='thePackage'> the package to be imported</param>
        /// <param name='packageName'> name of the package to be imported</param>
        /// <param name='version'> version identifier for the package to be imported</param>
        /// <remarks>since 9.8</remarks>
        public void ImportPackage(XsltPackage thePackage, string packageName, string version) {
            if (thePackage.Processor != this.processor) {
                throw new StaticError(
                    new TransformerException(
                        "The imported package and the XsltCompiler must belong to the same Processor"));
            }

            try {
                PackageDetails details = new PackageDetails();
                if (packageName == null) {
                    packageName = thePackage.PackageName;
                }

                if (version == null) {
                    version = thePackage.Version;
                }

                details.nameAndVersion = new VersionedPackageName(packageName, version);
                details.loadedPackage = thePackage.getUnderlyingPreparedPackage();
                jXsltCompiler.getUnderlyingCompilerInfo().getPackageLibrary().addPackage(details);
            }
            catch (TransformerException ex) {
                throw new StaticError(ex);
            }
        }


        /// <summary>
        /// Load a compiled package from a file or from a remote location.
        /// </summary>
        /// <remarks>
        /// The supplied URI represents the location of a resource which must have been originally
        /// created using <see cref="XsltPackage.Save(Stream)"/>.
        /// The result of loading the package is returned as an <code>XsltPackage</code> object.
        /// Note that this package is not automatically imported to this <code>XsltCompiler</code>;
        /// if the package is required for use in subsequent compilations then it must be explicitly
        /// imported.
        /// </remarks>
        /// <param name="location">the location from which the package is to be loaded, as a URI</param>
        /// <returns>the compiled package loaded from the supplied file or remote location</returns>
        public XsltPackage LoadLibraryPackage(Uri location) {
            try {
                net.sf.saxon.s9api.XsltPackage package =
                    jXsltCompiler.loadLibraryPackage(location);
                return new XsltPackage(processor, package);
            }
            catch (net.sf.saxon.trans.XPathException e) {
                throw new StaticError(e);
            }
        }

        /// <summary>
        /// Load a compiled package from a file or from a remote location, with the intent to use this as a complete
        /// executable stylesheet, not as a library package.
        /// </summary>
        /// <remarks>
        /// The supplied URI represents the location of a resource which must have been originally
        /// created using <see cref="XsltPackage.Save(Stream)"/>.
        /// </remarks>
        /// <param name="location"> the location from which the package is to be loaded, as a URI</param>
        /// <returns>the compiled package loaded from the supplied file or remote location</returns>
        public XsltExecutable LoadExecutablePackage(Uri location) {
            XsltExecutable executable = LoadLibraryPackage(location).Link();
            executable.InternalProcessor = processor;
            return executable;
        }


        ///	<summary>  
        /// Get the underlying <c>CompilerInfo</c> object, which provides more detailed (but less stable) control
        /// over some compilation options
        /// </summary>
        /// <returns> the underlying <c>CompilerInfo</c> object, which holds compilation-time options. The methods on
        /// this object are not guaranteed stable from release to release.
        /// </returns>
        public CompilerInfo GetUnderlyingCompilerInfo() {
            return jXsltCompiler.getUnderlyingCompilerInfo();
        }


        /// <summary>
        /// Escape hatch to the underlying Java-derived implementation of the <c>XsltCompiler</c>
        /// </summary>

        public net.sf.saxon.s9api.XsltCompiler Implementation {
            get { return jXsltCompiler; }
        }


        /// <summary>
        /// Externally set the value of a static parameter (new facility in XSLT 3.0) 
        /// </summary>
        /// <param name="name">The name of the parameter, expressed
        /// as a QName. If a parameter of this name has been declared in the
        /// stylesheet, the given value will be assigned to the variable. If the
        /// variable has not been declared, calling this method has no effect (it is
        /// not an error).</param>
        /// <param name="value">The value to be given to the parameter.
        /// If the parameter declaration defines a required type for the variable, then
        /// this value will be converted in the same way as arguments to function calls
        /// (for example, numeric promotion is applied).</param>
        public void SetParameter(QName name, XdmValue value) {
            if (value == null) {
                if (variableList.ContainsKey(name)) {
                    variableList.Remove(name);
                    jXsltCompiler.getUnderlyingCompilerInfo().setParameter(name.GetStructuredQName(), null);
                }
            }
            else {
                variableList[name] = value;
                jXsltCompiler.getUnderlyingCompilerInfo().setParameter(name.GetStructuredQName(), value.value);
            }
        }


        /// <summary>
        /// Clear the values of all stylesheet parameters previously set using <c>SetParameter(QName, XdmValue)</c>.
        /// This resets the parameters to their initial ("undeclared") state
        /// </summary>
        public void ClearParameters() {
            jXsltCompiler.clearParameters();
        }

        /// <summary>
        /// Property to check and set fast compilation. Fast compilation will generally be achieved at the expense of run-time performance
        /// and quality of diagnostics. Fast compilation is a good trade-off if (a) the stylesheet is known to be correct,
        /// and (b) once compiled, it is only executed once against a document of modest size.
        /// </summary>
        /// <remarks>
        /// <para>Fast compilation may result in static errors going unreported, especially if they occur in code
        /// that is never executed.</para>
        /// <para><i>The current implementation is equivalent to switching off all optimizations other than just-in-time
        /// compilation of template rules. Setting this option, however, indicates an intent rather than a mechanism,
        /// and the implementation details may change in future to reflect the intent.</i></para>
        /// <para>Set to true to request fast compilation; set to false to revert to the optimization options
        /// defined in the Configuration.</para>
        /// </remarks>

        public bool FastCompilation {
            set => jXsltCompiler.setFastCompilation(value);
            get => jXsltCompiler.isFastCompilation();
        }
        
        /// <summary>
        /// Compile a stylesheet supplied as a <c>TextReader</c>.
        /// </summary>
        /// <example>
        /// <code>
        /// String ss = "<![CDATA[<xsl:stylesheet version='2.0'>....</xsl:stylesheet>]]>";
        /// TextReader source = new StringReader(ss);
        /// XsltExecutable q = compiler.Compile(source);
        /// source.Close();
        /// </code>
        /// </example>
        /// <param name="input">A <c>TextReader</c> containing the source text of the stylesheet</param>
        /// <returns>An <c>XsltExecutable</c> which represents the compiled stylesheet object.
        /// The <c>XsltExecutable</c> may be loaded as many times as required, in the same or a different
        /// thread. The <c>XsltExecutable</c> is not affected by any changes made to the <c>XsltCompiler</c>
        /// once it has been compiled.</returns>
        /// <remarks>
        /// <para>If the stylesheet contains any <c>xsl:include</c> or <c>xsl:import</c> declarations,
        /// then the <c>BaseUri</c> property must be set to allow these to be resolved.</para>
        /// <para>The stylesheet is contained in the part of the input stream between its current
        /// position and the end of the stream. It is the caller's responsibility to close the 
        /// <c>TextReader</c> after use. If the compilation succeeded, then on exit the stream will be 
        /// exhausted; if compilation failed, the current position of the stream on exit is
        /// undefined.</para>
        /// </remarks>
        public XsltExecutable Compile(TextReader input) {
            try {
                StreamSource ss = new StreamSource(input);
                if (baseUri != null) {
                    ss.setSystemId(Uri.EscapeUriString(baseUri.ToString()));
                }

                XsltExecutable executable =
                    new XsltExecutable(jXsltCompiler.compile(ss)) {InternalProcessor = processor};
                return executable;
            }
            catch (SaxonApiException ex) {
                throw new StaticError(ex);
            }
        }

        /// <summary>
        /// Compile a stylesheet, retrieving the source using a URI.
        /// </summary>
        /// <remarks>
        /// The document located via the URI is parsed using the <c>System.Xml</c> parser. This
        /// URI is used as the base URI of the stylesheet: the <c>BaseUri</c> property of the
        /// <c>Compiler</c> is ignored.
        /// </remarks>
        /// <param name="uri">The URI identifying the location where the stylesheet document can be
        /// found</param>
        /// <returns>An <c>XsltExecutable</c> which represents the compiled stylesheet object.
        /// The <c>XsltExecutable</c> may be run as many times as required, in the same or a different
        /// thread. The <c>XsltExecutable</c> is not affected by any changes made to the <c>XsltCompiler</c>
        /// once it has been compiled.</returns>
        public XsltExecutable Compile(Uri uri) {
            Object obj = XmlResolver.GetEntity(uri, "application/xml", Type.GetType("System.IO.Stream"));
            if (obj is Stream) {
                // See bug issue #2306
                XsltExecutable executable = Compile((Stream) obj, uri.ToString(), true);
                executable.InternalProcessor = processor;
                return executable;
            }
            else {
                throw new ArgumentException("Invalid type of result from XmlResolver.GetEntity: " + obj);
            }
        }

        /// <summary>
        /// Compile a stylesheet, delivered using an <c>XmlReader</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <c>XmlReader</c> is responsible for parsing the document; this method builds a tree
        /// representation of the document (in an internal Saxon format) and compiles it.
        /// The <c>XmlReader</c> will be used as supplied; it is the caller's responsibility to
        /// ensure that the settings of the <c>XmlReader</c> are consistent with the requirements
        /// of the XSLT specification (for example, that entity references are expanded and whitespace
        /// is preserved).
        /// </para>
        /// <para>
        /// If the <c>XmlReader</c> has a <c>BaseUri</c> property, then that property determines
        /// the base URI of the stylesheet module, which is used when resolving any <c>xsl:include</c>
        /// or <c>xsl:import</c> declarations. If the <c>XmlReader</c> has no <c>BaseUri</c>
        /// property, then the <c>BaseUri</c> property of the <c>Compiler</c> is used instead.
        /// An <c>ArgumentNullException</c> is thrown if this property has not been supplied.
        /// </para>
        /// </remarks>
        /// <param name="reader">The XmlReader (that is, the XML parser) used to supply the document containing
        /// the principal stylesheet module.</param>
        /// <returns>An <c>XsltExecutable</c> which represents the compiled stylesheet object.
        /// The XsltExecutable may be run as many times as required, in the same or a different
        /// thread. The <c>XsltExecutable</c> is not affected by any changes made to the <c>XsltCompiler</c>
        /// once it has been compiled.</returns>
        /// 
        public XsltExecutable Compile(XmlReader reader) {
            DotNetPullProvider pp = new DotNetPullProvider(reader);
            PipelineConfiguration pipe = processor.Implementation.makePipelineConfiguration();
            pp.setPipelineConfiguration(pipe);
            PullSource source = new PullSource(pp);
            String baseu = reader.BaseURI;
            if (baseu == null || baseu == String.Empty) {
                // if no baseURI is supplied by the XmlReader, use the one supplied to this Compiler
                if (baseUri == null) {
                    throw new ArgumentNullException("BaseUri");
                }

                baseu = Uri.EscapeUriString(baseUri.ToString());
                pp.setBaseURI(baseu);
            }

            source.setSystemId(baseu);
            try {
                XsltExecutable executable = new XsltExecutable(jXsltCompiler.compile(source));
                executable.InternalProcessor = processor;
                return executable;
            }
            catch (SaxonApiException e) {
                throw new StaticError(e);
            }
        }

        /// <summary>
        /// Compile a stylesheet, located at an <c>XdmNode</c>. This may be a document node whose
        /// child is an <c>xsl:stylesheet</c> or <c>xsl:transform</c> element, or it may be
        /// the <c>xsl:stylesheet</c> or <c>xsl:transform</c> element itself.
        /// </summary>
        /// <param name="node">The document node or the outermost element node of the document
        /// containing the principal stylesheet module.</param>
        /// <returns>An <c>XsltExecutable</c> which represents the compiled stylesheet object.
        /// The <c>XsltExecutable</c> may be run as many times as required, in the same or a different
        /// thread. The <c>XsltExecutable</c> is not affected by any changes made to the <c>XsltCompiler</c>
        /// once it has been compiled.</returns>
        public XsltExecutable Compile(XdmNode node) {
            XsltExecutable executable = new XsltExecutable(jXsltCompiler.compile(node.Implementation));
            executable.InternalProcessor = processor;
            return executable;
        }

        /// <summary>Locate and compile a stylesheet identified by an <c>&lt;?xml-stylesheet?&gt;</c>
        /// processing instruction within a source document, and that match the given criteria.
        /// </summary>
        /// <param name="uri">The URI of the source document containing the xml-stylesheet processing instruction.</param>
        /// <param name="media">The media attribute to be matched. May be null, in which case the 
        /// "application/xml" mime type will be used when fetching the source document from the Uri.</param>
        /// <returns>An <c>XsltExecutable</c> which represents the compiled stylesheet object.</returns>
        /// <remarks>There are some limitations in the current implementation. Parameters of the xml-stylesheet 
        /// instruction other than the media type, are ignored. The
        /// <c>href</c> attribute must either reference an embedded stylesheet within the same
        /// document or a non-embedded external stylesheet.</remarks>
        public XsltExecutable CompileAssociatedStylesheet(Uri uri, String media) {
            Object obj = XmlResolver.GetEntity(uri, media == null ? "application/xml" : media,
                Type.GetType("System.IO.Stream"));
            if (obj is Stream) {
                StreamSource ss = new StreamSource((Stream) obj);

                ss.setSystemId(Uri.EscapeUriString(uri.ToString()));
                try {
                    // See bug issue #2306
                    XsltExecutable executable =
                        new XsltExecutable(
                            jXsltCompiler.compile(jXsltCompiler.getAssociatedStylesheet(ss, media, null, null)));
                    executable.InternalProcessor = processor;
                    return executable;
                }
                catch (SaxonApiException e) {
                    throw new StaticError(e);
                }
            }
            else {
                throw new ArgumentException("Invalid type of result from XmlResolver.GetEntity: " + obj);
            }
        }

        /// <summary>Locate and compile a stylesheet identified by an <c>&lt;?xml-stylesheet?&gt;</c>
        /// processing instruction within a source document.
        /// </summary>
        /// <param name="source">The document node of the source document containing the
        /// xml-stylesheet processing instruction.</param>
        /// <returns>An <c>XsltExecutable</c> which represents the compiled stylesheet object.</returns>
        /// <remarks>There are some limitations in the current implementation. The media type
        /// is ignored, as are the other parameters of the xml-stylesheet instruction. The
        /// <c>href</c> attribute must either reference an embedded stylesheet within the same
        /// document or a non-embedded external stylesheet.</remarks>
        public XsltExecutable CompileAssociatedStylesheet(XdmNode source) {
            // TODO: lift the restrictions
            if (source == null || source.NodeKind != XmlNodeType.Document) {
                throw new ArgumentException("Source must be a document node");
            }

            IEnumerator kids = source
                .Children(Predicates.IsProcessingInstruction().And(Predicates.HasName("", "xml-stylesheet")))
                .GetEnumerator();
            while (kids.MoveNext()) {
                XdmNode n = (XdmNode) kids.Current;
                // TODO: check the media type
                String href = ProcInstParser.getPseudoAttribute(n.StringValue, "href");
                if (href == null) {
                    throw new DynamicError("xml-stylesheet processing instruction has no href attribute");
                }

                String fragment = null;
                int hash = href.LastIndexOf('#');
                if (hash == 0) {
                    if (href.Length == 1) {
                        throw new DynamicError("Relative URI of '#' is invalid");
                    }

                    fragment = href.Substring(1);
                    NodeInfo target = ((TreeInfo) source.value).selectID(fragment, true);
                    XdmNode targetWrapper = null;
                    if (target == null) {
                        // There's a problem here because the Microsoft XML parser doesn't
                        // report id values, so selectID() will never work. We work around that
                        // by looking for an attribute named "id" appearing on an xsl:stylesheet
                        // or xsl:transform element
                        QName qid = new QName("", "id");
                        IEnumerator en = source.EnumerateAxis(XdmAxis.Descendant);
                        while (en.MoveNext()) {
                            XdmNode x = (XdmNode) en.Current;
                            if (x.NodeKind == XmlNodeType.Element &&
                                x.NodeName.Uri == "http://www.w3.org/1999/XSL/Transform" &&
                                (x.NodeName.LocalName == "stylesheet" || x.NodeName.LocalName == "transform" &&
                                    x.GetAttributeValue(qid) == fragment)) {
                                targetWrapper = x;
                            }
                        }
                    }
                    else {
                        targetWrapper = (XdmNode) XdmValue.Wrap(target);
                        targetWrapper.SetProcessor(processor);
                    }

                    if (targetWrapper == null) {
                        throw new DynamicError("No element with id='" + fragment + "' found");
                    }

                    return Compile(targetWrapper);
                }
                else if (hash > 0) {
                    throw new NotImplementedException(
                        "href cannot identify an embedded stylesheet in a different document");
                }
                else {
                    Uri uri = new Uri(n.BaseUri, href);
                    return Compile(uri);
                }
            }

            throw new DynamicError("xml-stylesheet processing instruction not found");
        }
    }
}