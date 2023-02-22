using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    internal class CommandLineOptions {
        public const int TYPE_BOOLEAN = 1;
        public const int TYPE_FILENAME = 2;
        public const int TYPE_CLASSNAME = 3;
        public const int TYPE_ENUMERATION = 4;
        public const int TYPE_INTEGER = 5;
        public const int TYPE_QNAME = 6;
        public const int TYPE_FILENAME_LIST = 7;
        public const int TYPE_DATETIME = 8;
        public const int TYPE_STRING = 9;
        public const int TYPE_INTEGER_PAIR = 10;
        public const int VALUE_REQUIRED = (1<<8);
        public const int VALUE_PROHIBITED = (2<<8);
        private readonly System.Collections.Generic.Dictionary<string,int> recognizedOptions = new System.Collections.Generic.Dictionary<string, int>();
        private readonly System.Collections.Generic.Dictionary<string,string> optionHelp = new System.Collections.Generic.Dictionary<string, string>();
        private readonly Saxon.Ejava.util.Properties namedOptions = new Saxon.Ejava.util.Properties();
        private readonly Saxon.Ejava.util.Properties configOptions = new Saxon.Ejava.util.Properties();
        private readonly System.Collections.Generic.IDictionary<string,System.Collections.Generic.ISet<string>> permittedValues = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.ISet<string>>();
        private readonly System.Collections.Generic.IDictionary<string,string> defaultValues = new System.Collections.Generic.Dictionary<string, string>();
        private readonly System.Collections.Generic.IList<string> positionalOptions = new System.Collections.Generic.List<string>();
        private readonly Saxon.Ejava.util.Properties paramValues = new Saxon.Ejava.util.Properties();
        private readonly Saxon.Ejava.util.Properties paramExpressions = new Saxon.Ejava.util.Properties();
        private readonly Saxon.Ejava.util.Properties paramFiles = new Saxon.Ejava.util.Properties();
        private readonly Saxon.Ejava.util.Properties serializationParams = new Saxon.Ejava.util.Properties();
        public void addRecognizedOption(string option, int optionProperties, string helpText) {
            Saxon.Impl.Helpers.MapUtils.Put(recognizedOptions, option, optionProperties);
            Saxon.Impl.Helpers.MapUtils.Put(optionHelp, option, helpText);
            if ((((optionProperties&unchecked((int)0xff))) == TYPE_BOOLEAN)) {
                setPermittedValues(option, new string[]{"on", "off"}, "on");
            }
        }
        public void setPermittedValues(string option, string[] values, string defaultValue) {
            System.Collections.Generic.ISet<string> valueSet = new System.Collections.Generic.HashSet<string>(Saxon.Ejava.util.Arrays.asList(values));
            Saxon.Impl.Helpers.MapUtils.Put(permittedValues, option, valueSet);
            if ((defaultValue != null)) {
                Saxon.Impl.Helpers.MapUtils.Put(defaultValues, option, defaultValue);
            }
        }
        private static string displayPermittedValues(System.Collections.Generic.ISet<string> permittedValues) {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(20);
            foreach (string val in permittedValues) {
                if ("".Equals(val)) {
                    sb.append("\"\"");
                } else {
                    sb.append(val);
                }
                sb.append('|');
            }
            sb.setLength((sb.length()-1));
            return sb.ToString();
        }
        public void setActualOptions(string[] args) {
            foreach (string arg in args) {
                if ("-".Equals(arg)) {
                    positionalOptions.Add(arg);
                } else {
                    if (arg.Equals("--?")) {
                        System.Console.Error.WriteLine(("Configuration features:"+featureKeys()));
                    } else {
                        if ((arg[0] == '-')) {
                            string option;
                            string value = "";
                            if (((arg.Length>5) && (arg[1] == '-'))) {
                                int colon = arg.IndexOf(':');
                                if (((colon>0) && (colon<(arg.Length-1)))) {
                                    option = arg.Substring(2, colon - 2);
                                    value = arg.Substring((colon+1));
                                    configOptions.setProperty(option, value);
                                } else {
                                    if (((colon>0) && (colon == (arg.Length-1)))) {
                                        option = arg.Substring(2, colon - 2);
                                        configOptions.setProperty(option, "");
                                    } else {
                                        option = arg.Substring(2);
                                        configOptions.setProperty(option, "true");
                                    }
                                }
                            } else {
                                int colon = arg.IndexOf(':');
                                if (((colon>0) && (colon<(arg.Length-1)))) {
                                    option = arg.Substring(1, colon - 1);
                                    value = arg.Substring((colon+1));
                                } else {
                                    option = arg.Substring(1);
                                }
                                if ((Saxon.Impl.Helpers.MapUtils.GetOrDefault(recognizedOptions, option, -1) == -1)) {
                                    throw new Saxon.Hej.trans.XPathException(((("Command line option -"+option)+" is not recognized. Options available: ")+displayPermittedOptions()));
                                }
                                if ((namedOptions.getProperty(option) != null)) {
                                    throw new Saxon.Hej.trans.XPathException((("Command line option -"+option)+" appears more than once"));
                                } else {
                                    if ("?".Equals(value)) {
                                        displayOptionHelp(option);
                                        throw new Saxon.Hej.trans.XPathException("No processing requested");
                                    } else {
                                        if ("".Equals(value)) {
                                            int prop = Saxon.Impl.Helpers.MapUtils.Get(recognizedOptions, option);
                                            if ((((prop&VALUE_REQUIRED)) != 0)) {
                                                string msg = (("Command line option -"+option)+" requires a value");
                                                if ((Saxon.Impl.Helpers.MapUtils.Get(permittedValues, option) != null)) {
                                                    msg += (": permitted values are "+displayPermittedValues(Saxon.Impl.Helpers.MapUtils.Get(permittedValues, option)));
                                                }
                                                throw new Saxon.Hej.trans.XPathException(msg);
                                            }
                                            string defaultValue = Saxon.Impl.Helpers.MapUtils.Get(defaultValues, option);
                                            if ((defaultValue != null)) {
                                                value = defaultValue;
                                            }
                                        } else {
                                            int prop = Saxon.Impl.Helpers.MapUtils.Get(recognizedOptions, option);
                                            if ((((prop&VALUE_PROHIBITED)) != 0)) {
                                                string msg = (("Command line option -"+option)+" does not expect a value");
                                                throw new Saxon.Hej.trans.XPathException(msg);
                                            }
                                        }
                                        System.Collections.Generic.ISet<string> permitted = Saxon.Impl.Helpers.MapUtils.Get(permittedValues, option);
                                        if (((permitted != null) && !(permitted.Contains(value)))) {
                                            throw new Saxon.Hej.trans.XPathException(((("Bad option value "+arg)+": permitted values are ")+displayPermittedValues(permitted)));
                                        }
                                        namedOptions.setProperty(option, value);
                                    }
                                }
                            }
                        } else {
                            int eq = arg.IndexOf('=');
                            if ((eq>=1)) {
                                string keyword = arg.Substring(0, eq - 0);
                                string value = "";
                                if ((eq<(arg.Length-1))) {
                                    value = arg.Substring((eq+1));
                                }
                                char ch = arg[0];
                                if (((ch == '!') && (eq>=2))) {
                                    serializationParams.setProperty(keyword.Substring(1), value);
                                } else {
                                    if (((ch == '?') && (eq>=2))) {
                                        paramExpressions.setProperty(keyword.Substring(1), value);
                                    } else {
                                        if (((ch == '+') && (eq>=2))) {
                                            paramFiles.setProperty(keyword.Substring(1), value);
                                        } else {
                                            paramValues.setProperty(keyword, value);
                                        }
                                    }
                                }
                            } else {
                                positionalOptions.Add(arg);
                            }
                        }
                    }
                }
            }
        }
        public bool definesParameterValues() {
            return (((!(serializationParams.isEmpty()) || !(paramExpressions.isEmpty())) || !(paramFiles.isEmpty())) || !(paramValues.isEmpty()));
        }
        public bool testIfSchemaAware() {
            return ((((((getOptionValue("sa") != null) || (getOptionValue("outval") != null)) || (getOptionValue("val") != null)) || (getOptionValue("vlax") != null)) || (getOptionValue("xsd") != null)) || (getOptionValue("xsdversion") != null));
        }
        public void applyToConfiguration(Saxon.Hej.s9api.Processor processor) {
            Saxon.Hej.Configuration config = processor.getUnderlyingConfiguration();
            foreach (string name in configOptions.stringPropertyNames()) {
                string value = configOptions.getProperty(name);
                string fullName = ("http://saxon.sf.net/feature/"+name);
                if ((!(name.StartsWith("parserFeature?")) && !(name.StartsWith("parserProperty?")))) {
                    if (Saxon.Hej.lib.FeatureIndex.exists(fullName)) {
                        Saxon.Hej.lib.FeatureData f = Saxon.Hej.lib.FeatureIndex.getData(fullName);
                        if ((f == null)) {
                            throw new Saxon.Hej.trans.XPathException(("Unknown configuration feature "+name));
                        }
                        if ((f.type == typeof(bool))) {
                            Saxon.Hej.Configuration.requireBoolean(name, value);
                        } else {
                            if ((f.type == typeof(int))) {
                                System.Int32.Parse(value);
                            } else {
                                if ((f.type != typeof(string))) {
                                    throw new Saxon.Hej.trans.XPathException((("Property --"+name)+" cannot be supplied as a string"));
                                }
                            }
                        }
                    } else {
                        throw new Saxon.Hej.trans.XPathException(("Unknown configuration property --"+name));
                    }
                }
                try {
                    processor.getUnderlyingConfiguration().setConfigurationProperty(fullName, value);
                } catch (Saxon.Ejava.lang.IllegalArgumentException err) {
                    throw new Saxon.Hej.trans.XPathException(((("Incorrect value for --"+name)+": ")+err.Message));
                }
            }
            string optionValue = getOptionValue("catalog");
            if ((optionValue != null)) {
                System.Collections.Generic.List<string> catalogs = new System.Collections.Generic.List<string>();
                if ((((getOptionValue("u") != null)) || isImplicitURI(optionValue))) {
                    Saxon.Hej.lib.JJ_ResourceRequest request = new Saxon.Hej.lib.JJ_ResourceRequest();
                    request.nature = "urn:oasis:names:tc:entity:xmlns:xml:catalog";
                    request.purpose = "catalog";
                    foreach (string s in System.Text.RegularExpressions.Regex.Split(optionValue, ";")) {
                        Saxon.Ejavax.xml.transform.Source sourceInput;
                        try {
                            request.uri = s;
                            sourceInput = new Saxon.Callbacks.DirectResourceResolver(config).resolve(request);
                        } catch (Saxon.Hej.trans.XPathException e) {
                            throw new Saxon.Hej.trans.XPathException(("Catalog file not found: "+s), e);
                        }
                        catalogs.Add(sourceInput.getSystemId());
                    }
                } else {
                    foreach (string s in System.Text.RegularExpressions.Regex.Split(optionValue, ";")) {
                        Saxon.Ejava.io.File catalogFile = new Saxon.Ejava.io.File(s);
                        if (!(catalogFile.exists())) {
                            throw new Saxon.Hej.trans.XPathException(("Catalog file not found: "+s));
                        }
                        catalogs.Add(catalogFile.toURI().OriginalString);
                    }
                }
                setCatalogFiles(config, catalogs);
            }
            optionValue = getOptionValue("dtd");
            if ((optionValue != null)) {
                switch (optionValue) {
                  case "on":
                    config.setBooleanProperty(Saxon.Hej.lib.Feature<bool>.DTD_VALIDATION, true);
                    config.getParseOptions().setDTDValidationMode(Saxon.Hej.lib.Validation.STRICT);
                    break ;
                  case "off":
                    config.setBooleanProperty(Saxon.Hej.lib.Feature<bool>.DTD_VALIDATION, false);
                    config.getParseOptions().setDTDValidationMode(Saxon.Hej.lib.Validation.SKIP);
                    break ;
                  case "recover":
                    config.setBooleanProperty(Saxon.Hej.lib.Feature<bool>.DTD_VALIDATION, true);
                    config.setBooleanProperty(Saxon.Hej.lib.Feature<bool>.DTD_VALIDATION_RECOVERABLE, true);
                    config.getParseOptions().setDTDValidationMode(Saxon.Hej.lib.Validation.LAX);
                    break ;
                }
            }
            optionValue = getOptionValue("ea");
            if ((optionValue != null)) {
                bool on = Saxon.Hej.Configuration.requireBoolean("ea", optionValue);
                config.getDefaultXsltCompilerInfo().setAssertionsEnabled(on);
            }
            optionValue = getOptionValue("expand");
            if ((optionValue != null)) {
                bool on = Saxon.Hej.Configuration.requireBoolean("expand", optionValue);
                config.getParseOptions().setExpandAttributeDefaults(on);
            }
            optionValue = getOptionValue("ext");
            if ((optionValue != null)) {
                bool on = Saxon.Hej.Configuration.requireBoolean("ext", optionValue);
                config.setBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_EXTERNAL_FUNCTIONS, on);
            }
            optionValue = getOptionValue("l");
            if ((optionValue != null)) {
                bool on = Saxon.Hej.Configuration.requireBoolean("l", optionValue);
                config.setBooleanProperty(Saxon.Hej.lib.Feature<bool>.LINE_NUMBERING, on);
            }
            optionValue = getOptionValue("m");
            if ((optionValue != null)) {
                config.setConfigurationProperty(Saxon.Hej.lib.Feature<string>.MESSAGE_EMITTER_CLASS, optionValue);
            }
            optionValue = getOptionValue("opt");
            if ((optionValue != null)) {
                config.setConfigurationProperty(Saxon.Hej.lib.Feature<System.Object>.OPTIMIZATION_LEVEL, optionValue);
            }
            optionValue = getOptionValue("outval");
            if ((optionValue != null)) {
                bool isRecover = "recover".Equals(optionValue);
                config.setConfigurationProperty(Saxon.Hej.lib.Feature<bool>.VALIDATION_WARNINGS, isRecover);
                config.setConfigurationProperty(Saxon.Hej.lib.Feature<bool>.VALIDATION_COMMENTS, isRecover);
            }
            optionValue = getOptionValue("r");
            if ((optionValue != null)) {
                config.setResourceResolver(config.makeResourceResolver(optionValue));
            }
            optionValue = getOptionValue("strip");
            if ((optionValue != null)) {
                config.setConfigurationProperty(Saxon.Hej.lib.Feature<string>.STRIP_WHITESPACE, optionValue);
            }
            optionValue = getOptionValue("T");
            if ((optionValue != null)) {
                config.setCompileWithTracing(true);
            }
            optionValue = getOptionValue("TJ");
            if ((optionValue != null)) {
                bool on = Saxon.Hej.Configuration.requireBoolean("TJ", optionValue);
                config.setBooleanProperty(Saxon.Hej.lib.Feature<bool>.TRACE_EXTERNAL_FUNCTIONS, on);
            }
            optionValue = getOptionValue("tree");
            if ((optionValue != null)) {
                switch (optionValue) {
                  case "linked":
                    config.setTreeModel(Saxon.Hej.@event.Builder.LINKED_TREE);
                    break ;
                  case "tiny":
                    config.setTreeModel(Saxon.Hej.@event.Builder.TINY_TREE);
                    break ;
                  case "tinyc":
                    config.setTreeModel(Saxon.Hej.@event.Builder.TINY_TREE_CONDENSED);
                    break ;
                }
            }
            optionValue = getOptionValue("val");
            if ((optionValue != null)) {
                if ("strict".Equals(optionValue)) {
                    processor.setConfigurationProperty(Saxon.Hej.lib.Feature<int>.SCHEMA_VALIDATION, Saxon.Hej.lib.Validation.STRICT);
                } else {
                    if ("lax".Equals(optionValue)) {
                        processor.setConfigurationProperty(Saxon.Hej.lib.Feature<int>.SCHEMA_VALIDATION, Saxon.Hej.lib.Validation.LAX);
                    }
                }
            }
            optionValue = getOptionValue("x");
            if ((optionValue != null)) {
                processor.setConfigurationProperty(Saxon.Hej.lib.Feature<string>.SOURCE_PARSER_CLASS, optionValue);
            }
            optionValue = getOptionValue("xi");
            if ((optionValue != null)) {
                bool on = Saxon.Hej.Configuration.requireBoolean("xi", optionValue);
                processor.setConfigurationProperty(Saxon.Hej.lib.Feature<bool>.XINCLUDE, on);
            }
            optionValue = getOptionValue("xmlversion");
            if ((optionValue != null)) {
                processor.setConfigurationProperty(Saxon.Hej.lib.Feature<string>.XML_VERSION, optionValue);
            }
            optionValue = getOptionValue("xsdversion");
            if ((optionValue != null)) {
                processor.setConfigurationProperty(Saxon.Hej.lib.Feature<string>.XSD_VERSION, optionValue);
            }
            optionValue = getOptionValue("xsiloc");
            if ((optionValue != null)) {
                bool on = Saxon.Hej.Configuration.requireBoolean("xsiloc", optionValue);
                processor.setConfigurationProperty(Saxon.Hej.lib.Feature<bool>.USE_XSI_SCHEMA_LOCATION, on);
            }
            optionValue = getOptionValue("y");
            if ((optionValue != null)) {
                processor.setConfigurationProperty(Saxon.Hej.lib.Feature<string>.STYLE_PARSER_CLASS, optionValue);
            }
            optionValue = getOptionValue("init");
            if ((optionValue != null)) {
                Saxon.Hej.lib.Initializer initializer = (Saxon.Hej.lib.Initializer)(config.getInstance(optionValue));
                initializer.initialize(config);
            }
        }
        private void setCatalogFiles(Saxon.Hej.Configuration config, System.Collections.Generic.IList<string> catalogs) {
            Saxon.Hej.lib.JJ_ResourceResolver rr = config.getResourceResolver();
            if ((rr is Saxon.Callbacks.ConfigurableResourceResolver)) {
                setCatalogFiles(((Saxon.Callbacks.ConfigurableResourceResolver)(rr)), catalogs);
            } else {
                throw new Saxon.Ejava.lang.IllegalStateException("The resolver in the Configuration is not a ConfigurableResourceResolver");
            }
        }
        public static void setCatalogFiles(Saxon.Callbacks.ConfigurableResourceResolver crr, System.Collections.Generic.IList<string> catalogs) {
            crr.setFeature(Org.XmlResolver.Features.ResolverFeature.CATALOG_FILES, catalogs);
        }
        public string displayPermittedOptions() {
            string[] options = new string[recognizedOptions.Count];
            System.Collections.Generic.List<string> keys = new System.Collections.Generic.List<string>(Saxon.Impl.Helpers.MapUtils.KeySet(recognizedOptions));
            options = keys.ToArray();
            Saxon.Ejava.util.Arrays.sort(options, Saxon.Ejava.text.Collator.getInstance());
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(100);
            foreach (string opt in options) {
                sb.append(" -");
                sb.append(opt);
            }
            sb.append(" --?");
            return sb.ToString();
        }
        private void displayOptionHelp(string option) {
            System.Console.Error.WriteLine((("Help for -"+option)+" option"));
            int prop = Saxon.Impl.Helpers.MapUtils.Get(recognizedOptions, option);
            if ((((prop&VALUE_PROHIBITED)) == 0)) {
                switch ((prop&unchecked((int)0xff))) {
                  case TYPE_BOOLEAN:
                    System.Console.Error.WriteLine("Value: on|off");
                    break ;
                  case TYPE_INTEGER:
                    System.Console.Error.WriteLine("Value: integer");
                    break ;
                  case TYPE_FILENAME:
                    System.Console.Error.WriteLine("Value: file name");
                    break ;
                  case TYPE_FILENAME_LIST:
                    System.Console.Error.WriteLine("Value: list of file names, semicolon-separated");
                    break ;
                  case TYPE_CLASSNAME:
                    System.Console.Error.WriteLine("Value: Java fully-qualified class name");
                    break ;
                  case TYPE_QNAME:
                    System.Console.Error.WriteLine("Value: QName in Clark notation ({uri}local)");
                    break ;
                  case TYPE_STRING:
                    System.Console.Error.WriteLine("Value: string");
                    break ;
                  case TYPE_INTEGER_PAIR:
                    System.Console.Error.WriteLine("Value: int,int");
                    break ;
                  case TYPE_ENUMERATION:
                    string message = "Value: one of ";
                    message += displayPermittedValues(Saxon.Impl.Helpers.MapUtils.Get(permittedValues, option));
                    System.Console.Error.WriteLine(message);
                    break ;
                  default:
                    break ;
                }
            }
            System.Console.Error.WriteLine(("Meaning: "+Saxon.Impl.Helpers.MapUtils.Get(optionHelp, option)));
        }
        public string getOptionValue(string option) {
            return namedOptions.getProperty(option);
        }
        public System.Collections.Generic.IList<string> getPositionalOptions() {
            return positionalOptions;
        }
        public void setParams(Saxon.Hej.s9api.Processor processor, Saxon.Hej.trans.CommandLineOptions.ParamSetter paramSetter) {
            foreach (string name in paramValues.stringPropertyNames()) {
                string value = paramValues.getProperty(name);
                /*Delegate.setParam()*/paramSetter(Saxon.Api.QName.FromClarkName(name), new Saxon.Api.XdmAtomicValue(value, Saxon.Api.XdmAtomicType.UntypedAtomic));
            }
            applyFileParameters(processor, paramSetter);
            foreach (string name in paramExpressions.stringPropertyNames()) {
                string value = paramExpressions.getProperty(name);
                Saxon.Hej.s9api.XPathCompiler xpc = processor.newXPathCompiler();
                Saxon.Hej.s9api.XPathExecutable xpe = xpc.compile(value);
                Saxon.Api.XdmValue val = xpe.load().evaluate();
                /*Delegate.setParam()*/paramSetter(Saxon.Api.QName.FromClarkName(name), val);
            }
        }
        private void applyFileParameters(Saxon.Hej.s9api.Processor processor, Saxon.Hej.trans.CommandLineOptions.ParamSetter paramSetter) {
            bool useURLs = "on".Equals(getOptionValue("u"));
            foreach (string name in paramFiles.stringPropertyNames()) {
                string value = paramFiles.getProperty(name);
                System.Collections.Generic.IList<Saxon.Ejavax.xml.transform.Source> sourceList = new System.Collections.Generic.List<Saxon.Ejavax.xml.transform.Source>();
                loadDocuments(value, useURLs, processor, true, sourceList);
                if (!(sourceList.Count==0)) {
                    System.Collections.Generic.IList<Saxon.Api.XdmNode> nodeList = new System.Collections.Generic.List<Saxon.Api.XdmNode>(sourceList.Count);
                    Saxon.Hej.s9api.DocumentBuilder builder = processor.newDocumentBuilder();
                    foreach (Saxon.Ejavax.xml.transform.Source s in sourceList) {
                        nodeList.Add(builder.build(s));
                    }
                    Saxon.Api.XdmValue nodes = new Saxon.Api.XdmValue(nodeList);
                    /*Delegate.setParam()*/paramSetter(Saxon.Api.QName.FromClarkName(name), nodes);
                } else {
                    /*Delegate.setParam()*/paramSetter(Saxon.Api.QName.FromClarkName(name), Saxon.Api.XdmEmptySequence.Instance);
                }
            }
        }
        public void setSerializationProperties(Saxon.Hej.s9api.Serializer serializer) {
            foreach (string name in serializationParams.stringPropertyNames()) {
                string key = name;
                string value = serializationParams.getProperty(key);
                if (key.StartsWith("saxon:")) {
                    key = ((("{"+Saxon.Hej.lib.NamespaceConstant.SAXON)+"}")+key.Substring(6));
                }
                serializer.setOutputProperty(Saxon.Api.QName.FromClarkName(key), value);
            }
        }
        public delegate void ParamSetter (Saxon.Api.QName qName, Saxon.Api.XdmValue value);
        public void applyStaticParams(Saxon.Hej.s9api.XsltCompiler compiler) {
            Saxon.Hej.s9api.Processor processor = compiler.getProcessor();
            foreach (string name in paramValues.stringPropertyNames()) {
                string value = paramValues.getProperty(name);
                compiler.setParameter(Saxon.Api.QName.FromClarkName(name), new Saxon.Api.XdmAtomicValue(value, Saxon.Api.XdmAtomicType.UntypedAtomic));
            }
            foreach (string name in paramExpressions.stringPropertyNames()) {
                string value = paramExpressions.getProperty(name);
                Saxon.Hej.s9api.XPathCompiler xpc = processor.newXPathCompiler();
                Saxon.Hej.s9api.XPathExecutable xpe = xpc.compile(value);
                Saxon.Api.XdmValue val = xpe.load().evaluate();
                compiler.setParameter(Saxon.Api.QName.FromClarkName(name), val);
            }
        }
        public void applyFileParams(Saxon.Hej.s9api.Processor processor, Saxon.Hej.s9api.Xslt30Transformer transformer) {
            if (!(paramFiles.isEmpty())) {
                System.Collections.Generic.IDictionary<Saxon.Api.QName,Saxon.Api.XdmValue> @params = new System.Collections.Generic.Dictionary<Saxon.Api.QName, Saxon.Api.XdmValue>();
                applyFileParameters(processor, (k, v) => Saxon.Impl.Helpers.MapUtils.Put(@params, k, v));
                transformer.setStylesheetParameters(@params);
            }
        }
        public static bool loadDocuments(string sourceFileName, bool useURLs, Saxon.Hej.s9api.Processor processor, bool useSAXSource, System.Collections.Generic.IList<Saxon.Ejavax.xml.transform.Source> sources) {
            Saxon.Ejavax.xml.transform.Source sourceInput;
            Saxon.Hej.Configuration config = processor.getUnderlyingConfiguration();
            if ((useURLs || isImplicitURI(sourceFileName))) {
                try {
                    Saxon.Hej.lib.JJ_ResourceRequest request = new Saxon.Hej.lib.JJ_ResourceRequest();
                    request.uri = sourceFileName;
                    request.nature = Saxon.Hej.lib.JJ_ResourceRequest.XML_NATURE;
                    request.purpose = "source document";
                    sourceInput = request.resolve(config.getResourceResolver(), new Saxon.Callbacks.DirectResourceResolver(config));
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Api.SaxonApiException(e);
                }
                sources.Add(sourceInput);
                return false;
            } else {
                if (sourceFileName.Equals("-")) {
                    sourceInput = new Saxon.Ejavax.xml.transform.stream.StreamSource(System.Console.In);
                    sources.Add(sourceInput);
                    return false;
                } else {
                    Saxon.Ejava.io.File sourceFile = new Saxon.Ejava.io.File(sourceFileName);
                    if (!(sourceFile.exists())) {
                        throw new Saxon.Api.SaxonApiException((("Source file "+sourceFile)+" does not exist"));
                    }
                    if (sourceFile.isDirectory()) {
                        string[] files = sourceFile.list();
                        if ((files != null)) {
                            foreach (string file1 in files) {
                                Saxon.Ejava.io.File file = new Saxon.Ejava.io.File(sourceFile, file1);
                                if ((!(file.isDirectory()) && !(file.isHidden()))) {
                                    sourceInput = new Saxon.Ejavax.xml.transform.stream.StreamSource(file.toURI().ToString());
                                    sources.Add(sourceInput);
                                }
                            }
                        }
                        return true;
                    } else {
                        sourceInput = new Saxon.Ejavax.xml.transform.stream.StreamSource(sourceFile.toURI().ToString());
                        sources.Add(sourceInput);
                        return false;
                    }
                }
            }
        }
        public static bool isImplicitURI(string name) {
            return (((name.StartsWith("http:") || name.StartsWith("https:")) || name.StartsWith("file:")) || name.StartsWith("classpath:"));
        }
        public static void loadAdditionalSchemas(Saxon.Hej.Configuration config, string additionalSchemas) {
            Saxon.Ejava.util.StringTokenizer st = new Saxon.Ejava.util.StringTokenizer(additionalSchemas, System.IO.Path.PathSeparator.ToString());
            while (st.hasMoreTokens()) {
                string schema = st.nextToken();
                Saxon.Ejava.io.File schemaFile = new Saxon.Ejava.io.File(schema);
                if (!(schemaFile.exists())) {
                    throw new Saxon.Hej.type.SchemaException((("Schema document "+schema)+" not found"));
                }
                config.addSchemaSource(new Saxon.Ejavax.xml.transform.stream.StreamSource(schemaFile));
            }
        }
        public static string featureKeys() {
            int index = "http://saxon.sf.net/feature/".Length;
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder();
            foreach (string name in Saxon.Hej.lib.FeatureIndex.getNames()) {
                sb.append("\n  ").append(name.Substring(index));
            }
            return sb.ToString();
        }
        public static string getCommandName(System.Object command) {
            return "dotnet saxoncs " + command.GetType().Name.ToLower();
        }
    
    }
}