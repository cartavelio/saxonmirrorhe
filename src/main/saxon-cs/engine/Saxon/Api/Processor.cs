using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Xml;
using javax.xml.transform.stream;
using net.sf.saxon;
using net.sf.saxon.@event;
using net.sf.saxon.trans;
using Saxon.Api.Support;
using Version = net.sf.saxon.Version;

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

        private net.sf.saxon.s9api.Processor jProcessor;
        private SchemaManager schemaManager = null;
        private TextWriter textWriter = Console.Error;
        private IQueryResolver moduleResolver;
        private ICollectionFinder collectionFinder = null;
        private StandardCollectionFinder standardCollectionFinder;

        internal Processor(net.sf.saxon.s9api.Processor p) {
            jProcessor = p;
            jProcessor.getUnderlyingConfiguration().setProcessor(this);
            collectionFinder = new StandardCollectionFinder(Implementation.getCollectionFinder());
            standardCollectionFinder = new StandardCollectionFinder(Implementation.getStandardCollectionFinder());
        }

        /// <summary>
        /// Create a new <c>Processor</c>. This <c>Processor</c> will have capabilities that depend on the version
        /// of the software that has been loaded, and on the features that have been licensed.
        /// </summary>
        public Processor() {
            jProcessor = new net.sf.saxon.s9api.Processor(false);
            jProcessor.getUnderlyingConfiguration().setProcessor(this);
            jProcessor.getUnderlyingConfiguration().registerExternalObjectModel(new DotNetObjectModelDefinition());
            collectionFinder = new StandardCollectionFinder(Implementation.getCollectionFinder());
            standardCollectionFinder = new StandardCollectionFinder(Implementation.getStandardCollectionFinder());
        }

        /// <summary>
        /// Create a <c>Processor</c>.
        /// </summary>
        /// <param name="licensedEdition">Set to true if the <c>Processor</c> is to use a licensed edition of Saxon
        /// (that is, Saxon-PE or Saxon-EE). If true, the <c>Processor</c> will attempt to enable the capabilities
        /// of the licensed edition of Saxon, according to the version of the software that is loaded, and will
        /// verify the license key. If false, the <c>Processor</c> will load a default <c>Configuration</c> that gives restricted
        /// capability and does not require a license, regardless of which version of the software is actually being run.</param>
        public Processor(bool licensedEdition)
            // newline needed by documentation stylesheet
            : this(licensedEdition, false) {
        }

        /// <summary>
        /// Create a <c>Processor</c>.
        /// </summary>
        /// <param name="licensedEdition">Set to true if the <c>Processor</c> is to use a licensed edition of Saxon
        /// (that is, Saxon-PE or Saxon-EE). If true, the <c>Processor</c> will attempt to enable the capabilities
        /// of the licensed edition of Saxon, according to the version of the software that is loaded, and will
        /// verify the license key. If false, the <c>Processor</c> will load a default <c>Configuration</c> that gives restricted
        /// capability and does not require a license, regardless of which version of the software is actually being run.</param>
        /// <param name="loadLocally">This option has no effect at this release.</param>
        public Processor(bool licensedEdition, bool loadLocally) {
            jProcessor = new net.sf.saxon.s9api.Processor(licensedEdition);
            jProcessor.getUnderlyingConfiguration().registerExternalObjectModel(new DotNetObjectModelDefinition());
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
            config.registerExternalObjectModel(new DotNetObjectModelDefinition());
            jProcessor = new net.sf.saxon.s9api.Processor(config);
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
            config.registerExternalObjectModel(new DotNetObjectModelDefinition());
            jProcessor = new net.sf.saxon.s9api.Processor(config);
        }

        public ICollectionFinder CollectionFinder {
            set {
                if (value != null) {
                    collectionFinder = value;
                    Implementation.setCollectionFinder(new CollectionFinderWrapper(collectionFinder));
                }
            }
            get { return collectionFinder; }
        }


        public StandardCollectionFinder StandardCollectionFinder {
            get { return standardCollectionFinder; }
        }

        public void RegisterCollection(String collectionURI, IResourceCollection collection) {
            StandardCollectionFinder.RegisterCollection(collectionURI, collection);
            if (CollectionFinder is StandardCollectionFinder && collectionFinder != standardCollectionFinder) {
                ((StandardCollectionFinder) collectionFinder).RegisterCollection(collectionURI, collection);
            }
        }


        /// <summary>
        /// Declare a mapping from a specific namespace URI to a .NET class
        /// This will get applied to Saxon-PEN or Saxon-EEN product
        /// </summary>
        /// <param name="uri">the namespace URI of the function name</param>
        /// <param name="type">the .NET class that implements the functions in this namespace</param>
        public void BindExtensions(string uri, System.Type type) {
            Configuration config = jProcessor.getUnderlyingConfiguration();
            config.bindExtensions(uri, type);
        }


        /// <summary>
        /// Get the full name of the Saxon product version implemented by this <c>Processor</c>
        /// </summary>

        public string ProductTitle {
            get { return Version.getProductTitle(); }
        }

        /// <summary>
        /// Get the Saxon product version number (for example, "9.2.0.2")
        /// </summary>

        public string ProductVersion {
            get { return Version.getProductVersion(); }
        }

        /// <summary>
        /// Get the Saxon product edition (for example, "EE" for Enterprise Edition)
        /// </summary>
        /// 

        public string Edition {
            get { return jProcessor.getUnderlyingConfiguration().getEditionCode(); }
        }


        /// <summary>
        /// Gets the <c>SchemaManager</c> for the <c>Processor</c>. Returns null
        /// if the <c>Processor</c> is not schema-aware.
        /// </summary>

        public SchemaManager SchemaManager {
            get {
                if (schemaManager == null) {
                    schemaManager = new SchemaManager(this);
                }

                return schemaManager;
            }
        }

        /// <summary>
        /// An <c>XmlResolver</c>, which will be used while compiling and running queries, 
        /// XPath expressions, and stylesheets, if no other <c>XmlResolver</c> is nominated
        /// </summary>
        /// <remarks>
        /// <para>By default an <c>XmlUrlResolver</c> is used. This means that the responsibility
        /// for resolving and dereferencing URIs rests with the .NET platform, not with the
        /// IKVM/OpenJDK runtime.</para>
        /// <para>When Saxon invokes a user-written <c>XmlResolver</c>, the <c>GetEntity</c> method
        /// may return any of: a <c>System.IO.Stream</c>; a <c>System.IO.TextReader</c>; or a
        /// <c>java.xml.transform.Source</c>.</para>
        /// </remarks>

        public XmlResolver XmlResolver {
            get {
                javax.xml.transform.URIResolver resolver = jProcessor.getUnderlyingConfiguration().getURIResolver();
                if (resolver is JDotNetURIResolver) {
                    return ((JDotNetURIResolver) resolver).getXmlResolver();
                }
                else {
                    return new XmlUrlResolver();
                }
            }
            set { jProcessor.getUnderlyingConfiguration().setURIResolver(new JDotNetURIResolver(value)); }
        }


        /// <summary>
        /// A <c>TextWriter</c> used as the destination of miscellaneous error, warning, and progress messages.
        /// </summary>
        /// <remarks>
        /// <para>By default the <c>Console.Error</c> is used for all such messages.</para>
        /// <para>A user can supply their own <c>TextWriter</c> to redirect error messages from the standard output.</para>
        /// </remarks>
        public TextWriter ErrorWriter {
            get { return textWriter; }
            set {
                textWriter = value;
                StandardLogger logger = new StandardLogger(value);
                jProcessor.getUnderlyingConfiguration().setLogger(logger);
            }
        }

        /// <summary>
        /// Create a new <c>DocumentBuilder</c>, which may be used to build XDM documents from
        /// a variety of sources.
        /// </summary>
        /// <returns>A new <c>DocumentBuilder</c></returns>
        public DocumentBuilder NewDocumentBuilder() {
            DocumentBuilder builder = new DocumentBuilder(this);
            builder.XmlResolver = XmlResolver;
            return builder;
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
            Serializer s = new Serializer(jProcessor.newSerializer());
            s.SetProcessor(this); //TODO this method call might no longer be needed
            return s;
        }

        /// <summary>
        /// Create a <c>Serializer</c> initialized to write to a given <c>TextWriter</c>.
        /// Closing the writer after use is the responsibility of the caller.
        /// </summary>
        /// <param name="textWriter">The <c>TextWriter</c> to which the <c>Serializer</c> will write</param>
        /// <returns> a new <c>Serializer</c> </returns>
        public Serializer NewSerializer(TextWriter textWriter) {
            Serializer s = new Serializer(jProcessor.newSerializer());
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
            Serializer s = new Serializer(jProcessor.newSerializer());
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
            get {
                return (jProcessor.getUnderlyingConfiguration().getXMLVersion() == Configuration.XML10 ? 1.0m : 1.1m);
            }
            set {
                if (value == 1.0m) {
                    jProcessor.setXmlVersion("1.0");
                }
                else if (value == 1.1m) {
                    jProcessor.setXmlVersion("1.1");
                }
                else {
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
            JDotNetComparator comparator = new JDotNetComparator(uri.ToString(), compareInfo, options);
            Implementation.registerCollation(uri.ToString(), comparator);
        }

        /// <summary>
        /// Register a named collection. A collection is identified by a URI (the collection URI),
        /// and its content is represented by an <c>IEnumerable</c> that enumerates the contents
        /// of the collection. The values delivered by this enumeration are Uri values, which 
        /// can be mapped to nodes using the registered <c>XmlResolver</c>.
        /// </summary>
        /// <param name="collectionUri">The URI used to identify the collection in a call
        /// of the XPath <c>collection()</c> function. The default collection is registered
        /// by supplying null as the value of this argument (this is the collection returned
        /// when the XPath <c>collection()</c> function is called with no arguments).</param> 
        /// <param name="contents">An enumerable object that represents the contents of the
        /// collection, as a sequence of document URIs. The enumerator returned by this
        /// IEnumerable object must return instances of the Uri class.</param>
        /// <remarks>
        /// <para>Collections should be stable: that is, two calls to retrieve the same collection URI
        /// should return the same sequence of document URIs. This requirement is imposed by the
        /// W3C specifications, but in the case of a user-defined collection it is not enforced by
        /// the Saxon product.</para>
        /// <para>A collection may be replaced by specifying the URI of an existing
        /// collection.</para>
        /// <para>Collections registered with a processor are available to all queries and stylesheets
        /// running under the control of that processor. Collections should not normally be registered
        /// while queries and transformations are in progress.</para>
        /// </remarks>
        /// 
        public void RegisterCollection(Uri collectionUri, IEnumerable contents) {
            String u = (collectionUri == null ? null : collectionUri.ToString());
            Configuration config = jProcessor.getUnderlyingConfiguration();
            config.registerCollection(u, new JDotNetEnumerableCollection(config, contents));
        }

        /// <summary>
        /// Register an extension function with the <c>Processor</c>
        /// </summary>
        /// <param name="function">
        /// An object that defines the extension function, including its name, arity, arguments types, and
        /// a reference to the class that implements the extension function call.
        /// </param>
        public void RegisterExtensionFunction(ExtensionFunctionDefinition function) {
            WrappedExtensionFunctionDefinition f = new WrappedExtensionFunctionDefinition(function);
            jProcessor.registerExtensionFunction(f);
        }

        /// <summary>
        /// Register a simple external/extension function that is to be made available within any stylesheet, query
        /// or XPath expression compiled under the control of this <c>Processor</c>
        /// </summary>
        /// <param name="function">
        /// This interface provides only for simple extensions that have no side-effects and no
        /// dependencies on the static or dynamic context.
        /// </param>
        public void RegisterExtensionFunction(ExtensionFunction function) {
            WrappedExtensionFunction f = new WrappedExtensionFunction(function);
            jProcessor.registerExtensionFunction(f);
        }


        /// <summary>
        /// Set the media type to be associated with a file extension by  the standard collection handler
        /// </summary>
        /// <param name="extension">the file extension, for exmaple "xml". The value "" sets the 
        /// default media type to be used for unregistered file extensions</param>
        /// <param name="mediaType">the corresponding media type, for example "application/xml". The
        /// choice of media type determines how a resource with this extension gets parsed, when the file 
        /// appears as part of a collection</param>
        public void RegisterFileExtension(String extension, String mediaType) {
            Implementation.registerFileExtension(extension, mediaType);
        }

        /// <summary>
        /// Associated a media type with a resource factory. This methodm may be called
        /// to customize the behaviour of a collection to recognize different file extensions
        /// </summary>
        /// <param name="contentType">a media type or MIME type, for example application/xsd+xml</param>
        /// <param name="factory">a ResourceFactory used to parse (or otherweise process) resource of that type</param>
        public void RegisterMediaType(String contentType, IResourceFactory factory) {
            Implementation.registerMediaType(contentType, new ResourceFactoryWrapper(factory, this));
        }


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
        public String GetMediaTypeForFileExtension(String extension) {
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
        public void WriteXdmValue(XdmValue sequence, XmlDestination destination) {
            try {
                PipelineConfiguration pipe = jProcessor.getUnderlyingConfiguration().makePipelineConfiguration();
                Receiver r = destination.GetUnderlyingDestination().getReceiver(pipe,
                    pipe.getConfiguration().obtainDefaultSerializationProperties());
                r.open();
                foreach (XdmItem item in sequence) {
                    r.append(item.Unwrap().head());
                }

                r.close();
            }
            catch (XPathException err) {
                throw new DynamicError(err);
            }
        }

        /// <summary>
        /// The underlying <c>Configuration</c> object in the Saxon implementation
        /// </summary>
        /// <remarks>
        /// <para>This property provides access to internal methods in the Saxon engine that are
        /// not specifically exposed in the .NET API. In general these methods should be
        /// considered to be less stable than the classes in the Saxon.Api namespace.</para> 
        /// <para>The internal methods follow
        /// Java naming conventions rather than .NET conventions.</para>
        /// <para>Information about the returned <see cref="net.sf.saxon.Configuration"/> object 
        /// (and the objects it provides access to) is included in the Saxon JavaDoc docmentation.
        /// </para>
        /// </remarks>

        public Configuration Implementation => jProcessor.getUnderlyingConfiguration();

        /// <summary>
        /// A user-supplied <c>IQueryResolver</c> used to resolve location hints appearing in an
        /// <c>import module</c> declaration.
        /// </summary>
        /// <remarks>
        /// <para>This acts as the default value for the ModuleURIResolver.</para>
        /// <para> The URI Resolver for XQuery modules. May be null, in which case any 
        /// existing Module URI Resolver is removed  from the Configuration</para>
        /// </remarks>
        public IQueryResolver QueryResolver {
            get => moduleResolver;
            set {
                moduleResolver = value;
                jProcessor.getUnderlyingConfiguration()
                    .setModuleURIResolver((value == null ? null : new DotNetModuleURIResolver(value)));
            }
        }

        /// <summary>
        /// The underlying <c>net.sf.saxon.s9api.Processor</c> in the Java implementation
        /// </summary>

        public net.sf.saxon.s9api.Processor JProcessor {
            get { return jProcessor; }
        }

        /// <summary>
        /// Set a configuration property
        /// </summary>
        /// <remarks>
        /// <para>This method provides the ability to set named properties of the configuration.
        /// The property names are set as strings, whose values can be found in the Java
        /// class <c>net.sf.saxon.FeatureKeys</c>. The property values are always strings. 
        /// Properties whose values are other types are not available via this interface:
        /// however all properties have an effective equivalent whose value is a string.
        /// Note that on/off properties are set using the strings "true" and "false".</para>
        /// <para><i>Method added in Saxon 9.1</i></para>
        /// </remarks>
        /// <param name="name">The property name</param>
        /// <param name="value">The property value</param>
        public void SetProperty(String name, String value) {
            if (name.Equals("http://saxonica.com/oem-data")) {
                jProcessor.setConfigurationProperty("http://saxonica.com/oem-data", value);
            }
            else {
                jProcessor.setConfigurationProperty(name, value);
            }
        }

        /// <summary>
        /// Set a configuration property
        /// </summary>
        /// <param name="feature">The property feature</param>
        /// <param name="value">The property value</param>
        public void SetProperty<T>(Feature<T> feature, T value) {
            Implementation.setConfigurationProperty(feature);
            if (value.GetType() == typeof(Boolean)) {
                jProcessor.setFeature(feature.JFeature.code, value);
            }
            else {
                jProcessor.setConfigurationProperty(feature.JFeature, (object) value);
            }
        }

        /// <summary>
        /// Get the value of a configuration property
        /// </summary>
        /// <remarks>
        /// <para>This method provides the ability to get named properties of the configuration.
        /// The property names are supplied as strings, whose values can be found in the Java
        /// class <c>net.sf.saxon.FeatureKeys</c>. The property values are always returned as strings. 
        /// Properties whose values are other types are returned by converting the value to a string.
        /// Note that on/off properties are returned using the strings "true" and "false".</para>
        /// <para><i>Method added in Saxon 9.1</i></para>
        /// </remarks>
        /// <param name="name">The property name</param>
        /// <returns>The property value, as a string; or null if the property is unset.</returns>
        public String GetProperty(String name) {
            Object obj = jProcessor.getConfigurationProperty(net.sf.saxon.lib.Feature.byName(name));
            return (obj == null ? null : obj.ToString());
        }


        /// <summary>
        /// Get a property of the configuration
        /// </summary>
        /// <typeparam name="T">See the class <c>Feature</c> for constants 
        /// representing the properties that can be requested.</typeparam>
        /// <param name="feature">the required property. </param>
        /// <returns>the value of the property</returns>
        public T GetProperty<T>(Feature<T> feature) {
            return (T) jProcessor.getConfigurationProperty(feature.JFeature);
        }
    }
}