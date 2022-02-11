using System;
using SysCollGen = System.Collections.Generic;
using System.Text;
using Saxon.Api;
using System.Xml;
using System.IO;
using System.Globalization;
using System.Net;
using JConfiguration = net.sf.saxon.Configuration;
using JPreparedStylesheet = net.sf.saxon.PreparedStylesheet;
using JClause = net.sf.saxon.expr.flwor.Clause;
using JCodeInjector = net.sf.saxon.expr.parser.CodeInjector;
using JSimpleCollection = net.sf.saxon.expr.sort.SimpleCollation;
using JDecimalFormatManager = net.sf.saxon.trans.DecimalFormatManager;
using JDecimalSymbols = net.sf.saxon.trans.DecimalSymbols;
using JXpathException = net.sf.saxon.trans.XPathException;
using JUnparsedTextURIResolver = net.sf.saxon.lib.UnparsedTextURIResolver;
using JFeatureKeys = net.sf.saxon.lib.FeatureKeys;
using JStringCollator = net.sf.saxon.lib.StringCollator;
using JAnyURIValue = net.sf.saxon.value.AnyURIValue;
using JItem = net.sf.saxon.om.Item;
using JNamespaceConstant = net.sf.saxon.lib.NamespaceConstant;
using JExpression = net.sf.saxon.expr.Expression;
using JStructuredQName = net.sf.saxon.om.StructuredQName;
using java.net;
using net.sf.saxon;
using net.sf.saxon.expr;
using net.sf.saxon.lib;
using net.sf.saxon.om;
using net.sf.saxon.resource;
using net.sf.saxon.style;
using Saxon.Impl.Helpers;
using TestRunner;
using DocumentBuilder = Saxon.Api.DocumentBuilder;
using Environment = System.Environment;
using Processor = Saxon.Api.Processor;
using Resource = net.sf.saxon.lib.Resource;
using XPathCompiler = Saxon.Api.XPathCompiler;
using XPathSelector = Saxon.Api.XPathSelector;
using XQueryCompiler = Saxon.Api.XQueryCompiler;
using XsltCompiler = Saxon.Api.XsltCompiler;
using XsltExecutable = Saxon.Api.XsltExecutable;
using XsltPackage = Saxon.Api.XsltPackage;

namespace TestRunner {
    /**
     * This class represents a collection of resources (source documents, schemas, collections etc) used for a number
     * of test cases.
     */
    public class Environment : XmlUrlResolver {

        public Processor processor;
        public SysCollGen.Dictionary<string, XdmNode> sourceDocs = new SysCollGen.Dictionary<string, XdmNode>();

        public SysCollGen.Dictionary<string, string>
            streamedSecondaryDocs = new SysCollGen.Dictionary<string, string>();

        public string streamedPath;
        public string streamedContent;
        public XPathCompiler xpathCompiler;
        public XQueryCompiler xqueryCompiler;
        public XsltCompiler xsltCompiler;
        public XsltExecutable xsltExecutable;
        public XdmItem contextItem;
        public SysCollGen.Dictionary<QName, XdmValue> params1 = new SysCollGen.Dictionary<QName, XdmValue>();
        public bool xml11 = false;
        public bool usable = true;
        public StringBuilder paramDeclarations = new StringBuilder(256);
        public StringBuilder paramDecimalDeclarations = new StringBuilder(256);
        public JUnparsedTextURIResolver unparsedTextResolver;
        public SysCollGen.IList<ResetAction> resetActions = new SysCollGen.List<ResetAction>();
        public bool outputTree = true;
        public bool outputSerialize = false;
        public String streamedInputValidation = "skip";
        internal bool failedToBuild = false;


        /**
         * Construct a local default environment for a test set
         */
        public static Environment createLocalEnvironment(Uri baseURI, bool unfolded, Spec spec) {
            Environment environment = new Environment {
                processor = new Processor(true)
            };

            environment.processor.Implementation.setLogger(new TestDriver.MyLogger());

            //environment.processor.SetProperty(JFeatureKeys.TRACE_EXTERNAL_FUNCTIONS, "true");
            environment.xpathCompiler = environment.processor.NewXPathCompiler();
            environment.xpathCompiler.BaseUri = baseURI.ToString(); 
            environment.xqueryCompiler = environment.processor.NewXQueryCompiler();
            environment.xqueryCompiler.BaseUri = baseURI.ToString();
            environment.xsltCompiler = environment.processor.NewXsltCompiler();
            environment.xsltCompiler.BaseUri = new Uri(baseURI.ToString());
            environment.xsltCompiler.XsltLanguageVersion = spec.Equals(Spec.XT30) ? "3.0" : "2.0";

            if (unfolded) {
                // environment.xqueryCompiler.Implementation.setCodeInjector(new LazyLiteralInjector()); //TODO
            }

            environment.processor.Implementation.setDefaultCollection(null);
            return environment;
        }

        private static void DeclareOutputControls(TestRunner.TestDriver driver, XPathCompiler xpc, XdmItem env,
            Environment environment) {
            String needsTree = xpc.Evaluate("string((output/@tree,'yes')[1])", env).ToString();
            environment.outputTree = "yes".Equals(needsTree);
            String needsSerialization = xpc.Evaluate("string((output/@serialize,'no')[1])", env).ToString();
            environment.outputSerialize = "yes".Equals(needsSerialization);
        }

        /**
         * Construct an Environment
         *
         * @param xpc          the XPathCompiler used to process the catalog file
         * @param env          the Environment element in the catalog file
         * @param environments the set of environments to which this one should be added (may be null)
         * @return the constructed Environment object
         * @throws SaxonApiException
         */
        public static Environment processEnvironment(TestRunner.TestDriver driver,
            XPathCompiler xpc, XdmNode env, SysCollGen.Dictionary<string, Environment> environments,
            Environment defaultEnvironment) {
            Environment environment = new Environment();
            String name = env.GetAttributeValue(new QName("name"));
            if (name != null) {
                System.Console.WriteLine("Loading environment " + name);
            }

            environment.processor = new Processor(true);
            if (defaultEnvironment != null) {
                environment.processor.SetProperty(JFeatureKeys.XSD_VERSION,
                    defaultEnvironment.processor.Implementation.getConfigurationProperty(JFeatureKeys.XSD_VERSION)
                        .ToString());
            }

            // AutoActivate.activate(environment.processor);

            environment.xpathCompiler = environment.processor.NewXPathCompiler();
            environment.xpathCompiler.BaseUri = ((XdmNode) env).BaseUri.ToString();
            environment.xqueryCompiler = environment.processor.NewXQueryCompiler();
            environment.xqueryCompiler.BaseUri = ((XdmNode) env).BaseUri.ToString();
            if (driver.Spec.ToString().Contains("XT")) {
                environment.xsltCompiler = environment.processor.NewXsltCompiler();
                environment.xsltCompiler.XsltLanguageVersion = ((SpecAttr) (driver.Spec.GetAttr())).version;
            }

            if (driver.Unfolded) {
                // environment.xqueryCompiler.Implementation.setCodeInjector(new LazyLiteralInjector()); //TODO
            }

            DocumentBuilder builder = environment.processor.NewDocumentBuilder();
            builder.TreeModel = driver.TreeModel;
            environment.sourceDocs = new SysCollGen.Dictionary<string, XdmNode>();
            if (environments != null && name != null) {
                try {
                    environments.Add(name, environment);
                }
                catch (Exception) {
                }
            }

            foreach (XdmItem dependency in xpc.Evaluate("dependency", env)) {
                if (!driver.EnsureDependencySatisfied((XdmNode) dependency, environment)) {
                    environment.usable = false;
                }
            }

            // set the base URI if specified

            SetBaseUri(driver, xpc, env, environment);

            // set any requested collations

            RegisterCollations(xpc, (XdmNode) env, environment);

            // declare the requested namespaces

            DeclareNamespaces(xpc, env, environment);

            // load the requested schema documents

            Saxon.Api.SchemaManager manager = environment.processor.SchemaManager;
            bool validateSources = LoadSchemaDocuments(driver, xpc, env, environment, manager);

            // load the requested source documents

            LoadSourceDocuments(driver, xpc, env, environment, builder, manager, validateSources);

            // register the requested collections

            CreateCollectionFinder(driver, xpc, env, environment, builder);

            // create an unparsed text resolver to handle any unparsed text resources

            CreateUnparsedTextResolver(driver, xpc, env, environment);

            // register any required decimal formats

            RegisterDecimalFormats(driver, xpc, env, environment);

            // declare any variables

            DeclareExternalVariables(driver, xpc, env, environment);

            // declare any output controls
            DeclareOutputControls(driver, xpc, env, environment);

            // handle requested context item
            foreach (XdmItem param in xpc.Evaluate("context-item", env)) {
                String select = ((XdmNode) param).GetAttributeValue(new QName("select"));
                XdmValue value = xpc.Evaluate(select, null);
                environment.contextItem = (XdmItem) value;
            }

            XmlUrlResolver res = new XmlUrlResolver();
            // compile any stylesheet defined as part of the environment (only one allowed)
            DocumentBuilder builder1 = environment.processor.NewDocumentBuilder();
            foreach (XdmItem stylesheet in xpc.Evaluate("stylesheet[not(@role='secondary')]", env)) {
                string fileName = ((XdmNode) stylesheet).GetAttributeValue(new QName("file"));
                try {
                    XdmNode styleSource = builder1.Build(res.ResolveUri(((XdmNode) env).BaseUri, fileName));
                    environment.xsltExecutable = environment.xsltCompiler.Compile(styleSource);
                }
                catch (Exception e) {
                    environment.failedToBuild = true;
                    environment.usable = false;
                    driver.println("**** failure while compiling environment-defined stylesheet " + fileName);
                }
            }


            // compile any stylesheet packages defined as part of the environment
            // Support this only in EE - an unusable environment in PE/HE
            foreach (XdmItem stylesheet in xpc.Evaluate("package[@role='secondary']", env)) {
                if (!"EE".Equals(environment.processor.Edition)) {
                    environment.usable = false;
                    break;
                }

                string fileName = ((XdmNode) stylesheet).GetAttributeValue(new QName("file"));
                Uri uri = res.ResolveUri(((XdmNode) env).BaseUri, fileName);
                FileStream file = new FileStream(uri.AbsolutePath, FileMode.Open, FileAccess.Read);
                try {
                    XsltPackage pkg = environment.xsltCompiler.CompilePackage(file);
                    environment.xsltCompiler.ImportPackage(pkg);
                }
                catch (Exception e) {
                    //e.printStackTrace();
                    driver.println("**** failure while compiling environment-defined stylesheet package " + fileName);
                    driver.println("****Failure " + e.Message + " in compiling environment " + name);
                    environment.usable = false;
                    environment.failedToBuild = true;
                }
            }

            return environment;
        }


        private static void SetBaseUri(TestDriver driver, XPathCompiler xpc, XdmItem env, Environment environment) {
            foreach (XdmItem basei in xpc.Evaluate("static-base-uri", env)) {
                string uri = ((XdmNode) basei).GetAttributeValue(new QName("uri"));
                if (uri == null || "#UNDEFINED".Equals(uri)) {
                    driver.println("**** Error: The BaseURI null and #UNDEFINED is not supported");
                }
                else {
                    try {
                        environment.xpathCompiler.BaseUri = uri;
                        environment.xqueryCompiler.BaseUri = uri;
                    }
                    catch (Exception e) {
                        driver.println("**** invalid base URI " + uri);
                    }
                }
            }
        }

        private static void RegisterCollations(XPathCompiler xpc, XdmNode env, Environment environment) {
            //CompareInfo info = CultureInfo.CurrentCulture.CompareInfo;
            //CompareOptions options = CompareOptions.IgnoreCase;
            foreach (XdmNode basei in env.Children("collation")) {
                string uri = basei.GetAttributeValue("uri");
                if (uri.Equals("http://www.w3.org/2010/09/qt-fots-catalog/collation/caseblind") ||
                    uri.Equals("http://www.w3.org/xslts/collation/caseblind")) {
                    try {
                        JConfiguration config = xpc.Processor.Implementation;
                        JStringCollator collator = config.getCollationURIResolver()
                            .resolve("http://saxon.sf.net/collation?ignore-case=yes", config);

                        environment.processor.DeclareCollation(new Uri(uri), CultureInfo.CurrentCulture.CompareInfo,
                            CompareOptions.IgnoreCase);
                    }
                    catch (Exception e) {
                        throw new Exception(e.Message);
                    }
                }

                string defaultAtt = basei.GetAttributeValue("default");
                if (defaultAtt != null && (defaultAtt.Trim().Equals("true") || defaultAtt.Trim().Equals("1"))) {
                    environment.xpathCompiler.DeclareDefaultCollation(uri);
                    //environment.xqueryCompiler.DeclareCollation(uri, true);
                    environment.xsltCompiler.DefaultCollationName = uri;
                }
            }
        }

        private static void DeclareNamespaces(XPathCompiler xpc, XdmItem env, Environment environment) {
            foreach (XdmItem nsElement in xpc.Evaluate("namespace", env)) {
                String prefix = ((XdmNode) nsElement).GetAttributeValue(new QName("prefix"));
                String uri = ((XdmNode) nsElement).GetAttributeValue(new QName("uri"));
                environment.xpathCompiler.DeclareNamespace(prefix, uri);
                environment.xqueryCompiler.DeclareNamespace(prefix, uri);
                if (uri.Equals("http://expath.org/ns/file")) {
                    // For EXPath file tests, set the EXPath base directory to the catalog directory
                    string basei = ((XdmNode) nsElement).BaseUri.ToString();
                    if (basei.StartsWith("file:///")) {
                        basei = basei.Substring(7);
                    }
                    else if (basei.StartsWith("file:/")) {
                        basei = basei.Substring(5);
                    }

                    FileStream file = new FileStream(basei, FileMode.Open, FileAccess.Read); //new File(base);

                    //System.SetProperty("expath.base.directory", file.ToString());
                }
            }
        }

        private static bool LoadSchemaDocuments(TestDriver driver, XPathCompiler xpc, XdmNode env,
            Environment environment, Saxon.Api.SchemaManager manager) {
            bool validateSources = false;
            Configuration config = environment.processor.Implementation;
            XmlUrlResolver res = new XmlUrlResolver();
            foreach (XdmNode schema in env.Children("schema")) {
                String role = schema.GetAttributeValue(new QName("role"));
                String xsdVersion = schema.GetAttributeValue(new QName("xsd-version"));
                if (manager == null || !driver.schemaAware) {
                    driver.println("** Processor is not schema aware **");
                    environment.usable = false;
                    return false;
                }

                if (xsdVersion != null) {
                    manager.XsdVersion = xsdVersion;
                }

                if (!"secondary".Equals(role)) {
                    String href = ((XdmNode) schema).GetAttributeValue(new QName("file"));
                    String ns = schema.GetAttributeValue("uri");
                    if (href == null) {
                        if (ns == "http://www.w3.org/2005/xpath-functions") {
                            Console.Error.WriteLine("No need to load schema document " + ns);
                        }
                        else {
                            Uri uri = res.ResolveUri(null, ns);
                            Console.Error.WriteLine("Load schema document " + uri);
                            Stream file = KnownResourceFinder.fetchSystemId(uri.ToString(), config).stream;
                            if (file == null) {
                                file = new FileStream(uri.AbsolutePath, FileMode.Open, FileAccess.Read);
                            }

                            try {
                                manager.Compile(file, uri);
                            }
                            catch (Exception err) {
                                System.Console.WriteLine("*** Failed to load schema: " + err.Message + ", Trace:" +
                                                         err.StackTrace);
                            }
                        }
                    }
                    else {
                        Uri uri = res.ResolveUri(((XdmNode) env).BaseUri, href);
                        Console.Error.WriteLine("Load schema document " + uri + " ~ " + href);
                        FileStream file = new FileStream(uri.AbsolutePath, FileMode.Open, FileAccess.Read);
                        try {
                            manager.Compile(file, uri);
                        }
                        catch (Exception err) {
                            System.Console.WriteLine("*** Failed to load schema: " + err.Message + ", Trace:" +
                                                     err.StackTrace);
                        }
                    }

                    xpc.ImportSchemaNamespace(ns);
                    environment.xpathCompiler.ImportSchemaNamespace(ns);
                    if ("source-reference".Equals(role)) {
                        validateSources = true;
                    }

                    if ("stylesheet-import".Equals(role)) {
                        environment.xsltCompiler.SchemaAware = true;
                    }
                }
            }

            return validateSources;
        }

        private static void DeclareExternalVariables(TestDriver driver, XPathCompiler xpc, XdmItem env,
            Environment environment) {
            foreach (XdmItem param in xpc.Evaluate("param", env)) {
                String varName = ((XdmNode) param).GetAttributeValue(new QName("name"));
                XdmValue value;
                String source = ((XdmNode) param).GetAttributeValue(new QName("source"));
                if (source != null) {
                    XdmNode sourceDoc = null;
                    try {
                        sourceDoc = environment.sourceDocs[source];
                    }
                    catch (Exception) {
                    }

                    if (sourceDoc == null) {
                        driver.println("**** Unknown source document " + source);
                    }

                    value = sourceDoc;
                }
                else {
                    String select = ((XdmNode) param).GetAttributeValue(new QName("select"));
                    value = xpc.Evaluate(select, null);
                }

                bool isStatic = Xslt30TestSuiteDriver.IsTrue("static").Invoke((XdmNode) param);
                QName varQName;
                int colon = varName.IndexOf(':');
                if (colon >= 0) {
                    varQName = new QName(varName, (XdmNode) param);
                }
                else {
                    varQName = new QName(varName);
                }


                environment.xpathCompiler.DeclareVariable(varQName);
                String declared = ((XdmNode) param).GetAttributeValue(new QName("declared"));
                if (declared != null && "true".Equals(declared) || "1".Equals(declared)) {
                    // no action
                }
                else {
                    environment.paramDeclarations.Append("declare variable $" + varName + " external; ");
                }

                if (isStatic) {
                    environment.xsltCompiler.SetParameter(varQName, value);
                }
                else {
                    environment.params1.Add(varQName, value);
                }
            }
        }

        private static void RegisterDecimalFormats(TestDriver driver, XPathCompiler xpc, XdmItem env,
            Environment environment) {
            foreach (XdmItem decimalFormat in xpc.Evaluate("decimal-format", env)) {
                //DecimalFormatManager dfm = environment.xpathCompiler..getUnderlyingStaticContext().getDecimalFormatManager();

                XdmNode formatElement = (XdmNode) decimalFormat;
                String formatName = formatElement.GetAttributeValue(new QName("name"));
                QName formatQName = null;
                if (formatName != null) {
                    if (formatName.IndexOf(':') < 0) {
                        formatQName = new QName("", "", formatName);
                    }
                    else {
                        try {
                            formatQName = new QName(formatName, formatElement);
                        }
                        catch (Exception e) {
                            driver.println("**** Invalid QName as decimal-format name");
                            formatQName = new QName("", "", "error-name");
                        }
                    }

                    environment.paramDecimalDeclarations.Append("declare decimal-format " + formatQName.EQName + " ");
                }
                else {
                    environment.paramDecimalDeclarations.Append("declare default decimal-format ");
                }

                foreach (XdmItem decimalFormatAtt in xpc.Evaluate("@* except @name", formatElement)) {
                    XdmNode formatAttribute = (XdmNode) decimalFormatAtt;
                    String property = formatAttribute.NodeName.LocalName;
                    String value = formatAttribute.StringValue;
                    environment.paramDecimalDeclarations.Append(property + "=\"" + value + "\" ");
                    try {
                        environment.xpathCompiler.SetDecimalFormatProperty(formatQName, property, value);
                    }
                    catch (Exception e) {
                        driver.println("**** Error in Decimal format: " + e.Message);
                    }
                }

                environment.paramDecimalDeclarations.Append(";");
            }
        }

        private static void CreateUnparsedTextResolver(TestDriver driver, XPathCompiler xpc, XdmItem env,
                Environment environment) {
            SysCollGen.Dictionary<Uri, Func<TextReader>> resources = new SysCollGen.Dictionary<Uri, Func<TextReader>>();
            SysCollGen.Dictionary<Uri, String> encodings = new SysCollGen.Dictionary<Uri, String>();
            XmlUrlResolver res = new XmlUrlResolver();
            
            // For each resource in the environment, create a dictionary entry that maps
            // the URI of the resource to a factory function that delivers a TextReader
            // for that resource
            
            foreach (XdmItem resource in xpc.Evaluate("resource", env)) {
                String nominalUriStr = ((XdmNode) resource).GetAttributeValue(new QName("uri"));
                Uri nominalUri = new Uri(nominalUriStr, UriKind.RelativeOrAbsolute);
                if (!nominalUri.IsAbsoluteUri) {
                    nominalUri = new Uri(((XdmNode) env).BaseUri, nominalUri);
                }
                String href = ((XdmNode) resource).GetAttributeValue(new QName("file"));
                String encoding = ((XdmNode) resource).GetAttributeValue(new QName("encoding"));
                if (encoding == null) {
                    encoding = "utf-8"; // TODO: should infer from content etc?
                }
                
                Encoding enc1 = Encoding.GetEncoding(encoding, 
                    EncoderFallback.ExceptionFallback, 
                    new DecoderExceptionFallback());
                String mediaType = ((XdmNode) resource).GetAttributeValue(new QName("media-type"));
                if (href != null) {
                    if (href.StartsWith("http")) {
                        resources.Add(nominalUri, () => {
                            WebClient wc = new WebClient();
                            wc.Encoding = enc1;
                            byte[] content = wc.DownloadData(href);
                            string str = enc1.GetString(content);
                            //string str = wc.DownloadString(href);
                            return new StringReader(str);
                        });
                    }
                    else {
                        Uri resourceUri = new Uri(((XdmNode) env).BaseUri, href);
                        // //Uri alias = new Uri(uri, UriKind.RelativeOrAbsolute);
                        // if (!resourceUri.IsAbsoluteUri) {
                        //     alias = new Uri(((XdmNode) env).BaseUri, alias);
                        // }

                        
                        resources.Add(nominalUri, () => {
                            try {
                                // TODO: this logic should surely be in the standard UnparsedTextResolver?
                                WebClient wc = new WebClient();
                                byte[] content = wc.DownloadData(resourceUri);
                                Encoding enc2 = enc1;
                                if (encoding == "utf-16" && content.Length >= 2) {
                                    // Seems we need to resolve endianness ourselves
                                    if (content[0] == 0xfe && content[1] == 0xff) {
                                        enc2 = Encoding.BigEndianUnicode;
                                    } else if (content[0] == 0xff && content[1] == 0xfe) {
                                        enc2 = Encoding.Unicode;
                                    }
                                }
                                string str = enc2.GetString(content);
                                if (str.Length > 0 && str[0] == 0xfffe) {
                                    str = str.Substring(1);
                                }
                                //string str = wc.DownloadString(href);
                                return new StringReader(str);
                            }
                            catch (System.Text.DecoderFallbackException e) {
                                throw new JXpathException(e.Message, "FOUT1190");
                            }
                        });
                        
                    }
                    

                    try {
                        encodings.Add(nominalUri, encoding);
                        

                        // environment.catalogResources.Add();
                    }
                    catch (Exception e) {
                        driver.println("** Invalid URI in environment: " + e.Message);
                    }

                    if (mediaType != null && mediaType.EndsWith("xquery")) {
                        driver.RegisterXQueryModule(nominalUri,
                            res.ResolveUri(((XdmNode) env).BaseUri, href).AbsolutePath);
                    }
                }
            }

            if (resources.Count != 0) {
                environment.unparsedTextResolver =
                    new TestUnparsedTextURIResolver(encodings, resources);
            }
        }

        public class TestUnparsedTextURIResolver : UnparsedTextURIResolver {
            SysCollGen.Dictionary<Uri, String> encodings;
            SysCollGen.Dictionary<Uri, Func<TextReader>> resources;

            public TestUnparsedTextURIResolver(SysCollGen.Dictionary<Uri, String> encodings,
                SysCollGen.Dictionary<Uri, Func<TextReader>> resources) {
                this.encodings = encodings;
                this.resources = resources;
            }


            public TextReader resolve(Uri absoluteURI, string encoding, Configuration config) {
                if (encoding == null) {
                    encodings.TryGetValue(new Uri(absoluteURI.ToString()), out encoding);
                }
                else {
                    try {
                        Encoding.GetEncoding(encoding);
                    }
                    catch (ArgumentException err) {
                        throw new net.sf.saxon.trans.XPathException("Unknown encoding " + encoding, "FOUT1190");
                    }
                }

                if (encoding == null) {
                    encoding = "utf-8";
                }

                try {
                    return resources[absoluteURI].Invoke();
                }
                catch (Exception ioe) {
                    net.sf.saxon.trans.XPathException xpe = new net.sf.saxon.trans.XPathException("Unknown URI " + absoluteURI, ioe);
                    xpe.setErrorCode("FOUT1170");
                    throw xpe;
                }

                /*catch (IllegalCharsetNameException icne)
                {
                    throw new XPathException("Invalid encoding name: " + encoding, "FOUT1190");
                }
                catch (UnsupportedCharsetException uce)
                {
                    throw new XPathException("Invalid encoding name: " + encoding, "FOUT1190");
                }*/
            }
        }

        private static void CreateCollectionFinder(TestDriver driver, XPathCompiler xpc, XdmNode env,
            Environment environment, DocumentBuilder builder) {
            
            Configuration config = environment.processor.Implementation;
            XmlUrlResolver res = new XmlUrlResolver();
        
            if (!environment.usable) {
                return;
            }
        
            foreach (XdmNode coll in env.Children("collection")) {
                var resourceList = new SysCollGen.List<Saxon.Api.IResource>();
                String collectionURI = coll.GetAttributeValue(new QName("uri"));
                if (collectionURI == null) {
                    collectionURI = "";
                }
        
                if (collectionURI.Equals("")) {
                    collectionURI = "http://www.w.org/qt3-test-suite/default.collection.uri";
                    config.setDefaultCollection(collectionURI);
                }

                
                Uri u;
                try {
                    u = new Uri(collectionURI, UriKind.RelativeOrAbsolute);
                }
                catch (Exception e) {
                    driver.println("**** Invalid collection URI " + collectionURI);
                    break;
                }
        
                if (!collectionURI.Equals("") && !u.IsAbsoluteUri) {
                    u = res.ResolveUri(env.BaseUri, collectionURI);
                    collectionURI = u.ToString();
                    driver.println("Registering collection URI: " + collectionURI);
                }
        
                ExplicitResourceCollection collection = new ExplicitResourceCollection(collectionURI, resourceList);
                environment.processor.RegisterCollection(collectionURI, collection);
        
                foreach (XdmNode source in coll.Children("source")) {
                    String href = source.GetAttributeValue(new QName("file"));
                    String frag = null;
                    int hash = href.IndexOf('#');
                    if (hash > 0) {
                        frag = href.Substring(hash + 1);
                        href = href.Substring(0, hash);
                    }
        
                    FileStream file = new FileStream(res.ResolveUri(env.BaseUri, href).AbsolutePath,
                        FileMode.Open, FileAccess.Read);
        
                    string uriStr = res.ResolveUri(env.BaseUri, href).AbsoluteUri;
                    builder.BaseUri = new Uri(uriStr);
                    XdmNode doc = builder.Build(file);
                    
                    if (frag != null) {
                        XdmNode selected =
                            (XdmNode) environment.xpathCompiler.EvaluateSingle("id('" + frag + "')", doc);
                        if (selected == null) {
                            driver.println("**** Fragment not found: " + frag);
                            break;
                        }
        
                        resourceList.Add(Saxon.Api.Resource.XmlResource(uriStr, selected));
                    }
                    else {
                        resourceList.Add(Saxon.Api.Resource.XmlResource(uriStr, doc));
                    }
        
                    environment.sourceDocs.Add(uriStr, doc);
                }
                
                foreach (XdmNode query in coll.Children("query")) {
                    String queryExpr = query.GetStringValue();
                    XQueryEvaluator evaluator = environment.xqueryCompiler.Compile(queryExpr).Load();
                    XdmValue result = evaluator.Evaluate();

                    foreach (XdmItem itemi in result) {
                        resourceList.Add(new AtomicResource(null, (XdmAtomicValue)itemi));
                    }
                }
                
        
                // foreach (XdmItem source in xpc.Evaluate("resources", coll)) {
                //     String uri = ((XdmNode) source).GetAttributeValue(new QName("uri"));
                //     String href = ((XdmNode) source).GetAttributeValue(new QName("file"));
                //     String encoding = ((XdmNode) source).GetAttributeValue(new QName("encoding"));
                //     String media = ((XdmNode) source).GetAttributeValue(new QName("media-type"));
                //     net.sf.saxon.resource.AbstractResourceCollection.InputDetails details =
                //         new net.sf.saxon.resource.AbstractResourceCollection.InputDetails();
                //     details.encoding = encoding;
                //     details.contentType = media;
                //     FileStream file = null;
                //     if (href != null) {
                //         if (href.StartsWith("http")) {
                //             try {
                //                 URL url = new URL(href);
                //                 URLConnection connection = url.openConnection();
                //                 if (details.contentType == null) {
                //                     details.contentType = connection.getContentType();
                //                 }
                //
                //                 details.contentType = connection.getContentType();
                //             }
                //             catch (Exception e) {
                //                 throw new Exception(e.StackTrace);
                //             }
                //         }
                //         else {
                //             try {
                //                 file = new FileStream(res.ResolveUri(((XdmNode) env).BaseUri, href).AbsolutePath,
                //                     FileMode.Open, FileAccess.Read);
                //                 Resource resourcej = collectioni.MakeResource(config, details,
                //                     res.ResolveUri(((XdmNode) env).BaseUri, href).AbsolutePath);
                //                 if (resourcej != null) {
                //                     resourceList.Add(resourcej);
                //                 }
                //                 else {
                //                     driver.println("** Error in building collection environment: Resource " + href +
                //                                    " not found: ");
                //                 }
                //             }
                //             catch (Exception e) {
                //                 driver.println("** IO Error in building collection environment: " + e.Message);
                //             }
                //
                //             if (media != null && href.EndsWith("xquery")) {
                //                 driver.RegisterXQueryModule(res.ResolveUri(((XdmNode) env).BaseUri, href),
                //                     res.ResolveUri(((XdmNode) env).BaseUri, href).AbsolutePath);
                //             }
                //         }
                //     }
                // }
        
                
            }

            
        }

        public class AtomicResource : IResource {
            private readonly string uri;
            private readonly XdmAtomicValue value;

            public AtomicResource(string uri, XdmAtomicValue value) {
                this.uri = uri;
                this.value = value;
            }

            public string ResourceURI => uri;
            public XdmItem GetXdmItem(DynamicContext context) {
                return value;
            }

            public string ContentType => "application/xdm-atomic-value";


        }


        public override object GetEntity(Uri href, string role, Type ofObjectToReturn) {
            XdmNode node = null;
            String hrefStr = href.ToString();
            //hrefStr = hrefStr.Substring(hrefStr.LastIndexOf ('/')+1);

            bool foundDoc = sourceDocs.TryGetValue(hrefStr, out node);

            if (!foundDoc) {
                hrefStr = hrefStr.Substring(hrefStr.LastIndexOf('/') + 1);
                foundDoc = sourceDocs.TryGetValue(hrefStr, out node);
            }

            if (!foundDoc) {
                string val;
                hrefStr = href.ToString();
                //hrefStr = hrefStr.Substring(hrefStr.LastIndexOf ('/')+1);

                bool foundSecDoc = streamedSecondaryDocs.TryGetValue(hrefStr, out val);
                if (!foundSecDoc) {
                    hrefStr = hrefStr.Substring(hrefStr.LastIndexOf('/') + 1);
                    foundSecDoc = streamedSecondaryDocs.TryGetValue(hrefStr, out val);
                }

                if (foundSecDoc) {
                    //string hrefStr = href.ToString ();
                    String hrefStri = (hrefStr.StartsWith("file:///") ? hrefStr.Substring(7) : hrefStr);
                    Stream stream = new FileStream(hrefStri, FileMode.Open);
                    
                    javax.xml.transform.Source sourcei = new javax.xml.transform.stream.StreamSource(stream);


                    if (!val.Equals("skip")) {
                        sourcei = net.sf.saxon.lib.AugmentedSource.makeAugmentedSource(sourcei);
                        ((net.sf.saxon.lib.AugmentedSource) sourcei).setSchemaValidationMode(
                            net.sf.saxon.lib.Validation.getCode(val));
                    }

                    sourcei.setSystemId(hrefStr);
                    return sourcei;
                }
                else {
                    return null;
                }
            }
            else {
                return node.Implementation;
            }
        }

        public override Uri ResolveUri(Uri basei, string href) {
            return base.ResolveUri(basei, href);
        }

        private static void LoadSourceDocuments(TestDriver driver, XPathCompiler xpc, XdmItem env,
            Environment environment, DocumentBuilder builder, SchemaManager manager, bool validateSources) {
            XmlUrlResolver res = new XmlUrlResolver();
            foreach (XdmItem source in xpc.Evaluate("source", env)) {
                XdmNode doc = null;

                String uri = ((XdmNode) source).GetAttributeValue(new QName("uri"));
                String mime = ((XdmNode) source).GetAttributeValue(new QName("media-type"));
                
                if (mime != null && "application/xml".Equals(mime)) {
                    continue;
                }

                String validation = ((XdmNode) source).GetAttributeValue(new QName("", "validation"));
                if (validation == null) {
                    validation = "skip";
                }

                String streaming = ((XdmNode) source).GetAttributeValue(new QName("", "streaming"));
                if (!validateSources && validation.Equals("skip")) {
                    builder.SchemaValidationMode = SchemaValidationMode.None;
                    builder.SchemaValidator = null;
                }
                else {
                    SchemaValidator validator = manager.NewSchemaValidator();
                    if ("lax".Equals(validation)) {
                        validator.IsLax = true;
                        builder.SchemaValidator = validator;
                        builder.SchemaValidationMode = SchemaValidationMode.Lax;
                    }
                    else {
                        builder.SchemaValidator = validator;
                        builder.SchemaValidationMode = SchemaValidationMode.Strict;
                    }

                    environment.xpathCompiler.SchemaAware = true;
                    environment.xqueryCompiler.SchemaAware = true;
                    if (environment.xsltCompiler != null) {
                        environment.xsltCompiler.SchemaAware = true;
                    }
                }

                String role = ((XdmNode) source).GetAttributeValue(new QName("role"));
                String href = ((XdmNode) source).GetAttributeValue(new QName("file"));
                
                Console.Error.WriteLine("Load source document " + href);
                
                if ("true".Equals(streaming)) {
                    if (".".Equals(role)) {
                        if (href == null) {
                            environment.streamedContent = xpc.Evaluate("string(content)", source).ToString();
                        }
                        else {
                            environment.streamedPath = res.ResolveUri(((XdmNode) env).BaseUri, href).AbsolutePath;
                        }

                        environment.streamedInputValidation = validation;
                    }
                    else {
                        try {
                            System.Console.WriteLine(res.ResolveUri(((XdmNode) env).BaseUri, href).AbsolutePath);
                            System.Console.WriteLine(res.ResolveUri(((XdmNode) env).BaseUri, href).AbsoluteUri);
                            System.Console.WriteLine(res.ResolveUri(((XdmNode) env).BaseUri, href).ToString());
                            environment.streamedSecondaryDocs.Add(
                                res.ResolveUri(((XdmNode) env).BaseUri, href).AbsoluteUri, validation);
                        }
                        catch (Exception) {
                        }
                    }
                }
                else {
                    Stream ss;
                    /*if (uri != null) {
                        uri = res.ResolveUri(((XdmNode)env).BaseUri, uri).AbsoluteUri;
                    }*/
                    if (href != null) {
                        try {
                            Uri fileLoc = res.ResolveUri(((XdmNode) env).BaseUri, href);
                            FileStream file = null;
                            if (fileLoc.Scheme.Equals("file")) {
                                file = new FileStream(res.ResolveUri(((XdmNode) env).BaseUri, href).AbsolutePath,
                                    FileMode.Open, FileAccess.Read);
                                if (uri == null) {
                                    uri = fileLoc.AbsoluteUri;
                                }
                            }

                            ss = (file == null
                                ? new FileStream(fileLoc.ToString(), FileMode.Open, FileAccess.Read)
                                : file);

                            builder.BaseUri = ((XdmNode) env).BaseUri;
                        }
                        catch (Exception e) {
                            driver.println("*** failed to find source document " + href + ", Exception:" + e.Message);
                            continue;
                        }
                    }
                    else {
                        // content is inline in the catalog
                        if (uri == null) {
                            uri = ((XdmNode) env).BaseUri.AbsolutePath;
                        }

                        string content = xpc.Evaluate("string(content)", source).ToString();
                        byte[] byteArray = Encoding.ASCII.GetBytes(content);
                        ss = new MemoryStream(byteArray);
                        builder.BaseUri = ((XdmNode) env).BaseUri;
                    }


                    try {
                        builder.BaseUri = new Uri(res.ResolveUri(((XdmNode) env).BaseUri, uri).AbsoluteUri);
                        doc = builder.Build(ss);
                        environment.sourceDocs.Add(uri, doc);
                    }
                    catch (Exception e) {
                        Console.Error.WriteLine(e);
                        driver.println("*** failed to build source document " + href + ", Exception:" + e.Message);
                    }

                    XdmItem selectedItem = doc;
                    String select = ((XdmNode) source).GetAttributeValue(new QName("select"));
                    if (select != null) {
                        XPathSelector selector = environment.xpathCompiler.Compile(select).Load();
                        selector.ContextItem = selectedItem;
                        selectedItem = selector.EvaluateSingle();
                    }


                    if (role != null) {
                        if (".".Equals(role)) {
                            environment.contextItem = selectedItem;
                        }
                        else if (role.StartsWith("$")) {
                            String varName = role.Substring(1);
                            environment.params1.Add(new QName(varName), selectedItem);
                            environment.xpathCompiler.DeclareVariable(new QName(varName));
                            environment.paramDeclarations.Append("declare variable $" + varName + " external; ");
                        }
                    }
                }

                String definesStylesheet = ((XdmNode) source).GetAttributeValue(new QName("defines-stylesheet"));
                if (definesStylesheet != null) {
                    definesStylesheet = definesStylesheet.Trim();
                }

                if ("true".Equals(definesStylesheet) || "1".Equals(definesStylesheet)) {
                    // try using an embedded stylesheet from the source document
                    try {
                        environment.xsltExecutable =
                            environment.xsltCompiler.CompileAssociatedStylesheet(((XdmNode) environment.contextItem));
                    }
                    catch (Exception e) {
                        driver.println("*** failed to compile stylesheet referenced in source document " + href);
                        Console.Error.Write(e.StackTrace);
                    }
                }
            }
        }

        // public class JAbstractResourceCollection : ResourceCollection {
        //     private String cURI;
        //     private List<Resource> resourcesOrig;
        //     private java.util.List resourcesi = new java.util.ArrayList();
        //
        //     public JAbstractResourceCollection(String uri, List<Resource> ri) {
        //         resourcesOrig = ri;
        //         cURI = uri;
        //     }
        //
        //     public IEnumerator<Resource> getResources(XPathContext value) {
        //         resourcesi.clear();
        //         foreach (Resource r in resourcesOrig) {
        //             resourcesi.add(r);
        //         }
        //
        //         return resourcesi.iterator();
        //     }
        //
        //     public IEnumerator<string> getResourceURIs(XPathContext value) {
        //         SysCollGen.IList<string> resourceUris = SysCollGen.ArrayList<string>();
        //         foreach (Resource r in resourcesOrig) {
        //             resourcesi.add(r.getResourceURI());
        //         }
        //
        //         return resourcesi.iterator();
        //     }
        //
        //     string ResourceCollection.getCollectionURI() {
        //         return cURI;
        //     }
        //
        //     Iterator ResourceCollection.getResources(XPathContext xpc) {
        //         SysCollGen.List<string> resourceUris = new SysCollGen.List<string>();
        //         foreach (Resource r in resourcesOrig) {
        //             resourcesi.add(r);
        //         }
        //
        //         return resourcesi.iterator();
        //     }
        //
        //     Iterator ResourceCollection.getResourceURIs(XPathContext xpc) {
        //         SysCollGen.List<string> resourceUris = new SysCollGen.List<string>();
        //         foreach (Resource r in resourcesOrig) {
        //             resourcesi.add(r.getResourceURI());
        //         }
        //
        //         return resourcesi.iterator();
        //     }
        //
        //     bool ResourceCollection.isStable(XPathContext xpc) {
        //         return false;
        //     }
        //
        //     bool ResourceCollection.stripWhitespace(SpaceStrippingRule ssr) {
        //         throw new NotImplementedException();
        //     }
        //
        //     internal Resource MakeResource(JConfiguration config, AbstractResourceCollection.InputDetails details,
        //         string absolutePath) {
        //         net.sf.saxon.lib.ResourceFactory factory = null;
        //         String contentType = details.contentType;
        //         if (contentType != null) {
        //             factory = config.getResourceFactoryForMediaType(contentType);
        //         }
        //
        //         if (factory == null) {
        //             factory = BinaryResource.FACTORY;
        //         }
        //
        //         return factory.makeResource(config, details);
        //     }
        // }


        // public class CollectionFinder : net.sf.saxon.lib.CollectionFinder {
        //     SysCollGen.Dictionary<Uri, ResourceCollection> collections;
        //
        //     public CollectionFinder(SysCollGen.Dictionary<Uri, ResourceCollection> collections) {
        //         this.collections = collections;
        //     }
        //
        //     public ResourceCollection findCollection(XPathContext xpc, string str) {
        //         if (str == null) {
        //             str = "";
        //         }
        //
        //         return (ResourceCollection) collections[new Uri(str)];
        //     }
        // }


        // public class CollectionResolver /*:net.sf.saxon.lib.CollectionURIResolver */ {
        //     System.Collections.Hashtable collections;
        //
        //     public CollectionResolver(System.Collections.Hashtable collections) {
        //         this.collections = collections;
        //     }
        //
        //     public net.sf.saxon.om.SequenceIterator resolve(string href, string baseStr,
        //         net.sf.saxon.expr.XPathContext context) {
        //         try {
        //             List<Uri> docs;
        //             if (href == null) {
        //                 docs = (List<Uri>) collections[new Uri("http://www.saxonica.com/defaultCollection")];
        //             }
        //             else {
        //                 XmlUrlResolver res = new XmlUrlResolver();
        //                 Uri abs = res.ResolveUri(new Uri(baseStr), href);
        //                 try {
        //                     docs = (List<Uri>) collections[abs];
        //                 }
        //                 catch (Exception) {
        //                     docs = null;
        //                 }
        //             }
        //
        //             if (docs == null) {
        //                 return net.sf.saxon.tree.iter.EmptyIterator.getInstance();
        //             }
        //             else {
        //                 SysCollGen.List<string> list = new SysCollGen.List<string>();
        //                 foreach (Uri uri in docs) {
        //                     list.Add(new net.sf.saxon.value.AnyURIValue(uri.ToString()));
        //                 }
        //
        //                 return new net.sf.saxon.tree.iter.ListIterator(list);
        //             }
        //         }
        //         catch (java.net.URISyntaxException e) {
        //             System.Console.WriteLine("** Invalid Uri: " + e.Message);
        //             return net.sf.saxon.tree.iter.EmptyIterator.getInstance();
        //         }
        //     }
        // }
        
        public delegate void ResetAction(Environment env);
    }
}