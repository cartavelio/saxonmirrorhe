using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
using Saxon.Hej.expr;
using Saxon.Hej.expr.parser;
using Saxon.Hej.sxpath;
using Saxon.Hej.type;
using Saxon.Hej.value;
using Version = Saxon.Hej.Version;

namespace Saxon.Api {
    /// <summary>
    /// The <c>Processor</c> class serves three purposes: it allows global Saxon configuration
    /// options to be set; it acts as a factory for generating XQuery, XPath, and XSLT
    /// compilers; and it owns certain shared resources such as the Saxon <c>NamePool</c> and 
    /// compiled schemas.
    /// </summary>
    /// <remarks>
    /// <para>This is the first object that a Saxon application should create. Most
    /// applications should only create one <c>Processor</c> object. Once established, a 
    /// <c>Processor</c> may be used in multiple threads.</para>
    /// </remarks>

    [Serializable]
	public class Processor : Saxon.Hej.Configuration.ApiProvider {
        // For largely historic reasons, the Saxon.Api.Processor is implemented
        // as a wrapper around the s9api Processor, which is C# code converted from Java

        private SchemaManager schemaManager;
        private TextWriter textWriter = Console.Error;
        private CollectionFinder userCollectionFinder;

        private TextResolver textResolver;

        internal Processor(Saxon.Hej.s9api.Processor p) {
            JProcessor = p;
            JProcessor.getUnderlyingConfiguration().setProcessor(this);
            init();
        }

        /// <summary>
        /// Create a new Saxon-CS <c>Processor</c>. This <c>Processor</c> will have capabilities that depend on the version
        /// of the software that has been loaded, and on the features that have been licensed.
        /// </summary>
        public Processor() {
            JProcessor = new Saxon.Hej.s9api.Processor(true);
            JProcessor.getUnderlyingConfiguration().setProcessor(this);
            init();
        }

        /// <summary>
        /// Create a <c>Processor</c>.
        /// </summary>
        /// <param name="licensedEdition">Must be set to true (kept for future use).</param>
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        public Processor(bool licensedEdition) {
            if (!licensedEdition) {
                throw new IllegalArgumentException("Argument to new Processor() must be set to true if supplied");
            }
            JProcessor = new Saxon.Hej.s9api.Processor(true);
            JProcessor.getUnderlyingConfiguration().setProcessor(this);
            init();
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
            init();
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
            init();
        }

        /// <summary>
        /// Create a <c>Processor</c>, wrapping an existing instance of the internal Saxon class <c>Configuration</c>
        /// </summary>
        /// <param name="config">The <c>Configuration</c> to be wrapped.</param>
        /// <remarks>
        /// <para>This method can be used to create two <c>Processor</c> objects that share the same underlying
        /// <c>Configuration</c>, though this is not usually advisable, because most changes to one <c>Processor</c>
        /// actually modify the <c>Configuration</c> and hence affect the other <c>Processor</c>.</para>
        /// <para>As an alternative to this method, consider using the <c>Processor</c> that was originally used
        /// to construct the <c>Configuration</c>, which will often be available via <c>Configuration.getProcessor()</c>.</para>
        /// </remarks>
		public Processor(Saxon.Hej.Configuration config) {
            JProcessor = new Saxon.Hej.s9api.Processor(config);
            config.setProcessor(this);
            init();
        }

        private void init() {
            checkLicense();
            Saxon.Hej.expr.XPathContext context = new EarlyEvaluationContext(Implementation);
            StandardCollectionFinder = uri => new ResourceCollectionWrapper(
                new StandardCollectionFinder().findCollection(context, uri.ToString()), context);
            userCollectionFinder = StandardCollectionFinder;
        }

        private void checkLicense() {
            Implementation.checkLicensedFeature(
                Configuration.LicenseFeature.SCHEMA_VALIDATION, "Saxon-CS", -1);
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
            s.OutputWriter = textWriter;
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
            s.OutputStream = stream;
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
        /// A used-supplied function that is used to retrieve a resource collection given a collection URI.
        /// </summary>
        /// <remarks>
        /// <para>If no collection finder is supplied, then the built-in collection finder is used. The
        /// built-in collection finder recognizes various kinds of collection including directory collections
        /// and catalog collections. A user-defined collection finder can delegate to the built-in collection finder
        /// by invoking the function available as <c>Processor.StandardCollectionFinder</c>.</para>
        /// <para>A collection finder should be set only while the processor is being initialized; the
        /// effect of changing the collection finder (or changing any data on which it depends) while
        /// queries or transformations are in progress is undefined.</para>
        /// <para>The collection finder returns a set of <c>IResource</c> objects. Typically a used-defined
        /// class implementing <c>IResource</c> is used. An <c>IResource</c> has a resource URI, a content type
        /// (MIME or media type), and a method to deliver the resource content as an <c>XdmItem</c>. Typically
        /// fetching the resource over the network, and parsing it according to the rules of the relevant content
        /// type, is delayed until the <c>GetXdmItem</c> method is called.</para>
        /// <para>For deterministic results, and for W3C conformance, it is recommended that the supplied function
        /// should be deterministic: repeated calls for the same collection URI should deliver the same set of
        /// resources. However, this requirement can lead to high memory requirements, so Saxon does not enforce it.</para>
        /// <para>The <c>CollectionFinder</c> registered with a processor is available to all queries and stylesheets
        /// running under the control of that processor.</para>
        /// </remarks>
        
        public CollectionFinder CollectionFinder {
            set {
                userCollectionFinder = value;
                Implementation.setCollectionFinder(
                        (context, uri) => new ResourceCollectionUnwrapper(uri, value.Invoke(new Uri(uri))));
                
            }
            get => userCollectionFinder;
        }

        /// <summary>
        /// The standard (built-in) collection finder that is used by default if no user-defined
        /// collection finder has been supplied. This is exposed as a property of the <c>Processor</c>
        /// so that it can be invoked by a user-written collection finder.
        /// </summary>

        public CollectionFinder StandardCollectionFinder { get; private set; }

        /// <summary>
        /// Register a specific collection URI
        /// </summary>
        /// <param name="collectionUri">The collection URI</param>
        /// <param name="collection">The contents of the collection</param>
        /// <remarks>
        /// Registered collections take priority over any a user-supplied <c>CollectionFinder</c>
        /// </remarks>
        /// 
        public void RegisterCollection(Uri collectionUri, IEnumerable<IResource> collection) {
            string collUri = collectionUri.ToString();
            Implementation.registerCollection(collUri, new ResourceCollectionUnwrapper(collUri, collection));
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
        /// <param name="name">The name of the function, as a QName</param>
        /// <param name="type">The type of the function, for example <c>ParseItemType("function(xs:double) as xs:double")</c>.
        /// This must be a specific function type.</param>
        /// <param name="function">The body of the function, supplied typically as a lambda expression.
        /// This interface provides only for extensions that have no side-effects and no
        /// dependencies on the static or dynamic context. It does however allow the type signature
        /// of the function to be defined.
        /// </param>

        public void RegisterExtensionFunction(QName name, XdmItemType type, Func<XdmValue[], XdmValue> function) {
            if (type.Unwrap() is not SpecificFunctionType ft) {
                throw new ArgumentException("Supplied type must be a specific function type");
            }
            // TODO (for diagnostics): introspect the type of the supplied function to check that the arity is consistent with the declared type
            Implementation.registerExtensionFunction(new LambdaExtensionFunctionWrapper(name.GetStructuredQName(), ft, function));
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
        // /// <param name="factory">a ResourceFactory used to parse (or otherwise process) resource of that type</param>
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
                Receiver r = destination.UnderlyingDestination.getReceiver(pipe,
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

		public Saxon.Hej.Configuration Implementation => JProcessor.getUnderlyingConfiguration();

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

        /// <summary>
        /// Construct an XdmSequenceType object by parsing a sequence type definition in XPath syntax
        /// </summary>
        /// <param name="input">The input string, in XPath syntax (for example <c>array(xs:integer)*</c>). The only
        /// namespace prefix recognized is "xs", representing the XML Schema namespace.</param>
        /// <exception cref="SaxonApiException">If the syntax of the input is invalid, or if there are references
        /// to undeclared schema types</exception>
        public XdmSequenceType ParseSequenceType(string input) {
            try {
                XPathParser parser = new ();
                parser.setLanguage(XPathParser.ParsedLanguage.SEQUENCE_TYPE, 31);
                SequenceType st = parser.parseSequenceType(input, new IndependentContext(Implementation));
                return XdmSequenceType.FromSequenceType(st);
            }
            catch (XPathException e) {
                throw new SaxonApiException(e);
            }
        }
        /// <summary>
        /// Construct an XdmItemType object by parsing an item type definition in XPath syntax
        /// </summary>
        /// <param name="input">The input string, in XPath syntax (for example <c>array(xs:integer)</c>). The only
        /// namespace prefix recognized is "xs", representing the XML Schema namespace.</param>
        /// <exception cref="SaxonApiException">If the syntax of the input is invalid, or if there are references
        /// to undeclared schema types</exception>
        public XdmItemType ParseItemType(string input) {
            try {
                XPathParser parser = new ();
                IndependentContext env = new IndependentContext(Implementation);
                env.declareNamespace("xs", NamespaceConstant.SCHEMA);
                parser.setLanguage(XPathParser.ParsedLanguage.SEQUENCE_TYPE, 31);
                SequenceType st = parser.parseSequenceType(input, env);
                if (st.getCardinality() != StaticProperty.EXACTLY_ONE) {
                    throw new XPathException("Item type must not have an occurrence indicator", "XPST0003");
                }
                return XdmItemType.MakeXdmItemType(st.getPrimaryType(), Implementation);
            }
            catch (XPathException e) {
                throw new SaxonApiException(e);
            }
        }

        /// <summary>
        /// Get the system function with a given name and arity
        /// </summary>
        /// <param name="name">The required function name</param>
        /// <param name="arity">The required arity</param>
        /// <returns>The system function if it exists, or null otherwise</returns>
        public XdmFunctionItem GetSystemFunction(QName name, int arity) {
            Saxon.Hej.om.Function f = Implementation.getSystemFunction(name.GetStructuredQName(), arity);
            return f==null ? null : new XdmFunctionItem(f);
        }
    }
}

// Copyright (c) 2021 Saxonica Limited