using System;
using System.IO;
using System.Text;
using System.Xml;
using Saxon.Ejava.util.function;
using Sys = System;
using Saxon.Ejavax.xml.transform.stream;
using Saxon.Hej;
using Saxon.Hej.@event;
using Saxon.Hej.lib;
using Saxon.Hej.s9api;
using Saxon.Hej.trans;
using Saxon.Api.Support;
using Saxon.Callbacks;
using StandardMessageHandler = Saxon.Callbacks.StandardMessageHandler;

namespace Saxon.Api {
    /// <summary inherits="XdmDestination">
    /// An <c>XsltTransformer</c> represents a compiled and loaded stylesheet ready for execution.
    /// The <c>XsltTransformer</c> holds details of the dynamic evaluation context for the stylesheet.
    /// </summary>
    /// <remarks>
    /// <para>An <c>XsltTransformer</c> must not be used concurrently in multiple threads. It is safe,
    /// however, to reuse the object within a single thread to run the same stylesheet several times.
    /// Running the stylesheet does not change the context that has been established.</para>
    /// <para>An <c>XsltTransformer</c> is always constructed by running the <c>Load</c> method of
    /// an <c>XsltExecutable</c>.</para>
    /// <para>The <see cref="Xslt30Transformer"/> class provides invocation options that are more closely aligned
    /// with the XSLT 3.0 specification, for example streamed evaluation. However, both <c>XsltTransformer</c> 
    /// and <c>Xslt30Transformer</c> can be used irrespective of the XSLT language version used in the stylesheet.</para>
    /// </remarks>
    [Serializable]
    public class XsltTransformer : IDestination {
        private IDestination xmlDestination;
        private TextWriter traceFunctionDestination;
        private ResultDocumentHandler resultDocumentHandler;
        private QName initialTemplateName;
        private Saxon.Hej.s9api.XsltTransformer s9Transformer;
        private readonly StandardMessageHandler _standardMessageHandler;
        private ResourceResolver textResolver;
        private ResourceResolver xmlDocumentResolver;
        private ErrorReporter errorReporter;

        // internal constructor

        internal XsltTransformer(Saxon.Hej.s9api.XsltTransformer transformer) {
            this.s9Transformer = transformer;
            this._standardMessageHandler = new StandardMessageHandler(transformer.controller.getConfiguration());
            transformer.controller.setMessageHandler(message => this._standardMessageHandler.accept(message));
        }

        /// <summary>
        /// The initial context item for the stylesheet, as a node.
        /// </summary>
        /// <remarks><para>Although XSLT 3.0 allows the global context item to be any item,
        /// this interface only allows it to be a node.
        /// Most commonly it will be a document node, which might be constructed
        /// using the <c>Build</c> method of the <c>DocumentBuilder</c> object.</para>
        /// <para>Note that this can be inefficient if the stylesheet uses <c>xsl:strip-space</c>
        /// to strip whitespace, or <c>input-type-annotations="strip"</c> to remove type
        /// annotations, since this will result in the transformation operating on a virtual document
        /// implemented as a view or wrapper of the supplied document.</para>
        /// <para>The name of this property is potentially misleading. If the supplied node is not
        /// a document node (for example, if it is the outermost element in the document), then
        /// the global context item for the transformation (that is, the value of "." when referenced
        /// withing a global variable in the stylesheet) will actually be the root node of the
        /// containing document tree. The supplied element node, however, is used as the
        /// "initial match selection": that is, the transformation will start by finding a template
        /// rule that matches the element, and evaluating that template with the element as its
        /// context item.</para> 
        /// </remarks>

        public XdmNode InitialContextNode {
            get => s9Transformer.getInitialContextNode();
            set => s9Transformer.setInitialContextNode(value);
        }

        /// <summary>
        /// Supply the principal input document for the transformation in the form of a stream.
        /// </summary>
        /// <remarks>
        /// <para>If this method is used, the <c>InitialContextNode</c> is ignored.</para>
        /// <para>The supplied stream will be consumed by the <c>Run()</c> method.
        /// Closing the input stream after use is the client's responsibility.</para>
        /// <para>A base URI must be supplied in all cases. It is used to resolve relative
        /// URI references appearing within the input document.</para>
        /// <para>Schema validation is applied to the input document according to the value of
        /// the <c>SchemaValidationMode</c> property.</para>
        /// <para>Whitespace stripping is applied according to the value of the
        /// <c>xsl:strip-space</c> and <c>xsl:preserve-space</c> declarations in the stylesheet.</para>
        /// </remarks>
        /// <param name="input">
        /// The stream containing the source code of the principal input document to the transformation. The document
        /// node at the root of this document will be the global context item for the transformation.
        /// </param>
        /// <param name="baseUri">
        /// The base URI of the principal input document. This is used for example by the <c>document()</c>
        /// function if the document contains links to other documents in the form of relative URIs.</param>
        public void SetInputStream(Stream input, Uri baseUri) {
            StreamSource streamSource = new StreamSource(input,
                baseUri.ToString());
            s9Transformer.setSource(streamSource);
        }

        /// <summary>
        /// Supply the principal input document for the transformation in the form of a <c>TextReader</c>.
        /// </summary>
        /// <remarks>
        /// <para>If this method is used, the <c>InitialContextNode</c> is ignored.</para>
        /// <para>The supplied reader will be consumed by the <c>Run()</c> method.
        /// Closing the input reader after use is the client's responsibility.</para>
        /// <para>A base URI must be supplied in all cases. It is used to resolve relative
        /// URI references appearing within the input document.</para>
        /// <para>Schema validation is applied to the input document according to the value of
        /// the <c>SchemaValidationMode</c> property.</para>
        /// <para>Whitespace stripping is applied according to the value of the
        /// <c>xsl:strip-space</c> and <c>xsl:preserve-space</c> declarations in the stylesheet.</para>
        /// </remarks>
        /// <param name="input">
        /// The reader containing the source XML of the principal input document to the transformation. The document
        /// node at the root of this document will be the global context item for the transformation.
        /// </param>
        /// <param name="baseUri">
        /// The base URI of the principal input document. This is used for example by the <c>document()</c>
        /// function if the document contains links to other documents in the form of relative URIs.</param>
        public void SetInputTextReader(TextReader input, Uri baseUri) {
            StreamSource streamSource = new StreamSource(input,
                baseUri.ToString());
            s9Transformer.setSource(streamSource);
        }

        /// <summary>
        /// The initial mode for the stylesheet. This is either a <c>QName</c>, for a 
        /// specific mode, or null, for the default mode.
        /// </summary>
        /// <remarks>
        /// The default mode will usually be the unnamed mode, but if the stylesheet declares a
        /// named mode as the default mode, then supplying null as the <c>InitialMode</c> invokes this default.
        /// </remarks>

        public QName InitialMode {
            get => s9Transformer.getInitialMode();
            set => s9Transformer.setInitialMode(value);
        }


        /// <summary>
        /// The initial template for the stylesheet. This is either a <c>QName</c>, for a 
        /// named template, or null, if no initial template has been set.
        /// </summary>
        /// <remarks>
        /// If the stylesheet is to be invoked by calling the template named <c>xsl:initial-template</c>,
        /// then the <c>InitialTemplate</c> property should be set to this <c>QName</c> explicitly.
        /// </remarks>
        /// <exception cref="SaxonApiException">Setting this property to the name of a template
        /// that does not exist in the stylesheet throws a <c>SaxonApiException</c> with error 
        /// code XTDE0040. Setting it to the name of a template that has template
        /// parameters throws a <c>SaxonApiException</c> with error code XTDE0060.</exception>

        public QName InitialTemplate {
            get => initialTemplateName;
            set {
                initialTemplateName = value;
                s9Transformer.setInitialTemplate(initialTemplateName);
            }
        }

        /// <summary>
        /// The base output URI, which acts as the base URI for resolving the <c>href</c>
        /// attribute of <c>xsl:result-document</c>.
        /// </summary>

        public Uri BaseOutputUri {
            get => new Uri(s9Transformer.getBaseOutputURI());
            set => s9Transformer.setBaseOutputURI(value.ToString());
        }


        /// <summary>
        /// The <c>SchemaValidationMode</c> to be used in this transformation, especially for documents
        /// loaded using the <c>doc()</c>, <c>document()</c>, or <c>collection()</c> functions.
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
                    new ResultDocumentHandlerWrapper(value,
                        s9Transformer.getUnderlyingController().makePipelineConfiguration()));
            }
        }

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
        /// Destination for output of messages using the <c>trace()</c> function.
        /// </summary>
        /// <remarks> 
        /// <para>If no value is supplied by the caller, trace information will be written to
        /// the standard error stream.</para>
        /// <para>The supplied destination is ignored if a <c>TraceListener</c> is in use.</para>
        /// <para>The <c>TextWriter</c> should be closed by the caller after use.</para>
        /// </remarks>

        public TextWriter TraceFunctionDestination {
            set {
                traceFunctionDestination = value;
                s9Transformer.setTraceFunctionDestination(new StandardLogger(value));
            }
            get => traceFunctionDestination;
        }
        
        /// <summary>
        /// The <code>ErrorReporter</code> to be used at run-time to notify any dynamic
        /// errors that occur.
        /// </summary>

        public ErrorReporter ErrorReporter {
            get => errorReporter;
            set {
                errorReporter = value;
                s9Transformer.setErrorReporter(new ErrorReporterWrapper(value));
            }
        }

        /// <summary>
        /// Set the value of a stylesheet parameter.
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
            s9Transformer.setParameter(name, value);
        }

        /// <summary>
        /// The underlying implementation object. Although this method is public, it is intended
        /// for advanced applications only, and should not be considered stable from release to release.
        /// </summary>
        /// <value>
        ///     The underlying implementation object, which will be an instance of the class
        ///     <c>Saxon.Hej.s9api.XsltTransformer</c>. Information about this class is available in the
        ///     Saxon-EE Javadoc documentation for the very similar Java class <see cref="Saxon.Hej.s9api.XsltTransformer"/>.
        /// </value>
        public Saxon.Hej.s9api.Destination UnderlyingDestination {
            get { return s9Transformer; }
        }

        /// <summary>
        /// The destination for the result of the transformation. The class <c>XmlDestination</c> is an abstraction 
        /// that allows a number of different kinds of destination to be specified.
        /// </summary>
        /// <remarks>
        /// <para>The Destination can be used to chain transformations into a pipeline, by using one
        /// <c>XsltTransformer</c> as the destination of another.</para>
        /// </remarks>

        public IDestination Destination {
            get => this.xmlDestination;
            set {
                this.xmlDestination = value;
                s9Transformer.setDestination(value.UnderlyingDestination);
            }
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

        protected internal static UnparsedTextURIResolver WrapTextResolver(Saxon.Api.ResourceResolver value) {
            return new Xslt30Transformer.WrappedTextResolver(value);
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
        /// Close the <c>Destination</c>, releasing any resources that need to be released.
        /// </summary>
        /// <remarks>
        /// This method is called by the system on completion of a query or transformation.
        /// Some kinds of <c>Destination</c> may need to close an output stream, others might
        /// not need to do anything. The default implementation does nothing.
        /// </remarks>
        public void Close() {
            s9Transformer.close();
        }

        /// <summary>
        /// Define an action to be performed when writing to the Destination is complete.
        /// </summary>
        /// <param name="action">The action to be performed.</param>
        public void OnClose(System.Action action) {
            s9Transformer.onClose(action.Invoke);
        }


        // internal method

        internal Receiver GetDestinationReceiver(IDestination destination) {
            Controller controller = s9Transformer.getUnderlyingController();
            PipelineConfiguration pipe = makePipelineConfiguration();
            Receiver r = destination.UnderlyingDestination
                .getReceiver(pipe, controller.getExecutable().getPrimarySerializationProperties());
            pipe.setController(controller);
            return r;
            //}
        }

        internal PipelineConfiguration makePipelineConfiguration() {
            return s9Transformer.getUnderlyingController().getConfiguration().makePipelineConfiguration();
        }

        /// <summary>
        /// Run the transformation, sending the result to a specified destination.
        /// </summary>
        /// <param name="destination">
        /// The destination for the results of the stylesheet. The class <c>XmlDestination</c>
        /// is an abstraction that allows a number of different kinds of destination
        /// to be specified.
        /// </param>
        /// <exception cref="Saxon.Api.SaxonApiException">Throws a <c>SaxonApiException</c> if the transformation
        /// fails.</exception>
        public void Run(IDestination destination) {
            if (destination == null) {
                throw new SaxonApiException("Destination is null");
            }

            s9Transformer.setDestination(destination.UnderlyingDestination);
            s9Transformer.transform();
        }

        /// <summary>
        /// Run the transformation, sending the result to the destination set in the
        /// <c>Destination</c> property (which must have been given a value).
        /// </summary>
        /// <exception cref="Saxon.Api.SaxonApiException">Throws a <c>SaxonApiException</c> if the transformation
        /// fails, or if no destination has been set.</exception>
        public void Run() {
            if (xmlDestination == null) {
                throw new SaxonApiException("Destination has not been set");
            }

            s9Transformer.setDestination(xmlDestination.UnderlyingDestination);
            s9Transformer.transform();
        }


        /// <summary>
        /// Escape hatch to the underlying implementation
        /// </summary>

        public Saxon.Hej.trans.XsltController Implementation => s9Transformer.getUnderlyingController();
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited
