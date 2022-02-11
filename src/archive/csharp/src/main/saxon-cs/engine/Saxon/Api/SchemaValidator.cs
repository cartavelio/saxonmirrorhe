using System;
using System.Collections.Generic;
using System.Impl.Helpers;
using System.IO;
using System.Xml;
using Saxon.Ejava.util.function;
using Saxon.Ejavax.xml.transform;
using Saxon.Ejavax.xml.transform.stream;
using Saxon.Hej;
using Saxon.Hej.@event;
using Saxon.Hej.lib;
using Saxon.Hej.om;
using Saxon.Hej.pull;
using Saxon.Api.Support;

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
    /// <para>The <c>SchemaValidator.Run</c> method can be used either to validate a single
    /// document (supplied using <c>SetSource()</c>), or multiple documents (supplied using
    /// <c>AddSource</c>.</para>
    ///  
    /// <para>When validating a single document, if validation succeeds, the validated
    /// document can optionally be written to a specified destination. This will be a copy of
    /// the original document, augmented with default values for absent elements and attributes,
    /// and carrying type annotations derived from the schema processing. Saxon does not deliver
    /// the full PSVI as described in the XML schema specifications, only the subset of the
    /// PSVI properties featured in the XDM data model.</para>
    ///
    /// <para>When validating multiple documents, they are validated in parallel (in multiple
    /// threads), and the validated documents are not made available. It is recommended in this
    /// case to use <c>SetValidityReporting</c> to capture any validation errors in a single
    /// XML report document.</para>
    /// 
    /// <para>If validation fails, an exception is thrown. This may indicate invalidity against
    /// the schema, or it might result from other conditions, such as an input document not being
    /// well-formed XML, or not existing.</para>
    /// </remarks>    
    [Serializable]
    public class SchemaValidator : IDestination {
        private Configuration config;
        private List<Source> sources = new();

        // internal constructor

        internal SchemaValidator(Saxon.Hej.s9api.SchemaValidator validator, Processor processor) {
            this.UnderlyingSchemaValidator = validator;
            this.config = processor.Implementation;
        }

        /// <summary>
        /// The validation mode may be either strict or lax. The default is strict;
        /// this property is set to indicate that lax validation is required. With strict validation,
        /// validation fails if no element declaration can be located for the outermost element. With lax
        /// validation, the absence of an element declaration results in the content being considered valid.
        /// </summary>

        public bool Lax {
            get => UnderlyingSchemaValidator.isLax();
            set => UnderlyingSchemaValidator.setLax(value);
        }

        /// <summary>
        /// This property defines whether the schema processor will recognize, and attempt to
        /// dereference, any <c>xsi:schemaLocation</c> and <c>xsi:noNamespaceSchemaLocation</c>
        /// attributes encountered in the instance document. The default value is true.
        /// </summary>

        public bool UseXsiSchemaLocation {
            get => UnderlyingSchemaValidator.isUseXsiSchemaLocation();
            set => UnderlyingSchemaValidator.setUseXsiSchemaLocation(value);
        }


        /// <summary>Set the validation reporting feature, which saves the validation errors in an XML file</summary>
        /// <param name="destination"> destination where XML will be sent</param>
        public void SetValidityReporting(IDestination destination) {
            UnderlyingSchemaValidator.setValidityReporting(destination.UnderlyingDestination);
        }
        
        /// <summary>Set the <c>InvalidityHandler</c> to be used when validating instance documents;
        /// the invalidity handler is called once at the start of validation, once for each
        /// validation failure, and once at the end.</summary>
        public IInvalidityHandler InvalidityHandler {
            set => UnderlyingSchemaValidator.setInvalidityHandler(new InvalidityHandlerWrapper(value));
        }
        
        /// <summary>Set the <c>InvalidityListener</c> to be used when validating instance documents;
        /// this is a simple delegate callback which is called once for each validation failure.</summary>
        /// <remarks>Previous versions of this API allowed validation failures to be gathered
        /// in a user-supplied list. This effect can be achieved by declaring <c>List&lt;ValidationFailure> ErrorList</c>
        /// and setting the <c>InvalidityListener</c> property to <c>ErrorList.Add</c>.</remarks> 
        
        public Action<ValidationFailure> InvalidityListener {
            set => InvalidityHandler = new InvalidityDelegate(value);
        }

        /// <summary>
        /// Add an instance document to the list of documents to be validated by a subsequent call
        /// on <c>ValidateMultipleDocuments</c>.
        /// </summary>
        /// <param name="source">Source document supplied as a <c>Stream</c></param>
        /// <param name="baseUri">Base URI of the source document</param>
        public void AddSource(Stream source, Uri baseUri) {
            sources.Add(new StreamSource(source, baseUri.ToString()));
        }
        
        /// <summary>
        /// Add an instance document to the list of documents to be validated by a subsequent call
        /// on <c>ValidateMultipleDocuments</c>. The base URI is taken as the current working
        /// directory.
        /// </summary>
        /// <param name="source">Source document supplied as a <c>Stream</c></param>
        
        public void AddSource(Stream source) {
            AddSource(source, new Uri(Environment.CurrentDirectory));
        }
        
        /// <summary>
        /// Add an instance document to the list of documents to be validated by a subsequent call
        /// on <c>ValidateMultipleDocuments</c>.
        /// </summary>
        /// <param name="source">Source document supplied as a <c>TextReader</c></param>
        /// <param name="baseUri">Base URI of the source document</param>
        public void AddSource(TextReader source, Uri baseUri) {
            sources.Add(new StreamSource(source, baseUri.ToString()));
        }
        
        /// <summary>
        /// Add an instance document to the list of documents to be validated by a subsequent call
        /// on <c>ValidateMultipleDocuments</c>. The base URI is taken as the current working
        /// directory.
        /// </summary>
        /// <param name="source">Source document supplied as a <c>TextReader</c></param>
        
        public void AddSource(TextReader source) {
            AddSource(source, new Uri(Environment.CurrentDirectory));
        }

        /// <summary>
        /// Add an instance document to the list of documents to be validated by a subsequent call
        /// on <c>ValidateMultipleDocuments</c>.
        /// </summary>
        /// <param name="uri">URI of the source document</param>
        /// 
        public void AddSource(Uri uri) {
            StreamSource ss = new StreamSource(uri.ToString());
            AugmentedSource aug = AugmentedSource.makeAugmentedSource(ss);
            aug.setPleaseCloseAfterUse(true);
            sources.Add(aug);
        }

        /// <summary>
        /// Add an instance document to the list of documents to be validated by a subsequent call
        /// on <c>ValidateMultipleDocuments</c>.
        /// </summary>
        /// <param name="reader">Source document supplied as an <c>XmlReader</c></param>
        /// 
        public void AddSource(XmlReader reader) {
            PullProvider pp = new DotNetPullProvider(reader);
            PipelineConfiguration pipe = config.makePipelineConfiguration();
            pipe.setUseXsiSchemaLocation(UnderlyingSchemaValidator.isUseXsiSchemaLocation());
            pp.setPipelineConfiguration(pipe);
            // pp = new PullTracer(pp);  /* diagnostics */
            PullSource pSource = new PullSource(pp);
            pSource.setSystemId(reader.BaseURI);
            sources.Add(pSource);
        }

        /// <summary>
        /// Add an instance document to the list of documents to be validated  by a subsequent call
        /// on <c>ValidateMultipleDocuments</c>.
        /// </summary>
        /// <param name="source">Source document supplied as an <c>XdmNode</c> value</param>
        public void AddSource(XdmNode source) {
            sources.Add(((NodeInfo) source.value).asActiveSource());
        }

        /// <summary>
        /// Validate an instance document supplied in the form of a <c>Stream</c>
        /// </summary>
        /// <param name="source">A stream containing the XML document to be parsed
        /// and validated. This stream will be consumed by the validation process,
        /// but it will not be closed after use: that is the responsibility of the
        /// caller.</param>
        /// <param name="baseUri">The base URI to be used for resolving any relative
        /// references, for example a reference to an <c>xsi:schemaLocation</c></param>                  
        public void Validate(Stream source, Uri baseUri) {
            UnderlyingSchemaValidator.validate(new StreamSource(source, baseUri.ToString()));
        }
        
        /// <summary>
        /// Validate an instance document supplied in the form of a <c>Stream</c>.
        /// The base Uri of the document is taken as the current working directory.
        /// </summary>
        /// <param name="source">A stream containing the XML document to be parsed
        /// and validated. This stream will be consumed by the validation process,
        /// but it will not be closed after use: that is the responsibility of the
        /// caller.</param>
        /// 
        public void Validate(Stream source) {
            Validate(source, new Uri(Environment.CurrentDirectory));
        }
        
        /// <summary>
        /// Validate an instance document supplied in the form of a <c>TextReader</c>
        /// </summary>
        /// <param name="source">A <c>TextReader</c> that supplies the XML document to be parsed
        /// and validated. This reader will be consumed by the validation process,
        /// but it will not be closed after use: that is the responsibility of the
        /// caller.</param>
        /// <param name="baseUri">The base URI to be used for resolving any relative
        /// references, for example a reference to an <c>xsi:schemaLocation</c></param>                  
        public void Validate(TextReader source, Uri baseUri) {
            UnderlyingSchemaValidator.validate(new StreamSource(source, baseUri.ToString()));
        }
        
        /// <summary>
        /// Validate an instance document supplied in the form of a <c>TextReader</c>.
        /// The base Uri of the document is taken as the current working directory.
        /// </summary>
        /// <param name="source">A <c>TextReader</c> that supplies the XML document to be parsed
        /// and validated. This reader will be consumed by the validation process,
        /// but it will not be closed after use: that is the responsibility of the
        /// caller.</param>
                 
        public void Validate(TextReader source) {
            Validate(source, new Uri(Environment.CurrentDirectory));
        }

        /// <summary>
        /// Validate an instance document, supplied in the form of a <c>Uri</c> reference
        /// </summary>
        /// <param name="uri">URI of the document to be validated</param>                  
        public void Validate(Uri uri) {
            StreamSource ss = new StreamSource(uri.ToString());
            AugmentedSource aug = AugmentedSource.makeAugmentedSource(ss);
            aug.setPleaseCloseAfterUse(true);
            UnderlyingSchemaValidator.validate(aug);
        }

        /// <summary>
        /// Validate an instance document, supplied in the form of an <c>XmlReader</c>.
        /// </summary>
        /// <remarks>
        /// The <c>XmlReader</c> is responsible for parsing the document; this method validates it.
        /// </remarks>
        /// <param name="reader">The <c>XmlReader</c> used to read and parse the instance
        /// document being validated. This is used as supplied. For conformance, use of a
        /// plain <c>XmlTextReader</c> is discouraged, because it does not expand entity
        /// references. This may cause validation failures.
        /// </param>
        /// <exception cref="SaxonApiException">Throws an exception if the document is found to be
        /// invalid (and also on other conditions like a failure to read the source document)</exception>
        public void Validate(XmlReader reader) {
            PullProvider pp = new DotNetPullProvider(reader);
            PipelineConfiguration pipe = config.makePipelineConfiguration();
            pipe.setUseXsiSchemaLocation(UnderlyingSchemaValidator.isUseXsiSchemaLocation());
            pp.setPipelineConfiguration(pipe);
            // pp = new PullTracer(pp);  /* diagnostics */
            PullSource psource = new PullSource(pp);
            psource.setSystemId(reader.BaseURI);
            UnderlyingSchemaValidator.validate(psource);
        }

        /// <summary>
        /// Validate an instance document, supplied in the form of an <c>XdmNode</c>.
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
        /// <exception cref="SaxonApiException">Throws an exception if the document is found to be
        /// invalid (and also on other conditions like a failure to read the source document)</exception> 
        public void Validate(XdmNode source) {
            UnderlyingSchemaValidator.validate(((NodeInfo) source.value).asActiveSource());
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
        public void SetDestination(IDestination destination) {
            UnderlyingSchemaValidator.setDestination(destination.UnderlyingDestination);
        }

        /// <summary>
        /// Close the destination, allowing resources to be released. Saxon calls this method
        /// when it has finished writing to the destination
        /// </summary>
        public void Close() {
            UnderlyingSchemaValidator.close();
        }


        /// <summary>
        /// The name of the required top-level element of the document to be validated (that is, the
        /// name of the outermost element of the document).
        /// If no value is supplied, there is no constraint on the required element name.
        /// </summary>
        public QName DocumentElementName {
            get => UnderlyingSchemaValidator.getDocumentElementName();
            set => UnderlyingSchemaValidator.setDocumentElementName(value);
        }


        /// <summary>
        /// The name of the required type of the top-level element of the document to be validated.
        /// QName of the name of the required type of the document element, or null if no value has been set.
        /// If no value is supplied, there is no constraint on the required type
        /// </summary>
        public QName DocumentElementTypeName {
            get => UnderlyingSchemaValidator.getDocumentElementTypeName();
            set => UnderlyingSchemaValidator.setDocumentElementTypeName(value);
        }
        

        internal Saxon.Hej.s9api.SchemaValidator UnderlyingSchemaValidator { get; }


        /// <summary>
        /// Set the value of a schema parameter (a parameter defined in the schema using the <c>saxon:param</c> extension)
        /// </summary>
        /// <param name="name">the name of the schema parameter, as a QName</param>
        /// <param name="value">the value of the schema  parameter, or null to clear a previously set value</param>
        public void SetParameter(QName name, XdmValue value) {
            UnderlyingSchemaValidator.setParameter(name, value);
        }


        /// <summary>
        /// Get the value that has been set for a schema processor (a parameter defined in the
        /// schema using the <c>saxon:param</c> extension)
        /// </summary>
        /// <param name="name">the parameter whose name is required</param>
        /// <returns>the value that has been set for the parameter, or the EmptySequence if no value has been set</returns>
        public XdmValue GetParameter(QName name) {
            return UnderlyingSchemaValidator.getParameter(name);
        }


        /// <summary>
        /// Ask and set whether attribute defaults defined in a schema are to be expanded or not
        /// (by default, fixed and default attribute values are expanded, that is, they are inserted) into
        /// the document during validation as if they were present in the instance being validated)
        /// </summary>
        public bool ExpendAttributeDefaults {
            get => UnderlyingSchemaValidator.isExpandAttributeDefaults();
            set => UnderlyingSchemaValidator.setExpandAttributeDefaults(value);
        }

        /// <summary>
        /// Run the validation of a set of instance documents, previously
        /// supplied by calling <c>AddSource</c>.
        /// </summary>
        /// <remarks>
        /// <para>Unless multithreading has been disabled at <c>Processor</c> level, the
        /// instance documents are validated in parallel in multiple threads.</para>
        /// <para>This requires care when deciding how invalidity should be reported: the
        /// recommended option is to generate an XML validation report by calling
        /// <c>SetValidityReporting</c>.</para>
        /// <para>The validated documents (with type annotations and expanded defaults: that is,
        /// the PSVIs) are not made available to the calling application.</para>
        /// <para>The method does not throw an exception if any of the source documents
        /// is found to be invalid. It does however throw an exception if any of the source
        /// documents cannot be read, or is not well-formed XML. (This method
        /// can therefore be useful even when there is only one document to be validated,
        /// to avoid the exception condition.)</para>
        /// </remarks>
        public void ValidateMultipleDocuments() {
            UnderlyingSchemaValidator.validateMultiple(sources);
        }
        
        /// <summary>
        /// The underlying <c>Saxon.Hej.s9api.Destination</c> object in the Saxon implementation.
        /// Note that this property represents the schema validator itself as the destination of
        /// some other process (for example, a transformation whose output is to be validated);
        /// it must not be confused with the destination to which output is sent after validation,
        /// as set using the <c>SetDestination()</c> method.
        /// </summary>
        /// <remarks>
        /// <para>This property provides access to internal methods in the Saxon engine that are
        /// not specifically exposed in the .NET API. In general these methods should be
        /// considered to be less stable than the classes in the Saxon.Api namespace.</para> 
        /// <para>The internal methods follow
        /// Java naming conventions rather than .NET conventions.</para>
        /// <para>Information about the returned <see cref="Saxon.Hej.s9api.Destination"/> object 
        /// (and the objects it provides access to) is included in the Saxon JavaDoc documentation.
        /// </para>
        /// </remarks>
        /// <value>The underlying Destination object</value>
        public Saxon.Hej.s9api.Destination UnderlyingDestination => UnderlyingSchemaValidator;
        
        /// <summary>
        /// Define an action to be performed when writing to the Destination is complete.
        /// </summary>
        /// <param name="action">The action to be performed.</param>
        public void OnClose(System.Action action) {
            UnderlyingSchemaValidator.onClose(action.Invoke);
        }

    }
}

// Copyright (c) 2021 Saxonica Limited