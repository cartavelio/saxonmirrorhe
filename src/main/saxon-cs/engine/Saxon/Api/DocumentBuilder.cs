using System;
using System.Impl.Helpers;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Saxon.Callbacks;
using Saxon.Ejava.lang;
using Saxon.Ejavax.xml.transform;
using Saxon.Ejavax.xml.transform.stream;
using Saxon.Hej;
using Saxon.Hej.@event;
using Saxon.Hej.expr.parser;
using Saxon.Hej.lib;
using Saxon.Hej.om;
using Saxon.Hej.pull;
using Saxon.Hej.serialize;
using Saxon.Hej.trans;
using Saxon.Impl.Dom;
using Saxon.Impl.XDoc;

namespace Saxon.Api {
    /// <summary>
    /// The <c>DocumentBuilder</c> class enables XDM documents to be built from various sources.
    /// The class is always instantiated using the <c>NewDocumentBuilder</c> method
    /// on the <c>Processor</c> object.
    /// </summary>
    [Serializable]
    public class DocumentBuilder {
        private Processor processor;
        private Configuration config;
        private SchemaValidationMode validation;
        private SchemaValidator schemaValidator;
        private Uri baseUri;
        private QName topLevelElement;
        private XQueryExecutable projectionQuery;
        private ResourceResolver resourceResolver;

        private Saxon.Hej.s9api.DocumentBuilder s9Builder;
        
        internal DocumentBuilder(Processor processor) {
            this.processor = processor;
            this.s9Builder = processor.JProcessor.newDocumentBuilder();
            this.config = processor.Implementation;
        }
        
        /// <summary>
        /// An XmlDocumentResolver used to resolve any URI passed to the <c>Build</c> method.
        /// </summary>
        /// <remarks>
        /// <para>If a resolver is supplied, it must take total responsibility for resolving all URIs;
        /// there is no fallback if it returns null or raises an error. If the resolver is to handle
        /// some URIs but delegate the handling of others, this can be achieved by creating a
        /// <c>CommonResourceResolver</c> and chaining a <c>DirectResourceResolver</c>.</para>
        /// </remarks>
        
        public ResourceResolver XmlDocumentResolver {
            get => resourceResolver;
            set => resourceResolver = value;
        }

        /// <summary>
        /// A <c>System.Xml.XmlResolver</c>, which will be used to resolve references to external entities
        /// within XML documents being loaded (including any external DTD) when the <c>DocumentBuilder</c>
        /// allocates an <c>XmlReader</c>.
        /// </summary>
        /// <remarks>
        /// <para>If no <c>XmlResolver</c> is supplied, the <c>ResourceResolver</c> associated with the
        /// Saxon configuration is used (<c>Configuration.getResourceResolver()</c>)</para>
        /// <para>In Saxon releases prior to 11.1, the supplied <c>XmlResolver</c> was also used to
        /// resolve any relative URI passed to the <c>DocumentBuilder.Build()</c> method.</para>
        /// </remarks>

        public XmlResolver.XmlResolver XmlResolver { get; set; }

        /// <summary>
        /// Determines whether line numbering is enabled for documents loaded using this
        /// <c>DocumentBuilder</c>.
        /// </summary>
        /// <remarks>
        /// <para>By default, line numbering is disabled.</para>
        /// <para>Line numbering is not available for all kinds of source: in particular,
        /// it is not available when loading from an existing <c>XmlDocument</c>.</para>
        /// <para>The resulting line numbers are accessible to applications using the
        /// extension function <c>saxon:line-number()</c> applied to a node.</para>  
        /// <para>Line numbers are maintained only for element nodes; the line number
        /// returned for any other node will be that of the most recent element.</para> 
        /// </remarks>

        public bool LineNumbering {
            get => s9Builder.isLineNumbering();
            set => s9Builder.setLineNumbering(value);
        }

        /// <summary>
        /// Determines whether schema validation is applied to documents loaded using this
        /// <c>DocumentBuilder</c>, and if so, whether it is strict or lax. If schema validation
        /// is requested and the document is not valid, then the <c>Build</c> method will fail
        /// with an exception.
        /// </summary>
        /// <remarks>
        /// <para>By default, no schema validation takes place.</para>
        /// <para>This option requires Saxon Enterprise Edition (Saxon-EE).</para>
        /// </remarks>

        public SchemaValidationMode SchemaValidationMode {
            get => validation;
            set {
                validation = value;
                if (value is SchemaValidationMode.Strict or SchemaValidationMode.Lax) {
                    schemaValidator = processor.SchemaManager.NewSchemaValidator();
                    schemaValidator.Lax = value == SchemaValidationMode.Lax;
                    s9Builder.setSchemaValidator(schemaValidator.UnderlyingSchemaValidator);
                }
            }
        }


        /// <summary>
        /// Property to set and get the schemaValidator to be used. This determines whether schema validation is applied
        /// to an input document and whether type annotations in a supplied document are retained. If no schemaValidator
        /// is supplied, then schema validation does not take place.
        /// </summary>
        /// <remarks>
        /// <para>If validation is requested using this mechanism, and the document is not valid,
        /// then no exception is raised; it is for the application to handle any invalidity reports
        /// from the SchemaValidator.
        /// </para>
        /// <para>The supplied <code>SchemaValidator</code> is not actually used directly when a document is built
        ///  (the {@link SchemaValidator#validate(Source)}
        ///  method is never called). Rather, some of the properties of the <code>SchemaValidator</code> are used to control
        ///  how schema validation is performed by the <code>DocumentBuilder</code>. The particular properties
        ///  that take effect include:</para>
        ///  <list>
        ///  <item>The validation mode (strict or lax)</item>
        ///  <item>The required top-level element declaration (see {@link SchemaValidator#setDocumentElementName(QName)}</item>
        ///  <item>The required type of the top-level element (see {@link SchemaValidator#setDocumentElementTypeName(QName)}</item>
        ///  <item>The option {@link SchemaValidator#isUseXsiSchemaLocation()}</item>
        ///  <item>The option {@link SchemaValidator#isExpandAttributeDefaults()}</item>
        ///  <item>Validation parameters set using {@link SchemaValidator#setParameter}</item>
        ///  <item>The {@link net.sf.saxon.lib.InvalidityHandler}</item>
        ///  </list>
        ///  <p>Properties that currently do NOT have any effect include:</p>
        ///  <list>
        ///  <item>The option {@link SchemaValidator#isCollectStatistics()}</item>
        ///  </list>
        /// </remarks>
        public SchemaValidator SchemaValidator {
            get => schemaValidator;
            set {
                schemaValidator = value;
                s9Builder.setSchemaValidator(schemaValidator?.UnderlyingSchemaValidator);
            }
        }

        /// <summary>
        /// The required name of the top level element in a document instance being validated
        /// against a schema.
        /// </summary>
        /// <remarks>
        /// <para>If this property is set, and if schema validation is requested, then validation will
        /// fail unless the outermost element of the document has the required name.</para>
        /// <para>This option requires the schema-aware version of the Saxon product (Saxon-EE).</para>
        /// </remarks> 

        public QName TopLevelElementName {
            get => topLevelElement;
            set => topLevelElement = value;
        }

        /// <summary>
        /// Determines whether DTD validation is applied to documents loaded using this
        /// <c>DocumentBuilder</c>.
        /// </summary>
        /// <remarks>
        /// <para>By default, no DTD validation takes place.</para>
        /// </remarks>

        public bool DtdValidation {
            get => s9Builder.isDTDValidation();
            set => s9Builder.setDTDValidation(value);
        }

        /// <summary>
        /// Determines the whitespace stripping policy applied when loading a document
        /// using this <c>DocumentBuilder</c>.
        /// </summary>
        /// <remarks>
        /// <para>By default, whitespace text nodes appearing in element-only content
        /// are stripped, and all other whitespace text nodes are retained.</para>
        /// <para>If DTD or schema validation is applied, the only permitted setting
        /// is <c>WhitespacePolicy#IGNORABLE</c>. Any other value results
        /// in an exception from the <c>Build()</c> method.</para>
        /// </remarks>

        public WhitespacePolicy WhitespacePolicy {
            get => s9Builder.getWhitespaceStrippingPolicy();
            set => s9Builder.setWhitespaceStrippingPolicy(value);
        }


        ///<summary>
        /// The Tree Model implementation to be used for the constructed document. By default
        /// the <c>TinyTree</c> is used. The main reason for using the <c>LinkedTree</c> alternative is if
        /// updating is required (the <c>TinyTree</c> is not updateable).
        ///</summary>

        public TreeModel TreeModel {
            get => (TreeModel) s9Builder.getTreeModel().getSymbolicValue();
            set => s9Builder.setTreeModel(Saxon.Hej.om.TreeModel.getTreeModel((int) value));
        }

        /// <summary>
        /// The base URI of a document loaded using this <c>DocumentBuilder</c>.
        /// This is used for resolving any relative URIs appearing
        /// within the document, for example in references to DTDs and external entities.
        /// </summary>
        /// <remarks>
        /// This information is required when the document is loaded from a source that does not
        /// provide an intrinsic URI, notably when loading from a <c>Stream</c> or a <c>TextReader</c>.
        /// </remarks>


        public Uri BaseUri {
            get => baseUri;
            set {
                baseUri = value;
                if (baseUri != null) {
                    s9Builder.setBaseURI(new Uri(value.AbsoluteUri));
                }
            }
        }


        /// <summary>
        /// Set a compiled query to be used for implementing document projection. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// The effect of using this option is that the tree constructed by the 
        /// <c>DocumentBuilder</c> contains only those parts
        /// of the source document that are needed to answer this query. Running this query against
        /// the projected document should give the same results as against the raw document, but the
        /// projected document typically occupies significantly less memory. It is permissible to run
        /// other queries against the projected document, but unless they are carefully chosen, they
        /// will give the wrong answer, because the document being used is different from the original.
        /// </para>
        /// <para>The query should be written to use the projected document as its initial context item.
        /// For example, if the query is <code>//ITEM[COLOR='blue']</code>, then only <code>ITEM</code>
        /// elements and their <code>COLOR</code> children will be retained in the projected document.</para>
        /// <para>This facility is only available in Saxon-EE; if the facility is not available,
        /// calling this method has no effect.</para>
        /// </remarks>


        public XQueryExecutable DocumentProjectionQuery {
            get => projectionQuery;
            set {
                projectionQuery = value;
                s9Builder.setDocumentProjectionQuery(projectionQuery?.getUnderlyingCompiledQuery());
            }
        }

        /// <summary>
        /// Load an XML document, retrieving it via a URI.
        /// </summary>
        /// <remarks>
        /// <para>Note that the type <c>Uri</c> requires an absolute URI.</para>
        /// <para>The URI is dereferenced using the registered <c>XmlResolver</c>.</para>
        /// <para>This method takes no account of any fragment part in the URI.</para>
        /// <para>The <c>role</c> passed to the <c>GetEntity</c> method of the <c>XmlResolver</c> 
        /// is "application/xml", and the required return type is <c>System.IO.Stream</c>.</para>
        /// <para>The document located via the URI is parsed using the <c>System.Xml</c> parser.</para>
        /// <para>Note that the Microsoft <c>System.Xml</c> parser does not report whether attributes are
        /// defined in the DTD as being of type <c>ID</c> and <c>IDREF</c>. This is true whether or not
        /// DTD-based validation is enabled. This means that such attributes are not accessible to the 
        /// <c>id()</c> and <c>idref()</c> functions.</para>
        /// </remarks>
        /// <param name="uri">The URI identifying the location where the document can be
        /// found. This will also be used as the base URI of the document (regardless
        /// of the setting of the <c>BaseUri</c> property).</param>
        /// <returns>An <c>XdmNode</c>, the document node at the root of the tree of the resulting
        /// in-memory document.
        /// </returns>
        public XdmNode Build(Uri uri) {
            if (XmlDocumentResolver == null) {
                return s9Builder.build(new StreamSource(uri.ToString()));
            }
            ResourceRequest request = new () {
                Uri = uri,
                Nature = ResourceRequest.XML_NATURE
            };
            IResource resource = XmlDocumentResolver.Invoke(request);
            if (resource == null) {
                return s9Builder.build(new StreamSource(uri.ToString()));
            }
            switch (resource) {
                case TextResource text when text.hasReader():
                    return Build(text.TextReader, resource.ResourceUri);
                case TextResource text when text.hasStream():
                    return Build(text.InputStream, resource.ResourceUri);
                case TextResource text:
                    return Build(new StringReader(text.Content), resource.ResourceUri);
                case XmlReaderResource xrr:
                    return Build(xrr.XmlReader);
                default: {
                    XdmItem item = resource.GetXdmItem();
                    if (item is XdmNode node) {
                        return node;
                    }

                    break;
                }
            }

            throw new ArgumentException("Invalid type of result from ResourceResolver");

        }

        /// <summary>
        /// Load an XML document supplied as raw (lexical) XML on a <c>Stream</c>.
        /// </summary>
        /// <remarks>
        /// <para>The document is parsed using the Microsoft <c>System.Xml</c> parser.</para>
        /// <para>Before calling this method, the <c>BaseUri</c> property should be set to identify the
        /// base URI of this document, used for resolving any relative URIs contained within it;
        /// if it has not been set, the current working directory is assumed.</para>
        /// <para>Note that the Microsoft <c>System.Xml</c> parser does not report whether attributes are
        /// defined in the DTD as being of type <c>ID</c> and <c>IDREF</c>. This is true whether or not
        /// DTD-based validation is enabled. This means that such attributes are not accessible to the 
        /// <c>id()</c> and <c>idref()</c> functions.</para>         
        /// </remarks>
        /// <param name="input">The <c>Stream</c> containing the XML source to be parsed. Closing this stream
        /// on completion is the responsibility of the caller.</param>
        /// <returns>An <c>XdmNode</c>, the document node at the root of the tree of the resulting
        /// in-memory document.
        /// </returns>
        public XdmNode Build(Stream input) {
            Uri localBaseUri = baseUri ?? new Uri(Environment.CurrentDirectory);
            return Build(input, localBaseUri);
        }

        // Build a document from a given stream, with the base URI supplied
        // as an extra argument

        internal XdmNode Build(Stream input, Uri baseUri) {

            XmlReaderSettings settings = new () {
                DtdProcessing = DtdProcessing.Parse // must expand entity references
            };

            if (XmlResolver != null) {
                settings.XmlResolver = XmlResolver.GetXmlResolver();
            }
            else {
                settings.XmlResolver = new XmlResolverWrappingResourceResolver(config.getResourceResolver());
            }

            settings.ValidationType = (DtdValidation ? ValidationType.DTD : ValidationType.None);

            XmlReader parser = XmlReader.Create(input, settings, baseUri.ToString());
            DotNetPullProvider dotNetPullProvider = new (parser);
            if (WhitespacePolicy == WhitespacePolicy.StripIgnorable && DtdValidation) {
                dotNetPullProvider.IgnoreIgnorableWhitespace = true;
            }

            Source source = new PullSource(dotNetPullProvider);
            source.setSystemId(baseUri.ToString());
       

            try {
                return s9Builder.build(source);
            }
            catch (XPathException ex) {
                throw new SaxonApiException(ex);
            }
        }

        /// <summary>
        /// Load an XML document supplied using a <c>TextReader</c>.
        /// </summary>
        /// <remarks>
        /// <para>The document is parsed using the Microsoft <c>System.Xml</c> parser.</para>
        /// <para>Before calling this method, the <c>BaseUri</c> property should be set to identify the
        /// base URI of this document, used for resolving any relative URIs contained within it;
        /// if it has not been set, the current working directory is assumed.</para>
        /// <para>Note that the Microsoft <c>System.Xml</c> parser does not report whether attributes are
        /// defined in the DTD as being of type <c>ID</c> and <c>IDREF</c>. This is true whether or not
        /// DTD-based validation is enabled. This means that such attributes are not accessible to the 
        /// <c>id()</c> and <c>idref()</c> functions.</para>         
        /// </remarks>
        /// <param name="input">The <c>TextReader</c> containing the XML source to be parsed</param>
        /// <returns>An <c>XdmNode</c>, the document node at the root of the tree of the resulting
        /// in-memory document.
        /// </returns>
        public XdmNode Build(TextReader input) {
            Uri localBaseUri = baseUri ?? new Uri(Environment.CurrentDirectory);
            return Build(input, localBaseUri);
        }

        // Build a document from a given TextReader, with the base URI supplied
        // as an extra argument

        internal XdmNode Build(TextReader input, Uri baseUri) {
            XmlReaderSettings settings = new() {
                DtdProcessing = DtdProcessing.Parse // must expand entity references
            };

            if (XmlResolver != null)
            {
                settings.XmlResolver = XmlResolver.GetXmlResolver();
            } else {
                settings.XmlResolver = new XmlResolverWrappingResourceResolver(config.getResourceResolver());
            }

            settings.ValidationType = (DtdValidation ? ValidationType.DTD : ValidationType.None);

            XmlReader parser = XmlReader.Create(input, settings, baseUri.ToString());
            DotNetPullProvider dotNetPullProvider = new (parser);
            if (WhitespacePolicy == WhitespacePolicy.StripIgnorable) {
                dotNetPullProvider.IgnoreIgnorableWhitespace = true;
            }
            Source source = new PullSource(dotNetPullProvider);
            source.setSystemId(baseUri.ToString());

            return s9Builder.build(source);
        }

        private Source augmentSource(Source source, ParseOptions options) {
            return new AugmentedSource(source, options);
        }
        
        /// <summary>
        /// Load an XML document, delivered using an <c>XmlReader</c>.
        /// </summary>
        /// <remarks>
        /// <para>The <c>XmlReader</c> is responsible for parsing the document; this method builds a tree
        /// representation of the document (in an internal Saxon format) and returns its document node.
        /// The <c>XmlReader</c> is not required to perform validation but it must expand any entity references.
        /// Saxon uses the properties of the <c>XmlReader</c> as supplied.</para>
        /// <para>Use of a plain <c>XmlTextReader</c> is discouraged, because it does not expand entity
        /// references. This should only be used if you know in advance that the document will contain
        /// no entity references (or perhaps if your query or stylesheet is not interested in the content
        /// of text and attribute nodes). Instead, with .NET 1.1 use an <c>XmlValidatingReader</c> (with <c>ValidationType</c>
        /// set to <c>None</c>). The constructor for <c>XmlValidatingReader</c> is obsolete in .NET 2.0,
        /// but the same effect can be achieved by using the <c>Create</c> method of <c>XmlReader</c> with
        /// appropriate <c>XmlReaderSettings</c>.</para>
        /// <para>The base URI of the resulting document is taken from the <c>BaseURI</c> property
        /// of the <c>XmlReader</c> if this is non-null and non-empty; otherwise it is taken from the
        /// <c>BaseUri</c> property of this <c>DocumentBuilder</c>.</para>
        /// <para>Conformance with the W3C specifications requires that the <c>Normalization</c> property
        /// of an <c>XmlTextReader</c> should be set to <c>true</c>. However, Saxon does not insist
        /// on this.</para>
        /// <para>If the <c>XmlReader</c> performs schema validation, Saxon will ignore any resulting type
        /// information. Type information can only be obtained by using Saxon's own schema validator, which
        /// will be run if the <c>SchemaValidationMode</c> property is set to <c>Strict</c> or <c>Lax</c>.</para>
        /// <para>Note that the Microsoft <c>System.Xml</c> parser does not report whether attributes are
        /// defined in the DTD as being of type <c>ID</c> and <c>IDREF</c>. This is true whether or not
        /// DTD-based validation is enabled. This means that such attributes are not accessible to the 
        /// <c>id()</c> and <c>idref()</c> functions.</para>
        /// <para>Note that setting the <c>XmlResolver</c> property of the <c>DocumentBuilder</c>
        /// has no effect when this method is used; if an <c>XmlResolver</c> is required, it must
        /// be set on the <c>XmlReader</c> itself.</para>
        /// </remarks>
        /// <param name="reader">The <c>XMLReader</c> that supplies the parsed XML source</param>
        /// <returns>An <c>XdmNode</c>, the document node at the root of the tree of the resulting
        /// in-memory document.
        /// </returns>
        public XdmNode Build(XmlReader reader) {
            PullProvider pp = new DotNetPullProvider(reader);
            pp.setPipelineConfiguration(config.makePipelineConfiguration());
            // pp = new PullTracer(pp);  /* diagnostics */
            Source source = new PullSource(pp);
            string documentBaseUri = reader.BaseURI;
            if (string.IsNullOrEmpty(documentBaseUri) && this.BaseUri != null) {
                documentBaseUri = this.BaseUri.ToString();
            }
            source.setSystemId(documentBaseUri);
            ParseOptions options = config.getParseOptions();

            source = augmentSource(source, options);
            XdmNode doc = s9Builder.build(source);
            return doc;
        }

        /// <summary>
        /// Load an Linq document or element node, supplied as an <c>XContainer</c>, into a Saxon <c>XdmNode</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The returned document will contain only the subtree rooted at the supplied node.
        /// </para>
        /// <para>
        /// This method copies the Linq tree to create a Saxon tree. See the <c>Wrap</c> method for
        /// an alternative that creates a wrapper around the Linq tree, allowing it to be modified in situ.
        /// </para>
        /// </remarks>
        /// <param name="source">The Linq document or element node to be copied to form a Saxon tree</param>
        /// <returns>An <c>XdmNode</c>, the document or element node corresponding to the supplied
        /// Linq node. If the supplied source was an XDocument node, the result will be an XDM document node;
        /// if it was an XElement node, it will be an XDM element node forming the outermost element of
        /// a tree whose root is an XDM document node.
        /// </returns>
        public XdmNode Build(XContainer source) {
            XdmNode doc = Build(source.CreateReader());
            if (source is XElement) {
                return doc.Select(Steps.Child("*")).First();
            }
            else {
                return doc;
            }
        }
        
        /// <summary>
        /// Load an XML DOM document, supplied as an <c>XmlNode</c>, into a Saxon <c>XdmNode</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The returned document will contain only the subtree rooted at the supplied node.
        /// </para>
        /// <para>
        /// This method copies the DOM tree to create a Saxon tree. See the <c>Wrap</c> method for
        /// an alternative that creates a wrapper around the DOM tree, allowing it to be modified in situ.
        /// </para>
        /// </remarks>
        /// <param name="source">The DOM Node to be copied to form a Saxon tree</param>
        /// <returns>An <c>XdmNode</c>, the document node at the root of the tree of the resulting
        /// in-memory document.
        /// </returns>
        public XdmNode Build(XmlNode source) {
            return Build(new XmlNodeReader(source));
        }


        /// <summary>
        /// Wrap an XML DOM document, supplied as an <c>XmlDocument</c>, as a Saxon <c>XdmNode</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method must be applied at the level of the Document Node. Unlike the
        /// <c>Build</c> method, the original DOM is not copied. This saves memory and
        /// time, but it also means that it is not possible to perform operations such as
        /// whitespace stripping and schema validation.
        /// </para>
        /// </remarks>
        /// <param name="doc">The DOM document node to be wrapped</param>
        /// <returns>An <c>XdmNode</c>, the Saxon document node at the root of the tree of the resulting
        /// in-memory document.
        /// </returns>
        public XdmNode Wrap(XmlDocument doc) {
            String baseu = (baseUri == null ? null : baseUri.ToString());
            DomDocumentTree tree = new DomDocumentTree(doc, baseu, config);
            return (XdmNode) XdmValue.Wrap(tree.getRootNode());
        }
        
        /// <summary>
        /// Wrap an XML DOM node (other than a document node), as a Saxon <c>XdmNode</c>.
        /// </summary>
        /// <param name="docWrapper">The wrapper for the containing DOM document node</param>
        /// <param name="node">The DOM node containing the node to be wrapped</param>
        /// <returns>An <c>XdmNode</c>, wrapping the supplied DOM node
        /// </returns>
        public XdmNode Wrap(XdmNode docWrapper, XmlNode node) {
            if (!(docWrapper.Implementation is DomNodeWrapper && docWrapper.NodeKind == XdmNodeKind.Document)) {
                throw new IllegalArgumentException("First argument is not a wrapped DOM document node");
            }

            DomNodeWrapper doc = ((DomNodeWrapper) docWrapper.Implementation);
            if (node.OwnerDocument != doc.getUnderlyingNode()) {
                throw new IllegalArgumentException(
                    "The node supplied in the second argument does not belong to the document supplied in the first argument");
            }
            DomDocumentTree tree = (DomDocumentTree)doc.getTreeInfo();
            DomNodeWrapper wrappedNode = tree.wrap(node);
            return new XdmNode(wrappedNode);
        }
        
        /// <summary>
        /// Wrap a Linq document node, supplied as an <c>System.Linq.XDocument</c>, as a Saxon <c>XdmNode</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method must be applied at the level of the Document Node. Unlike the
        /// <c>Build</c> method, the original tree is not copied. This saves memory and
        /// time, but it also means that it is not possible to perform operations such as
        /// whitespace stripping and schema validation.
        /// </para>
        /// </remarks>
        /// <param name="doc">The Linq document node to be wrapped</param>
        /// <returns>An <c>XdmNode</c>, the Saxon document node at the root of the tree of the resulting
        /// in-memory document.
        /// </returns>
        public XdmNode Wrap(XDocument doc) {
            string baseu = (baseUri == null ? null : baseUri.ToString());
            XDocumentTree tree = new XDocumentTree(doc, baseu, config);
            return (XdmNode) XdmValue.Wrap(tree.getRootNode());
        }
        
        /// <summary>
        /// Wrap a Linq element node, as a Saxon <c>XdmNode</c>.
        /// </summary>
        /// <param name="docWrapper">The wrapper of the containing XDocument node</param>
        /// <param name="node">The Linq element node containing the node to be wrapped</param>
        /// <returns>An <c>XdmNode</c>, wrapping the supplied Linq element node
        /// </returns>
        public XdmNode Wrap(XdmNode docWrapper, XNode node) {
            if (!(docWrapper.Implementation is XNodeWrapper && docWrapper.NodeKind == XdmNodeKind.Document)) {
                throw new IllegalArgumentException("First argument is not a wrapped XDocument node");
            }

            XNodeWrapper doc = ((XNodeWrapper) docWrapper.Implementation);
            if (node.Document != doc.getUnderlyingNode()) {
                throw new IllegalArgumentException(
                    "The node supplied in the second argument does not belong to the document supplied in the first argument");
            }
            XDocumentTree tree = (XDocumentTree)doc.getTreeInfo();
            XNodeWrapper wrappedNode = tree.wrap(node);
            return new XdmNode(wrappedNode);
        }
        
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited