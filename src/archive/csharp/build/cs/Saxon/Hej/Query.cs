using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej {
    public class Query {
        private Saxon.Hej.s9api.Processor processor;
        protected internal Saxon.Hej.Configuration config;
        protected internal bool showTime = false;
        protected internal int repeat = 1;
        protected internal string sourceXmlFileName = null;
        protected internal string sourceJsonFileName = null;
        protected internal string queryFileName = null;
        protected internal bool useURLs = false;
        protected internal string outputFileName = null;
        protected internal string moduleURIResolverClass = null;
        protected internal readonly string uriResolverClass = null;
        protected internal bool explaining = false;
        protected internal bool wrap = false;
        protected internal bool projection = false;
        protected internal bool streaming = false;
        protected internal bool updating = false;
        protected internal bool writeback = false;
        protected internal bool backup = true;
        protected internal string explainOutputFileName = null;
        private Saxon.Hej.lib.Logger traceDestination = new Saxon.Callbacks.StandardLogger();
        private bool closeTraceDestination = false;
        private bool allowExit = true;
        protected internal string languageVersion = "3.1";
        protected internal Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public static void main(string[] args) {
            new Saxon.Hej.Query().doQuery(args, "java net.sf.saxon.Query");
        }
        internal void setPermittedOptions(Saxon.Hej.trans.CommandLineOptions options) {
            options.addRecognizedOption("backup", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Save updated documents before overwriting");
            options.addRecognizedOption("config", (Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Use specified configuration file");
            options.addRecognizedOption("cr", (Saxon.Hej.trans.CommandLineOptions.TYPE_CLASSNAME|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Use specified collection URI resolver class");
            options.addRecognizedOption("dtd", Saxon.Hej.trans.CommandLineOptions.TYPE_ENUMERATION, "Validate using DTD");
            options.setPermittedValues("dtd", new string[]{"on", "off", "recover"}, "on");
            options.addRecognizedOption("expand", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Expand attribute defaults from DTD or Schema");
            options.addRecognizedOption("explain", Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME, "Display compiled expression tree and optimization decisions");
            options.addRecognizedOption("ext", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Allow calls to Java extension functions and xsl:result-document");
            options.addRecognizedOption("json", (Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Source file for primary JSON input");
            options.addRecognizedOption("l", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Maintain line numbers for source documents");
            options.addRecognizedOption("now", (Saxon.Hej.trans.CommandLineOptions.TYPE_DATETIME|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Run with specified current date/time");
            options.addRecognizedOption("o", (Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Use specified file for primary output");
            options.addRecognizedOption("opt", (Saxon.Hej.trans.CommandLineOptions.TYPE_STRING|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Enable/disable optimization options [-]cfgklmnsvwx");
            options.addRecognizedOption("outval", (Saxon.Hej.trans.CommandLineOptions.TYPE_ENUMERATION|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Action when validation of output file fails");
            options.setPermittedValues("outval", new string[]{"recover", "fatal"}, null);
            options.addRecognizedOption("p", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Recognize query parameters in URI passed to doc()");
            options.addRecognizedOption("projection", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Use source document projection");
            options.addRecognizedOption("q", (Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Query filename");
            options.addRecognizedOption("qs", (Saxon.Hej.trans.CommandLineOptions.TYPE_STRING|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Query string (usually in quotes)");
            options.addRecognizedOption("qversion", (Saxon.Hej.trans.CommandLineOptions.TYPE_STRING|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "XQuery language version: 3.1 or 4.0. Default is 3.1");
            options.setPermittedValues("qversion", new string[]{"3.1", "4.0"}, "3.1");
            options.addRecognizedOption("repeat", (Saxon.Hej.trans.CommandLineOptions.TYPE_INTEGER|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Run N times for performance measurement");
            options.addRecognizedOption("s", (Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Source file for primary XML input");
            options.addRecognizedOption("sa", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Run in schema-aware mode");
            options.addRecognizedOption("scmin", Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME, "Pre-load schema in SCM format");
            options.addRecognizedOption("stream", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Execute in streamed mode");
            options.addRecognizedOption("strip", (Saxon.Hej.trans.CommandLineOptions.TYPE_ENUMERATION|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Handling of whitespace text nodes in source documents");
            options.setPermittedValues("strip", new string[]{"none", "all", "ignorable"}, null);
            options.addRecognizedOption("t", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Display version and timing information");
            options.addRecognizedOption("T", Saxon.Hej.trans.CommandLineOptions.TYPE_CLASSNAME, "Use named TraceListener class, or standard TraceListener");
            options.addRecognizedOption("TB", Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME, "Trace hotspot bytecode generation to specified XML file");
            options.addRecognizedOption("TJ", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Debug binding and execution of extension functions");
            options.setPermittedValues("TJ", new string[]{"on", "off"}, "on");
            options.addRecognizedOption("tree", (Saxon.Hej.trans.CommandLineOptions.TYPE_ENUMERATION|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Use specified tree model for source documents");
            options.addRecognizedOption("Tlevel", Saxon.Hej.trans.CommandLineOptions.TYPE_STRING, "Level of detail for trace listener output");
            options.setPermittedValues("Tlevel", new string[]{"none", "low", "normal", "high"}, "normal");
            options.addRecognizedOption("Tout", Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME, "File for trace listener output");
            options.addRecognizedOption("traceout", (Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "File for output of trace() messages");
            options.setPermittedValues("tree", new string[]{"linked", "tiny", "tinyc"}, null);
            options.addRecognizedOption("u", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Interpret filename arguments as URIs");
            options.setPermittedValues("u", new string[]{"on", "off"}, "on");
            options.addRecognizedOption("update", (Saxon.Hej.trans.CommandLineOptions.TYPE_ENUMERATION|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Enable or disable XQuery updates, or enable the syntax but discard the updates");
            options.setPermittedValues("update", new string[]{"on", "off", "discard"}, null);
            options.addRecognizedOption("val", Saxon.Hej.trans.CommandLineOptions.TYPE_ENUMERATION, "Apply validation to source documents");
            options.setPermittedValues("val", new string[]{"strict", "lax"}, "strict");
            options.addRecognizedOption("wrap", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Wrap result sequence in XML elements");
            options.addRecognizedOption("xsd", (Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME_LIST|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "List of schema documents to be preloaded");
            options.addRecognizedOption("xsdversion", (Saxon.Hej.trans.CommandLineOptions.TYPE_ENUMERATION|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Indicate whether XSD 1.1 is supported");
            options.setPermittedValues("xsdversion", new string[]{"1.0", "1.1"}, null);
            options.addRecognizedOption("xsiloc", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Load schemas named in xsi:schemaLocation (default on)");
            options.addRecognizedOption("?", Saxon.Hej.trans.CommandLineOptions.VALUE_PROHIBITED, "Display command line help text");
        }
        protected internal void doQuery(string[] args, string command) {
            Saxon.Hej.trans.CommandLineOptions options = new Saxon.Hej.trans.CommandLineOptions();
            setPermittedOptions(options);
            try {
                options.setActualOptions(args);
            } catch (Saxon.Hej.trans.XPathException err) {
                quit(err.Message, 2);
            }
            bool schemaAware = false;
            string configFile = options.getOptionValue("config");
            if ((configFile != null)) {
                try {
                    config = Saxon.Hej.Configuration.readConfiguration(new Saxon.Ejavax.xml.transform.stream.StreamSource(configFile));
                    initializeConfiguration(config);
                    schemaAware = config.isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XQUERY);
                } catch (Saxon.Hej.trans.XPathException e) {
                    quit(e.Message, 2);
                }
            }
            if (((config == null) && !(schemaAware))) {
                schemaAware = options.testIfSchemaAware();
            }
            if ((config == null)) {
                config = Saxon.Hej.Configuration.newConfiguration();
                initializeConfiguration(config);
            }
            processor = new Saxon.Hej.s9api.Processor(config);
            config.setProcessor(processor);
            try {
                parseOptions(options);
                Saxon.Hej.s9api.XQueryCompiler compiler = processor.newXQueryCompiler();
                compiler.setSchemaAware(schemaAware);
                compiler.setLanguageVersion(languageVersion);
                if (updating) {
                    compiler.setUpdatingEnabled(true);
                }
                if ((config.getTraceListener() != null)) {
                    compiler.setCompileWithTracing(true);
                }
                if ((moduleURIResolverClass != null)) {
                    System.Object mr = config.getInstance(moduleURIResolverClass);
                    if (!(((mr is Saxon.Hej.lib.ModuleURIResolver)))) {
                        badUsage((moduleURIResolverClass+" is not a ModuleURIResolver"));
                    }
                    if ((mr is Saxon.Callbacks.StandardModuleURIResolver)) {
                        ((Saxon.Callbacks.StandardModuleURIResolver)(mr)).setConfiguration(config);
                    }
                    compiler.setModuleURIResolver((Saxon.Hej.lib.ModuleURIResolver)(mr));
                }
                if ((uriResolverClass != null)) {
                    config.setURIResolver(config.makeURIResolver(uriResolverClass));
                }
                config.displayLicenseMessage();
                if ((schemaAware && !(config.isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XQUERY)))) {
                    if ("EE".Equals(config.getEditionCode())) {
                        quit("Installed license does not allow schema-aware query", 2);
                    } else {
                        quit("Schema-aware query requires Saxon Enterprise Edition", 2);
                    }
                }
                if (explaining) {
                    config.setBooleanProperty(Saxon.Hej.lib.Feature<bool>.TRACE_OPTIMIZER_DECISIONS, true);
                }
                compiler.setStreaming(streaming);
                Saxon.Ejavax.xml.transform.Source sourceInput = null;
                if ((sourceXmlFileName != null)) {
                    sourceInput = processSourceFile(sourceXmlFileName, useURLs);
                }
                long startTime = Saxon.Ejava.lang.System.nanoTime();
                if (showTime) {
                    config.getLogger().info(("Analyzing query from "+queryFileName));
                }
                Saxon.Hej.s9api.XQueryExecutable exp = null;
                try {
                    exp = compileQuery(compiler, queryFileName, useURLs);
                    if (showTime) {
                        long endTime = Saxon.Ejava.lang.System.nanoTime();
                        config.getLogger().info((("Analysis time: "+((((endTime-startTime))/1e6)))+" milliseconds"));
                        startTime = endTime;
                    }
                } catch (Saxon.Api.SaxonApiException e) {
                    if ((e.InnerException is Saxon.Hej.trans.XPathException)) {
                        Saxon.Hej.trans.XPathException err = (Saxon.Hej.trans.XPathException)(e.InnerException);
                        int line = -1;
                        string module = null;
                        if ((err.getLocator() != null)) {
                            line = err.getLocator().getLineNumber();
                            module = err.getLocator().getSystemId();
                        }
                        if (err.hasBeenReported()) {
                            quit("Static error(s) in query", 2);
                        } else {
                            if ((line == -1)) {
                                config.getLogger().error(("Static error in query: "+err.Message));
                            } else {
                                config.getLogger().error((((("Static error at line "+line)+" of ")+module)+':'));
                                config.getLogger().error(err.Message);
                            }
                        }
                        exp = null;
                        if (allowExit) {
                            Saxon.Ejava.lang.System.exit(2);
                        } else {
                            throw new System.Exception(err.Message);
                        }
                    } else {
                        quit(e.Message, 2);
                    }
                }
                if ((explaining && (exp != null))) {
                    Saxon.Hej.s9api.Serializer @out;
                    if (((explainOutputFileName == null) || explainOutputFileName.Equals(""))) {
                        @out = processor.newSerializer(System.Console.Error);
                    } else {
                        @out = processor.newSerializer(new Saxon.Ejava.io.File(explainOutputFileName));
                    }
                    @out.setOutputProperty(Saxon.Hej.s9api.Serializer.Property.METHOD, "xml");
                    @out.setOutputProperty(Saxon.Hej.s9api.Serializer.Property.INDENT, "yes");
                    @out.setOutputProperty(Saxon.Hej.s9api.Serializer.Property.OMIT_XML_DECLARATION, "yes");
                    if (processor.Equals(processor.getSaxonEdition())) {
                        @out.setOutputProperty(Saxon.Hej.s9api.Serializer.Property.SAXON_INDENT_SPACES, "2");
                    }
                    exp.explain(@out);
                }
                exp.getUnderlyingCompiledQuery().setAllowDocumentProjection(projection);
                Saxon.Hej.s9api.XQueryEvaluator evaluator = exp.load();
                evaluator.setTraceFunctionDestination(traceDestination);
                if ((options.getOptionValue("now") != null)) {
                    string now = options.getOptionValue("now");
                    Saxon.Hej.type.ConversionResult dt = Saxon.Hej.value.DateTimeValue.makeDateTimeValue(Saxon.Hej.str.StringView.tidy(now), config.getConversionRules());
                    if ((dt is Saxon.Hej.value.DateTimeValue)) {
                        evaluator.getUnderlyingQueryContext().setCurrentDateTime((Saxon.Hej.value.DateTimeValue)(dt));
                    } else {
                        config.getLogger().warning((("Invalid dateTime: "+now)+" (ignored)"));
                    }
                }
                if ((uriResolverClass != null)) {
                    evaluator.setURIResolver(config.makeURIResolver(uriResolverClass));
                }
                processSource(sourceInput, exp, evaluator);
                if ((sourceJsonFileName != null)) {
                    try {
                        System.IO.TextReader jsonReader = new System.IO.StreamReader(System.IO.File.OpenRead(sourceJsonFileName), System.Text.Encoding.UTF8);
                        Saxon.Api.XdmValue jsonTree = parseJson(processor, jsonReader);
                        if (!(jsonTree.Count==0)) {
                            evaluator.setContextItem(jsonTree.ItemAt(0));
                        }
                    } catch (System.IO.IOException e) {
                        throw new Saxon.Api.SaxonApiException(("Cannot read JSON input: "+e.Message));
                    }
                }
                options.setParams(processor, (name, value) => evaluator.setExternalVariable(name, value));
                startTime = Saxon.Ejava.lang.System.nanoTime();
                long totalTime = 0;
                int r;
                for (r = 0; (r<repeat); r++) {
                    try {
                        Saxon.Hej.s9api.Serializer serializer;
                        if ((outputFileName != null)) {
                            Saxon.Ejava.io.File outputFile = new Saxon.Ejava.io.File(outputFileName);
                            if (outputFile.isDirectory()) {
                                quit("Output is a directory", 2);
                            }
                            createFileIfNecessary(outputFile);
                            serializer = processor.newSerializer(new System.IO.FileStream(outputFile.getPath(), System.IO.FileMode.Create));
                        } else {
                            serializer = processor.newSerializer(System.Console.Out);
                        }
                        try {
                            options.setSerializationProperties(serializer);
                        } catch (Saxon.Ejava.lang.IllegalArgumentException e) {
                            quit(e.Message, 2);
                        }
                        if ((updating && exp.isUpdateQuery())) {
                            serializer.setOutputProperties(exp.getUnderlyingCompiledQuery().getExecutable().getPrimarySerializationProperties().getProperties());
                            runUpdate(exp, evaluator, serializer);
                        } else {
                            runQuery(exp, evaluator, sourceInput, serializer);
                        }
                    } catch (Saxon.Api.SaxonApiException err) {
                        if (((err.InnerException is Saxon.Hej.trans.XPathException) && ((Saxon.Hej.trans.XPathException)(err.InnerException)).hasBeenReported())) {
                            string category = (((Saxon.Hej.trans.XPathException)(err.InnerException)).isTypeError() ? "type" : "dynamic");
                            quit(((("Query failed with "+category)+" error: ")+err.InnerException.Message), 2);
                        } else {
                            throw;
                        }
                    }
                    if (showTime) {
                        long endTime = Saxon.Ejava.lang.System.nanoTime();
                        if ((r>=3)) {
                            totalTime += (endTime-startTime);
                        }
                        if ((repeat<100)) {
                            config.getLogger().info(("Execution time: "+Saxon.Hej.trans.Timer.showExecutionTimeNano((endTime-startTime))));
                            config.getLogger().info(Saxon.Hej.trans.Timer.showMemoryUsed());
                            Saxon.Hej.trace.Instrumentation.report();
                        } else {
                            if ((totalTime>1000000000000L)) {
                                break ;
                            }
                        }
                        startTime = endTime;
                    }
                }
                if ((repeat>3)) {
                    config.getLogger().info(("Average execution time: "+Saxon.Hej.trans.Timer.showExecutionTimeNano((totalTime/((r-3))))));
                }
                if ((options.getOptionValue("TB") != null)) {
                    config.createByteCodeReport(options.getOptionValue("TB"));
                }
            } catch (Saxon.Hej.expr.instruct.TerminationException err) {
                quit(err.Message, 1);
            } catch (Saxon.Hej.type.SchemaException err) {
                quit(("Schema processing failed: "+err.Message), 2);
            } catch (Exception err) when (err is Saxon.Hej.trans.XPathException || err is Saxon.Hej.trans.LicenseException || err is Saxon.Api.SaxonApiException) {
                quit(("Query processing failed: "+err.Message), 2);
            } catch (Saxon.Ejavax.xml.transform.TransformerFactoryConfigurationError err) {
                System.Console.Error.WriteLine(err);
                quit("Query processing failed", 2);
            } catch (System.Exception err2) {
                System.Console.Error.WriteLine(err2);
                quit(((("Fatal error during query: "+err2.GetType().Name)+": ")+(((err2.Message == null) ? " (no message)" : err2.Message))), 2);
            }
        }
        internal static  /* REPLACING static */ Saxon.Api.XdmValue parseJson(Saxon.Hej.s9api.Processor processor, System.IO.TextReader jsonReader) {
            return new Saxon.Api.JsonBuilder(processor.getUnderlyingConfiguration()).parseJson(jsonReader);
        }
        protected internal void initializeConfiguration(Saxon.Hej.Configuration config) {
        }
        internal void parseOptions(Saxon.Hej.trans.CommandLineOptions options) {
            options.applyToConfiguration(processor);
            allowExit = !("off".Equals(options.getOptionValue("quit")));
            backup = "on".Equals(options.getOptionValue("backup"));
            explainOutputFileName = options.getOptionValue("explain");
            explaining = (explainOutputFileName != null);
            moduleURIResolverClass = options.getOptionValue("mr");
            outputFileName = options.getOptionValue("o");
            streaming = "on".Equals(options.getOptionValue("stream"));
            string value = options.getOptionValue("p");
            if ("on".Equals(value)) {
                config.setParameterizedURIResolver();
                useURLs = true;
            }
            projection = "on".Equals(options.getOptionValue("projection"));
            value = options.getOptionValue("q");
            if ((value != null)) {
                queryFileName = value;
            }
            value = options.getOptionValue("qs");
            if ((value != null)) {
                queryFileName = (("{"+value)+"}");
            }
            string qv = options.getOptionValue("qversion");
            if ((((qv != null) && !("3.1".Equals(qv))) && !("4.0".Equals(qv)))) {
                config.getLogger().warning("-qversion ignored: 3.1 is assumed");
            } else {
                if ("4.0".Equals(qv)) {
                    languageVersion = "4.0";
                }
            }
            value = options.getOptionValue("repeat");
            if ((value != null)) {
                repeat = System.Int32.Parse(value);
            }
            sourceXmlFileName = options.getOptionValue("s");
            sourceJsonFileName = options.getOptionValue("json");
            value = options.getOptionValue("t");
            if ("on".Equals(value)) {
                config.getLogger().info(config.getProductTitle());
                config.getLogger().info(Saxon.Hej.Version.platform.getPlatformVersion());
                config.setTiming(true);
                showTime = true;
            }
            value = options.getOptionValue("traceout");
            if ((value != null)) {
                switch (value) {
                  case "#err":
                    break ;
                  case "#out":
                    traceDestination = new Saxon.Callbacks.StandardLogger(System.Console.Out);
                    break ;
                  case "#null":
                    traceDestination = null;
                    break ;
                  default:
                    try {
                        traceDestination = new Saxon.Callbacks.StandardLogger(new Saxon.Ejava.io.File(value));
                        closeTraceDestination = true;
                    } catch (Saxon.Ejava.io.FileNotFoundException e) {
                        badUsage((("Trace output file "+value)+" cannot be created"));
                    }
                    break ;
                }
            }
            value = options.getOptionValue("T");
            if ((value != null)) {
                if ("".Equals(value)) {
                    makeXQueryTraceListener(options);
                } else {
                    config.setTraceListenerClass(value);
                }
                config.setLineNumbering(true);
            }
            value = options.getOptionValue("Tout");
            if ((value != null)) {
                config.setTraceListenerOutputFile(value);
                if ((options.getOptionValue("T") == null)) {
                    makeXQueryTraceListener(options);
                }
            }
            value = options.getOptionValue("TB");
            if ((value != null)) {
                config.setBooleanProperty(Saxon.Hej.lib.Feature<bool>.MONITOR_HOT_SPOT_BYTE_CODE, true);
            }
            value = options.getOptionValue("u");
            if ((value != null)) {
                useURLs = "on".Equals(value);
            }
            value = options.getOptionValue("update");
            if ((value != null)) {
                if (!("off".Equals(value))) {
                    updating = true;
                }
                writeback = !("discard".Equals(value));
            }
            wrap = "on".Equals(options.getOptionValue("wrap"));
            value = options.getOptionValue("x");
            if ((value != null)) {
                config.setSourceParserClass(value);
            }
            string additionalSchemas = options.getOptionValue("xsd");
            value = options.getOptionValue("?");
            if ((value != null)) {
                badUsage("");
            }
            if (((options.getOptionValue("xsiloc") != null) && (options.getOptionValue("val") == null))) {
                config.getLogger().warning("-xsiloc is ignored when -val is absent");
            }
            if (((sourceXmlFileName != null) && (sourceJsonFileName != null))) {
                badUsage("Cannot supply both -s and -json");
            }
            applyLocalOptions(options, config);
            System.Collections.Generic.IList<string> positional = options.getPositionalOptions();
            int currentPositionalOption = 0;
            if ((queryFileName == null)) {
                if ((positional.Count == currentPositionalOption)) {
                    badUsage("No query file name");
                }
                queryFileName = positional[currentPositionalOption++];
            }
            if ((currentPositionalOption<positional.Count)) {
                badUsage(("Unrecognized option: "+positional[currentPositionalOption]));
            }
            string scmInput = options.getOptionValue("scmin");
            if ((scmInput != null)) {
                config.importComponents(new Saxon.Ejavax.xml.transform.stream.StreamSource(scmInput));
            }
            if ((additionalSchemas != null)) {
                Saxon.Hej.trans.CommandLineOptions.loadAdditionalSchemas(config, additionalSchemas);
            }
        }
        private void makeXQueryTraceListener(Saxon.Hej.trans.CommandLineOptions options) {
            Saxon.Hej.trace.XQueryTraceListener listener = new Saxon.Hej.trace.XQueryTraceListener();
            string value = options.getOptionValue("Tout");
            if ((value != null)) {
                try {
                    listener.setOutputDestination(new Saxon.Callbacks.StandardLogger(new Saxon.Ejava.io.PrintStream(value)));
                } catch (Saxon.Ejava.io.FileNotFoundException e) {
                    badUsage(("Cannot write to "+value));
                }
            }
            value = options.getOptionValue("Tlevel");
            if ((value != null)) {
                switch (value) {
                  case "none":
                    listener.setLevelOfDetail(0);
                    break ;
                  case "low":
                    listener.setLevelOfDetail(1);
                    break ;
                  case "normal":
                    listener.setLevelOfDetail(2);
                    break ;
                  case "high":
                    listener.setLevelOfDetail(3);
                    break ;
                }
            }
            config.setTraceListener(listener);
        }
        internal  /* REPLACING protected internal */ void applyLocalOptions(Saxon.Hej.trans.CommandLineOptions options, Saxon.Hej.Configuration config) {
        }
        protected internal Saxon.Ejavax.xml.transform.Source processSourceFile(string sourceFileName, bool useURLs) {
            Saxon.Ejavax.xml.transform.Source sourceInput;
            if ((useURLs || Saxon.Hej.trans.CommandLineOptions.isImplicitURI(sourceFileName))) {
                sourceInput = config.getURIResolver().resolve(sourceFileName, null);
                if ((sourceInput == null)) {
                    sourceInput = config.getSystemURIResolver().resolve(sourceFileName, null);
                }
            } else {
                if (sourceFileName.Equals("-")) {
                    string sysId = new Saxon.Ejava.io.File(Saxon.Ejava.lang.System.getProperty("user.dir")).toURI().OriginalString;
                    sourceInput = new Saxon.Ejavax.xml.transform.stream.StreamSource(System.Console.In, sysId);
                } else {
                    Saxon.Ejava.io.File sourceFile = new Saxon.Ejava.io.File(sourceFileName);
                    if (!(sourceFile.exists())) {
                        quit((("Source file "+sourceFile)+" does not exist"), 2);
                    }
                    sourceInput = new Saxon.Ejavax.xml.transform.stream.StreamSource(sourceFile.toURI().ToString());
                }
            }
            return sourceInput;
        }
        internal  /* REPLACING protected internal */ Saxon.Hej.s9api.XQueryExecutable compileQuery(Saxon.Hej.s9api.XQueryCompiler compiler, string queryFileName, bool useURLs) {
            Saxon.Hej.s9api.XQueryExecutable exp;
            if (queryFileName.Equals("-")) {
                System.IO.TextReader queryReader = standardInputReader();
                compiler.setBaseURI(new Saxon.Ejava.io.File(Saxon.Ejava.lang.System.getProperty("user.dir")).toURI());
                exp = compiler.compile(queryReader);
            } else {
                if ((queryFileName.StartsWith("{") && queryFileName.EndsWith("}"))) {
                    string q = queryFileName.Substring(1, (queryFileName.Length-1) - 1);
                    compiler.setBaseURI(new Saxon.Ejava.io.File(Saxon.Ejava.lang.System.getProperty("user.dir")).toURI());
                    exp = compiler.compile(q);
                } else {
                    if ((useURLs || Saxon.Hej.trans.CommandLineOptions.isImplicitURI(queryFileName))) {
                        Saxon.Hej.lib.ModuleURIResolver resolver = compiler.getModuleURIResolver();
                        bool isStandardResolver = false;
                        if ((resolver == null)) {
                            resolver = getConfiguration().getStandardModuleURIResolver();
                            isStandardResolver = true;
                        }
                        while (true) {
                            string[] locations = {queryFileName};
                            Saxon.Ejavax.xml.transform.Source[] sources;
                            try {
                                sources = resolver.resolve(null, null, locations);
                            } catch (System.Exception e) {
                                if ((e is Saxon.Hej.trans.XPathException)) {
                                    throw new Saxon.Api.SaxonApiException(e);
                                } else {
                                    Saxon.Hej.trans.XPathException xe = new Saxon.Hej.trans.XPathException("Exception in ModuleURIResolver: ", e);
                                    xe.setErrorCode("XQST0059");
                                    throw new Saxon.Api.SaxonApiException(xe);
                                }
                            }
                            if ((sources == null)) {
                                if (isStandardResolver) {
                                    quit("System problem: standard ModuleURIResolver returned null", 4);
                                } else {
                                    resolver = getConfiguration().getStandardModuleURIResolver();
                                    isStandardResolver = true;
                                }
                            } else {
                                if (((sources.Length != 1) || !(((sources[0] is Saxon.Ejavax.xml.transform.stream.StreamSource))))) {
                                    quit("Module URI Resolver must return a single StreamSource", 2);
                                }
                                try {
                                    string queryText = Saxon.Hej.query.QueryReader.readSourceQuery((Saxon.Ejavax.xml.transform.stream.StreamSource)(sources[0]), config.getValidCharacterChecker());
                                    exp = compiler.compile(queryText);
                                } catch (Saxon.Hej.trans.XPathException e) {
                                    throw new Saxon.Api.SaxonApiException(e);
                                }
                                break ;
                            }
                        }
                    } else {
                        using (System.IO.Stream queryStream = System.IO.File.OpenRead(queryFileName)) {                            try {
                                compiler.setBaseURI(new Saxon.Ejava.io.File(queryFileName).toURI());
                                exp = compiler.compile(queryStream);
                            } finally {
                            }
                        }                    }
                }
            }
            return exp;
        }
        private System.IO.TextReader standardInputReader() {
            return System.Console.In;
        }
        protected internal void explain(Saxon.Hej.query.XQueryExpression exp) {
            Saxon.Ejavax.xml.transform.stream.StreamResult explainOutput;
            if (((explainOutputFileName == null) || "".Equals(explainOutputFileName))) {
                explainOutput = standardErrorResult();
            } else {
                explainOutput = fileResult(explainOutputFileName);
            }
            Saxon.Hej.serialize.SerializationProperties props = Saxon.Hej.trace.ExpressionPresenter.makeDefaultProperties(config);
            Saxon.Hej.@event.Receiver diag = config.getSerializerFactory().getReceiver(explainOutput, props);
            Saxon.Hej.trace.ExpressionPresenter expressionPresenter = new Saxon.Hej.trace.ExpressionPresenter(config, diag);
            exp.explain(expressionPresenter);
        }
        private Saxon.Ejavax.xml.transform.stream.StreamResult standardErrorResult() {
            return new Saxon.Ejavax.xml.transform.stream.StreamResult(System.Console.Error);
        }
        private Saxon.Ejavax.xml.transform.stream.StreamResult fileResult(string fileName) {
            return new Saxon.Ejavax.xml.transform.stream.StreamResult(new System.IO.FileStream(fileName, System.IO.FileMode.OpenOrCreate));
        }
        internal  /* REPLACING protected internal */ void processSource(Saxon.Ejavax.xml.transform.Source sourceInput, Saxon.Hej.s9api.XQueryExecutable exp, Saxon.Hej.s9api.XQueryEvaluator evaluator) {
            if (((sourceInput != null) && !(streaming))) {
                Saxon.Hej.s9api.DocumentBuilder builder = processor.newDocumentBuilder();
                if (exp.isUpdateQuery()) {
                    builder.setTreeModel(Saxon.Hej.om.TreeModel.LINKED_TREE);
                }
                if (showTime) {
                    config.getLogger().info(("Processing "+sourceInput.getSystemId()));
                }
                if (!(exp.getUnderlyingCompiledQuery().usesContextItem())) {
                    config.getLogger().warning("Source document ignored - query can be evaluated without reference to the context item");
                    return ;
                }
                if (projection) {
                    builder.setDocumentProjectionQuery(exp);
                    if (explaining) {
                        exp.getUnderlyingCompiledQuery().explainPathMap();
                    }
                }
                builder.setDTDValidation(getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.DTD_VALIDATION));
                if (getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.DTD_VALIDATION_RECOVERABLE)) {
                    sourceInput = new Saxon.Hej.lib.AugmentedSource(sourceInput, getConfiguration().getParseOptions());
                }
                Saxon.Api.XdmNode doc = builder.build(sourceInput);
                evaluator.setContextItem(doc);
            }
        }
        internal  /* REPLACING protected internal */ void runQuery(Saxon.Hej.s9api.XQueryExecutable exp, Saxon.Hej.s9api.XQueryEvaluator evaluator, Saxon.Ejavax.xml.transform.Source input, Saxon.Hej.s9api.Destination destination) {
            try {
                if (wrap) {
                    try {
                        Saxon.Hej.query.XQueryExpression e = exp.getUnderlyingCompiledQuery();
                        Saxon.Hej.om.SequenceIterator results = e.iterator(evaluator.getUnderlyingQueryContext());
                        Saxon.Hej.om.NodeInfo resultDoc = Saxon.Hej.query.QueryResult.wrap(results, config);
                        Saxon.Api.XdmValue wrappedResultDoc = Saxon.Api.XdmValue.Wrap(resultDoc);
                        processor.writeXdmValue(wrappedResultDoc, destination);
                        destination.closeAndNotify();
                    } catch (Saxon.Hej.trans.XPathException e1) {
                        throw new Saxon.Api.SaxonApiException(e1);
                    }
                } else {
                    if (streaming) {
                        evaluator.runStreamed(input, destination);
                    } else {
                        evaluator.run(destination);
                    }
                }
            } finally {
                if ((closeTraceDestination && (traceDestination != null))) {
                    traceDestination.close();
                }
            }
        }
        internal  /* REPLACING protected internal */ void runUpdate(Saxon.Hej.s9api.XQueryExecutable exp, Saxon.Hej.s9api.XQueryEvaluator evaluator, Saxon.Hej.s9api.Serializer serializer) {
            try {
                if ((serializer.getOutputProperty(Saxon.Hej.s9api.Serializer.Property.METHOD) == null)) {
                    serializer.setOutputProperty(Saxon.Hej.s9api.Serializer.Property.METHOD, "xml");
                }
                if (writeback) {
                    System.Collections.Generic.IList<Saxon.Api.SaxonApiException> errors = new System.Collections.Generic.List<Saxon.Api.SaxonApiException>(3);
                    Saxon.Hej.query.UpdateAgent agent = (node, controller) => {
                        try {
                            Saxon.Hej.om.DocumentPool pool = controller.getDocumentPool();
                            string documentURI = pool.getDocumentURI(node);
                            if ((documentURI != null)) {
                                rewriteToDisk(node, serializer, backup, (showTime ? config.getLogger() : null));
                            } else {
                                if (showTime) {
                                    config.getLogger().warning("Updated document discarded because it was not read using doc()");
                                }
                            }
                        } catch (Saxon.Api.SaxonApiException err) {
                            config.getLogger().warning(err.Message);
                            errors.Add(err);
                        }
                    } ;
                    evaluator.run();
                    try {
                        exp.getUnderlyingCompiledQuery().runUpdate(evaluator.getUnderlyingQueryContext(), agent);
                    } catch (Saxon.Hej.trans.XPathException e) {
                        throw new Saxon.Api.SaxonApiException(e);
                    }
                    if (!(errors.Count==0)) {
                        throw errors[0];
                    }
                } else {
                    try {
                        if ((evaluator.getContextItem() != null)) {
                            System.Collections.Generic.ISet<Saxon.Hej.om.MutableNodeInfo> affectedDocuments = exp.getUnderlyingCompiledQuery().runUpdate(evaluator.getUnderlyingQueryContext());
                            Saxon.Hej.om.Item initial = ((Saxon.Hej.om.Item)evaluator.getContextItem().Unwrap()).head();
                            if (((initial is Saxon.Hej.om.NodeInfo) && affectedDocuments.Contains(initial))) {
                                processor.writeXdmValue(evaluator.getContextItem(), serializer);
                            }
                        }
                    } catch (Saxon.Hej.trans.XPathException e) {
                        throw new Saxon.Api.SaxonApiException(e);
                    }
                }
            } finally {
                if ((closeTraceDestination && (traceDestination != null))) {
                    traceDestination.close();
                }
            }
        }
        private static void rewriteToDisk(Saxon.Hej.om.NodeInfo doc, Saxon.Hej.s9api.Serializer serializer, bool backup, Saxon.Hej.lib.Logger logger) {
            switch (doc.getNodeKind()) {
              case Saxon.Hej.type.Type.DOCUMENT:
                break ;
              case Saxon.Hej.type.Type.ELEMENT:
                Saxon.Hej.om.NodeInfo parent = doc.getParent();
                if (((parent != null) && (parent.getNodeKind() != Saxon.Hej.type.Type.DOCUMENT))) {
                    throw new Saxon.Api.SaxonApiException("Cannot rewrite an element node unless it is top-level");
                }
                break ;
              default:
                throw new Saxon.Api.SaxonApiException("Node to be rewritten must be a document or element node");
            }
            string uri = doc.getSystemId();
            if (((uri == null) || uri.Length==0)) {
                throw new Saxon.Api.SaxonApiException("Cannot rewrite a document with no known URI");
            }
            System.Uri u;
            try {
                u = new System.Uri(uri, System.UriKind.RelativeOrAbsolute);
            } catch (System.UriFormatException e) {
                throw new Saxon.Api.SaxonApiException(("SystemId of updated document is not a valid URI: "+uri));
            }
            Saxon.Ejava.io.File existingFile = new Saxon.Ejava.io.File(u);
            Saxon.Ejava.io.File dir = existingFile.getParentFile();
            if ((backup && existingFile.exists())) {
                Saxon.Ejava.io.File backupFile = new Saxon.Ejava.io.File(dir, (existingFile.getName()+".bak"));
                if ((logger != null)) {
                    logger.info(("Creating backup file "+backupFile));
                }
                bool success = existingFile.renameTo(backupFile);
                if (!(success)) {
                    throw new Saxon.Api.SaxonApiException(("Failed to create backup file of "+backupFile));
                }
            }
            if (!(existingFile.exists())) {
                if ((logger != null)) {
                    logger.info(("Creating file "+existingFile));
                }
                try {
                    existingFile.createNewFile();
                } catch (System.IO.IOException e) {
                    throw new Saxon.Api.SaxonApiException(("Failed to create new file "+existingFile));
                }
            } else {
                if ((logger != null)) {
                    logger.info(("Overwriting file "+existingFile));
                }
            }
            serializer.setOutputFile(existingFile);
            serializer.getProcessor().writeXdmValue(Saxon.Api.XdmValue.Wrap(doc), serializer);
        }
        protected internal void quit(string message, int code) {
            if (((config != null) && (config.getLogger() != null))) {
                config.getLogger().warning(message);
            } else {
                System.Console.Error.WriteLine(message);
            }
            if (allowExit) {
                Saxon.Ejava.lang.System.exit(code);
            } else {
                throw new System.Exception(message);
            }
        }
        protected internal void badUsage(string message) {
            Saxon.Hej.lib.Logger logger = config.getLogger();
            if (!("".Equals(message))) {
                logger.error(message);
            }
            if (!(showTime)) {
                logger.info(config.getProductTitle());
            }
            logger.info("Usage: see http://www.saxonica.com/documentation/index.html#!using-xquery/commandline");
            logger.info((("Format: "+Saxon.Hej.trans.CommandLineOptions.getCommandName(this))+" options params"));
            Saxon.Hej.trans.CommandLineOptions options = new Saxon.Hej.trans.CommandLineOptions();
            setPermittedOptions(options);
            logger.info(("Options available:"+options.displayPermittedOptions()));
            logger.info("Use -XYZ:? for details of option XYZ or --? to list configuration features");
            logger.info("Params: ");
            logger.info("  param=value           Set query string parameter");
            logger.info("  +param=filename       Set query document parameter");
            logger.info("  ?param=expression     Set query parameter using XPath");
            logger.info("  !param=value          Set serialization parameter");
            if (allowExit) {
                Saxon.Ejava.lang.System.exit(("".Equals(message) ? 0 : 2));
            } else {
                throw new System.Exception(message);
            }
        }
        public static void createFileIfNecessary(Saxon.Ejava.io.File file) {
            if (!(file.exists())) {
                Saxon.Ejava.io.File directory = file.getParentFile();
                if (((directory != null) && !(directory.exists()))) {
                    directory.mkdirs();
                }
                file.createNewFile();
            }
        }
        private string getCommandName() {
            string s = this.GetType().Name;
            if (s.Equals("cli.Saxon.Cmd.DotNetQuery")) {
                s = "Query";
            }
            return s;
        }
    
    }
}