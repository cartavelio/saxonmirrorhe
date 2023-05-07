using System;
using System.Collections.Generic;
using System.Impl.Helpers;
using System.IO;
using System.Text;
using System.Xml;
using Saxon.Ejavax.xml.transform.stream;
using Saxon.Hej.@event;
using Saxon.Hej.lib;
using Saxon.Hej.s9api;
using Saxon.Api.Support;
using Saxon.Callbacks;
using Saxon.Ejavax.xml.transform;
using Saxon.Hej;
using Saxon.Hej.pull;
using StandardMessageHandler = Saxon.Callbacks.StandardMessageHandler;

namespace Saxon.Api {
    /// <summary>An <code>Xslt30Transformer</code> represents a compiled and loaded stylesheet ready for execution.
    /// The <code>Xslt30Transformer</code> holds details of the dynamic evaluation context for the stylesheet.</summary>
    /// <remarks><para>The <code>Xslt30Transformer</code> differs from <see cref="XsltTransformer"/> 
    /// in supporting new options
    /// for invoking a stylesheet, corresponding to facilities defined in the XSLT 3.0 specification. However,
    /// it is not confined to use with XSLT 3.0, and most of the new invocation facilities (for example,
    /// calling a stylesheet-defined function directly) work equally well with XSLT 2.0 and in some cases
    /// XSLT 1.0 stylesheets.</para>
    /// <para>An <code>Xslt30Transformer</code> must not be used concurrently in multiple threads.
    /// It is safe, however, to reuse the object within a single thread to run the same
    /// stylesheet several times. Running the stylesheet does not change the context
    /// that has been established.</para>
    /// <para>An <code>Xslt30Transformer</code> is always constructed by running the <code>Load30</code>
    /// method of an <see cref="XsltExecutable"/>.</para>
    /// <para>Unlike <code>XsltTransformer</code>, an <code>Xslt30Transformer</code> is not a <code>Destination</code>.
    /// To pipe the results of one transformation into another, the target should be an <code>XsltTransfomer</code>
    /// rather than an <code>Xslt30Transformer</code>.</para>
    /// <para>Evaluation of an Xslt30Transformer proceeds in a number of phases:</para>
    /// <list type="number">
    /// <item>First, values may be supplied for stylesheet parameters and for the global context item. The
    /// global context item is used when initializing global variables. Unlike earlier transformation APIs,
    /// the global context item is quite independent of the "principal source document".
    /// </item>
    /// <item>The stylesheet may now be repeatedly invoked. Each invocation takes
    /// one of three forms:
    /// <list type="number">
    /// <item>Invocation by applying templates. In this case, the information required is (i) an initial
    /// mode (which defaults to the unnamed mode), (ii) an initial match sequence, which is any
    /// XDM value, which is used as the effective "select" expression of the implicit apply-templates
    /// call, and (iii) optionally, values for the tunnel and non-tunnel parameters defined on the
    /// templates that get invoked (equivalent to using <code>xsl:with-param</code> on the implicit
    /// <code>apply-templates</code> call).</item>
    /// <item>Invocation by calling a named template. In this case, the information required is
    /// (i) the name of the initial template (which defaults to "xsl:initial-template"), and
    /// (ii) optionally, values for the tunnel and non-tunnel parameters defined on the
    /// templates that get invoked (equivalent to using <code>xsl:with-param</code> on the implicit
    /// <code>call-template</code> instruction).</item>
    /// <item>Invocation by calling a named function. In this case, the information required is
    /// the sequence of arguments to the function call.</item>
    /// </list>
    /// </item>
    /// <item>Whichever invocation method is chosen, the result may either be returned directly, as an arbitrary
    /// XDM value, or it may effectively be wrapped in an XML document. If it is wrapped in an XML document,
    /// that document can be processed in a number of ways, for example it can be materialized as a tree in
    /// memory, it can be serialized as XML or HTML, or it can be subjected to further transformation.</item>
    /// </list>
    /// <para>Once the stylesheet has been invoked (using any of these methods), the values of the global context
    /// item and stylesheet parameters cannot be changed. If it is necessary to run another transformation with
    /// a different context item or different stylesheet parameters, a new <c>Xslt30Transformer</c>
    /// should be created from the original <c>XsltExecutable</c>.</para>
    /// <para> @since 9.6</para> 
    /// </remarks>
    [Serializable]
    public class Xslt30Transformer {
        private ResultDocumentHandler resultDocumentHandler;
        private TextWriter traceFunctionDestination;
        private Saxon.Hej.s9api.Xslt30Transformer s9Transformer;
        private readonly StandardMessageHandler _standardMessageHandler;
        private ResourceResolver textResolver;
        private ResourceResolver xmlDocumentResolver;

        // internal constructor

        internal Xslt30Transformer(Saxon.Hej.s9api.Xslt30Transformer transformer) {
            this.s9Transformer = transformer;
            _standardMessageHandler = new StandardMessageHandler(transformer.controller.getConfiguration());
            transformer.controller.setMessageHandler(message => _standardMessageHandler.accept(message));
            //this.globalParameterSet = new GlobalParameterSet(globalParameters);
        }


        /// <summary> Supply the context item to be used when evaluating global variables and parameters.
        /// This argument can be null if no context item is to be supplied.</summary>

        public XdmItem GlobalContextItem {
            set => s9Transformer.setGlobalContextItem(value);
            get => s9Transformer.getGlobalContextItem();
        }


        /// <summary> Get the underlying <c>Controller</c> used to implement this <c>XsltTransformer</c>. This provides access
        /// to lower-level methods not otherwise available in the Saxon.Api interface. Note that classes
        /// and methods obtained by this route cannot be guaranteed stable from release to release.</summary>

		public Saxon.Hej.trans.XsltController GetUnderlyingController => s9Transformer.getUnderlyingController();

        internal Saxon.Hej.s9api.Xslt30Transformer GetUnderlyingXslt30Transformer => s9Transformer;


        /// <summary>
        /// Construct a <c>Destination</c> object whose effect is to perform this transformation
        /// on any input that is sent to that <c>Destination</c>: for example, it allows this transformation
        /// to post-process the results of another transformation, or to process the output
        /// of streamed validation.
        /// </summary>
        /// <remarks>
        /// <para>This method allows a pipeline of transformations to be created in which
        /// one transformation is used as the destination of another. The transformations
        /// may use streaming, in which case intermediate results will not be materialized
        /// in memory. If a transformation does not use streaming, then its input will
        /// first be assembled in memory as a node tree.</para>
        /// <para>The <c>Destination</c> returned by this method performs <em>sequence normalization</em>
        /// as defined in the serialization specification: that is, the raw result of the transformation
        /// sent to this destination is wrapped into a document node. Any item-separator present in
        /// any serialization parameters is ignored (adjacent atomic values are separated by whitespace). 
        /// This makes the method unsuitable for passing intermediate results other than XML document
        /// nodes.</para>
        /// </remarks>
        /// <param name="finalDestination">supplied final destination</param>
        /// <returns>a <c>Destination</c> which accepts an XML document (typically as a stream
        /// of events) and which transforms this supplied XML document (possibly using streaming)
        /// as defined by the stylesheet from which which this <c>Xslt30Transformer</c>
        /// was generated, sending the principal result of the transformation to the supplied
        /// <c>finalDestination</c>.
        /// The transformation is performed as if by the <see cref="ApplyTemplates(Stream, IDestination)"/>
        /// method: that is, by applying templates to the root node of the supplied XML document.
        /// </returns>
        public IDestination AsDocumentDestination(IDestination finalDestination) {
            return new TransformerDestination(this, finalDestination);
        }

        // internal method
        internal Receiver GetDestinationReceiver(IDestination destination) {
            Receiver r = s9Transformer.getDestinationReceiver(s9Transformer.getUnderlyingController(),
                destination.UnderlyingDestination);
            return r;
        }


        /// <summary>
        /// The <c>IResultDocumentHandler</c> to be used at run-time to process the output
        /// produced by any <c>xsl:result-document</c> instruction with an <c>href</c>
        /// attribute.
        /// </summary>
        /// <remarks>
        /// In the absence of a user-supplied result document handler, the <c>href</c>
        /// attribute of the <c>xsl:result-document</c> instruction must be a valid relative
        /// URI, which is resolved against the value of the <c>BaseOutputUri</c> property,
        /// and the resulting absolute URI must identify a writable resource (typically
        /// a file in filestore, using the <c>file:</c> URI scheme).
        /// </remarks>

        public ResultDocumentHandler ResultDocumentHandler {
            get => resultDocumentHandler;
            set {
                resultDocumentHandler = value;
                s9Transformer.getUnderlyingController().setResultDocumentResolver(
                    new ResultDocumentHandlerWrapper(value,makePipelineConfiguration()));
            }
        }

        private PipelineConfiguration makePipelineConfiguration() {
            return s9Transformer.getUnderlyingController().makePipelineConfiguration();
        }

        /// <summary>
        /// The <c>SchemaValidationMode</c> to be used in this transformation, especially for documents
        /// loaded using the <code>doc()</code>, <code>document()</code>, or <code>collection()</code> functions.
        /// </summary>

        public SchemaValidationMode SchemaValidationMode {
            get {
                switch (s9Transformer.getUnderlyingController().getSchemaValidationMode()) {
                    case Validation.STRICT:
                        return SchemaValidationMode.Strict;
                    case Validation.LAX:
                        return SchemaValidationMode.Lax;
                    case Validation.STRIP:
                        return SchemaValidationMode.None;
                    case Validation.PRESERVE:
                        return SchemaValidationMode.Preserve;
                    case Validation.DEFAULT:
                    default:
                        return SchemaValidationMode.Unspecified;
                }
            }


            set {
                switch (value) {
                    case SchemaValidationMode.Strict:
                        s9Transformer.setSchemaValidationMode(ValidationMode.STRICT);
                        break;
                    case SchemaValidationMode.Lax:
                        s9Transformer.setSchemaValidationMode(ValidationMode.LAX);
                        break;
                    case SchemaValidationMode.None:
                        s9Transformer.setSchemaValidationMode(ValidationMode.STRIP);
                        break;
                    case SchemaValidationMode.Preserve:
                        s9Transformer.setSchemaValidationMode(ValidationMode.PRESERVE);
                        break;
                    case SchemaValidationMode.Unspecified:
                    default:
                        s9Transformer.setSchemaValidationMode(ValidationMode.DEFAULT);
                        break;
                }
            }
        }


        /// <summary> Supply the values of global stylesheet variables and parameters.</summary>
        /// <param name="parameters"> A <c>Dictionary</c> whose keys are QNames identifying global stylesheet parameters,
        /// and whose corresponding values are the values to be assigned to those parameters. If necessary
        /// the supplied values are converted to the declared type of the parameter.
        /// The contents of the supplied <c>Dictionary</c> are copied by this method,
        /// so subsequent changes to the <c>Dictionary</c> have no effect.</param>
        public void SetStylesheetParameters(Dictionary<QName, XdmValue> parameters) {
            try {
                s9Transformer.setStylesheetParameters(parameters);
            }
            catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }


        /// <summary>Get the base output URI.</summary>
        /// <remarks><para> This returns the value set using the setter method. If no value has been set
        /// explicitly, then the method returns null if called before the transformation, or the computed
        /// default base output URI if called after the transformation.
        /// </para>
        /// <para> The base output URI is used for resolving relative URIs in the <code>href</code> attribute
        /// of the <code>xsl:result-document</code> instruction.</para></remarks>
        /// <returns> The base output URI</returns>

        public string BaseOutputURI {
            set => s9Transformer.setBaseOutputURI(value);
            get => s9Transformer.getBaseOutputURI();
        }

        /// <summary>
        /// The <c>TextResolver</c> to be used at run-time to resolve and dereference URIs
        /// supplied to the <c>unparsed-text()</c>, <c>unparsed-text-lines()</c>,
        /// <c>unparsed-text-available()</c>, and <c>json-doc()</c> functions.
        /// </summary>
        /// <remarks>
        /// <para>If no <c>TextResolver</c> is supplied, or if the supplied <c>TextResolver</c> returns null
        /// when called, the <c>CommonResourceResolver</c> for the Saxon <c>Configuration</c> is invoked,
        /// with a <c>ResourceRequest</c> having <c>Nature == ResourceRequest.TEXT_NATURE</c>. If this
        /// returns null, then the "direct" URI resolution provided by <c>Org.XmlResolver.UriUtils</c> is
        /// invoked: this is capable of handling regular <c>file:</c>, <c>http:</c>, and <c>https:</c>
        /// URIs, as well as the <c>data:</c> and <c>pack:</c> schemes.
        /// </para>
        /// <para>If the call on <c>unparsed-text()</c> supplies an encoding parameter, this is made
        /// available in the call to the <c>TextResolver</c>. 
        /// </para>
        /// </remarks>

        public ResourceResolver TextResolver {
            get => textResolver;
            set {
                textResolver = value;
                s9Transformer.setUnparsedTextResolver(WrapTextResolver(value));
            }
        }

        /// <summary>
        /// The <code>ResourceResolver</code> to be used at run-time to resolve and dereference URIs
        /// supplied to functions retrieving XML, such as the <c>fn:doc()</c>, <c>fn:document()</c>, and
        /// <c>fn:doc-available()</c> functions, as well as <c>fn:transform()</c>
        /// </summary>
        /// <remarks>
        /// <para>
        /// The URI supplied as the first argument to the resolver is the absolute URI requested,
        /// that is, the result of resolving the requested relative URI against the relevant base URI.
        /// </para>
        /// <para>The requested relative URI and the corresponding base URI are supplied as separate
        /// properties within the <c>ResourceProperties</c> argument, in case they are needed.
        /// </para>
        /// <para>The returned <c>IResource</c> must be one of the following:
        /// <list>
        /// <item>An <c>IResource</c> such as a <c>NodeResource</c> or an <c>XmlReaderResource</c>
        /// whose <c>getItem()</c> method delivers an <c>XdmNode</c>;</item>
        /// <item>A <c>TextResource</c> delivering lexical XML in its <c>Content</c> property; this
        /// will then be parsed to deliver the corresponding XML document.</item>
        /// </list>
        /// </para>
        /// <para>If no <c>XmlDocumentResolver</c> is supplied, or if it returns null, then
        /// the <c>CommonResourceResolver</c> associated with the Saxon Configuration is invoked.
        /// </para>
        /// </remarks>

        public ResourceResolver XmlDocumentResolver {
            get => xmlDocumentResolver;
            set {
                xmlDocumentResolver = value;
                s9Transformer.setResourceResolver(new JResourceResolverWrappingCS(
                    value, s9Transformer.getUnderlyingController().getConfiguration()));
            }
        }

        /// <summary>
        /// Ask whether assertions (<c>xsl:assert</c> instructions) have been enabled at run time. 
        /// </summary>
        /// <remarks>By default they are disabled at compile time. If assertions are enabled at compile time, then by
        /// default they will also be enabled at run time; but they can be disabled at run time by
        /// specific request. At compile time, assertions can be enabled for some packages and
        /// disabled for others; at run time, they can only be enabled or disabled globally.</remarks>
        /// <returns>true if assertions are enabled at run time</returns>
        /// <remarks>Since 9.7</remarks>

        public bool AssertionsEnabled => s9Transformer.isAssertionsEnabled();
        
        /// <summary>
        /// Handler for messages output using <c>&lt;xsl:message&gt;</c>.
        /// </summary>
        /// <remarks> 
        /// <para>The caller may supply a message handler before calling <c>Run</c>;
        /// the processor will then invoke the handler once for each message generated during
        /// the transformation. Each message will be output as an object of type <c>XdmNode</c>
        /// representing a document node.</para>
        /// <para>If no message handler is supplied by the caller, message information will be written to
        /// the standard error stream.</para>
        /// <para>Each message is presented as an XML document node. Calling <c>ToString()</c>
        /// on the message object will usually generate an acceptable representation of the
        /// message.</para>
        /// <para>When the <c>xsl:message</c> instruction specifies <c>terminate="yes"</c>,
        /// the message is first notified using this interface, and then an exception is thrown
        /// which terminates the transformation.</para>
        /// </remarks>

        public Action<Message> MessageListener {
            get => msg => s9Transformer.controller.getMessageHandler().Invoke(msg);
            set => s9Transformer.controller.setMessageHandler(value.Invoke);
        }

        /// <summary>
        /// Destination for reporting warnings and non-fatal errors (including validation errors
        /// that occur when validating constructed nodes, which will eventually cause the transformation
        /// to fail)
        /// </summary>
        ///
        public ErrorReporter ErrorReporter {
            set => s9Transformer.setErrorReporter(new ErrorReporterWrapper(value));
        }

        /// <summary>
        /// Destination for output of messages using the <c>trace()</c> function. 
        /// </summary>
        /// <remarks>
        /// <para>If no message listener is supplied by the caller, message information will be written to
        /// the standard error stream.</para>
        /// <para>The supplied destination is ignored if a <code>TraceListener</code> is in use.</para>
        /// <para>The <c>TextWriter</c> should be closed by the caller after use.</para>
        /// </remarks>

		public TextWriter TraceFunctionDestination {
            set {
                traceFunctionDestination = value;
                s9Transformer.setTraceFunctionDestination(new StandardLogger(value));
            }
            get => traceFunctionDestination;
        }


        /// <summary>Set parameters to be passed to the initial template. These are used
        /// whether the transformation is invoked by applying templates to an initial source item,
        /// or by invoking a named template. The parameters in question are the <c>xsl:param</c> elements
        /// appearing as children of the <c>xsl:template</c> element.
        /// </summary>
        /// <remarks>
        /// <para>The parameters are supplied in the form of a map; the key is a <c>QName</c> which must
        /// match the name of the parameter; the associated value is an <c>XdmValue</c> containing the
        /// value to be used for the parameter. If the initial template defines any required
        /// parameters, the map must include a corresponding value. If the initial template defines
        /// any parameters that are not present in the map, the default value is used. If the map
        /// contains any parameters that are not defined in the initial template, these values
        /// are silently ignored.</para>
        /// <para>The supplied values are converted to the required type using the function conversion
        /// rules. If conversion is not possible, a run-time error occurs (not now, but later, when
        /// the transformation is actually run).</para>
        /// <para>The <code>Xslt30Transformer</code> retains a reference to the supplied map, so parameters can be added or
        /// changed until the point where the transformation is run.</para>
        /// <para>The XSLT 3.0 specification makes provision for supplying parameters to the initial
        /// template, as well as global stylesheet parameters. Although there is no similar provision
        /// in the XSLT 1.0 or 2.0 specifications, this method works for all stylesheets, regardless whether
        /// XSLT 3.0 is enabled or not.</para></remarks>
        /// <param name="parameters"> The parameters to be used for the initial template</param>
        /// <param name="tunnel"> true if these values are to be used for setting tunnel parameters;
        /// false if they are to be used for non-tunnel parameters</param>
        public void SetInitialTemplateParameters(Dictionary<QName, XdmValue> parameters, bool tunnel) {
            s9Transformer.setInitialTemplateParameters(parameters, tunnel);
        }


        /// <summary>Initial mode for the transformation. This is used if the stylesheet is
        /// subsequently invoked by any of the <code>applyTemplates</code> methods.</summary>
        /// <remarks><para>The value may be the name of the initial mode, or null to indicate the default
        /// (unnamed) mode</para></remarks>

        public QName InitialMode {
            set => s9Transformer.setInitialMode(value);
            get => s9Transformer.getInitialMode();
        }


        /// <summary>Invoke the stylesheet by applying templates to a supplied source document, 
        /// sending the results to a given <c>IDestination</c>. The 
        /// invocation uses any initial mode set using <see cref="InitialMode"/>,
        /// and any template parameters set using <see cref="SetInitialTemplateParameters"/>.
        /// </summary>
        /// <param name="input">The source document. To apply more than one transformation to the same source 
        /// document, the source document tree can be pre-built using a <see cref="Saxon.Hej.s9api.DocumentBuilder"/>.</param>
        /// <param name="destination">The destination of the result document produced by wrapping the result 
        /// of the apply-templates call in a document node.  If the destination is a <see cref="Serializer"/>, 
        /// then the serialization parameters set in the serializer are combined with those defined in the 
        /// stylesheet (the parameters set in the serializer take precedence).</param>
        /// <remarks>
        /// <para>If the initial mode is declared streamable, this will perform a streamed transformation.</para>
        /// <para>Note that this method does not cause the supplied input to become the global context item
        /// (that is, to be available as the value of "." when evaluating global variables). For this, the
        /// <code>GlobalContextItem</code> property should be set.</para>
        /// </remarks>
        public void ApplyTemplates(Stream input, IDestination destination) {
            s9Transformer.applyTemplates(new StreamSource(input), destination.UnderlyingDestination);
        }


        /// <summary>Invoke the stylesheet by applying templates to a supplied source document, 
        /// using the supplied base URI,
        /// sending the results to a given <c>IDestination</c>. The 
        /// invocation uses any initial mode set using <see cref="InitialMode"/>,
        /// and any template parameters set using <see cref="SetInitialTemplateParameters"/>.
        /// </summary>
        /// <param name="input">The source document. To apply more than one transformation to the same source 
        /// document, the source document tree can be pre-built using a <see cref="Saxon.Hej.s9api.DocumentBuilder"/>.</param>
        /// <param name="baseUri">Base URI used for the input document</param>
        /// <param name="destination">The destination of the result document produced by wrapping the result 
        /// of the apply-templates call in a document node.  If the destination is a <see cref="Serializer"/>, 
        /// then the serialization parameters set in the serializer are combined with those defined in the 
        /// stylesheet (the parameters set in the serializer take precedence).</param>
        /// <remarks>
        /// <para>If the initial mode is declared streamable, this will perform a streamed transformation.</para>
        /// <para>Note that this method does not cause the supplied input to become the global context item
        /// (that is, to be available as the value of "." when evaluating global variables). For this, the
        /// <code>GlobalContextItem</code> property should be set.</para>
        /// </remarks>
        public void ApplyTemplates(Stream input, Uri baseUri, IDestination destination) {
            StreamSource streamSource =
                new (input, baseUri.ToString());
            s9Transformer.applyTemplates(streamSource, destination.UnderlyingDestination);
        }
        
        /// <summary>Invoke the stylesheet by applying templates to a supplied source document, 
        /// sending the results to a given <c>IDestination</c>. The 
        /// invocation uses any initial mode set using <see cref="InitialMode"/>,
        /// and any template parameters set using <see cref="SetInitialTemplateParameters"/>.
        /// </summary>
        /// <param name="input">The source document. To apply more than one transformation to the same source 
        /// document, the source document tree can be pre-built using a <see cref="Saxon.Hej.s9api.DocumentBuilder"/>.</param>
        /// <param name="destination">The destination of the result document produced by wrapping the result 
        /// of the apply-templates call in a document node.  If the destination is a <see cref="Serializer"/>, 
        /// then the serialization parameters set in the serializer are combined with those defined in the 
        /// stylesheet (the parameters set in the serializer take precedence).</param>
        /// <remarks>
        /// <para>If the initial mode is declared streamable, this will perform a streamed transformation.</para>
        /// <para>Note that this method does not cause the supplied input to become the global context item
        /// (that is, to be available as the value of "." when evaluating global variables). For this, the
        /// <code>GlobalContextItem</code> property should be set.</para>
        /// </remarks>
      
        public void ApplyTemplates(TextReader input, IDestination destination) {
            s9Transformer.applyTemplates(new StreamSource(input), destination.UnderlyingDestination);
        }


        /// <summary>Invoke the stylesheet by applying templates to a supplied source document, 
        /// using the supplied base URI,
        /// sending the results to a given <c>IDestination</c>. The 
        /// invocation uses any initial mode set using <see cref="InitialMode"/>,
        /// and any template parameters set using <see cref="SetInitialTemplateParameters"/>.
        /// </summary>
        /// <param name="input">The source document. To apply more than one transformation to the same source 
        /// document, the source document tree can be pre-built using a <see cref="Saxon.Hej.s9api.DocumentBuilder"/>.</param>
        /// <param name="baseUri">Base URI used for the input document</param>
        /// <param name="destination">The destination of the result document produced by wrapping the result 
        /// of the apply-templates call in a document node.  If the destination is a <see cref="Serializer"/>, 
        /// then the serialization parameters set in the serializer are combined with those defined in the 
        /// stylesheet (the parameters set in the serializer take precedence).</param>
        /// <remarks>
        /// <para>If the initial mode is declared streamable, this will perform a streamed transformation.</para>
        /// <para>Note that this method does not cause the supplied input to become the global context item
        /// (that is, to be available as the value of "." when evaluating global variables). For this, the
        /// <code>GlobalContextItem</code> property should be set.</para>
        /// </remarks>
        public void ApplyTemplates(TextReader input, Uri baseUri, IDestination destination) {
            StreamSource streamSource = new (input, baseUri.ToString());
            s9Transformer.applyTemplates(streamSource, destination.UnderlyingDestination);
        }
        
        /// <summary>
        /// Invoke the stylesheet by applying templates to a supplied source document, sending the results
        /// to a given <c>Destination</c>. The invocation uses the initial mode set using <see cref="InitialMode"/>
        /// (defaulting to the default mode defined in the stylesheet itself, which by default is the unnamed mode).
        /// It also uses any template parameters set using <see cref="SetInitialTemplateParameters"/>.
        /// </summary>
        /// <param name="input">The source document. To apply more than one transformation to the same source 
        /// document, the source document tree can be pre-built using a <see cref="Saxon.Hej.s9api.DocumentBuilder"/>.</param>
        /// <param name="destination">The destination of the principal result of the transformation.
        /// If the destination is a <see cref="Serializer"/>, then the serialization
        /// parameters set in the serializer are combined with those defined in the stylesheet
        /// (the parameters set in the serializer take precedence).</param>
        /// <remarks>
        /// <para>If the initial mode is declared streamable, this will perform a streamed transformation.</para>
        /// <para>Note that this method does not cause the supplied input to become the global context item
        /// (that is, to be available as the value of "." when evaluating global variables). For this, the
        /// <code>GlobalContextItem</code> property should be set.</para>
        /// </remarks>
        public void Transform(Stream input, IDestination destination) {
            s9Transformer.transform(new StreamSource(input), destination.UnderlyingDestination);
        }

        /// <summary>
        /// Invoke the stylesheet by applying templates to a supplied Source document,  
        /// using the supplied base URI, sending the results
        /// to a given <c>Destination</c>. The invocation uses the initial mode set using <see cref="InitialMode"/>
        /// (defaulting to the default mode defined in the stylesheet itself, which by default is the unnamed mode).
        /// It also uses any template parameters set using <see cref="SetInitialTemplateParameters"/>.
        /// </summary>
        /// <param name="input">The source document. To apply more than one transformation to the same source 
        /// document, the source document tree can be pre-built using a <see cref="Saxon.Hej.s9api.DocumentBuilder"/>.</param>
        /// <param name="baseUri">Base URI used for the input document</param>
        /// <param name="destination">The destination of the principal result of the transformation.
        /// If the destination is a <see cref="Serializer"/>, then the serialization
        /// parameters set in the serializer are combined with those defined in the stylesheet
        /// (the parameters set in the serializer take precedence).</param>
        /// <remarks>
        /// <para>If the initial mode is declared streamable, this will perform a streamed transformation.</para>
        /// <para>Note that this method does not cause the supplied input to become the global context item
        /// (that is, to be available as the value of "." when evaluating global variables). For this, the
        /// <code>GlobalContextItem</code> property should be set.</para>
        /// </remarks>
        public void Transform(Stream input, Uri baseUri, IDestination destination) {
            StreamSource streamSource = new (input, baseUri.ToString());
            s9Transformer.transform(streamSource, destination.UnderlyingDestination);
        }


        /// <summary>Invoke the stylesheet by applying templates to a supplied source document, 
        /// using the supplied base URI, returning the raw results as an <c>XdmValue</c>. The 
        /// invocation uses any initial mode set using <see cref="InitialMode"/>,
        /// and any template parameters set using <see cref="SetInitialTemplateParameters"/>.
        /// </summary>
        /// <param name="input">The source document. To apply more than one transformation to the same source 
        /// document, the source document tree can be pre-built using a <see cref="Saxon.Hej.s9api.DocumentBuilder"/>.</param>
        /// <param name="baseUri">Base URI</param>
        /// <returns>the raw result of applying templates to the supplied selection value, without wrapping in
        /// a document node or serializing the result. If there is more than one item in the selection, the result
        /// is the concatenation of the results of applying templates to each item in turn.</returns>
        /// <remarks>
        /// <para>If the initial mode is declared streamable, this will perform a streamed transformation.</para>
        /// <para>Note that this method does not cause the supplied input to become the global context item
        /// (that is, to be available as the value of "." when evaluating global variables). For this, the
        /// <code>GlobalContextItem</code> property should be set.</para>
        /// </remarks>
        public XdmValue ApplyTemplates(Stream input, Uri baseUri) {
            StreamSource streamSource = new (input, baseUri.ToString());
            return s9Transformer.applyTemplates(streamSource);
        }
        
        /// <summary>Invoke the stylesheet by applying templates to a supplied source document, 
        /// using the supplied base URI, returning the raw results as an <c>XdmValue</c>. The 
        /// invocation uses any initial mode set using <see cref="InitialMode"/>,
        /// and any template parameters set using <see cref="SetInitialTemplateParameters"/>.
        /// </summary>
        /// <param name="input">The source document. To apply more than one transformation to the same source 
        /// document, the source document tree can be pre-built using a <see cref="Saxon.Hej.s9api.DocumentBuilder"/>.</param>
        /// <param name="baseUri">Base URI</param>
        /// <returns>the raw result of applying templates to the supplied selection value, without wrapping in
        /// a document node or serializing the result. If there is more than one item in the selection, the result
        /// is the concatenation of the results of applying templates to each item in turn.</returns>
        /// <remarks>
        /// <para>If the initial mode is declared streamable, this will perform a streamed transformation.</para>
        /// <para>Note that this method does not cause the supplied input to become the global context item
        /// (that is, to be available as the value of "." when evaluating global variables). For this, the
        /// <code>GlobalContextItem</code> property should be set.</para>
        /// </remarks>
        public XdmValue ApplyTemplates(TextReader input, Uri baseUri) {
            StreamSource streamSource = new (input, baseUri.ToString());
            return s9Transformer.applyTemplates(streamSource);
        }

        /// <summary>Invoke the stylesheet by applying templates to a supplied source document, 
        /// sending the results to a given <c>IDestination</c>. The 
        /// invocation uses any initial mode set using <see cref="InitialMode"/>,
        /// and any template parameters set using <see cref="SetInitialTemplateParameters"/>.
        /// </summary>
        /// <param name="input">The source document, encapsulated by an <c>XmlReader</c>.
        /// Saxon will not change any property settings on the <c>XmlReader</c> (for example,
        /// it will not reset its <c>XmlResolver</c> property.)</param>
        /// <param name="destination">The destination of the result document produced by wrapping the result 
        /// of the apply-templates call in a document node.  If the destination is a <see cref="Serializer"/>, 
        /// then the serialization parameters set in the serializer are combined with those defined in the 
        /// stylesheet (the parameters set in the serializer take precedence).</param>
        /// <remarks>
        /// <para>If the initial mode is declared streamable, this will perform a streamed transformation.</para>
        /// <para>Note that this method does not cause the supplied input to become the global context item
        /// (that is, to be available as the value of "." when evaluating global variables). For this, the
        /// <code>GlobalContextItem</code> property should be set.</para>
        /// </remarks>
      
        public void ApplyTemplates(XmlReader input, IDestination destination) {
            s9Transformer.applyTemplates(makePullSource(input), destination.UnderlyingDestination);
        }
        
        /// <summary>Invoke the stylesheet by applying templates to a supplied source document, 
        /// using the supplied base URI, returning the raw results as an <c>XdmValue</c>. The 
        /// invocation uses any initial mode set using <see cref="InitialMode"/>,
        /// and any template parameters set using <see cref="SetInitialTemplateParameters"/>.
        /// </summary>
        /// <param name="input">The source document, encapsulated by an <c>XmlReader</c>.
        /// Saxon will not change any property settings on the <c>XmlReader</c> (for example,
        /// it will not reset its <c>XmlResolver</c> property.)</param>
        /// <returns>the raw result of applying templates to the supplied selection value, without wrapping in
        /// a document node or serializing the result. If there is more than one item in the selection, the result
        /// is the concatenation of the results of applying templates to each item in turn.</returns>
        /// <remarks>
        /// <para>If the initial mode is declared streamable, this will perform a streamed transformation.</para>
        /// <para>Note that this method does not cause the supplied input to become the global context item
        /// (that is, to be available as the value of "." when evaluating global variables). For this, the
        /// <code>GlobalContextItem</code> property should be set.</para>
        /// </remarks>
        public XdmValue ApplyTemplates(XmlReader input) {
            return s9Transformer.applyTemplates(makePullSource(input));
        }

        private PullSource makePullSource(XmlReader reader) {
            Configuration config = s9Transformer.getUnderlyingController().getConfiguration();
            PullProvider pp = new DotNetPullProvider(reader);
            pp.setPipelineConfiguration(config.makePipelineConfiguration());
            // pp = new PullTracer(pp);  /* diagnostics */
            PullSource source = new (pp);
            source.setSystemId(reader.BaseURI);
            return source;
        }

        /// <summary>
        /// Invoke the stylesheet by applying templates to a supplied input sequence, sending the results 
        /// to a given <c>IDestination</c>. The invocation uses any initial mode set using 
        /// <see cref="InitialMode"/>, and any template parameters set using <see cref="SetInitialTemplateParameters"/>.
        /// </summary>
        /// <param name="selection">The initial value to which templates are to be applied (equivalent to the <code>select</code>
        /// attribute of <code>xsl:apply-templates</code>)</param>
        /// <param name="destination">The destination of the result document produced by wrapping the result of the apply-templates
        /// call in a document node.  If the destination is a <see cref="Serializer"/>, then the serialization
        /// parameters set in the serializer are combined with those defined in the stylesheet
        /// (the parameters set in the serializer take precedence).</param>
        /// <remarks>
        /// <para>This method is unsuitable for performing a streamed transformation.</para>
        /// <para>Note that this method does not cause the supplied input to become the global context item
        /// (that is, to be available as the value of "." when evaluating global variables). For this, the
        /// <code>GlobalContextItem</code> property should be set.</para>
        /// </remarks>
        public void ApplyTemplates(XdmValue selection, IDestination destination) {
            s9Transformer.applyTemplates(selection, destination.UnderlyingDestination);
        }


        /// <summary>
        /// Invoke the stylesheet by applying templates to a supplied input sequence, returning the raw results
        /// as an <see cref="XdmValue"/>. The invocation uses any initial mode set using <see cref="InitialMode"/>,
        /// and any template parameters set using <see cref="SetInitialTemplateParameters"/>.
        /// </summary>
        /// <param name="selection">The initial value to which templates are to be applied (equivalent to the <code>select</code>
        /// attribute of <code>xsl:apply-templates</code>)</param>
        /// <returns>the raw result of applying templates to the supplied selection value, without wrapping in
        /// a document node or serializing the result. If there is more than one item in the selection, the result
        /// is the concatenation of the results of applying templates to each item in turn.</returns>
        /// <remarks>
        /// <para>This method is unsuitable for performing a streamed transformation.</para>
        /// <para>Note that this method does not cause the supplied input to become the global context item
        /// (that is, to be available as the value of "." when evaluating global variables). For this, the
        /// <code>GlobalContextItem</code> property should be set.</para>
        /// </remarks>
        /// 
        public XdmValue ApplyTemplates(XdmValue selection) {
            return s9Transformer.applyTemplates(selection);
        }


        /// <summary> Invoke a transformation by calling a named template. The results of calling
        /// the template are wrapped in a document node, which is then sent to the specified
        /// destination. If <see cref="SetInitialTemplateParameters"/> has been
        /// called, then the parameters supplied are made available to the called template (no error
        /// occurs if parameters are supplied that are not used).</summary> 
        /// <param name="templateName"> The name of the initial template. This must match the name of a
        /// public named template in the stylesheet. If the value is null,
        /// the QName <code>xsl:initial-template</code> is used.</param>
        /// <param name="destination"> The destination of the result document produced by wrapping the result 
        /// of the apply-templates call in a document node.  If the destination is a <see cref="Serializer"/>, 
        /// then the serialization parameters set in the serializer are combined with those defined in the stylesheet
        /// (the parameters set in the serializer take precedence).</param> 
        public void CallTemplate(QName templateName, IDestination destination) {
            templateName ??= new QName("xsl", NamespaceConstant.XSLT, "initial-template");
            s9Transformer.callTemplate(templateName, destination.UnderlyingDestination);
        }


        /// <summary>
        /// Invoke a transformation by calling a named template. The results of calling
        /// the template are returned as a raw value, without wrapping in a document node
        /// or serializing.
        /// </summary>
        /// <param name="templateName">The name of the initial template. This must match the name of a
        /// public named template in the stylesheet. If the value is null, the QName <c>xsl:initial-template</c> is used.</param>
        /// <returns>the raw results of the called template, without wrapping in a document node or serialization.</returns>
        public XdmValue CallTemplate(QName templateName) {
            templateName ??= new QName("xsl", NamespaceConstant.XSLT, "initial-template");
            return s9Transformer.callTemplate(templateName);
        }


        /// <summary> Call a public user-defined function in the stylesheet. </summary>
        /// <param name="function"> The name of the function to be called</param>
        /// <param name="arguments">  The values of the arguments to be supplied to the function. These
        /// will be converted if necessary to the type as defined in the function signature, using
        /// the function conversion rules.</param>
        /// <returns> the result of calling the function. This is the raw result, without wrapping in a document
        /// node and without serialization.</returns>
        public XdmValue CallFunction(QName function, XdmValue[] arguments) {
            return s9Transformer.callFunction(function, arguments);
        }


        /// <summary>Call a public user-defined function in the stylesheet, wrapping the result in an XML document, 
        /// and sending this document to a specified destination</summary>    
        /// <param name="function"> The name of the function to be called</param>
        /// <param name="arguments"> The values of the arguments to be supplied to the function. These
        /// will be converted if necessary to the type as defined in the function signature, using
        /// the function conversion rules.</param>
        /// <param name="destination"> The destination of the result document produced by wrapping the 
        /// result of the apply-templates call in a document node.  If the destination is a <see cref="Serializer"/>, 
        /// then the serialization parameters set in the serializer are combined with those defined in the stylesheet
        /// (the parameters set in the serializer take precedence).</param>
        public void CallFunction(QName function, XdmValue[] arguments, IDestination destination) {
            s9Transformer.callFunction(function, arguments, destination.UnderlyingDestination);
        }


        protected internal static UnparsedTextURIResolver WrapTextResolver(Saxon.Api.ResourceResolver value) {
            return new WrappedTextResolver(value);
        }
        
        /// <summary>
        /// An implementation of <c>UnparsedTextURIResolver</c> that wraps (and delegates to)
        /// a supplied <c>Saxon.Api.ResourceResolver</c>
        /// </summary>
        internal class WrappedTextResolver : UnparsedTextURIResolver {
            
            private readonly Saxon.Api.ResourceResolver resolver;

            /// <summary>
            /// Create a wrapped text resolver
            /// </summary>
            /// <param name="resolver">the resolver to be wrapped</param>
            public WrappedTextResolver(Saxon.Api.ResourceResolver resolver) {
                this.resolver = resolver;
            }
            
            /// <summary>
            /// Resolve a URI for an unparsed text document
            /// </summary>
            /// <param name="absoluteURI">The requested URI, made absolute</param>
            /// <param name="encoding">The requested encoding if any (or null if absent)</param>
            /// <param name="config">The Saxon configuration</param>
            /// <returns>A reader giving access to the text</returns>
            /// <exception cref="Saxon.Hej.trans.XPathException">If the encoding is not available,
            /// or if the returned resource is not a text resource.</exception>

            public TextReader resolve(Uri absoluteURI, string encoding, Configuration config) {
                Encoding enc;
                if (encoding == null) {
                    enc = null;
                }
                else {
                    if (encoding == "iso-646") {
                        encoding = "us-ascii";
                    }
                    try {
                        enc = Encoding.GetEncoding((string)encoding);
                    }
                    catch (System.ArgumentException e) {
                        throw new Saxon.Hej.trans.XPathException("Unparsed-text encoding problem: " + e.Message, "FOUT1190");
                    }
                }
                Saxon.Api.ResourceRequest request = new() {
                    BaseUri = absoluteURI,
                    Uri = absoluteURI,
                    Encoding = enc,
                    Nature = ResourceRequest.TEXT_NATURE
                };
                IResource resource = resolver.Invoke(request);
                return resource switch {
                    null => null,
                    TextResource text => text.TextReader,
                    _ => throw new Saxon.Hej.trans.XPathException(
                        "Resolver for unparsed-text URI " + absoluteURI + " returned an IResource that is not a TextResource",
                        "FOUT1170")
                };
            }
        }
        
 

    }
    
    
}

// Copyright (c) 2021 - 2023 Saxonica Limited
