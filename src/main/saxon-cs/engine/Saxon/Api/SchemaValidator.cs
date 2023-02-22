using System;
using System.Collections.Generic;
using System.Impl.Helpers;
using System.IO;
using System.Xml;
using javax.xml.transform;
using javax.xml.transform.stream;
using net.sf.saxon;
using net.sf.saxon.@event;
using net.sf.saxon.lib;
using net.sf.saxon.om;
using net.sf.saxon.pull;
using net.sf.saxon.type;

namespace Saxon.Api {
    /// <summary>
    /// A <c>SchemaValidator</c> is an object that is used for validating instance documents
    /// against a schema. The schema consists of the collection of schema components that are
    /// available within the schema cache maintained by the <c>SchemaManager</c>, together with
    /// any additional schema components located during the course of validation by means of an
    /// <c>xsl:schemaLocation</c> or <c>xsi:noNamespaceSchemaLocation</c> attribute within the
    /// instance document.
    /// </summary>
    /// <remarks>
    /// If validation fails, an exception is thrown. If validation succeeds, the validated
    /// document can optionally be written to a specified destination. This will be a copy of
    /// the original document, augmented with default values for absent elements and attributes,
    /// and carrying type annotations derived from the schema processing. Saxon does not deliver
    /// the full PSVI as described in the XML schema specifications, only the subset of the
    /// PSVI properties featured in the XDM data model.
    /// </remarks>    
    [Serializable]
    public class SchemaValidator {
        private Configuration config;
        private bool lax = false;
        private Source source;
        private List<Source> sources = new List<Source>();
        private IList<ValidationFailure> errorList = null;
        private net.sf.saxon.s9api.SchemaValidator jSchemaValidator;
        private SchemaType documentElementType;

        // internal constructor

        internal SchemaValidator(net.sf.saxon.s9api.SchemaValidator validator, Processor processor) {
            this.jSchemaValidator = validator;
            this.config = processor.Implementation;
        }

        /// <summary>
        /// The validation mode may be either strict or lax. The default is strict;
        /// this property is set to indicate that lax validation is required. With strict validation,
        /// validation fails if no element declaration can be located for the outermost element. With lax
        /// validation, the absence of an element declaration results in the content being considered valid.
        /// </summary>

        public bool IsLax {
            get => jSchemaValidator.isLax();
            set => jSchemaValidator.setLax(value);
        }

        /// <summary>
        /// This property defines whether the schema processor will recognize, and attempt to
        /// dereference, any <c>xsi:schemaLocation</c> and <c>xsi:noNamespaceSchemaLocation</c>
        /// attributes encountered in the instance document. The default value is true.
        /// </summary>

        public bool UseXsiSchemaLocation {
            get => jSchemaValidator.isUseXsiSchemaLocation();
            set => jSchemaValidator.setUseXsiSchemaLocation(value);
        }


        /// <summary>Set the validation reporting feature, which saves the validation errors in an XML file</summary>
        /// <param name="destination"> destination where XML will be sent</param>
        public void SetValidityReporting(XmlDestination destination) {
            jSchemaValidator.setValidityReporting(destination.GetUnderlyingDestination());
        }


        /// <summary>Set the <c>InvalidityHandler</c> to be used when validating instance documents</summary>
        /// <param name="inHandler">the <c>InvalidityHandler</c> to be used</param>
        public void SetInvalidityHandler(IInvalidityHandler inHandler) {
            jSchemaValidator.setInvalidityHandler(new InvalidityHandlerWrapper(inHandler));
        }

        /// <summary>
        /// Add an instance document to the list of documents to be validated.
        /// </summary>
        /// <param name="source">Source document supplied as a <c>Stream</c></param>
        /// <param name="baseUri">Base URI of the source document</param>
        public void AddSource(Stream source, Uri baseUri) {
            StreamSource ss = new StreamSource(source);
            ss.setSystemId(baseUri.ToString());
            sources.Add(ss);
        }

        /// <summary>
        /// Add an instance document to the list of documents to be validated
        /// </summary>
        /// <param name="uri">URI of the source document</param>
        public void AddSource(Uri uri) {
            StreamSource ss = new StreamSource(uri.ToString());
            AugmentedSource aug = AugmentedSource.makeAugmentedSource(ss);
            aug.setPleaseCloseAfterUse(true);
            sources.Add(aug);
        }

        /// <summary>
        /// Add an instance document to the list of documents to be validated
        /// </summary>
        /// <param name="reader">Source document supplied as an <c>XmlReader</c></param>
        public void AddSource(XmlReader reader) {
            PullProvider pp = new DotNetPullProvider(reader);
            PipelineConfiguration pipe = config.makePipelineConfiguration();
            pipe.setUseXsiSchemaLocation(jSchemaValidator.isUseXsiSchemaLocation());
            pp.setPipelineConfiguration(pipe);
            // pp = new PullTracer(pp);  /* diagnostics */
            PullSource pSource = new PullSource(pp);
            pSource.setSystemId(reader.BaseURI);
            sources.Add(pSource);
        }

        /// <summary>
        /// Add an instance document to the list of documents to be validated.
        /// </summary>
        /// <param name="source">Source document supplied as an <c>XdmNode</c> value</param>
        public void AddSource(XdmNode source) {
            sources.Add((NodeInfo) source.value);
        }

        /// <summary>
        /// Supply the instance document to be validated in the form of a <c>Stream</c>
        /// </summary>
        /// <param name="source">A stream containing the XML document to be parsed
        /// and validated. This stream will be consumed by the validation process,
        /// but it will not be closed after use: that is the responsibility of the
        /// caller.</param>
        /// <param name="baseUri">The base URI to be used for resolving any relative
        /// references, for example a reference to an <c>xsi:schemaLocation</c></param>                  
        public void SetSource(Stream source, Uri baseUri) {
            StreamSource ss = new StreamSource(source);
            ss.setSystemId(baseUri.ToString());
            this.source = ss;
            sources.Clear();
        }

        /// <summary>
        /// Supply the instance document to be validated in the form of a <c>Uri</c> reference
        /// </summary>
        /// <param name="uri">URI of the document to be validated</param>                  
        public void SetSource(Uri uri) {
            StreamSource ss = new StreamSource(uri.ToString());
            AugmentedSource aug = AugmentedSource.makeAugmentedSource(ss);
            aug.setPleaseCloseAfterUse(true);
            this.source = aug;
            sources.Clear();
        }

        /// <summary>
        /// Supply the instance document to be validated, in the form of an <c>XmlReader</c>.
        /// </summary>
        /// <remarks>
        /// The <c>XmlReader</c> is responsible for parsing the document; this method validates it.
        /// </remarks>
        /// <param name="reader">The <c>XmlReader</c> used to read and parse the instance
        /// document being validated. This is used as supplied. For conformance, use of a
        /// plain <c>XmlTextReader</c> is discouraged, because it does not expand entity
        /// references. This may cause validation failures.
        /// </param>
        public void SetSource(XmlReader reader) {
            PullProvider pp = new DotNetPullProvider(reader);
            PipelineConfiguration pipe = config.makePipelineConfiguration();
            pipe.setUseXsiSchemaLocation(jSchemaValidator.isUseXsiSchemaLocation());
            pp.setPipelineConfiguration(pipe);
            // pp = new PullTracer(pp);  /* diagnostics */
            PullSource psource = new PullSource(pp);
            psource.setSystemId(reader.BaseURI);
            this.source = psource;
            sources.Clear();
        }

        /// <summary>
        /// Supply the instance document to be validated in the form of an <c>XdmNode</c>.
        /// </summary>
        /// <remarks>
        /// <para>The supplied node must be either a document node or an element node.
        /// If an element node is supplied, then the subtree rooted at this element is
        /// validated as if it were a complete document: that is, it must not only conform
        /// to the structure required of that element, but any referential constraints
        /// (keyref, IDREF) must be satisfied within that subtree.
        /// </para>
        /// </remarks>
        /// <param name="source">The document or element node at the root of the tree
        /// to be validated</param>        
        public void SetSource(XdmNode source) {
            this.source = (NodeInfo) source.value;
            sources.Clear();
        }

        /// <summary>
        /// Supply the destination to hold the validated document. If no destination
        /// is supplied, the validated document is discarded.
        /// </summary>
        /// <remarks>
        /// The destination differs from the source in that (a) default values of missing
        /// elements and attributes are supplied, and (b) the typed values of elements and
        /// attributes are available. However, typed values can only be accessed if the result
        /// is represented using the XDM data model, that is, if the destination is supplied
        /// as an <c>XdmDestination</c>.
        /// </remarks>
        /// <param name="destination">
        /// The destination to hold the validated document.
        /// </param>
        public void SetDestination(XmlDestination destination) {
            jSchemaValidator.setDestination(destination.GetUnderlyingDestination());
        }

        /// <summary>
        /// Close the destination, allowing resources to be released. Saxon calls this method
        /// when it has finished writing to the destination
        /// </summary>
        public void Close() {
            jSchemaValidator.close();
        }


        /// <summary>
        /// The name of the required top-level element of the document to be validated (that is, the
        /// name of the outermost element of the document).
        /// if no value is supplied, there is no constraint on the required type
        /// </summary>
        public QName DocumentElementName {
            get {
                net.sf.saxon.s9api.QName qname = jSchemaValidator.getDocumentElementName();
                return qname == null ? null : new QName(qname.getEQName());
            }
            set { jSchemaValidator.setDocumentElementName(value.UnderlyingQName()); }
        }


        /// <summary>
        /// The name of the required type of the top-level element of the document to be validated.
        /// QName of the name of the required type of the document element, or null if no value has been set.
        /// If no value is supplied, there is no constrin on the required type
        /// </summary>
        public QName DocumentElementTypeName {
            get {
                net.sf.saxon.s9api.QName qtname = jSchemaValidator.getDocumentElementTypeName();
                return qtname == null ? null : new QName(qtname.getEQName());
            }
            set {
                jSchemaValidator.setDocumentElementTypeName(value.UnderlyingQName());
                documentElementType = config.getSchemaType(value.GetStructuredQName());
            }
        }

        internal SchemaType DocumentElementType {
            get { return documentElementType; }
        }

        internal net.sf.saxon.s9api.SchemaValidator UnderlyingSchemaValidator {
            get { return jSchemaValidator; }
        }


        /// <summary>
        /// Set the value of a schema parameter (a parameter defined in the schema using the <c>saxon:param</c> extension)
        /// </summary>
        /// <param name="name">the name of the schema parameter, as a QName</param>
        /// <param name="value">the value of the schema  parameter, or null to clear a previously set value</param>
        public void SetParameter(QName name, XdmValue value) {
            try {
                jSchemaValidator.setParameter(name.UnderlyingQName(),
                    value == null ? null : XdmValue.FromGroundedValueToJXdmValue(value.value));
            }
            catch (net.sf.saxon.s9api.SaxonApiUncheckedException ex) {
                throw new StaticError(ex);
            }
        }


        /// <summary>
        /// Get the value that has set for a schema processor (a parameter defined in the schema using the <c>saxon:param</c>
        /// extension)
        /// </summary>
        /// <param name="name">the parameter whose name is required</param>
        /// <returns>the value that has been set for the parameter, or the EmptySequence if no value has been set</returns>
        public XdmValue GetParameter(QName name) {
            net.sf.saxon.s9api.XdmValue value = jSchemaValidator.getParameter(name.UnderlyingQName());
            return value == null ? null : XdmValue.Wrap(value.getUnderlyingValue());
        }


        /// <summary>
        /// Ask and set whether attribute defaults defined in a schema are to be expanded or not
        /// (by default, fixed and default attribute values are expaned, that is, they are inserted) into
        /// the document during validation as if they were present in the instance being validated)
        /// </summary>
        public bool ExpendAttributeDefaults {
            get { return jSchemaValidator.isExpandAttributeDefaults(); }
            set { jSchemaValidator.setExpandAttributeDefaults(value); }
        }

        /// <summary>
        /// List of errors. The caller may supply an empty list before calling <c>Compile</c>;
        /// the processor will then populate the list with error information obtained during
        /// the schema compilation. Each error will be included as an object of type <c>StaticError</c>.
        /// If no error list is supplied by the caller, error information will be written to
        /// the standard error stream.
        /// </summary>
        /// <remarks>
        /// <para>By supplying a custom <c>List</c> with a user-written <c>add()</c> method, it is possible to
        /// intercept error conditions as they occur.</para>
        /// <para>Note that this error list is used only for errors detected while 
        /// using the schema to validate a source document. It is not used to report errors
        /// in the schema itself.</para>
        /// </remarks>

        public IList<ValidationFailure> ErrorList {
            set {
                errorList = value;
                jSchemaValidator.setInvalidityHandler(new InvalidityGatherer(errorList));
            }
            get { return errorList; }
        }


        /// <summary>
        /// Run the validation of the supplied source document, optionally
        /// writing the validated document to the supplied destination.
        /// </summary>
        public void Run() {
            if (source == null) {
                if (sources.Count == 0) {
                    throw new StaticError(
                        new net.sf.saxon.trans.XPathException("No source(s) set for the SchemaValidator"));
                }

                try {
                    jSchemaValidator.validateMultiple(sources);
                }
                catch (net.sf.saxon.s9api.SaxonApiException ex) {
                    throw new StaticError(ex);
                }
            }
            else {
                AugmentedSource aug = AugmentedSource.makeAugmentedSource(source);
                aug.setSchemaValidationMode(lax ? Validation.LAX : Validation.STRICT);
                try {
                    jSchemaValidator.validate(aug);
                }
                catch (net.sf.saxon.s9api.SaxonApiException ex) {
                    throw new StaticError(ex);
                }
            }
        }
    }
}