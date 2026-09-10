using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej {
    public class Transform {
        private Saxon.Hej.s9api.Processor processor;
        private Saxon.Hej.s9api.XsltCompiler compiler;
        protected internal bool useURLs = false;
        protected internal bool showTime = false;
        protected internal int repeat = 1;
        protected internal string sourceParserName = null;
        protected internal bool schemaAware = false;
        protected internal bool allowExit = true;
        protected internal bool run = true;
        private Saxon.Hej.lib.Logger traceDestination = new Saxon.Callbacks.StandardLogger();
        private bool closeTraceDestination = false;
        public static void main(string[] args) {
            new Saxon.Hej.Transform().doTransform(args);
        }
        internal  /* REPLACING public */ void setPermittedOptions(Saxon.Hej.trans.CommandLineOptions options) {
            options.addRecognizedOption("catalog", (Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Use specified catalog file to resolve URIs");
            options.addRecognizedOption("config", (Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Use specified configuration file");
            options.addRecognizedOption("diag", Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME, "Display runtime diagnostics");
            options.addRecognizedOption("dtd", Saxon.Hej.trans.CommandLineOptions.TYPE_ENUMERATION, "Validate using DTD");
            options.setPermittedValues("dtd", new string[]{"on", "off", "recover"}, "on");
            options.addRecognizedOption("ea", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Enable assertions");
            options.addRecognizedOption("expand", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Expand attribute defaults from DTD or Schema");
            options.addRecognizedOption("explain", Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME, "Display compiled expression tree and optimization decisions in human-readable form");
            options.addRecognizedOption("export", Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME, "Display compiled expression tree and optimization decisions for exportation");
            options.addRecognizedOption("ext", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Allow calls to Java extension functions and xsl:result-document");
            options.addRecognizedOption("im", (Saxon.Hej.trans.CommandLineOptions.TYPE_QNAME|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Name of initial mode");
            options.addRecognizedOption("it", Saxon.Hej.trans.CommandLineOptions.TYPE_QNAME, "Name of initial template");
            options.addRecognizedOption("jit", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Just-in-time compilation");
            options.addRecognizedOption("json", (Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Source JSON file for primary input");
            options.addRecognizedOption("l", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Maintain line numbers for source documents");
            options.addRecognizedOption("lib", (Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME_LIST|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "List of file names of library packages used by the stylesheet");
            options.addRecognizedOption("nogo", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Compile only, no evaluation");
            options.addRecognizedOption("now", (Saxon.Hej.trans.CommandLineOptions.TYPE_DATETIME|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Run with specified current date/time");
            options.addRecognizedOption("ns", (Saxon.Hej.trans.CommandLineOptions.TYPE_STRING|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Default namespace for element names (URI, or ##any, or ##html5)");
            options.addRecognizedOption("o", (Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Use specified file for primary output");
            options.addRecognizedOption("opt", (Saxon.Hej.trans.CommandLineOptions.TYPE_STRING|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Enable/disable optimization options [-]cfgjklmnrsvwx");
            options.addRecognizedOption("outval", (Saxon.Hej.trans.CommandLineOptions.TYPE_ENUMERATION|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Action when validation of output file fails");
            options.setPermittedValues("outval", new string[]{"recover", "fatal"}, null);
            options.addRecognizedOption("p", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Recognize query parameters in URI passed to doc()");
            options.addRecognizedOption("relocate", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Produce relocatable packages");
            options.addRecognizedOption("repeat", (Saxon.Hej.trans.CommandLineOptions.TYPE_INTEGER|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Run N times for performance measurement");
            options.addRecognizedOption("s", (Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Source XML file for primary input");
            options.addRecognizedOption("sa", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Run in schema-aware mode");
            options.addRecognizedOption("scmin", Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME, "Pre-load schema in SCM format");
            options.addRecognizedOption("strip", (Saxon.Hej.trans.CommandLineOptions.TYPE_ENUMERATION|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Handling of whitespace text nodes in source documents");
            options.setPermittedValues("strip", new string[]{"none", "all", "ignorable"}, null);
            options.addRecognizedOption("t", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Display version and timing information, and names of output files");
            options.addRecognizedOption("target", (Saxon.Hej.trans.CommandLineOptions.TYPE_ENUMERATION|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Target Saxon edition for execution via -export");
            options.setPermittedValues("target", new string[]{"EE", "PE", "HE", "JS"}, null);
            options.addRecognizedOption("T", Saxon.Hej.trans.CommandLineOptions.TYPE_CLASSNAME, "Use named TraceListener class, or standard TraceListener");
            options.addRecognizedOption("Tlevel", Saxon.Hej.trans.CommandLineOptions.TYPE_STRING, "Level of detail for trace listener output");
            options.setPermittedValues("Tlevel", new string[]{"none", "low", "normal", "high"}, "normal");
            options.addRecognizedOption("Tout", Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME, "File for trace listener output");
            options.addRecognizedOption("tree", (Saxon.Hej.trans.CommandLineOptions.TYPE_ENUMERATION|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Use specified tree model for source documents");
            options.setPermittedValues("tree", new string[]{"linked", "tiny", "tinyc"}, null);
            options.addRecognizedOption("traceout", (Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "File for output of trace() and -T output");
            options.addRecognizedOption("u", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Interpret filename arguments as URIs");
            options.setPermittedValues("u", new string[]{"on", "off"}, "on");
            options.addRecognizedOption("val", Saxon.Hej.trans.CommandLineOptions.TYPE_ENUMERATION, "Apply validation to source documents");
            options.setPermittedValues("val", new string[]{"strict", "lax"}, "strict");
            options.addRecognizedOption("xsd", (Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME_LIST|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "List of schema documents to be preloaded");
            options.addRecognizedOption("xsdversion", (Saxon.Hej.trans.CommandLineOptions.TYPE_ENUMERATION|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Indicate whether XSD 1.1 is supported");
            options.setPermittedValues("xsdversion", new string[]{"1.0", "1.1"}, null);
            options.addRecognizedOption("xsiloc", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Load schemas named in xsi:schemaLocation (default on)");
            options.addRecognizedOption("xsl", (Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Main stylesheet file");
            options.addRecognizedOption("?", Saxon.Hej.trans.CommandLineOptions.VALUE_PROHIBITED, "Display command line help text");
        }
        public void doTransform(string[] args) {
            Saxon.Hej.Configuration config;
            string sourceXmlFileName = null;
            string sourceJsonFileName = null;
            string styleFileName = null;
            Saxon.Ejava.io.File outputFile = null;
            string outputFileName = null;
            bool useAssociatedStylesheet = false;
            bool wholeDirectory = false;
            bool dtdValidation = false;
            string styleParserName = null;
            bool explain = false;
            bool export = false;
            string explainOutputFileName = null;
            string exportOutputFileName = null;
            string additionalSchemas = null;
            Saxon.Hej.lib.TraceListener traceListener = null;
            int threadCount = 0;
            bool jit = true;
            Saxon.Hej.trans.CommandLineOptions options = new Saxon.Hej.trans.CommandLineOptions();
            setPermittedOptions(options);
            try {
                options.setActualOptions(args);
            } catch (Saxon.Hej.trans.XPathException err) {
                quit(err.Message, 2);
            }
            schemaAware = false;
            string configFileName = options.getOptionValue("config");
            if ((configFileName != null)) {
                Saxon.Ejava.io.File configFile = new Saxon.Ejava.io.File(configFileName);
                if (!(configFile.exists())) {
                    quit((("Configuration file "+configFileName)+" does not exist"), 2);
                }
                try {
                    config = Saxon.Hej.Configuration.readConfiguration(new Saxon.Ejavax.xml.transform.stream.StreamSource(configFile.toURI().ToString()));
                    initializeConfiguration(config);
                    processor = new Saxon.Hej.s9api.Processor(config);
                    schemaAware = config.isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT);
                } catch (Saxon.Hej.trans.XPathException e) {
                    quit(e.Message, 2);
                }
            }
            if (((processor == null) && !(schemaAware))) {
                schemaAware = options.testIfSchemaAware();
            }
            if ((processor == null)) {
                processor = new Saxon.Hej.s9api.Processor(true);
                config = processor.getUnderlyingConfiguration();
                initializeConfiguration(config);
                try {
                    setFactoryConfiguration(schemaAware, null);
                    Saxon.Hej.trans.CompilerInfo defaultCompilerInfo = config.getDefaultXsltCompilerInfo();
                    defaultCompilerInfo.setSchemaAware(schemaAware);
                } catch (System.Exception err) {
                    System.Console.Error.WriteLine(err);
                    quit(err.Message, 2);
                }
            }
            config = processor.getUnderlyingConfiguration();
            try {
                options.applyToConfiguration(processor);
                compiler = processor.newXsltCompiler();
                allowExit = !("off".Equals(options.getOptionValue("quit")));
                string value = options.getOptionValue("explain");
                if ((value != null)) {
                    explain = true;
                    processor.setConfigurationProperty(Saxon.Hej.lib.Feature<bool>.TRACE_OPTIMIZER_DECISIONS, true);
                    jit = false;
                    compiler.setJustInTimeCompilation(jit);
                    if (!("".Equals(value))) {
                        explainOutputFileName = value;
                    }
                }
                value = options.getOptionValue("export");
                if ((value != null)) {
                    export = true;
                    jit = false;
                    compiler.setJustInTimeCompilation(jit);
                    if (!("".Equals(value))) {
                        exportOutputFileName = value;
                    }
                }
                value = options.getOptionValue("target");
                if ((value != null)) {
                    compiler.setTargetEdition(value);
                }
                value = options.getOptionValue("relocate");
                if (((value != null) && !("off".Equals(value)))) {
                    compiler.setRelocatable(true);
                }
                value = options.getOptionValue("jit");
                if ((value != null)) {
                    if ((("on".Equals(value) && (exportOutputFileName == null)) && run)) {
                        if (export) {
                            jit = false;
                            config.getLogger().warning("Warning: -jit:on is ignored when -export:on is set");
                        } else {
                            jit = true;
                        }
                    } else {
                        if ("off".Equals(value)) {
                            jit = false;
                        }
                    }
                    compiler.setJustInTimeCompilation(jit);
                }
                value = options.getOptionValue("lib");
                if ((value != null)) {
                    Saxon.Ejava.util.StringTokenizer st = new Saxon.Ejava.util.StringTokenizer(value, System.IO.Path.PathSeparator.ToString());
                    System.Collections.Generic.ISet<Saxon.Ejava.io.File> packs = new System.Collections.Generic.HashSet<Saxon.Ejava.io.File>();
                    while (st.hasMoreTokens()) {
                        string n = st.nextToken();
                        packs.Add(new Saxon.Ejava.io.File(n));
                    }
                    Saxon.Hej.trans.packages.PackageLibrary lib = null;
                    try {
                        lib = new Saxon.Hej.trans.packages.PackageLibrary(compiler.getUnderlyingCompilerInfo(), packs);
                    } catch (Saxon.Hej.trans.XPathException e) {
                        quit(e.Message, 2);
                    }
                    compiler.getUnderlyingCompilerInfo().setPackageLibrary(lib);
                }
                value = options.getOptionValue("ns");
                if ((value != null)) {
                    if (value.Equals("##any")) {
                        compiler.setUnprefixedElementMatchingPolicy(Saxon.Api.UnprefixedElementMatchingPolicy.AnyNamespace);
                    } else {
                        if (value.Equals("##html5")) {
                            compiler.setDefaultElementNamespace(Saxon.Hej.lib.NamespaceConstant.XHTML);
                            compiler.setUnprefixedElementMatchingPolicy(Saxon.Api.UnprefixedElementMatchingPolicy.DefaultNamespaceOrNone);
                        } else {
                            compiler.setDefaultElementNamespace(value);
                        }
                    }
                }
                value = options.getOptionValue("o");
                if ((value != null)) {
                    outputFileName = value;
                }
                value = options.getOptionValue("nogo");
                if ((value != null)) {
                    if (jit) {
                        config.getLogger().warning("Warning: -jit:on is ignored when -nogo is set");
                    }
                    run = false;
                    compiler.setJustInTimeCompilation(false);
                }
                value = options.getOptionValue("p");
                if ("on".Equals(value)) {
                    config.setBooleanProperty(Saxon.Hej.lib.Feature<bool>.RECOGNIZE_URI_QUERY_PARAMETERS, true);
                    useURLs = true;
                }
                value = options.getOptionValue("repeat");
                if ((value != null)) {
                    try {
                        repeat = Saxon.Impl.Helpers.Utils.ParseInt(value);
                    } catch (Saxon.Ejava.lang.NumberFormatException err) {
                        badUsage("Bad number after -repeat");
                    }
                }
                value = options.getOptionValue("s");
                if ((value != null)) {
                    sourceXmlFileName = value;
                }
                value = options.getOptionValue("json");
                if ((value != null)) {
                    sourceJsonFileName = value;
                }
                value = options.getOptionValue("threads");
                if ((value != null)) {
                    threadCount = Saxon.Impl.Helpers.Utils.ParseInt(value);
                }
                value = options.getOptionValue("t");
                if ((value != null)) {
                    config.getLogger().info(config.getProductTitle());
                    config.getLogger().info(Saxon.Hej.Version.platform.getPlatformVersion());
                    processor.setConfigurationProperty(Saxon.Hej.lib.Feature<bool>.TIMING, true);
                    showTime = true;
                }
                value = options.getOptionValue("T");
                if ((value != null)) {
                    string @out = options.getOptionValue("Tout");
                    if ((@out != null)) {
                        config.setTraceListenerOutputFile(@out);
                    }
                    if ("".Equals(value)) {
                        traceListener = new Saxon.Hej.trace.XSLTTraceListener();
                    } else {
                        traceListener = config.makeTraceListener(value);
                    }
                    processor.setConfigurationProperty(Saxon.Hej.lib.Feature<Saxon.Hej.lib.TraceListener>.TRACE_LISTENER, traceListener);
                    processor.setConfigurationProperty(Saxon.Hej.lib.Feature<bool>.LINE_NUMBERING, true);
                    value = options.getOptionValue("Tlevel");
                    if (((value != null) && (traceListener is Saxon.Hej.trace.AbstractTraceListener))) {
                        switch (value) {
                          case "none":
                            ((Saxon.Hej.trace.AbstractTraceListener)(traceListener)).setLevelOfDetail(0);
                            break ;
                          case "low":
                            ((Saxon.Hej.trace.AbstractTraceListener)(traceListener)).setLevelOfDetail(1);
                            break ;
                          case "normal":
                            ((Saxon.Hej.trace.AbstractTraceListener)(traceListener)).setLevelOfDetail(2);
                            break ;
                          case "high":
                            ((Saxon.Hej.trace.AbstractTraceListener)(traceListener)).setLevelOfDetail(3);
                            break ;
                        }
                    }
                }
                value = options.getOptionValue("traceout");
                if ((value == null)) {
                    if ((traceDestination == null)) {
                        traceDestination = config.getLogger();
                    }
                } else {
                    switch (value) {
                      case "#err":
                        traceDestination = new Saxon.Callbacks.StandardLogger();
                        break ;
                      case "#out":
                        traceDestination = new Saxon.Callbacks.StandardLogger(System.Console.Out);
                        break ;
                      case "#null":
                        traceDestination = null;
                        break ;
                      default:
                        traceDestination = new Saxon.Callbacks.StandardLogger(new Saxon.Ejava.io.File(value));
                        closeTraceDestination = true;
                        break ;
                    }
                }
                value = options.getOptionValue("u");
                if ((value != null)) {
                    useURLs = "on".Equals(value);
                }
                value = options.getOptionValue("x");
                if ((value != null)) {
                    sourceParserName = value;
                    processor.setConfigurationProperty(Saxon.Hej.lib.Feature<string>.SOURCE_PARSER_CLASS, sourceParserName);
                }
                value = options.getOptionValue("xsd");
                if ((value != null)) {
                    additionalSchemas = value;
                }
                value = options.getOptionValue("xsdversion");
                if ((value != null)) {
                    processor.setConfigurationProperty(Saxon.Hej.lib.Feature<string>.XSD_VERSION, value);
                }
                value = options.getOptionValue("xsl");
                if ((value != null)) {
                    styleFileName = value;
                }
                value = options.getOptionValue("y");
                if ((value != null)) {
                    styleParserName = value;
                    processor.setConfigurationProperty(Saxon.Hej.lib.Feature<string>.STYLE_PARSER_CLASS, value);
                }
                value = options.getOptionValue("?");
                if ((value != null)) {
                    badUsage("");
                }
                if (!(config.getEditionCode().Equals("HE"))) {
                    string lic = options.getOptionValue("license");
                    if (((lic == null) || "on".Equals(lic))) {
                        config.displayLicenseMessage();
                    } else {
                        config.disableLicensing();
                    }
                }
                applyLocalOptions(options, config);
                if (((sourceXmlFileName != null) && (sourceJsonFileName != null))) {
                    badUsage("-s and -json options cannot be used together");
                }
                if (((options.getOptionValue("it") != null) && useAssociatedStylesheet)) {
                    badUsage("-it and -a options cannot be used together");
                }
                if (((sourceJsonFileName != null) && useAssociatedStylesheet)) {
                    badUsage("-json and -a options cannot be used together");
                }
                if (((options.getOptionValue("xsiloc") != null) && (options.getOptionValue("val") == null))) {
                    config.getLogger().warning("-xsiloc is ignored when -val is absent");
                }
                System.Collections.Generic.IList<string> positional = options.getPositionalOptions();
                int currentPositionalOption = 0;
                if ((((run && (options.getOptionValue("it") == null)) && (sourceXmlFileName == null)) && (sourceJsonFileName == null))) {
                    if ((positional.Count == currentPositionalOption)) {
                        badUsage("No source file name");
                    }
                    sourceXmlFileName = positional[currentPositionalOption++];
                }
                if ((!(useAssociatedStylesheet) && (styleFileName == null))) {
                    if ((positional.Count<=currentPositionalOption)) {
                        badUsage("No stylesheet file name");
                    }
                    styleFileName = positional[currentPositionalOption++];
                }
                if ((currentPositionalOption<positional.Count)) {
                    badUsage(("Unrecognized option: "+positional[currentPositionalOption]));
                }
                value = options.getOptionValue("scmin");
                if ((value != null)) {
                    config.importComponents(new Saxon.Ejavax.xml.transform.stream.StreamSource(value));
                }
                if ((additionalSchemas != null)) {
                    Saxon.Hej.trans.CommandLineOptions.loadAdditionalSchemas(config, additionalSchemas);
                }
                options.applyStaticParams(compiler);
                System.Collections.Generic.IList<Saxon.Ejavax.xml.transform.Source> sources = new System.Collections.Generic.List<Saxon.Ejavax.xml.transform.Source>();
                if ((sourceXmlFileName != null)) {
                    bool useSAXSource = ((sourceParserName != null) || dtdValidation);
                    wholeDirectory = Saxon.Hej.trans.CommandLineOptions.loadDocuments(sourceXmlFileName, useURLs, processor, useSAXSource, sources);
                    sources = preprocess(sources);
                    if (wholeDirectory) {
                        if ((outputFileName == null)) {
                            quit("To process a directory, -o must be specified", 2);
                        } else {
                            if (outputFileName.Equals(sourceXmlFileName)) {
                                quit("Output directory must be different from input", 2);
                            } else {
                                outputFile = new Saxon.Ejava.io.File(outputFileName);
                                if (!(outputFile.isDirectory())) {
                                    quit("Input is a directory, but output is not", 2);
                                }
                            }
                        }
                    }
                }
                if (((outputFileName != null) && !(wholeDirectory))) {
                    outputFile = new Saxon.Ejava.io.File(outputFileName);
                    if (outputFile.isDirectory()) {
                        quit("Output is a directory, but input is not", 2);
                    }
                }
                if (useAssociatedStylesheet) {
                } else {
                    long startTime = now();
                    bool isURI = (useURLs || Saxon.Hej.trans.CommandLineOptions.isImplicitURI(styleFileName));
                    Saxon.Hej.s9api.XsltExecutable sheet = null;
                    Saxon.Ejavax.xml.transform.Source styleSource = null;
                    if (isURI) {
                        Saxon.Hej.lib.JJ_ResourceRequest request = new Saxon.Hej.lib.JJ_ResourceRequest();
                        request.relativeUri = styleFileName;
                        System.Uri cwd = new Saxon.Ejava.io.File(Saxon.Ejava.lang.System.getProperty("user.dir")).toURI();
                        request.baseUri = cwd.ToString();
                        request.uri = new System.Uri(cwd, styleFileName).ToString();
                        request.nature = Saxon.Hej.lib.JJ_ResourceRequest.XSLT_NATURE;
                        request.purpose = "transformation";
                    } else {
                        if (styleFileName.Equals("-")) {
                            string sysId = new Saxon.Ejava.io.File(Saxon.Ejava.lang.System.getProperty("user.dir")).toURI().OriginalString;
                            styleSource = new Saxon.Ejavax.xml.transform.stream.StreamSource(System.Console.In, sysId);
                        } else {
                            Saxon.Hej.trans.packages.PackageLibrary library = config.getDefaultXsltCompilerInfo().getPackageLibrary();
                            Saxon.Hej.trans.packages.PackageDetails details = library.findDetailsForAlias(styleFileName);
                            if ((details != null)) {
                                Saxon.Hej.s9api.XsltPackage pack = compiler.obtainPackageWithAlias(styleFileName);
                                sheet = pack.link();
                            } else {
                                Saxon.Ejava.io.File sheetFile = new Saxon.Ejava.io.File(styleFileName);
                                if (!(sheetFile.exists())) {
                                    quit((("Stylesheet file "+sheetFile)+" does not exist"), 2);
                                }
                                styleSource = new Saxon.Ejavax.xml.transform.stream.StreamSource(sheetFile.toURI().ToString());
                            }
                        }
                    }
                    if (((styleSource == null) && (sheet == null))) {
                        quit("URIResolver for stylesheet file must return a Source", 2);
                    }
                    if ((sheet == null)) {
                        if ((export && !(run))) {
                            try {
                                Saxon.Hej.s9api.XsltPackage pack = compiler.compilePackage(styleSource);
                                pack.save(new Saxon.Ejava.io.File(exportOutputFileName));
                                if (showTime) {
                                    config.getLogger().info(("Stylesheet exported to: "+new Saxon.Ejava.io.File(exportOutputFileName).getAbsolutePath()));
                                }
                                return ;
                            } catch (Saxon.Api.SaxonApiException err) {
                                quit(err.Message, 2);
                            }
                        }
                        int repeatComp = repeat;
                        if ((repeatComp>20)) {
                            repeatComp = 20;
                        }
                        if ((repeatComp == 1)) {
                            sheet = compiler.compile(styleSource);
                            if (showTime) {
                                long endTime = now();
                                config.getLogger().info(("Stylesheet compilation time: "+Saxon.Hej.trans.Timer.showExecutionTimeNano((endTime-startTime))));
                            }
                        } else {
                            startTime = now();
                            long totalTime = 0;
                            Saxon.Hej.lib.Logger logger = config.getLogger();
                            int threshold = ((logger is Saxon.Callbacks.StandardLogger) ? ((Saxon.Callbacks.StandardLogger)(logger)).getThreshold() : 0);
                            for (int j = -(repeatComp); (j<repeatComp); j++) {
                                if ((j == 0)) {
                                    startTime = now();
                                    if ((logger is Saxon.Callbacks.StandardLogger)) {
                                        ((Saxon.Callbacks.StandardLogger)(logger)).setThreshold(threshold);
                                    }
                                    Saxon.Hej.style.Compilation.TIMING = true;
                                } else {
                                    if ((j<0)) {
                                        if ((logger is Saxon.Callbacks.StandardLogger)) {
                                            ((Saxon.Callbacks.StandardLogger)(logger)).setThreshold(Saxon.Hej.lib.Logger.ERROR);
                                        }
                                        Saxon.Hej.style.Compilation.TIMING = false;
                                    }
                                }
                                sheet = compiler.compile(styleSource);
                                if ((showTime && (j>=0))) {
                                    long endTime = now();
                                    long elapsed = (endTime-startTime);
                                    config.getLogger().info(("Stylesheet compilation time: "+Saxon.Hej.trans.Timer.showExecutionTimeNano(elapsed)));
                                    startTime = endTime;
                                    totalTime += elapsed;
                                }
                            }
                            if (showTime) {
                                config.getLogger().info(("Average compilation time: "+Saxon.Hej.trans.Timer.showExecutionTimeNano((totalTime/repeatComp))));
                            }
                        }
                        if (schemaAware) {
                            int licenseId = sheet.getUnderlyingCompiledStylesheet().getTopLevelPackage().getLocalLicenseId();
                            config.checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT, "schema-aware XSLT", licenseId);
                        }
                    }
                    if (explain) {
                        Saxon.Hej.s9api.Serializer @out;
                        if ((explainOutputFileName == null)) {
                            @out = processor.newSerializer(System.Console.Error);
                        } else {
                            @out = processor.newSerializer(new Saxon.Ejava.io.File(explainOutputFileName));
                        }
                        @out.setOutputProperty(Saxon.Hej.s9api.Serializer.Property.METHOD, "xml");
                        @out.setOutputProperty(Saxon.Hej.s9api.Serializer.Property.INDENT, "yes");
                        @out.setOutputProperty(Saxon.Hej.s9api.Serializer.Property.OMIT_XML_DECLARATION, "yes");
                        if (config.isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION)) {
                            @out.setOutputProperty(Saxon.Hej.s9api.Serializer.Property.SAXON_INDENT_SPACES, "2");
                        }
                        sheet.explain(@out);
                    }
                    if (export) {
                        sheet.export(new System.IO.FileStream(exportOutputFileName, System.IO.FileMode.OpenOrCreate));
                        if (showTime) {
                            config.getLogger().info(("Stylesheet exported to: "+new Saxon.Ejava.io.File(exportOutputFileName).getAbsolutePath()));
                        }
                    }
                    if (run) {
                        try {
                            if (wholeDirectory) {
                                processDirectory(this, sources, sheet, outputFile, options);
                            } else {
                                Saxon.Ejavax.xml.transform.Source source = (((sources == null) || sources.Count==0) ? null : sources[0]);
                                processFile(source, sheet, outputFile, options);
                            }
                        } finally {
                            if ((closeTraceDestination && (traceDestination != null))) {
                                traceDestination.close();
                            }
                        }
                    }
                    if ((options.getOptionValue("TB") != null)) {
                        config.createByteCodeReport(options.getOptionValue("TB"));
                    }
                }
            } catch (Saxon.Hej.expr.instruct.TerminationException err) {
                quit(err.Message, 1);
            } catch (Saxon.Api.SaxonApiException err) {
                quit(err.Message, 2);
            } catch (Saxon.Ejavax.xml.transform.TransformerException err) {
                quit(("Transformation failed: "+err.Message), 2);
            } catch (Saxon.Hej.trans.LicenseException err) {
                quit(("Transformation failed with license problem: "+err.Message), 2);
            } catch (Saxon.Ejavax.xml.transform.TransformerFactoryConfigurationError err) {
                quit(("Transformation failed with configuration problem: "+err.Message), 2);
            } catch (System.Exception err2) {
                System.Console.Error.WriteLine(err2);
                quit(((("Fatal error during transformation: "+err2.GetType().Name)+": ")+(((err2.Message == null) ? " (no message)" : err2.Message))), 2);
            }
        }
        protected internal void initializeConfiguration(Saxon.Hej.Configuration config) {
        }
        public void setFactoryConfiguration(bool schemaAware, string className) {
        }
        internal  /* REPLACING protected internal */ void applyLocalOptions(Saxon.Hej.trans.CommandLineOptions options, Saxon.Hej.Configuration config) {
        }
        public System.Collections.Generic.IList<Saxon.Ejavax.xml.transform.Source> preprocess(System.Collections.Generic.IList<Saxon.Ejavax.xml.transform.Source> sources) {
            return sources;
        }
        protected internal Saxon.Hej.Configuration getConfiguration() {
            return processor.getUnderlyingConfiguration();
        }
        protected internal void quit(string message, int code) {
            try {
                getConfiguration().getLogger().warning(message);
            } catch (System.Exception err) {
                System.Console.Error.WriteLine(message);
            }
            if (allowExit) {
                Saxon.Ejava.lang.System.exit(code);
            } else {
                throw new System.Exception(message);
            }
        }
        private Saxon.Ejava.io.File makeOutputFile(Saxon.Ejava.io.File directory, string localName, Saxon.Hej.s9api.XsltExecutable sheet) {
            string mediaType = sheet.getUnderlyingCompiledStylesheet().getPrimarySerializationProperties().getProperty(Saxon.Ejavax.xml.transform.OutputKeys.MEDIA_TYPE);
            string suffix = ".xml";
            if ("text/html".Equals(mediaType)) {
                suffix = ".html";
            } else {
                if ("text/plain".Equals(mediaType)) {
                    suffix = ".txt";
                }
            }
            string prefix = localName;
            if ((localName.EndsWith(".xml") || localName.EndsWith(".XML"))) {
                prefix = localName.Substring(0, (localName.Length-4) - 0);
            }
            return new Saxon.Ejava.io.File(directory, (prefix+suffix));
        }
        internal  /* REPLACING protected internal */ Saxon.Hej.s9api.Xslt30Transformer newTransformer(Saxon.Hej.s9api.XsltExecutable sheet, Saxon.Hej.trans.CommandLineOptions options) {
            Saxon.Hej.Configuration config = getConfiguration();
            Saxon.Hej.s9api.Xslt30Transformer transformer = sheet.load30();
            transformer.setTraceFunctionDestination(traceDestination);
            string initialMode = options.getOptionValue("im");
            if ((initialMode != null)) {
                transformer.setInitialMode(Saxon.Api.QName.FromClarkName(initialMode));
            }
            string now = options.getOptionValue("now");
            if ((now != null)) {
                try {
                    Saxon.Hej.value.DateTimeValue currentDateTime = (Saxon.Hej.value.DateTimeValue)(Saxon.Hej.value.DateTimeValue.makeDateTimeValue(Saxon.Hej.str.BMPString.of(now), config.getConversionRules()).asAtomic());
                    transformer.getUnderlyingController().setCurrentDateTime(currentDateTime);
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Api.SaxonApiException(("Failed to set current time: "+e.Message), e);
                }
            }
            if ("on".Equals(options.getOptionValue("ea"))) {
                transformer.getUnderlyingController().setAssertionsEnabled(true);
            } else {
                if ("off".Equals(options.getOptionValue("ea"))) {
                    transformer.getUnderlyingController().setAssertionsEnabled(true);
                }
            }
            return transformer;
        }
        protected internal static long now() {
            return Saxon.Ejava.lang.System.nanoTime();
        }
        private static void processDirectory(Saxon.Hej.Transform t, System.Collections.Generic.IList<Saxon.Ejavax.xml.transform.Source> sources, Saxon.Hej.s9api.XsltExecutable sheet, Saxon.Ejava.io.File outputDir, Saxon.Hej.trans.CommandLineOptions options) {
            int failures = 0;
            foreach (Saxon.Ejavax.xml.transform.Source source in sources) {
                string localName = getLocalFileName(source);
                try {
                    Saxon.Ejava.io.File outputFile = t.makeOutputFile(outputDir, localName, sheet);
                    t.processFile(source, sheet, outputFile, options);
                } catch (Saxon.Api.SaxonApiException err) {
                    failures++;
                    t.getConfiguration().getLogger().info((((("While processing "+localName)+": ")+err.Message)+'\n'));
                }
            }
            if ((failures>0)) {
                throw new Saxon.Api.SaxonApiException((((failures+" transformation")+(((failures == 1) ? "" : "s")))+" failed"));
            }
        }
        private static string getLocalFileName(Saxon.Ejavax.xml.transform.Source source) {
            try {
                string path = new System.Uri(source.getSystemId(), System.UriKind.RelativeOrAbsolute).LocalPath;
                while (true) {
                    int sep = path.IndexOf('/');
                    if ((sep<0)) {
                        return path;
                    } else {
                        path = path.Substring((sep+1));
                    }
                }
            } catch (System.UriFormatException err) {
                throw new Saxon.Ejava.lang.IllegalArgumentException(err.Message);
            }
        }
        internal  /* REPLACING protected internal */ void processFile(Saxon.Ejavax.xml.transform.Source source, Saxon.Hej.s9api.XsltExecutable sheet, Saxon.Ejava.io.File outputFile, Saxon.Hej.trans.CommandLineOptions options) {
            long totalTime = 0;
            int runs = 0;
            int halfway = ((repeat/2)-1);
            string jsonSource = options.getOptionValue("json");
            for (int r = 0; (r<repeat); r++) {
                if (showTime) {
                    string msg = "Processing ";
                    if ((source != null)) {
                        msg += source.getSystemId();
                    } else {
                        if ((jsonSource != null)) {
                            msg += ("JSON from "+jsonSource);
                        } else {
                            msg += " (no source document)";
                        }
                    }
                    string initialMode = options.getOptionValue("im");
                    if ((initialMode != null)) {
                        msg += (" initial mode = "+initialMode);
                    }
                    string initialTemplateOption = options.getOptionValue("it");
                    if ((initialTemplateOption != null)) {
                        msg += (" initial template = "+((initialTemplateOption.Length==0 ? "xsl:initial-template" : initialTemplateOption)));
                    }
                    getConfiguration().getLogger().info(msg);
                }
                long startTime = now();
                if ((r == halfway)) {
                    runs = 0;
                    totalTime = 0;
                }
                runs++;
                if ((r<halfway)) {
                    traceDestination = null;
                }
                Saxon.Hej.s9api.Xslt30Transformer transformer = newTransformer(sheet, options);
                Saxon.Hej.s9api.Serializer serializer;
                if ((outputFile == null)) {
                    transformer.setBaseOutputURI(new Saxon.Ejava.io.File(Saxon.Ejava.lang.System.getProperty("user.dir")).toURI().OriginalString);
                    serializer = processor.newSerializer(System.Console.Out);
                } else {
                    serializer = processor.newSerializer(outputFile);
                }
                try {
                    options.setSerializationProperties(serializer);
                } catch (Saxon.Ejava.lang.IllegalArgumentException e) {
                    quit(e.Message, 2);
                }
                bool buildResultTree;
                string buildTreeProperty = serializer.getOutputProperty(Saxon.Hej.s9api.Serializer.Property.BUILD_TREE);
                if ("yes".Equals(buildTreeProperty)) {
                    buildResultTree = true;
                } else {
                    if ("no".Equals(buildTreeProperty)) {
                        buildResultTree = false;
                    } else {
                        string method = serializer.getOutputProperty(Saxon.Hej.s9api.Serializer.Property.METHOD);
                        buildResultTree = !((("json".Equals(method) || "adaptive".Equals(method))));
                    }
                }
                string initialTemplate = options.getOptionValue("it");
                if ((source != null)) {
                    Saxon.Hej.PreparedStylesheet pss = sheet.getUnderlyingCompiledStylesheet();
                    Saxon.Hej.expr.instruct.GlobalContextRequirement requirement = pss.getGlobalContextRequirement();
                    bool buildSourceTree;
                    if ((requirement == null)) {
                        buildSourceTree = ((initialTemplate != null) || !(transformer.getUnderlyingController().getInitialMode().isDeclaredStreamable()));
                    } else {
                        buildSourceTree = !(requirement.isAbsentFocus());
                    }
                    if (buildSourceTree) {
                        Saxon.Hej.s9api.DocumentBuilder builder = processor.newDocumentBuilder();
                        builder.setDTDValidation(getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.DTD_VALIDATION));
                        builder.setWhitespaceStrippingPolicy(sheet.getWhitespaceStrippingPolicy());
                        if (getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.DTD_VALIDATION_RECOVERABLE)) {
                            source = new Saxon.Hej.lib.AugmentedSource(source, getConfiguration().getParseOptions());
                        }
                        Saxon.Hej.style.StylesheetPackage top = pss.getTopLevelPackage();
                        if (top.isStripsTypeAnnotations()) {
                            source = Saxon.Hej.lib.AugmentedSource.makeAugmentedSource(source);
                            ((Saxon.Hej.lib.AugmentedSource)(source)).getParseOptions().addFilter(receiver => getConfiguration().getAnnotationStripper(receiver));
                        }
                        Saxon.Api.XdmNode node = builder.build(source);
                        transformer.setGlobalContextItem(node, true);
                        source = xdmNodeAsSource(node);
                    }
                }
                Saxon.Api.XdmItem jsonContextItem = null;
                if ((jsonSource != null)) {
                    try {
                        System.IO.TextReader jsonReader = new System.IO.StreamReader(System.IO.File.OpenRead(jsonSource), System.Text.Encoding.UTF8);
                        Saxon.Api.XdmValue jsonTree = Saxon.Hej.Query.parseJson(processor, jsonReader);
                        if (!(jsonTree.Count==0)) {
                            transformer.setGlobalContextItem(jsonContextItem = jsonTree.ItemAt(0));
                        }
                    } catch (System.IO.IOException e) {
                        throw new Saxon.Api.SaxonApiException(("Cannot read JSON input: "+e.Message));
                    }
                }
                options.applyFileParams(processor, transformer);
                if ((initialTemplate != null)) {
                    Saxon.Api.QName initialTemplateName;
                    if (initialTemplate.Length==0) {
                        initialTemplateName = new Saxon.Api.QName("xsl", Saxon.Hej.lib.NamespaceConstant.XSLT, "initial-template");
                    } else {
                        initialTemplateName = Saxon.Api.QName.FromClarkName(initialTemplate);
                    }
                    if (buildResultTree) {
                        transformer.callTemplate(initialTemplateName, serializer);
                    } else {
                        Saxon.Api.XdmValue result = transformer.callTemplate(initialTemplateName);
                        serializer.serializeXdmValue(result);
                    }
                } else {
                    if ((jsonContextItem != null)) {
                        if (buildResultTree) {
                            transformer.applyTemplates(jsonContextItem, serializer);
                        } else {
                            Saxon.Api.XdmValue result = transformer.applyTemplates(jsonContextItem);
                            serializer.serializeXdmValue(result);
                        }
                    } else {
                        if (buildResultTree) {
                            transformer.applyTemplates(source, serializer);
                        } else {
                            Saxon.Api.XdmValue result = transformer.applyTemplates(source);
                            serializer.serializeXdmValue(result);
                        }
                    }
                }
                long endTime = now();
                totalTime += (endTime-startTime);
                if (showTime) {
                    getConfiguration().getLogger().info(("Execution time: "+Saxon.Hej.trans.Timer.showExecutionTimeNano((endTime-startTime))));
                    getConfiguration().getLogger().info(Saxon.Hej.trans.Timer.showMemoryUsed());
                    if ((repeat>1)) {
                        getConfiguration().getLogger().info("-------------------------------");
                    }
                    if (Saxon.Hej.trace.Instrumentation.ACTIVE) {
                        #pragma warning disable 0162
                        Saxon.Hej.trace.Instrumentation.report();
                        Saxon.Hej.trace.Instrumentation.reset();
                        #pragma warning restore 0162
                    }
                }
                if (((repeat == 999999) && (totalTime>60000))) {
                    break ;
                }
            }
            if ((repeat>1)) {
                getConfiguration().getLogger().info(((("*** Average execution time over last "+runs)+" runs: ")+Saxon.Hej.trans.Timer.showExecutionTimeNano((totalTime/runs))));
            }
        }
        private Saxon.Ejavax.xml.transform.Source xdmNodeAsSource(Saxon.Api.XdmNode node) {
            return node.Implementation.asActiveSource();
        }
        protected internal void badUsage(string message) {
            Saxon.Hej.lib.Logger logger = getConfiguration().getLogger();
            if (!("".Equals(message))) {
                logger.error(message);
            }
            if (!(showTime)) {
                logger.info(getConfiguration().getProductTitle());
            }
            logger.info("Usage: see http://www.saxonica.com/documentation/index.html#!using-xsl/commandline");
            logger.info((("Format: "+Saxon.Hej.trans.CommandLineOptions.getCommandName(this))+" options params"));
            Saxon.Hej.trans.CommandLineOptions options = new Saxon.Hej.trans.CommandLineOptions();
            setPermittedOptions(options);
            logger.info(("Options available:"+options.displayPermittedOptions()));
            logger.info("Use -XYZ:? for details of option XYZ");
            logger.info("Params: ");
            logger.info("  param=value           Set stylesheet string parameter");
            logger.info("  +param=filename       Set stylesheet document parameter");
            logger.info("  ?param=expression     Set stylesheet parameter using XPath");
            logger.info("  !param=value          Set serialization parameter");
            if (allowExit) {
                if ("".Equals(message)) {
                    Saxon.Ejava.lang.System.exit(0);
                } else {
                    Saxon.Ejava.lang.System.exit(2);
                }
            } else {
                throw new System.Exception(message);
            }
        }
    
    }
}