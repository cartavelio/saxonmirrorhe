package com.saxonica.testdriver.old;


import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.config.ProfessionalConfiguration;
import com.saxonica.testdriver.Licensor;
import net.sf.saxon.Configuration;
import net.sf.saxon.Version;
import net.sf.saxon.expr.number.Numberer_en;
import net.sf.saxon.functions.ResolveURI;
import net.sf.saxon.lib.*;
import net.sf.saxon.om.NameChecker;
import net.sf.saxon.om.NamePool;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.s9api.*;
import net.sf.saxon.style.StylesheetModule;
import net.sf.saxon.testdriver.CanonicalXML;
import net.sf.saxon.testdriver.Environment;
import net.sf.saxon.testdriver.TestDriver;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.FastStringBuffer;
import net.sf.saxon.value.DateTimeValue;
import org.w3c.tidy.Tidy;
import org.xml.sax.EntityResolver;
import org.xml.sax.InputSource;
import org.xml.sax.SAXException;
import org.xml.sax.XMLReader;

import javax.xml.parsers.ParserConfigurationException;
import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamWriter;
import javax.xml.transform.OutputKeys;
import javax.xml.transform.Source;
import javax.xml.transform.SourceLocator;
import javax.xml.transform.TransformerException;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.*;
import java.net.URI;
import java.net.URISyntaxException;
import java.util.*;
import java.util.regex.Pattern;

/**
 * This class runs the W3C XSLT Test Suite, driven from the test catalog.
 */
public class Xslt20TestSuiteDriver extends TestDriver {

    private Licensor licensor = new Licensor();

    public static String RNS = "http://www.w3.org/2006/xslt20-test-result";

    @Override
    public String catalogNamespace() {
        return RNS;
    }

    @Override
    public void processSpec(String specStr) {
        //To change body of implemented methods use File | Settings | File Templates.
    }

    @Override
    protected void createGlobalEnvironments(XdmNode catalog, XPathCompiler xpc) throws SaxonApiException {
        //To change body of implemented methods use File | Settings | File Templates.
    }

    @Override
    protected void runTestCase(XdmNode testCase, XPathCompiler catalogXpc) throws SaxonApiException {
        //To change body of implemented methods use File | Settings | File Templates.
    }

    @Override
    public boolean ensureDependencySatisfied(XdmNode dependency, Environment env) {
        return false;  //To change body of implemented methods use File | Settings | File Templates.
    }

    /**
     * Run the testsuite using Saxon.
     *
     * @param args Array of parameters passed to the application
     *             via the command line.
     */
    public static void main(String[] args) throws Exception {
        if (args.length == 0 || args[0].equals("-?")) {
            System.err.println("XsltTestSuiteDriver testsuiteDir [-c:catalog] [-fast] [-compile[:debug]] [-ee] testName?");
        }

        System.err.println("Testing Saxon " + Version.getProductVersion());
        new Xslt20TestSuiteDriver().go(args);
    }

    String testSuiteDir;
    Processor factory;// = new Processor(true);
    Processor sfactory;// = new Processor(true);
    Processor pfactory;// = new Processor(true);
    Processor factory11;// = new Processor(true);
    Processor schemaAsFactory = new Processor(true);  // use a separate config for schemaas tests
    MyErrorListener errorListener;
    XMLReader parser;
    XMLReader resultParser;
    XMLReader fragmentParser;
    Pattern testPattern = null;
    boolean showWarnings = false;
    boolean xml11 = false;
    boolean xsd11 = false;
    boolean timingOnly = false;
    boolean fastTestsOnly = false;
    boolean compile = false;
    boolean debugCompile = false;
    boolean eeproduct = false;
    private int testsRun = 0;
    private int testsFailed = 0;

    XMLStreamWriter results;

    /**
     * Some tests use schemas that conflict with others, so they can't use the common schema cache.
     * These tests are run in a Configuration of their own. (Ideally we would put this list in a
     * catalogue file of some kind).
     */

    static HashSet noCacheTests = new HashSet(30);

    static {
        noCacheTests.add("schema092");
        noCacheTests.add("schemainline20_005_01");
        noCacheTests.add("schemamatch20_001_01");
        noCacheTests.add("schemamatch20_003_01");
        noCacheTests.add("schemamatch20_005_01");
        noCacheTests.add("schemamatch20_007_01");
        noCacheTests.add("schemamatch20_036_01");
        noCacheTests.add("schemamatch20_038_01");
        noCacheTests.add("schemamatch20_061_01");
        noCacheTests.add("schemamatch20_079_01");
        noCacheTests.add("schemamatch20_092_01");
        noCacheTests.add("schemamatch20_123_01");
        noCacheTests.add("schemamatch20_140_01");
        noCacheTests.add("schemanodetest20_001_01");
        noCacheTests.add("schemanodetest20_023_01");
        noCacheTests.add("schvalid001");
        noCacheTests.add("schvalid004");
        noCacheTests.add("schvalid005");
        noCacheTests.add("schvalid009");
        noCacheTests.add("schvalid014");
        noCacheTests.add("schvalid015");
        noCacheTests.add("schvalid020");
        noCacheTests.add("striptype20_003_01");
        noCacheTests.add("striptype20_006_01");
        noCacheTests.add("striptype20_008_01");
        noCacheTests.add("striptype20_011_01");
        noCacheTests.add("striptype20_012_01");
        noCacheTests.add("striptype20_039_01");

    }

    static HashSet<String> undetectedRecoverableErrors = new HashSet<String>();

    static {
        undetectedRecoverableErrors.add("XTRE0270");
    }

    static HashSet<String> unrecoverableErrors = new HashSet<String>();

    static {
        unrecoverableErrors.add("SESU0007");
    }

    private boolean isSlow(String testName) {
        return testName.startsWith("reclass") ||
                testName.equals("re00985") ||
                testName.equals("re00986") ||
                testName.equals("re00987") ||
                testName.equals("unicode010");
    }


    public void printError(String error, Exception e) {
        printError(error, e.getMessage());
        e.printStackTrace();
    }

    public void printResults() {
        if (shell != null) {
            DocumentBuilder builder = factory.newDocumentBuilder();
            XdmNode resultsDoc = null;
            try {
                resultsDoc = builder.build(
                        new StreamSource(new File(testSuiteDir + '/' + getResultDirectoryName() + "/results"
                                + Version.getProductVersion() + ".xml")));
                XPathCompiler xpath = factory.newXPathCompiler();

                xpath.declareNamespace("t", "http://www.w3.org/2006/xslt20-test-result");
                XdmItem passValue = (XdmItem) xpath.evaluate("count(//t:testcase[@result='full'])", resultsDoc);
                int passTestCount = Integer.parseInt(passValue.getStringValue());
                XdmItem failValue = (XdmItem) xpath.evaluate("count(//t:testcase[@result='differ'])", resultsDoc);
                int failTestCount = Integer.parseInt(failValue.getStringValue());
                XdmItem errorValue = (XdmItem) xpath.evaluate("count(//t:testcase[@result='different-error'])", resultsDoc);
                int errorCodeTestCount = Integer.parseInt(errorValue.getStringValue());
                shell.printResults(
                        "Results: " + passTestCount + " passed, " + failTestCount + " failed, " + errorCodeTestCount + " incorrect error code",
                        testSuiteDir + '/' + getResultDirectoryName() + "/results" + Version.getProductVersion() + ".xml",
                        testSuiteDir + '/' + getResultDirectoryName());
            } catch (SaxonApiException e) {
                e.printStackTrace();
            } catch (NumberFormatException e) {
                e.printStackTrace();
            }
        }
    }

    public void go(String[] args) throws SAXException, ParserConfigurationException {


        testSuiteDir = args[0];
        String catalogName = "catalog";
        HashSet exceptions = new HashSet();

        for (int i = 1; i < args.length; i++) {
            if (args[i].equals("-w")) {
                showWarnings = true;
            } else if (args[i].equals("-t")) {
                timingOnly = true;
            } else if (args[i].equals("-compile")) {
                compile = true;
                debugCompile = false;
            } else if (args[i].equals("-compile:debug")) {
                compile = true;
                debugCompile = true;
            } else if (args[i].startsWith("-c:")) {
                catalogName = args[i].substring(3);
            } else if (args[i].equals("-fast")) {
                fastTestsOnly = true;
            } else if (args[i].equals("-ee")) {
                eeproduct = true;
            } else if (args[i].startsWith("-")) {
                System.err.println("*** Ignoring unknown option " + args[i]);
            } else {
                testPattern = Pattern.compile(args[i]);
            }
        }

        try {
//            parser = SAXParserFactory.newInstance().newSAXParser().getXMLReader();
//            if (!parser.getFeature("http://xml.org/sax/features/xml-1.1")) {
//                System.err.println("Warning: XML parser does not support XML 1.1 - " + parser.getClass());
//            };
            //NamePool pool = new DiagnosticNamePool();
            NamePool pool = new NamePool();
            //Configuration config = new Configuration();
            Configuration config;
            if (eeproduct) {
                config = new EnterpriseConfiguration();
                config.setBooleanProperty(Feature.GENERATE_BYTE_CODE, compile);
                config.setConfigurationProperty(Feature.STREAMABILITY, "extended");
            } else {
                config = new Configuration();
            }

            parser = config.getSourceParser();
            if (!parser.getFeature("http://xml.org/sax/features/xml-1.1")) {
                println("Warning: XML parser does not support XML 1.1 - " + parser.getClass());
            }
            resultParser = config.getSourceParser();
            resultParser.setEntityResolver(
                    new EntityResolver() {
                        public InputSource resolveEntity(String publicId, String systemId) {
                            return new InputSource(new StringReader(""));
                        }
                    }
            );
            fragmentParser = config.getSourceParser();
            EnterpriseConfiguration sconfig = new EnterpriseConfiguration();
            sconfig.setExtensionElementNamespace(
                    NamespaceConstant.SQL, "net.sf.saxon.option.sql.SQLElementFactory");
            sconfig.setBooleanProperty(Feature.GENERATE_BYTE_CODE, compile);
            sconfig.setBooleanProperty(Feature.DEBUG_BYTE_CODE, debugCompile);
            //sconfig.setConfigurationProperty(FeatureKeys.STREAMABILITY, "extended");

            ProfessionalConfiguration pconfig = new ProfessionalConfiguration();
            pconfig.setExtensionElementNamespace(
                    NamespaceConstant.SQL, "net.sf.saxon.option.sql.SQLElementFactory");

            EnterpriseConfiguration sasconfig = new EnterpriseConfiguration();
            sasconfig.setBooleanProperty(Feature.GENERATE_BYTE_CODE, compile);
            sasconfig.setBooleanProperty(Feature.DEBUG_BYTE_CODE, debugCompile);
            //sasconfig.setConfigurationProperty(FeatureKeys.STREAMABILITY, "extended");

            Configuration config11 = new Configuration();
            config11.setXMLVersion(Configuration.XML11);
            config.setNamePool(pool);
            sconfig.setNamePool(pool);
            pconfig.setNamePool(pool);
            config11.setNamePool(pool);
            sfactory = new Processor(sconfig);
            licensor.activate(sfactory);
            pfactory = new Processor(pconfig);
            licensor.activate(pfactory);
            factory = new Processor(config);
            licensor.activate(factory);
            factory11 = new Processor(config11);
            licensor.activate(factory11);
            schemaAsFactory = new Processor(sasconfig);
            licensor.activate(schemaAsFactory);

            setLocalizers(config);

            errorListener = new MyErrorListener();
            //factory.setErrorListener(errorListener);
            //sfactory.setErrorListener(errorListener);


            String testURI = "http://www.w3.org/2005/05/xslt20-test-catalog";

            DocumentBuilder builder = factory.newDocumentBuilder();
            if (!(new File(testSuiteDir + '/' + getResultDirectoryName())).exists()) {
                new File(testSuiteDir + '/' + getResultDirectoryName()).mkdir();
            }
            File exceptionFile = new File(testSuiteDir + '/' + getResultDirectoryName() + "/exceptions.xml");

            XdmNode exceptionsDoc;
            try {
                exceptionsDoc = builder.build(new StreamSource(exceptionFile));
            } catch (SaxonApiException e1) {
                exceptionsDoc = builder.build(new StreamSource(new StringReader("<exceptions/>")));

                Writer resultWriter = new BufferedWriter(new FileWriter(testSuiteDir + '/' + getResultDirectoryName() + "/exceptions.xml"));

                Serializer serializer = factory.newSerializer(resultWriter);
                serializer.setOutputProperty(Serializer.Property.METHOD, "xml");
                serializer.setOutputProperty(Serializer.Property.INDENT, "yes");
                serializer.serializeNode(exceptionsDoc);
                resultWriter.close();
            }

            XPathCompiler xpath = factory.newXPathCompiler();
            xpath.declareNamespace("t", testURI);
            xpath.setCaching(true);

            for (XdmItem item : xpath.evaluate("//exception/@test-case", exceptionsDoc)) {
                //XdmItem testCaseItem = (XdmItem)xpath.evaluate("//exception/@test-case", item);
                String name = item.getStringValue();
                StringTokenizer tok = new StringTokenizer(name);
                while (tok.hasMoreElements()) {
                    exceptions.add(tok.nextElement());
                }
            }

            String date = DateTimeValue.getCurrentDateTime(null).getStringValue().substring(0, 10);
            XdmNode catalog;

            if (catalogName.endsWith(".entity")) {
                String uri = "file:///" + System.getProperty("user.dir").replace('\\', '/') + "/cat/" + catalogName;
                String wrapper =
                        "<!DOCTYPE testcases [\n" +
                                "<!ENTITY e SYSTEM '" + uri + "'>" +
                                "]>\n" +
                                "<testcases xmlns=\"http://www.w3.org/2005/05/xslt20-test-catalog\" SourceOffsetPath=\"./\"\n" +
                                "           ResultOffsetPath=\"ExpectedTestResults/\"\n" +
                                "           InputOffsetPath=\"TestInputs/\"\n" +
                                "           testSuiteVersion=\"1.1.0\">" +
                                "&e;</testcases>";
                StringReader sr = new StringReader(wrapper);
                StreamSource ss = new StreamSource(sr, System.getProperty("user.dir"));
                catalog = builder.build(ss);

            } else {
                catalog = builder.build(
                        new StreamSource(new File(testSuiteDir + "/" + catalogName + ".xml")));
            }

            writeResultFilePreamble(config, date);

            long startTime = (new Date()).getTime();

            for (XdmItem item : xpath.evaluate("//t:testcase", catalog)) {
                xml11 = false;
                xsd11 = false;
                XdmNode testCase = (XdmNode) item;

                String testName = xpath.evaluateSingle("t:name", testCase).getStringValue();
                if (testPattern != null && !testPattern.matcher(testName).matches()) {
                    continue;
                }
                if (exceptions.contains(testName)) {
                    continue;
                }
                if (isExcluded(testName)) {
                    continue;
                }
                if (fastTestsOnly && isSlow(testName)) {
                    continue;
                }
                //if (!timingOnly) {
                println("Test " + testName);
                //}
                runTestCase(xpath, testCase, testName);
            }

            // }

            writeResultFilePostamble();
            println("Time: " + (new Date().getTime() - startTime) + " ms");
            println("Ran " + testsRun + " tests, failed " + testsFailed);
            printResults();

        } catch (Exception e) {
            printError("*** Failed to process exceptions file", e);
        }
    }

    private void runTestCase(XPathCompiler xpath, XdmNode testCase, String testName) throws SaxonApiException {
        testsRun++;
        XdmNode testInput = (XdmNode) xpath.evaluateSingle("t:input", testCase);
        XdmNode stylesheet = (XdmNode) xpath.evaluateSingle("t:stylesheet", testInput);
        String absXSLName = null;
        boolean useAssociated = false;
        if (stylesheet == null) {
            useAssociated = true;
        } else {
            String fileName = getAttribute(stylesheet, "file");
            File xslFile = new File(new File(testSuiteDir, "TestInputs"), fileName);
            absXSLName = getCanonicalPath(xslFile);
        }

        XdmNode sourceDocument = (XdmNode) xpath.evaluateSingle("t:source-document[@role='principal']", testInput);
        String absXMLName = null;
        if (sourceDocument != null) {
            String fileName = getAttribute(sourceDocument, "file");
            File xmlFile = new File(new File(testSuiteDir, "/TestInputs/"), fileName);
            absXMLName = getCanonicalPath(xmlFile);
        }

        final Map<String, String> documentUriMap = new HashMap<String, String>();
        XdmValue sourceUris = xpath.evaluate("t:source-document[@uri]", testInput);
        for (XdmItem item : sourceUris) {
            XdmNode node = (XdmNode) item;
            String uri = getAttribute(node, "uri");
            String fileName = getAttribute(node, "file");
            File xmlFile = new File(new File(testSuiteDir, "/TestInputs/"), fileName);
            String absName = getCanonicalPath(xmlFile);
            documentUriMap.put(uri, absName);
        }

        final Map<String, File> unparsedtextUriMap = new HashMap<String, File>();
        XdmValue textUris = xpath.evaluate("t:unparsed-text", testInput);
        for (XdmItem item : textUris) {
            XdmNode node = (XdmNode) item;
            String uri = getAttribute(node, "uri");
            String fileName = getAttribute(node, "file");
            File textFile;
            if (fileName.startsWith("file:///")) {
                textFile = new File(fileName.substring(7));
            } else if (fileName.startsWith("file:/")) {
                textFile = new File(fileName.substring(5));
            } else {
                textFile = new File(new File(testSuiteDir, "/TestInputs/"), fileName);
            }
            unparsedtextUriMap.put(uri, textFile);
        }

        boolean schemaAware = false;
        boolean nonSchemaAware = false;
        String edition = "HE";
        boolean recoverRecoverable = true;
        boolean backwardsCompatibility = true;
        boolean supportsDOE = true;
        boolean inapplicable = false;
        boolean streaming = false;
        Set<String> versionSet = new HashSet<String>();

        for (XdmItem discretionaryItems : xpath.evaluate("t:discretionary-items", testCase)) {
            for (XdmItem feature : xpath.evaluate("t:discretionary-feature", discretionaryItems)) {
                String featureName = getAttribute(feature, "name");
                String behavior = getAttribute(feature, "behavior");
                if ("schema_aware".equals(featureName)) {
                    schemaAware = "on".equals(behavior);
                    nonSchemaAware = "off".equals(behavior);
                } else if ("streaming".equals(featureName)) {
                    if ("on".equals(behavior)) {
                        edition = "EE";
                        streaming = true;
                    }
                } else if ("Saxon-PE".equals(featureName)) {
                    if ("on".equals(behavior)) {
                        edition = "PE";
                    }
                } else if ("Saxon-EE".equals(featureName)) {
                    if ("on".equals(behavior)) {
                        edition = "EE";
                    }
                } else if ("XML_1.1".equals(featureName)) {
                    xml11 = "on".equals(behavior);
                } else if ("XSD_1.1".equals(featureName)) {
                    xsd11 = "on".equals(behavior);
                } else if ("backwards_compatibility".equals(featureName)) {
                    backwardsCompatibility = "on".equals(behavior);
                } else if ("disabling_output_escaping".equals(featureName)) {
                    supportsDOE = "on".equals(behavior);
                }
            }
            for (XdmItem choice : xpath.evaluate("t:discretionary-choice", discretionaryItems)) {
                String featureName = getAttribute(choice, "name");
                String behavior = getAttribute(choice, "behavior");
                if ("error".equals(behavior)) {
                    recoverRecoverable = false;
                }
                if (unrecoverableErrors.contains(featureName) && "recovery".equals(behavior)) {
                    inapplicable = true;
                }
                if (undetectedRecoverableErrors.contains(featureName) && "error".equals(behavior)) {
                    inapplicable = true;
                }
            }
            for (XdmItem spec : xpath.evaluate("t:discretionary-version/@spec", discretionaryItems)) {
                versionSet.add(spec.getStringValue());
            }
        }

        if (compile) {
            edition = "EE";
        }

        if (compile && nonSchemaAware) {
            // Saxon cannot compile when a non-schema-aware processor is requested
            writeTestcaseElement(testName, "not run", "cannot compile when a basic processor is required");
            return;
        }

        if (!backwardsCompatibility) {
            // Saxon cannot run with BC switched off
            writeTestcaseElement(testName, "not run", "requires backwards-compatibility=off");
            return;
        }

        if (!supportsDOE) {
            // Saxon cannot run with DOE switched off
            writeTestcaseElement(testName, "not run", "requires disable-output-escaping=off");
            return;
        }

        if (inapplicable) {
            // Saxon cannot recover from error SESU0007
            writeTestcaseElement(testName, "not run", "unsupported recoverable error choice");
            return;
        }

        QName initialMode = getQNameAttribute(xpath, testInput, "t:initial-mode/@qname");
        QName initialTemplate = getQNameAttribute(xpath, testInput, "t:entry-named-template/@qname");

        XdmNode initialContextNode = (XdmNode) xpath.evaluateSingle("t:initial-context-node", testInput);
        String initialContextPath = null;
        if (initialContextNode != null) {
            initialContextPath = initialContextNode.getStringValue();
        }

        XdmNode validation = (XdmNode) xpath.evaluateSingle("t:validation", testInput);
        String validationMode = (validation == null ? null : getAttribute(validation, "mode"));

        HashMap params = new HashMap();
        for (XdmItem param : xpath.evaluate("t:stylesheet-parameters/t:param", testInput)) {
            String name = getAttribute(param, "qname");
            // TODO: turn into a clark name
            String value = param.getStringValue();
            params.put(name, value);
        }

        for (XdmItem schema : xpath.evaluate("t:schema", testInput)) {
            String role = getAttribute(schema, "role");
            if (("source-validator".equals(role) || "source-reference".equals(role))) {
                validationMode = "strict";
                // TODO: control which source documents are validated...
            }
        }

        XdmNode testOutput = (XdmNode) xpath.evaluateSingle("t:output", testCase);

        XdmNode principalResultDocuments = (XdmNode) xpath.evaluateSingle("t:result-document[@role='principal']", testOutput);
        XdmValue secondaryResultDocuments = xpath.evaluate("t:result-document[@role='secondary']", testOutput);

        List<ComparisonTask> comparisonTasks = new ArrayList<ComparisonTask>();

        // TODO: handle alternative and multiple result documents
        String refFileName = null;
        String outFileName;
        String comparator = "xml";
        boolean ignoreIndentation = false;
        boolean ignoreIndentationForSecondary = false;

        if (principalResultDocuments != null) {
            String outFileAtt = getAttribute(principalResultDocuments, "file");
            int slash = outFileAtt.indexOf('/');
            String outDir = outFileAtt;
            if (slash >= 0) {
                outDir = outFileAtt.substring(0, slash);
            }
            File refFile = new File(new File(testSuiteDir, "/ExpectedTestResults/"), outFileAtt);

            refFileName = getCanonicalPath(refFile);  //testSuiteDir + "/ExpectedTestResults/" + outFileAtt;


            File outFile = new File(new File(new File(testSuiteDir, getResultDirectoryName()), "."), outFileAtt);
            outFileName = getCanonicalPath(outFile);  //testSuiteDir + '/' + getResultDirectoryName() + '/' + outDir + '/' + testName + ".out";
            comparator = getAttribute(principalResultDocuments, "type");
            ignoreIndentation = "yes".equals(getAttribute(principalResultDocuments, "indent-ignore"));

            ComparisonTask task = new ComparisonTask();
            task.referenceResults = refFileName;
            task.actualResults = outFileName;
            task.comparisonMode = comparator;
            task.ignoreIndentation = ignoreIndentation;
            comparisonTasks.add(task);

        } else {
            File outFile = new File(new File(testSuiteDir, getResultDirectoryName()), "temp.out");
            outFileName = getCanonicalPath(outFile); //testSuiteDir + '/' + getResultDirectoryName() + "/temp.out";
        }

        for (XdmItem result : secondaryResultDocuments) {
            String outFileAtt = getAttribute(result, "file");
            File refSecondaryFile = new File(new File(testSuiteDir, "/ExpectedTestResults/"), outFileAtt);
            String refSecondaryFileName = getCanonicalPath(refSecondaryFile);

            File outSecondaryFile = new File(new File(new File(testSuiteDir, getResultDirectoryName()), "."), outFileAtt);
            String outSecondaryFileName = getCanonicalPath(outSecondaryFile);

            ComparisonTask task = new ComparisonTask();
            task.referenceResults = refSecondaryFileName;
            task.actualResults = outSecondaryFileName;
            task.comparisonMode = getAttribute(principalResultDocuments, "type");
            task.ignoreIndentation = "yes".equals(getAttribute(principalResultDocuments, "indent-ignore"));
            comparisonTasks.add(task);
        }

        Set<QName> expectedErrors = new HashSet<>(4);
        boolean allowAnyError = false;
        for (XdmItem errorId : xpath.evaluate("t:error", testOutput)) {
            String code = ((XdmNode) errorId).attribute("error-id");
            if (code.equals("*")) {
                allowAnyError = true;
            } else if (NameChecker.isValidNCName(code)) {
                expectedErrors.add(new QName("", NamespaceConstant.ERR, code));
            } else {
                expectedErrors.add(new QName(code, (XdmNode) errorId));
            }
        }

        List<String> expectedMessages = new ArrayList<String>();
        for (XdmItem msg : xpath.evaluate("t:xsl-message", testOutput)) {
            expectedMessages.add(msg.getStringValue());
        }
        boolean success;
        errorListener.errorCodes.clear();

        XdmItem optimizationAssertion = xpath.evaluateSingle("t:optimization/@assert", testCase);
        String assertion = (optimizationAssertion == null ? null : optimizationAssertion.getStringValue());

        final List<String> actualMessages = new ArrayList<String>();
        try {
            Serializer sr = xpath.getProcessor().newSerializer(new File(outFileName));
            Processor f;
            if (noCacheTests.contains(testName) || testName.startsWith("schemaas20") ||
                    testName.startsWith("striptype20") || testName.startsWith("notation20")) {
                EnterpriseConfiguration sconfig = new EnterpriseConfiguration();
                sconfig.setNamePool(factory.getUnderlyingConfiguration().getNamePool());
                sconfig.setBooleanProperty(Feature.GENERATE_BYTE_CODE, compile);
                sconfig.setBooleanProperty(Feature.DEBUG_BYTE_CODE, debugCompile);
                f = new Processor(sconfig);
                sconfig.setConfigurationProperty(Feature.XSLT_SCHEMA_AWARE, true);
            } else {
                if (schemaAware || edition.equals("EE") || versionSet.contains("XSLT30")) {
                    f = sfactory;
                    f.setConfigurationProperty(FeatureKeys.XSLT_SCHEMA_AWARE, true);
                    if (xml11) {
                        f.setConfigurationProperty(FeatureKeys.XML_VERSION, "1.1");
                    }
                } else if (edition.equals("PE")) {
                    f = pfactory;
                } else if (xml11) {
                    f = factory11;
                } else {
                    f = factory;
                }
            }

            XdmNode sourceNode = null;
            final Configuration config = f.getUnderlyingConfiguration();
            config.setBooleanProperty(Feature.TRACE_OPTIMIZER_DECISIONS, false);
            if (validationMode == null) {
                config.setSchemaValidationMode(Validation.STRIP);
            } else {
                config.setSchemaValidationMode(Validation.getCode(validationMode));
            }

            config.setConfigurationProperty(Feature.XSD_VERSION, xsd11 ? "1.1" : "1.0");
            config.setCollationURIResolver(new StandardCollationURIResolver() {
                public StringCollator resolve(String uri, Configuration config) throws XPathException {
                    if ("http://www.w3.org/xslts/collation/caseblind".equals(uri)) {
                        return super.resolve("http://saxon.sf.net/collation?ignore-case=yes", config);
                    } else {
                        return super.resolve(uri, config);
                    }
                }
            });
            // Dropped prior to 9.7 release
//            config.setCollectionURIResolver(new StandardCollectionURIResolver() {
//                public Iterator<Source> resolve(String href, String base, Properties properties, XPathContext context) throws XPathException {
//                    // Map the default collection to an empty sequence
//                    // TODO: implement the input/collection element in the catalog (see mdocs28)
//                    if (href == null) {
//                        List<Source> list = Collections.emptyList();
//                        return list.iterator();
//                    }
//                    return super.resolve(href, base, properties, context);
//                }
//            });

            Source styleSource;
            if (useAssociated) {
                // in this case only, we build the source document before compiling the stylesheet
                sourceNode = buildSourceDoc(absXMLName, initialContextPath, validationMode, f, null);
                try {
                    styleSource = StylesheetModule.getAssociatedStylesheet(
                            config, config.getURIResolver(), sourceNode.asSource(), null, null, null);
                } catch (XPathException e) {
                    throw new SaxonApiException(e);
                }
            } else {
                styleSource = new StreamSource(new File(absXSLName));
            }

            // Compile the stylesheet

            XsltCompiler compiler = f.newXsltCompiler();
            compiler.setErrorListener(errorListener);
            XsltExecutable sheet = compiler.compile(styleSource);

            if (assertion != null) {
                XdmDestination builder = new XdmDestination();
                sheet.explain(builder);
                XdmNode expressionTree = builder.getXdmNode();
                XPathCompiler xpe = f.newXPathCompiler();
                XPathSelector exp = xpe.compile(assertion).load();
                exp.setContextItem(expressionTree);
                XdmAtomicValue bv = (XdmAtomicValue) exp.evaluateSingle();
                if (!bv.getBooleanValue()) {
                    println("** Optimization assertion failed");
                    println(expressionTree.toString());
                    throw new SaxonApiException("Expected optimization not performed");
                }
            }

            // Build the source document if not already done

            if (absXMLName != null && sourceNode == null && !streaming) {
                sourceNode = buildSourceDoc(absXMLName, initialContextPath, validationMode, f, sheet);
            }

            // Prepare to run the transformation

            XsltTransformer inst = sheet.load();

            if (initialMode != null) {
                inst.setInitialMode(initialMode);
            }
            if (initialTemplate != null) {
                inst.setInitialTemplate(initialTemplate);
            }
            if (params != null) {
                Iterator iter = params.keySet().iterator();
                while (iter.hasNext()) {
                    String name = (String) iter.next();
                    String value = (String) params.get(name);
                    inst.setParameter(new QName(name), new XdmAtomicValue(value));
                }
                if (testName.startsWith("reclass")) {
                    inst.setParameter(new QName("result-dirs"), new XdmAtomicValue("3.1"));
                }
            }
            if ("strict".equals(validationMode)) {
                inst.setSchemaValidationMode(ValidationMode.STRICT);
            } else {
                inst.setSchemaValidationMode(ValidationMode.STRIP);
            }
            //inst.setURIResolver(factory.getUnderlyingConfiguration().getURIResolver());

            inst.setURIResolver(new StandardURIResolver() {
                public Source resolve(String href, String base) throws XPathException {
                    try {
                        String uri = ResolveURI.makeAbsolute(href, base).toString();
                        String file = documentUriMap.get(uri);
                        if (file != null) {
                            return new StreamSource(file);
                        }
                    } catch (URISyntaxException err) {
                        // delegate to superclass
                    }
                    return super.resolve(href, base);
                }
            });

            inst.getUnderlyingController().setUnparsedTextURIResolver(new StandardUnparsedTextResolver() {
                public Reader resolve(URI absoluteURI, String encoding, Configuration config) throws XPathException {
                    File file = unparsedtextUriMap.get(absoluteURI.toString());
                    if (file != null) {
                        try {
                            return new FileReader(file);
                        } catch (IOException e) {
                            throw new XPathException(e);
                        }
                    } else {
                        return super.resolve(absoluteURI, encoding, config);
                    }
                }
            });

            inst.setErrorListener(errorListener);
            inst.getUnderlyingController().setRecoveryPolicy(
                    recoverRecoverable
                            ? (showWarnings ? Configuration.RECOVER_WITH_WARNINGS : Configuration.RECOVER_SILENTLY)
                            : Configuration.DO_NOT_RECOVER);
            // To avoid test results being dependent on the date and time (and timezone), set a fixed
            // date and time for the run
            //((Controller)inst).setCurrentDateTime(new DateTimeValue("2005-01-01T12:49:30.5+01:00"));

            inst.setMessageListener(new MessageListener() {
                public void message(XdmNode content, boolean terminate, SourceLocator locator) {
                    actualMessages.add(content.toString());
                }
            });
            if (streaming && absXMLName != null) {
                inst.setSource(new StreamSource(absXMLName));
            }
            if (sourceNode != null) {
                inst.setInitialContextNode(sourceNode);
                inst.getUnderlyingController().setStripSourceTrees(false);
            }
            inst.setDestination(sr);
            ////////////////////////////////////////
            inst.transform();        // do the work!
            ////////////////////////////////////////
            success = true;
            if ((!expectedErrors.isEmpty() || allowAnyError) && principalResultDocuments == null && !timingOnly) {
                println("Test failed. Expected error " + listErrors(expectedErrors) + ", got success");
                success = false;
                writeTestcaseElement(testName, "differ",
                        "Expected error " + (allowAnyError ? "*" : listErrors(expectedErrors)) + ", got success");
                testsFailed++;
            }
        } catch (SaxonApiException e) {
            // Static or dynamic error in stylesheet
            if (timingOnly) {
                return;
            }
            Set<QName> actual = errorListener.errorCodes;
            QName code = e.getErrorCode();
            if (code != null) {
                actual.add(code);
            }
            if (allowAnyError) {
                println("Test succeeded (" + listErrors(actual) + ')');
                writeTestcaseElement(testName, "full", "Error " + listErrors(actual) + " as permitted");
            } else if (intersects(expectedErrors, actual)) {
                println("Test succeeded (" + listErrors(actual) + ')');
                writeTestcaseElement(testName, "full", "Error " + listErrors(actual) + " as expected");
            } else if (!expectedErrors.isEmpty()) {
                println("Test succeeded (??) (expected " + listErrors(expectedErrors) + ", got " + listErrors(actual) + ')');
                writeTestcaseElement(testName, "different-error", "Expected " +
                        listErrors(expectedErrors) + " got " + listErrors(actual));
            } else {
                //e.printStackTrace();
                println("Test failed. Expected success, got " + listErrors(actual));
                writeTestcaseElement(testName, "differ", "Expected success, got " + listErrors(actual));
                testsFailed++;
            }
            return;
        } catch (Exception e2) {
            // Unexpected crash - print stack trace and continu with the next test
            writeTestcaseElement(testName, "differ", "Crashed (" + e2.getMessage() + ") - see stack trace");
            success = false;
            e2.printStackTrace();
            testsFailed++;
        }


        if (success && !timingOnly) {
            // Test completed without failure - compare the results
            boolean same = true;
            for (ComparisonTask task : comparisonTasks) {
                boolean ok = compare(task.actualResults, task.referenceResults, task.comparisonMode, task.ignoreIndentation);
                if (!ok) {
                    same = false;
                    break;
                }
            }
            if (same) {
                String messageComment = compareMessages(expectedMessages, actualMessages);
                writeTestcaseElement(testName, "full",
                        (messageComment == null ? null : "Message output differs: " + messageComment));
                if (messageComment != null) {
                    System.err.println("*** " + messageComment + " ***");
                }
            } else {
                writeTestcaseElement(testName, "differ", null);
                testsFailed++;
            }
        }
    }

    private static boolean intersects(Set<QName> a, Set<QName> b) {
        for (QName s : b) {
            if (a.contains(s)) {
                return true;
            }
        }
        return false;
    }

    private static String getCanonicalPath(File file) {
        try {
            return file.getCanonicalPath();
        } catch (IOException err) {
            return file.getAbsolutePath();
        }
    }

    private void writeResultFilePreamble(Configuration config, String date) throws IOException, XPathException, XMLStreamException {
        Writer resultWriter = new BufferedWriter(new FileWriter(new File(testSuiteDir + "/SaxonResults/results"
                + Version.getProductVersion() + ".xml")));
        Properties resultProperties = new Properties();
        resultProperties.setProperty(OutputKeys.METHOD, "xml");
        resultProperties.setProperty(OutputKeys.INDENT, "yes");
        resultProperties.setProperty(SaxonOutputKeys.LINE_LENGTH, "120");
        results = config.getSerializerFactory().getXMLStreamWriter(
                new StreamResult(resultWriter), resultProperties);

        results.writeStartElement("test-suite-result");
        results.writeDefaultNamespace(RNS);
        results.writeStartElement("implementation");
        results.writeAttribute("name", "Saxon-EE");
        results.writeAttribute("version", Version.getProductVersion());
        results.writeAttribute("anonymous-result-column", "false");
        results.writeEmptyElement("organization");
        results.writeAttribute("name", "http://www.saxonica.com/");
        results.writeAttribute("anonymous", "false");
        results.writeEmptyElement("submitter");
        results.writeAttribute("name", "Michael Kay");
        results.writeAttribute("email", "mike@saxonica.com");
        outputDiscretionaryItems();
        results.writeEndElement(); //implementation
        results.writeEmptyElement("test-run");
        results.writeAttribute("dateRun", date);
        results.writeAttribute("testsuiteVersion", "1.1.0");
    }

    private void writeTestcaseElement(String name, String result, String comment) {
        try {
            results.writeEmptyElement("testcase");
            results.writeAttribute("name", name);
            results.writeAttribute("result", result);
            if (comment != null) {
                results.writeAttribute("comment", comment);
            }
        } catch (XMLStreamException e) {
            System.err.println("*** Failed to write to results file: " + e.getMessage());
            e.printStackTrace();
        }
    }

    protected void writeResultFilePostamble() throws XMLStreamException {
        results.writeEndElement(); //test-suite-result
        results.close();
    }

    private static String getAttribute(XdmItem element, String attributeName) throws SaxonApiException {
        return ((XdmNode) element).attribute(attributeName);
    }

    private static QName getQNameAttribute(XPathCompiler xpath, XdmItem contextItem, String attributePath) throws SaxonApiException {
        String exp = "for $att in " + attributePath +
                " return if (contains($att, ':')) then resolve-QName($att, $att/..) else QName('', $att)";
        XdmAtomicValue qname = (XdmAtomicValue) xpath.evaluateSingle(exp, contextItem);
        return (qname == null ? null : (QName) qname.getValue());
    }

    private String listErrors(Set<QName> errors) {
        if (errors.isEmpty()) {
            return "{}";
        } else {
            FastStringBuffer sb = new FastStringBuffer(40);
            boolean first = true;
            for (QName error : errors) {
                if (!first) {
                    sb.append('|');
                }
                first = false;
                sb.append(error.getLocalName());
            }
            return sb.toString();
        }
    }

    protected String getResultDirectoryName() {
        return "SaxonResults";
    }

    protected boolean isExcluded(String testName) {
        return false;
    }

    private XdmNode buildSourceDoc(String xml, String initialContextPath, String validationMode, Processor f, XsltExecutable exec) throws SaxonApiException {
        XdmNode sourceNode;
        DocumentBuilder builder = f.newDocumentBuilder();
        if (exec != null) {
            builder.setWhitespaceStrippingPolicy(exec.getWhitespaceStrippingPolicy());
        }
        Source ss = new StreamSource(new File(xml));
        if ("strict".equals(validationMode)) {
            SchemaValidator sv = f.getSchemaManager().newSchemaValidator();
            sv.setLax(false);
            builder.setSchemaValidator(sv);
        }
        sourceNode = builder.build(ss);

        if (initialContextPath != null) {
            XPathExecutable expr = f.newXPathCompiler().compile(initialContextPath);
            XPathSelector xs = expr.load();
            xs.setContextItem(sourceNode);
            sourceNode = ((XdmNode) xs.evaluateSingle());
        }
        return sourceNode;
    }

    /**
     * Construct source object. This method allows subclassing e.g. to build a DOM or XOM source.
     *
     * @param xml
     * @return
     * @throws XPathException
     */

    protected Source buildSource(String xml, String validationMode) {
        Source ss = new StreamSource(new File(xml));
        if ("strict".equals(validationMode)) {
            ss = AugmentedSource.makeAugmentedSource(ss);
            ((AugmentedSource) ss).setSchemaValidationMode(Validation.STRICT);
        }
        return ss;
    }


    static CanonicalXML canon = new CanonicalXML();

    private boolean compare(String outfile, String reffile, String comparator, boolean ignoreIndentation) {
        if (reffile == null) {
            println("*** No reference results available");
            return false;
        }
        File outfileFile = new File(outfile);
        File reffileFile = new File(reffile);

        if (!reffileFile.exists()) {
            println("*** No reference results available in " + reffileFile);
            return false;
        }

        // try direct comparison first

        String refResult = null;
        String actResult = null;

        try {
            // This is decoding bytes assuming the platform default encoding
            FileReader reader1 = new FileReader(outfileFile);
            FileReader reader2 = new FileReader(reffileFile);
            char[] contents1 = new char[65536];
            char[] contents2 = new char[65536];
            int size1 = reader1.read(contents1, 0, 65536);
            int size2 = reader2.read(contents2, 0, 65536);
            reader1.close();
            reader2.close();
            int offset1 = 0;
            int offset2 = 0;
            if (contents1[0] == '\u00ef' && contents1[1] == '\u00bb' && contents1[2] == '\u00bf') {
                offset1 += 3;
            }
            if (contents2[0] == '\u00ef' && contents2[1] == '\u00bb' && contents2[2] == '\u00bf') {
                offset2 += 3;
            }
            actResult = (size1 == -1 ? "" : new String(contents1, offset1, size1 - offset1));
            refResult = (size2 == -1 ? "" : new String(contents2, offset2, size2 - offset2));
            boolean b = compareContent(actResult, refResult, comparator, ignoreIndentation);
            if (b) {
                return true;
            }

        } catch (Exception e) {
            e.printStackTrace();
            return false;
        }

        if (comparator.endsWith("-output")) {
            System.err.println("*** Serialized results differ: check by hand");
        }

        // HTML: can't do logical comparison

        if (comparator.equals("html-output")) {
            // TODO: Tidy gets upset by byte-order-marks. Use the strings constructed above as input.
            try {
                Tidy tidy = new Tidy();
                tidy.setXmlOut(true);
                tidy.setQuiet(true);
                tidy.setShowWarnings(false);
                tidy.setCharEncoding(org.w3c.tidy.Configuration.UTF8);
                InputStream in1 = new FileInputStream(outfile);
                File xml1 = new File(outfile + ".xml");
                xml1.createNewFile();
                OutputStream out1 = new FileOutputStream(xml1);
                tidy.parse(in1, out1);
                InputStream in2 = new FileInputStream(reffile);
                File xml2 = new File(reffile + ".xml");
                xml2.createNewFile();
                OutputStream out2 = new FileOutputStream(xml2);
                tidy.parse(in2, out2);
                in1.close();
                in2.close();
                out2.close();
                return compare(xml1.toString(), xml2.toString(), "xml", ignoreIndentation);
            } catch (IOException e) {
                e.printStackTrace();
                return false;
            }
        } else if (comparator.equals("xhtml-output")) {
            refResult = canonizeXhtml(refResult);
            actResult = canonizeXhtml(actResult);
            return (actResult.equals(refResult));

        } else if (comparator.equals("xml-frag")) {
            try {
                boolean results = compareFragments(outfileFile, reffileFile, outfile);

                return compareFragments(outfileFile, reffileFile, outfile);
            } catch (Exception err2) {
                println("Failed to compare results for: " + outfile);
                err2.printStackTrace();
                return false;
            }
        } else {
            // convert both files to Canonical XML and compare them again
            try {
//                InputSource out = new InputSource(new FileInputStream(outfileFile));
//                InputSource ref = new InputSource(new FileInputStream(reffileFile));
                InputSource out = new InputSource(outfileFile.toURI().toString());
                InputSource ref = new InputSource(reffileFile.toURI().toString());
                String outxml = canon.toCanonicalXML2(resultParser, out, false);
                String refxml = canon.toCanonicalXML2(resultParser, ref, false);
                if (!outxml.equals(refxml)) {
                    if (ignoreIndentation) {
                        // try comparing again, this time without whitespace nodes
                        outxml = canon.toCanonicalXML2(resultParser, out, true);
                        refxml = canon.toCanonicalXML2(resultParser, ref, true);
                        if (outxml.equals(refxml)) {
                            //System.err.println("*** Match after stripping whitespace nodes: " + outfile);
                            return true;
                        }
                    }
                    println("Mismatch with reference results: " + outfile);
                    println("REFERENCE RESULTS:");
                    println(truncate(refxml));
                    println("ACTUAL RESULTS:");
                    println(truncate(outxml));
                    findDiff(refxml, outxml);
                    return false;
                } else {
                    return true;
                }
            } catch (Exception err) {
                try {
                    println("Failed to compare results for: " + outfile + ": " + err.getMessage());
                    err.printStackTrace();
                    println("** Attempting XML Fragment comparison");
                    boolean b = compareFragments(outfileFile, reffileFile, outfile);
                    println("** " + (b ? "Success" : "Still different"));
                    return b;
                } catch (Exception err2) {
                    println("Again failed to compare results for: " + outfile);
                    err2.printStackTrace();
                }
                return false;
            }
        }
    }

    private boolean compareContent(String actResult, String refResult, String comparator, boolean ignoreIndentation) {
        try {
            actResult = actResult.replace("\r\n", "\n");
            refResult = refResult.replace("\r\n", "\n");
            if (actResult.startsWith("\uFEFF")) actResult = actResult.substring(1);
            if (refResult.startsWith("\uFEFF")) refResult = refResult.substring(1);
            if (actResult.equals(refResult)) {
                return true;
            }
            if (ignoreIndentation && stripIndentation(actResult, comparator).equals(stripIndentation(refResult, comparator))) {
                return true;
            }
            int size1 = actResult.length();
            int size2 = refResult.length();
            if (size1 == 0) {
                if (refResult.startsWith("<?xml") && size2 < 60 && refResult.endsWith("?>")) {
                    // we'll assume it's just an XML declaration
                    return true;
                }
                println("** ACTUAL RESULTS EMPTY; REFERENCE RESULTS LENGTH " + size2);
                return false;
            }
            if (size2 == 0) {
                if (actResult.startsWith("<?xml") && size1 < 60 && actResult.endsWith("?>")) {
                    // we'll assume it's just an XML declaration
                    return true;
                }
                println("** REFERENCED RESULTS EMPTY; ACTUAL RESULTS LENGTH " + size2);
                return false;
            }
//            int min= Math.min(size1, size2);
//            if (actResult.substring(0, min).equals(refResult.substring(0,min))) {
//                if (size1 < size2) {
//                    System.err.println("** ACTUAL = FIRST " + min + " CHARS OF REFERENCE; EXCESS = §" + refResult.substring(min) + "§");
//                } else {
//                    System.err.println("** REFERENCE = FIRST " + min + " CHARS OF ACTUAL; EXCESS = §" + actResult.substring(min) + "§");
//                }
//            } else {
//                for (int i=0; i<min; i++) {
//                    if (actResult.charAt(i) != refResult.charAt(i)) {
//                        System.err.println("** RESULTS DIFFER AT CHAR " + i);
//                        break;
//                    }
//                }
//            }
            return false;
        } catch (Exception e) {
            e.printStackTrace();
            return false;
        }
    }

    private String stripIndentation(String xml, String comparator) {
        xml = xml.trim();
        xml = xml.replaceAll("\\s+<", "<");
        xml = xml.replaceAll(">\\s+", ">");
        if (comparator.equals("html-output")) {
            xml = xml.replaceAll("[\\s\\t\\r\\n]+", " ");
        }
        return xml;
    }

    XsltExecutable xhtmlCanonizer;

    private String canonizeXhtml(String input) {
        try {
            XsltExecutable canonizer = getXhtmlCanonizer();
            XsltTransformer t = canonizer.load();
            StringWriter sw = new StringWriter();
            InputSource is = new InputSource(new StringReader(input));
            SAXSource ss = new SAXSource(resultParser, is);
            t.setSource(ss);
            t.setDestination(canonizer.getProcessor().newSerializer(sw));
            t.transform();
            return sw.toString();
        } catch (SaxonApiException err) {
            println("*** Failed to compile/run XHTML canonicalizer stylesheet");
            err.printStackTrace();
        }
        return "";
    }

    private XsltExecutable getXhtmlCanonizer() throws SaxonApiException {
        if (xhtmlCanonizer == null) {
            Source source = new StreamSource(new File(testSuiteDir + "/SaxonResults/canonizeXhtml.xsl"));
            xhtmlCanonizer = factory.newXsltCompiler().compile(source);
        }
        return xhtmlCanonizer;
    }

    private boolean compareFragments(File outfileFile, File reffileFile, String outfile) {
        // if we can't parse the output as a document, try it as an external entity, with space stripping
        String outurl = outfileFile.toURI().toString();
        String refurl = reffileFile.toURI().toString();
        String outdoc = "<?xml version='1.1'?><!DOCTYPE doc [ <!ENTITY e SYSTEM '" + outurl + "'>]><doc>&e;</doc>";
        String refdoc = "<?xml version='1.1'?><!DOCTYPE doc [ <!ENTITY e SYSTEM '" + refurl + "'>]><doc>&e;</doc>";
        InputSource out2 = new InputSource(new StringReader(outdoc));
        InputSource ref2 = new InputSource(new StringReader(refdoc));
        String outxml2 = canon.toCanonicalXML(fragmentParser, out2, true);
        String refxml2 = canon.toCanonicalXML(fragmentParser, ref2, true);

        if (outxml2 != null && refxml2 != null && !outxml2.equals(refxml2)) {
            println("Mismatch with reference results: " + outfile);
            println("REFERENCE RESULTS:");
            println(truncate(refxml2));
            println("ACTUAL RESULTS:");
            println(truncate(outxml2));
            findDiff(refxml2, outxml2);
            return false;
        } else if (outxml2 == null) {
            println("Cannot canonicalize actual results");
            return false;
        } else if (refxml2 == null) {
            println("Cannot canonicalize reference results");
            return false;
        }
        return true;
    }

    private static String truncate(String s) {
        if (s.length() > 200) return s.substring(0, 200);
        return s;
    }

    private static void findDiff(String s1, String s2) {
        FastStringBuffer sb1 = new FastStringBuffer(s1.length());
        sb1.append(s1);
        FastStringBuffer sb2 = new FastStringBuffer(s2.length());
        sb2.append(s2);
        int i = 0;
        while (true) {
            if (s1.charAt(i) != s2.charAt(i)) {
                int j = (i < 50 ? 0 : i - 50);
                int k = (i + 50 > s1.length() || i + 50 > s2.length() ? i + 1 : i + 50);
                System.err.println("Different at char " + i + "\n+" + s1.substring(j, k) +
                        "\n+" + s2.substring(j, k));
                break;
            }
            if (i >= s1.length()) break;
            if (i >= s2.length()) break;
            i++;
        }
    }

    /**
     * Compare the actual messages output with the expected messages in the test catalog
     *
     * @param expectedMessages the expected messages
     * @param actualMessages   the actual messages
     * @return the content of the first actual message that differs; or null if no differences found.
     */

    private String compareMessages(List<String> expectedMessages, List<String> actualMessages) {
        if (expectedMessages.size() != actualMessages.size() && expectedMessages.size() != 0) {
            return "expected " + expectedMessages.size() + " messages, got " + actualMessages.size();
        }
        for (int i = 0; i < expectedMessages.size(); i++) {
            // Note, we expect the messages to be listed in the right order - a questionable assumption
            boolean b = compareContent(expectedMessages.get(i), actualMessages.get(i), "xml-frag", true);
            if (!b) {
                return actualMessages.get(i);
            }
        }
        return null;
    }

    private void outputDiscretionaryItems() throws XMLStreamException {
        results.writeEmptyElement("discretionary-items");
    }

    private void setLocalizers(Configuration config) {
        config.setLocalizerFactory(new LocalizerFactory() {

            public Numberer getNumberer(String language, String country) {
                Numberer n;
                if (language == null) {
                    n = new Numberer_en();
                } else if (language.equals("da")) {
                    n = new net.sf.saxon.option.local.Numberer_da();
                } else if (language.equals("de")) {
                    n = new net.sf.saxon.option.local.Numberer_de();
                } else if (language.equals("fr")) {
                    n = new net.sf.saxon.option.local.Numberer_fr();
                } else if (language.equals("fr-BE")) {
                    n = new net.sf.saxon.option.local.Numberer_frBE();
                } else if (language.equals("he")) {
                    n = new net.sf.saxon.option.local.Numberer_he();
                } else if (language.equals("it")) {
                    n = new net.sf.saxon.option.local.Numberer_it();
                } else if (language.equals("nl")) {
                    n = new net.sf.saxon.option.local.Numberer_nl();
                } else if (language.equals("nl-BE")) {
                    n = new net.sf.saxon.option.local.Numberer_nlBE();
                } else if (language.equals("sv")) {
                    n = new net.sf.saxon.option.local.Numberer_sv();
                } else {
                    n = new Numberer_en();
                }
                if (country != null) {
                    n.setCountry(country);
                }
                return n;
            }
        });

    }

    private class MyErrorListener extends StandardErrorListener {

        public Set<QName> errorCodes = new HashSet<QName>();

        /**
         * Receive notification of a recoverable error.
         */

        public void error(TransformerException exception) {
            if (exception instanceof XPathException) {
                StructuredQName sq = ((XPathException) exception).getErrorCodeQName();
                if (sq != null) {
                    QName code = new QName(sq.getURI(), sq.getLocalPart());
                    errorCodes.add(code);
                    // following lines suppressed because XSLT test error248 fails
//                    if ("FODC0005".equals(sq.getLocalPart()) || "FODC0002".equals(sq.getLocalPart())) {
//                        super.fatalError(exception);
//                        return;
//                    }
                } else {
                    errorCodes.add(new QName("UnknownError"));
                }
            }
            super.error(exception);
        }

        /**
         * Receive notification of a non-recoverable error.
         */

        public void fatalError(TransformerException exception) {
            if (exception instanceof XPathException) {
                StructuredQName sq = ((XPathException) exception).getErrorCodeQName();
                if (sq != null) {
                    QName code = new QName(sq.getURI(), sq.getLocalPart());
                    errorCodes.add(code);
                    // following lines suppressed because XSLT test error248 fails
//                    if ("FODC0005".equals(sq.getLocalPart()) || "FODC0002".equals(sq.getLocalPart())) {
//                        super.fatalError(exception);
//                        return;
//                    }
                } else {
                    errorCodes.add(new QName("UnknownError"));
                }
            }
            super.fatalError(exception);
        }

        /**
         * Receive notification of a warning.
         */

        public void warning(TransformerException exception) {
            if (showWarnings) {
                super.warning(exception);
            }
        }

        /**
         * Make a clean copy of this ErrorListener. This is necessary because the
         * standard error listener is stateful (it remembers how many errors there have been)
         */

        public StandardErrorListener makeAnother(int hostLanguage) {
            return new MyErrorListener();
        }

    }

    private class ComparisonTask {
        public String referenceResults;
        public String actualResults;
        public String comparisonMode;
        public boolean ignoreIndentation;
    }
}
