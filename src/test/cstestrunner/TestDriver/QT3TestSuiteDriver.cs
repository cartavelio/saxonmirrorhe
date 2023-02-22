using System;
using SysCollGen = System.Collections.Generic;
using System.IO;
using Saxon.Api;
using static Saxon.Api.Steps;
using static Saxon.Api.Predicates;

// file:////C:/work/repositories/hg/qt3-testsuite/ C:/work/repositories/hg/qt3-testsuite/results/saxon/ -lang:XQ30
//using Saxon.Hej.Version;
using JFeatureKeys = Saxon.Hej.lib.FeatureKeys;
using JConfiguration = Saxon.Hej.Configuration;
using JVersion = Saxon.Hej.Version;
using System.Linq;
using System.Net;
using System.Text;
using Saxon.Eej.functions.registry;
using Saxon.Hej.trans;


namespace TestDriver {
    /**
     * Test Driver for the QT3/QT4 test suite
     */
    public class QT3TestSuiteDriver : Driver {
        public static string RNS = "http://www.w3.org/2012/08/qt-fots-results";
        public static string CNS = "http://www.w3.org/2010/09/qt-fots-catalog";


        public object getDependencyDictionary() {
            return dependencyDictionary;
        }

        private readonly SysCollGen.Dictionary<string, Dependency> dependencyDictionary = new ();
        

        public override string catalogNamespace() {
            return CNS;
        }


        protected override void processSpec(string specStr) {
            if (specStr.Equals("XP20")) {
                requestedSpec = Spec.XP20;
            }
            else if (specStr.Equals("XP30")) {
                requestedSpec = Spec.XP30;
            }
            else if (specStr.Equals("XP31")) {
                requestedSpec = Spec.XP31;
            }
            else if (specStr.Equals("XP40")) {
                requestedSpec = Spec.XP40;
            }
            else if (specStr.Equals("XQ10")) {
                requestedSpec = Spec.XQ10;
            }
            else if (specStr.Equals("XQ30")) {
                requestedSpec = Spec.XQ30;
            }
            else if (specStr.Equals("XQ31")) {
                requestedSpec = Spec.XQ31;
            }
            else if (specStr.Equals("XQ40")) {
                requestedSpec = Spec.XQ40;
            }
            else {
                System.Console.WriteLine(
                    "The specific language must be one of the following: XP20, XP30, XQ10, XQ30 or XQ31");
            }

            resultsDoc = new FotsResultsDocument(this.getResultsDir(), requestedSpec, reportFilename);
        }
        
        public static void MainQT(string[] args) {
            if (args.Length == 0 || args[0].Equals("-?")) {
                System.Console.WriteLine("testsuiteDir  [-o:resultsdir] [-s:testSetName]" +
                                         " [-t:testNamePattern] [-unfolded] [-tree] [-lang:XP20|XP30|XQ10|XQ30|XQ31]");
                return;
            }

            System.Console.WriteLine("Testing Saxon " + (new Processor()).ProductVersion);
            new QT3TestSuiteDriver().go(args);
        }


        protected override void createGlobalEnvironments(XdmNode catalog, XPathCompiler xpc) {
            Environment environment = null;

            Environment defaultEnv = null;
            try {
                defaultEnv = localEnvironments["default"];
            }
            catch (Exception) {
            }

            foreach (XdmNode env in catalog.Select(Descendant("environment"))) {
                environment = Environment.processEnvironment(
                    this, xpc, env, globalEnvironments, defaultEnv);
            }

            buildDependencyDictionary(driverProc, environment, catalog);
        }

        /**
         * Decide whether a dependency is satisfied
         *
         * @param dependency the dependency element in the catalog
         * @param env        an environment in the catalog, which can be modified to satisfy the dependency if necessary.
         *                   May be null.
         * @return true if the environment satisfies the dependency, else false
         */
        public override bool EnsureDependencySatisfied(XdmNode dependency, Environment env) {
            string type = dependency.GetAttributeValue(new QName("type"));
            string value = dependency.GetAttributeValue(new QName("value"));
            string edition = env.processor.Edition;
            bool inverse = "false".Equals(dependency.GetAttributeValue(new QName("satisfied")));
            switch (type) {
                case "xml-version": {
                    if (value.Equals("1.0:4-") && !inverse) {
                        // we don't support XML 1.0 4th edition or earlier
                        return false;
                    }

                    if (value.Contains("1.1") && !inverse) {
                        return false;
                        // if (env != null) {
                        //     env.processor.SetProperty(JFeatureKeys.XML_VERSION, "1.1");
                        // }
                        // else {
                        //     return false;
                        // }
                    }
                    else if (value.Contains("1.0") && !inverse) {
                        if (env != null) {
                            env.processor.SetProperty(JFeatureKeys.XML_VERSION, "1.0");
                        }
                        else {
                            return false;
                        }
                    }

                    return true;
                }
                case "xsd-version": {
                    if ("1.1".Equals(value)) {
                        if (env != null) {
                            env.processor.SetProperty(JFeatureKeys.XSD_VERSION, (inverse ? "1.0" : "1.1"));
                        }
                        else {
                            return false;
                        }
                    }
                    else if ("1.0".Equals(value)) {
                        if (env != null) {
                            env.processor.SetProperty(JFeatureKeys.XSD_VERSION, (inverse ? "1.1" : "1.0"));
                        }
                        else {
                            return false;
                        }
                    }

                    return true;
                }
                case "limits": {
                    return "year_lt_0".Equals(value) && !inverse;
                }
                case "spec": {
                    return isApplicableToSpecVersion(value, requestedSpec);
                    string xpcVersion = env.xpathCompiler.XPathLanguageVersion;
                    // string xqcVersion = env.xqueryCompiler.XQueryLanguageVersion;
                    // string xtVersion = env.xsltCompiler != null ? env.xsltCompiler.XsltLanguageVersion : "3.0";
                    // Spec test
                    // env.xpathCompiler.XPathLanguageVersion = ((SpecAttr) (requestedSpec.GetAttr())).version;
                    // env.xqueryCompiler.XQueryLanguageVersion = ((SpecAttr) (requestedSpec.GetAttr())).version;
                    // if (env.xsltCompiler != null) {
                    //     env.xsltCompiler.XsltLanguageVersion = ((SpecAttr) (requestedSpec.GetAttr())).version;
                    // }
                    // env.resetActions.Add((envt) =>  {
                    //     envt.xpathCompiler.XPathLanguageVersion=xpcVersion;
                    //     envt.xqueryCompiler.XQueryLanguageVersion=xqcVersion;
                    //     if (envt.xsltCompiler != null) {
                    //         envt.xsltCompiler.XsltLanguageVersion=xtVersion;
                    //     }
                    // });
                    // return true;
                }
                case "collection-stability": {
                    // SAXON has a problem here - we don't support stable collections
                    return ("false".Equals(value) != inverse);
                }
                case "default-language": {
                    return ("en".Equals(value) != inverse);
                }
                case "directory-as-collection-uri": {
                    return ("true".Equals(value) != inverse);
                }
                case "language": {
                    string[] supported = {"en", "de", "fr", "it", "xib"};
                    return supported.Contains(value) != inverse;
                }
                case "calendar": {
                    return (("AD".Equals(value) || "ISO".Equals(value)) != inverse);
                }
                case "format-integer-sequence": {
                    return !inverse;
                }
                case "unicode-normalization-form": {
                    return value.Equals("FULLY-NORMALIZED", StringComparison.OrdinalIgnoreCase) ? inverse : !inverse;
                }
                case "unicode-version": {
                    return value.Equals("13") != inverse;
                }
                case "put": {
                    return ("comment" == value || "text" == value
                                                    || "processing-instruction" == value
                                                    || "attribute" == value) == inverse;
                }
                case "revalidation": {
                    return !inverse;
                }
                case "saxon:config": {
                    string name = dependency.GetAttributeValue("name");
                    if (name != null) {
                        string oldValue = env.processor.GetProperty(name);
                        env.processor.SetProperty(name, value);
                        env.resetActions.Add((env) => { env.processor.SetProperty(name, oldValue); });
                    }
                    return true;
                }
                case "saxon:platform": {
                    return value.Split(' ').Contains("CS");
                }
                case "feature": {
                    if ("namespace-axis".Equals(value)) {
                        return !inverse;
                    }
                    else if ("higherOrderFunctions".Equals(value)) {
                        return !inverse;
                    }
                    else if ("schemaImport".Equals(value) || "schemaValidation".Equals(value) ||
                             "schemaAware".Equals(value) || "schema-location-hint".Equals(value)) {
                        return makeSchemaAware(env, inverse);

                    }
                    else if ("staticTyping".Equals(value)) {
                        return inverse;
                    }
                    else if ("moduleImport".Equals(value)) {
                        return !inverse;
                    }
                    else if ("schema-location-hint".Equals(value)) {
                        return !inverse;
                    }
                    else if ("info-dtd".Equals(value)) {
                        if (treeModel == "tiny" || treeModel == "linked" ||
                            treeModel == "condensed") {
                            return !inverse;
                        }
                        else {
                            return inverse;
                        }
                    }
                    else if ("serialization".Equals(value)) {
                        return true;
                    }
                    else if ("non_codepoint_collation".Equals(value)) {
                        return !inverse;
                    }
                    else if ("non_empty_sequence_collection".Equals(value)) {
                        return !inverse;
                    }
                    else if ("fn-load-xquery-module".Equals(value)) {
                        return !inverse;
                    }
                    else if ("fn-transform-XSLT".Equals(value)) {
                        return !inverse;
                    }
                    else if ("fn-transform-XSLT30".Equals(value)) {
                        return (edition.Equals("PE") || edition.Equals("EE")) ^ inverse;
                    }
                    else if ("fn-format-integer-CLDR".Equals(value)) {
                        return (edition.Equals("PE") || edition.Equals("EE")) ^ inverse;
                    }
                    else if ("simple-uca-fallback".Equals(value)) {
                        return !inverse;
                    }
                    else if ("advanced-uca-fallback".Equals(value)) {
                        return (edition.Equals("PE") || edition.Equals("EE")) ^ inverse;
                    }
                    else if ("XQUpdate" == value) {
                        if (edition == "EE" && !inverse) {
                            env.xqueryCompiler.UpdatingEnabled = true;
                        }
                        return edition == "EE" && !inverse;
                    }
                    
                    else {
                        println("**** feature = " + value + "  ????");
                        return false;
                    }
                }
                default: {
                    println("**** dependency not recognized: " + type);
                    return false;
                }
            }
        }
        
        private static bool isApplicableToSpecVersion(string value, Spec spec) {
            bool applicable = false;
            string tag = SpecAttr.ShortNameAndVersionOf(spec);
            if (!value.Contains(SpecAttr.ShortNameOf(spec))) {
                applicable = false;
            } else if (value.Contains(tag)) {
                applicable = true;
            } else if ((tag is "XQ30" or "XQ31" or "XQ40") &&
                       (value.Contains("XQ10+") || value.Contains("XQ30+") || value.Contains("XQ31+"))) {
                applicable = true;
            } else if ((tag is "XP30" or "XP31" or "XP40") &&
                       (value.Contains("XP20+") || value.Contains("XP30+") || value.Contains("XP31+"))) {
                applicable = true;
            }
            return applicable;
        }

        protected bool makeSchemaAware(Environment env, bool inverse) {
            // if (!env.processor.SchemaAware) {
            //     return false; // cannot use the selected tree model for schema-aware tests
            // }

            if (inverse) {
                // force use of a non-schema-aware processor by creating a ProfessionalConfiguration
                Processor savedProcessor = env.processor;
                Saxon.Eej.config.ProfessionalConfiguration pConfig = new Saxon.Eej.config.ProfessionalConfiguration();
                pConfig.setNamePool(env.processor.Implementation.getNamePool());
                env.processor = new Processor(pConfig);
                env.processor.SetProperty(Feature.STABLE_UNPARSED_TEXT, true);


                // Note that at present no variables are copied across
                bool xpcState = env.xpathCompiler.SchemaAware;
                bool xqcState = env.xqueryCompiler.SchemaAware;
                bool xtState = env.xsltCompiler != null && env.xsltCompiler.SchemaAware;
                env.xpathCompiler = env.processor.NewXPathCompiler();
                env.xqueryCompiler = env.processor.NewXQueryCompiler();
                env.xsltCompiler = env.processor.NewXsltCompiler();
                env.xpathCompiler.SchemaAware = false;
                env.xqueryCompiler.SchemaAware = false;
                env.xsltCompiler.SchemaAware = false;
                env.resetActions.Add((envt) =>  {
                        envt.xpathCompiler.SchemaAware = (xpcState);
                        envt.xqueryCompiler.SchemaAware = (xqcState);
                        if (envt.xsltCompiler != null) {
                            envt.xsltCompiler.SchemaAware = (xtState);
                        }
                    }
                );
            } else {
                bool xpcState = env.xpathCompiler.SchemaAware;
                bool xqcState = env.xqueryCompiler.SchemaAware;
                bool xtState = env.xsltCompiler != null && env.xsltCompiler.SchemaAware;
                env.xpathCompiler.SchemaAware = (true);
                env.xqueryCompiler.SchemaAware = (true);
                if (env.xsltCompiler != null) {
                    env.xsltCompiler.SchemaAware = (true);
                }
                env.resetActions.Add((envt) =>  {
                    envt.xpathCompiler.SchemaAware=(xpcState);
                    envt.xqueryCompiler.SchemaAware=(xqcState);
                    if (envt.xsltCompiler != null) {
                        envt.xsltCompiler.SchemaAware=(xtState);
                    }
                });
            }
            
            return true;
        }

        private bool isSlow(string name) {
            return name.StartsWith("unicode90");
        }

        /**
         * Run a test case
         *
         *
         * @param testCase the test case element in the catalog
         * @param xpc      the XPath compiler to be used for compiling XPath expressions against the catalog
         * @
         */
        protected override void runTestCase(XdmNode testCase, XPathCompiler xpc) {
            bool run = true;
            bool xpDependency = false;
            string hostLang;
            string langVersion;
            Spec specOpt = Spec.NULL;
            XPathCompiler xpath = driverProc.NewXPathCompiler();
            string testCaseName = testCase.GetAttributeValue(new QName("name"));
            string testSetName = testCase.Parent.GetAttributeValue(new QName("name"));

            XdmNode resultElement = testCase.Select(Child("result")).FirstOrDefault();
            ;
            bool needSerializedResult = resultElement.Select(Descendant("assert-serialization-error")).Any()
                                        || resultElement.Select(Descendant("serialization-matches")).Any();
            bool needResultValue = true;
            if (needSerializedResult) {
                needResultValue = resultElement.Select(Descendant(IsElement()).Where(Not(AnyOf(
                    HasLocalName("serialization-matches"),
                    HasLocalName("assert-serialization-error"),
                    HasLocalName("any-of"),
                    HasLocalName("all-of"))))).Any();
            }


            XdmNode alternativeResult = null;
            XdmNode optimization = null;


            hostLang = ((SpecAttr) requestedSpec.GetAttr()).sname;
            langVersion = ((SpecAttr) requestedSpec.GetAttr()).version;


            Environment env = getEnvironment(testCase, xpc);
            if (env == null || !env.usable) {
                notrun++;
                return;
            }

            env.xpathCompiler.BackwardsCompatible = false;
            env.processor.XmlVersion = (decimal) 1.0;

            env.xqueryCompiler.XQueryResolver = (moduleUri, baseUri, locationHints) => {
                XdmValue files =
                    xpc.Evaluate("./module[@uri='" + moduleUri + "']/@file/string()", testCase);
                if (files.Count == 0) {
                    return null;
                    //throw new Exception("Failed to find module entry for " + moduleUri);
                }

                TextResource[] sources = new TextResource[files.Count];
                int u = 0;
                foreach (XdmItem file in files) {
                    string filename = file.StringValue;
                    Uri abs = new Uri(testCase.BaseUri, filename);
                    sources[u++] = new TextResource(new WebClient().DownloadString(abs), abs);
                }

                return sources;
            };



            //test
            /*bool icuColCheck = Saxon.Hej.Version.platform.hasICUCollator ();
			bool icuNumCheck = Saxon.Hej.Version.platform.hasICUNumberer();
			Console.WriteLine ("ICUCol: " + (icuColCheck ? "true" : "false"));
			Console.WriteLine ("ICUNum: " + (icuNumCheck ? "true" : "false"));*/
            //end of test
            
            // List<XdmNode> dependencies = testCase.getParent().select(child("dependency")).asList();
            // dependencies.addAll(testCase.select(child("dependency")).asList());

            
            SysCollGen.IList<XdmNode> dependencies =
                new SysCollGen.List<XdmNode>(testCase.Select(Child("dependency")));
            SysCollGen.IEnumerable<XdmNode> allDeps = dependencies.Concat(testCase.Parent.Select(Child("dependency")));
            foreach (XdmNode dependency in allDeps) {
                string type = dependency.GetAttributeValue("type");
                if (type == null) {
                    // throw new IllegalStateException("dependency/@type is missing"); //TODO
                }

                string value = (dependency).GetAttributeValue("value");
                if (value == null) {
                    //throw new IllegalStateException("dependency/@value is missing"); //TODO
                }

                if (type.Equals("spec")) {
                    bool applicable = false;
                    if (!value.Contains(((SpecAttr) requestedSpec.GetAttr()).sname)) {
                        applicable = false;
                    }
                    else if (value.Contains(((SpecAttr) requestedSpec.GetAttr()).svname)) {
                        applicable = true;
                    }
                    else if ((((SpecAttr) requestedSpec.GetAttr()).svname.Equals("XQ30") ||
                              ((SpecAttr) requestedSpec.GetAttr()).svname.Equals("XQ31")||
                              ((SpecAttr) requestedSpec.GetAttr()).svname.Equals("XQ40")) &&
                             (value.Contains("XQ10+") || value.Contains("XQ30+") || value.Contains("XQ31+"))) {
                        applicable = true;
                    }
                    else if ((((SpecAttr) requestedSpec.GetAttr()).svname.Equals("XP30") ||
                              ((SpecAttr) requestedSpec.GetAttr()).svname.Equals("XP31")) &&
                             (value.Contains("XP20+") || value.Contains("XP30+"))) {
                        applicable = true;
                    }

                    if (!applicable) {
                        writeTestcaseElement(testCaseName, "n/a", "not" + ((SpecAttr) requestedSpec.GetAttr()).svname, requestedSpec);
                        notrun++;
                        return;
                    }
                }

                if (langVersion.StartsWith("3") || langVersion.StartsWith("4")) {
                    env.processor.Implementation.setConfigurationProperty(JFeatureKeys.ENVIRONMENT_VARIABLE_RESOLVER,
                        new EnvironmentVariableResolver());
                }

 
                if (!EnsureDependencySatisfied((XdmNode) dependency, env)) {
                    println("*** Dependency not satisfied: " +
                            ((XdmNode) dependency).GetAttributeValue(new QName("type")));
                    writeTestcaseElement(testCaseName, "n/a", "Dependency not satisfied", requestedSpec);
                    run = false;
                    notrun++;
                    return;
                }
            }

            if (isSlow(testCaseName)) {
                writeTestcaseElement(testCaseName, "tooBig", "Test is too slow", requestedSpec);
                run = false;
                notrun++;
                return;
            }

            XdmNode exceptionElement;

            exceptionsMap.TryGetValue(testCaseName, out exceptionElement);


            if (exceptionElement != null) {
                XdmItem config = xpath.EvaluateSingle("configuration", exceptionElement);

                string runAtt = exceptionElement.GetAttributeValue(new QName("run"));
                string reasonMsg = xpath.EvaluateSingle("reason", exceptionElement).ToString();
                string reportAtt = exceptionElement.GetAttributeValue(new QName("report"));

                if (config != null) {
                    XdmItem paramValue =
                        xpath.EvaluateSingle("param[@name='not-unfolded' and @value='yes']/@name", config);
                    if (unfolded && paramValue != null) {
                        writeTestcaseElement(testCaseName, "notRun", reasonMsg, requestedSpec);
                        notrun++;
                        return;
                    }
                }

                if ("false".Equals(runAtt)) {
                    writeTestcaseElement(testCaseName, reportAtt, reasonMsg, requestedSpec);
                    notrun++;
                    return;
                }

                alternativeResult = (XdmNode) xpc.EvaluateSingle("result", exceptionElement);
                optimization = (XdmNode) xpc.EvaluateSingle("optimization", exceptionElement);
            }

            if (run && (specOpt == Spec.NULL || specOpt == requestedSpec)) {

                XdmValue previousResult = null;
                TestOutcome outcome = new TestOutcome(this);
                foreach (XdmItem testQuery in testCase.Children("test")) {
                    string exp = null;
                    try {
                        exp = xpc.Evaluate(
                                "if (@file) then unparsed-text(resolve-uri(@file, base-uri(.))) else string(.)",
                                testQuery)
                            .ToString();
                    }
                    catch (SaxonApiException err) {
                        // Because we're using unparsed-text(), we can't read the query if it contains characters 
                        // that are illegal in XML. So substitute a dummy query.
                        println("*** Failed to read query: " + err.Message);
                        exp = "))) Failed to read query " + err.Message + "((("; // Force a syntax error
                    }



                    //noinspection ThrowableResultOfMethodCallIgnored
                    if (outcome.GetException() == null) {
                        if (hostLang.Equals("XP") || hostLang.Equals("XT")) {
                            XPathCompiler testXpc = env.xpathCompiler;
                            testXpc.XPathLanguageVersion = langVersion;
                            testXpc.DeclareNamespace("fn", "http://www.w3.org/2005/xpath-functions");
                            testXpc.DeclareNamespace("xs", "http://www.w3.org/2001/XMLSchema");
                            testXpc.DeclareNamespace("map", "http://www.w3.org/2005/xpath-functions/map");
                            testXpc.DeclareNamespace("array", "http://www.w3.org/2005/xpath-functions/array");
                            //testXpc.DeclareNamespace("math", NamespaceConstant.MATH);
                            //testXpc.DeclareNamespace("Dictionary", NamespaceConstant.Dictionary_FUNCTIONS);

                            ModuleResolver mr = new ModuleResolver(null, this);
                            mr.setTestCase(testCase);
                            env.processor.Implementation.setModuleURIResolver(mr);
                            
                            try {
                                XPathSelector selector = testXpc.Compile(exp).Load();

                                foreach (QName varName in env.params1.Keys) {
                                    selector.SetVariable(varName, env.params1[varName]);
                                }

                                if (env.contextItem != null) {
                                    selector.ContextItem = env.contextItem;
                                }

                                selector.XmlDocumentResolver = env.GetXmlDocumentResolver();

                                if (env.unparsedTextResolver != null) {
                                    selector.TextResolver = env.unparsedTextResolver;
                                }

                                XdmValue result = selector.Evaluate();
                                outcome.SetPrincipalResult(result);
                            }
                            catch (Exception err) {
                                println(err.Message);

                                outcome.SetException(err);
                            }
                        }
                        else if (hostLang.Equals("XQ")) {
                            XQueryCompiler testXqc = env.xqueryCompiler;
                            testXqc.XQueryLanguageVersion = langVersion;
                            testXqc.DeclareNamespace("fn", "http://www.w3.org/2005/xpath-functions");
                            testXqc.DeclareNamespace("xs", "http://www.w3.org/2001/XMLSchema");
                            //testXqc.DeclareNamespace("math", NamespaceConstant.MATH);
                            testXqc.DeclareNamespace("map", "http://www.w3.org/2005/xpath-functions/map");
                            testXqc.DeclareNamespace("array", "http://www.w3.org/2005/xpath-functions/array");
                            SysCollGen.List<Error> errorList = new SysCollGen.List<Error>();
                            testXqc.ErrorReporter = err => errorList.Add(err);
                            string decVars = env.paramDecimalDeclarations.ToString();
                            if (decVars.Length != 0) {
                                int x = (exp.IndexOf("(:%DECL%:)", StringComparison.Ordinal));
                                if (x < 0) {
                                    exp = decVars + exp;
                                }
                                else {
                                    exp = exp[..x] + decVars + exp[(x + 13)..];
                                }
                            }
                            
                            ModuleResolver mr = new ModuleResolver(null, this);
                            mr.setTestCase(testCase);
                            env.processor.Implementation.setModuleURIResolver(mr);

                            string vars = env.paramDeclarations.ToString();
                            if (vars.Length != 0) {
                                // ReSharper disable once StringLiteralTypo
                                int x = (exp.IndexOf("(:%VARDECL%:)", StringComparison.Ordinal));
                                if (x < 0) {
                                    exp = vars + exp;
                                }
                                else {
                                    exp = exp[..x] + vars + exp[(x + 13)..];
                                }
                            }

                            try {
                                XQueryExecutable q = testXqc.Compile(exp);
                                if (optimization != null) {
                                    // Test whether required optimizations have been performed
                                    XdmDestination expDest = new XdmDestination();
                                    JConfiguration config = driverProc.Implementation;
                                    //ExpressionPresenter presenter = new ExpressionPresenter(config, expDest.getReceiver(config));
                                    //q.getUnderlyingCompiledQuery().explain(presenter);
                                    //presenter.close();
                                    XdmNode explanation = expDest.XdmNode;
                                    XdmItem optResult =
                                        xpc.EvaluateSingle(optimization.GetAttributeValue(new QName("assert")),
                                            explanation);
                                    if (((XdmAtomicValue)optResult).AsBoolean()) {
                                        println("Optimization result OK");
                                    }
                                    else {
                                        println("Failed optimization test");
                                        Serializer ser = env.processor.NewSerializer();
                                        ser.OutputStream = System.Console.OpenStandardError();
                                        driverProc.WriteXdmValue(explanation, ser);
                                        writeTestcaseElement(testCaseName, "fail", "Failed optimization assertions",
                                            requestedSpec);
                                        recordFailure(testSetName, testCaseName);
                                        return;
                                    }
                                }

                                XQueryEvaluator selector = q.Load();
                                foreach (QName varName in env.params1.Keys) {
                                    selector.SetExternalVariable(varName, env.params1[varName]);
                                }

                                if (previousResult != null) {
                                    if (previousResult is XdmItem) {
                                        selector.ContextItem = (XdmItem)previousResult;
                                    }
                                    else {
                                        selector.ContextItem = null;
                                    }
                                }
                                else if (env.contextItem != null) {
                                    selector.ContextItem = env.contextItem;
                                }

                                selector.XmlDocumentResolver = env.GetXmlDocumentResolver();
                                if (env.unparsedTextResolver != null) {
                                    selector.TextResolver = env.unparsedTextResolver;
                                }

                                if (Environment.isTrue("update")(testQuery) && q.IsUpdateQuery) {
                                    XdmNode[] updatedDocs = selector.RunUpdate();
                                    XdmNode resultDoc = null;
                                    int numberOfUpdatedDocs = 0;
                                    foreach (XdmNode n in updatedDocs) {
                                        numberOfUpdatedDocs++;
                                        resultDoc = n;
                                    }

                                    if (resultDoc == null) {
                                        // Some test queries don't actually update anything, in which case
                                        // getUpdatedDocuments() returns nothing. The tests are written on
                                        // the assumption that we process the "main input" of the previous
                                        // step, which is either the context item or the value of the
                                        // variable $input-context. Very pragmatic code here...
                                        if (previousResult == null) {
                                            QName key = new QName("input-context");
                                            resultDoc = env.params1.ContainsKey(key) ? (XdmNode)env.params1[key] : null;
                                        }
                                        else {
                                            resultDoc = (XdmNode)previousResult;
                                        }

                                        if (resultDoc == null) {
                                            outcome.SetPrincipalResult(XdmEmptySequence.Instance);
                                        }
                                        else {
                                            outcome.SetPrincipalResult(resultDoc);
                                        }
                                    }

                                    if (numberOfUpdatedDocs == 1) {
                                        previousResult = resultDoc;
                                        outcome.SetPrincipalResult(resultDoc);
                                    }
                                }
                                else {

                                    if (needSerializedResult) {
                                        StringWriter sw = new StringWriter();
                                        Serializer serializer = env.processor.NewSerializer();
                                        serializer.OutputWriter = sw;
                                        selector.Run(serializer);
                                        outcome.SetPrincipalSerializedResult(sw.ToString());
                                    }

                                    if (needResultValue) {
                                        try {
                                            XdmValue result = selector.Evaluate();

                                            outcome.SetPrincipalResult(result);
                                        }
                                        catch (StackOverflowException err) {
                                            println("in TestSet " + testSetName);
                                            println(err.Message);
                                            outcome.SetException(err);
                                        }
                                    }
                                }
                            }
                            catch (StackOverflowException err) {
                                println("in TestSet " + testSetName);
                                println(err.Message);
                                outcome.SetException(err);
                            }
                            catch (SaxonApiException err) {
                                println("in TestSet " + testSetName);
                                println(err.Message);
                                outcome.SetException(err);
                            }
                            catch (Exception err) {
                                println("in TestSet " + testSetName);
                                println(err.ToString());
                                writeTestcaseElement(testCaseName, "fail", "Unexpected exception " + err.GetType() 
                                    + " " + err.Message,
                                    requestedSpec);
                                recordFailure(testSetName, testCaseName);
                                outcome.SetException(err);
                                //	outcome.SetErrorsReported(testXqc.ErrorList); //TODO
                            }
                        }
                        else {
                            writeTestcaseElement(testCaseName, "notRun", "No processor found", requestedSpec);
                            notrun++;
                            return;
                        }
                    }
                }

                bool testIsSchemaAware = env.xpathCompiler.SchemaAware || env.xqueryCompiler.SchemaAware;
                if (env.resetActions != null) {
                    foreach (var act in env.resetActions) {
                        act.Invoke(env);
                    }
                    env.resetActions.Clear();
                }

                XdmNode assertion;
                if (alternativeResult != null) {
                    assertion = (XdmNode) xpc.EvaluateSingle("*[1]", alternativeResult);
                }
                else {
                    assertion = (XdmNode) xpc.EvaluateSingle("result/*[1]", testCase);
                }

                if (assertion == null) {
                    println("*** No assertions found for test case " + testCaseName);
                    writeTestcaseElement(testCaseName, "disputed", "No assertions in test case", requestedSpec);
                    feedback.Feedback(successes, failures, total);
                    recordFailure(testSetName, testCaseName);
                    return;
                }

                XPathCompiler assertXpc = env.processor.NewXPathCompiler();
                assertXpc.XPathLanguageVersion = "3.1";
                assertXpc.SchemaAware = testIsSchemaAware;
                assertXpc.DeclareNamespace("fn", "http://www.w3.org/2005/xpath-functions");
                assertXpc.DeclareNamespace("xs", "http://www.w3.org/2001/XMLSchema");
                assertXpc.DeclareNamespace("math", "http://www.w3.org/2005/xpath-functions/math");
                assertXpc.DeclareNamespace("map", "http://www.w3.org/2005/xpath-functions/map");
                assertXpc.DeclareNamespace("array", "http://www.w3.org/2005/xpath-functions/array");
                assertXpc.DeclareNamespace("j", "http://www.w3.org/2005/xpath-functions");
                assertXpc.DeclareVariable(new QName("result"));
                assertXpc.BaseUri = assertion.BaseUri;

                bool b = outcome.TestAssertion(outcome, assertion, outcome.GetPrincipalResultDoc(), assertXpc, xpc,
                    debug);
                if (b) {
                    //println("OK");
                    writeTestcaseElement(testCaseName, "pass", null, requestedSpec);
                    feedback.Feedback(successes++, failures, total);
                }
                else {
                    if (outcome.IsException()) {
                        XdmItem expectedError = xpc.EvaluateSingle("result//error/@code", testCase);

                        if (expectedError == null) {
                            //                        if (debug) {
                            //                            outcome.getException().printStackTrace(System.out);
                            //                        }

                            writeTestcaseElement(testCaseName, "fail", "Expected success, got ", requestedSpec);
                            println("*** fail, result " + outcome.GetException() +
                                    " Expected success.");
                            feedback.Feedback(successes, failures, total);
                            recordFailure(testSetName, testCaseName);
                        }
                        else {
                            string exceptionMessage =
                                outcome.GetException() == null ? outcome.GetException().Message : "";
                            string errorCode = (outcome.GetErrorCode() == null
                                ? "unknown - " + exceptionMessage
                                : outcome.GetErrorCode().LocalName);
                            writeTestcaseElement(testCaseName, "wrongError",
                                "Expected error:" + expectedError.ToString() + ", got " + errorCode, requestedSpec);
                            println("*** fail, result " + errorCode +
                                    " Expected error:" + expectedError.ToString());
                            wrongErrorResults++;
                            feedback.Feedback(successes++, failures, total);
                        }
                    }
                    else {
                        writeTestcaseElement(testCaseName, "fail", "Wrong results, got " +
                                                                   truncate(outcome.Serialize(assertXpc.Processor,
                                                                       outcome.GetPrincipalResultDoc())), requestedSpec);
                        feedback.Feedback(successes, failures, total);
                        recordFailure(testSetName, testCaseName);
                        if (debug) {
                            try {
                                println("Result:");
                                driverProc.WriteXdmValue(outcome.GetPrincipalResult(), driverSerializer);
                                println("<=======");
                            }
                            catch (Exception err) {
                            }

                            //println(outcome.getResult());
                        }
                        else {
                            println("*** fail (use -debug to show actual result)");
                            //failures++;
                        }
                    }
                }
            }
        }

        private string truncate(string in1) {
            if (in1.Length > 80) {
                return in1.Substring(0, 80) + "...";
            }
            else {
                return in1;
            }
        }


        public void writeResultFilePreamble(Processor processor, XdmNode catalog) {
            resultsDoc.writeResultFilePreamble(processor, catalog);
        }

        public void writeResultFilePostamble() {
            resultsDoc.writeResultFilePostamble();
        }


        public void startTestSetElement(XdmNode testSetNode) {
            resultsDoc.startTestSetElement(testSetNode);
        }


        protected void writeTestSetEndElement() {
            resultsDoc.endElement();
        }

        private void writeTestcaseElement(string name, string result, string comment, Spec specOpt) {
            resultsDoc.writeTestcaseElement(name, result, comment);
        }

 
        public class EnvironmentVariableResolver : Saxon.Hej.lib.EnvironmentVariableResolver {
            public EnvironmentVariableResolver() {
            }

            public SysCollGen.ISet<string> getAvailableEnvironmentVariables() {
                SysCollGen.ISet<string> strings = new SysCollGen.HashSet<string>();
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

        

        /**
         * Collect together information about all the dependencies of tests that use a given environment
         * @param processor the Saxon processor
         * @param env the environment for which dependency information is to be gathered
         * @
         */
        private void buildDependencyDictionary(Processor processor, Environment env, XdmNode catalog) {
            XQueryCompiler xqCompiler = processor.NewXQueryCompiler();
            xqCompiler.XQueryLanguageVersion = "3.1";
            xqCompiler.BaseUri = new Uri("file://" + System.IO.Path.GetFullPath(testSuiteDir) + "/", UriKind.RelativeOrAbsolute);
            XQueryExecutable exec = xqCompiler.Compile(
                "        declare namespace fots = \"http://www.w3.org/2010/09/qt-fots-catalog\";\n" +
                "        let $testsets := //fots:test-set/@file/doc(resolve-uri(., exactly-one(base-uri(.))))\n" +
                "        for $dependencyTS in $testsets//fots:dependency\n" +
                "        let $name := $dependencyTS/@type\n" +
                "        let $value := $dependencyTS/@value\n" +
                "        group by $name, $value\n" +
                "        order by $name, $value\n" +
                "        return <dependency type='{$name}' value='{$value}' />");
            XQueryEvaluator selector = exec.Load();
            selector.ContextItem = catalog;
            XdmValue result = selector.Evaluate();
            
            foreach (XdmItem item in result) {
                XdmNode node = (XdmNode) item;
                string type = node.GetAttributeValue(new QName("type"));
                string value = node.GetAttributeValue(new QName("value"));
                addDependency(type, value, EnsureDependencySatisfied(node, env));
            }
        }


        protected class Dependency {
            public string dType;
            public bool satisfied;
        }


        public void addDependency(string depStr, string value, bool satisfied) {
            if (!dependencyDictionary.ContainsKey(value)) {
                Dependency dep = new Dependency();
                dep.dType = depStr;
                dep.satisfied = satisfied;
                dependencyDictionary.Add(value, dep);
            }
        }
    }
}

