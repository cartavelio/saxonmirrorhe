using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Saxon.Api;
using Saxon.Hej.om;
using DocumentBuilder = Saxon.Api.DocumentBuilder;
using NullDestination = Saxon.Api.NullDestination;
using Processor = Saxon.Api.Processor;
using SchemaManager = Saxon.Api.SchemaManager;
using SchemaValidator = Saxon.Api.SchemaValidator;

namespace TestDriver {
    /**
         * This class runs the W3C XML Schema Test Suite, driven from the test catalog.
         */
    public class SchemaTestSuiteDriver {
        private const string testNS = "http://www.w3.org/XML/2004/xml-schema-test-suite/";

        /**
         * Run the testsuite using Saxon.
         *
         * @param args Array of parameters passed to the application
         * via the command line.
         */
        private string testSuiteDir;

        private string exceptionsFile;
        
        private bool onwards;
        private bool debug;
        private string versionUnderTest = "1.1";
        private XmlWriter results;
        private QName xlinkHref;
        private readonly SortedSet<string> failingTests = new();

        private XdmNode getChildElement(XdmNode parent, QName child) {
            IEnumerator e = parent.EnumerateAxis(XdmAxis.Child, child);
            return e.MoveNext() ? (XdmNode) e.Current : null;
        }

        
        private XdmNode getLinkedDocument(XdmNode element, Processor processor) {
            String href = element.GetAttributeValue(xlinkHref);
            DocumentBuilder builder = processor.NewDocumentBuilder();
            Uri target = new Uri(element.BaseUri, href);
            builder.LineNumbering = true;
            return builder.Build(target);
        }
        
        private bool validateLinkedDocument(XdmNode element, Processor processor) {
            try {
                String href = element.GetAttributeValue(xlinkHref);
                Uri target = new Uri(element.BaseUri, href);
                SchemaValidator v = processor.SchemaManager.NewSchemaValidator();
                v.Lax = false;
                List<ValidationFailure> failures = new ();
                v.InvalidityListener = failures.Add;
                v.SetDestination(new NullDestination());
                v.Validate(target);

                if (debug) {
                    foreach (ValidationFailure invalidity in failures)
                    {
                        Console.Error.WriteLine("+++> " + invalidity.Message);
                    }
                }

                return failures.Count == 0;
            }
            catch (Exception e) {
                Console.WriteLine(e);
                return false;
            }
        }

        public static void MainXSD(string[] args) {
            long startTime = Saxon.Ejava.lang.System.nanoTime();
            new SchemaTestSuiteDriver().go(args);
            long endTime = Saxon.Ejava.lang.System.nanoTime();
            Console.Error.WriteLine("Execution time: "+Saxon.Hej.trans.Timer.showExecutionTimeNano((endTime-startTime)));

        }
        /**
         * Run the tests
         * @param args command line arguments
         * @throws SAXException
         * @throws ParserConfigurationException
         * @throws XPathException
         * @throws IOException
         * @throws URISyntaxException
         */
        private void go(String[] args) {
            string usage =
                "SchemaTestSuiteDriver testDir -lang:XSD [-w] [-onwards] -c:contributor? -s:setName? -g:groupName? [-o:outputdir]";
            if (args.Length == 0 || args[0] == "-?") {
                Console.WriteLine(usage);
            }

            Processor processor = new Processor(true);
            Console.WriteLine("Testing Saxon " + processor.ProductVersion);

            testSuiteDir = args[0];
            if (testSuiteDir.EndsWith("/")) {
                testSuiteDir = testSuiteDir.Substring(0, testSuiteDir.Length - 1);
            }

            string saxonDir = testSuiteDir.Replace("xsdtests", "xsdtests-results");
            exceptionsFile = saxonDir + "/exceptions11.xml";

            string outputDir = saxonDir;

            string testSetPattern = null; // TODO use a regex
            string testGroupPattern = null;
            string contributor = null;
            var exceptions = new Dictionary<string, string>();

            for (int i = 1; i < args.Length; i++) {
                if (args[i] == ("-w")) {
                    //showWarnings = true;
                }
                else if (args[i] == ("-onwards")) {
                    onwards = true;
                }
                else if (args[i] == ("-debug")) {
                    debug = true;
                }
                else if (args[i].StartsWith("-c:")) {
                    contributor = args[i][3..];
                }
                else if (args[i].StartsWith("-s:")) {
                    testSetPattern = args[i][3..];
                }
                else if (args[i].StartsWith("-g:")) {
                    testGroupPattern = args[i][3..];
                } 
                else if (args[i].StartsWith("-v:")) {
                    // Note, although we allow setting -v:1.0 to run the tests appropriate to a 1.0 processor,
                    // we don't expect 100% success, because Saxon does not implement a conformant 1.0 processor,
                    // it only offers 1.1 conformance.
                    versionUnderTest = args[i][3..];
                }
                else if (args[i].StartsWith("-o:")) {
                    outputDir = args[i][3..];
                }
                else if (args[i].StartsWith("-exceptions:")) {
                    exceptionsFile = args[i].Substring("-exceptions:".Length);
                }
                else if (args[i] == "-?") {
                    Console.WriteLine(usage);
                }
            }

            int total = 39700;
            int passed = 0;
            int failed = 0;

            try {
                xlinkHref = new QName("xlink", "http://www.w3.org/1999/xlink", "href");

                QName testSetNT = new QName(testNS, "testSet");
                QName instanceDocumentNT = new QName(testNS, "instanceDocument");
                QName currentNT = new QName(testNS, "current");

                QName validityAtt = new QName("", "", "validity");
                QName nameAtt = new QName("", "", "name");
                QName contributorAtt = new QName("", "", "contributor");

                DocumentBuilder builder = processor.NewDocumentBuilder();
                builder.BaseUri = new Uri(testSuiteDir + "/");
                XdmNode catalog = builder.Build(
                    new FileStream(testSuiteDir + "/suite.xml", FileMode.Open, FileAccess.Read, FileShare.Read));

                
                results = XmlWriter.Create(File.OpenWrite(outputDir + "/SaxonResults"
                                                        + processor.ProductVersion + "n.xml"),
                    new XmlWriterSettings(){Indent = true});
                results.WriteStartElement("testSuiteResults", testNS);
                results.WriteAttributeString("xmlns", "saxon", null, NamespaceConstant.SAXON);
                results.WriteAttributeString("suite", "TS_2006");
                results.WriteAttributeString("processor", "Saxon-EE " + Saxon.Hej.Version.getProductVersion());
                results.WriteAttributeString("submitDate", DateTime.Now.ToString(CultureInfo.InvariantCulture));
                results.WriteAttributeString("pubicationPermission", "public");
                
                XdmNode exceptionsDoc = builder.Build(
                    new FileStream(exceptionsFile,
                        FileMode.Open, FileAccess.Read, FileShare.Read));

                foreach (XdmNode exception in exceptionsDoc.Select(Steps.Descendant("exception"))) {
                    string set = exception.GetAttributeValue("test-set");
                    string group = exception.GetAttributeValue("test-case");
                    string comment = "unspecified reason";
                    if (exception.Children("reason").Count() > 0) {
                        comment = exception.Children("reason").First().StringValue;
                    }
                    exceptions[set + "#" + group] = comment;
                }

                foreach (var testSetRef in catalog.Select(Steps.Descendant("testSetRef"))) {
                    XdmNode testSetDoc = getLinkedDocument(testSetRef, processor);
                    XdmNode testSetElement = getChildElement(testSetDoc, testSetNT);

                    if (testSetElement == null) {
                        feedback("test set doc has no TestSet child: " + testSetDoc.BaseUri, true);
                        continue;
                    }

                    string testSetName = testSetElement.GetAttributeValue(nameAtt);
                    if (testSetPattern != null && !testSetName.StartsWith(testSetPattern)) {
                        continue;
                    }

                    if (contributor != null && contributor != testSetElement.GetAttributeValue(contributorAtt)) {
                        continue;
                    }

                    string testSetVn = testSetElement.GetAttributeValue("version");
                    
                    if (testSetVn == null) {
                        testSetVn = "";
                    }
                    if (testSetVn == "full-xpath-in-CTA") {
                        testSetVn = "1.1";
                    }
                    if (testSetVn.Length != 0 && !testSetVn.Contains(versionUnderTest)) {
                        continue;
                    }

                    foreach (var testGroup in testSetElement.Children("testGroup")) {
                        string testGroupName = testGroup.GetAttributeValue(nameAtt);
                        bool isException = exceptions.TryGetValue(testSetName + "#" + testGroupName, out var exception);

                        if (testGroupPattern != null && !testGroupName.StartsWith(testGroupPattern)) {
                            continue;
                        }
                        
                        String testGroupVn = testGroup.GetAttributeValue("version");
                        if (testGroupVn == null) {
                            testGroupVn = "";
                        }
                        if (testGroupVn == "full-xpath-in-CTA") {
                            testGroupVn = "1.1";
                        }
                        if (testGroupVn.Length != 0 && !testGroupVn.Contains(versionUnderTest)) {
                            continue;
                        }

                        Console.WriteLine("TEST SET " + testSetName + " GROUP " + testGroupName, false);
                        if (onwards) {
                            testGroupPattern = null;
                            testSetPattern = null;
                        }

                        Processor testConfig = new Processor(true);
                        testConfig.SetProperty(Feature.XSD_VERSION, versionUnderTest);

                        SchemaManager schemaManager = testConfig.SchemaManager;


                        //testConfig.setHostLanguage(Configuration.XML_SCHEMA);
                        testConfig.SetProperty("http://saxon.sf.net/feature/validation-warnings", "true");
                        bool schemaQueried = false;
                        string bugzillaRef = null;
                        bool versionCondition(XdmNode node) =>
                            node.GetAttributeValue("version") == null 
                            || node.GetAttributeValue("version").Contains(versionUnderTest);

                        bool schemaOK = true;
                        foreach (XdmNode schemaTest in testGroup.Children("schemaTest")) {

                            bugzillaRef = null;
                            string testName = schemaTest.GetAttributeValue("name");
                            if (isException) {
                                results.WriteStartElement("testResult");
                                results.WriteAttributeString("set", testSetName);
                                results.WriteAttributeString("group", testGroupName);
                                results.WriteAttributeString("test", testName);
                                results.WriteAttributeString("validity", "notKnown");
                                results.WriteAttributeString("saxon", "outcome", NamespaceConstant.SAXON, "notRun");
                                results.WriteAttributeString("saxon", "comment", NamespaceConstant.SAXON, exception);
                                results.WriteEndElement();
                                
                                continue;
                            }
                            
                            string schemaTestVn = schemaTest.GetAttributeValue("version");
                            if (schemaTestVn != null && !schemaTestVn.Contains(versionUnderTest)) {
                                continue;
                            }

                            bool queried = false;
                            XdmNode statusElement = getChildElement(schemaTest, currentNT);
                            if (statusElement != null) {
                                string status = statusElement.GetAttributeValue("status");
                                queried = "queried" == status || "disputed-test" == status || "disputed-spec"  == status;
                                bugzillaRef = statusElement.GetAttributeValue("bugzilla");
                            }

                            if (queried) {
                                schemaQueried = true;
                            }

                            Console.WriteLine("TEST SCHEMA " + testName + (queried ? " (queried)" : ""));
                            
                            
                            foreach (XdmNode schemaDocumentRef in schemaTest.Children("schemaDocument")) {

                                Console.WriteLine("Loading schema at " +
                                                  schemaDocumentRef.GetAttributeValue(xlinkHref));
                                XdmNode schemaDoc;
                                try {
                                    schemaDoc = getLinkedDocument(schemaDocumentRef, testConfig);
                                }
                                catch (Exception schemaParseErr) {
                                    // Happens with documents saying xml version="1.1"
                                    feedback(
                                        "Failed to parse schema document " + schemaDocumentRef + ": " + schemaParseErr.Message, true);
                                    results.WriteComment(schemaParseErr.Message);
                                    schemaOK = false;
                                    break;
                                }

                                XdmNode rootElement = schemaDoc.Children("schema").FirstOrDefault();
                                if (rootElement != null) {
                                    string targetNamespace = rootElement.GetAttributeValue("targetNamespace");
                                    if (targetNamespace != null &&
                                        testConfig.Implementation.isSchemaAvailable(NamespaceUri.of(targetNamespace))) {
                                        // the schema has already been read, typically by an import/include/override in a previous schema document.
                                        // Ignore the duplicate.
                                        continue;
                                    }
                                }

                                List<Error> errorList = new ();
                                schemaManager.ErrorReporter = errorList.Add;
                                try {
                                    schemaManager.Compile(schemaDoc);
                                }
                                catch (Exception e) {
                                    if (errorList.Count == 0) {
                                        feedback(
                                            "In " + testName + ", exception thrown but no errors in ErrorList\n", true);
                                        results.WriteComment(e.Message);
                                        Console.Error.WriteLine(e);
                                        schemaOK = false;
                                        break;
                                    }
                                }

                                foreach (Error err in errorList) {
                                    if (!err.IsWarning) {
                                        schemaOK = false;
                                        break;
                                    }
                                }
                            }


                            XdmNode expected = schemaTest.Children("expected").Where(versionCondition).FirstOrDefault();
                            
                            String expectedOutcome = (expected == null ? "indeterminate" :
                                expected.GetAttributeValue("validity"));
                            bool ok = "indeterminate" == expectedOutcome ||
                                      "implementation-defined" == expectedOutcome ||
                                      "implementation-dependent" == expectedOutcome ||
                                      schemaOK == ("valid" == expectedOutcome);
                            if (ok) {
                                passed++;
                            }
                            else if (queried) {
                                if (debug) {
                                    feedback("Failed disputed schema test " + testName 
                                                                              + " (expected outcome " + expectedOutcome + ")", true);
                                }
                            }
                            else {
                                if (debug) {
                                    feedback("Failed schema test " + testName 
                                                                     + " (expected outcome " + expectedOutcome + ")", true);
                                }
                                failed++;
                                failingTests.Add(contributor + "/" + testSetName + "/" + testGroupName + "/" + testName);
                            }

                            //feedback("Passed " + passed + " failed " + failed + " total " + total, true);
                            results.WriteStartElement("testResult");
                            results.WriteAttributeString("set", testSetName);
                            results.WriteAttributeString("group", testGroupName);
                            results.WriteAttributeString("test", testName);
                            if (queried) {
                                results.WriteAttributeString("saxon", "queried", "true");
                                results.WriteAttributeString("saxon", "bugzilla", bugzillaRef);
                            }
                            results.WriteAttributeString("validity", (schemaOK ? "valid" : "invalid"));
                            results.WriteAttributeString("saxon", "outcome", NamespaceConstant.SAXON, (ok ? "same" : "different"));
                            results.WriteAttributeString("saxon", "comment", NamespaceConstant.SAXON, exception);
                            results.WriteEndElement();
                        }

                        if (schemaOK) {
                            foreach (XdmNode instanceTest in testGroup.Children("instanceTest")) {
                                string testName = instanceTest.GetAttributeValue("name");

                                if (exception != null) {
                                    results.WriteStartElement("testResult");
                                    results.WriteAttributeString("set", testSetName);
                                    results.WriteAttributeString("group", testGroupName);
                                    results.WriteAttributeString("test", testName);
                                    results.WriteAttributeString("validity", "notKnown");
                                    results.WriteAttributeString("saxon", "outcome", NamespaceConstant.SAXON, "notRun");
                                    results.WriteAttributeString("saxon", "comment", NamespaceConstant.SAXON, exception);
                                    results.WriteEndElement();
                                    continue;
                                }
                                
                                string instanceTestVn = instanceTest.GetAttributeValue("version");
                                if (instanceTestVn != null && !instanceTestVn.Contains(versionUnderTest)) {
                                    continue;
                                }

                                bool queried = false;
                                XdmNode statusElement = getChildElement(instanceTest, currentNT);
                                if (statusElement != null) {
                                    string status = statusElement.GetAttributeValue("status");
                                    queried = ("queried" == status);
                                    string instanceBug = statusElement.GetAttributeValue("bugzilla");
                                    if (instanceBug != null) {
                                        bugzillaRef = instanceBug;
                                    }
                                }

                                queried |= schemaQueried;

                                Console.WriteLine("TEST Instance " + testName + (queried ? " (queried)" : ""));

                                XdmNode instanceDocument = getChildElement(instanceTest, instanceDocumentNT);

                                bool success = validateLinkedDocument(instanceDocument, testConfig);

                            
                                XdmNode expected = instanceTest.Children("expected").Where(versionCondition).FirstOrDefault();
                            
                                // XdmNode expected = (XdmNode) xpath.evaluateSingle(
                                //     "expected[not(@version) or contains(@version, '" + versionUnderTest + "')]", instanceTest);
                                String expectedOutcome = (expected == null ? "indeterminate" :
                                    expected.GetAttributeValue("validity"));
                                bool ok = "indeterminate" == expectedOutcome ||
                                          "implementation-defined" == expectedOutcome ||
                                          "implementation-dependent" == expectedOutcome ||
                                          success == ("valid" == expectedOutcome);


                                //XdmNode expected = getChildElement(instanceTest, expectedNT);
                                bool expectedSuccess = expected == null ||
                                                       expected.GetAttributeValue(validityAtt) == "valid";
                                if (ok) {
                                    passed++;
                                }
                                else if (queried) {
                                    if (debug) {
                                        feedback("Failed disputed instance test " + testName 
                                            + " (expected " + (expectedSuccess ? "validity)" : "invalidity)"), true);
                                    }
                                }
                                else {
                                    if (debug) {
                                        feedback("Failed instance test " + testName 
                                                                         + " (expected " + (expectedSuccess ? "validity)" : "invalidity)"), true);
                                    }
                                    failed++;
                                    failingTests.Add(contributor + "/" + testSetName + "/" + testGroupName + "/" + testName);
                                }

                                //feedback("Passed " + passed + " failed " + failed + " total " + total, true);
                                results.WriteStartElement("testResult");
                                results.WriteAttributeString("set", testSetName);
                                results.WriteAttributeString("group", testGroupName);
                                results.WriteAttributeString("test", testName);
                                results.WriteAttributeString("validity", (success ? "valid" : "invalid"));
                                if (queried) {
                                    results.WriteAttributeString("saxon", "queried", "true");
                                    results.WriteAttributeString("saxon", "bugzilla", bugzillaRef);
                                }
                                results.WriteAttributeString("saxon", "outcome", NamespaceConstant.SAXON, (success == expectedSuccess ? "same" : "different"));
                                results.WriteEndElement();
                            }
                        }
                    }
                }
                feedback("Passed " + passed + " failed " + failed + " total " + total, true);
                foreach (string testDetails in failingTests)
                {
                    feedback("Failed " + testDetails, true);
                }
                results.WriteEndElement();
                results.WriteEndDocument();
                results.Close();
            }
            catch (Exception e) {
                Console.Error.WriteLine(e);
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine(e.StackTrace);
                feedback("Test failed: " + e.Message, true);
            }
        }

        protected String getResultDirectoryName() {
            return "SaxonResults";
        }
        

        private void feedback(string message, bool unused) {
            Console.Error.WriteLine(message);
        }
    }
}