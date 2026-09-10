using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J2N.Text;
using Saxon.Api;
using Saxon.Ejavax.xml.transform;
using static Saxon.Api.Steps;

namespace SaxonCS_NUnit {
    class TestXQueryEvaluator {
        [Test]
        public void TestXdmNodeResult() {
            try {
                Processor processor = new();
                DocumentBuilder documentBuilder = processor.NewDocumentBuilder();
                documentBuilder.BaseUri = new Uri("http://example.com");
                documentBuilder.LineNumbering = true;
                documentBuilder.WhitespacePolicy = WhitespacePolicy.PreserveAll;
                XQueryCompiler compiler = processor.NewXQueryCompiler();
                string query = "//listitem"; // BuildXqueryString();
                var source = File.OpenRead(ConfigTest.XMARK_DIR + "xmark10.xml");

                if (!string.IsNullOrEmpty(query)) {
                    XQueryExecutable executable = compiler.Compile(query);
                    XQueryEvaluator evaluator = executable.Load();
                    XdmNode xmlNode = documentBuilder.Build(source);
                    evaluator.ContextItem = xmlNode;
                    var evaluations = evaluator.Evaluate();
                    var evaluationsAsList = evaluations.GetList();
                    //List<XdmNode> evaluationsAsList = evaluations.GetList();//.Cast < XdmNode().ToList()>;
                    bool check = false;
                    foreach (var evaluation in evaluationsAsList) {
                        if (evaluation is XdmNode) {
                            check = true;
                        }
                        else {
                            check = false;
                            Assert.Fail("Item is not of Type XdmNode");
                        }
                        //runEvaluation((XdmNode)evaluation, document);
                    }

                    if (!check) {
                        Assert.Fail("Item is not of Type XdmNode");
                    }
                }
            }
            catch (Exception ex) {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public void TestRegularExpression() {
            try {
                Processor processor = new();
                DocumentBuilder documentBuilder = processor.NewDocumentBuilder();
                documentBuilder.BaseUri = new Uri("http://example.com");
                documentBuilder.LineNumbering = true;
                documentBuilder.WhitespacePolicy = WhitespacePolicy.PreserveAll;
                XQueryCompiler compiler = processor.NewXQueryCompiler();
                const string query = "matches('qUery', 'q u', 'xi;n')";


                if (!string.IsNullOrEmpty(query)) {
                    XQueryExecutable executable = compiler.Compile(query);
                    XQueryEvaluator evaluator = executable.Load();

                    XdmValue evaluations = evaluator.Evaluate();
                    XdmValue evaluation = evaluations.Simplify();

                    Assert.That(evaluation is XdmAtomicValue);
                    Assert.That(((XdmAtomicValue) evaluation).AsBoolean(), "Regex match result");
                }
            }
            catch (Exception ex) {
                Assert.Fail(ex.Message);
            }
        }


        [Test]
        public void TestMemoryRelease() {
            var tasks = new Task[10];
            for (int i = 0; i < 10; i++) {
                tasks[i] = Task.Run(() => {
                        Processor processor = new Processor();
                        DocumentBuilder documentBuilder = processor.NewDocumentBuilder();
                        documentBuilder.LineNumbering = true;
                        documentBuilder.WhitespacePolicy = WhitespacePolicy.PreserveAll;
                        XQueryCompiler compiler = processor.NewXQueryCompiler();
                        using FileStream source = File.OpenRead(ConfigTest.XMARK_DIR + "xmark10.xml");
                        using FileStream queryFile = File.OpenRead(ConfigTest.XMARK_DIR + "q10.xq");
                        XQueryExecutable executable = compiler.Compile(queryFile);
                        XQueryEvaluator evaluator = executable.Load();
                        documentBuilder.BaseUri = new Uri("http://example.com");
                        evaluator.ContextItem = documentBuilder.Build(source);
                        XdmValue evaluations = evaluator.Evaluate();
                    }
                );
            }
        }


        [Test]
        public void testReturnDocumentNode() {
            try {
                Processor p = new ();
                DocumentBuilder b = p.NewDocumentBuilder();
                XdmNode foo = b.Build(new StringReader("<foo/>"));
                XQueryCompiler c = p.NewXQueryCompiler();
                XQueryEvaluator eval = c.Compile("document{.}").Load();
                eval.ContextItem = (foo);

                // First run in push mode
                XdmDestination dest = new ();
                eval.Run(dest);
                XdmNode doc = dest.XdmNode;
                Assert.AreSame(doc.NodeKind, XdmNodeKind.Document, "Push result is document");

                // Now run in pull mode
                XdmNode doc2 = (XdmNode) eval.Evaluate();
                Assert.AreSame(doc2.NodeKind, XdmNodeKind.Document, "Pull result is document");
            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void testXQueryLineNumber() {
            try {
                Processor proc = new();
                XQueryCompiler comp = proc.NewXQueryCompiler();

                DocumentBuilder builder = proc.NewDocumentBuilder();
                builder.LineNumbering = (true);
                builder.WhitespacePolicy = (WhitespacePolicy.StripAll);
                XdmNode source = builder.Build(new Uri(ConfigTest.DataDirUri, "books.xml"));

                XQueryExecutable exp = comp.Compile("saxon:line-number((//WEIGHT)[1])");
                XQueryEvaluator eval = exp.Load();
                eval.ContextItem = (source);
                StringWriter sw = new StringWriter();
                Serializer ser = proc.NewSerializer(sw);
                ser.SetOutputProperty(Serializer.METHOD, "xml");
                ser.SetOutputProperty(Serializer.INDENT, "yes");
                ser.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");
                eval.Run(ser);
                //System.out.println(sw.ToString());
                Assert.AreEqual("15", sw.ToString(), "linenumber");
            }
            catch (SaxonApiException e) {
                //e.printStackTrace();
                Assert.Fail();
            }
        }

        [Test]
        public void testXdmDestinationMultipleNodes() {
            try {
                Processor p = new();
                XQueryCompiler c = p.NewXQueryCompiler();
                XQueryEvaluator eval = c.Compile("document{<a/>}, document{<b/>}, document{<c/>}").Load();

                XdmDestination dest = new();
                eval.Run(dest);
                XdmNode doc = dest.XdmNode;
                Assert.AreEqual(doc.NodeKind, XdmNodeKind.Document, "kind");
                Assert.AreEqual(3, doc.Select(Child()).Count(), "child-count");
            }
            catch (SaxonApiException e) {
                //e.printStackTrace();
                Assert.Fail();
            }
        }

        private static void separatorTest(string separator, string expected) {
            try {
                Processor p = new();
                XQueryCompiler c = p.NewXQueryCompiler();
                string q = "declare namespace output = \"http://www.w3.org/2010/xslt-xquery-serialization\";" +
                           "declare option output:method   \"xml\";" +
                           "declare option output:omit-xml-declaration   \"yes\";" +
                           "declare option output:item-separator '" + separator + "';" +
                           "<!--start-->, 1 to 3, <!--end-->";
                XQueryEvaluator eval = c.Compile(q).Load();

                XdmDestination dest = new XdmDestination();
                eval.Run(dest);
                XdmNode doc = dest.XdmNode;
                Assert.AreEqual(XdmNodeKind.Document, doc.NodeKind, "kind");
                Assert.AreEqual(3, doc.Select(Child()).Count(), "child-count");
                Assert.AreEqual(expected, doc.ToString(), "content");
            }
            catch (SaxonApiException e) {
                //e.printStackTrace();
                Assert.Fail();
            }
        }

        [Test]
        public void testXdmDestinationItemSeparator() {
            separatorTest("~", "<!--start-->~1~2~3~<!--end-->");
        }

        [Test]
        public void testXdmDestinationSpaceSeparator() {
            separatorTest("#absent", "<!--start-->1 2 3<!--end-->");
        }


        /**
     * Test writing a text node only to an XdmDestination
     */
        [Test]
        public void testXdmDestinationTextNodeOnly() {
            try {
                Processor processor = new ();
                XQueryCompiler xpc = processor.NewXQueryCompiler();
                XQueryEvaluator s = xpc.Compile("text{'abcd'}").Load();
                XdmDestination dest = new ();
                s.Run(dest);
                XdmNode doc = dest.XdmNode;
                Assert.AreEqual(XdmNodeKind.Document, doc.NodeKind, "kind");
                Assert.AreEqual(1, doc.Select(Child()).Count(), "child-count");
                Assert.AreEqual(XdmNodeKind.Text, doc.Select(Child()).First().NodeKind, "child-kind");
            }
            catch (SaxonApiException e) {
                //e.printStackTrace();
                Assert.Fail(e.Message);
            }
        }


        [Test]
        public void testRawDestinationMultipleDocumentNodes() {
            try {
                Processor p = new();
                XQueryCompiler c = p.NewXQueryCompiler();
                XQueryEvaluator eval = c.Compile("document{<a/>}, document{<b/>}, document{<c/>}").Load();

                RawDestination dest = new();
                eval.Run(dest);
                XdmValue result = dest.XdmValue;
                Assert.AreEqual(XdmNodeKind.Document, ((XdmNode) result.ItemAt(0)).NodeKind, "kind");
                Assert.AreEqual(3, result.Count, "count");
            }
            catch (SaxonApiException e) {
                //e.printStackTrace();
                Assert.Fail();
            }
        }

        [Test]
        public void testRawDestinationMultipleElementNodes() {
            try {
                Processor p = new();
                XQueryCompiler c = p.NewXQueryCompiler();
                XQueryEvaluator eval = c.Compile("<a x='1'><y/></a>, <b x='2'><y/></b>, <c x='3'><y/></c>").Load();

                RawDestination dest = new RawDestination();
                eval.Run(dest);
                XdmValue result = dest.XdmValue;
                Assert.AreEqual(XdmNodeKind.Element, ((XdmNode) result.ItemAt(0)).NodeKind, "kind");
                Assert.AreEqual("2", ((XdmNode) result.ItemAt(1)).GetAttributeValue("x"), "att");
                Assert.AreEqual("y", result.ItemAt(2).Select(Child("y")).First().NodeName.LocalName, "child");
                Assert.AreEqual(3, result.Count, "count");
            }
            catch (SaxonApiException e) {
                //e.printStackTrace();
                Assert.Fail();
            }
        }

        [Test]
        public void testRawDestinationMultipleAttributeNodes() {
            try {
                Processor p = new();
                XQueryCompiler c = p.NewXQueryCompiler();
                XQueryEvaluator eval = c.Compile("(1 to 3)!attribute{'a'}{string(.)}").Load();

                RawDestination dest = new();
                eval.Run(dest);
                XdmValue result = dest.XdmValue;
                Assert.AreEqual(XdmNodeKind.Attribute, ((XdmNode) result.ItemAt(0)).NodeKind, "kind");
                Assert.AreEqual("2", result.ItemAt(1).StringValue, "att");
                Assert.AreEqual("a", ((XdmNode) result.ItemAt(2)).NodeName.LocalName, "child");
                Assert.AreEqual(3, result.Count, "count");
            }
            catch (SaxonApiException e) {
                //e.printStackTrace();
                Assert.Fail();
            }
        }


        [Test]
        public void testRawDestinationAtomic() {
            try {
                Processor p = new();
                XQueryCompiler c = p.NewXQueryCompiler();
                XQueryEvaluator eval = c.Compile("1 to 10").Load();

                RawDestination dest = new ();
                eval.Run(dest);
                XdmValue result = dest.XdmValue;
                Assert.AreEqual(10, result.Count);
                Assert.AreEqual(1, ((XdmAtomicValue) result.ItemAt(0)).AsLong());
            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void testSerializerWithItemSeparator() {
            try {
                Processor p = new();
                XQueryCompiler c = p.NewXQueryCompiler();
                XQueryEvaluator eval = c.Compile(
                        "declare option Q{http://www.w3.org/2010/xslt-xquery-serialization}item-separator '~'; 1 to 10")
                    .Load();

                StringWriter writer = new ();
                Serializer sz = p.NewSerializer(writer);
                sz.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");
                eval.Run(sz);
                Assert.AreEqual("1~2~3~4~5~6~7~8~9~10", writer.ToString());
            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void testSerializerWithItemSeparator2() {
            try {
                Processor p = new ();
                XQueryCompiler c = p.NewXQueryCompiler();
                XQueryEvaluator eval = c.Compile(
                        "declare option Q{http://www.w3.org/2010/xslt-xquery-serialization}item-separator '~'; 1 to 10")
                    .Load();

                StringWriter writer = new StringWriter();
                Serializer sz = p.NewSerializer(writer);
                sz.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");
                eval.Run(sz);
                Assert.AreEqual("1~2~3~4~5~6~7~8~9~10", writer.ToString());
            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }
        }


        [Test]
        public void testSeparateCompilationFromFile() {
            try {
                Processor proc = new Processor(true);
                XQueryCompiler comp = proc.NewXQueryCompiler();
                comp.BaseUri = ConfigTest.DataDirUri;
                comp.CompileLibrary(new FileStream(ConfigTest.DATA_DIR + "module-one.xq", FileMode.Open));
                string q1 = "import module namespace m='http://www.example.com/module-one'; m:add(2,2)";
                XQueryEvaluator ev = comp.Compile(q1).Load();
                Assert.AreEqual("4", ev.Evaluate().ToString(), "2+2");
                string q2 = "import module namespace m='http://www.example.com/module-one'; m:add($m:ten,5)";
                ev = comp.Compile(q2).Load();
                Assert.AreEqual("15", ev.Evaluate().ToString(), "10+5");
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }


        /**
     * Show a direct call from the Java application to a function defined in the
     * Query. This is a very efficient way of invoking a query
     */
        [Test]
        public void testDirectFunction() {
            try {
                Processor processor = new();
                XQueryCompiler qc = processor.NewXQueryCompiler();
                XQueryExecutable exp1 = qc.Compile("declare namespace f='f.ns';" +
                                                   "declare function f:t1($v1 as xs:integer, $v2 as xs:untypedAtomic*) { " +
                                                   "   $v1 div $v2" +
                                                   "};" +
                                                   "10");
                XQueryEvaluator ev = exp1.Load();
                XdmValue v1 = new XdmAtomicValue(10);
                XdmValue v2 = new XdmAtomicValue("5", XdmAtomicType.UntypedAtomic);
                XdmValue result = ev.CallFunction(new QName("f.ns", "f:t1"), v1, v2);
                Assert.That(result is XdmAtomicValue, "isAtomic");
                Assert.AreEqual("2", result.ToString(), "equals2");
            }
            catch (SaxonApiException e) {
                //e.printStackTrace();
                Assert.Fail(e.Message);
            }
        }

        /**
     * Show a direct call from the Java application to a function defined in the
     * Query. This is a very efficient way of invoking a query
     */
        [Test]
        public void testDirectFunctionWithGlobalVar() {
            try {
                Processor processor = new Processor(true);
                XQueryCompiler qc = processor.NewXQueryCompiler();
                XQueryExecutable exp1 = qc.Compile("declare namespace f='f.ns';" +
                                                   "declare variable $f:value := 89;" +
                                                   "declare function f:t1($v1 as xs:integer, $v2 as xs:untypedAtomic*) { " +
                                                   "   $v1 div $v2 + $f:value" +
                                                   "};" +
                                                   "10");
                XQueryEvaluator ev = exp1.Load();
                XdmValue v1 = new XdmAtomicValue(10);
                XdmValue v2 = new XdmAtomicValue("5", XdmAtomicType.UntypedAtomic);
                XdmValue result = ev.CallFunction(new QName("f.ns", "f:t1"), v1, v2);
                Assert.That(result is XdmAtomicValue, "isAtomic");
                Assert.AreEqual("91", result.ToString(), "equals2");
            }
            catch (SaxonApiException e) {
                //e.printStackTrace();
                Assert.Fail(e.Message);
            }
        }


        /**
     * Show a direct call from the Java application to a function defined in the
     * Query, with mismatched arguments
     */
        [Test]
        public void testDirectFunctionArgConversion() {
            try {
                Processor processor = new();
                XQueryCompiler qc = processor.NewXQueryCompiler();
                XQueryExecutable exp1 = qc.Compile("declare namespace f='f.ns';" +
                                                   "declare function f:t1($v1 as xs:double, $v2 as xs:double) { " +
                                                   "   $v1 div $v2" +
                                                   "};" +
                                                   "10");
                XQueryEvaluator ev = exp1.Load();
                XdmValue v1 = new XdmAtomicValue(10);
                XdmValue v2 = new XdmAtomicValue("5e0", XdmAtomicType.UntypedAtomic);
                XdmValue result = ev.CallFunction(new QName("f.ns", "f:t1"), new XdmValue[] {v1, v2});
                Assert.That(result is XdmAtomicValue, "isAtomic");
                Assert.AreEqual("double", ((XdmAtomicValue) result).TypeName.LocalName);
                Assert.AreEqual("double", ((XdmAtomicValue) result).PrimitiveTypeName.LocalName);
                Assert.AreEqual("2", result.ToString(), "equals2");
            }
            catch (SaxonApiException e) {
                //e.printStackTrace();
                Assert.That(e.Message.Contains("does not match the required type"), "type-error");
            }
        }

        /**
     * Show a direct call from the Java application to a function defined in the
     * Query, with mismatched arguments
     */
        [Test]
        public void testDirectFunctionTypeError() {
            try {
                Processor processor = new Processor(true);
                XQueryCompiler qc = processor.NewXQueryCompiler();
                XQueryExecutable exp1 = qc.Compile("declare namespace f='f.ns';" +
                                                   "declare function f:t1($v1 as xs:integer, $v2 as xs:untypedAtomic*) { " +
                                                   "   $v1 div $v2" +
                                                   "};" +
                                                   "10");
                XQueryEvaluator ev = exp1.Load();
                XdmValue v1 = new XdmAtomicValue(10);
                XdmValue v2 = new XdmAtomicValue("banana");
                ev.CallFunction(new QName("f.ns", "f:t1"), v1, v2);
                Assert.Fail("Failed to throw error"); // expect failure
            }
            catch (SaxonApiException e) {
                //e.printStackTrace();
                Assert.That(e.Message.Contains("required item type of the second argument"), "type-error");
            }
        }

        /**
     * Show a direct call from the Java application to a function defined in the
     * Query. Use global variable and global parameter, do two calls that share the global variable computation
     */
        [Test]
        public void testDirectFunctionRepeatedCalls() {
            try {
                Processor processor = new();
                XQueryCompiler qc = processor.NewXQueryCompiler();
                XQueryExecutable exp1 = qc.Compile("declare namespace f='f.ns';" +
                                                   "declare variable $z := 1 + xs:integer(doc-available('file://" +
                                                   ConfigTest.DATA_DIR + "books.xml'));" +
                                                   "declare variable $p as xs:integer external;" +
                                                   "declare function f:t1($v1 as xs:integer) { " +
                                                   "   $v1 div $z + $p" +
                                                   "};" +
                                                   "10");
                XQueryEvaluator ev = exp1.Load();
                ev.SetExternalVariable(new QName("p"), new XdmAtomicValue(39));
                XdmValue v1 = new XdmAtomicValue(10);
                XdmValue result = ev.CallFunction(new QName("f.ns", "f:t1"), v1);
                Assert.That(result is XdmAtomicValue);
                Assert.AreEqual("44", result.ToString());
                v1 = new XdmAtomicValue(20);
                result = ev.CallFunction(new QName("f.ns", "f:t1"), v1);
                Assert.That(result is XdmAtomicValue);
                Assert.AreEqual("49", result.ToString());
            }
            catch (SaxonApiException e) {
                //e.printStackTrace();
                Assert.Fail();
            }
        }

        [Test]
        public void testDirectFunctionInLibraryModule() {
            try {
                Processor processor = new Processor(true);
                processor.SetProperty(Feature.OPTIMIZATION_LEVEL, "0");
                XQueryCompiler qc = processor.NewXQueryCompiler();
                qc.BaseUri = new Uri(Environment.CurrentDirectory);
                qc.CompileLibrary(
                    "module namespace lib = 'http://example.com/library';" +
                    "declare variable $lib:bang external := '!';" +
                    "declare function lib:foo($a) { lib:bar($a) };" +
                    "declare function lib:bar($b) { concat($b, $lib:bang) };"
                );

                XQueryExecutable exp = qc.Compile(
                    "import module namespace lib = 'http://example.com/library';()");


                XQueryEvaluator ev = exp.Load();
                XdmValue v1 = new XdmAtomicValue("Hello");
                QName foo = new QName("http://example.com/library", "foo");
                XdmValue result = ev.CallFunction(foo, v1);
                Assert.That(result is XdmAtomicValue);
                Assert.AreEqual("Hello!", result.ToString());
                v1 = new XdmAtomicValue("Goodbye");
                ev.SetExternalVariable(new QName("http://example.com/library", "bang"), new XdmAtomicValue("?"));
                result = ev.CallFunction(foo, v1);
                Assert.That(result is XdmAtomicValue);
                Assert.AreEqual("Goodbye?", result.ToString());
            }
            catch (SaxonApiException e) {
                //e.printStackTrace();
                Assert.Fail();
            }
        }

        [Test]
        public void testDirectFunctionInLibraryModule2() {
            try {
                Processor processor = new();
                XQueryCompiler qc = processor.NewXQueryCompiler();
                qc.BaseUri = new Uri(Environment.CurrentDirectory);
                qc.CompileLibrary(
                    "module namespace lib = 'http://example.com/library';" +
                    "declare variable $lib:bang := '!';" +
                    "declare function lib:foo() { lib:bar() };" +
                    "declare function lib:bar() { " +
                    "   let $a := parse-xml('<xml><ABC>azaz</ABC></xml>')  " +
                    "   return upper-case($a//xml/ABC) };"
                );

                XQueryExecutable exp = qc.Compile(
                    "import module namespace lib = 'http://example.com/library';()");


                XQueryEvaluator ev = exp.Load();

                QName foo = new QName("http://example.com/library", "foo");
                XdmValue result = ev.CallFunction(foo, Array.Empty<XdmValue>());
                Assert.That(result is XdmAtomicValue);
                Assert.AreEqual("AZAZ", ((XdmAtomicValue) result).StringValue);
            }
            catch (SaxonApiException e) {
                //e.printStackTrace();
                Assert.Fail();
            }
        }

        // [Test]
        // public void testTraceListener() {
        //     try {
        //         Processor proc = new ();
        //         proc.SetProperty(Feature<bool>.COMPILE_WITH_TRACING, true);
        //         XQueryCompiler comp = proc.NewXQueryCompiler();
        //         string qq = "declare variable $TestVariable := 7.5;\n" +
        //                     "<demo_using_math>\n" +
        //                     "    {$TestVariable + 1}\n" +
        //                     "</demo_using_math>";
        //         XQueryEvaluator eval = comp.Compile(qq).Load();
        //         eval.setTraceListener(new TestXslt30TransformerDebug.MyTraceListener());
        //         XdmValue result = eval.EvaluateSingle();
        //         Assert.That(result.ToString().Contains("8.5"));
        //     }
        //     catch (SaxonApiException err) {
        //         //err.printStackTrace();
        //         Assert.Fail(err.Message);
        //     }
        // }
        //
        // [Test]
        // public void testFlworTracing() {
        //     try {
        //         string doc = "<a xmlns='http://a/'><b>1</b><b>2</b><b>3</b><b>4</b></a>";
        //         string q = "declare namespace p='http://a/';\n" +
        //                    "for $i in //p:b \n" +
        //                    "where $i mod 2 = 0 \n" +
        //                    "order by -$i \n" +
        //                    "return $i";
        //         Processor p = new ();
        //         p.SetProperty(Feature<int>.OPTIMIZATION_LEVEL, 0);
        //         XQueryCompiler comp = p.NewXQueryCompiler();
        //         comp.setCompileWithTracing(true);
        //         XQueryExecutable exec = comp.Compile(q);
        //         XQueryEvaluator eval = exec.Load();
        //         eval.ContextItem = (p.NewDocumentBuilder().Build(new StringReader(doc));
        //         eval.setTraceListener(new TraceListener() {
        //             int i = 0;
        //             @Override
        //             public void setOutputDestination(Logger stream) {
        //         }
        //
        //         @Override
        //
        //         public void open(Controller controller) {
        //             // no action
        //         }
        //
        //         @Override
        //
        //         public void close() {
        //             // no action
        //         }
        //
        //         @Override
        //
        //         public void enter(Traceable instruction, Map<string, Object> properties, XPathContext context) {
        //             if (!(instruction instanceof TraceExpression)) {
        //                 Console.Error.WriteLine(i + ": @" + instruction.getLocation().getLineNumber() + " " +
        //                                         (instruction instanceof ClauseInfo
        //                     ? ((ClauseInfo) instruction).getClause().getClauseKey()
        //                     : instruction.getClass()));
        //                 switch (i++) {
        //                     case 0:
        //                         Assert.That(instruction instanceof XQueryExpression, "isXQueryExpression");
        //                         break;
        //                     case 1:
        //                     case 2:
        //                     case 4:
        //                     case 5:
        //                         Assert.That(instruction instanceof ClauseInfo, "isClauseInfo0");
        //                         Assert.AreEqual("http://a/",
        //                             ((ClauseInfo) instruction).getNamespaceResolver().getURIForPrefix("p", false),
        //                             "rightPrefix0");
        //                         Assert.That(((ClauseInfo) instruction).getClause() instanceof ForClause,
        //                             "isForClause0");
        //                         Assert.AreEqual(2, instruction.getLocation().getLineNumber(), "lineNumber0");
        //                         break;
        //                     case 3:
        //                     case 6:
        //                         Assert.That(instruction instanceof ClauseInfo, "isClauseInfo1");
        //                         Assert.AreEqual("http://a/",
        //                             ((ClauseInfo) instruction).getNamespaceResolver().getURIForPrefix("p", false),
        //                             "rightPrefix1");
        //                         Assert.That(((ClauseInfo) instruction).getClause() instanceof WhereClause,
        //                             "isWhereClause1");
        //                         Assert.AreEqual(3, instruction.getLocation().getLineNumber(), "lineNumber1");
        //                         break;
        //                     case 7:
        //                     case 8:
        //                         Assert.That(instruction instanceof ClauseInfo, "isClauseInfo4");
        //                         Assert.AreEqual("http://a/",
        //                             ((ClauseInfo) instruction).getNamespaceResolver().getURIForPrefix("p", false),
        //                             "rightPrefix4");
        //                         Assert.That(((ClauseInfo) instruction).getClause() instanceof OrderByClause,
        //                             "isOrderBy4");
        //                         Assert.AreEqual(4, instruction.getLocation().getLineNumber(), "lineNumber4");
        //                         break;
        //                     default:
        //                 }
        //             }
        //         }
        //
        //         @Override
        //
        //         public void leave(Traceable instruction) {
        //             //To change body of implemented methods use File | Settings | File Templates.
        //         }
        //
        //         @Override
        //
        //         public void startCurrentItem(Item currentItem) {
        //             //To change body of implemented methods use File | Settings | File Templates.
        //         }
        //
        //         @Override
        //
        //         public void endCurrentItem(Item currentItem) {
        //             //To change body of implemented methods use File | Settings | File Templates.
        //         }
        //
        //
        //         public void endRuleSearch(Object rule, SimpleMode mode) {
        //             //
        //         }
        //
        //
        //
        //         public void startRuleSearch() {
        //             //
        //         }
        //
        //         });
        //         eval.setDestination(new SAXDestination(new XMLFilterImpl()));
        //         eval.Run();
        //     }
        //     catch (SaxonApiException err) {
        //         //err.printStackTrace();
        //         Assert.Fail();
        //     }
        // }

        [Test]
        public void testStreaming() {
            try {
                Processor p = new ();
                XQueryCompiler c = p.NewXQueryCompiler();
                c.StreamingEnabled = (true);
                XQueryEvaluator eval = c.Compile("round(avg(//QUANTITY/data()), 2)").Load();
                StringWriter sw = new StringWriter();
                Serializer ser = p.NewSerializer(sw);
                ser.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");
                eval.RunStreamed(File.Open(ConfigTest.DATA_DIR + "books.xml", FileMode.Open), ConfigTest.DataDirUri,
                    ser);
                Assert.AreEqual("128.67", sw.ToString());
            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void testStreamingFailure() {
            try {
                Processor p = new ();
                XQueryCompiler c = p.NewXQueryCompiler();
                c.StreamingEnabled = (true);
                XQueryEvaluator eval = c.Compile("round(avg(descendant::ITEM/(QUANTITY*PRICE)), 2)").Load();
                StringWriter sw = new StringWriter();
                Serializer ser = p.NewSerializer(sw);
                ser.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");
                eval.RunStreamed(File.Open(ConfigTest.DATA_DIR + "books.xml", FileMode.Open), ConfigTest.DataDirUri,
                    ser);
                Assert.Fail("Unexpected success");
            }
            catch (SaxonApiException e) {
                //e.printStackTrace();
                Assert.AreEqual("XTSE3430", e.ErrorCode.LocalName);
            }
        }

        [Test]
        public void testXmlParseTryCatch() {
            // Bug 2896: try to prevent reporting parse errors to the standard error output if the error
            // then gets caught.
            try {
                string query = "xquery version \"3.0\";\n" +
                               "declare namespace output = \"http://www.w3.org/2010/xslt-xquery-serialization\";\n" +
                               "declare option output:omit-xml-declaration   \"true\";\n" +
                               "\n" +
                               "try {\n" +
                               "   let $xml-string := '&lt;para&gt;Paragraph&lt;/p&gt;'\n" +
                               "   return parse-xml($xml-string)\n" +
                               "}\n" +
                               "catch * {\n" +
                               "   <error code='{$err:code}' desc='{$err:description}'>\n" +
                               "    {for $o in $err:value return <o>{$o}</o>}</error>\n" +
                               "}";
                Processor p = new();
                XQueryCompiler c = p.NewXQueryCompiler();
                XQueryExecutable q = c.Compile(query);
                XdmValue result = q.Load().EvaluateSingle();
                Console.Error.WriteLine(result.ToString());
                Assert.That(result.ToString().Contains("code=\"err:FODC0006\""));
                Assert.That(result.ToString().Contains("does not match the end tag"));
            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void testContextItemExternalWithDefault() {
            try {
                Processor p = new();
                DocumentBuilder b = p.NewDocumentBuilder();
                XdmNode foo = b.Build(new StringReader("<foo/>")).Select(Child()).First();
                XQueryCompiler c = p.NewXQueryCompiler();

                XQueryExecutable exec = c.Compile("declare context item as node() external := <bar/>; .");

                // First run with supplied arg
                XQueryEvaluator eval = exec.Load();
                eval.ContextItem = (foo);
                XdmItem result = eval.EvaluateSingle();
                Assert.That(result is XdmNode);
                Assert.AreEqual("foo", ((XdmNode) result).NodeName.LocalName);

                // Now with no supplied arg
                eval = exec.Load();
                result = eval.EvaluateSingle();
                Assert.That(result is XdmNode);
                Assert.AreEqual("bar", ((XdmNode) result).NodeName.LocalName);

                // Now with arg of wrong type
                try {
                    eval = exec.Load();
                    eval.ContextItem = (new XdmAtomicValue(23));
                    eval.EvaluateSingle();
                    Assert.Fail();
                }
                catch (SaxonApiException e) {
                    Assert.AreEqual("XPTY0004", e.ErrorCode.LocalName);
                }
            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void testContextItemInternalWithDefault() {
            try {
                Processor p = new();
                DocumentBuilder b = p.NewDocumentBuilder();
                XdmNode foo = b.Build(new StringReader("<foo/>")).Select(Child()).First();
                XQueryCompiler c = p.NewXQueryCompiler();

                XQueryExecutable exec = c.Compile("declare context item as node() := <bar/>; .");

                // First run with supplied arg
                XQueryEvaluator eval;
                XdmItem result;
                try {
                    eval = exec.Load();
                    eval.ContextItem = (foo);
                    eval.EvaluateSingle();
                    Assert.Fail();
                }
                catch (SaxonApiException e) {
                    // expected failure
                    Console.Error.WriteLine(e.Message);
                }

                // Now with no supplied arg
                eval = exec.Load();
                result = eval.EvaluateSingle();
                Assert.That(result is XdmNode);
                Assert.AreEqual("bar", ((XdmNode) result).NodeName.LocalName);

                // Now with arg of wrong type
                try {
                    eval = exec.Load();
                    eval.ContextItem = (new XdmAtomicValue(23));
                    eval.EvaluateSingle();
                    Assert.Fail();
                }
                catch (SaxonApiException e) {
                    // expected failure
                    Console.Error.WriteLine(e.Message);
                }
            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void testContextItemExternalWithoutDefault() {
            try {
                Processor p = new();
                DocumentBuilder b = p.NewDocumentBuilder();
                XdmNode foo = b.Build(new StringReader("<foo/>")).Select(Child()).First();
                XQueryCompiler c = p.NewXQueryCompiler();

                XQueryExecutable exec = c.Compile("declare context item as node() external; .");

                // First run with supplied arg
                XQueryEvaluator eval = exec.Load();
                eval.ContextItem = (foo);
                XdmItem result = eval.EvaluateSingle();
                Assert.That(result is XdmNode);
                Assert.AreEqual("foo", ((XdmNode) result).NodeName.LocalName);

                // Now with no supplied arg
                try {
                    eval = exec.Load();
                    eval.EvaluateSingle();
                    Assert.Fail();
                }
                catch (SaxonApiException e) {
                    // expected failure
                    Assert.AreEqual("XPDY0002", e.ErrorCode.LocalName);
                    //Console.Error.WriteLine(e.Message);
                }

                // Now with arg of wrong type
                try {
                    eval = exec.Load();
                    eval.ContextItem = (new XdmAtomicValue(23));
                    eval.EvaluateSingle();
                    Assert.Fail();
                }
                catch (SaxonApiException e) {
                    Assert.AreEqual("XPTY0004", e.ErrorCode.LocalName);
                }
            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void testContextItemInternalWithoutDefault() {
            try {
                Processor p = new();
                XQueryCompiler c = p.NewXQueryCompiler();
                c.Compile("declare context item as node(); .");
                Assert.Fail();
            }
            catch (SaxonApiException e) {
                Assert.AreEqual("XPST0003", e.ErrorCode.LocalName);
            }
        }

        [Test]
        public void testMapItemType() {
            // bug 4490
            try {
                Processor p = new();
                XQueryCompiler c = p.NewXQueryCompiler();

                XQueryExecutable exec = c.Compile(
                    "declare namespace map = \"http://www.w3.org/2005/xpath-functions/map\";\n"
                    + "map:merge((\n"
                    + "    map:entry(attribute one {\"one\"}, 1),\n"
                    + "    map:entry(\"two\", 2)\n"
                    + "))");
                XdmValue result = exec.Load().Evaluate();
                Assert.That(result is XdmMap);
            }
            catch (SaxonApiException e) {
                Assert.Fail();
            }
        }

        [Test]
        public void testLocalURIResolver() {
            try {
                Processor p = new ();
                XQueryCompiler c = p.NewXQueryCompiler();
                XQueryEvaluator eval = c.Compile("doc('file:///a.xml')/z").Load();
                eval.XmlDocumentResolver = request => new TextResource("<z>17</z>", request.Uri);

                XdmItem result = eval.First();
                Assert.AreEqual("17", result.StringValue);
            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }
        }

        // [Test]
        // public void testGlobalURIResolver() {
        //     try {
        //         Processor p = new();
        //         p.XmlDocumentResolver = (uri, props) => new TextResource("<z>17</z>", uri);
        //         XQueryCompiler c = p.NewXQueryCompiler();
        //         XQueryEvaluator eval = c.Compile("doc('file:///a.xml')/z").Load();
        //         XdmItem result = eval.First();
        //         Assert.AreEqual("17", result.StringValue);
        //     }
        //     catch (SaxonApiException e) {
        //         Assert.Fail(e.Message);
        //     }
        // }
    }
}