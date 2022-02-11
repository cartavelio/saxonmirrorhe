using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej {
    public class Validate {
        private Saxon.Hej.s9api.Processor processor;
        private Saxon.Eej.config.EnterpriseConfiguration config;
        private bool useURLs = false;
        private bool showTime = false;
        private int repeat = 1;
        private string validationMode = "strict";
        private int xsdLanguageVersion = Saxon.Hej.Configuration.XSD11;
        private string topLevelElementName = null;
        private bool allowExit = true;
        public static void main(string[] args) {
            Saxon.Eej.Validate validate = new Saxon.Eej.Validate();
            int rc = validate.doValidate(args);
            if (((rc != 0) && validate.allowExit)) {
                Saxon.Ejava.lang.System.exit(rc);
            }
        }
        protected internal void setConfiguration() {
            config = new Saxon.Eej.config.EnterpriseConfiguration();
        }
        internal  /* REPLACING protected internal */ void setPermittedOptions(Saxon.Hej.trans.CommandLineOptions options) {
            options.addRecognizedOption("config", ((Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED)), "Use specified configuration file");
            options.addRecognizedOption("dtd", Saxon.Hej.trans.CommandLineOptions.TYPE_ENUMERATION, "Validate using DTD");
            options.setPermittedValues("dtd", new string[]{"on", "off", "recover"}, "on");
            options.addRecognizedOption("export", (Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Schema output file in SCM format");
            options.addRecognizedOption("limits", Saxon.Hej.trans.CommandLineOptions.TYPE_INTEGER_PAIR, "Limits for correct processing of minOccurs and maxOccurs");
            options.addRecognizedOption("opt", (Saxon.Hej.trans.CommandLineOptions.TYPE_INTEGER|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Use optimization level 0..10");
            options.addRecognizedOption("report", (Saxon.Hej.trans.CommandLineOptions.TYPE_CLASSNAME|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Report XML file name for validation error reporting");
            options.addRecognizedOption("s", (Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME_LIST|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "List of source files to be validated");
            options.addRecognizedOption("scmin", (Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Schema input file in SCM format");
            options.addRecognizedOption("scmout", (Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Retained synonym of -export");
            options.addRecognizedOption("stats", (Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Statistics output file");
            options.addRecognizedOption("t", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Display version and timing information, and names of schema documents loaded");
            options.addRecognizedOption("top", Saxon.Hej.trans.CommandLineOptions.TYPE_QNAME, "QName of required outermost element node");
            options.addRecognizedOption("u", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Interpret filename arguments as URIs");
            options.setPermittedValues("u", new string[]{"on", "off"}, "on");
            options.addRecognizedOption("val", (Saxon.Hej.trans.CommandLineOptions.TYPE_ENUMERATION|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Apply strict or lax validation");
            options.setPermittedValues("val", new string[]{"strict", "lax"}, "strict");
            options.addRecognizedOption("xsd", (Saxon.Hej.trans.CommandLineOptions.TYPE_FILENAME_LIST|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "List of schema documents to be preloaded");
            options.addRecognizedOption("xsdversion", (Saxon.Hej.trans.CommandLineOptions.TYPE_ENUMERATION|Saxon.Hej.trans.CommandLineOptions.VALUE_REQUIRED), "Indicate whether XSD 1.1 is supported");
            options.setPermittedValues("xsdversion", new string[]{"1.0", "1.1"}, null);
            options.addRecognizedOption("xsiloc", Saxon.Hej.trans.CommandLineOptions.TYPE_BOOLEAN, "Load schemas named in xsi:schemaLocation (default on)");
            options.addRecognizedOption("?", Saxon.Hej.trans.CommandLineOptions.VALUE_PROHIBITED, "Display command line help text");
        }
        public int doValidate(string[] args) {
            System.Collections.Generic.IList<string> sourceFileNames = new System.Collections.Generic.List<string>();
            System.Collections.Generic.IList<string> schemaFileNames = new System.Collections.Generic.List<string>();
            string scmInput;
            string scmOutput;
            int validCount = 0;
            int invalidCount = 0;
            int unreadableCount = 0;
            string reportFileName;
            Saxon.Hej.lib.InvalidityReportGenerator reportHandler;
            Saxon.Hej.trans.CommandLineOptions options = new Saxon.Hej.trans.CommandLineOptions();
            setPermittedOptions(options);
            try {
                options.setActualOptions(args);
            } catch (Saxon.Hej.trans.XPathException err) {
                return quit(err.Message, 2);
            }
            string configFile = options.getOptionValue("config");
            if ((configFile != null)) {
                try {
                    Saxon.Hej.Configuration fconfig = Saxon.Hej.Configuration.readConfiguration(new Saxon.Ejavax.xml.transform.stream.StreamSource(configFile));
                    if (!(((fconfig is Saxon.Eej.config.EnterpriseConfiguration)))) {
                        return quit((("Configuration produced from the configuration file is an instance of "+fconfig.GetType())+" (not EnterpriseConfiguration): check the value of configuration/@edition"), 2);
                    }
                    config = (Saxon.Eej.config.EnterpriseConfiguration)(fconfig);
                } catch (Saxon.Hej.trans.XPathException e) {
                    return quit(e.Message, 2);
                }
            }
            if ((config == null)) {
                setConfiguration();
            }
            processor = new Saxon.Hej.s9api.Processor(config);
            try {
                options.applyToConfiguration(processor);
                allowExit = !("off".Equals(options.getOptionValue("quit")));
                string value = options.getOptionValue("?");
                if ((value != null)) {
                    badUsage("");
                    return 0;
                }
                value = options.getOptionValue("limits");
                if ((value != null)) {
                    int comma = value.IndexOf(',');
                    if ((comma<1)) {
                        badUsage("format: -limits:min,max");
                        return 2;
                    }
                    int min = System.Int32.Parse(value.Substring(0, comma - 0));
                    int max = System.Int32.Parse(value.Substring((comma+1)));
                    config.setOccurrenceLimits(min, max);
                }
                scmInput = options.getOptionValue("scmin");
                scmOutput = options.getOptionValue("export");
                if ((scmOutput == null)) {
                    scmOutput = options.getOptionValue("scmout");
                }
                value = options.getOptionValue("s");
                if ((value != null)) {
                    Saxon.Ejava.util.StringTokenizer tokenizer = new Saxon.Ejava.util.StringTokenizer(value, System.IO.Path.PathSeparator.ToString());
                    while (tokenizer.hasMoreTokens()) {
                        sourceFileNames.Add(tokenizer.nextToken());
                    }
                }
                value = options.getOptionValue("t");
                if ((value != null)) {
                    processor.setConfigurationProperty(Saxon.Hej.lib.Feature<bool>.TIMING, true);
                    showTime = true;
                }
                topLevelElementName = options.getOptionValue("top");
                useURLs = "on".Equals(options.getOptionValue("u"));
                value = options.getOptionValue("val");
                if ((value != null)) {
                    validationMode = value;
                }
                value = options.getOptionValue("xsd");
                if ((value != null)) {
                    Saxon.Ejava.util.StringTokenizer tokenizer = new Saxon.Ejava.util.StringTokenizer(value, ";");
                    while (tokenizer.hasMoreTokens()) {
                        schemaFileNames.Add(tokenizer.nextToken());
                    }
                }
                xsdLanguageVersion = ("1.1".Equals(processor.getConfigurationProperty(Saxon.Hej.lib.Feature<string>.XSD_VERSION)) ? Saxon.Hej.Configuration.XSD11 : Saxon.Hej.Configuration.XSD10);
                Saxon.Impl.Helpers.ListUtils.AddAll(sourceFileNames, options.getPositionalOptions());
                applyLocalOptions(options, config);
                if ((!(schemaFileNames.Count==0) && (scmInput != null))) {
                    badUsage("Cannot mix xsd and scm input");
                    return 2;
                }
                if (((schemaFileNames.Count==0 && (scmInput == null)) && sourceFileNames.Count==0)) {
                    badUsage("No action requested");
                    return 2;
                }
                if (showTime) {
                    config.getLogger().info(config.getProductTitle());
                    config.getLogger().info(Saxon.Hej.Version.platform.getPlatformVersion());
                }
                config.displayLicenseMessage();
                processor.setConfigurationProperty(Saxon.Hej.lib.Feature<string>.SCHEMA_VALIDATION_MODE, validationMode);
                long startTime = now();
                Saxon.Hej.s9api.SchemaManager manager = processor.getSchemaManager();
                foreach (string schemaFileName in schemaFileNames) {
                    Saxon.Ejavax.xml.transform.Source schemaSource;
                    if ((useURLs || Saxon.Hej.trans.CommandLineOptions.isImplicitURI(schemaFileName))) {
                        schemaSource = config.getURIResolver().resolve(schemaFileName, null);
                        if ((schemaSource == null)) {
                            schemaSource = config.getSystemURIResolver().resolve(schemaFileName, null);
                        }
                    } else {
                        if (schemaFileName.Equals("-")) {
                            schemaSource = new Saxon.Ejavax.xml.transform.stream.StreamSource(System.Console.In);
                        } else {
                            Saxon.Ejava.io.File schemaFile = new Saxon.Ejava.io.File(schemaFileName);
                            if (!(schemaFile.exists())) {
                                return quit((("Schema file "+schemaFile)+" does not exist"), 2);
                            }
                            schemaSource = new Saxon.Ejavax.xml.transform.stream.StreamSource(schemaFile.toURI().ToString());
                        }
                    }
                    if ((schemaSource == null)) {
                        return quit("URIResolver for schema file must return a Source", 2);
                    }
                    try {
                        manager.load(schemaSource);
                    } catch (Saxon.Api.SaxonApiException e) {
                        if ((e.InnerException is Saxon.Hej.type.SchemaException)) {
                            throw (Saxon.Hej.type.SchemaException)(e.InnerException);
                        } else {
                            throw;
                        }
                    }
                }
                if ((scmInput != null)) {
                    Saxon.Ejavax.xml.transform.Source scm = new Saxon.Ejavax.xml.transform.stream.StreamSource(new Saxon.Ejava.io.File(scmInput));
                    manager.importComponents(scm);
                }
                if ((showTime && !(schemaFileNames.Count==0))) {
                    long endTime = now();
                    config.getLogger().info((("Schema checking successful. Time: "+(((endTime-startTime))/1000000))+"ms."));
                    config.getLogger().info(Saxon.Hej.trans.Timer.showMemoryUsed());
                }
                Saxon.Hej.s9api.SchemaValidator validator = manager.newSchemaValidator();
                validator.setLax("lax".Equals(validationMode));
                if ((topLevelElementName != null)) {
                    validator.setDocumentElementName(Saxon.Api.QName.FromClarkName(topLevelElementName));
                }
                if (options.definesParameterValues()) {
                    options.setParams(processor, (name, val) => validator.setParameter(name, val));
                }
                Saxon.Hej.s9api.Serializer statsSerializer = null;
                value = options.getOptionValue("stats");
                if ((value != null)) {
                    validator.setCollectStatistics(true);
                    Saxon.Ejava.io.File statsFile = new Saxon.Ejava.io.File(value);
                    statsSerializer = processor.newSerializer(statsFile);
                    statsSerializer.setOutputProperty(Saxon.Hej.s9api.Serializer.Property.METHOD, "xml");
                    statsSerializer.setOutputProperty(Saxon.Hej.s9api.Serializer.Property.INDENT, "yes");
                }
                value = options.getOptionValue("report");
                if ((value != null)) {
                    reportFileName = value;
                    Saxon.Hej.s9api.Serializer reportDestination = processor.newSerializer();
                    reportDestination.setOutputProperty(Saxon.Hej.s9api.Serializer.Property.INDENT, "yes");
                    reportDestination.setOutputFile(new Saxon.Ejava.io.File(reportFileName));
                    validator.setValidityReporting(reportDestination);
                    reportHandler = (Saxon.Hej.lib.InvalidityReportGenerator)(validator.getInvalidityHandler());
                    string reportSchemaNames = options.getOptionValue("xsd");
                    if ((reportSchemaNames != null)) {
                        reportHandler.setSchemaName(reportSchemaNames);
                    }
                    string reportXsdVersion = options.getOptionValue("xsdversion");
                    if ((reportXsdVersion != null)) {
                        reportHandler.setXsdVersion(reportXsdVersion);
                    }
                } else {
                    validator.setInvalidityHandler(new Saxon.Hej.lib.StandardInvalidityHandler(config));
                }
                if ((sourceFileNames.Count>1)) {
                    System.Collections.Generic.IList<Saxon.Ejavax.xml.transform.Source> sources = new System.Collections.Generic.List<Saxon.Ejavax.xml.transform.Source>(sourceFileNames.Count);
                    foreach (string sourceFileName in sourceFileNames) {
                        Saxon.Ejavax.xml.transform.Source source = loadDocument(sourceFileName, useURLs, config);
                        sources.Add(source);
                    }
                    validator.validateMultiple(sources);
                } else {
                    foreach (string sourceFileName in sourceFileNames) {
                        try {
                            Saxon.Ejavax.xml.transform.Source source = loadDocument(sourceFileName, useURLs, config);
                            processFile(source, validator);
                            if (showTime) {
                                config.getLogger().info("Validation successful");
                            }
                            validCount++;
                        } catch (Saxon.Api.SaxonApiException e) {
                            if ((e.InnerException is Saxon.Hej.type.ValidationException)) {
                                if (showTime) {
                                    config.getLogger().warning("Validation complete: errors found");
                                } else {
                                    config.getLogger().warning((("Validation of file "+sourceFileName)+" completed: errors found"));
                                }
                                invalidCount++;
                            } else {
                                config.getLogger().error(((("Failed to process file "+sourceFileName)+": ")+e.Message));
                                unreadableCount++;
                            }
                        }
                    }
                    if ((showTime && ((validCount+invalidCount)>1))) {
                        config.getLogger().info((((((((validCount+" valid document")+(((validCount == 1) ? "" : "s")))+"; ")+invalidCount)+" invalid document")+(((invalidCount == 1) ? "" : "s")))+(((unreadableCount == 0) ? "" : ((("; "+unreadableCount)+" unreadable or ill-formed document")+(((unreadableCount == 1) ? "" : "s")))))));
                    }
                }
                if ((statsSerializer != null)) {
                    validator.reportValidationStatistics(statsSerializer);
                }
                Saxon.Hej.trace.Instrumentation.report();
                if ((scmOutput != null)) {
                    Saxon.Ejava.io.File scmOutputFile = new Saxon.Ejava.io.File(scmOutput);
                    Saxon.Hej.s9api.Serializer scmSerializer = processor.newSerializer(scmOutputFile);
                    scmSerializer.setOutputProperty(Saxon.Hej.s9api.Serializer.Property.METHOD, "xml");
                    scmSerializer.setOutputProperty(Saxon.Hej.s9api.Serializer.Property.INDENT, "yes");
                    manager.exportComponents(scmSerializer);
                    if (showTime) {
                        config.getLogger().info(("Schema component model written to "+scmOutputFile.getAbsolutePath()));
                    }
                }
            } catch (Saxon.Hej.type.SchemaException err) {
                return quit(("Schema processing failed: "+err.Message), 2);
            } catch (Saxon.Hej.type.ValidationException err) {
                config.reportFatalError(err);
                return quit("Validation of source document failed", 2);
            } catch (Saxon.Hej.trans.XPathException err) {
                config.reportFatalError(err);
                return quit("Processing failed", 2);
            } catch (Saxon.Ejavax.xml.transform.TransformerFactoryConfigurationError err) {
                return quit(("Validation failed: "+err.Message), 2);
            } catch (System.Exception err2) {
                System.Console.Error.WriteLine(err2);
                return quit(("Fatal error during validation: "+err2.Message), 2);
            }
            if ((unreadableCount>0)) {
                return 2;
            } else {
                if ((invalidCount>0)) {
                    return 1;
                } else {
                    return 0;
                }
            }
        }
        internal  /* REPLACING protected internal */ void applyLocalOptions(Saxon.Hej.trans.CommandLineOptions options, Saxon.Hej.Configuration config) {
        }
        protected internal int quit(string message, int code) {
            config.getLogger().info(message);
            return code;
        }
        public static Saxon.Ejavax.xml.transform.Source loadDocument(string sourceFileName, bool useURLs, Saxon.Hej.Configuration config) {
            Saxon.Ejavax.xml.transform.Source sourceInput;
            if ((useURLs || Saxon.Hej.trans.CommandLineOptions.isImplicitURI(sourceFileName))) {
                try {
                    sourceInput = config.getURIResolver().resolve(sourceFileName, null);
                } catch (Saxon.Ejavax.xml.transform.TransformerException e) {
                    throw Saxon.Hej.trans.XPathException.makeXPathException(e);
                }
                if ((sourceInput == null)) {
                    sourceInput = config.getSystemURIResolver().resolve(sourceFileName, null);
                }
            } else {
                if (sourceFileName.Equals("-")) {
                    sourceInput = new Saxon.Ejavax.xml.transform.stream.StreamSource(System.Console.In);
                } else {
                    Saxon.Ejava.io.File sourceFile = new Saxon.Ejava.io.File(sourceFileName);
                    if (!(sourceFile.exists())) {
                        throw new Saxon.Hej.trans.XPathException((("Source file "+sourceFile)+" does not exist"));
                    }
                    sourceInput = new Saxon.Ejavax.xml.transform.stream.StreamSource(sourceFile.toURI().ToString());
                }
            }
            return sourceInput;
        }
        internal  /* REPLACING public */ void processFile(Saxon.Ejavax.xml.transform.Source source, Saxon.Hej.s9api.SchemaValidator validator) {
            if (showTime) {
                config.getLogger().info(("Processing "+source.getSystemId()));
            }
            long startTime = now();
            for (int r = 0; (r<repeat); r++) {
                validator.validate(source);
            }
            if (showTime) {
                long endTime = now();
                config.getLogger().info((("Validation time: "+(((endTime-startTime))/1000000))+"ms."));
                config.getLogger().info(Saxon.Hej.trans.Timer.showMemoryUsed());
            }
        }
        protected internal static long now() {
            return Saxon.Ejava.lang.System.nanoTime();
        }
        protected internal void badUsage(string message) {
            if (!("".Equals(message))) {
                config.getLogger().error(message);
            }
            if (!(showTime)) {
                config.getLogger().info(config.getProductTitle());
            }
            config.getLogger().info("Usage: see http://www.saxonica.com/documentation/index.html#!schema-processing/commandline");
            config.getLogger().info((("Format: "+Saxon.Hej.trans.CommandLineOptions.getCommandName(this))+" options"));
            Saxon.Hej.trans.CommandLineOptions options = new Saxon.Hej.trans.CommandLineOptions();
            setPermittedOptions(options);
            config.getLogger().info(("Options available:"+options.displayPermittedOptions()));
            config.getLogger().info("Use -XYZ:? for details of option XYZ");
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