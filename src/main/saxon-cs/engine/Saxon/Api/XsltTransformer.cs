using System;
using System.IO;
using System.Xml;
using java.util.function;
using Sys = System;
using javax.xml.transform.stream;
using net.sf.saxon;
using net.sf.saxon.@event;
using net.sf.saxon.lib;
using net.sf.saxon.om;
using net.sf.saxon.s9api;
using net.sf.saxon.trans;
using Saxon.Api.Support;
using Saxon.Callbacks;
using Saxon.Impl.Helpers;
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
    public class XsltTransformer : XmlDestination {
        private XmlDestination xmlDestination;
        private StandardLogger traceFunctionDestination;
        private IResultDocumentHandler resultDocumentHandler;
        private QName initialTemplateName;
        private XdmNode initialContextNode;
        private net.sf.saxon.s9api.XsltTransformer transformer;
        private readonly StandardMessageHandler _standardMessageHandler;

        // internal constructor

        internal XsltTransformer(net.sf.saxon.s9api.XsltTransformer transformer) {
            this.transformer = transformer;
            this._standardMessageHandler = new StandardMessageHandler(transformer.controller.getConfiguration());
            transformer.controller.setMessageHandler(message => this._standardMessageHandler.accept(message));
        }

        /// <summary>
        /// The global context item for the stylesheet, as a node.
        /// </summary>
        /// <remarks><para>Although XSLT 3.0 allows the global context item to be any item,
        /// this interface only allows it to be a node.
        /// Most commonly it will be a document node, which might be constructed
        /// using the <c>Build</c> method of the <c>DocumentBuilder</c> object.</para>
        /// <para>Note that this can be inefficient if the stylesheet uses <c>xsl:strip-space</c>
        /// to strip whitespace, or <c>input-type-annotations="strip"</c> to remove type
        /// annotations, since this will result in the transformation operating on a virtual document
        /// implemented as a view or wrapper of the supplied document.</para>
        /// </remarks>

        public XdmNode InitialContextNode {
            get => transformer.getInitialContextNode();
            set => transformer.setInitialContextNode(value);
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
                Uri.EscapeUriString(baseUri.ToString()));
            transformer.setSource(streamSource);
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
            get => transformer.getInitialMode();
            set => transformer.setInitialMode(value);
        }


        /// <summary>
        /// The initial template for the stylesheet. This is either a <c>QName</c>, for a 
        /// named template, or null, if no initial template has been set.
        /// </summary>
        /// <remarks>
        /// If the stylesheet is to be invoked by calling the template named <c>xsl:initial-template</c>,
        /// then the <c>InitialTemplate</c> property should be set to this <c>QName</c> explicitly.
        /// </remarks>
        /// <exception cref="DynamicError">Setting this property to the name of a template
        /// that does not exist in the stylesheet throws a <c>DynamicError</c> with error 
        /// code XTDE0040. Setting it to the name of a template that has template
        /// parameters throws a <c>DynamicError</c> with error code XTDE0060.</exception>

        public QName InitialTemplate {
            get => initialTemplateName;
            set {
                initialTemplateName = value;
                transformer.setInitialTemplate(initialTemplateName);
            }
        }

        /// <summary>
        /// The base output URI, which acts as the base URI for resolving the <c>href</c>
        /// attribute of <c>xsl:result-document</c>.
        /// </summary>

        public Uri BaseOutputUri {
            get => new Uri(transformer.getBaseOutputURI());
            set => transformer.setBaseOutputURI(value.ToString());
        }


        /// <summary>
        /// The <c>SchemaValidationMode</c> to be used in this transformation, especially for documents
        /// loaded using the <c>doc()</c>, <c>document()</c>, or <c>collection()</c> functions.
        /// </summary>

        public SchemaValidationMode SchemaValidationMode {
            get {
                switch (transformer.getUnderlyingController().getSchemaValidationMode()) {
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
                        transformer.setSchemaValidationMode(ValidationMode.STRICT);
                        break;
                    case SchemaValidationMode.Lax:
                        transformer.setSchemaValidationMode(ValidationMode.LAX);
                        break;
                    case SchemaValidationMode.None:
                        transformer.setSchemaValidationMode(ValidationMode.STRIP);
                        break;
                    case SchemaValidationMode.Preserve:
                        transformer.setSchemaValidationMode(ValidationMode.PRESERVE);
                        break;
                    case SchemaValidationMode.Unspecified:
                    default:
                        transformer.setSchemaValidationMode(ValidationMode.DEFAULT);
                        break;
                }
            }
        }


        /// <summary>
        /// The <c>XmlResolver</c> to be used at run-time to resolve and dereference URIs
        /// supplied to the <c>doc()</c> and <c>document()</c> functions.
        /// </summary>

        public XmlResolver InputXmlResolver {
            get => ((StandardURIResolver) transformer.getURIResolver()).getXmlResolver();
            set => transformer.setURIResolver(
                new StandardURIResolver(
                    transformer.getUnderlyingController().getConfiguration(),
                    value));
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

        public IResultDocumentHandler ResultDocumentHandler {
            get => resultDocumentHandler;
            set {
                resultDocumentHandler = value;
                transformer.getUnderlyingController().setResultDocumentResolver(
                    new ResultDocumentHandlerWrapper(value,
                        transformer.getUnderlyingController().makePipelineConfiguration()));
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

        public Consumer<Message> MessageListener {
            get {
                return transformer.controller.getMessageHandler();
            }
            set {
                transformer.controller.setMessageHandler(value);
            }
        }


        /// <summary>
        /// Destination for output of messages using the <c>trace()</c> function.
        /// </summary>
        /// <remarks> 
        /// <para>If no message listener is supplied by the caller, message information will be written to
        /// the standard error stream.</para>
        /// <para>The supplied destination is ignored if a <c>TraceListener</c> is in use.</para>
        /// </remarks>

        public StandardLogger TraceFunctionDestination {
            set {
                traceFunctionDestination = value;
                transformer.setTraceFunctionDestination(value);
            }
            get { return traceFunctionDestination; }
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
            transformer.setParameter(name, value);
        }

        public Destination GetUnderlyingDestination() {
            return transformer;
        }

        /// <summary>
        /// The destination for the result of the transformation. The class <c>XmlDestination</c> is an abstraction 
        /// that allows a number of different kinds of destination to be specified.
        /// </summary>
        /// <remarks>
        /// <para>The Destination can be used to chain transformations into a pipeline, by using one
        /// <c>XsltTransformer</c> as the destination of another.</para>
        /// </remarks>

        public XmlDestination Destination {
            get => this.xmlDestination;
            set {
                this.xmlDestination = value;
                transformer.setDestination(value.GetUnderlyingDestination());
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
            transformer.close();
        }

        // internal method

        internal Receiver GetDestinationReceiver(XmlDestination destination) {
            Controller controller = transformer.getUnderlyingController();
            PipelineConfiguration pipe = makePipelineConfiguration();
            Receiver r = destination.GetUnderlyingDestination()
                .getReceiver(pipe, controller.getExecutable().getPrimarySerializationProperties());
            pipe.setController(controller);
            return r;
            //}
        }

        internal PipelineConfiguration makePipelineConfiguration() {
            return transformer.getUnderlyingController().getConfiguration().makePipelineConfiguration();
        }

        /// <summary>
        /// Run the transformation, sending the result to a specified destination.
        /// </summary>
        /// <param name="destination">
        /// The destination for the results of the stylesheet. The class <c>XmlDestination</c>
        /// is an abstraction that allows a number of different kinds of destination
        /// to be specified.
        /// </param>
        /// <exception cref="DynamicError">Throws a <c>DynamicError</c> if the transformation
        /// fails.</exception>
        public void Run(XmlDestination destination) {
            if (destination == null) {
                throw new DynamicError("Destination is null");
            }

            try {
                transformer.setDestination(destination.GetUnderlyingDestination());
                transformer.transform();
            }
            catch (SaxonApiException err) {
                throw new DynamicError(err);
            }
        }

        /// <summary>
        /// Escape hatch to the underlying implementation
        /// </summary>

        public XsltController Implementation => transformer.getUnderlyingController();
    }
}