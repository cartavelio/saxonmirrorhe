using System;
using System.Collections;
using System.Collections.Generic;
using System.Impl.Helpers;
using System.IO;
using System.Xml;
using Saxon.Ejavax.xml.transform.stream;
using Saxon.Hej.@event;
using Saxon.Hej.om;
using Saxon.Hej.pull;
using Saxon.Hej.trans;
using Saxon.Hej.trans.packages;
using Saxon.Api.Support;
using Saxon.Callbacks;
using Saxon.Impl.Helpers;
using JXsltCompiler = Saxon.Hej.s9api.XsltCompiler;

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
    public class XsltCompiler {
        private Processor processor;
        private Uri baseUri;
        private IDictionary<QName, XdmValue> variableList = new Dictionary<QName, XdmValue>();
        private JXsltCompiler jXsltCompiler;
        private ResourceResolver resourceResolver;
        private ErrorReporter _errorReporter;

        // internal constructor: the public interface is a factory method
        // on the Processor object

        internal XsltCompiler(Processor processor) {
            this.processor = processor;
            jXsltCompiler = processor.JProcessor.newXsltCompiler();
            jXsltCompiler.getUnderlyingCompilerInfo().setResourceResolver(processor.Implementation.getResourceResolver());
            resourceResolver = null;
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
        /// The name of the default collation used by stylesheets compiled using this <c>XsltCompiler</c>.
        /// This must be the name of a collation that is known to the <c>Processor</c>.
        /// </summary>

        public string DefaultCollationName {
            get => jXsltCompiler.getDefaultCollation();
            set => jXsltCompiler.declareDefaultCollation(value);
        }

        /// <summary>
        /// The <c>Processor</c> from which this <c>XsltCompiler</c> was constructed
        /// </summary>

        public Processor Processor => processor;

        /// <summary>
        /// A <c>ResourceResolver</c>, which will be used to resolve URI references appearing
        /// in <c>xsl:include</c> and <c>xsl:import</c> declarations within a stylesheet.
        /// </summary>
        /// <remarks>
        /// <para>If no <c>StylesheetModuleResolver</c> is set for the <c>XsltCompiler</c>, or if the
        /// supplied <c>ResourceResolver</c> returns null, then the relevant URIs will be resolved using
        /// the common resolver defined at the Saxon Configuration level; failing this, the system falls
        /// back to default mechanisms for URI resolution. 
        /// </para>
        /// </remarks>

        public ResourceResolver StylesheetModuleResolver {
            get => resourceResolver;
            set {
                resourceResolver = value;
                jXsltCompiler.setResourceResolver(new JResourceResolverWrappingCS(value, processor.Implementation));
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
        /// Indicates whether or not any compiled package produced by this compiler is relocatable. If a package
        /// is relocatable, then any run-time operation that depends on the static base URI (for example, the
        /// <c>fn:doc</c> function) uses the deployed location of the package, not the location of the original
        /// source code.
        /// </summary>

        public bool Relocatable {
            get => jXsltCompiler.isRelocatable();
            set => jXsltCompiler.setRelocatable(value);
        }

        /// <summary>
        /// Indicates the Saxon edition under which any compiled package is intended to execute. The value
        /// should be set to "JS" when compiling packages intended for execution under SaxonJS 2.0 or later.
        /// </summary>
        /// 
        public string TargetEdition {
            get => jXsltCompiler.getTargetEdition();
            set => jXsltCompiler.setTargetEdition(value);
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
            get => jXsltCompiler.getXsltLanguageVersion();
            set => jXsltCompiler.setXsltLanguageVersion(value);
        }
        
        /// <summary>
        /// Property to say whether just-in-time compilation of template rules should be used.
        /// Set to true if just-in-time compilation is to be enabled. With this option enabled,
        /// static analysis of a template rule is deferred until the first time that the
        /// template is matched.This can improve performance when many template
        /// rules are rarely used during the course of a particular transformation; however,
        /// it means that static errors in the stylesheet will not necessarily cause the
        /// <code>Compile(Source)</code> method to throw an exception (errors in code that is
        ///  actually executed will still be notified to the registered <code>ErrorReporter</code>
        ///  or <code>ErrorList</code>, but this may happen after the {
        /// <c>Compile(Source)</c>
        ///  method returns). This option is enabled by default in Saxon-EE, and is not available
        ///  in Saxon-HE or Saxon-PE.
        ///  <p><b> Recommendation:</b> disable this option unless you are confident that the
        ///  stylesheet you are compiling is error-free.</p>
        /// </summary>
        public bool JustInTimeCompilation {
            get => jXsltCompiler.isJustInTimeCompilation();
            set => jXsltCompiler.setJustInTimeCompilation(value);
        }

        /// <summary>
        /// The <c>ErrorReporter</c> to be used when reporting static errors in a stylesheet.
        /// </summary>
        public ErrorReporter ErrorReporter {
            get => this._errorReporter;
            set {
                this._errorReporter = value;
                jXsltCompiler.setErrorReporter(new ErrorReporterWrapper(value));
            }
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
        /// <exception cref="SaxonApiException">Throws a <c>SaxonApiException</c> if errors were detected
        /// during static analysis of the stylesheet. Details of the errors will be reported to the
        /// <c>ErrorReporter</c> if supplied; otherwise they will be written to the standard
        /// error stream. The exception that is returned from this method is merely a summary indicating the
        /// status, and is not useful for diagnosing the actual error.</exception> 
        public XsltExecutable Compile(Stream input) {
            StreamSource ss = new StreamSource(input);
            if (baseUri != null) {
                ss.setSystemId(Uri.EscapeUriString(baseUri.ToString()));
            }

            XsltExecutable executable = new XsltExecutable(jXsltCompiler.compile(ss));
            executable.InternalProcessor = processor;
            return executable;
        }
        
        /// <summary>
        /// Compile a stylesheet supplied as a Stream, along with a base URI.
        /// </summary>
        /// <example>
        /// <code>
        /// Stream source = new FileStream("input.xsl", FileMode.Open, FileAccess.Read);
        /// XsltExecutable q = compiler.Compile(source, new Uri();
        /// source.Close();
        /// </code>
        /// </example>
        /// <param name="input">A stream containing the source text of the stylesheet</param>
        /// <param name="baseUri">The base URI of the stylesheet, used for resolving
        /// any relative URIs in <c>xsl:include</c> and <c>xsl:import</c> declarations,
        /// and anywhere else that the static base URI of the stylesheet is relevant.</param>
        /// <returns>An <c>XsltExecutable</c> which represents the compiled stylesheet object.
        /// The <c>XsltExecutable</c> may be loaded as many times as required, in the same or a different
        /// thread. The <c>XsltExecutable</c> is not affected by any changes made to the <c>XsltCompiler</c>
        /// once it has been compiled.</returns>
        /// <remarks>
        /// <para>The <c>BaseUri</c> property of the <c>XsltCompiler</c> is ignored.</para>
        /// <para>The stylesheet is contained in the part of the input stream between its current
        /// position and the end of the stream. It is the caller's responsibility to close the input 
        /// stream after use. If the compilation succeeded, then on exit the stream will be 
        /// exhausted; if compilation failed, the current position of the stream on exit is
        /// undefined.</para>
        /// </remarks>
        /// <exception cref="SaxonApiException">Throws a <c>SaxonApiException</c> if errors were detected
        /// during static analysis of the stylesheet. Details of the errors will be reported to the
        /// <c>ErrorReporter</c> if supplied; otherwise they will be written to the standard
        /// error stream. The exception that is returned from this method is merely a summary indicating the
        /// status, and is not useful for diagnosing the actual error.</exception> 
        public XsltExecutable Compile(Stream input, Uri baseUri) {
            StreamSource ss = new (input, baseUri.OriginalString);
            XsltExecutable executable = new(jXsltCompiler.compile(ss)) {
                InternalProcessor = processor
            };
            return executable;
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
        // <exception cref="SaxonApiException">Throws a <c>SaxonApiException</c> if errors were detected
        // during static analysis of the stylesheet. Details of the errors will be reported to the
        // <c>ErrorReporter</c> if supplied; otherwise they will be written to the standard
        // error stream. The exception that is returned from this method is merely a summary indicating the
        // status, and is not useful for diagnosing the actual error.</exception> 

        internal XsltExecutable Compile(Stream input, string theBaseUri, bool closeStream) {
            //See bug 2306

            StreamSource ss = new StreamSource(input);
            if (theBaseUri != null) {
                ss.setSystemId(Uri.EscapeUriString(theBaseUri));
            }
            else {
                if (baseUri != null) {
                    ss.setSystemId(Uri.EscapeUriString(baseUri.ToString()));
                }
            }

            Saxon.Hej.s9api.XsltExecutable jexecutable = jXsltCompiler.compile(ss);

            if (closeStream) {
                input.Close();
            }

            XsltExecutable executable = new(jexecutable) {
                InternalProcessor = processor
            };
            return executable;
        }


        /// <summary>Compile a library package.</summary>
        /// <remarks>
        /// <para>The source argument identifies an input stream containing an <c>xsl:package</c> element. Any packages
        /// on which this package depends must have been made available to the <c>XsltCompiler</c>
        /// by importing them using <c>ImportPackage</c>.</para>
        /// <para>The base URI of the package (used for resolving <c>xsl:import</c> and <c>xsl:include</c>
        /// declarations, and for any other constructs that depend on the static base URI) is taken
        /// from the <c>BaseUri</c> property of the compiler.</para>>
        /// <para>The caller is responsible for closing the input stream after use.</para>
        /// </remarks>
        /// <param name='input'>source identifies an XML document holding the the XSLT package to be compiled</param>
        /// <returns>The <c>XsltPackage</c> that results from the compilation. Note that this package
        /// is not automatically imported to this <c>XsltCompiler</c>; if the package is required
        /// for use in subsequent compilations then it must be explicitly imported.</returns>
        public XsltPackage CompilePackage(Stream input) {
            StreamSource ss = new (input);
            if (baseUri != null) {
                ss.setSystemId(Uri.EscapeUriString(baseUri.ToString()));
            }
            return new XsltPackage(processor, jXsltCompiler.compilePackage(ss));
        }
        
        /// <summary>Compile a library package, supplying a stream and a base URI</summary>
        /// <remarks>
        /// <para>The source argument identifies an input stream containing an <c>xsl:package</c> element. Any packages
        /// on which this package depends must have been made available to the <c>XsltCompiler</c>
        /// by importing them using <see cref="ImportPackage(XsltPackage)"/> or similar.</para>
        /// <para>The <c>BaseUri</c> property of the compiler is ignored</para>>
        /// <para>The caller is responsible for closing the input stream after use.</para>
        /// </remarks>
        /// <param name='input'>a stream containing an XML document holding the the XSLT package to be compiled</param>
        /// <param name='baseUri'>the base URI of the stream, used for resolving any relative references</param>
        /// <returns>The <c>XsltPackage</c> that results from the compilation. Note that this package
        /// is not automatically imported to this <c>XsltCompiler</c>; if the package is required
        /// for use in subsequent compilations then it must be explicitly imported.</returns>

        public XsltPackage CompilePackage(Stream input, Uri baseUri) {
            StreamSource ss = new (input, baseUri.OriginalString);
            return new XsltPackage(processor, jXsltCompiler.compilePackage(ss));
        }


        /// <summary>Import a library package. Calling this method makes the supplied package available for reference
        /// in the <code>xsl:use-package</code> declaration of subsequent compilations performed using this
        /// <code>XsltCompiler</code>.</summary>
        /// <param name='thePackage'> the package to be imported</param>
        /// <remarks>since 9.6</remarks>
        public void ImportPackage(XsltPackage thePackage) {
            if (thePackage.Processor != this.processor) {
                throw new Saxon.Api.SaxonApiException(
                        "The imported package and the XsltCompiler must belong to the same Processor");
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
                throw new Saxon.Api.SaxonApiException(
                        "The imported package and the XsltCompiler must belong to the same Processor");
            }

            try {
                PackageDetails details = new();
                packageName ??= thePackage.PackageName;
                version ??= thePackage.Version;
                details.nameAndVersion = new VersionedPackageName(packageName, version);
                details.loadedPackage = thePackage.getUnderlyingPreparedPackage();
                jXsltCompiler.getUnderlyingCompilerInfo().getPackageLibrary().addPackage(details);
            }
            catch (Saxon.Api.SaxonApiException) {
                throw;
            }
            catch (Exception ex) {
                throw new Saxon.Api.SaxonApiException(ex);
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
            Saxon.Hej.s9api.XsltPackage package = jXsltCompiler.loadLibraryPackage(location);
            return new XsltPackage(processor, package);
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
		public Saxon.Hej.trans.CompilerInfo GetUnderlyingCompilerInfo() {
            return jXsltCompiler.getUnderlyingCompilerInfo();
        }


        /// <summary>
        /// Escape hatch to the underlying Java-derived implementation of the <c>XsltCompiler</c>
        /// </summary>

        private Saxon.Hej.s9api.XsltCompiler Implementation => jXsltCompiler;


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
        /// <exception cref="Saxon.Api.SaxonApiException">If compilation fails, a <code>SaxonApiException</code> is thrown.
        /// Note that this will not contain useful error information; to capture details of
        /// specific errors, nominate an <code>ErrorReporter</code> to receive notifications.</exception>
        /// <remarks>
        /// <para>If the stylesheet contains any <c>xsl:include</c> or <c>xsl:import</c> declarations,
        /// then the <c>BaseUri</c> property must be set to allow these to be resolved.</para>
        /// <para>The stylesheet is contained in the part of the input stream between its current
        /// position and the end of the stream. It is the caller's responsibility to close the 
        /// <c>TextReader</c> after use. If the compilation succeeded, then on exit the stream will be 
        /// exhausted; if compilation failed, the current position of the stream on exit is
        /// undefined.</para>
        /// </remarks>
        /// <exception cref="SaxonApiException">Throws a <c>SaxonApiException</c> if errors were detected
        /// during static analysis of the stylesheet. Details of the errors will be reported to the
        /// <c>ErrorReporter</c> if supplied; otherwise they will be written to the standard
        /// error stream. The exception that is returned from this method is merely a summary indicating the
        /// status, and is not useful for diagnosing the actual error.</exception> 
        public XsltExecutable Compile(TextReader input) {
            StreamSource ss = new StreamSource(input);
            if (baseUri != null) {
                ss.setSystemId(Uri.EscapeUriString(baseUri.ToString()));
            }

            XsltExecutable executable = new(jXsltCompiler.compile(ss)) {
                InternalProcessor = processor
            };
            return executable;
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
        /// found. If a <c>StylesheetModuleResolver</c> has been defined, it will be used to resolve
        /// the supplied URI.</param>
        /// <returns>An <c>XsltExecutable</c> which represents the compiled stylesheet object.
        /// The <c>XsltExecutable</c> may be run as many times as required, in the same or a different
        /// thread. The <c>XsltExecutable</c> is not affected by any changes made to the <c>XsltCompiler</c>
        /// once it has been compiled.</returns>
        /// <exception cref="SaxonApiException">Throws a <c>SaxonApiException</c> if errors were detected
        /// during static analysis of the stylesheet. Details of the errors will be reported to the
        /// <c>ErrorReporter</c> if supplied; otherwise they will be written to the standard
        /// error stream. The exception that is returned from this method is merely a summary indicating the
        /// status, and is not useful for diagnosing the actual error.</exception> 
        public XsltExecutable Compile(Uri uri) {
            XsltExecutable executable;
            ResourceRequest request = new () {
                Uri = uri,
                Nature = ResourceRequest.XSLT_NATURE
            };
            if (resourceResolver == null) {
                executable = new XsltExecutable(jXsltCompiler.compile(new StreamSource(uri.ToString())));
            }
            else {
                IResource resource = resourceResolver.Invoke(request);
                if (resource is TextResource text) {
                    if (text.hasStream()) {
                        executable = Compile(text.InputStream, uri.ToString(), true);
                    }
                    else {
                        executable = Compile(text.TextReader);
                    }
                }
                else if (resource is XmlReaderResource xrr) {
                    executable = Compile(xrr.XmlReader);
                }
                else {
                    XdmItem item = resource.GetXdmItem();
                    if (item is XdmNode node) {
                        executable = Compile(node);
                    }
                    else {
                        throw new ArgumentException("Invalid type of result from URI resolver");
                    }
                }
            }

            executable.InternalProcessor = processor;
            return executable;
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
        /// The <c>XsltExecutable</c> may be run as many times as required, in the same or a different
        /// thread. The <c>XsltExecutable</c> is not affected by any changes made to the <c>XsltCompiler</c>
        /// once it has been compiled.</returns>
        /// <exception cref="SaxonApiException">Throws a <c>SaxonApiException</c> if errors were detected
        /// during static analysis of the stylesheet. Details of the errors will be reported to the
        /// <c>ErrorReporter</c> if supplied; otherwise they will be written to the standard
        /// error stream. The exception that is returned from this method is merely a summary indicating the
        /// status, and is not useful for diagnosing the actual error.</exception>
        /// <exception cref="SaxonApiException">Throws a <c>SaxonApiException</c> if errors were detected
        /// during static analysis of the stylesheet. Details of the errors will be reported to the
        /// <c>ErrorReporter</c> if supplied; otherwise they will be written to the standard
        /// error stream. The exception that is returned from this method is merely a summary indicating the
        /// status, and is not useful for diagnosing the actual error.</exception>  
        public XsltExecutable Compile(XmlReader reader) {
            DotNetPullProvider pp = new(reader);
            PipelineConfiguration pipe = processor.Implementation.makePipelineConfiguration();
            pp.setPipelineConfiguration(pipe);
            PullSource source = new PullSource(pp);
            string baseu = reader.BaseURI;
            if (string.IsNullOrEmpty(baseu)) {
                // if no baseURI is supplied by the XmlReader, use the one supplied to this Compiler
                if (baseUri == null) {
                    throw new ArgumentNullException("BaseUri");
                }

                baseu = Uri.EscapeUriString(baseUri.ToString());
                pp.setBaseURI(baseu);
            }

            source.setSystemId(baseu);

            XsltExecutable executable = new(jXsltCompiler.compile(source)) {
                InternalProcessor = processor
            };
            return executable;
            
            
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
        /// <exception cref="SaxonApiException">Throws a <c>SaxonApiException</c> if errors were detected
        /// during static analysis of the stylesheet. Details of the errors will be reported to the
        /// <c>ErrorReporter</c> if supplied; otherwise they will be written to the standard
        /// error stream. The exception that is returned from this method is merely a summary indicating the
        /// status, and is not useful for diagnosing the actual error.</exception>
        /// <exception cref="SaxonApiException">Throws a <c>SaxonApiException</c> if errors were detected
        /// during static analysis of the stylesheet. Details of the errors will be reported to the
        /// <c>ErrorReporter</c> if supplied; otherwise they will be written to the standard
        /// error stream. The exception that is returned from this method is merely a summary indicating the
        /// status, and is not useful for diagnosing the actual error.</exception> 
        public XsltExecutable Compile(XdmNode node) {
            XsltExecutable executable = new XsltExecutable(jXsltCompiler.compile(node.Implementation.asActiveSource()));
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
        /// <exception cref="SaxonApiException">Throws a <c>SaxonApiException</c> if errors were detected
        /// during static analysis of the stylesheet. Details of the errors will be reported to the
        /// <c>ErrorReporter</c> if supplied; otherwise they will be written to the standard
        /// error stream. The exception that is returned from this method is merely a summary indicating the
        /// status, and is not useful for diagnosing the actual error.</exception>
        
        public XsltExecutable CompileAssociatedStylesheet(Uri uri, string media) {
            // TODO: reinstate
            // object obj = XmlResolver.GetEntity(uri, media ?? "application/xml",
            //     Type.GetType("System.IO.Stream"));
            // if (obj is Stream stream) {
            //     StreamSource ss = new (stream, uri.OriginalString);
            //     // See bug issue #2306
            //     XsltExecutable executable =
            //         new(jXsltCompiler.compile(jXsltCompiler.getAssociatedStylesheet(ss, media, null, null))) {
            //             InternalProcessor = processor
            //         };
            //     return executable;
            //     
            // }
            throw new ArgumentException("Invalid type of result from XmlResolver.GetEntity: ");
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
        /// <exception cref="SaxonApiException">Throws a <c>SaxonApiException</c> if errors were detected
        /// during static analysis of the stylesheet. Details of the errors will be reported to the
        /// <c>ErrorReporter</c> if supplied; otherwise they will be written to the standard
        /// error stream. The exception that is returned from this method is merely a summary indicating the
        /// status, and is not useful for diagnosing the actual error.</exception>
        public XsltExecutable CompileAssociatedStylesheet(XdmNode source) {
            if (source == null || source.NodeKind != XdmNodeKind.Document) {
                throw new ArgumentException("Source must be a document node");
            }
            
            foreach (XdmNode n in source
                .Children(
                    Predicates.AllOf(Predicates.IsProcessingInstruction(), 
                    Predicates.HasName("", "xml-stylesheet")))) {
                // TODO: check the media type
                String href = ProcInstParser.getPseudoAttribute(n.StringValue, "href");
                if (href == null) {
                    throw new SaxonApiException("xml-stylesheet processing instruction has no @href attribute");
                }

                int hash = href.LastIndexOf('#');
                if (hash == 0) {
                    if (href.Length == 1) {
                        throw new SaxonApiException("Relative URI of '#' is invalid");
                    }
            
                    string fragment = href.Substring(1);
                    NodeInfo target = ((NodeInfo)source.value).getTreeInfo().selectID(fragment, true);
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
                            if (x.NodeKind == XdmNodeKind.Element &&
                                x.NodeName.Uri == "http://www.w3.org/1999/XSL/Transform" &&
                                (x.NodeName.LocalName == "stylesheet" || x.NodeName.LocalName == "transform" &&
                                    x.GetAttributeValue(qid) == fragment)) {
                                targetWrapper = x;
                            }
                        }
                    }
                    else {
                        targetWrapper = (XdmNode) XdmValue.Wrap(target);
                    }
            
                    if (targetWrapper == null) {
                        throw new SaxonApiException("No element with id='" + fragment + "' found");
                    }
            
                    return Compile(targetWrapper);
                }

                if (hash > 0) {
                    throw new NotImplementedException(
                        "href cannot identify an embedded stylesheet in a different document");
                }

                Uri uri = new (n.BaseUri, href);
                return Compile(uri);
            }
            
            throw new SaxonApiException("xml-stylesheet processing instruction not found");
        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited