using System;
using System.Collections.Generic;
using Saxon.Api;
using System.IO;
using System.Text.RegularExpressions;

using JFeatureKeys = Saxon.Hej.lib.FeatureKeys;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using Saxon.Ejava.util;
using Saxon.Ejavax.xml.transform.stream;
using TestRunner;

namespace TestDriver {
    public abstract class Driver {
        protected string resultsDir = null!;
        protected ResultsDocument resultsDoc = null!;
        protected int successes = 0;
        protected readonly int total = 200000;
        protected int failures = 0;
        protected int notrun = 0;
        protected int wrongErrorResults = 0;
        protected bool unfolded = false;
        protected bool saveResults = false;
        protected string treeModel = "tiny";
        protected bool debug = false;
        private Regex testPattern = null!;
        private string requestedTestSet = null!;
        protected string testSuiteDir = null!;
        protected string reportFilename = null!;
        protected readonly Processor driverProc = new Processor(true);
        protected Serializer driverSerializer;
        protected Spec requestedSpec;
        protected readonly Dictionary<string, XdmNode> exceptionsMap = new();
        protected readonly Dictionary<string, Environment> globalEnvironments = new();
        protected readonly Dictionary<string, Environment> localEnvironments = new();
        protected readonly Dictionary<string, string> optimizationAssertions = new();
        private readonly List<string> failingTestCases = new();
        private readonly Dictionary<string, int> failuresPerTestSet = new();
        public readonly Dictionary<Uri, string> queryModules = new();
        protected IFeedbackListener feedback = new DefaultFeedbackListener();
        protected bool useXslt30Transformer = true; // Temporary for controlling test processor
        protected bool strict = false;
        protected bool jitFlag = false;
#if EE        
        protected string edition = "EE";
#else
        protected string edition = "HE";
#endif        
        public bool schemaAware = false;


        static readonly HashSet<string> unsharedEnvironments = new();
        protected bool export = false;
        private string exceptionsFileName = "exceptions.xml";

        public abstract string catalogNamespace();


        private class DefaultFeedbackListener : IFeedbackListener {
            public void Feedback(int passed, int failed, int total) {
                //Console.WriteLine("Done " + (passed + failed) + " of " + total);
                //Trace.WriteLine("Done " + (passed + failed) + " of " + total);
            }


            public void Message(string message, bool popup) {
                try {
                    Console.WriteLine(message);
                    //Trace.WriteLine(message);
                }
                catch (Exception e) {
                    Console.WriteLine("issue with the message");
                }
            }
        }

        protected void recordFailure(string testSet, string testName) {
            if (!failingTestCases.Contains(testName)) {
                failures++;
                failingTestCases.Add(testName);
            }
            if (failuresPerTestSet.ContainsKey(testSet)) {
                failuresPerTestSet[testSet]++;
            }
            else {
                failuresPerTestSet[testSet] = 1;
            }
        }


        public Spec RequestedSpec => requestedSpec;

        public void setFeedbackListener(IFeedbackListener f) {
            feedback = f;
        }

        public int Failures {
            get => failures;
            set => failures = value;
        }

        public bool Unfolded {
            get => unfolded;
            set => unfolded = value;
        }

        public string TreeModel {
            get => treeModel;
            set => treeModel = value;
        }

        public static void Main(string[] args) {
            
            string language = "XQ";
            foreach (string a in args)
            {
                if (a.StartsWith("-lang:XT")) {
                    language = "XT";
                } else if (a.StartsWith("-lang:XSD")) {
                    language = "XSD";
                } else if (a.StartsWith("-lang:XMark")) {
                    language = "XMark";
                } else if (a.StartsWith("-lang:XQ")) {
                    language = "XQ";
                } else if (a.StartsWith("-lang:XP")) {
                    language = "XP";
                }

                if (a.StartsWith("-locale:")) {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(a[7..]);
                }
            }

            switch (language) {
                case "XT":
                    Xslt30TestSuiteDriver.MainXSLT(args);
                    break;
                case "XSD":
                    SchemaTestSuiteDriver.MainXSD(args);
                    break;
                case "XMark":
                    XMarkBenchmark.XMark(args);
                    break;
                case "XQ" or "XP":
                    QT3TestSuiteDriver.MainQT(args);
                    break;
            }
        }

        protected void go(string[] args) {
            try {
                driverSerializer = driverProc.NewSerializer();
                //testSuiteDir = args[0];

                Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

                testSuiteDir = args[0];
                if (testSuiteDir.EndsWith("/")) {
                    testSuiteDir = testSuiteDir.Substring(0, testSuiteDir.Length - 1);
                }

                System.Environment.SetEnvironmentVariable("expath.base.directory", testSuiteDir + "/file");

                

                string catalogFile = "catalog.xml";
                string specStr = null;

                for (int i = 1; i < args.Length; i++) {
                    if (args[i].StartsWith("-catalog")) {
                        catalogFile = args[i].Substring(9);
                    }
                    if (args[i].StartsWith("-o")) {
                        resultsDir = args[i].Substring(3);
                        if (resultsDir.EndsWith("/")) {
                            resultsDir = resultsDir.Substring(0, resultsDir.Length - 1);
                        }
                    }
                    if (args[i].StartsWith("-t:")) {
                        testPattern = new Regex(args[i].Substring(3));
                    }

                    if (args[i].StartsWith("-s:")) {
                        requestedTestSet = args[i].Substring(3);
                    }

                    if (args[i].StartsWith("-reportfilename:")) {
                        reportFilename = args[i].Substring(16);
                    }

                    if (args[i].StartsWith("-debug")) {
                        debug = true;
                    }

                    if (args[i].Equals("-unfolded")) {
                        unfolded = true;
                    }

                    if (args[i].Equals("-strict")) {
                        strict = true;
                    }

                    if (args[i].Equals("-export")) {
                        export = true;
                    }
                    
                    if (args[i].StartsWith("-exceptions")) {
                        exceptionsFileName = args[i].Substring("-exceptions:".Length);
                    }

                    if (args[i].Equals("-save")) {
                        saveResults = true;
                    }
                    
                    if (args[i].StartsWith("-jit") && args[i] != "-jit:off") {
                        jitFlag = true;
                    }

                    if (args[i].StartsWith("-lang")) {
                        specStr = args[i].Substring(6);
                        processSpec(specStr);
                    }
                    
                    if (args[i].StartsWith("-tree:")) {
                        string model = args[i].Substring(6);
                        if (model == "tiny" || model == "linked" || model == "condensed" 
                                || model == "dom" || model == "linq") {
                            treeModel = model;
                        }
                        else {
                            feedback.Message("Unknown tre model " + model + " (use tiny|linked|condensed|dom|linq)", true);
                        }
                    }

                    // Temporary for controlling test processor
                    if (args[i].StartsWith("-xt30")) {
                        if (args[i].Substring(6).Equals("on")) {
                            useXslt30Transformer = true;
                        }
                        else if (args[i].Substring(6).Equals("off")) {
                            useXslt30Transformer = false;
                        }
                    }
                }

                string catalog = testSuiteDir + "/" + catalogFile;

                if (resultsDir == null) {
                    feedback.Message("No results directory specified (use -o:dirname)", true);
                }

                if (resultsDoc == null) {
                    feedback.Message("No results document specified (use -lang option)", true);
                }

                schemaAware = edition == "EE";
                driverSerializer.OutputStream = Console.OpenStandardError();
                driverSerializer.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");
                processCatalog(catalog);
                printResults(resultsDir + "/results" + driverProc.ProductVersion + ".xml");
            }
            catch (Exception e) {
                Console.Error.WriteLine(e);
            }
        }

        protected string getResultsDir() {
            return resultsDir;
        }

        public void RegisterXQueryModule(Uri uri, string resource) {
            queryModules.Add(uri, resource);
        }

        protected abstract void processSpec(string specStr);

        internal class MyLogger : Saxon.Hej.lib.Logger {
            public MyLogger() {
                // writer =File.CreateText("logger.txt");
            }

            public override StreamResult asStreamResult() {
                return null;
            }

            public override void println(string str, int i) {
                Trace.WriteLine(str);
                //  writer.WriteLine(str);
            }
        }


        private void processCatalog(string catalogFile) {
            DocumentBuilder catBuilder = driverProc.NewDocumentBuilder();
            switch (treeModel) {
                case "linked": 
                    catBuilder.TreeModel = Saxon.Api.TreeModel.LinkedTree;
                    break;
                case "condensed": 
                    catBuilder.TreeModel = Saxon.Api.TreeModel.TinyTreeCondensed;
                    break;
                default: 
                    catBuilder.TreeModel = Saxon.Api.TreeModel.TinyTree;
                    break;
            }
            XdmNode catalog = catBuilder.Build(new Uri(Path.GetFullPath(catalogFile)));
            XPathCompiler xpc = driverProc.NewXPathCompiler();
            //xpc.XPathLanguageVersion = "3.1";
            xpc.Caching = true;
            xpc.DeclareNamespace("", catalogNamespace());

            createGlobalEnvironments(catalog, xpc);

            try {
                writeResultFilePreamble(driverProc, catalog);
            }
            catch (Exception e) {
                System.Console.WriteLine(e.Message);
            }

            readExceptionsFile();


            if (requestedTestSet != null) {
                try {
                    XdmNode funcSetNode =
                        (XdmNode) xpc.EvaluateSingle("//test-set[@name='" + requestedTestSet + "']", catalog);
                    if (funcSetNode == null) {
                        throw new Exception("Test-set " + requestedTestSet + " not found!");
                    }

                    processTestSet(catBuilder, xpc, funcSetNode);
                }
                catch (Exception e1) {
                    //e1.printStackTrace();
                    System.Console.WriteLine(e1.Message);
                }
            }
            else {
                foreach (XdmItem testSet in xpc.Evaluate("//test-set", catalog)) {
                    processTestSet(catBuilder, xpc, ((XdmNode) testSet.Simplify()));
                }
            }

            try {
                writeResultFilePostamble();
            }
            catch (Exception e) {
                System.Console.WriteLine(e.Message);
                //e.printStackTrace();
            }
        }

        private String GetNameOfExceptionsFile() {
            return exceptionsFileName;
        }

        /**
		 * Look for an exceptions.xml document with the general format:
		 * <p/>
		 * <exceptions xmlns="...test catalog namespace...">
		 * <exception test-set ="testset1" test-case="testcase" run="yes/no/not-unfolded"
		 * bug="bug-reference" reason="">
		 * <results>
		 * ... alternative expected results ...
		 * </results>
		 * <optimization>
		 * ... assertions about the "explain" tree
		 * </optimization>
		 * </exception>
		 * </exceptions>
		 */
        protected void readExceptionsFile() {
            DocumentBuilder exceptBuilder = driverProc.NewDocumentBuilder();
            QName testSet = new QName("", "test-set");
            QName testCase = new QName("", "test-case");
            QName run = new QName("", "run");
            QName editionAtt = new QName("", "edition");
            string saxonEdition = driverProc.Edition;
            Uri exceptionsFile = null;
            try {
                String suppliedName = GetNameOfExceptionsFile();
                Match m = Regex.Match(suppliedName, "^[a-zA-Z]:.*");

                if (suppliedName.StartsWith("/") || m.Success) {
                    exceptionsFile = new Uri(Path.GetFullPath(suppliedName));
                }
                else {
                    exceptionsFile = new Uri(Path.GetFullPath(resultsDir + "/" + suppliedName));
                }

                Console.WriteLine("Loading exceptions file " + exceptionsFile.AbsolutePath);
                XdmNode exceptionsDoc = exceptBuilder.Build(exceptionsFile);
                
                foreach(XdmNode entry in exceptionsDoc.Select(Steps.Descendant("exception"))) {
                    string testName = entry.GetAttributeValue(testCase);
                    if (testName == null) {
                        testName = "$" + entry.GetAttributeValue(testSet);
                    }

                    string runVal = entry.GetAttributeValue(run);
                    string editionVal = entry.GetAttributeValue(editionAtt);
                    runVal ??= "false";
                    editionVal ??= saxonEdition;

                    string[] substrings = Regex.Split(editionVal.Trim(), "\\s+");
                    bool appliesThisEdition = substrings.Any(ed => ed.Equals(saxonEdition));

                    if (appliesThisEdition) {
                        if (runVal == "false") {
                            foreach (string tc in Regex.Split(testName.Trim(), "\\s+")) {
                                exceptionsMap.TryAdd(tc, entry);
                            }
                        }
                        else {
                            foreach (XdmNode optim in entry.Children("optimization")) {
                                optimizationAssertions.TryAdd(testName, optim.GetAttributeValue("assert"));
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                feedback.Message("*** Failed to process exceptions file: " + exceptionsFile + " :" + e.Message, true);
            }
        }

        protected abstract void createGlobalEnvironments(XdmNode catalog, XPathCompiler xpc);

        private void createLocalEnvironments(XdmNode testSetDocNode) {
            localEnvironments.Clear();
            Environment defaultEnvironment =
                Environment.createLocalEnvironment(testSetDocNode.BaseUri, unfolded, requestedSpec);
            localEnvironments.Add("default", defaultEnvironment);
        }

        protected Environment getEnvironment(XdmNode testCase, XPathCompiler xpc) {
            string testCaseName = testCase.GetAttributeValue(new QName("name"));
            XdmNode environmentNode = (XdmNode) xpc.EvaluateSingle("environment", testCase);
            Environment env;
            if (environmentNode == null) {
                env = localEnvironments["default"];
            }
            else {
                string envName = environmentNode.GetAttributeValue(new QName("ref"));
                if (envName == null || envName.Equals("")) {
                    env = Environment.processEnvironment(this, xpc, environmentNode, null,
                        localEnvironments["default"]);
                    bool baseUriCheck =
                        ((XdmAtomicValue) xpc.EvaluateSingle("static-base-uri/@uri='#UNDEFINED'", environmentNode))
                        .AsBoolean();
                    if (baseUriCheck) {
                        //writeTestcaseElement(testCaseName, "notRun", "static-base-uri not supported", null);
                        return null;
                    }
                }
                else {
                    try {
                        env = localEnvironments[envName];
                    }
                    catch (Exception) {
                        env = null;
                    }

                    if (env == null) {
                        try {
                            env = globalEnvironments[envName];
                        }
                        catch (Exception) {
                            env = null;
                        }
                    }

                    if (env == null) {
                        foreach (XdmNode e in testCase.Parent.Select(Steps.Descendant("environment")
                            .Where(Predicates.AttributeEq("name", envName)))) {
                            Environment.processEnvironment(this, xpc, e, localEnvironments,
                                localEnvironments["default"]);
                        }

                        try {
                            env = localEnvironments[envName];
                        }
                        catch (Exception) {
                            env = null;
                        }
                    }

                    if (env == null) {
                        System.Console.WriteLine("*** Unknown environment " + envName);
                        failures++;
                        failingTestCases.Add(testCaseName);
                        return null;
                    }
                    
                    if (!env.usable) {
                        System.Console.WriteLine("*** Environment " + envName +  " is unusable");
                        return null;
                    }
                }
            }

            return env;
        }

        private void writeResultFilePreamble(Processor processor, XdmNode catalog) {
            resultsDoc.writeResultFilePreamble(processor, catalog);
        }

        private void writeResultFilePostamble() {
            resultsDoc.writeResultFilePostamble();
        }

        private void startTestSetElement(XdmNode testSetNode) {
            resultsDoc.startTestSetElement(testSetNode);
        }

        private void writeTestSetEndElement() {
            resultsDoc.endElement();
        }


        private void processTestSet(DocumentBuilder catbuilder, XPathCompiler xpc, XdmNode testSetNode) {
            startTestSetElement(testSetNode);
            Uri testSetFile =
                new Uri(Path.GetFullPath(testSuiteDir + "/" + testSetNode.GetAttributeValue(new QName("file"))));
            XdmNode testSetDocNode = catbuilder.Build(testSetFile);
            createLocalEnvironments(testSetDocNode);
            bool run = true;

            IEnumerable<XdmNode> testdocEnum =
                testSetDocNode.Select(Steps.Child("test-set").Then(Steps.Child("dependency")));

            IEnumerator<XdmNode> testDocEnumerator = testdocEnum.GetEnumerator();
            if (testDocEnumerator.MoveNext()) {
                // pick up any test-set level dependencies in the QT3 catalog
                foreach (XdmNode dependency in testSetDocNode.Select(Steps.Child("test-set")
                    .Then(Steps.Child("dependency")))) {
                    if (!EnsureDependencySatisfied(dependency, localEnvironments["default"])) {
                        foreach (XdmNode testCase in testSetDocNode.Select(Steps.Descendant("test-case"))) {
                            string testCaseName = testCase.GetAttributeValue("name");
                            resultsDoc.writeTestcaseElement(testCaseName, "n/a", "test-set dependencies not satisfied");
                            notrun++;
                        }

                        run = false;
                        break;
                    }
                }

                // pick up any test-set level dependencies in the QT3 catalog
                foreach (XdmNode dependency in testSetDocNode.Select(Steps.Path("test-set", "dependencies", "*"))) {
                    if (!EnsureDependencySatisfied(dependency, localEnvironments["default"])) {
                        foreach (XdmNode testCase in testSetDocNode.Select(Steps.Descendant("test-case"))) {
                            string type = dependency.NodeName.LocalName;
                            string value = testCase.GetAttributeValue("value");
                            if (value == null) {
                                value = type;
                            }
                            else {
                                value = type + ":" + value;
                            }

                            if ("false".Equals(dependency.GetAttributeValue("satisfied"))) {
                                value = "!" + value;
                            }

                            String testCaseName = testCase.GetAttributeValue("name");
                            resultsDoc.writeTestcaseElement(testCaseName, "n/a",
                                "test-set dependencies not satisfied: " + value);
                            notrun++;
                        }

                        run = false;
                        break;
                    }
                }
            }

            if (run) {
                if (testPattern == null) {
                    foreach (XdmNode env in testSetDocNode.Select(Steps.Descendant("environment")
                        .Where(Predicates.HasAttribute("name")))) {
                        string envName = env.GetAttributeValue("name");
                        if (!unsharedEnvironments.Contains(envName)) {
                            try {
                                Environment.processEnvironment(this, xpc, env, localEnvironments,
                                    localEnvironments["default"]);
                            }
                            catch (Exception ex) {
                                Console.WriteLine("Failure loading environment, in processTestSet " + ex.Message);
                            }
                        }
                    }
                }

                string testSet = testSetDocNode.Children("test-set").First().GetAttributeValue("name");
                if (testSet == null) {
                    Console.WriteLine("Missing test-set/@name");
                    return;
                }

                if (SkipBecauseSlow(testSet)) {
                    return;
                }
                
                if (exceptionsMap.ContainsKey("$" + testSet)) {
                    return;
                }

                foreach (XdmNode testCase in testSetDocNode.Select(Steps.Descendant("test-case"))) {
                    string name = testCase.GetAttributeValue("name");
                    if (testPattern == null || testPattern.Match(name).Success) {
                        println("-s:" + testSet + " -t:" + name);

                        try {
                            runTestCase(testCase, xpc);
                        }
                        catch (Exception ex) {
                            Console.WriteLine("*** Error in evaluating testcase:" + ex.Message);
                            Console.WriteLine(ex.StackTrace);
                        }
                    }
                }
            }

            writeTestSetEndElement();
        }

        private bool SkipBecauseSlow(string testSet) {
            return testSet == "unicode-90";
        }

        protected abstract void runTestCase(XdmNode testCase, XPathCompiler catalogXpc);

        public void println(string data) {
            feedback.Message(data, false);
        }

        private void printResults(string resultsFileStr) {
            feedback.Message(
                successes + " successes, " + failures + " failures, " + wrongErrorResults + " incorrect ErrorCode, " +
                notrun + " not run", false);
            if (requestedTestSet != null) {
                feedback.Message("Failing tests:", false);
                foreach (string t in failingTestCases) {
                    feedback.Message("  " + t, false);
                }
            }
            else {
                feedback.Message("Failing tests:", false);
                foreach (string testSet in failuresPerTestSet.Keys) {
                    feedback.Message("  " + testSet + " - " + failuresPerTestSet[testSet], false);
                }
            }
        }
        
        public abstract bool EnsureDependencySatisfied(XdmNode dependency, Environment env);

    }
    
    

}