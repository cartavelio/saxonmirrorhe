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

namespace Saxon.Api.Support {
    
    public class XsdValidatorEE : Saxon.Api.SchemaValidator {
        private readonly Configuration config;
        private List<Source> sources = new();

        // internal constructor

        internal XsdValidatorEE(Saxon.Hej.s9api.SchemaValidator validator, Processor processor) {
            this.UnderlyingSchemaValidator = validator;
            this.config = processor.Implementation;
        }

        public override bool Lax {
            get => UnderlyingSchemaValidator.isLax();
            set => UnderlyingSchemaValidator.setLax(value);
        }

        public override bool UseXsiSchemaLocation {
            get => UnderlyingSchemaValidator.isUseXsiSchemaLocation();
            set => UnderlyingSchemaValidator.setUseXsiSchemaLocation(value);
        }

        public override void SetValidityReporting(IDestination destination) {
            UnderlyingSchemaValidator.setValidityReporting(destination.UnderlyingDestination);
        }
        
        public override IInvalidityHandler InvalidityHandler {
            set => UnderlyingSchemaValidator.setInvalidityHandler(new InvalidityHandlerWrapper(value));
        }
        
        public override Action<ValidationFailure> InvalidityListener {
            set => InvalidityHandler = new InvalidityDelegate(value);
        }

        public override void AddSource(Stream source, Uri baseUri) {
            sources.Add(new StreamSource(source, baseUri.ToString()));
        }
        
        public override void AddSource(Stream source) {
            AddSource(source, new Uri(Environment.CurrentDirectory));
        }
        
        public override void AddSource(TextReader source, Uri baseUri) {
            sources.Add(new StreamSource(source, baseUri.ToString()));
        }
        
        public override void AddSource(TextReader source) {
            AddSource(source, new Uri(Environment.CurrentDirectory));
        }
        
        public override void AddSource(Uri uri) {
            StreamSource ss = new StreamSource(uri.ToString());
            AugmentedSource aug = AugmentedSource.makeAugmentedSource(ss);
            aug.setPleaseCloseAfterUse(true);
            sources.Add(aug);
        }
        
        public override void AddSource(XmlReader reader) {
            PullProvider pp = new DotNetPullProvider(reader);
            PipelineConfiguration pipe = config.makePipelineConfiguration();
            pipe.setUseXsiSchemaLocation(UnderlyingSchemaValidator.isUseXsiSchemaLocation());
            pp.setPipelineConfiguration(pipe);
            // pp = new PullTracer(pp);  /* diagnostics */
            PullSource pSource = new PullSource(pp);
            pSource.setSystemId(reader.BaseURI);
            sources.Add(pSource);
        }

        public override void AddSource(XdmNode source) {
            sources.Add(((NodeInfo) source.value).asActiveSource());
        }

        public override void Validate(Stream source, Uri baseUri) {
            UnderlyingSchemaValidator.validate(new StreamSource(source, baseUri.ToString()));
        }
        
        public override void Validate(Stream source) {
            Validate(source, new Uri(Environment.CurrentDirectory));
        }
        
        public override void Validate(TextReader source, Uri baseUri) {
            UnderlyingSchemaValidator.validate(new StreamSource(source, baseUri.ToString()));
        }
        
        public override void Validate(TextReader source) {
            Validate(source, new Uri(Environment.CurrentDirectory));
        }

        public override void Validate(Uri uri) {
            StreamSource ss = new StreamSource(uri.ToString());
            AugmentedSource aug = AugmentedSource.makeAugmentedSource(ss);
            aug.setPleaseCloseAfterUse(true);
            UnderlyingSchemaValidator.validate(aug);
        }

        public override void Validate(XmlReader reader) {
            PullProvider pp = new DotNetPullProvider(reader);
            PipelineConfiguration pipe = config.makePipelineConfiguration();
            pipe.setUseXsiSchemaLocation(UnderlyingSchemaValidator.isUseXsiSchemaLocation());
            pp.setPipelineConfiguration(pipe);
            // pp = new PullTracer(pp);  /* diagnostics */
            PullSource psource = new PullSource(pp);
            psource.setSystemId(reader.BaseURI);
            UnderlyingSchemaValidator.validate(psource);
        }

        public override void Validate(XdmNode source) {
            UnderlyingSchemaValidator.validate(((NodeInfo) source.value).asActiveSource());
        }

        public override void SetDestination(IDestination destination) {
            UnderlyingSchemaValidator.setDestination(destination.UnderlyingDestination);
        }

        public override void Close() {
            UnderlyingSchemaValidator.close();
        }


        public override QName DocumentElementName {
            get => UnderlyingSchemaValidator.getDocumentElementName();
            set => UnderlyingSchemaValidator.setDocumentElementName(value);
        }


        public override QName DocumentElementTypeName {
            get => UnderlyingSchemaValidator.getDocumentElementTypeName();
            set => UnderlyingSchemaValidator.setDocumentElementTypeName(value);
        }
        

        internal override Saxon.Hej.s9api.SchemaValidator UnderlyingSchemaValidator { get; }


        public override void SetParameter(QName name, XdmValue value) {
            UnderlyingSchemaValidator.setParameter(name, value);
        }


        public override XdmValue GetParameter(QName name) {
            return UnderlyingSchemaValidator.getParameter(name);
        }


        public override bool ExpandAttributeDefaults {
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
        public override void ValidateMultipleDocuments() {
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
        public override Saxon.Hej.s9api.Destination UnderlyingDestination => UnderlyingSchemaValidator;
        
        /// <summary>
        /// Define an action to be performed when writing to the Destination is complete.
        /// </summary>
        /// <param name="action">The action to be performed.</param>
        public override void OnClose(System.Action action) {
            UnderlyingSchemaValidator.onClose(action.Invoke);
        }

    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited