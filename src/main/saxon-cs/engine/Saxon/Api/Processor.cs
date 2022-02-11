using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Xml;
using Saxon.Ejavax.xml.transform.stream;
using Saxon.Hej;
using Saxon.Hej.@event;
using Saxon.Hej.lib;
using Saxon.Hej.resource;
using Saxon.Hej.trans;
using Saxon.Api.Support;
using Saxon.Callbacks;
using Saxon.Ejava.lang;
using Saxon.Impl.Helpers;
using Version = Saxon.Hej.Version;

namespace Saxon.Api {
    /// <summary>
    /// The <c>Processor</c> class serves three purposes: it allows global Saxon configuration
    /// options to be set; it acts as a factory for generating XQuery, XPath, and XSLT
    /// compilers; and it owns certain shared resources such as the Saxon <c>NamePool</c> and 
    /// compiled schemas. This is the first object that a Saxon application should create. Once
    /// established, a <c>Processor</c> may be used in multiple threads.
    /// </summary>
    [Serializable]
    public class Processor : Configuration.ApiProvider {
        // For largely historic reasons, the Saxon.Api.Processor is implemented
        // as a wrapper around the s9api Processor, which is C# code converted from Java

        private SchemaManager schemaManager = null;
        private TextWriter textWriter = Console.Error;
        private CollectionFinder collectionFinder = (cxt, uri) => new ResourceCollectionWrapper(
            new StandardCollectionFinder().findCollection(cxt.context, uri));

        private TextResolver textResolver;

        internal Processor(Saxon.Hej.s9api.Processor p) {
            JProcessor = p;
            JProcessor.getUnderlyingConfiguration().setProcessor(this);
            checkLicense();
        }

        /// <summary>
        /// Create a new Saxon-CS <c>Processor</c>. This <c>Processor</c> will have capabilities that depend on the version
        /// of the software that has been loaded, and on the features that have been licensed.
        /// </summary>
        public Processor() {
            JProcessor = new Saxon.Hej.s9api.Processor(true);
            JProcessor.getUnderlyingConfiguration().setProcessor(this);
            checkLicense();
        }

        /// <summary>
        /// Create a <c>Processor</c>.
        /// </summary>
        /// <param name="licensedEdition">Must be set to true (kept for future use).</param>
        public Processor(bool licensedEdition) {
            if (!licensedEdition) {
                throw new IllegalArgumentException("Argument to new Processor() must be set to true if supplied");
            }
            JProcessor = new Saxon.Hej.s9api.Processor(true);
            JProcessor.getUnderlyingConfiguration().setProcessor(this);
            checkLicense();
        }

        /// <summary>
        /// Create a <c>Processor</c>, based on configuration information supplied in a configuration file.
        /// </summary>
        /// <param name="configurationFile">A stream holding the text of the XML configuration file. Details of the file format
        /// can be found in the Saxon documentation.</param>
        [Obsolete("Use the Processor(Stream, Uri) constructor instead.")]
        public Processor(Stream configurationFile) {
            StreamSource ss = new StreamSource(configurationFile);
            Configuration config = Configuration.readConfiguration(ss);
            //config.registerExternalObjectModel(new DotNetObjectModelDefinition());
            JProcessor = new Saxon.Hej.s9api.Processor(config);
            config.setProcessor(this);
            checkLicense();
        }

        /// <summary>
        /// Create a <c>Processor</c>, based on configuration information supplied in a configuration file.
        /// </summary>
        /// <param name="configurationFile">A stream holding the text of the XML configuration file. Details of the file format
        /// can be found in the Saxon documentation.</param>
        /// <param name="baseUri">baseUri of the configuration file used for resolving any relative URIs in the file</param> 
        public Processor(Stream configurationFile, Uri baseUri) {
            StreamSource ss = new StreamSource(configurationFile, baseUri.AbsoluteUri);
            Configuration config = Configuration.readConfiguration(ss);
            //config.registerExternalObjectModel(new DotNetObjectModelDefinition());
            JProcessor = new Saxon.Hej.s9api.Processor(config);
            config.setProcessor(this);
            checkLicense();
        }

        public Processor(Configuration config) {
            JProcessor = new Saxon.Hej.s9api.Processor(config);
            config.setProcessor(this);
            checkLicense();
        }

        private void checkLicense() {
            Implementation.checkLicensedFeature(
                Saxon.Hej.Configuration.LicenseFeature.SCHEMA_VALIDATION, "Saxon-CS", -1);
        }
        
        /// <summary>
        /// Get the full name of the Saxon product version implemented by this <c>Processor</c>
        /// </summary>

        public string ProductTitle => Version.getProductTitle();

        /// <summary>
        /// Get the Saxon product version number (for example, "9.2.0.2")
        /// </summary>

        public string ProductVersion => Version.getProductVersion();

        /// <summary>
        /// Get the Saxon product edition (for example, "EE" for Enterprise Edition)
        /// </summary>
        /// 

        public string Edition => JProcessor.getUnderlyingConfiguration().getEditionCode();


        /// <summary>
        /// Gets the <c>SchemaManager</c> for this <c>Processor</c>. 
        /// </summary>

        public SchemaManager SchemaManager => schemaManager ??= new SchemaManager(this);

        /// <summary>
        /// An <c>XmlResolver</c>, which will be used while compiling and running queries, 
        /// XPath expressions, and stylesheets, if no other <c>XmlResolver</c> is nominated
        /// </summary>
        /// <remarks>
        /// <para>By default a <c>StandardXmlResolver</c> is used.</para>
        /// <para>When Saxon invokes a user-written <c>XmlResolver</c>, the <c>GetEntity</c> method
        /// may return any of: a <c>System.IO.Stream</c>; a <c>System.IO.TextReader</c>; or a
        /// <c>Saxon.Ejava.xml.transform.Source</c>.</para>
        /// </remarks>

        public XmlResolver XmlResolver {
            get {
                Saxon.Ejavax.xml.transform.URIResolver resolver = JProcessor.getUnderlyingConfiguration().getURIResolver();
                if (resolver is StandardURIResolver uriResolver) {
                    return uriResolver.getXmlResolver();
                }

                return new StandardXmlResolver(Implementation);
            }
            set => JProcessor.getUnderlyingConfiguration().setURIResolver(
                                new StandardURIResolver(JProcessor.getUnderlyingConfiguration(), value));
        }
        
        /// <summary>
        /// An <c>TextResolver</c>, which will be used for resolving URIs passed to calls on
        /// <c>unparsed-text()</c>, <c>unparsed-text-lines()</c>, <c>unparsed-text-available()</c>,
        /// and <c>json-doc()</c>. The default is null, which indicates that Saxon will use its
        /// own internal mechanisms.
        /// </summary>

        public TextResolver TextResolver {
            get => textResolver;
            set {
                textResolver = value;
                Implementation.setUnparsedTextURIResolver((uri, encoding, config) =>
                    value(uri, Encoding.GetEncoding(encoding)));
            }
        }



        /// <summary>
        /// A <c>TextWriter</c> used as the destination of miscellaneous error, warning, and progress messages.
        /// </summary>
        /// <remarks>
        /// <para>By default the <c>Console.Error</c> is used for all such messages.</para>
        /// <para>A user can supply their own <c>TextWriter</c> to redirect error messages from the standard output.</para>
        /// </remarks>
        public TextWriter ErrorWriter {
            get => textWriter;
            set {
                textWriter = value;
                StandardLogger logger = new StandardLogger(value);
                JProcessor.getUnderlyingConfiguration().setLogger(logger);
            }
        }

        /// <summary>
        /// Create a new <c>DocumentBuilder</c>, which may be used to build XDM documents from
        /// a variety of XML-based sources.
        /// </summary>
        /// <returns>A new <c>DocumentBuilder</c></returns>
        public DocumentBuilder NewDocumentBuilder() {
            return new DocumentBuilder(this) {XmlResolver = XmlResolver};
        }
        
        /// <summary>
        /// Create a new <c>JsonBuilder</c>, which may be used to build XDM instances from
        /// input in JSON format.
        /// </summary>
        /// <returns>A new <c>JsonBuilder</c></returns>
        public JsonBuilder NewJsonBuilder() {
            return new JsonBuilder(Implementation);
        }

        /// <summary>
        /// Create a new <c>XQueryCompiler</c>, which may be used to compile XQuery queries.
        /// </summary>
        /// <remarks>
        /// The returned <c>XQueryCompiler</c> retains a live link to the <c>Processor</c>, and
        /// may be affected by subsequent changes to the <c>Processor</c>.
        /// </remarks>
        /// <returns>A new <c>XQueryCompiler</c></returns>
        public XQueryCompiler NewXQueryCompiler() {
            return new XQueryCompiler(this);
        }

        /// <summary>
        /// Create a new <c>XsltCompiler</c>, which may be used to compile XSLT stylesheets.
        /// </summary>
        /// <remarks>
        /// The returned <c>XsltCompiler</c> retains a live link to the <c>Processor</c>, and
        /// may be affected by subsequent changes to the <c>Processor</c>.
        /// </remarks>
        /// <returns>A new <c>XsltCompiler</c></returns>
        public XsltCompiler NewXsltCompiler() {
            return new XsltCompiler(this);
        }

        /// <summary>
        /// Create a new <c>XPathCompiler</c>, which may be used to compile XPath expressions.
        /// </summary>
        /// <remarks>
        /// The returned <c>XPathCompiler</c> retains a live link to the <c>Processor</c>, and
        /// may be affected by subsequent changes to the <c>Processor</c>.
        /// </remarks>
        /// <returns>A new <c>XPathCompiler</c></returns>
        public XPathCompiler NewXPathCompiler() {
            return new XPathCompiler(this, JProcessor.newXPathCompiler());
        }

        /// <summary>
        /// Create a <c>Serializer</c>
        /// </summary>
        /// <returns> a new <c>Serializer</c> </returns>
        public Serializer NewSerializer() {
            return new Serializer(this, JProcessor.newSerializer());
        }

        /// <summary>
        /// Create a <c>Serializer</c> initialized to write to a given <c>TextWriter</c>.
        /// Closing the writer after use is the responsibility of the caller.
        /// </summary>
        /// <param name="textWriter">The <c>TextWriter</c> to which the <c>Serializer</c> will write</param>
        /// <returns> a new <c>Serializer</c> </returns>
        public Serializer NewSerializer(TextWriter textWriter) {
            Serializer s = new Serializer(this, JProcessor.newSerializer());
            s.SetOutputWriter(textWriter);
            return s;
        }

        /// <summary>
        /// Create a <c>Serializer</c> initialized to write to a given output <c>Stream</c>.
        /// Closing the output stream after use is the responsibility of the caller.
        /// </summary>
        /// <param name="stream">The output <c>Stream</c> to which the <c>Serializer</c> will write</param>
        /// <returns> a new Serializer </returns>
        public Serializer NewSerializer(Stream stream) {
            Serializer s = new Serializer(this, JProcessor.newSerializer());
            s.SetOutputStream(stream);
            return s;
        }

        /// <summary>
        /// The XML version used in this <c>Processor</c>
        /// </summary>
        /// <remarks>
        /// The value must be 1.0 or 1.1, as a <c>decimal</c>. The default version is currently 1.0, but may
        /// change in the future.
        /// </remarks>

        public decimal XmlVersion {
            get => JProcessor.getUnderlyingConfiguration().getXMLVersion() == Configuration.XML10 ? 1.0m : 1.1m;
            set {
                switch (value) {
                    case 1.0m:
                        JProcessor.setXmlVersion("1.0");
                        break;
                    case 1.1m:
                        JProcessor.setXmlVersion("1.1");
                        break;
                    default:
                        throw new ArgumentException("Invalid XML version: " + value);
                }
            }
        }

        /// <summary>
        /// Create a collation based on a given <c>CompareInfo</c> and <c>CompareOptions</c>    
        /// </summary>
        /// <param name="uri">The collation URI to be used within an XPath expression to refer to this collation</param>
        /// <param name="compareInfo">The <c>CompareInfo</c>, which determines the language-specific
        /// collation rules to be used</param>
        /// <param name="options">Options to be used in performing comparisons, for example
        /// whether they are to be case-blind and/or accent-blind</param>
        public void DeclareCollation(Uri uri, CompareInfo compareInfo, CompareOptions options) {
            ComparingCollation collator = new ComparingCollation(uri.ToString(), compareInfo, options);
            Implementation.registerCollation(uri.ToString(), collator);
        }

        /// <summary>
        /// Register a named collection. A collection is identified by a URI (the collection URI),
        /// and its content is represented by an <c>IResourceCollection</c>.
        /// </summary>
        /// <param name="collectionURI">The URI used to identify the collection in a call
        /// of the XPath <c>collection()</c> function. The default collection is registered
        /// by supplying null as the value of this argument (this is the collection returned
        /// when the XPath <c>collection()</c> function is called with no arguments).</param> 
		/// <param name="collection">An object that represents the contents of the
        /// collection.</param>
        /// <remarks>
        /// <para>Collections should be stable: that is, two calls to retrieve the same collection URI
        /// should return the same sequence of document URIs. This requirement is imposed by the
        /// W3C specifications, but in the case of a user-defined collection it is not enforced by
        /// the Saxon product.</para>
        /// <para>A collection may be replaced by specifying the URI of an existing
        /// collection. However, this should only be done during initialization of the processor;
        /// the effect of changing collection bindings while transformations or queries are
        /// in progress is undefined.</para>
        /// <para>Collections registered with a processor are available to all queries and stylesheets
        /// running under the control of that processor.</para>
        /// <para>If a collection URI has been registered using this method, the registered collection
        /// is used, and the collection finder is not called.</para>
        /// </remarks>
        /// 
        
        public void RegisterCollection(string collectionURI, IResourceCollection collection) {
            Configuration config = JProcessor.getUnderlyingConfiguration();
            config.registerCollection(collectionURI, ResourceCollectionUnwrapper.Unwrap(collection));
        }
        
        /// <summary>
        /// Get or set a function that is used to retrieve a resource collection given a collection URI.
        /// </summary>
        /// <remarks>
        /// <para>This function will be called only if the collection URI has not been registered using
        /// <c>RegisterCollection</c>.</para>
        /// <para>A collection finder should be set only while the processor is being initialized; the
        /// effect of changing the collection finder (or changing any data on which it depends) while
        /// queries or transformations are in progress is undefined.
        /// </para>
        /// <para>The initial value of the property is a system collection finder that recognizes various
        /// kinds of collection including directory collections and catalog collections. It is always possible
        /// for a user-defined collection finder to delegate to this system collection finder.</para>
        /// </remarks>
        
        public CollectionFinder CollectionFinder {
            set {
                if (value != null) {
                    this.collectionFinder = value;
                    Implementation.setCollectionFinder(
                        (context, uri) => ResourceCollectionUnwrapper.Unwrap(value(new DynamicContext(context), uri)));
                }
            }
            get => collectionFinder;
        }

        /// <summary>
        /// Register an extension function with the <c>Processor</c>. This interface provides
        /// rich capability for defining extension functions with complex requirements, for
        /// example accessing information in the static and dynamic context, and declaring
        /// the presence of side-effects. For simpler extension functions use an
        /// <c>ExtensionFunction</c> in preference.
        /// </summary>
        /// <param name="function">
        /// An object that defines the extension function, including its name, arity, arguments types, and
        /// a reference to the class that implements the extension function call. This function
        /// becomes available for use in all queries, stylesheets, and XPath expressions
        /// compiled under the control of this Processor.
        /// </param>
        public void RegisterExtensionFunction(ExtensionFunctionDefinition function) {
            Configuration config = JProcessor.getUnderlyingConfiguration();
            config.registerExtensionFunction(new ExtensionFunctionDefinitionWrapper(function));
        }

        /// <summary>
        /// Register a simple external/extension function that is to be made available within any stylesheet, query
        /// or XPath expression compiled under the control of this <c>Processor</c>
        /// </summary>
        /// <param name="function">
        /// This interface provides only for extensions that have no side-effects and no
        /// dependencies on the static or dynamic context. It does however allow the type signature
        /// of the function to be defined.
        /// </param>
        public void RegisterExtensionFunction(ExtensionFunction function) {
            Implementation.registerExtensionFunction(new SimpleExtensionFunctionWrapper(function));
        }
        
        /// <summary>
        /// Register a simple external/extension function that is to be made available within any stylesheet, query
        /// or XPath expression compiled under the control of this <c>Processor</c>
        /// </summary>
        /// <param name="name">The name of the function, as a QName</param>
        /// <param name="arity">The arity of the function (number of arguments), as an integer</param>
        /// <param name="function">The body of the function, supplied typically as a lambda expression.
        /// This interface provides only for extensions that have no side-effects and no
        /// dependencies on the static or dynamic context. It does however allow the type signature
        /// of the function to be defined.
        /// </param>

        public void RegisterExtensionFunction(QName name, int arity, Func<XdmValue[], XdmValue> function) {
            RegisterExtensionFunction(new LambdaExtensionFunction(name, arity, function));
        }

        /// <summary>
        /// Set the media type to be associated with a file extension by the standard collection handler
        /// </summary>
        /// <param name="extension">the file extension, for example "xml". The value "" sets the 
        /// default media type to be used for unregistered file extensions</param>
        /// <param name="mediaType">the corresponding media type, for example "application/xml". The
        /// choice of media type determines how a resource with this extension gets parsed, when the file 
        /// appears as part of a collection</param>
        /// 
        public void RegisterFileExtension(string extension, string mediaType) {
            Implementation.registerFileExtension(extension, mediaType);
        }

        // /// <summary>
        // /// Associated a media type with a resource factory. This method may be called
        // /// to customize the behaviour of a collection to recognize different file extensions
        // /// </summary>
        // /// <param name="contentType">a media type or MIME type, for example application/xsd+xml</param>
        // /// <param name="factory">a ResourceFactory used to parse (or otherweise process) resource of that type</param>
        // public void RegisterMediaType(String contentType, IResourceFactory factory) {
        //     Implementation.registerMediaType(contentType, new ResourceFactoryWrapper(factory, this));
        // }


        /// <summary>
        /// Get the media type to be associated with a file extension by the standard 
        /// collection handler
        /// </summary>
        /// <param name="extension">the file extension, for example "xml". The value "" gets
        /// the default media type to be used for unregistered file extensions. The default 
        /// media type is also returned if the supplied file extension is not registered</param>
        /// <returns>the corresponding media type, for example "application/xml". The choice
        /// of media type determines how a resource with this extension gets parsed, when the file
        /// appears as part of a collection.</returns>
        public string GetMediaTypeForFileExtension(string extension) {
            return Implementation.getMediaTypeForFileExtension(extension);
        }

        /// <summary>
        /// Copy an <c>XdmValue</c> to an <c>XmlDestination</c>
        /// </summary>
        /// <remarks>
        /// In principle this method can be used to copy any kind of <c>XdmValue</c> to any kind
        /// of <c>XmlDestination</c>. However, some kinds of destination may not accept arbitrary
        /// sequences of items; for example, some may reject function items. Some destinations
        /// perform sequence normalization, as defined in the W3C serialization specification,
        /// to convert the supplied sequence to a well-formed XML document; it is a property
        /// of the chosen <c>XmlDestination</c> whether it does this or not.</remarks>
        /// <param name="sequence">The value to be written</param>
        /// <param name="destination">The destination to which the value should be written</param>
        /// 
        public void WriteXdmValue(XdmValue sequence, IDestination destination) {
            try {
                PipelineConfiguration pipe = JProcessor.getUnderlyingConfiguration().makePipelineConfiguration();
                Receiver r = destination.GetUnderlyingDestination().getReceiver(pipe,
                    pipe.getConfiguration().obtainDefaultSerializationProperties());
                r.open();
                foreach (XdmItem item in sequence) {
                    r.append(item.Unwrap().head());
                }

                r.close();
            }
            catch (XPathException err) {
                throw new SaxonApiException(err);
            }
        }

        /// <summary>
        /// The underlying <c>Configuration</c> object in the Saxon implementation
        /// </summary>
        /// <remarks>
        /// <para>This property provides access to internal methods in the Saxon engine that are
        /// not specifically exposed in the Saxon-CS API. In general these methods should be
        /// considered to be less stable than the classes in the Saxon.Api namespace.</para> 
        /// <para>The internal methods follow
        /// Java naming conventions rather than .NET conventions.</para>
        /// <para>Information about the returned <see cref="Saxon.Hej.Configuration"/> object 
        /// (and the objects it provides access to) is included in the Saxon JavaDoc documentation.
        /// </para>
        /// </remarks>

        public Configuration Implementation => JProcessor.getUnderlyingConfiguration();

        /// <summary>
        /// The underlying <c>Saxon.Hej.s9api.Processor</c> in the Java implementation
        /// </summary>

        internal Saxon.Hej.s9api.Processor JProcessor { get; }

        /// <summary>
        /// Set a configuration property
        /// </summary>
        /// <remarks>
        /// <para>This method provides the ability to set named properties of the configuration.
        /// The property names are set as strings, whose values can be found in the Java
        /// class <c>Saxon.Hej.FeatureKeys</c>. The property values are always strings. 
        /// Properties whose values are other types are not available via this interface:
        /// however all properties have an effective equivalent whose value is a string.
        /// Note that on/off properties are set using the strings "true" and "false".</para>
        /// <para><i>Method added in Saxon 9.1</i></para>
        /// </remarks>
        /// <param name="name">The property name</param>
        /// <param name="value">The property value</param>
        public void SetProperty(string name, string value) {
            Implementation.setConfigurationProperty(name, value);
        }

        /// <summary>
        /// Set a configuration property. This method provides a type-safe way of
        /// setting properties of any type: for example a boolean property can be
        /// set by calling <code>SetProperty(Feature&lt;bool&gt;.ALLOW_SYNTAX_EXTENSIONS, true)</code>.
        /// </summary>
        /// <param name="feature">The property feature</param>
        /// <param name="value">The property value</param>
        public void SetProperty<T>(Feature<T> feature, T value) {
            Implementation.setFeature(FeatureIndex.getData(feature.Code), value);
        }

        /// <summary>
        /// Get the value of a configuration property
        /// </summary>
        /// <remarks>
        /// <para>This method provides the ability to get named properties of the configuration.
        /// The property names are supplied as strings, whose values can be found in the Java
        /// class <c>Saxon.Hej.FeatureKeys</c>. The property values are always returned as strings. 
        /// Properties whose values are other types are returned by converting the value to a string.
        /// Note that on/off properties are returned using the strings "true" and "false".</para>
        /// <para><i>Method added in Saxon 9.1</i></para>
        /// </remarks>
        /// <param name="name">The property name</param>
        /// <returns>The property value, as a string; or null if the property is unset.</returns>
        public string GetProperty(string name) {
            object obj = Implementation.getConfigurationProperty(name);
            return obj?.ToString();
        }

        /// <summary>
        /// Get a property of the configuration.
        /// </summary>
        /// <remarks>
        /// <para>This method provides a type-safe way of getting the value of a configuration
        /// property of any type: for example, <c>GetProperty(Feature&lt;bool>.XQUERY_ALLOW_UPDATE)</c>
        /// returns a boolean property as a boolean. </para>
        /// </remarks>
        /// <typeparam name="T">See the class <c>Feature</c> for constants 
        /// representing the properties that can be requested.</typeparam>
        /// <param name="feature">the required property. </param>
        /// <returns>the value of the property</returns>
        public T GetProperty<T>(Feature<T> feature) {
            return (T)Implementation.getFeature(FeatureIndex.getData(feature.Code));
        }
    }
}