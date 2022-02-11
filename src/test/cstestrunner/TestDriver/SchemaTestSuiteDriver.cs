using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using net.sf.saxon.functions;
using net.sf.saxon.s9api;
using Saxon.Api;
using DocumentBuilder = Saxon.Api.DocumentBuilder;
using NullDestination = Saxon.Api.NullDestination;
using Processor = Saxon.Api.Processor;
using SchemaManager = Saxon.Api.SchemaManager;
using SchemaValidator = Saxon.Api.SchemaValidator;
using StaticError = Saxon.Api.StaticError;

namespace TestRunner {
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
        
        private bool onwards = false;
        private bool debug = false;
        private StreamWriter results;
        private QName xlinkHref;
        private SortedSet<string> failingTests = new SortedSet<string>();

        private XdmNode getChildElement(XdmNode parent, QName child) {
            IEnumerator e = parent.EnumerateAxis(XdmAxis.Child, child);
            return e.MoveNext() ? (XdmNode) e.Current : null;
        }

        
        private XdmNode getLinkedDocument(XdmNode element, Processor processor) {
            String href = element.GetAttributeValue(xlinkHref);
            DocumentBuilder builder = processor.NewDocumentBuilder();
            Uri target = new Uri(element.BaseUri, href);
            builder.IsLineNumbering = true;
            return builder.Build(target);
        }
        
        private bool validateLinkedDocument(XdmNode element, Processor processor) {
            try {
                String href = element.GetAttributeValue(xlinkHref);
                Uri target = new Uri(element.BaseUri, href);
                SchemaValidator v = processor.SchemaManager.NewSchemaValidator();
                v.IsLax = false;
                v.ErrorList = new List<ValidationFailure>();
                v.SetSource(target);
                v.SetDestination(new NullDestination());
                v.Run();

                if (debug) {
                    foreach (ValidationFailure invalidity in v.ErrorList)
                    {
                        Console.Error.WriteLine("+++> " + invalidity.GetMessage());
                    }
                }

                return v.ErrorList.Count == 0;
            }
            catch (Exception e) {
                Console.WriteLine(e);
                return false;
            }
        }

        public static void MainXSD(string[] args) {
            long startTime = java.lang.System.nanoTime();
            new SchemaTestSuiteDriver().go(args);
            long endTime = java.lang.System.nanoTime();
            Console.Error.WriteLine("Execution time: "+net.sf.saxon.trans.CommandLineOptions.showExecutionTimeNano((endTime-startTime)));

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
        public void go(String[] args) {
            if (args.Length == 0 || args[0] == "-?") {
                Console.WriteLine(
                    "SchemaTestSuiteDriver testDir -lang:XSD [-w] [-onwards] -c:contributor? -s:setName? -g:groupName?");
            }

            Processor processor = new Processor(true);
            Console.WriteLine("Testing Saxon " + processor.ProductVersion);

            testSuiteDir = args[0];
            if (testSuiteDir.EndsWith("/")) {
                testSuiteDir = testSuiteDir.Substring(0, testSuiteDir.Length - 1);
            }

            string saxonDir = testSuiteDir.Replace("xsdtests", "xsdtests-results");
            exceptionsFile = saxonDir + "/exceptions11.xml";

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
                    contributor = args[i].Substring(3);
                }
                else if (args[i].StartsWith("-s:")) {
                    testSetPattern = args[i].Substring(3);
                }
                else if (args[i].StartsWith("-g:")) {
                    testGroupPattern = args[i].Substring(3);
                }
                else if (args[i].StartsWith("-exceptions:")) {
                    exceptionsFile = args[i].Substring("-exceptions:".Length);
                }
                else if (args[i] == "-?") {
                    Console.WriteLine(
                        "Usage: SchemaTestSuiteDriver testDir [-w] [-s:testSetPattern] [-g:testGroupPattern]");
                }
            }

            int total = 39700;
            int passed = 0;
            int failed = 0;

            try {
                xlinkHref = new QName("xlink", "http://www.w3.org/1999/xlink", "href");

                QName testCaseNT = new QName("", "", "testcase");
                QName commentNT = new QName("", "", "comment");

                QName testSetRefNT = new QName(testNS, "testSetRef");
                QName testGroupNT = new QName(testNS, "testGroup");
                QName testSetNT = new QName(testNS, "testSet");
                QName schemaTestNT = new QName(testNS, "schemaTest");
                QName instanceTestNT = new QName(testNS, "instanceTest");
                QName schemaDocumentNT = new QName(testNS, "schemaDocument");
                QName instanceDocumentNT = new QName(testNS, "instanceDocument");
                QName expectedNT = new QName(testNS, "expected");
                QName currentNT = new QName(testNS, "current");

                QName validityAtt = new QName("", "", "validity");
                QName nameAtt = new QName("", "", "name");
                QName contributorAtt = new QName("", "", "contributor");
                QName setAtt = new QName("", "", "test-set");
                QName groupAtt = new QName("", "", "test-case");
                QName statusAtt = new QName("", "", "status");
                QName bugzillaAtt = new QName("", "", "bugzilla");
                QName targetNamespaceAtt = new QName("", "", "targetNamespace");
                QName schemaVersion = new QName("saxon", "http://saxon.sf.net/", "schemaVersion");


                DocumentBuilder builder = processor.NewDocumentBuilder();
                builder.BaseUri = new Uri(testSuiteDir + "/");
                XdmNode catalog = builder.Build(
                    new FileStream(testSuiteDir + "/suite.xml", FileMode.Open, FileAccess.Read, FileShare.Read));

                
                results = new StreamWriter(saxonDir + "/SaxonResults"
                                                        + processor.ProductVersion + "n.xml");

                results.Write("<testSuiteResults xmlns='" + testNS + "' xmlns:saxon='http://saxon.sf.net/' " +
                              "suite='TS_2006' " +
                              "processor='Saxon-SA (Java) 8.8++' submitDate='2007-01-05' publicationPermission='public'>\n");

                XdmNode exceptionsDoc = builder.Build(
                    new FileStream(exceptionsFile,
                        FileMode.Open, FileAccess.Read, FileShare.Read));

                foreach (XdmNode exception in exceptionsDoc.Select(Steps.Descendant("exception"))) {
                    string set = exception.GetAttributeValue("test-set");
                    string group = exception.GetAttributeValue("test-case");
                    string comment = exception.Children("reason").First().GetStringValue();
                    exceptions[set + "#" + group] = comment;
                }

                IEnumerator testSets = catalog.EnumerateAxis(XdmAxis.Descendant, testSetRefNT);
                while (testSets.MoveNext()) {
                    XdmNode testSetRef = (XdmNode) testSets.Current;
                    XdmNode testSetDoc = getLinkedDocument(testSetRef, processor);
                    XdmNode testSetElement = getChildElement(testSetDoc, testSetNT);

                    if (testSetElement == null) {
                        feedback("test set doc has no TestSet child: " + testSetDoc.BaseUri, true);
                        continue;
                    }

                    String testSetName = testSetElement.GetAttributeValue(nameAtt);
                    if (testSetPattern != null && !testSetName.StartsWith(testSetPattern)) {
                        continue;
                    }

                    if (contributor != null && contributor != testSetElement.GetAttributeValue(contributorAtt)) {
                        continue;
                    }

                    bool needs11 = (testSetElement.GetAttributeValue(schemaVersion) == "1.1");

                    IEnumerator testGroups = testSetElement.EnumerateAxis(XdmAxis.Child, testGroupNT);
                    while (testGroups.MoveNext()) {
                        XdmNode testGroup = (XdmNode) testGroups.Current;

                        string testGroupName = testGroup.GetAttributeValue(nameAtt);
                        bool isException = exceptions.TryGetValue(testSetName + "#" + testGroupName, out var exception);

                        if (testGroupPattern != null && !testGroupName.StartsWith(testGroupPattern)) {
                            continue;
                        }

                        Console.WriteLine("TEST SET " + testSetName + " GROUP " + testGroupName, false);
                        if (onwards) {
                            testGroupPattern = null;
                            testSetPattern = null;
                        }

                        Processor testConfig = new Processor(true);
                        if (needs11) {
                            testConfig.SetProperty("http://saxon.sf.net/feature/xsd-version", "1.1");
                        }

                        SchemaManager schemaManager = testConfig.SchemaManager;


                        //testConfig.setHostLanguage(Configuration.XML_SCHEMA);
                        testConfig.SetProperty("http://saxon.sf.net/feature/validation-warnings", "true");
                        IEnumerator schemaTests = testGroup.EnumerateAxis(XdmAxis.Child, schemaTestNT);
                        bool schemaQueried = false;
                        string bugzillaRef = null;

                        while (schemaTests.MoveNext()) {
                            XdmNode schemaTest = (XdmNode) schemaTests.Current;
                            if (schemaTest == null) {
                                break;
                            }

                            bugzillaRef = null;
                            string testName = schemaTest.GetAttributeValue(nameAtt);
                            if (isException) {
                                results.Write("<testResult set='" + testSetName +
                                              "' group='" + testGroupName +
                                              "' test='" + testName +
                                              "' validity='notKnown' saxon:outcome='notRun' saxon:comment='" +
                                              exception +
                                              "'/>\n");
                                continue;
                            }

                            bool queried = false;
                            XdmNode statusElement = getChildElement(schemaTest, currentNT);
                            if (statusElement != null) {
                                String status = statusElement.GetAttributeValue(statusAtt);
                                queried = ("queried" == status);
                                bugzillaRef = statusElement.GetAttributeValue(bugzillaAtt);
                            }

                            if (queried) {
                                schemaQueried = true;
                            }

                            Console.WriteLine("TEST SCHEMA " + testName + (queried ? " (queried)" : ""));
                            bool success = true;
                            IEnumerator schemata = schemaTest.EnumerateAxis(XdmAxis.Child, schemaDocumentNT);
                            while (schemata.MoveNext()) {
                                XdmNode schemaDocumentRef = (XdmNode) schemata.Current;
                                if (schemaDocumentRef == null) {
                                    break;
                                }

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
                                    results.Write("<!--" + schemaParseErr.Message + "-->");
                                    success = false;
                                    break;
                                }

                                IEnumerator schemaDocKids = schemaDoc.EnumerateAxis(XdmAxis.Child);
                                XdmNode schemaElement = null;
                                while (schemaDocKids.MoveNext()) {
                                    schemaElement = (XdmNode) schemaDocKids.Current;
                                    if (schemaElement.NodeKind == XmlNodeType.Element) {
                                        break;
                                    }
                                }

                                string targetNamespace = schemaElement.GetAttributeValue(targetNamespaceAtt);
                                //if (targetNamespace != null && schemaManager. isSchemaAvailable(targetNamespace)) {
                                // do nothing
                                // TODO: this is the only way I can get MS additional test addB132 to work.
                                // It's not ideal: addSchemaSource() ought to be a no-op if the schema components
                                // are already loaded, but in fact recompiling the imported schema document on its
                                // own is losing the substitution group membership that was defined in the
                                // importing document.
                                //} else {
                                IList<StaticError> errorList = new List<StaticError>();
                                schemaManager.ErrorList = errorList;
                                try {
                                    schemaManager.Compile(schemaDoc);
                                }
                                catch (Exception e) {
                                    if (errorList.Count == 0) {
                                        feedback(
                                            "In " + testName + ", exception thrown but no errors in ErrorList\n", true);
                                        results.Write("<!--" + e.Message + "-->");
                                        Console.Error.WriteLine(e);
                                        success = false;
                                        break;
                                    }
                                }

                                foreach (StaticError err in errorList) {
                                    if (!err.IsWarning) {
                                        success = false;
                                        break;
                                    }
                                }
                            }

                            XdmNode expected = getChildElement(schemaTest, expectedNT);
                            bool expectedSuccess = expected == null ||
                                                   expected.GetAttributeValue(validityAtt) == "valid";
                            if (success == expectedSuccess) {
                                passed++;
                            }
                            else {
                                if (debug) {
                                    feedback("Failed schema test " + testName 
                                                                     + " (expected " + (expectedSuccess ? "validity)" : "invalidity)"), true);
                                }
                                failed++;
                                failingTests.Add(contributor + "/" + testSetName + "/" + testGroupName + "/" + testName);
                            }

                            //feedback("Passed " + passed + " failed " + failed + " total " + total, true);
                            results.Write("<testResult set='" + testSetName +
                                          "' group='" + testGroupName +
                                          "' test='" + testName +
                                          "' validity='" + (success ? "valid" : "invalid") +
                                          (queried ? "' saxon:queried='true' saxon:bugzilla='" + bugzillaRef : "") +
                                          "' saxon:outcome='" + (success == expectedSuccess ? "same" : "different") +
                                          "'/>\n");
                        }

                        foreach (XdmNode instanceTest in testGroup.Children("instanceTest")) {
                            string testName = instanceTest.GetAttributeValue("name");

                            if (exception != null) {
                                results.Write("<testResult set='" + testSetName +
                                              "' group='" + testGroupName +
                                              "' test='" + testName +
                                              "' validity='notKnown' saxon:outcome='notRun' saxon:comment='" +
                                              exception +
                                              "'/>\n");
                                continue;
                            }

                            bool queried = false;
                            XdmNode statusElement = getChildElement(instanceTest, currentNT);
                            if (statusElement != null) {
                                String status = statusElement.GetAttributeValue("status");
                                queried = ("queried" == status);
                                String instanceBug = statusElement.GetAttributeValue("bugzilla");
                                if (instanceBug != null) {
                                    bugzillaRef = instanceBug;
                                }
                            }

                            queried |= schemaQueried;

                            Console.WriteLine("TEST INSTANCE " + testName + (queried ? " (queried)" : ""));

                            XdmNode instanceDocument = getChildElement(instanceTest, instanceDocumentNT);

                            bool success = validateLinkedDocument(instanceDocument, testConfig);
                            

                            XdmNode expected = getChildElement(instanceTest, expectedNT);
                            bool expectedSuccess = expected == null ||
                                                   expected.GetAttributeValue(validityAtt) == "valid";
                            if (success == expectedSuccess) {
                                passed++;
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
                            results.Write("<testResult set='" + testSetName +
                                          "' group='" + testGroupName +
                                          "' test='" + testName +
                                          "' validity='" + (success ? "valid" : "invalid") +
                                          (queried ? "' saxon:queried='true' saxon:bugzilla='" + bugzillaRef : "") +
                                          "' saxon:outcome='" + (success == expectedSuccess ? "same" : "different") +
                                          "'/>\n");
                        }
                    }
                }
                feedback("Passed " + passed + " failed " + failed + " total " + total, true);
                foreach (string testDetails in failingTests)
                {
                    feedback("Failed " + testDetails, true);
                }
                results.Write("</testSuiteResults>");
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

        protected bool isExcluded(String testName) {
            return false;
        }

        private void feedback(string message, bool unused) {
            Console.Error.WriteLine(message);
        }
    }
}