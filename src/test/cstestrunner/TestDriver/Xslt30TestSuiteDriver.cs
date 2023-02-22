using System;
using System.Collections;
using SysCollGen = System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using Saxon.Api;
using JFeatureKeys = Saxon.Hej.lib.FeatureKeys;
using JConfiguration = Saxon.Hej.Configuration;
using JVersion = Saxon.Hej.Version;
using JResult = Saxon.Ejavax.xml.transform.Result;
using System.Diagnostics;
using System.Linq;
using Saxon.Ejavax.xml.transform.stream;
using Saxon.Hej.lib;
using static Saxon.Api.Steps;
using static Saxon.Api.Predicates;
using NamespaceConstant = Saxon.Api.NamespaceConstant;

namespace TestDriver {
    //C:/work/repositories/hg/xslt30-test C:/work/repositories/hg/xslt30-test/results/saxon/ -lang:XT30 -xt30:on

    // file:////C:/work/repositories/hg/xslt30-test/ C:/work/repositories/hg/xslt30-test/results/saxon/ -lang:XT30 -xt30:on -s:mode -t:mode-1801
    /**
     * This class runs the W3C XSLT Test Suite, driven from the test catalog.
     */
    public class Xslt30TestSuiteDriver : Driver {
        private readonly SysCollGen.HashSet<string> alwaysOn = new SysCollGen.HashSet<string>();

        private readonly SysCollGen.HashSet<string> alwaysOff = new SysCollGen.HashSet<string>();

        private readonly SysCollGen.HashSet<string> needsEE = new SysCollGen.HashSet<string>();

        private readonly SysCollGen.HashSet<string> needsPE = new SysCollGen.HashSet<string>();

        public Xslt30TestSuiteDriver() {
            SetDependencyData();
        }


        public static void MainXSLT(string[] args) {
            if (args.Length == 0 || args[0].Equals("-?")) {
                System.Console.WriteLine("testsuiteDir catalog [-o:resultsdir] [-s:testSetName]" +
                                         " [-t:testNamePattern] [-bytecode:on|off|debug] [-tree] [-lang] [-save][-export][-strict]");
            }

            System.Console.WriteLine("Testing Saxon " + (new Processor()).ProductVersion);
            new Xslt30TestSuiteDriver().go(args);
        }

        public static Predicate<XdmItem> IsTrue(string attName) {
            return element => {
                if (element is XdmNode node) {
                    string attVal = node.GetAttributeValue(attName);
                    if (attVal != null) {
                        attVal = attVal.Trim();
                        return ("yes" == attVal || "1" == attVal || "true" == attVal);
                    }
                }
                return false;
            };
        }

        private void SetDependencyData() {
            alwaysOn.Add("feature/disabling_output_escaping");
            alwaysOn.Add("feature/serialization");
            alwaysOn.Add("feature/namespace_axis");
            alwaysOff.Add("feature/dtd"); // Microsoft parser does not report ID attributes
            alwaysOn.Add("feature/built_in_derived_types");
            alwaysOn.Add("feature/remote_http");
            alwaysOn.Add("feature/xsl-stylesheet-processing-instruction");
            alwaysOn.Add("feature/fn-transform-XSLT");
            alwaysOn.Add("available_documents");
            alwaysOn.Add("ordinal_scheme_name");
            alwaysOn.Add("default_calendar_in_date_formatting_functions");
            alwaysOn.Add("supported_calendars_in_date_formatting_functions");
            alwaysOn.Add("maximum_number_of_decimal_digits");
            alwaysOn.Add("default_output_encoding");
            alwaysOn.Add("unparsed_text_encoding");
            alwaysOn.Add("recognize_id_as_uri_fragment");
            alwaysOn.Add("feature/XPath_3.1");
            alwaysOn.Add("feature/backwards_compatibility");
            alwaysOn.Add("feature/HTML4");
            alwaysOn.Add("feature/HTML5");

            needsPE.Add("feature/Saxon-PE");
            needsPE.Add("feature/dynamic_evaluation");

            //needsEE.Add("languages_for_numbering");
            needsEE.Add("feature/streaming");
            needsEE.Add("feature/schema_aware");
            needsEE.Add("feature/Saxon-EE");
            //needsEE.Add("feature/XSD_1.1");


            needsEE.Add("feature/xquery_invocation");
            needsEE.Add("feature/higher_order_functions");

            alwaysOn.Add("detect_accumulator_cycles");
        }


        public override string catalogNamespace() {
            return "http://www.w3.org/2012/10/xslt-test-catalog";
        }

        public void writeResultFilePreamble(Processor processor, XdmNode catalog) {
            resultsDoc = new Xslt30ResultsDocument(this.resultsDir, Spec.XT30, reportFilename);
            //super.writeResultFilePreamble(processor, catalog);
        }


        protected override void processSpec(string specStr) {
            if (specStr.Equals("XT10")) {
                requestedSpec = Spec.XT10;
            }
            else if (specStr.Equals("XT20")) {
                requestedSpec = Spec.XT20;
            }
            else if (specStr.Equals("XT30")) {
                requestedSpec = Spec.XT30;
            }
            else if (specStr.Equals("XT40")) {
                requestedSpec = Spec.XT40;
            }
            else {
                throw new Exception("Unknown spec " + specStr);
            }

            resultsDoc = new Xslt30ResultsDocument(this.resultsDir, Spec.XT30, reportFilename);
            // No action: always use XSLT
        }


        protected override void createGlobalEnvironments(XdmNode catalog, XPathCompiler xpc) {
            Environment environment = null;
            Environment defaultEnv = null;
            try {
                defaultEnv = localEnvironments["default"];
            }
            catch (Exception) {
                // no action
            }

            foreach (XdmNode env in catalog.Select(Descendant("environment"))) {
                environment = Environment.processEnvironment(this, xpc, env, globalEnvironments, defaultEnv);
            }

            //buildDependencyMap(driverProc, environment);
        }

        /**
         * Return a set of named parameters as a map
         *
         * @param xpath     The XPath compiler to use
         * @param node      The node to search for &lt;param> children
         * @param getStatic Whether to collect static or non-static sets
         * @param tunnel    Whether to collect tunnelled or non-tunnelled sets
         * @return Map of the evaluated parameters, keyed by QName
         * @throws SaxonApiException
         */
        internal SysCollGen.Dictionary<QName, XdmValue> GetNamedParameters(
            XPathCompiler xpath, XdmNode node, bool getStatic, bool tunnel) {
            SysCollGen.Dictionary<QName, XdmValue> params1 = new SysCollGen.Dictionary<QName, XdmValue>();
            int i = 1;
            string staticTest = getStatic ? "@static=('yes','true','1')" : "not(@static=('yes','true','1'))";
            foreach (XdmItem parami in xpath.Evaluate("param[" + staticTest + "]", node)) {
                QName name = GetQNameAttribute(xpath, parami, "@name");
                string select = ((XdmNode) parami).GetAttributeValue(new QName("select"));
                string tunnelled = ((XdmNode) parami).GetAttributeValue(new QName("tunnel"));
                bool required = tunnel == (tunnelled != null && tunnelled.Equals("yes"));
                XdmValue value;
                if (name == null) {
                    System.Console.WriteLine("*** No name for parameter " + i + " in initial-template");
                    throw new Exception("*** No name for parameter " + i + " in initial-template");
                }

                try {
                    value = xpath.Evaluate(select, null);
                    i++;
                }
                catch (Exception e) {
                    System.Console.WriteLine("*** Error evaluating parameter " + name + " in initial-template : " +
                                             e.Message);
                    throw e;
                }

                if (required) {
                    params1.Add(name, value);
                }
            }

            return params1;
        }

        internal XdmValue[] getParameters(XPathCompiler xpath, XdmNode node) {
            SysCollGen.List<XdmValue> params1 = new SysCollGen.List<XdmValue>();

            int i = 1;
            foreach (XdmItem param in xpath.Evaluate("param[not(@static='yes')]", node)) {
                string select = ((XdmNode) param).GetAttributeValue(new QName("select"));
                XdmValue value;
                try {
                    value = xpath.Evaluate(select, null);
                    i++;
                }
                catch (Exception e) {
                    System.Console.WriteLine("*** Error evaluating parameter " + i + " in initial-function : " +
                                             e.Message);
                    throw;
                }

                params1.Add(value);
            }

            return params1.ToArray();
        }


        internal static QName GetQNameAttribute(XPathCompiler xpath, XdmItem contextItem, string attributePath) {
            string exp = "for $att in " + attributePath +
                         " return if (contains($att, ':')) then resolve-QName($att, $att/..) else " +
                         " if (contains($att,'{')) then QName(substring-before(substring-after($att,'{'),'}'),substring-after($att,'}')) else" +
                         " xs:string($att)";
            XdmAtomicValue qname = null;
            try {
                qname = (XdmAtomicValue) xpath.EvaluateSingle(exp, contextItem);
            }
            catch (Exception ex) {
                Trace.WriteLine("Error in GetQNameAttribute : " + ex.Message);
                return null;
            }

            if (qname == null) {
                return null;
            }

            String qnameStr = qname.StringValue;
            if (qnameStr.Equals("#unnamed")) {
                return new QName(NamespaceConstant.XSLT, "unnamed");
            }
            else if (qnameStr.Equals("#default")) {
                return new QName(NamespaceConstant.XSLT, "default");
            }
            else if (qnameStr.StartsWith("{")) {
                return QName.FromClarkName(qnameStr);
            }
            else if (qnameStr.StartsWith("Q{")) {
                return QName.FromEQName(qnameStr);
            }

            if (qnameStr.Contains(":")) {
                QName qqname = new QName(qnameStr, (XdmNode) contextItem);
                return qqname;
            }

            return qname == null ? null : new QName(qname.ToString());
        }

        private bool isSlow(string testName) {
            return testName.StartsWith("regex-classes") ||
                   testName == "normalize-unicode-008" ||
                   testName == "catalog005"; // TODO: this one hangs, need to investigate
            ;
        }

        internal void CopySchemaNamespace(Environment env, XPathCompiler testXpc) {
            JConfiguration config = env.xpathCompiler.Processor.Implementation;
            SysCollGen.ISet<string> ns = config.getImportedNamespaces();
            foreach (string s in ns.ToArray()) {
                testXpc.ImportSchemaNamespace(s);
            }
        }


        protected override void runTestCase(XdmNode testCase, XPathCompiler xpath) {
            TestOutcome outcome = new TestOutcome(this);
            string testName = testCase.GetAttributeValue(new QName("name"));
            string testSetName = testCase.Parent.GetAttributeValue(new QName("name"));

            if (exceptionsMap.ContainsKey(testName)) {
                notrun++;
                resultsDoc.writeTestcaseElement(testName, "notRun",
                    exceptionsMap[testName].GetAttributeValue(new QName("reason")));
                return;
            }

            if (exceptionsMap.ContainsKey(testName) || isSlow(testName)) {
                notrun++;
                resultsDoc.writeTestcaseElement(testName, "notRun", "requires excessive resources");
                return;
            }

            Environment env = getEnvironment(testCase, xpath);
            if (env == null) {
                resultsDoc.writeTestcaseElement(testName, "notRun", "test catalog error");
                return;
            }

            if (testName.Contains("environment-variable")) {
                env.processor.Implementation.setConfigurationProperty(JFeatureKeys.ENVIRONMENT_VARIABLE_RESOLVER,
                    new EnvironmentVariableResolver());
            }

            XdmValue testSetDependencies = new XdmValue(testCase.Parent.Children("dependencies"));
            XdmValue testCaseDependencies = new XdmValue(testCase.Children("dependencies"));
            XdmValue allDependencies = testSetDependencies.Append(testCaseDependencies);

            XdmNode lastSpecValue = allDependencies.Select(Path("spec", "@value")).FirstOrDefault();
            String specAtt = lastSpecValue != null ? lastSpecValue.StringValue : "XSLT10+";
            foreach (XdmNode dep in allDependencies.Select(Child(IsElement()))) {
                if (!EnsureDependencySatisfied((XdmNode) dep, env)) {
                    notrun++;
                    string type = dep.NodeName.LocalName;
                    string value = dep.GetAttributeValue("value");
                    if (value == null) {
                        value = type;
                    }
                    else {
                        value = type + ":" + value;
                    }

                    if ("false".Equals(dep.GetAttributeValue("satisfied"))) {
                        value = "!" + value;
                    }

                    String message = "dependency not satisfied" + value;
                    if (value.StartsWith("feature:")) {
                        message = "requires optional " + value;
                    }

                    resultsDoc.writeTestcaseElement(testName, "notRun", message);
                    return;
                }
            }

            if (env.failedToBuild) {
                resultsDoc.writeTestcaseElement(testName, "fail", "unable to build environment");
                //noteFailure(testSetName, testName);
                return;
            }

            if (!env.usable) {
                resultsDoc.writeTestcaseElement(testName, "n/a", "environment dependencies not satisfied");
                notrun++;
                return;
            }

            if (testName.Contains("load-xquery-module")) {
                ModuleResolver mr = new ModuleResolver(null, this);
                mr.setTestCase(testCase);
                env.processor.Implementation.setModuleURIResolver(mr);
            }

            XdmNode testInput = testCase.Select(Child("test")).FirstOrDefault();
            XdmNode stylesheet = testInput
                .Select(Child("stylesheet").Where(Not(AttributeEq("role", "secondary"))))
                .FirstOrDefault();
            XdmNode postureAndSweep = testInput.Select(Child("posture-and-sweep")).FirstOrDefault();
            XdmNode principalPackage = testInput
                .Select(Child("package").Where(AttributeEq("role", "principal"))).FirstOrDefault();
            SysCollGen.IEnumerable<XdmNode> usedPackages =
                testInput.Select(Child("package").Where(AttributeEq("role", "secondary")));

            bool strictStreamability = testCase.Select(
                Child("result").Then(Steps.Descendant("error"))
                    .Where(AttributeEq("code", "XTSE3430"))).Any();
            if (strictStreamability) {
                env.processor.SetProperty(Saxon.Api.Feature<bool>.STRICT_STREAMABILITY, true);
                env.resetActions.Add(env =>
                    env.processor.SetProperty(Saxon.Api.Feature<bool>.STRICT_STREAMABILITY, false));
            }

            // If a static error is expected, suppress JIT compilation so we catch it eagerly
            bool IsStaticError(XdmItem item) => item.StringValue.StartsWith("XTSE");
            bool expectStaticError = testCase.Select(
                Steps.Path("result", "//", "error", "@code").Where((Predicate<XdmItem>) IsStaticError)).Any();
            if (expectStaticError) {
                bool saved = env.xsltCompiler.JustInTimeCompilation;
                env.xsltCompiler.JustInTimeCompilation = false;
                env.resetActions.Add(env => env.xsltCompiler.JustInTimeCompilation = saved);
            }


            XsltExecutable sheet = env.xsltExecutable;

            XmlUrlResolver res = new XmlUrlResolver();
            //string xsltLanguageVersion = specAtt.Contains("XSLT30") || specAtt.Contains("XSLT20+") ? "3.0" : "2.0";
            if (stylesheet != null) {
                //env.processor.SetProperty(FeatureKeys.ALLOW_MULTITHREADING, "false");
                XsltCompiler compiler = env.xsltCompiler;
                compiler.ErrorReporter = err => {
                    if (!expectStaticError) {
                        string code = err.ErrorCode == null ? "(no error code)" : err.ErrorCode.LocalName;
                        Location loc = err.Location;
                        try {
                            Console.Error.WriteLine("*** STATIC ERROR: " + code + " at " + 
                                                    loc.SystemId + "#" + loc.LineNumber + 
                                                    "\n   " + err.Message);
                        }
                        catch (Exception e) {
                            //Console.Error.WriteLine("*** STATIC ERROR: " + err.Message);
                        }
                    }
                    outcome.ReportError(err.ErrorCode);
                };
                Stream styleSource;
                try {
                    Uri hrefFile = res.ResolveUri(stylesheet.BaseUri, stylesheet.GetAttributeValue(new QName("file")));
                    styleSource = new FileStream(hrefFile.AbsolutePath, FileMode.Open, FileAccess.Read);
                    compiler.BaseUri = hrefFile;
                }
                catch (Exception) {
                    Console.WriteLine("*** Failed to load stylesheet");
                    recordFailure(testSetName, testName);
                    throw;
                }
                // compiler.XsltLanguageVersion =
                //     (specAtt.Contains("XSLT30") || specAtt.Contains("XSLT20+") ? "3.0" : "2.0");
                compiler.ClearParameters();

                SysCollGen.IEnumerable<XdmNode> paramsi = testInput.Select(Child("param").Where(IsTrue("static")));
                foreach (XdmNode param in paramsi) {
                    String name = param.GetAttributeValue("name");
                    String select = param.GetAttributeValue("select");
                    XdmValue value;
                    try {
                        value = xpath.Evaluate(select, null);
                    }
                    catch (Exception e) {
                        Console.WriteLine("*** Error evaluating parameter " + name + ": " + e.Message);
                        //throw e;
                        continue;
                    }

                    compiler.SetParameter(new QName(name), value);
                }

                foreach (XdmNode pack in usedPackages) {
                    String fileName2 = pack.GetAttributeValue("file");
                    Uri fileUri = res.ResolveUri(pack.BaseUri, fileName2);
                    Stream styleSource2 = new FileStream(fileUri.AbsolutePath, FileMode.Open, FileAccess.Read);
                    XsltPackage xpack = compiler.CompilePackage(styleSource2, fileUri);
                    xpack = exportImportPackage(testName, testSetName, outcome, compiler, xpack);
                    compiler.ImportPackage(xpack);
                    // Following needed for dynamic loading of packages using fn:transform()
                    // env.processor.getUnderlyingConfiguration().getDefaultXsltCompilerInfo().getPackageLibrary().addPackage(xpack.getUnderlyingPreparedPackage()); //TODO - check this on .net
                }


                try {
                    sheet = exportImport(testName, testSetName, outcome, compiler, sheet, styleSource);
                }
                catch (Exception err) {
                    Console.WriteLine(err.Message);
                    outcome.SetException(err);
                }


                optimizationAssertions.TryGetValue(testName, out var optimizationAssertion);
                if (optimizationAssertion != null && sheet != null) {
                    try {
                        bool ok = AssertOptimization(sheet, optimizationAssertion);
                        if (ok) {
                            Console.WriteLine("Optimization OK: " + optimizationAssertion);
                        }
                        else {
                            outcome.SetComment("Optimization assertion failed");
                            if (strict) {
                                //noteFailure(testSetName, testName);
                                resultsDoc.writeTestcaseElement(testName, "fail", outcome.GetComment());
                            }
                        }

                        Console.WriteLine("Optimization OK: " + optimizationAssertion);
                    }
                    catch (Exception) {
                        Console.WriteLine("Optimization assertion failed: " + optimizationAssertion);
                    }
                }


                //  compiler.setErrorListener(collector);
            }
            else if (principalPackage != null) {
                XsltCompiler compiler = env.xsltCompiler;
                compiler.ErrorReporter = err => outcome.ReportError(err.ErrorCode);
                compiler.ClearParameters();
                SysCollGen.IEnumerable<XdmNode> paramEnumerable =
                    testInput.Select(Child("param").Where(IsTrue("static")));

                foreach (XdmNode itemxx in paramEnumerable) {
                    String name = itemxx.GetAttributeValue("name");
                    String select = itemxx.GetAttributeValue("select");
                    XdmValue value;
                    try {
                        value = xpath.Evaluate(select, null);
                    }
                    catch (Exception e) {
                        Console.WriteLine("*** Error evaluating parameter " + name + ": " + e.Message);
                        throw;
                    }

                    compiler.SetParameter(new QName(name), value);
                }

                foreach (XdmNode pack in usedPackages) {
                    String pfileName = pack.GetAttributeValue("file");
                    Uri pfileUri = res.ResolveUri(testCase.BaseUri, pfileName);
                    Stream pstyleSource = new FileStream(pfileUri.AbsolutePath, FileMode.Open, FileAccess.Read);

                    XsltPackage xpack = compiler.CompilePackage(pstyleSource, pfileUri);
                    compiler.ImportPackage(xpack);
                }

                String fileName = principalPackage.GetAttributeValue("file");
                Uri fileUri = res.ResolveUri(testCase.BaseUri, fileName);
                Stream styleSource = new FileStream(fileUri.AbsolutePath, FileMode.Open, FileAccess.Read);
                compiler.ErrorReporter = err => outcome.ReportError(err.ErrorCode);
                compiler.BaseUri = fileUri;
                try {
                    XsltPackage xpack = compiler.CompilePackage(styleSource, fileUri);
                    sheet = xpack.Link();
                }
                catch (Exception err) {
                    println(err.Message);
                    outcome.SetException(err);
                }


                sheet = exportImport(testName, testSetName, outcome, compiler, sheet, styleSource);
            }

            if (sheet != null) {
                XdmItem contextItem = env.contextItem;
                XdmValue initialMatchSelection = null;
                XdmNode initialMode = (XdmNode) xpath.EvaluateSingle("initial-mode", testInput);
                XdmNode initialFunction = (XdmNode) xpath.EvaluateSingle("initial-function", testInput);
                XdmNode initialTemplate = (XdmNode) xpath.EvaluateSingle("initial-template", testInput);

                QName initialModeName = initialMode == null ? null : GetQNameAttribute(xpath, initialMode, "@name");
                QName initialTemplateName =
                    initialTemplate == null ? null : GetQNameAttribute(xpath, initialTemplate, "@name");
                string baseOutputURI = new System.Uri(resultsDir + "/results/output.xml").AbsoluteUri;
                XdmNode outputNode = testInput.Select(Steps.Path("output", "@file")).FirstOrDefault();
                string outputUri = (outputNode != null ? outputNode.StringValue : null);

                if (outputUri != null) {
                    if (outputUri.Equals("")) {
                        String testSetUri = testCase.BaseUri.AbsoluteUri;
                        int c = testSetUri.IndexOf("/tests/");
                        outputUri = testSetUri.Substring(c + 7);
                    }

                    baseOutputURI = (new XmlUrlResolver()).ResolveUri(new Uri(baseOutputURI), outputUri).AbsolutePath;
                }

                outcome.BaseOutputUri = new Uri(baseOutputURI);

                if (initialMode != null) {
                    XdmItem select = xpath.EvaluateSingle("@select", initialMode);
                    if (select != null) {
                        initialMatchSelection = env.xpathCompiler.Evaluate(@select.StringValue, null);
                    }
                }

                if (useXslt30Transformer) {
                    try {
                        bool assertsSerial =
                            xpath.Evaluate(
                                "result//(assert-serialization|assert-serialization-error|serialization-matches)",
                                testCase).Count > 0;
                        bool resultAsTree = env.outputTree;
                        bool serializationDeclared = env.outputSerialize;
                        XdmNode needsTree = (XdmNode) xpath.EvaluateSingle("output/@tree", testInput);
                        if (needsTree != null) {
                            resultAsTree = needsTree.StringValue.Equals("yes");
                        }

                        XdmNode needsSerialization = (XdmNode) xpath.EvaluateSingle("output/@serialize", testInput);
                        if (needsSerialization != null) {
                            serializationDeclared = needsSerialization.StringValue.Equals("yes");
                        }

                        bool resultSerialized = serializationDeclared || assertsSerial;

                        if (assertsSerial) {
                            String comment = outcome.GetComment();
                            comment = (comment == null ? "" : comment) + "*Serialization " +
                                      (serializationDeclared ? "declared* " : "required* ");
                            outcome.SetComment(comment);
                        }


                        Xslt30Transformer transformer = sheet.Load30();
                        transformer.XmlResolver = env;
                        if (env.unparsedTextResolver != null) {
                            transformer.TextResolver = env.unparsedTextResolver;
                        }

                        SysCollGen.Dictionary<QName, XdmValue> caseGlobalParams =
                            GetNamedParameters(xpath, testInput, false, false);
                        SysCollGen.Dictionary<QName, XdmValue> caseStaticParams =
                            GetNamedParameters(xpath, testInput, true, false);
                        SysCollGen.Dictionary<QName, XdmValue> globalParams =
                            new SysCollGen.Dictionary<QName, XdmValue>(env.params1);

                        foreach (SysCollGen.KeyValuePair<QName, XdmValue> entry in caseGlobalParams) {
                            globalParams.Add(entry.Key, entry.Value);
                        }

                        transformer.SetStylesheetParameters(globalParams);

                        if (contextItem != null) {
                            transformer.GlobalContextItem = contextItem;
                        }

                        transformer.BaseOutputURI = baseOutputURI;

                        transformer.MessageListener = msg => {
                            outcome.AddXslMessage(msg.Content);
                            Console.Error.WriteLine(msg.Content);
                        };

                        XdmValue result = null;

                        TextWriter sw = new StringWriter();

                        Serializer serializer = env.processor.NewSerializer();
                        serializer.OutputWriter = sw;

                        IDestination dest = null;
                        if (resultAsTree && !resultSerialized) {
                            // If we want non-serialized, we need to accumulate any result documents as trees too
                            ResultDocumentHandler rdh = new (env.processor, outcome, baseOutputURI, false);
                            transformer.ResultDocumentHandler = rdh.HandleResultDocument;
                            dest = new XdmDestination();
                        } else {
                            ResultDocumentHandler rdh = new (env.processor, outcome, baseOutputURI, true);
                            transformer.ResultDocumentHandler = rdh.HandleResultDocument;
                            if (resultSerialized) {
                                dest = serializer;
                            }
                        }


                        Stream src = null;
                        Uri srcBaseUri = new Uri("http://uri");
                        XdmNode srcNode = null;
                        DocumentBuilder builder2 = env.processor.NewDocumentBuilder();

                        if (env.streamedPath != null) {
                            src = new FileStream(env.streamedPath, FileMode.Open, FileAccess.Read);
                            srcBaseUri = new Uri(env.streamedPath);
                        }
                        else if (env.streamedContent != null) {
                            byte[] byteArray = Encoding.UTF8.GetBytes(env.streamedContent);
                            src = new MemoryStream(byteArray); //, "inlineDoc");
                            builder2.BaseUri = new Uri("http://uri");
                        }
                        else if (initialTemplate == null && contextItem != null) {
                            srcNode = (XdmNode) (contextItem);
                        }

                        SchemaValidator validator = null;
                        if (src != null && !"skip".Equals(env.streamedInputValidation)) {
                            validator = env.processor.SchemaManager.NewSchemaValidator();
                        }

                        if (initialMode != null) {
                            QName name = GetQNameAttribute(xpath, initialMode, "@name");
                            try {
                                if (name != null) {
                                    transformer.InitialMode = name;
                                }
                            }
                            catch (Exception e) {
                                if (e.InnerException is Saxon.Hej.trans.XPathException) {
                                    Console.WriteLine(e.Message);
                                    outcome.SetException(e);
                                    //throw new SaxonApiException(e.getCause());
                                }
                                else {
                                    throw;
                                }
                            }
                        }

                        if (src == null && srcNode == null && initialFunction == null && initialTemplateName == null &&
                            initialModeName == null) {
                            initialTemplateName = new QName("xsl", NamespaceConstant.XSLT, "initial-template");
                        }

                        if (initialMode != null || initialTemplate != null) {
                            XdmNode init = (XdmNode) (initialMode == null ? initialTemplate : initialMode);
                            SysCollGen.Dictionary<QName, XdmValue> params1 =
                                GetNamedParameters(xpath, init, false, false);
                            SysCollGen.Dictionary<QName, XdmValue> tunnelledParams =
                                GetNamedParameters(xpath, init, false, true);
                            if (env.xsltCompiler.XsltLanguageVersion.Equals("2.0")) {
                                if (!(params1.Count == 0 && tunnelledParams.Count == 0)) {
                                    Console.WriteLine("*** Initial template parameters ignored for XSLT 2.0");
                                }
                            }
                            else {
                                transformer.SetInitialTemplateParameters(params1, false);
                                transformer.SetInitialTemplateParameters(tunnelledParams, true);
                            }
                        }

                        if (initialTemplateName != null) {
                            transformer.GlobalContextItem = contextItem;
                            if (dest == null) {
                                result = transformer.CallTemplate(initialTemplateName);
                            }
                            else {
                                transformer.CallTemplate(initialTemplateName, dest);
                            }
                        }
                        else if (initialFunction != null) {
                            QName name = GetQNameAttribute(xpath, initialFunction, "@name");
                            XdmValue[] params2 = getParameters(xpath, initialFunction);
                            if (dest == null) {
                                result = transformer.CallFunction(name, params2);
                            }
                            else {
                                transformer.CallFunction(name, params2, dest);
                            }
                        }
                        else if (initialMatchSelection != null) {
                            if (dest == null) {
                                result = transformer.ApplyTemplates(initialMatchSelection);
                            }
                            else {
                                transformer.ApplyTemplates(initialMatchSelection, dest);
                            }
                        }
                        else {
                            if (dest == null) {
                                if (src != null) {
                                    result = transformer.ApplyTemplates(src, srcBaseUri);
                                }
                                else {
                                    result = transformer.ApplyTemplates(srcNode);
                                }
                            }
                            else {
                                if (validator != null) {
                                    validator.SetDestination(transformer.AsDocumentDestination(dest));
                                    validator.Validate(src, srcBaseUri);
                                } else if (src != null) {
                                    transformer.ApplyTemplates(src, srcBaseUri, dest);
                                }
                                else {
                                    transformer.ApplyTemplates(srcNode, dest);
                                }
                            }
                        }

                        if (resultAsTree && !resultSerialized) {
                            result = ((XdmDestination) (dest)).XdmNode;
                        }

                        if (resultSerialized) {
                            outcome.SetPrincipalSerializedResult(sw.ToString());
                        }

                        outcome.SetPrincipalResult(result);

                        if (saveResults) {
                            String s = sw.ToString();
                            // If a transform result is entirely xsl:result-document, then result will be null
                            if (!resultSerialized && result != null) {
                                StringWriter sw2 = new StringWriter();
                                Serializer se = env.processor.NewSerializer(sw2);
                                se.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");
                                env.processor.WriteXdmValue(result, se);
                                se.Close();
                                s = sw2.ToString();
                            }

                            // currently, only save the principal result file in the result directory
                            saveResultsToFile(s, resultsDir + "/results/" + testSetName + "/" + testName + ".out");
                            SysCollGen.Dictionary<Uri, TestOutcome.SingleResultDoc> xslResultDocuments =
                                outcome.GetSecondaryResultDocuments();
                            foreach (SysCollGen.KeyValuePair<Uri, TestOutcome.SingleResultDoc> entry in
                                xslResultDocuments) {
                                Uri key = entry.Key;
                                string path = key.AbsolutePath;
                                string serialization = outcome.Serialize(env.processor, entry.Value);

                                saveResultsToFile(serialization, path);
                            }
                        }
                    }
                    catch (Exception err) {
                        outcome.SetException(err);
                    }
                }
                else {
                    try {
                        XsltTransformer transformer = sheet.Load();

                        //transformer.SetURIResolver(env); //TODO
                        // if (env.unparsedTextResolver != null) {
                        //     transformer.TextResolver = env.unparsedTextResolver;
                        // }

                        if (initialTemplate != null) {
                            transformer.InitialTemplate = initialTemplateName;
                        }

                        if (initialMode != null) {
                            transformer.InitialMode = initialModeName;
                        }

                        foreach (XdmItem param in xpath.Evaluate("param", testInput)) {
                            string name = ((XdmNode) param).GetAttributeValue(new QName("name"));
                            string select = ((XdmNode) param).GetAttributeValue(new QName("select"));
                            XdmValue value = xpath.Evaluate(select, null);
                            transformer.SetParameter(new QName(name), value);
                        }

                        if (contextItem != null) {
                            transformer.InitialContextNode = (XdmNode) contextItem;
                        }

                        if (env.streamedPath != null) {
                            transformer.SetInputStream(new FileStream(env.streamedPath, FileMode.Open, FileAccess.Read),
                                testCase.BaseUri);
                        }

                        foreach (QName varName in env.params1.Keys) {
                            transformer.SetParameter(varName, env.params1[varName]);
                        }

                        transformer.BaseOutputUri = new Uri(resultsDir + "/results/output.xml");

                        // Run the transformation twice, once for serialized results, once for a tree.
                        // TODO: we could be smarter about this and capture both

                        // run with serialization
                        StringWriter sw = new StringWriter();
                        Serializer serializer = env.processor.NewSerializer(sw);
                        ResultDocumentHandler rdh = new (env.processor, outcome, testCase.BaseUri.AbsolutePath, true);
                        transformer.ResultDocumentHandler = rdh.HandleResultDocument;
                        transformer.Run(serializer);

                        outcome.SetPrincipalSerializedResult(sw.ToString());
                        if (saveResults) {
                            // currently, only save the principal result file
                            saveResultsToFile(sw.ToString(),
                                resultsDir + "/results/" + testSetName + "/" + testName + ".out");
                            SysCollGen.Dictionary<Uri, TestOutcome.SingleResultDoc> xslResultDocument =
                                outcome.GetSecondaryResultDocuments();

                            foreach (SysCollGen.KeyValuePair<Uri, TestOutcome.SingleResultDoc> entry in
                                xslResultDocument) {
                                Uri key = entry.Key;
                                if (key != null) {
                                    String path = entry.Key.AbsolutePath;
                                    String serialization = outcome.Serialize(env.processor, entry.Value);
                                    saveResultsToFile(serialization, path);
                                }
                            }
                        }

                        transformer.MessageListener = msg => outcome.AddXslMessage(msg.Content);

                        // run without serialization
                        if (env.streamedPath != null) {
                            transformer.SetInputStream(new FileStream(env.streamedPath, FileMode.Open, FileAccess.Read),
                                testCase.BaseUri);
                        }

                        XdmDestination destination = new XdmDestination();
                        ResultDocumentHandler rdh2 = new (env.processor, outcome, testCase.BaseUri.AbsolutePath, false);
                        transformer.ResultDocumentHandler = rdh2.HandleResultDocument;
                        transformer.Run(destination);
                        outcome.SetPrincipalResult(destination.XdmNode);
                    }
                    catch (SaxonApiException err) {
                        outcome.SetException(err);
                    }
                    catch (Exception err) {
                        outcome.SetException(err);
                        Console.Error.WriteLine(err);
                    }
                }
            }
            
            if (env.resetActions != null) {
                foreach (var act in env.resetActions) {
                    act.Invoke(env);
                }
                env.resetActions.Clear();
            }

            XdmNode assertion = (XdmNode) xpath.EvaluateSingle("result/*", testCase);
            //testCase.Select(PathFromList(new List<>Child("result"), Child().Where(IsElement())));
            if (assertion == null) {
                recordFailure(testSetName, testName);
                resultsDoc.writeTestcaseElement(testName, "fail", "No test assertions found");
                return;
            }

            XPathCompiler assertionXPath = env.processor.NewXPathCompiler();
            if (env.xsltCompiler.SchemaAware) {
                assertionXPath.SchemaAware = true;
            }

            //assertionXPath.setLanguageVersion("3.0");
            CopySchemaNamespace(env, assertionXPath);
            bool success = outcome.TestAssertion(outcome, assertion, outcome.GetPrincipalResultDoc(), assertionXPath,
                xpath, debug);
            if (success) {
                if (outcome.GetWrongErrorMessage() != null) {
                    outcome.SetComment(outcome.GetWrongErrorMessage());
                    wrongErrorResults++;
                }
                else {
                    successes++;
                }

                resultsDoc.writeTestcaseElement(testName, "pass", outcome.GetComment());
            }
            else {
                recordFailure(testSetName, testName);
                resultsDoc.writeTestcaseElement(testName, "fail", outcome.GetComment());
            }
        }

        private bool AssertOptimization(XsltExecutable stylesheet, string assertion) {
            XdmDestination builder = new XdmDestination();
            stylesheet.Explain(builder);
            builder.Close();
            XdmNode expressionTree = builder.XdmNode;
            XPathCompiler xpe = stylesheet.Processor.NewXPathCompiler();
            XPathSelector exp = xpe.Compile(assertion).Load();
            exp.ContextItem = expressionTree;
            XdmAtomicValue bv = (XdmAtomicValue) exp.EvaluateSingle();
            if (bv == null || !bv.AsBoolean()) {
                println("** Optimization assertion failed");
                println(expressionTree.StringValue);
            }

            return bv != null && bv.AsBoolean();
        }

        protected XsltExecutable exportImport(string testName, string testSetName, TestOutcome outcome,
            XsltCompiler compiler, XsltExecutable sheet, Stream styleSource) {
            try {
                if (export && IsExportable(testName)) {
                    sheet = ExportStylesheet(testName, testSetName, compiler, sheet, styleSource);
                }
                else if (sheet == null) {
                    sheet = compiler.Compile(styleSource);
                }
            }

            catch (Exception err) {
                //err.printStackTrace();
                Console.WriteLine(err.Message);
                outcome.SetException(err);
                //outcome.setErrorsReported(collector.getErrorCodes());
            }

            return sheet;
        }

        private XsltExecutable ExportStylesheet(string testName, string testSetName, XsltCompiler compiler,
            XsltExecutable sheet, Stream styleSource) {
            try {
                Uri fileUri = new Uri(resultsDir + "/export/" + testSetName + "/" + testName + ".sef");
                Stream exportFile = new FileStream(fileUri.AbsolutePath, FileMode.Open, FileAccess.Read);

                XsltPackage compiledPack = compiler.CompilePackage(styleSource);
                compiledPack.Save(exportFile);

                sheet = ReloadExportedStylesheet(compiler, fileUri);
            }
            catch (Exception e) {
                // compiler.GetErrorList().Add(new StaticError(e));
                //System.err.println(e.getMessage());
                //e.printStackTrace();  //temporary, for debugging
                throw;
            }

            return sheet;
        }

        private XsltExecutable ReloadExportedStylesheet(XsltCompiler compiler, Uri exportFile) {
            return compiler.LoadExecutablePackage(exportFile);
        }

        private bool IsExportable(string testName) {
            return !(testName.Equals("load-xquery-module-004") || testName.Equals("transform-004"));
        }

        private XsltPackage exportImportPackage(string testName, string testSetName, TestOutcome outcome,
            XsltCompiler compiler, XsltPackage pack) {
            try {
                if (export) {
                    try {
                        Uri fileUri = new Uri(resultsDir + "/export/" + testSetName + "/" + testName + ".base.sef");
                        Stream exportFile = new FileStream(fileUri.AbsolutePath, FileMode.Open, FileAccess.Read);

                        Console.WriteLine("Exported package to " + fileUri);
                        pack.Save(exportFile);
                        return compiler.LoadLibraryPackage(fileUri);
                    }
                    catch (Exception e) {
                        Console.WriteLine(e.Message);
                        //e.printStackTrace();  //temporary, for debugging
                        throw;
                    }
                }
                else {
                    return pack;
                }
            }
            catch (Exception err) {
                outcome.SetException(err);

                //  outcome.SetErrorsReported(collector.getErrorCodes());
            }

            /* catch (Exception err)
             {
                 err.printStackTrace();
                 System.err.println(err.getMessage());
                 outcome.setException(new QName(err.getClass().getName()));
                 outcome.setErrorsReported(collector.getErrorCodes());
             }*/
            return pack;
        }

        public class EnvironmentVariableResolver : Saxon.Hej.lib.EnvironmentVariableResolver {
            public EnvironmentVariableResolver() {
            }

            public SysCollGen.ISet<string> getAvailableEnvironmentVariables() {
                SysCollGen.HashSet<string> strings = new SysCollGen.HashSet<string>();
                strings.Add("QTTEST");
                strings.Add("QTTEST2");
                strings.Add("QTTESTEMPTY");
                return strings;
            }

            public string getEnvironmentVariable(string name) {
                if (name.Equals("QTTEST")) {
                    return "42";
                }
                else if (name.Equals("QTTEST2")) {
                    return "other";
                }
                else if (name.Equals("QTTESTEMPTY")) {
                    return "";
                }
                else {
                    return null;
                }
            }
        }

        public class ResultDocumentHandler {
            private Processor proc;
            private TestOutcome outcome;
            private Uri baseOutputUri;
            bool serialized;

            public ResultDocumentHandler(Processor processor, TestOutcome outcome, String baseOutputURI,
                bool serialized) {
                this.proc = processor;
                this.outcome = outcome;
                try {
                    this.baseOutputUri = new Uri(baseOutputURI);
                }
                catch (Exception) {
                }

                this.serialized = serialized;
            }

            public IDestination HandleResultDocument(string href, Uri baseUri) {
                if (serialized) {
                    StringWriter writer = new StringWriter();
                    Serializer dest = proc.NewSerializer();

                    XmlUrlResolver res = new XmlUrlResolver();
                    Uri uri = res.ResolveUri(baseUri, href);
                    dest.BaseUri = uri;
                    dest.OutputWriter = writer;

                    if (uri.Equals(baseOutputUri)) {
                        dest.OnClose(() => outcome.SetPrincipalSerializedResult(writer.ToString()));
                    }
                    else {
                        dest.OnClose(() => outcome.SetSecondaryResult(uri, null, writer.ToString(), proc));
                    }

                    return dest;
                }
                else {
                    XdmDestination dest = new XdmDestination();
                    XmlUrlResolver res = new XmlUrlResolver();
                    Uri uri = res.ResolveUri(baseUri, href);
                    dest.BaseUri = uri;
                    dest.OnClose(() => outcome.SetSecondaryResult(uri, dest.XdmNode, null, proc));
                    return dest;
                }
            }
        }


        internal bool mustSerialize(XdmNode testCase, XPathCompiler xpath) {
            return saveResults ||
                   ((XdmAtomicValue) xpath.EvaluateSingle(
                       "exists(./result//(assert-serialization-error|serialization-matches|assert-serialization)[not(parent::*[self::assert-message|self::assert-result-document])])",
                       testCase)).AsBoolean();
        }

        private void saveResultsToFile(string content, string filePath) {
            try {
                System.IO.File.WriteAllText(filePath, content);
            }
            catch (IOException e) {
                println("*** Failed to save results to " + filePath);
                throw;
            }
        }


        public class ResetAction {
            Environment env = null;
            decimal oldVersion;

            ResetAction(Environment envi, decimal oldVer) {
                oldVersion = oldVer;
                env = envi;
            }

            public void reset(Environment env) {
                env.processor.XmlVersion = oldVersion;
            }
        }


        public override bool EnsureDependencySatisfied(XdmNode dependency, Environment env) {
            string type = dependency.NodeName.LocalName;
            string value = dependency.GetAttributeValue("value") ?? "*";

            String tv = type + "/" + value;
            bool inverse = "false".Equals(dependency.GetAttributeValue("satisfied"));
            bool needed = !"false".Equals(dependency.GetAttributeValue("satisfied"));

            if (alwaysOn.Contains(type) || alwaysOn.Contains(tv)) {
                return needed;
            }

            if (alwaysOff.Contains(type) || alwaysOff.Contains(tv)) {
                return !needed;
            }

            String edition = env.processor.Edition;

            if (needsPE.Contains(type) || needsPE.Contains(tv)) {
                return (edition.Equals("PE") || edition.Equals("EE")) == needed;
            }

            if (tv == "feature/schema_aware") {
                env.xsltCompiler.SchemaAware = needed;
            }

            if (needsEE.Contains(type) || needsEE.Contains(tv)) {
                return edition.Equals("EE") == needed;
            }


            switch (type) {
                case "spec":

                    bool atLeast = value.EndsWith("+");
                    value = value.Replace("+", "");

                    String specName = SpecAttr.ShortNameAndVersionOf(requestedSpec).Replace("XT", "XSLT");
                    int order = value.CompareTo(specName);
                    return atLeast ? order <= 0 : order == 0;

                case "feature":
                    switch (value) {
                        case "XML_1.1": {
                            return inverse;
                            // decimal requiredVersion = (decimal) (inverse ? 1.0 : 1.1);
                            // decimal oldVersion = env.processor.XmlVersion;
                            // env.resetActions.Add(envi => { envi.processor.XmlVersion = oldVersion; }
                            // );
                            // env.processor.XmlVersion = requiredVersion;
                            // return true;
                        }

                        case "XSD_1.1": {
                            String requiredVersion = (inverse ? "1.0" : "1.1");
                            String oldVersion = env.processor.GetProperty(Saxon.Api.Feature<string>.XSD_VERSION);
                            if (!oldVersion.Equals(requiredVersion)) {
                                env.processor.SetProperty(Saxon.Api.Feature<string>.XSD_VERSION, requiredVersion);
                                env.resetActions.Add(envi => {
                                    envi.processor.SetProperty(Saxon.Api.Feature<String>.XSD_VERSION, oldVersion);
                                });
                            }

                            return true;
                        }

                        case "higher_order_functions":
                            return !inverse;
                        case "simple-uca-fallback":
                            return !inverse;
                        case "advanced-uca-fallback":
                            return (edition.Equals("PE") || edition.Equals("EE")) ^ inverse;
                        case "streaming-fallback":
                            bool requiredi = !inverse;
                            bool old = (bool) env.processor.GetProperty(Saxon.Api.Feature<bool>.STREAMING_FALLBACK);
                            if (old != requiredi) {
                                env.processor.SetProperty(Saxon.Api.Feature<bool>.STREAMING_FALLBACK, requiredi);
                                env.resetActions.Add(envi => {
                                    env.processor.SetProperty(Saxon.Api.Feature<bool>.STREAMING_FALLBACK, old);
                                });
                            }

                            return true;


                        default:
                            Console.WriteLine("*** Unknown feature in HE: " + value);
                            return env.processor.Edition.Equals("HE")
                                ? false
                                : true; //in java the true is null. Check logic on .net
                    }

                case "platform":
                    string[] platforms = value.Split(' ');
                    return Array.IndexOf(platforms, "CS") >= 0;
                case "default_language_for_numbering": {
                    string old = env.processor.GetProperty(Saxon.Api.Feature<string>.DEFAULT_LANGUAGE);
                    if (!value.Equals(old)) {
                        env.processor.SetProperty(Saxon.Api.Feature<string>.DEFAULT_LANGUAGE, value);
                        env.resetActions.Add(envi => {
                                env.processor.SetProperty(Saxon.Api.Feature<string>.DEFAULT_LANGUAGE, old);
                            }
                        );
                    }

                    return true;
                }
                case "enable_assertions": {
                    bool on = !inverse;
                    bool old = env.xsltCompiler.AssertionsEnabled;
                    env.xsltCompiler.AssertionsEnabled = on;
                    env.resetActions.Add(envi => { env.xsltCompiler.AssertionsEnabled = old; });
                    return true;
                }
                case "extension-function":
                    /* if (value.Equals("Q{http://relaxng.org/ns/structure/1.0}schema-report#1")) {
                         try {
                             Configuration config = env.processor.getUnderlyingConfiguration();
     Object sf = config.getInstance("net.cfoster.saxonjing.SchemaFunction", null);
     env.processor.registerExtensionFunction((ExtensionFunctionDefinition) sf);
                             Object sfd = config.getInstance("net.cfoster.saxonjing.SchemaReportFunction", null);
     env.processor.registerExtensionFunction((ExtensionFunctionDefinition) sfd);
                             return true;
                         } catch (XPathException err) {
                             System.err.println("Failed to load Saxon-Jing extension functions");
                             return false;
                         }
                     }*/
                    return false;
                case "year_component_values":
                    if ("support year zero".Equals(value)) {
                        if (env != null) {
                            env.processor.SetProperty(Saxon.Api.Feature<string>.XSD_VERSION, inverse ? "1.0" : "1.1");
                            return true;
                        }
                        else {
                            return false;
                        }
                    }

                    return !inverse;
                case "additional_normalization_form":
                    if ("support FULLY-NORMALIZED".Equals(value)) {
                        return inverse;
                    }

                    return !inverse;

                case "on-multiple-match":
                    throw new Exception("on-multiple-match is no longer recognized");
//                env.resetActions.add(new Environment.ResetAction() {
//                    @Override
//                    public void reset(Environment env) {
//                        env.xsltCompiler.getUnderlyingCompilerInfo().setRecoveryPolicy(Mode.RECOVER_WITH_WARNINGS);
//                    }
//                });
//                if ("error".Equals(value)) {
//                    env.xsltCompiler.getUnderlyingCompilerInfo().setRecoveryPolicy(Mode.DO_NOT_RECOVER);
//                } else {
//                    env.xsltCompiler.getUnderlyingCompilerInfo().setRecoveryPolicy(Mode.RECOVER_SILENTLY);
//                }
//                return true;

                case "ignore_doc_failure":
                    return inverse;
                
                case "languages_for_numbering":
                    return value.Equals("en") || inverse;

                case "combinations_for_numbering":
                    if (value.Equals("COPTIC EPACT DIGIT ONE") || value.Equals("SINHALA ARCHAIC DIGIT ONE") ||
                        value.Equals("MENDE KIKAKUI DIGIT ONE")) {
                        return false;
                    }

                    return !inverse;

                case "xsd-version":
                    return env.processor.Edition.Equals("HE") ? false : true;

                case "sweep_and_posture":
                    return env.processor.Edition.Equals("HE") ? inverse : true;

                case "unicode-version":
                    return value.Equals("6.0"); // Avoid running Unicode 9.0 tests - they are slow!

                case "default_html_version": {
                    return value.Equals("5");
                }

                default:
                    println("**** dependency not recognized for HE: " + type);
                    return false;
            }


            // if ("spec".Equals(type)) {
            //     bool atLeast = value.EndsWith("+");
            //     value = value.Replace("+", "");
            //
            //     String specName = ((SpecAttr) spec.GetAttr()).svname.Replace("XT", "XSLT");
            //     int order = value.CompareTo(specName);
            //     return atLeast ? order <= 0 : order == 0;
            // }
            // else if ("feature".Equals(type)) {
            //     //            <xs:enumeration value="backwards_compatibility" />
            //     //            <xs:enumeration value="disabling_output_escaping" />
            //     //            <xs:enumeration value="schema_aware" />
            //     //            <xs:enumeration value="namespace_axis" />
            //     //            <xs:enumeration value="streaming" />
            //     //            <xs:enumeration value="XML_1.1" />
            //
            //     if ("XML_1.1".Equals(value) && !inverse) {
            //         if (env != null) {
            //             env.processor.XmlVersion = (decimal) 1.1;
            //             return true;
            //         }
            //         else {
            //             return false;
            //         }
            //     }
            //     else if ("disabling_output_escaping".Equals(value)) {
            //         return !inverse;
            //     }
            //     else if ("schema_aware".Equals(value)) {
            //         if (!env.xsltCompiler.SchemaAware) {
            //             return false; // cannot use the selected tree model for schema-aware tests
            //         }
            //
            //         if (env != null) {
            //             env.xsltCompiler.SchemaAware = !inverse;
            //         }
            //
            //         return true;
            //     }
            //     else if ("namespace_axis".Equals(value)) {
            //         return !inverse;
            //     }
            //     else if ("streaming".Equals(value)) {
            //         return !inverse;
            //     }
            //     else if ("backwards_compatibility".Equals(value)) {
            //         return !inverse;
            //     }
            //
            //     return false;
            // }
            // else if ("xsd-version".Equals(type)) {
            //     if ("1.1".Equals(value)) {
            //         if (env != null) {
            //             env.processor.SetProperty(JFeatureKeys.XSD_VERSION, (inverse ? "1.0" : "1.1"));
            //         }
            //         else {
            //             return false;
            //         }
            //     }
            //     else if ("1.0".Equals(value)) {
            //         if (env != null) {
            //             env.processor.SetProperty(JFeatureKeys.XSD_VERSION, (inverse ? "1.1" : "1.0"));
            //         }
            //         else {
            //             return false;
            //         }
            //     }
            //
            //     return true;
            // }
            // else if ("available_documents".Equals(type)) {
            //     return !inverse;
            // }
            // else if ("default_language_for_numbering".Equals(type)) {
            //     return !inverse;
            // }
            // else if ("languages_for_numbering".Equals(type)) {
            //     return !inverse;
            // }
            // else if ("supported_calendars_in_date_formatting_functions".Equals(type)) {
            //     return !inverse;
            // }
            // else if ("default_calendar_in_date_formatting_functions".Equals(type)) {
            //     return !inverse;
            // }
            // else if ("maximum_number_of_decimal_digits".Equals(type)) {
            //     return !inverse;
            //     //        } else if ("collation_uri".Equals(type)) {
            //     //            return !inverse;
            //     //        } else if ("statically_known_collations".Equals(type)) {
            //     //            if (value.Equals("http://www.w3.org/xslts/collation/caseblind") && !inverse) {
            //     //                env.processor.getUnderlyingConfiguration().setCollationURIResolver(
            //     //                        new StandardCollationURIResolver() {
            //     //                            public stringCollator resolve(string uri, string base, Configuration config) {
            //     //                                if ("http://www.w3.org/xslts/collation/caseblind".Equals(uri)) {
            //     //                                    return super.resolve("http://saxon.sf.net/collation?ignore-case=yes", "", config);
            //     //                                } else {
            //     //                                    return super.resolve(uri, base, config);
            //     //                                }
            //     //                            }
            //     //                        }
            //     //                );
            //     //            }
            //     //            // Alternative case-blind collation URI used in QT3 tests
            //     //            if (value.Equals("http://www.w3.org/2010/09/qt-fots-catalog/collation/caseblind") && !inverse) {
            //     //                env.processor.getUnderlyingConfiguration().setCollationURIResolver(
            //     //                        new StandardCollationURIResolver() {
            //     //                            public stringCollator resolve(string uri, string base, Configuration config) {
            //     //                                if ("http://www.w3.org/2010/09/qt-fots-catalog/collation/caseblind".Equals(uri)) {
            //     //                                    return super.resolve("http://saxon.sf.net/collation?ignore-case=yes", "", config);
            //     //                                } else {
            //     //                                    return super.resolve(uri, base, config);
            //     //                                }
            //     //                            }
            //     //                        }
            //     //                );
            //     //            }
            //     //            return true;
            // }
            // else if ("default_output_encoding".Equals(type)) {
            //     return !inverse;
            // }
            // else if ("unparsed_text_encoding".Equals(type)) {
            //     return !inverse;
            // }
            // else if ("additional_normalization_form".Equals(type)) {
            //     return !inverse;
            // }
            // else if ("recognize_id_as_uri_fragment".Equals(type)) {
            //     return !inverse;
            // }
            // else if ("on-multiple-match".Equals(type)) {
            //     throw new Exception("on-muplte-match is no longer recognized");
            //     /*if (value.Equals("error"))
            //     {
            //         env.xsltCompiler.GetUnderlyingCompilerInfo().setRecoveryPolicy((int)RecoveryPolicy.DoNotRecover);
            //     }
            //     else
            //     {
            //         env.xsltCompiler.GetUnderlyingCompilerInfo().setRecoveryPolicy((int)RecoveryPolicy.RecoverSilently);
            //     }
            //     return true; */
            // }
            // else if ("ignore_doc_failure".Equals(type)) {
            //     return inverse;
            // }
            // else if ("combinations_for_numbering".Equals(type)) {
            //     if (value.Equals("COPTIC EPACT DIGIT ONE") || value.Equals("SINHALA ARCHAIC DIGIT ONE") ||
            //         value.Equals("MENDE KIKAKUI DIGIT ONE")) {
            //         return false;
            //     }
            //
            //     return !inverse;
            // }
            // else if ("higher_order_functions".Equals(type)) {
            //     return !inverse;
            // }
            // else if ("ignore_doc_failure".Equals(type)) {
            //     //TODO
            //     return false;
            // }
            // else if ("simple-uca-fallback".Equals(type)) {
            //     return !inverse;
            // }
            // else if ("advanced-uca-fallback".Equals(type)) {
            //     return !inverse;
            // }
            // else if ("streaming-fallback".Equals(type)) {
            //     bool required = !inverse;
            //     bool old = env.processor.GetProperty(FeatureKeys.STREAMING_FALLBACK) == "true" ||
            //                env.processor.GetProperty(FeatureKeys.STREAMING_FALLBACK) == "on";
            //     if (old != required) {
            //         env.processor.SetProperty<bool>(Feature<bool>.STREAMING_FALLBACK, required);
            //     }
            //
            //     return true;
            // }
            // else if ("sweep_and_posture".Equals(type)) {
            //     return !inverse;
            // }
            // else if ("unicode_version".Equals(type)) {
            //     return value.Equals("6.0");
            // }
            // else if ("enable_assertions".Equals(type)) {
            //     return false;
            // }
            // else if ("year_component_values".Equals(type)) {
            //     if ("support year zero".Equals(value)) {
            //         if (env != null) {
            //             env.processor.SetProperty<String>(Feature<String>.XSD_VERSION, inverse ? "1.0" : "1.1");
            //         }
            //         else {
            //             return false;
            //         }
            //     }
            //
            //     return !inverse;
            // }
            // else {
            //     println("**** dependency not recognized: " + type);
            //     return false;
            // }
        }

        /*private static string getCanonicalPath(File file) {
            try {
                return file.getCanonicalPath();
            } catch (IOException err) {
                return file.getAbsolutePath();
            }
        }*/

        private static QName getQNameAttribute(XPathCompiler xpath, XdmItem contextItem, string attributePath) {
            string exp = "for $att in " + attributePath +
                         " return if (contains($att, ':')) then resolve-QName($att, $att/..) else QName('', $att)";
            XdmAtomicValue qname = (XdmAtomicValue) xpath.EvaluateSingle(exp, contextItem);
            return (qname == null ? null : (QName) qname.Value);
        }

    }

    public class ModuleResolver : ModuleURIResolver {
        // To support XSLT stylesheets that use load-query-module(), we're setting a query module resolver
        // in the Configuration. This isn't supported directly in the Saxon-CS API, we have to dive down into
        // internals to achieve the required effect
        XQueryCompiler catXQC;
        XdmNode testCase;
        Driver driver = null;

        public ModuleResolver(XQueryCompiler xqc, Driver driveri) {
            this.catXQC = xqc;
            driver = driveri;
        }

        public void setTestCase(XdmNode testCase) {
            this.testCase = testCase;
        }

        public Saxon.Ejavax.xml.transform.stream.StreamSource[] resolve(string moduleURI, string baseURI, string[] locations) {
            Uri absoluteUri = (new XmlUrlResolver()).ResolveUri(testCase.BaseUri, moduleURI);
            FileStream stream = new (driver.queryModules[absoluteUri], FileMode.Open, FileAccess.Read);
            StreamSource ss = new (stream, absoluteUri.ToString());
            return new[] {ss};
        }

    }
}