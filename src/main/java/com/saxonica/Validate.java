package com.saxonica;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.Configuration;
import net.sf.saxon.Version;
import net.sf.saxon.lib.*;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trace.Instrumentation;
import net.sf.saxon.trans.CommandLineOptions;
import net.sf.saxon.trans.Timer;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharp;
import net.sf.saxon.transpile.CSharpModifiers;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.ValidationException;
import org.xml.sax.InputSource;
import org.xml.sax.XMLReader;

import javax.xml.transform.Source;
import javax.xml.transform.TransformerFactoryConfigurationError;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.net.URI;
import java.util.ArrayList;
import java.util.List;
import java.util.StringTokenizer;

/**
 * This <b>Validate</b> class provides a command line interface that allows validation of a source
 * document against a schema, and/or checking that a source schema is a valid schema.
 * <p>Apart from error and optional progress messages, no output is produced.</p>
 */

public class Validate {

    private Processor processor;
    private EnterpriseConfiguration config;
    private boolean useURLs = false;
    private boolean showTime = false;
    private int repeat = 1;   // for performance testing
    private String validationMode = "strict";
    private int xsdLanguageVersion = Configuration.XSD11;
    private String topLevelElementName = null;
    private boolean allowExit = true;

    /**
     * Main program, can be used directly from the command line.
     *
     * @param args List of arguments supplied on operating system command line
     */

    public static void main(String[] args) {
        // the real work is delegated to another routine so that it can be used in a subclass
        final Validate validate = new Validate();
        int rc = validate.doValidate(args);
        if (rc != 0 && validate.allowExit) {
            System.exit(rc);
        }
    }

    /**
     * Create the configuration. This method is intended to be overridden in a subclass
     */

    protected void setConfiguration() {
        config = new EnterpriseConfiguration();
    }

    /**
     * Set the options that are recognized on the command line. This method can be overridden in a subclass
     * to define additional command line options.
     *
     * @param options the CommandLineOptions in which the recognized options are to be registered.
     */
    @CSharpModifiers(code = {"internal"})
    protected void setPermittedOptions(/*@NotNull*/ CommandLineOptions options) {
        options.addRecognizedOption("config", (CommandLineOptions.TYPE_FILENAME | CommandLineOptions.VALUE_REQUIRED),
                "Use specified configuration file");
        //#if CSHARP==false
        options.addRecognizedOption("catalog", CommandLineOptions.TYPE_FILENAME | CommandLineOptions.VALUE_REQUIRED,
                "Use specified catalog file to resolve URIs");
        //#endif
        options.addRecognizedOption("dtd", CommandLineOptions.TYPE_ENUMERATION,
                "Validate using DTD");
        options.setPermittedValues("dtd", new String[]{"on", "off", "recover"}, "on");
        options.addRecognizedOption("export", CommandLineOptions.TYPE_FILENAME | CommandLineOptions.VALUE_REQUIRED,
                                    "Schema output file in SCM format");
        //#if CSHARP==false
        options.addRecognizedOption("ext", CommandLineOptions.TYPE_BOOLEAN,
                "Allow calls to Java extension functions");
        //#endif
        options.addRecognizedOption("init", CommandLineOptions.TYPE_CLASSNAME,
                "User-supplied code to initialize the Saxon configuration");
        options.addRecognizedOption("limits", CommandLineOptions.TYPE_INTEGER_PAIR,
                "Limits for correct processing of minOccurs and maxOccurs");
        options.addRecognizedOption("opt", CommandLineOptions.TYPE_INTEGER | CommandLineOptions.VALUE_REQUIRED,
                "Use optimization level 0..10");
        //#if CSHARP==false
        options.addRecognizedOption("quit", CommandLineOptions.TYPE_BOOLEAN | CommandLineOptions.VALUE_REQUIRED,
                "Quit JVM if execution fails");
        options.addRecognizedOption("r", CommandLineOptions.TYPE_CLASSNAME | CommandLineOptions.VALUE_REQUIRED,
                "Use named URIResolver class");
        //#endif
        options.addRecognizedOption("report", CommandLineOptions.TYPE_CLASSNAME | CommandLineOptions.VALUE_REQUIRED,
                "Report XML file name for validation error reporting");
        options.addRecognizedOption("s", CommandLineOptions.TYPE_FILENAME_LIST | CommandLineOptions.VALUE_REQUIRED,
                "List of source files to be validated");
        options.addRecognizedOption("scmin", CommandLineOptions.TYPE_FILENAME | CommandLineOptions.VALUE_REQUIRED,
                "Schema input file in SCM format");
        options.addRecognizedOption("scmout", CommandLineOptions.TYPE_FILENAME | CommandLineOptions.VALUE_REQUIRED,
                                    "Retained synonym of -export");
        options.addRecognizedOption("stats", CommandLineOptions.TYPE_FILENAME | CommandLineOptions.VALUE_REQUIRED,
                "Statistics output file");
        options.addRecognizedOption("t", CommandLineOptions.TYPE_BOOLEAN,
                "Display version and timing information, and names of schema documents loaded");
        options.addRecognizedOption("top", CommandLineOptions.TYPE_QNAME,
                "QName of required outermost element node");
        options.addRecognizedOption("u", CommandLineOptions.TYPE_BOOLEAN,
                "Interpret filename arguments as URIs");
        options.setPermittedValues("u", new String[]{"on", "off"}, "on");
        options.addRecognizedOption("val", CommandLineOptions.TYPE_ENUMERATION | CommandLineOptions.VALUE_REQUIRED,
                "Apply strict or lax validation");
        options.setPermittedValues("val", new String[]{"strict", "lax"}, "strict");
        //#if CSHARP==false
        options.addRecognizedOption("x", CommandLineOptions.TYPE_CLASSNAME | CommandLineOptions.VALUE_REQUIRED,
                "Use named XMLReader class for parsing source documents");
        options.addRecognizedOption("xi", CommandLineOptions.TYPE_BOOLEAN,
                "Expand XInclude directives in source documents");
        options.addRecognizedOption("xmlversion", CommandLineOptions.TYPE_ENUMERATION | CommandLineOptions.VALUE_REQUIRED,
                "Indicate whether XML 1.1 is supported");
        options.setPermittedValues("xmlversion", new String[]{"1.0", "1.1"}, null);
        //#endif
        options.addRecognizedOption("xsd", CommandLineOptions.TYPE_FILENAME_LIST | CommandLineOptions.VALUE_REQUIRED,
                "List of schema documents to be preloaded");
        options.addRecognizedOption("xsdversion", CommandLineOptions.TYPE_ENUMERATION | CommandLineOptions.VALUE_REQUIRED,
                "Indicate whether XSD 1.1 is supported");
        options.setPermittedValues("xsdversion", new String[]{"1.0", "1.1"}, null);
        options.addRecognizedOption("xsiloc", CommandLineOptions.TYPE_BOOLEAN,
                "Load schemas named in xsi:schemaLocation (default on)");
        //#if CSHARP==false
        options.addRecognizedOption("y", CommandLineOptions.TYPE_CLASSNAME | CommandLineOptions.VALUE_REQUIRED,
                "Use named XMLReader class for parsing schema documents");
        //#endif
        options.addRecognizedOption("?", CommandLineOptions.VALUE_PROHIBITED,
                "Display command line help text");

    }


    /**
     * Support method for main program. This support method can also be invoked from subclasses
     * that support the same command line interface. It also provides a surrogate for the command line
     * interface that is amenable to JUnit testing. Calling this method never throws a checked Exception
     * and never does a System.exit();
     *
     * @param args the command-line arguments
     * @return a return code to be returned. Zero indicates success, +1 indicates that validation completed
     *         successfully and found the schema or instance to be invalid, +2 indicates that validation was not
     *         carried out successfully.
     */

    public int doValidate(String[] args) {

        List<String> sourceFileNames = new ArrayList<>();
        List<String> schemaFileNames = new ArrayList<>();
        String scmInput;
        String scmOutput;
        int validCount = 0;
        int invalidCount = 0;
        int unreadableCount = 0;
        String reportFileName;
        InvalidityReportGenerator reportHandler;
        CommandLineOptions options = new CommandLineOptions();
        setPermittedOptions(options);
        try {
            options.setActualOptions(args);
        } catch (XPathException err) {
            return quit(err.getMessage(), 2);
        }

        String configFile = options.getOptionValue("config");
        if (configFile != null) {
            try {
                Configuration fconfig = Configuration.readConfiguration(new StreamSource(configFile));
                if (!(fconfig instanceof EnterpriseConfiguration)) {
                    return quit("Configuration produced from the configuration file is an instance of " +
                            fconfig.getClass() + " (not EnterpriseConfiguration): check the value of configuration/@edition", 2);
                }
                config = (EnterpriseConfiguration) fconfig;
            } catch (XPathException e) {
                return quit(e.getMessage(), 2);
            }
        }

        // Check the command-line arguments.

        if (config == null) {
            setConfiguration();
        }
        processor = new Processor(config);
        //config.setHostLanguage(Configuration.XML_SCHEMA);

        try {

            // Apply those options which simply update the Configuration

            options.applyToConfiguration(processor);

            // Apply options that are processed locally

            allowExit = !"off".equals(options.getOptionValue("quit"));

            String value = options.getOptionValue("?");
            if (value != null) {
                badUsage("");
                return 0;
            }

            value = options.getOptionValue("limits");
            if (value != null) {
                int comma = value.indexOf(',');
                if (comma < 1) {
                    badUsage("format: -limits:min,max");
                    return 2;
                }
                try {
                    int min = Integer.parseInt(value.substring(0, comma));
                    int max = Integer.parseInt(value.substring(comma + 1));
                    config.setOccurrenceLimits(min, max);
                } catch (NumberFormatException e) {
                    badUsage("format: -limits:min,max where min and max are integers");
                }
            }

            scmInput = options.getOptionValue("scmin");
            scmOutput = options.getOptionValue("export");
            if (scmOutput == null) {
                scmOutput = options.getOptionValue("scmout");
            }

            value = options.getOptionValue("s");
            if (value != null) {
                String delim = File.pathSeparator;
                if (useURLs || CommandLineOptions.isImplicitURI(value)) {
                    delim = " \t\n\r\f";
                }
                StringTokenizer tokenizer = new StringTokenizer(value, delim);
                while (tokenizer.hasMoreTokens()) {
                    sourceFileNames.add(tokenizer.nextToken());
                }
            }

            value = options.getOptionValue("t");
            if (value != null) {
                processor.setConfigurationProperty(Feature.TIMING, true);
                showTime = true;
            }

            topLevelElementName = options.getOptionValue("top");
            useURLs = "on".equals(options.getOptionValue("u"));

            value = options.getOptionValue("val");
            if (value != null) {
                validationMode = value;
            }

            value = options.getOptionValue("xsd");
            if (value != null) {
                StringTokenizer tokenizer = new StringTokenizer(value, ";");
                while (tokenizer.hasMoreTokens()) {
                    schemaFileNames.add(tokenizer.nextToken());
                }
            }


            xsdLanguageVersion = "1.1".equals(processor.getConfigurationProperty(Feature.XSD_VERSION)) ?
                    Configuration.XSD11 : Configuration.XSD10;


            sourceFileNames.addAll(options.getPositionalOptions());

            // Customization hook for subclasses
            applyLocalOptions(options, config);

            if (!schemaFileNames.isEmpty() && scmInput != null) {
                badUsage("Cannot mix xsd and scm input");
                return 2;
            }

            if (schemaFileNames.isEmpty() && scmInput == null && sourceFileNames.isEmpty()) {
                badUsage("No action requested");
                return 2;
            }

            if (showTime) {
                config.getLogger().info(config.getProductTitle());
                config.getLogger().info(Version.platform.getPlatformVersion());
            }
            config.displayLicenseMessage();
            processor.setConfigurationProperty(Feature.SCHEMA_VALIDATION_MODE, validationMode);
            //processor.setConfigurationProperty(FeatureKeys.VALIDATION_WARNINGS, true);

            long startTime = now();

            XsdCompiler compiler = processor.newXsdCompiler();

            List<Source> schemaSources = new ArrayList<>();
            for (String schemaFileName : schemaFileNames) {
                Source schemaSource;
                if (useURLs || CommandLineOptions.isImplicitURI(schemaFileName)) {
                    ResourceRequest request = new ResourceRequest();
                    request.relativeUri = schemaFileName;
                    URI cwd = CommandLineOptions.getCurrentWorkingDirectory();
                    request.baseUri = cwd.toString();
                    request.uri = cwd.resolve(schemaFileName).toString();
                    request.nature = ResourceRequest.XSD_NATURE;
                    request.purpose = ResourceRequest.VALIDATION_PURPOSE;
                    schemaSource = request.resolve(config.getResourceResolver(), new DirectResourceResolver(config));
                } else if (schemaFileName.equals("-")) {
                    // take input from stdin
                    //#if CSHARP==false
                    if (Version.platform.isJava()) {
                        final InputSource inputSource = new InputSource(System.in);
                        inputSource.setSystemId(CommandLineOptions.getCurrentWorkingDirectory().toASCIIString());
                        schemaSource = new SAXSource(config.getStyleParser(), inputSource);
                    } else {
                        //#endif
                        schemaSource = new StreamSource(System.in);
                        //#if CSHARP==false
                    }
                    //#endif
                } else {
                    File schemaFile = new File(schemaFileName);
                    if (!schemaFile.exists()) {
                        return quit("Schema file " + schemaFile + " does not exist", 2);
                    }
                    //#if CSHARP==false
                    if (Version.platform.isJava()) {
                        InputSource eis = new InputSource(schemaFile.toURI().toString());
                        schemaSource = new SAXSource(config.getStyleParser(), eis);
                    } else {
                        //#endif
                        schemaSource = new StreamSource(schemaFile.toURI().toString());
                        //#if CSHARP==false
                    }
                    //#endif
                }

                if (schemaSource == null) {
                    return quit("URIResolver for schema file must return a Source", 2);
                }

                schemaSources.add(schemaSource);
            }

            XsdSchema schema;
            try {
                schema = compiler.compile(schemaSources.toArray(new Source[0]));
            } catch (SaxonApiException e) {
                if (e.getCause() instanceof SchemaException) {
                    // bug 2351
                    throw (SchemaException) e.getCause();
                } else {
                    throw e;
                }
            }


            if (scmInput != null) {
                Source scm = new StreamSource(new File(scmInput));
                schema = compiler.importComponents(scm);
            }

            if (showTime && !schemaFileNames.isEmpty()) {
                long endTime = now();
                config.getLogger().info("Schema checking successful. Time: " + (endTime - startTime)/1000000 + "ms.");
                config.getLogger().info(Timer.showMemoryUsed());
                //TimeUnit.MINUTES.sleep(1); // Useful for getting a heap dump
            }

            // Validate the document instances

            final SchemaValidator validator = schema.newValidator();
            validator.setLax("lax".equals(validationMode));
            if (topLevelElementName != null) {
                validator.setDocumentElementName(QName.fromClarkName(topLevelElementName));
            }

            if (options.definesParameterValues()) {
                options.setParams(processor, CSharp.methodRef(validator::setParameter));
            }

            Serializer statsSerializer = null;
            value = options.getOptionValue("stats");
            if (value != null) {
                validator.setCollectStatistics(true);
                File statsFile = new File(value);
                statsSerializer = processor.newSerializer(statsFile);
                statsSerializer.setOutputProperty(Serializer.Property.METHOD, "xml");
                statsSerializer.setOutputProperty(Serializer.Property.INDENT, "yes");
            }

            value = options.getOptionValue("report");


            if (value != null) {
                reportFileName = value;

                Serializer reportDestination = processor.newSerializer();
                reportDestination.setOutputProperty(Serializer.Property.INDENT, "yes");
                reportDestination.setOutputFile(new File(reportFileName));

                validator.setValidityReporting(reportDestination);
                reportHandler = (InvalidityReportGenerator) validator.getInvalidityHandler();
                String reportSchemaNames = options.getOptionValue("xsd");
                if (reportSchemaNames != null) {
                    reportHandler.setSchemaName(reportSchemaNames);
                }
                String reportXsdVersion = options.getOptionValue("xsdversion");
                if (reportXsdVersion != null) {
                    reportHandler.setXsdVersion(reportXsdVersion);
                }
            } else {
                validator.setInvalidityHandler(new StandardInvalidityHandler(config));
            }

            if (sourceFileNames.size() > 1) {
                List<Source> sources = new ArrayList<>(sourceFileNames.size());
                for (String sourceFileName : sourceFileNames) {
                    Source source = loadDocument(sourceFileName, useURLs, config);
                    sources.add(source);

                }

                validator.validateMultiple(sources);


            } else {
                for (String sourceFileName : sourceFileNames) {

                    try {
                        Source source = loadDocument(sourceFileName, useURLs, config);
                        processFile(source, validator);
                        if (showTime) {
                            config.getLogger().info("Validation successful");
                        }
                        validCount++;
                        //#if CSHARP==false
                        if (source instanceof SAXSource) {
                            XMLReader parser = ((SAXSource) source).getXMLReader();
                            if (parser != null) {
                                config.reuseSourceParser(parser);
                            }
                        }
                        //#endif
                    } catch (SaxonApiException e) {
                        if (e.getCause() instanceof ValidationException) {
                            if (showTime) {
                                config.getLogger().warning("Validation complete: errors found");
                            } else {
                                config.getLogger().warning("Validation of file " + sourceFileName + " completed: errors found");
                            }
                            invalidCount++;
                        } else {
                            config.getLogger().error("Failed to process file " + sourceFileName + ": " + e.getMessage());
                            unreadableCount++;
                        }
                    }
                }


                if (showTime && validCount + invalidCount > 1) {
                    config.getLogger().info(validCount + " valid document" + (validCount == 1 ? "" : "s") + "; " +
                            invalidCount + " invalid document" + (invalidCount == 1 ? "" : "s") +
                            (unreadableCount == 0 ? "" : "; " + unreadableCount + " unreadable or ill-formed document"
                                    + (unreadableCount == 1 ? "" : "s")));
                }

            }
            // If requested, produce the statistics

            if (statsSerializer != null) {
                validator.reportValidationStatistics(statsSerializer);
            }

            Instrumentation.report();

            // If requested, produce the serialized Schema Component Model

            if (scmOutput != null) {
                final File scmOutputFile = new File(scmOutput);
                Serializer scmSerializer = processor.newSerializer(scmOutputFile);
                scmSerializer.setOutputProperty(Serializer.Property.METHOD, "xml");
                scmSerializer.setOutputProperty(Serializer.Property.INDENT, "yes");
                schema.exportComponents(scmSerializer);
                if (showTime) {
                    config.getLogger().info("Schema component model written to " + scmOutputFile.getAbsolutePath());
                }
            }
        } catch (SchemaException err) {
            //err.printStackTrace();
            return quit("Schema processing failed: " + err.getMessage(), 2);
        } catch (ValidationException err) {
            config.reportFatalError(err);
            return quit("Validation of source document failed", 2);
        } catch (XPathException err) {
            config.reportFatalError(err);
            return quit("Processing failed", 2);
        } catch (TransformerFactoryConfigurationError err) {
            return quit("Validation failed: " + err.getMessage(), 2);
        } catch (Exception err2) {
            err2.printStackTrace();
            return quit("Fatal error during validation: " + err2.getMessage(), 2);
        }
        if (unreadableCount > 0) {
            return 2;
        } else if (invalidCount > 0) {
            return 1;
        } else {
            return 0;
        }
    }

    /**
     * Customisation hook: apply options defined locally in a subclass
     *
     * @param options the CommandLineOptions
     * @param config  the Saxon Configuration
     */

    @SuppressWarnings({"UnusedDeclaration"})
    @CSharpModifiers(code = {"internal"})
    protected void applyLocalOptions(CommandLineOptions options, Configuration config) {
        // no action: provided for subclasses to override
    }


    /**
     * Exit with a message
     *
     * @param message The message to be output
     * @param code    The result code to be returned to the operating
     *                system shell
     * @return the requested return code
     */

    protected int quit(String message, int code) {
        if (config != null && config.getLogger() != null) {
            config.getLogger().warning(message);
        } else {
            System.err.println(message);
        }
        if (allowExit) {
            System.exit(code);
        } else {
            throw new RuntimeException(message);
        }
        return code;
    }

    /**
     * Load a document, given a filename or URL
     *
     * @param sourceFileName the name of the source file
     * @param useURLs        true if name is to be treated as a URI
     * @param config         the Saxon configuration
     * @return the Source object that results from loading the document
     * @throws XPathException if a document cannot be loaded
     */

    public static Source loadDocument(/*@NotNull*/ String sourceFileName, boolean useURLs, /*@NotNull*/ Configuration config)
            throws XPathException {
        Source sourceInput;
        if (useURLs || CommandLineOptions.isImplicitURI(sourceFileName)) {
            ResourceRequest request = new ResourceRequest();
            request.relativeUri = sourceFileName;
            URI cwd = CommandLineOptions.getCurrentWorkingDirectory();
            request.baseUri = cwd.toString();
            request.uri = cwd.resolve(sourceFileName).toString();
            request.nature = ResourceRequest.XML_NATURE;
            request.purpose = ResourceRequest.ANY_PURPOSE;
            sourceInput = request.resolve(config.getResourceResolver(), new DirectResourceResolver(config));
        } else if (sourceFileName.equals("-")) {
            //#if CSHARP==false
            // take input from stdin
            if (Version.platform.isJava()) {
                String sysId = CommandLineOptions.getCurrentWorkingDirectory().toASCIIString();
                final InputSource inputSource = new InputSource(System.in);
                inputSource.setSystemId(sysId);
                sourceInput = new SAXSource(inputSource);
            } else {
                //#endif
                sourceInput = new StreamSource(System.in);
                //#if CSHARP==false
            }
            //#endif
        } else {
            File sourceFile = new File(sourceFileName);
            if (!sourceFile.exists()) {
                throw new XPathException("Source file " + sourceFile + " does not exist");
            }
            //#if CSHARP==false
            if (Version.platform.isJava()) {
                InputSource eis = new InputSource(sourceFile.toURI().toString());
                sourceInput = new SAXSource(eis);
            } else {
                //#endif
                sourceInput = new StreamSource(sourceFile.toURI().toString());
                //#if CSHARP==false
            }
            //#endif
        }
        return sourceInput;
    }


    /**
     * Process a single file using a supplied schema
     *
     * @param source    The source XML document to be validated
     * @param validator The schema validator used to perform the validation
     * @throws SaxonApiException If the validation fails (or if validation could not be attempted)
     */

    @CSharpModifiers(code = {"internal"})
    public void processFile(/*@NotNull*/ Source source, SchemaValidator validator)
            throws SaxonApiException {

        if (showTime) {
            config.getLogger().info("Processing " + source.getSystemId());
        }
        long startTime = now();

        for (int r = 0; r < repeat; r++) {      // repeat is for internal testing/timing
            validator.validate(source);
        }
        if (showTime) {
            long endTime = now();
            config.getLogger().info("Validation time: " + (endTime - startTime)/1000000 + "ms.");
            config.getLogger().info(Timer.showMemoryUsed());
        }
    }

    /**
     * Get current time in nanoseconds
     *
     * @return the current time in nanoseconds (since VM startup)
     */

    protected static long now() {
        return System.nanoTime();
    }


    /**
     * Report incorrect usage of the command line, with a list of the options and arguments that are available
     *
     * @param message The error message
     */
    protected void badUsage(String message) {
        if (!"".equals(message)) {
            config.getLogger().error(message);
        }
        if (!showTime) {
            config.getLogger().info(config.getProductTitle());
        }
        config.getLogger().info("Usage: see http://www.saxonica.com/documentation/index.html#!schema-processing/commandline");
        config.getLogger().info("Format: " + CommandLineOptions.getCommandName(this) + " options");
        CommandLineOptions options = new CommandLineOptions();
        setPermittedOptions(options);
        config.getLogger().info("Options available:" + options.displayPermittedOptions());
        config.getLogger().info("Use -XYZ:? for details of option XYZ");
        if (allowExit) {
            if ("".equals(message)) {
                System.exit(0);
            } else {
                System.exit(2);
            }
        } else {
            throw new RuntimeException(message);
        }
    }


}

// Copyright (c) 2004-2023 Saxonica Limited