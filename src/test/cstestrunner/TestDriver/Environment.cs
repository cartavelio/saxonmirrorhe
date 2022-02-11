using System;
using SysCollGen = System.Collections.Generic;
using System.Text;
using Saxon.Api;
using System.Xml;
using System.IO;
using System.Globalization;
using System.Net;
using JConfiguration = Saxon.Hej.Configuration;
using JPreparedStylesheet = Saxon.Hej.PreparedStylesheet;
using JClause = Saxon.Hej.expr.flwor.Clause;
using JCodeInjector = Saxon.Hej.expr.parser.CodeInjector;
using JSimpleCollection = Saxon.Hej.expr.sort.SimpleCollation;
using JDecimalFormatManager = Saxon.Hej.trans.DecimalFormatManager;
using JDecimalSymbols = Saxon.Hej.trans.DecimalSymbols;
using JXpathException = Saxon.Hej.trans.XPathException;
using JUnparsedTextURIResolver = Saxon.Hej.lib.UnparsedTextURIResolver;
using JFeatureKeys = Saxon.Hej.lib.FeatureKeys;
using JStringCollator = Saxon.Hej.lib.StringCollator;
using JAnyURIValue = Saxon.Hej.value.AnyURIValue;
using JItem = Saxon.Hej.om.Item;
using JNamespaceConstant = Saxon.Hej.lib.NamespaceConstant;
using JExpression = Saxon.Hej.expr.Expression;
using JStructuredQName = Saxon.Hej.om.StructuredQName;
using Saxon.Hej;
using Saxon.Hej.lib;
using Saxon.Callbacks;
using Saxon.Eej.ee.stream;
using Saxon.Impl.Helpers;
using DocumentBuilder = Saxon.Api.DocumentBuilder;
using Processor = Saxon.Api.Processor;
using XPathCompiler = Saxon.Api.XPathCompiler;
using XPathSelector = Saxon.Api.XPathSelector;
using XQueryCompiler = Saxon.Api.XQueryCompiler;
using XsltCompiler = Saxon.Api.XsltCompiler;
using XsltExecutable = Saxon.Api.XsltExecutable;
using XsltPackage = Saxon.Api.XsltPackage;

namespace TestDriver {
    /**
     * This class represents a collection of resources (source documents, schemas, collections etc) used for a number
     * of test cases.
     */
    public class Environment : XmlUrlResolver {

        public Processor processor;
        public SysCollGen.Dictionary<string, XdmNode> sourceDocs = new();
        private readonly SysCollGen.Dictionary<string, string> streamedSecondaryDocs = new();
        private readonly SysCollGen.Dictionary<Uri, SysCollGen.List<IResource>> collections = new(); 

        public string streamedPath;
        public string streamedContent;
        public XPathCompiler xpathCompiler;
        public XQueryCompiler xqueryCompiler;
        public XsltCompiler xsltCompiler;
        public XsltExecutable xsltExecutable;
        public XdmItem contextItem;
        public SysCollGen.Dictionary<QName, XdmValue> params1 = new();
        public bool xml11 = false;
        public bool usable = true;
        public StringBuilder paramDeclarations = new StringBuilder(256);
        public StringBuilder paramDecimalDeclarations = new StringBuilder(256);
        public TextResolver unparsedTextResolver;
        public SysCollGen.IList<ResetAction> resetActions = new SysCollGen.List<ResetAction>();
        public bool outputTree = true;
        public bool outputSerialize = false;
        public string streamedInputValidation = "skip";
        internal bool failedToBuild = false;


        /**
         * Construct a local default environment for a test set
         */
        public static Environment createLocalEnvironment(Uri baseURI, bool unfolded, Spec spec) {
            Environment environment = new Environment {
                processor = new Processor(true)
            };

            environment.processor.Implementation.setLogger(new Driver.MyLogger());

            //environment.processor.SetProperty(JFeatureKeys.TRACE_EXTERNAL_FUNCTIONS, "true");
            string version = SpecAttr.VersionOf(spec);
            string xpVersion = version == "3.0" ? "3.1" : version;
            string xqVersion = xpVersion == "2.0" ? "3.1" : version;
            string xtVersion = (version == "3.1" || version == "2.0") ? "3.0" : version;
            environment.xpathCompiler = environment.processor.NewXPathCompiler();
            environment.xpathCompiler.BaseUri = baseURI;
            environment.xpathCompiler.XPathLanguageVersion = xpVersion;
            environment.xqueryCompiler = environment.processor.NewXQueryCompiler();
            environment.xqueryCompiler.BaseUri = baseURI;
            environment.xqueryCompiler.XQueryLanguageVersion = xqVersion;
            environment.xsltCompiler = environment.processor.NewXsltCompiler();
            environment.xsltCompiler.BaseUri = baseURI;
            environment.xsltCompiler.XsltLanguageVersion = xtVersion;

            if (unfolded) {
                // environment.xqueryCompiler.Implementation.setCodeInjector(new LazyLiteralInjector()); //TODO
            }
            
            return environment;
        }

        private static void DeclareOutputControls(TestDriver.Driver driver, XPathCompiler xpc, XdmItem env,
            Environment environment) {
            string needsTree = xpc.Evaluate("string((output/@tree,'yes')[1])", env).ToString();
            environment.outputTree = "yes".Equals(needsTree);
            string needsSerialization = xpc.Evaluate("string((output/@serialize,'no')[1])", env).ToString();
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
        public static Environment processEnvironment(TestDriver.Driver driver,
                XPathCompiler xpc, XdmNode env, SysCollGen.Dictionary<string, Environment> environments,
                Environment defaultEnvironment) {
            Environment environment = new Environment();
            string name = env.GetAttributeValue(new QName("name"));
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
            string version = SpecAttr.VersionOf(driver.RequestedSpec);
            string xpVersion = version == "3.0" ? "3.1" : version;
            environment.xpathCompiler = environment.processor.NewXPathCompiler();
            environment.xpathCompiler.BaseUri = env.BaseUri;
            environment.xpathCompiler.XPathLanguageVersion = xpVersion;
            environment.xqueryCompiler = environment.processor.NewXQueryCompiler();
            environment.xqueryCompiler.BaseUri = env.BaseUri;
            environment.xqueryCompiler.XQueryLanguageVersion = (version == "2.0" ? "3.1" : xpVersion);
            if (driver.RequestedSpec.ToString().Contains("XT")) {
                environment.xsltCompiler = environment.processor.NewXsltCompiler();
                environment.xsltCompiler.XsltLanguageVersion = version;
            }

            if (driver.Unfolded) {
                // environment.xqueryCompiler.Implementation.setCodeInjector(new LazyLiteralInjector()); //TODO
            }

            DocumentBuilder builder = environment.processor.NewDocumentBuilder();
            builder.LineNumbering = true;
            builder.TreeModel = driver.TreeModel;
            environment.sourceDocs = new SysCollGen.Dictionary<string, XdmNode>();
            if (environments != null && name != null) {
                try {
                    environments.Add(name, environment);
                }
                catch (Exception) {
                    // no action
                }
            }
            
            // Handle dependencies

            foreach (XdmItem dependency in xpc.Evaluate("(dependency, ../dependencies/*)", env)) {
                if (!driver.EnsureDependencySatisfied((XdmNode) dependency, environment)) {
                    environment.usable = false;
                }
            }
            
            // Handle configuration properties

            foreach (XdmItem property in xpc.Evaluate("Q{http://saxon.sf.net/}property", env)) {
                string propertyName = ((XdmNode) property).GetAttributeValue(new QName("name"));
                string propertyValue = ((XdmNode) property).GetAttributeValue(new QName("value"));
                environment.processor.SetProperty(propertyName, propertyValue);
            }

            // set the base URI if specified

            SetBaseUri(driver, xpc, env, environment);

            // set any requested collations

            RegisterCollations(xpc, env, environment);

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
                string select = ((XdmNode) param).GetAttributeValue(new QName("select"));
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
                catch (Exception) {
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
                Uri uri = res.ResolveUri(env.BaseUri, fileName);
                FileStream file = new FileStream(uri.AbsolutePath, FileMode.Open, FileAccess.Read);
                try {
                    XsltPackage pkg = environment.xsltCompiler.CompilePackage(file, uri);
                    environment.xsltCompiler.ImportPackage(pkg);
                    // Make the package available for use from fn:transform
                    environment.processor.Implementation.getDefaultXsltCompilerInfo().getPackageLibrary().addPackage(pkg.getUnderlyingPreparedPackage());

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


        private static void SetBaseUri(Driver driver, XPathCompiler xpc, XdmItem env, Environment environment) {
            foreach (XdmItem basei in xpc.Evaluate("static-base-uri", env)) {
                string uri = ((XdmNode) basei).GetAttributeValue(new QName("uri"));
                if (uri is null or "#UNDEFINED") {
                    driver.println("**** Error: Test cannot be run: static-base-uri is null or '#UNDEFINED'");
                }
                else {
                    try {
                        environment.xpathCompiler.BaseUri = new Uri(uri, UriKind.RelativeOrAbsolute);
                        environment.xqueryCompiler.BaseUri = new Uri(uri, UriKind.RelativeOrAbsolute);
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
                        // JConfiguration config = xpc.Processor.Implementation;
                        // JStringCollator collator = config.getCollationURIResolver()
                        //     .resolve("http://saxon.sf.net/collation?ignore-case=yes", config);

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
                    environment.xqueryCompiler.DefaultCollationName = uri;
                    if (environment.xsltCompiler != null) {
                        environment.xsltCompiler.DefaultCollationName = uri;
                    }
                }
            }
        }

        private static void DeclareNamespaces(XPathCompiler xpc, XdmItem env, Environment environment) {
            foreach (XdmItem nsElement in xpc.Evaluate("namespace", env)) {
                string prefix = ((XdmNode) nsElement).GetAttributeValue(new QName("prefix"));
                string uri = ((XdmNode) nsElement).GetAttributeValue(new QName("uri"));
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

        private static bool LoadSchemaDocuments(Driver driver, XPathCompiler xpc, XdmNode env,
                Environment environment, Saxon.Api.SchemaManager manager) {
            bool validateSources = false;
            Configuration config = environment.processor.Implementation;
            XmlUrlResolver res = new TracingXmlResolver(env.BaseUri);
            foreach (XdmNode schema in env.Children("schema")) {
                string role = schema.GetAttributeValue(new QName("role"));
                string xsdVersion = schema.GetAttributeValue(new QName("xsd-version"));
                if (manager == null || !driver.schemaAware) {
                    driver.println("** Processor is not schema aware **");
                    environment.usable = false;
                    return false;
                }

                if (xsdVersion != null) {
                    manager.XsdVersion = xsdVersion;
                }

                if (!"secondary".Equals(role)) {
                    string href = ((XdmNode) schema).GetAttributeValue(new QName("file"));
                    string ns = schema.GetAttributeValue("uri");
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

        private static void DeclareExternalVariables(Driver driver, XPathCompiler xpc, XdmItem env,
            Environment environment) {
            foreach (XdmItem param in xpc.Evaluate("param", env)) {
                string varName = ((XdmNode) param).GetAttributeValue(new QName("name"));
                XdmValue value;
                string source = ((XdmNode) param).GetAttributeValue(new QName("source"));
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
                    string select = ((XdmNode) param).GetAttributeValue(new QName("select"));
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
                string declared = ((XdmNode) param).GetAttributeValue(new QName("declared"));
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

        private static void RegisterDecimalFormats(Driver driver, XPathCompiler xpc, XdmItem env,
            Environment environment) {
            foreach (XdmItem decimalFormat in xpc.Evaluate("decimal-format", env)) {
                //DecimalFormatManager dfm = environment.xpathCompiler..getUnderlyingStaticContext().getDecimalFormatManager();
                if (SpecAttr.ShortNameOf(driver.RequestedSpec) == "XP") {
                    environment.usable = false;
                    return;
                } 
                XdmNode formatElement = (XdmNode) decimalFormat;
                string formatName = formatElement.GetAttributeValue(new QName("name"));
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
                    string property = formatAttribute.NodeName.LocalName;
                    string value = formatAttribute.StringValue;
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

        private static void CreateUnparsedTextResolver(Driver driver, XPathCompiler xpc, XdmItem env,
                Environment environment) {
            SysCollGen.Dictionary<Uri, Func<TextReader>> resources = new SysCollGen.Dictionary<Uri, Func<TextReader>>();
            SysCollGen.Dictionary<Uri, Encoding> encodings = new SysCollGen.Dictionary<Uri, Encoding>();
            XmlUrlResolver res = new XmlUrlResolver();
            
            // For each resource in the environment, create a dictionary entry that maps
            // the URI of the resource to a factory function that delivers a TextReader
            // for that resource
            
            foreach (XdmItem resource in xpc.Evaluate("resource", env)) {
                string nominalUriStr = ((XdmNode) resource).GetAttributeValue(new QName("uri"));
                Uri nominalUri = new Uri(nominalUriStr, UriKind.RelativeOrAbsolute);
                if (!nominalUri.IsAbsoluteUri) {
                    nominalUri = new Uri(((XdmNode) env).BaseUri, nominalUri);
                }
                string href = ((XdmNode) resource).GetAttributeValue(new QName("file"));
                string encoding = ((XdmNode) resource).GetAttributeValue(new QName("encoding"));
                if (encoding == null) {
                    encoding = "utf-8"; // TODO: should infer from content etc?
                }
                
                Encoding enc1 = Encoding.GetEncoding(encoding, 
                    EncoderFallback.ExceptionFallback, 
                    new DecoderExceptionFallback());
                string mediaType = ((XdmNode) resource).GetAttributeValue(new QName("media-type"));
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
                                return new StringReader(str);
                            }
                            catch (System.Text.DecoderFallbackException e) {
                                throw new JXpathException(e.Message, "FOUT1190");
                            }
                        });
                        
                    }
                    

                    try {
                        //encodings.Add(nominalUri, encoding);
                        

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
                    new TestUnparsedTextURIResolver(encodings, resources, environment.processor.Implementation).resolve;
            }
        }

        public class TestUnparsedTextURIResolver {
            SysCollGen.Dictionary<Uri, Encoding> encodings;
            SysCollGen.Dictionary<Uri, Func<TextReader>> resources;
            private Configuration config;

            public TestUnparsedTextURIResolver(SysCollGen.Dictionary<Uri, Encoding> encodings,
                SysCollGen.Dictionary<Uri, Func<TextReader>> resources,
                Configuration config) {
                this.encodings = encodings;
                this.resources = resources;
                this.config = config;
            }


            public TextReader resolve(Uri absoluteURI, Encoding encoding) {
                if (resources.ContainsKey(absoluteURI)) {
                    if (encoding == null) {
                        encodings.TryGetValue(new Uri(absoluteURI.ToString()), out encoding);
                    }

                    if (encoding == null) {
                        encoding = Encoding.UTF8;
                    }

                    try {
                        return resources[absoluteURI].Invoke();
                    }
                    catch (Exception ioe) {
                        Saxon.Hej.trans.XPathException xpe =
                            new Saxon.Hej.trans.XPathException("Unknown URI " + absoluteURI, ioe);
                        xpe.setErrorCode("FOUT1170");
                        throw xpe;
                    }
                }
                else {
                    if (encoding == null) {
                        encoding = Encoding.UTF8;
                    }
                    return new StandardUnparsedTextResolver(config).resolve(absoluteURI, encoding.WebName, config);
                }

            }
        }

        private static void CreateCollectionFinder(Driver driver, XPathCompiler xpc, XdmNode env,
            Environment environment, DocumentBuilder builder) {
            
            Configuration config = environment.processor.Implementation;
            XmlUrlResolver res = new XmlUrlResolver();
        
            if (!environment.usable) {
                return;
            }
        
            foreach (XdmNode coll in env.Children("collection")) {
                var resourceList = new SysCollGen.List<Saxon.Api.IResource>();
                string collectionURI = coll.GetAttributeValue(new QName("uri"));
                if (collectionURI == null) {
                    collectionURI = "";
                }
        
                if (collectionURI.Equals("")) {
                    collectionURI = "http://www.w3.org/qt3-test-suite/default.collection.uri";
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
        
                environment.collections.Add(u, resourceList);
        
                foreach (XdmNode source in coll.Children("source")) {
                    string href = source.GetAttributeValue(new QName("file"));
                    string frag = null;
                    int hash = href.IndexOf('#');
                    if (hash > 0) {
                        frag = href.Substring(hash + 1);
                        href = href.Substring(0, hash);
                    }
        
                    FileStream file = new FileStream(res.ResolveUri(env.BaseUri, href).AbsolutePath,
                        FileMode.Open, FileAccess.Read);

                    Uri resourceUri = res.ResolveUri(env.BaseUri, href);
                    string uriStr = resourceUri.AbsoluteUri;
                    builder.BaseUri = resourceUri;
                    XdmNode doc = builder.Build(file);
                    
                    if (frag != null) {
                        XdmNode selected =
                            (XdmNode) environment.xpathCompiler.EvaluateSingle("id('" + frag + "')", doc);
                        if (selected == null) {
                            driver.println("**** Fragment not found: " + frag);
                            break;
                        }
        
                        resourceList.Add(new XdmNodeResource(resourceUri, selected));
                    }
                    else {
                        resourceList.Add(new XdmNodeResource(resourceUri, doc));
                    }

                    if (!environment.sourceDocs.ContainsKey(uriStr)) {
                        environment.sourceDocs.Add(uriStr, doc);
                    }
                }
                
                foreach (XdmNode query in coll.Children("query")) {
                    string queryExpr = query.StringValue;
                    XQueryEvaluator evaluator = environment.xqueryCompiler.Compile(queryExpr).Load();
                    XdmValue result = evaluator.Evaluate();

                    int counter = 0;
                    foreach (XdmItem itemi in result) {
                        resourceList.Add(new AtomicResource(u.ToString() + "/" + (counter++), itemi));
                    }
                }

            }
            //environment.processor.Implementation.setDefaultCollection(null);
            environment.processor.CollectionFinder = uri => environment.collections[uri];
            
        }

        // Misleading name, it can also be a map or array
        private class AtomicResource : IResource {
            private readonly XdmItem value;

            public AtomicResource(string uri, XdmItem value) {
                ResourceUri = new Uri(uri);
                this.value = value;
            }

            public Uri ResourceUri { get; }

            public XdmItem GetXdmItem(DynamicContext context) {
                return value;
            }

            public string ContentType => "application/xdm-atomic-value";


        }


        // Environment implements XmlResolver (though it relies on the caller accepting a Source object
        // as the return value, which is somewhat unorthodox...)
        
        public override object GetEntity(Uri href, string role, Type ofObjectToReturn) {
            XdmNode node = null;
            string hrefStr = href.ToString();
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
                    string hrefStri = (href.IsFile ? href.LocalPath : hrefStr);
                    Stream stream = new FileStream(hrefStri, FileMode.Open);
                    resetActions.Add(env => stream.Close());
                    Saxon.Ejavax.xml.transform.Source sourcei = new Saxon.Ejavax.xml.transform.stream.StreamSource(stream);


                    if (!val.Equals("skip")) {
                        sourcei = Saxon.Hej.lib.AugmentedSource.makeAugmentedSource(sourcei);
                        ((Saxon.Hej.lib.AugmentedSource) sourcei).setSchemaValidationMode(
                            Saxon.Hej.lib.Validation.getCode(val));
                    }

                    sourcei.setSystemId(hrefStr);
                    return sourcei;
                }
                else {
                    // Some tests, notably catalog tests, don't declare the documents they use
                    return new XmlUrlResolver().GetEntity(href, role, ofObjectToReturn);
                }
            }
            else {
                return node.Implementation.asActiveSource();
            }
        }

        public override Uri ResolveUri(Uri basei, string href) {
            return base.ResolveUri(basei, href);
        }

        private static void LoadSourceDocuments(Driver driver, XPathCompiler xpc, XdmItem env,
            Environment environment, DocumentBuilder builder, SchemaManager manager, bool validateSources) {
            XmlUrlResolver res = new XmlUrlResolver();
            
            foreach (XdmItem source in xpc.Evaluate("source", env)) {
                XdmNode doc = null;

                string uri = ((XdmNode)source).GetAttributeValue(new QName("uri"));
                string mime = ((XdmNode)source).GetAttributeValue(new QName("media-type"));

                if (mime != null && "application/xml".Equals(mime)) {
                    continue;
                }
                bool mutable = isTrue("mutable")(source);
                if (mutable) {
                    builder.TreeModel = TreeModel.LinkedTree;
                }
                string validation = ((XdmNode)source).GetAttributeValue(new QName("", "validation"));
                if (validation == null) {
                    validation = "skip";
                }

                string streaming = ((XdmNode)source).GetAttributeValue(new QName("", "streaming"));
                if (!validateSources && validation.Equals("skip")) {
                    builder.SchemaValidationMode = SchemaValidationMode.None;
                    builder.SchemaValidator = null;
                }
                else {
                    SchemaValidator validator = manager.NewSchemaValidator();
                    if ("lax".Equals(validation)) {
                        validator.Lax = true;
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

                string role = ((XdmNode)source).GetAttributeValue(new QName("role"));
                string href = ((XdmNode)source).GetAttributeValue(new QName("file"));

                Console.Error.WriteLine("Load source document " + href);

                if ("true".Equals(streaming)) {
                    if (".".Equals(role)) {
                        if (href == null) {
                            environment.streamedContent = xpc.Evaluate("string(content)", source).ToString();
                        }
                        else {
                            environment.streamedPath = res.ResolveUri(((XdmNode)env).BaseUri, href).AbsolutePath;
                        }

                        environment.streamedInputValidation = validation;
                    }
                    else {
                        try {
                            System.Console.WriteLine(res.ResolveUri(((XdmNode)env).BaseUri, href).AbsolutePath);
                            System.Console.WriteLine(res.ResolveUri(((XdmNode)env).BaseUri, href).AbsoluteUri);
                            System.Console.WriteLine(res.ResolveUri(((XdmNode)env).BaseUri, href).ToString());
                            environment.streamedSecondaryDocs.Add(
                                res.ResolveUri(((XdmNode)env).BaseUri, href).AbsoluteUri, validation);
                        }
                        catch (Exception) {
                        }
                    }
                }
                else {
                    if (href != null) {
                        try {
                            Uri fileLoc = res.ResolveUri(((XdmNode)env).BaseUri, href);
                            FileStream file = null;
                            if (fileLoc.Scheme.Equals("file")) {
                                file = new FileStream(res.ResolveUri(((XdmNode)env).BaseUri, href).AbsolutePath,
                                    FileMode.Open, FileAccess.Read);
                                if (uri == null) {
                                    uri = fileLoc.AbsoluteUri;
                                }
                            }

                            Stream ss = (file == null
                                ? new FileStream(fileLoc.ToString(), FileMode.Open, FileAccess.Read)
                                : file);

                            Uri absUri = builder.BaseUri =
                                new Uri(res.ResolveUri(((XdmNode)env).BaseUri, uri).AbsoluteUri);
                            try {
                                doc = builder.Build(ss);
                                environment.sourceDocs.Add(absUri.OriginalString, doc);
                            }
                            catch (Exception e) {
                                Console.Error.WriteLine(e);
                                driver.println("*** failed to build source document " + href + ", Exception:" +
                                               e.Message);
                            }
                        }
                        catch (Exception e) {
                            driver.println("*** failed to find source document " + href + ", Exception:" + e.Message);
                            continue;
                        }
                    }
                    else {
                        // content is inline in the catalog
                        if (uri == null) {
                            uri = ((XdmNode)env).BaseUri.AbsolutePath;
                        }

                        string content = xpc.Evaluate("string(content)", source).ToString();
                        Uri absUri = builder.BaseUri = new Uri(res.ResolveUri(((XdmNode)env).BaseUri, uri).AbsoluteUri);
                        try {
                            doc = builder.Build(new StringReader(content));
                            environment.sourceDocs.Add(absUri.OriginalString, doc);
                        }
                        catch (Exception e) {
                            driver.println("*** failed to parse inline source document, Exception:" + e.Message);
                            continue;
                        }

                    }

                    XdmItem selectedItem = doc;
                    string select = ((XdmNode)source).GetAttributeValue(new QName("select"));
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
                            string varName = role.Substring(1);
                            environment.params1.Add(new QName(varName), selectedItem);
                            environment.xpathCompiler.DeclareVariable(new QName(varName));
                            if (!isTrue("declared")(source)) {
                                environment.paramDeclarations.Append("declare variable $" + varName + " external; ");
                            }
                        }
                    }

                    string definesStylesheet = ((XdmNode)source).GetAttributeValue(new QName("defines-stylesheet"));
                    if (definesStylesheet != null) {
                        definesStylesheet = definesStylesheet.Trim();
                    }

                    if ("true".Equals(definesStylesheet) || "1".Equals(definesStylesheet)) {
                        // try using an embedded stylesheet from the source document
                        try {
                            environment.xsltExecutable =
                                environment.xsltCompiler.CompileAssociatedStylesheet(
                                    ((XdmNode)environment.contextItem));
                        }
                        catch (Exception e) {
                            driver.println("*** failed to compile stylesheet referenced in source document " + href);
                            Console.Error.Write(e.StackTrace);
                        }
                    }
                }
            }
        }





        public delegate void ResetAction(Environment env);

        public class XdmNodeResource : IResource {
            private readonly XdmNode node;

            public XdmNodeResource(Uri uri, XdmNode node) {
                ResourceUri = uri;
                this.node = node;
            }

            public Uri ResourceUri { get; }

            public XdmItem GetXdmItem(DynamicContext context) {
                return node;
            }

            public string ContentType => "application/xml";
        }
        
        /**
             * Construct a predicate that tests whether a specified attribute of the supplied
             * element has the value "true", "yes", or "1" after whitespace trimming
             * @param attName the name of the attribute
             * @return a function which, when applied to an XDM item, returns true if the item is
             * an element that has an attribute with the given name, and whose whitespace-normalized
             * value is "true", "yes", or "1".
             */
        public static Predicate<XdmItem> isTrue(string attName) {
            return element => {
                if (element is XdmNode) {
                    string attVal = ((XdmNode)element).GetAttributeValue(attName);
                    if (attVal != null) {
                        attVal = attVal.Trim();
                        return "yes".Equals(attVal) || "1".Equals(attVal) || "true".Equals(attVal);
                    }
                }

                return false;
            };
        }
    }
}