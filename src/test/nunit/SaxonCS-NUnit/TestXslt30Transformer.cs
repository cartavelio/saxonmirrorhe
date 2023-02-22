using System;
using System.Collections.Generic;
using System.Collections;

using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Saxon.Api;
using NUnit.Framework;
using System.Xml;
using Saxon.Hej;
using Saxon.Hej.functions;
using Saxon.Hej.type;
using Saxon.Hej.value;
using DocumentBuilder = Saxon.Api.DocumentBuilder;
using Error = Saxon.Api.Error;
using Location = Saxon.Api.Location;
using Processor = Saxon.Api.Processor;
using RawDestination = Saxon.Api.RawDestination;
using SchemaManager = Saxon.Api.SchemaManager;
using SchemaValidator = Saxon.Api.SchemaValidator;
using Serializer = Saxon.Api.Serializer;
using XdmDestination = Saxon.Api.XdmDestination;
using XdmNodeKind = Saxon.Api.XdmNodeKind;
using Xslt30Transformer = Saxon.Api.Xslt30Transformer;
using XsltCompiler = Saxon.Api.XsltCompiler;
using XsltExecutable = Saxon.Api.XsltExecutable;
using XsltPackage = Saxon.Api.XsltPackage;
using XsltTransformer = Saxon.Api.XsltTransformer;


namespace SaxonCS_NUnit {
    public class TestXslt30Transformer {
        [Test]
        public void TestSimple() {
            // Create a Processor instance.
            try {
                Processor processor =  new();
                //processor.SetProperty("http://saxon.sf.net/feature/timing", "true");
                processor.SetProperty(FeatureKeys.TIMING, "true");
                // Create a transformer for the stylesheet.
                Xslt30Transformer transformer = processor.NewXsltCompiler().Compile(
                    new XmlTextReader(new Uri(ConfigTest.DataDirUri, "caffo/Transform2.xslt").ToString())).Load30();

                // Create a serializer
                XdmDestination serializer = new XdmDestination();
                TimeSpan start = DateTime.Now.TimeOfDay;

                int runs = 1;

                // Transform the source XML to System.out.
                for (int i = 0; i < runs; i++) {
                    // Load the source document
                    XdmNode input = processor.NewDocumentBuilder().Build(
                        new XmlTextReader(new Uri(ConfigTest.DataDirUri, "caffo/Input.xml").ToString()));
                    // Set the root node of the source document to be the initial context node
                    transformer.GlobalContextItem = input;
                    transformer.ApplyTemplates(input, serializer);
                }

                TimeSpan end = DateTime.Now.TimeOfDay;
                Console.WriteLine("Av Time: " + (((end - start).Milliseconds) / 10) + "ms");
                Assert.True(true);
            }
            catch (Exception e) {
                //Console.WriteLine (e.printStackTrace());
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestXmlParserError() {
            Processor processor = new ();

            XsltCompiler xsltCompiler = processor.NewXsltCompiler();

            List<Error> errorList = new();
            xsltCompiler.ErrorReporter = errorList.Add;
            string xslt =
                @"<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""3.0""</xsl:stylesheet>";

            XsltExecutable xsltExecutable;

            using (StringReader xsltReader = new (xslt)) {
                try {
                    xsltExecutable = xsltCompiler.Compile(xsltReader);
                }
                catch (Exception) {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("Stylesheet compilation failed with {0} error(s):", errorList.Count);
                    sb.AppendLine();
                    int errCount = 0;
                    foreach (var error in errorList) {
                        errCount++;
                        string message = error.Message;
                        Location loc = error.Location;
                        int line = loc == null ? -1 : loc.LineNumber;
                        int col = loc == null ? -1 : loc.ColumnNumber;
                        sb.AppendFormat("Error {0} at line {1}:{2} : {3}", errCount, line, col, message);
                        sb.AppendLine();
                    }

                    Console.WriteLine(sb.ToString());
                }
            }
        }


        [Test, Explicit]  // Expect failure when the Crayfish server isn't available!
        public void TestSimpleUNC() {
            // Create a Processor instance.
            try {
                Processor processor = new (true);
                //processor.SetProperty("http://saxon.sf.net/feature/timing", "true");
                processor.SetProperty(FeatureKeys.TIMING, "true");
                // Create a transformer for the stylesheet.
                Xslt30Transformer transformer = processor.NewXsltCompiler().Compile(
                    new Uri("//crayfish/team/UNC_tests/test.xsl")).Load30();

                // Create a serializer
                StringWriter sw = new ();
                Serializer serializer = processor.NewSerializer(sw);

                // Set the root node of the source document to be the initial context node
                transformer.CallTemplate(new QName("main"), serializer);

                Console.WriteLine(sw.ToString());
                Assert.True(true);
            }
            catch (Exception e) {
                //Console.WriteLine (e.printStackTrace());
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestLineNumber() {
            Processor _processor =  new();
            string sourcePath = Path.GetFullPath(ConfigTest.XMARK_DIR + "xmark1.xml");
            string stylesheetPath = Path.GetFullPath(ConfigTest.XMARK_DIR + "q12.xsl");
            string outputPath = Path.ChangeExtension(sourcePath, ".out.xml");

            DocumentBuilder builder = _processor.NewDocumentBuilder();
            XmlReader reader = XmlReader.Create(sourcePath);
            builder.BaseUri = new Uri("http://example.com");
            builder.LineNumbering = true;


            XdmNode node = builder.Build(reader);
            int prevLine = 0;
            int prevCol = 0;
            int line; // = ((net.sf.saxon.om.NodeInfo)node.Unwrap()).getLineNumber();
            int col; // = ((net.sf.saxon.om.NodeInfo)node.Unwrap()).getColumnNumber();
            //XmlNode _node = node.Unwrap()
            IEnumerator itr = node.EnumerateAxis(XdmAxis.Descendant);
            while (itr.MoveNext()) {
                node = (XdmNode) itr.Current;
                line = node.LineNumber;
                col = node.ColumnNumber;
                Assert.That(line >= prevLine && line < 100000);
                Assert.That(line > prevLine || col >= prevCol);
                prevLine = line;
                prevCol = col;
                //Console.Error.WriteLine("Line number : " + line + ", Position:" + col);
            }
        }

        [Test]
        public void TestPipeline1() {
            var p = new Processor();
            var c = p.NewXsltCompiler();
            var xsltStep1 = c.Compile(new Uri(ConfigTest.DATA_DIR + "echo_given_sourcefile.xsl"));
            var xsltStep2 = c.Compile(new Uri(ConfigTest.DATA_DIR + "print_elements.xsl"));

            var step1 = xsltStep1.Load();
            var step2 = xsltStep2.Load();

            var dest = p.NewSerializer();
            dest.OutputFile = ConfigTest.DATA_DIR + "OutputStep2.xml";

            step2.Destination = dest;

            var inputFilePath = ConfigTest.DATA_DIR + "books.xml";
            using (var input = System.IO.File.OpenRead(inputFilePath)) {
                step1.SetInputStream(input, new Uri(inputFilePath));

                step1.Run(step2);
            }
        }

        [Test]
        public void TestSingleton() {
            try {
                Processor proc = new Processor();
                var compiler = proc.NewXPathCompiler();
                StringReader source = new StringReader("<doc><e>text</e></doc>");

                DocumentBuilder builder = proc.NewDocumentBuilder();
                builder.BaseUri = new Uri("http://www.example.com");
                XdmNode node = builder.Build(source);
                XdmValue value = compiler.Evaluate("//e[1]", node);
                IList<XdmItem> list = value.GetList();
                Assert.True(list.Count == 1, "List has one item");
                IEnumerator enumer = list.GetEnumerator();
                enumer.MoveNext();
                Assert.True(enumer.Current is XdmValue, "Item in List is an XdmValue");
            }
            catch (Exception ex) {
                Assert.Fail(ex.Message);
            }
        }
        /*
                [Test]
                public void TestExportFrom98()
                {
                    try
                    {
                        var p = new Processor(true);
                        var c = p.NewXsltCompiler();
                        var testXSLTP = c.Compile(new Uri(ConfigTest.NUNIT_DATA_DIR + "booksWithMain98.sef"));


                        var exec = testXSLTP.Load();
                        exec.InitialTemplate = new QName("main");

                        var dest = p.NewSerializer();
                        dest.SetOutputFile(ConfigTest.DATA_DIR + "OutputStep2.xml");
                        exec.Run(dest);
                        Assert.True(true);
                    }
                    catch (javax.xml.transform.TransformerException e)
                    {
                        Assert.Fail(e.getMessage());
                    }

                } */

        [Test]
        public void TestRelocatableExport() {
            try {
                Processor p = new Processor(true);
                XsltCompiler c = p.NewXsltCompiler();
                XsltExecutable executable = c.Compile(new Uri(ConfigTest.DATA_DIR + "booksWithMain10.sef"));

                Xslt30Transformer exec = executable.Load30();


                Serializer dest = p.NewSerializer();
                dest.SetOutputProperty(Serializer.INDENT, "yes");
                dest.OutputFile = ConfigTest.DATA_DIR + "/sandpit/OutputStep2.xml";
                exec.CallTemplate(new QName("main"), dest);
                Assert.True(true);
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }


        /**
         * Test for the case where the context item is not the root
         */
        [Test]
        public void TestContextNotRoot() {
            try {
                Processor proc =  new();
                StringReader source = new StringReader("<doc><e>text</e></doc>");

                DocumentBuilder builder = proc.NewDocumentBuilder();
                builder.BaseUri = new Uri("http://www.example.com");
                XdmNode node = builder.Build(source);
                XsltCompiler xqcomp = proc.NewXsltCompiler();
                StringReader style = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:variable name='x' select='.'/>" +
                    "<xsl:template match='/'>errorA</xsl:template>" +
                    "<xsl:template match='e'>[<xsl:value-of select='name($x)'/>]</xsl:template>" +
                    "</xsl:stylesheet>");

                Xslt30Transformer trans = xqcomp.Compile(style).Load30();
                StringWriter sw = new StringWriter();

                Serializer serializer = proc.NewSerializer(sw);

                IEnumerator<XdmNode> iter = node.EnumerateAxis(XdmAxis.Descendant, new QName("e"));
                bool hasNext = iter.MoveNext();
                if (hasNext) {
                    XdmNode e = (XdmNode) iter.Current;
                    trans.GlobalContextItem = node;
                    trans.ApplyTemplates(e, serializer);
                }


                String result = sw.ToString();
                Assert.False(result.Contains("errorA"), "matched root");
                Assert.True(result.Contains("[]"), "matched e");
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }


        /// <summary>
        /// Bug issue 4683
        /// </summary>
        [Test]
        public void TestChainedDescendantAxes() {
            try {
                Processor proc =  new();
                StringReader source = new StringReader(
                    "<?xml version='1.0' encoding='utf-8' ?><root><item/><item><foo id='foo1'/></item>" +
                    "<item></item><item><foo id='foo2'/></item></root>");
                XmlTextReader xmlReader = new XmlTextReader(source);
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlReader);

                DocumentBuilder builder = proc.NewDocumentBuilder();
                // builder.BaseUri = new Uri("http://www.example.com");
                XdmNode node = builder.Wrap(doc);
                XsltCompiler xqcomp = proc.NewXsltCompiler();
                StringReader style = new StringReader(
                    "<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                    " xmlns:xs='http://www.w3.org/2001/XMLSchema' exclude-result-prefixes='#all'><xsl:param name='DEBUG' static='yes' as='xs:boolean' select='true()'/>" +
                    "<xsl:output method='text' item-separator='&#10;'/><xsl:template match='/'><xsl:for-each select='//item'>" +
                    "<xsl:assert test='not(*) eq not(descendant::foo)' use-when='$DEBUG'>No child elements but descendants</xsl:assert>" +
                    "<xsl:value-of select='position(), not(*), descendant::foo/@id'/></xsl:for-each></xsl:template></xsl:transform>");

                Xslt30Transformer trans = xqcomp.Compile(style).Load30();
                StringWriter sw = new StringWriter();

                Serializer serializer = proc.NewSerializer(sw);

                trans.GlobalContextItem = node;
                trans.ApplyTemplates(node, serializer);


                string result = sw.ToString();
                Assert.AreEqual("1 true\n2 false foo1\n3 true\n4 false foo2", result);
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }


        [Test]
        public void TestResultDocHandlerFunction() {
            Processor processor = new Processor(true);

            XsltCompiler xsltCompiler = processor.NewXsltCompiler();
            xsltCompiler.BaseUri = new Uri("file:///dummy/");
            string sr = "<?xml version='1.0' encoding='UTF-8'?>" +
                        "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                        "   xmlns:xs='http://www.w3.org/2001/XMLSchema' exclude-result-prefixes='#all' version='3.0'> " +
                        "<xsl:output method='html' indent='yes' html-version='5'/>" +
                        "<xsl:template match='/' name='xsl:initial-template'>" +
                        "   <html><head>" +
                        " <title>Test</title></head>" +
                        "<body> " +
                        "   <h1>Test</h1>" +
                        "</body>" +
                        "</html>" +
                        "<xsl:result-document href='result-1.json' method='json' indent='yes'>" +
                        "   <xsl:sequence select=\"map { 'name' : 'foo', 'values' : [ 'a', 'b', 'c' ]}\"/>" +
                        "</xsl:result-document>" +
                        " </xsl:template></xsl:stylesheet>";

            try {
                Processor proc =  new();
                proc.SetProperty(FeatureKeys.TIMING, "true");
                XsltTransformer transformer =
                    proc.NewXsltCompiler().Compile(new MemoryStream(Encoding.ASCII.GetBytes(sr))).Load();

                // First run without a result document handler
                transformer.InitialTemplate = new QName("http://www.w3.org/1999/XSL/Transform", "initial-template");
                transformer.BaseOutputUri = ConfigTest.SandpitDirUri;
                transformer.Run(proc.NewSerializer(Console.Out));

                Console.WriteLine("Done phase 1");

                // Now run with a result document handler
                List<Serializer> serializers = new();

                transformer.BaseOutputUri = ConfigTest.SandpitDirUri;

                transformer.ResultDocumentHandler = (href, baseUri) => {
                    Serializer ser = proc.NewSerializer();
                    ser.BaseUri = baseUri;
                    ser.OutputFile = ConfigTest.SANDPIT_DIR + href;
                    serializers.Add(ser);
                    return ser;
                };

                transformer.Run(proc.NewSerializer(Console.Out));

                Assert.AreEqual(1, serializers.Count);
                Assert.AreEqual(new Uri(ConfigTest.SandpitDirUri, "result-1.json"), 
                    serializers[0].BaseUri);
                string downloadString = new WebClient().DownloadString(new Uri(ConfigTest.SandpitDirUri, "result-1.json"));
                Assert.That(downloadString.Contains("\"name\": \"foo\""));
                
            }
            catch (Exception ex) {
                Console.WriteLine(ex.StackTrace);
                Assert.Fail(ex.Message);
            }
        }
        
        [Test]
        public void TestDefaultResultDocHandler() {
            // Bug 5552: test that secondary result documents are closed
            Processor processor = new Processor(true);

            XsltCompiler xsltCompiler = processor.NewXsltCompiler();
            xsltCompiler.BaseUri = new Uri("file:///dummy/");
            string sr = "<?xml version='1.0' encoding='UTF-8'?>" +
                        "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                        "   xmlns:xs='http://www.w3.org/2001/XMLSchema' exclude-result-prefixes='#all' version='3.0'> " +
                        "<xsl:output method='html' indent='yes' html-version='5'/>" +
                        "<xsl:template match='/' name='xsl:initial-template'>" +
                        "   <html><head>" +
                        " <title>Test</title></head>" +
                        "<body> " +
                        "   <h1>Test</h1>" +
                        "</body>" +
                        "</html>" +
                        "<xsl:result-document href='result-1.txt' method='text'>" +
                        "   <xsl:text>This is the output</xsl:text>" +
                        "</xsl:result-document>" +
                        " </xsl:template></xsl:stylesheet>";

            try {
                Processor proc =  new();
                proc.SetProperty(FeatureKeys.TIMING, "true");
                XsltTransformer transformer =
                    proc.NewXsltCompiler().Compile(new MemoryStream(Encoding.ASCII.GetBytes(sr))).Load();

                // Run without a result document handler
                transformer.InitialTemplate = new QName("http://www.w3.org/1999/XSL/Transform", "initial-template");
                transformer.BaseOutputUri = ConfigTest.SandpitDirUri;
                transformer.Run(proc.NewSerializer(Console.Out));
                
                string downloadString = new WebClient().DownloadString(new Uri(ConfigTest.SandpitDirUri, "result-1.txt"));
                Assert.That(downloadString.Contains("This is the output"));
                
            }
            catch (Exception ex) {
                //e.printStackTrace();
                Console.WriteLine(ex.StackTrace);
                Assert.Fail(ex.Message);
            }
        }

        private class MonitoredStream : MemoryStream {
            private bool closed = false;
            public override void Close() {
                base.Close();
                closed = true;
            } 
        }

        [Test]
        public void TestParseFragment() {
            try {
                Processor proc =  new();
                StringReader source = new StringReader("<?xml version='1.0' encoding='UTF-8'?>" +
                                                       "<root><bob>&lt;hello&gt;Burt&lt;/hello&gt;</bob></root>");

                DocumentBuilder builder = proc.NewDocumentBuilder();
                builder.BaseUri = new Uri("http://www.example.com");
                XdmNode node = builder.Build(source);
                XsltCompiler xqcomp = proc.NewXsltCompiler();
                StringReader style = new StringReader(
                    "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:fn='http://www.w3.org/2005/xpath-functions'>" +
                    "<xsl:output method='xml' version='1.0' encoding='UTF-8' indent='yes' /><xsl:template match='bob'><greeting>" +
                    "<xsl:copy-of select='fn:parse-xml-fragment(\"&lt;hello&gt;Ted&lt;/hello&gt;\")'/>" +
                    "</greeting></xsl:template></xsl:stylesheet>");

                Xslt30Transformer trans = xqcomp.Compile(style).Load30();
                StringWriter sw = new StringWriter();

                Serializer serializer = proc.NewSerializer(sw);


                string result = sw.ToString();
                Console.WriteLine(result);
                Assert.True(result.Equals(""), "Output received");
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }


        public class TestUriResolver : XmlUrlResolver {
            XdmNode style1;

            public TestUriResolver(XdmNode style) {
                this.style1 = style;
            }

            public override Uri ResolveUri(Uri base1, string href) {
                if (href.StartsWith("http:")) {
                    return style1.BaseUri;
                }
                else {
                    return null;
                }
            }
        }

        [Test]
        public void TestXmlResolver() {
            try {
                Processor proc =  new();
                string source = "<doc><e>text</e></doc>";

                DocumentBuilder builder = proc.NewDocumentBuilder();
                builder.LineNumbering = true;
                builder.BaseUri = new Uri("http://importer/");
                XdmNode node = builder.Build(new StringReader(source));
                XsltCompiler xqcomp = proc.NewXsltCompiler();
                StringReader sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>\n" +
                    "<xsl:template match='e' as='node()'>\n" +
                    "<xsl:param name='x' select='0'/>\n" +
                    "<xsl:param name='p' as='node()*' select='()'/>\n" +
                    "<e1/>" +
                    "<xsl:sequence select='$p'/>\n" +
                    "</xsl:template>\n" +
                    "</xsl:stylesheet>");

                xqcomp.StylesheetModuleResolver = (request) => new TextResource(sr, request.Uri);
                XsltExecutable exec = xqcomp.Compile(new Uri("http://random-stylesheet.org/"));
                Xslt30Transformer trans = exec.Load30();
                RawDestination dest = new RawDestination();
                trans.ApplyTemplates(node, dest);
                Assert.True(dest.XdmValue[0] is XdmNode);
                Assert.True(((XdmNode)dest.XdmValue[0]).NodeName.LocalName.Equals("e1"));
            }
            catch (Exception exc) {
                Assert.Fail(exc.Message);
            }
        }


        [Test]
        public void TestSqrt() {
            string p =
                @"<xsl:package name='http://www.ctk.cz/math' " +
                @"  xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' version='3.0' " +
                @"  xmlns:math='http://www.ctk.cz/math' " +
                @"  xmlns:demo='http://example.math.co.uk/demo'>" +
                @"  <xsl:function name='math:sqrt' as='xs:double?' visibility='final'>" +
                @"     <xsl:param name='num' as='xs:double?'/>" +
                @"     <xsl:sequence select='demo:sqrt($num)'/>" +
                @"  </xsl:function>" +
                @"</xsl:package>";


            String s = @"<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                       @" xmlns:math='http://www.ctk.cz/math' " +
                       @" exclude-result-prefixes='math'> " +
                       @" <xsl:use-package name='http://www.ctk.cz/math'/>" +
                       @" <xsl:template name='xsl:initial-template' > " +
                       @" <out " +
                       @" sqrt2='{math:sqrt(2.0e0)}' " +
                       @" sqrtEmpty='{math:sqrt(())}'> " +
                       @" </out> " +
                       @" </xsl:template></xsl:transform>";

            try {
                Processor processor = new Processor(true);
                processor.RegisterExtensionFunction(new Sqrt());
                processor.SetProperty("http://saxon.sf.net/feature/optimizationLevel", "-f");
                XsltCompiler xsltCompiler = processor.NewXsltCompiler();
                xsltCompiler.XsltLanguageVersion = "3.0";
                var ms = new MemoryStream(Encoding.UTF8.GetBytes(p));
                XsltPackage pp = xsltCompiler.CompilePackage(ms);
                xsltCompiler.ImportPackage(pp);
                XsltExecutable exec = xsltCompiler.Compile(new StringReader(s));
                Xslt30Transformer transf = exec.Load30();
                XdmDestination dest = new XdmDestination();
                transf.CallTemplate(null, dest);
                Console.WriteLine(dest.XdmNode);
                Assert.True(true, "ok");
            }
            catch (Exception exc) {
                Console.WriteLine(exc.Message + "\n" + exc.StackTrace);
                Assert.Fail(exc.Message);
            }
        }

        public class Sqrt : ExtensionFunctionDefinition {
            public override QName FunctionName => new QName("http://example.math.co.uk/demo", "sqrt");

            public override int MinimumNumberOfArguments => 1;

            public override int MaximumNumberOfArguments => 1;

            public override XdmSequenceType[] ArgumentTypes =>
                new XdmSequenceType[] {
                    new XdmSequenceType(XdmAtomicType.BuiltInAtomicType(QName.XS_DOUBLE), '?')
                };

            public override XdmSequenceType ResultType(XdmSequenceType[] ArgumentTypes) {
                return new XdmSequenceType(XdmAtomicType.BuiltInAtomicType(QName.XS_DOUBLE), '?');
            }

            public override bool TrustResultType => true;


            public override ExtensionFunctionCall MakeFunctionCall() {
                return new SqrtCall();
            }
        }

        internal class SqrtCall : ExtensionFunctionCall {
            public override XdmValue Call(XdmValue[] arguments, DynamicContext context) {
                bool exists = arguments[0].Any();
                if (exists) {
                    XdmAtomicValue arg = (XdmAtomicValue) arguments[0][0];
                    double val = (double) arg.Value;
                    double sqrt = System.Math.Sqrt(val);
                    XdmAtomicValue result = new(sqrt);
                    return result;
                }
                else {
                    return XdmEmptySequence.Instance;
                }
            }
        };


        /**
         * Test location information for run-time errors when stylesheet built as an XdmNode
         */
        [Test]
        public void TestErrorLocationFromXdm() {
            Processor proc =  new();
            try {
                // Norm Walsh reported getting line numbers > 2^20. Can't reproduce.


                StringReader source = new StringReader("<doc><e>text</e></doc>");

                DocumentBuilder builder = proc.NewDocumentBuilder();
                builder.LineNumbering = true;
                builder.BaseUri = new Uri("http://importer/");
                XdmNode node = builder.Build(source);
                XsltCompiler xqcomp = proc.NewXsltCompiler();

                StringReader style2Sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>\n" +
                    "<xsl:import href='file:///" + ConfigTest.DATA_DIR + "identity.xsl'/>\n" +
                    "<xsl:import href='http://imported'/>\n" +
                    "<xsl:param name='x' select='0'/>\n" +
                    "<xsl:template match='/'>\n" +
                    "<xsl:apply-templates select='*'>\n" +
                    "<xsl:with-param name='p' select='()'/>\n" +
                    "</xsl:apply-templates>\n" +
                    "</xsl:template>\n" +
                    "</xsl:stylesheet>");

                XdmNode style2 = builder.Build(style2Sr);

                //XdmNode style2 = builder.Build(new StreamSource(sr, "http://importer/"));

                StringReader sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>\n" +
                    "<xsl:template match='e' as='node()'>\n" +
                    "<xsl:param name='x' select='0'/>\n" +
                    "<xsl:param name='p' as='node()*' select='()'/>\n" +
                    //"<e1/>" +
                    //"<e2/>" +
                    "<xsl:sequence select='$p'/>\n" +
                    "</xsl:template>\n" +
                    "</xsl:stylesheet>");
                builder.BaseUri = new Uri("http://imported/");
                XdmNode style1 = builder.Build(sr);

                //xqcomp.XmlResolver = new TestUriResolver(style1);

                Xslt30Transformer trans = xqcomp.Compile(style2.BaseUri).Load30();

                Serializer serializer = proc.NewSerializer();
                MemoryStream os = new MemoryStream();
                serializer.OutputStream = os;
                trans.ApplyTemplates(node, serializer);

                Assert.Fail("Transformation should fail");
            }
            catch (Exception e) {
                Assert.True(true, e.Message);
                //Assert.True(((net.sf.saxon.trans.XPathException) e.getCause()).getLocator().getLineNumber() == 5, "line=5");
            }
        }


        /**
         * Related to bug  issue 4253 
         */
        [Test]
        public void TestParseXmlFragment() {
            Processor processor = new Processor(true);

            string xslt =
                @"<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                @" <xsl:template name='xsl:initial-template' > " +
                @"  <xsl:variable name='doc'>to je test</xsl:variable>" +
                @"  <xsl:sequence select='parse-xml-fragment($doc)'/>" +
                @" </xsl:template>" +
                @"</xsl:transform>";

            try {
                XsltCompiler xsltCompiler = processor.NewXsltCompiler();
                xsltCompiler.XsltLanguageVersion = "3.0";
                XsltExecutable exec = xsltCompiler.Compile(new StringReader(xslt));
                Xslt30Transformer transf = exec.Load30();
                Serializer ser = processor.NewSerializer();
                StringWriter sw = new StringWriter();
                ser.OutputWriter = sw;
                transf.CallTemplate(null, ser);
                Assert.True(sw.ToString() != null);
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }


        /**
     * Test created ErrorListener picks up errors in imported stylesheet
     */
        [Test]
        public void TestErrorInImportedStylesheet() {
   
            List<Error> errors = new ();

            try {
                Processor proc = new Processor(true);


                XsltCompiler xqcomp = proc.NewXsltCompiler();
                xqcomp.ErrorReporter =  errors.Add;

                Xslt30Transformer trans = xqcomp.Compile(new XmlTextReader(((ConfigTest.NUNIT_DATA_DIR + "main.xsl"))))
                    .Load30();

                Serializer serializer = proc.NewSerializer();
                MemoryStream os = new MemoryStream();
                serializer.OutputStream = os;

                trans.CallTemplate(new QName("main"), serializer);

                Assert.Fail("Transformation should fail");
            }
            catch (SaxonApiException e) {
                Assert.True(errors.Count > 0, "errors");
                //e.printStackTrace();
            }
        }


        /**
         * Test for the case where the context item is not the root, when starting at a named template
         */
        [Test]
        public void TestContextNotRootNamedTemplate() {
            try {
                Processor proc =  new();
                StringReader source = new StringReader("<doc><e>text</e></doc>");

                DocumentBuilder builder = proc.NewDocumentBuilder();
                builder.BaseUri = new Uri("http://doc");
                XdmNode node = builder.Build(source);
                XsltCompiler xtComp = proc.NewXsltCompiler();
                StringReader style = new(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:variable name='x' select='.'/>" +
                    "<xsl:template match='/'>errorA</xsl:template>" +
                    "<xsl:template name='main'>[<xsl:value-of select='name($x)'/>]</xsl:template>" +
                    "</xsl:stylesheet>");

                Xslt30Transformer trans = xtComp.Compile(style).Load30();
                StringWriter os = new();
                Serializer serializer = proc.NewSerializer(os);


                trans.GlobalContextItem = node;
                trans.CallTemplate(new QName("main"), serializer);

                String result = os.ToString();
                Assert.False(result.Contains("errorA"), "matched root");
                Assert.True(result.Contains("[]"), "matched e");
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestResultDocument() {
            try {
                Processor proc =  new();
                StringReader source = new StringReader("<doc><e>text</e></doc>");

                DocumentBuilder builder = proc.NewDocumentBuilder();
                builder.BaseUri = new Uri("http://doc");
                XdmNode node = builder.Build(source);
                XsltCompiler xqcomp = proc.NewXsltCompiler();
                StringReader style = new StringReader(
                    "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template name='main'><xsl:result-document href='a/b/c/d.xml'>" +
                    "<test> output </test></xsl:result-document>" +
                    "</xsl:template>" +
                    "</xsl:stylesheet>");

                Xslt30Transformer trans = xqcomp.Compile(style).Load30();
                StringWriter os = new StringWriter();
                Serializer serializer = proc.NewSerializer(os);
                serializer.BaseUri = ConfigTest.SandpitDirUri;

                trans.GlobalContextItem = node;
                trans.CallTemplate(new QName("main"), serializer);
                Assert.True(File.Exists(new Uri(ConfigTest.SandpitDirUri, "a/b/c/d.xml").AbsolutePath));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }


        [Test]
        public void TestUseAssociated() {
            try {
                Processor processor =  new();

                XsltCompiler compiler = processor.NewXsltCompiler();

                String d = ConfigTest.DATA_DIR + "trax/";

                String foo_xml = d + "xml/foo.xml";
                XdmNode sourceNode = processor.NewDocumentBuilder().Build(new Uri(foo_xml));
                XsltExecutable executable = compiler.CompileAssociatedStylesheet(sourceNode);


                Xslt30Transformer transformer = executable.Load30();

                Serializer serializer = processor.NewSerializer();
                serializer.OutputWriter = Console.Out;
                transformer.ApplyTemplates(new FileStream(foo_xml, FileMode.Open), serializer);
            }
            catch (Exception ex) {
                Assert.Fail(ex.Message);
            }
        }

        /**
     * Build a document from a non-root element of a DOMSource
     */
        [Test]
        public void TestNonRootDomSource() {
            try {
                Processor proc =  new();
                XsltCompiler comp = proc.NewXsltCompiler();
                StringReader sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template match='/'>///<xsl:apply-templates/></xsl:template>" +
                    "<xsl:template match='*'>***<xsl:apply-templates/></xsl:template>" +
                    "</xsl:stylesheet>");
                XsltExecutable exec = comp.Compile(sr);
                Xslt30Transformer xsl = exec.Load30();
                String source = "<?xml version='1.0'?>" +
                                "<doc><a><b/></a></doc>";

                // Read an XML file to a DOM
                XmlTextReader xmlReader = new XmlTextReader(new StringReader(source));
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlReader);


                XmlElement ele = doc.DocumentElement;
                DocumentBuilder builder = proc.NewDocumentBuilder();

                StringWriter writer = new StringWriter();
                Serializer serializer = proc.NewSerializer(writer);
                xsl.ApplyTemplates(builder.Wrap(doc).TypedValue, serializer);

                Assert.False(writer.ToString().Contains("///"), "started at element node");
            }
            catch (Exception e) {
                //e.printStackTrace();
                Assert.Fail(e.Message);
            }
        }

        /**
         * Send output to an XdmDestination
         */
        [Test]
        public void TestXdmDestination() {
            try {
                Processor proc =  new();
                XsltCompiler comp = proc.NewXsltCompiler();
                StringReader style = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template name='go'><a/></xsl:template>" +
                    "</xsl:stylesheet>");
                XsltExecutable exec = comp.Compile(style);
                Xslt30Transformer xsl = exec.Load30();
                XdmDestination outDest = new XdmDestination();
                xsl.CallTemplate(new QName("go"), outDest);
                XdmNode root = outDest.XdmNode;
                Assert.True(XdmNodeKind.ForNode(root) == XdmNodeKind.Document, "result is document node");
            }
            catch (Exception e) {
                //Console.WriteLine(e.printStackTrace());
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestXdmDestinationWithItemSeparator() {
            try {
                Processor proc =  new();
                XsltCompiler comp = proc.NewXsltCompiler();
                StringReader sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template name='go'><xsl:comment>A</xsl:comment><out/><xsl:comment>Z</xsl:comment></xsl:template>" +
                    "<xsl:output method='xml' item-separator='§'/>" +
                    "</xsl:stylesheet>");
                XsltExecutable exec = comp.Compile(sr);
                Xslt30Transformer xsl = exec.Load30();
                XdmDestination outi = new XdmDestination();
                xsl.CallTemplate(new QName("go"), outi);
                XdmNode root = outi.XdmNode;
                Assert.AreEqual("<!--A-->§<out/>§<!--Z-->", root.ToString());
                Assert.True(root.NodeKind == XdmNodeKind.Document, "result is document node");
            }
            catch (Exception e) {
                //Console.WriteLine(e.printStackTrace());
                Assert.Fail(e.Message);
            }
        }

 
        [Test]
        public void TestExtensionWithSchemaAccess() {
            // Create a Processor instance.
            try {
                Processor proc = new Processor(true);
                proc.SetProperty("http://saxon.sf.net/feature/timing", "true");
                XdmItemType isOptionalType = proc.ParseItemType("function(node(), xs:QName) as xs:boolean");
                proc.RegisterExtensionFunction(new QName("java:s9apitest.TestXslt30Transformer", "isOptional"), 
                        isOptionalType,
                    (args) => {
                                XdmNode parent = (XdmNode) args[0];
                                QName qName = ((XdmAtomicValue) args[1]).AsQName();
                                Configuration config = proc.Implementation;
                                SchemaType parentType = parent.Implementation.getSchemaType();
                                int fp = config.getNamePool().allocateFingerprint(qName.Uri, qName.LocalName);
                                int cardinality = ((ComplexType) parentType).getElementParticleCardinality(fp, true);
                                return new XdmAtomicValue(Cardinality.allowsZero(cardinality));
                            });
                XsltCompiler comp = proc.NewXsltCompiler();
                comp.SchemaAware = true;
                StringReader style = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:my='java:s9apitest.TestXslt30Transformer' exclude-result-prefixes='my'>" +
                    "<xsl:template match='*'><xsl:copy><xsl:copy-of select='@*'/><xsl:apply-templates/></xsl:copy></xsl:template>\n" +
                    "<xsl:template match='*/*/*/*[my:isOptional(.., node-name(.))]'/> \n" +
                    "</xsl:stylesheet>");
                XsltExecutable exec = comp.Compile(style);
                Xslt30Transformer transformer = exec.Load30();

                SchemaManager sm = proc.SchemaManager;
                sm.Compile(new XmlTextReader(ConfigTest.DATA_DIR + "books.xsd"));
                DocumentBuilder builder = proc.NewDocumentBuilder();
                SchemaValidator schemaValidator = sm.NewSchemaValidator();
                builder.SchemaValidator = schemaValidator;
                XdmNode books = builder.Build(new Uri(ConfigTest.DataDirUri, "books.xml"));
                
                StringWriter sw = new StringWriter();
                Serializer serializer = proc.NewSerializer(sw);
                serializer.SetOutputProperty(Serializer.INDENT, "yes");
                XdmDestination dest = new XdmDestination();
                schemaValidator.SetDestination(dest);

                transformer.ApplyTemplates(books, serializer);
                Assert.True(sw.ToString().Contains("TITLE"));
                Assert.False(sw.ToString().Contains("AUTHOR"));
            }
            catch (Exception e) {
                //Console.WriteLine(e.printStackTrace());
                Assert.Fail(e.Message);
            }
        }
        
        [Test]
        public void TestPipeline() {
            try {
                Processor proc =  new();
                XsltCompiler comp = proc.NewXsltCompiler();

                String xsl = "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                             "<xsl:template match='/'><a><xsl:copy-of select='.'/></a></xsl:template>" +
                             "</xsl:stylesheet>";
                String xml = "<z/>";
                DocumentBuilder builder = proc.NewDocumentBuilder();
                builder.BaseUri = new Uri("http://example.com");
                XdmNode inNode = builder.Build(new StringReader(xml));
                Xslt30Transformer stage1 = comp.Compile(new StringReader(xsl)).Load30();
                Xslt30Transformer stage2 = comp.Compile(new StringReader(xsl)).Load30();
                Xslt30Transformer stage3 = comp.Compile(new StringReader(xsl)).Load30();
                Xslt30Transformer stage4 = comp.Compile(new StringReader(xsl)).Load30();
                Xslt30Transformer stage5 = comp.Compile(new StringReader(xsl)).Load30();
                XdmDestination d1 = new XdmDestination();
                stage1.ApplyTemplates(inNode, d1);
                XdmDestination d2 = new XdmDestination();
                stage2.ApplyTemplates(d1.XdmNode, d2);
                XdmDestination d3 = new XdmDestination();
                stage3.ApplyTemplates(d2.XdmNode, d3);
                XdmDestination d4 = new XdmDestination();
                stage4.ApplyTemplates(d3.XdmNode, d4);
                StringWriter sw = new StringWriter();
                Serializer outi = proc.NewSerializer(sw);
                stage5.ApplyTemplates(d4.XdmNode, outi);
                Assert.True(sw.ToString().Contains("<a><a><a><a><a><z/></a></a></a></a></a>"), "output");
            }
            catch (Exception err) {
                Console.Error.WriteLine(err.StackTrace);
                Assert.Fail(err.Message);
            }
        }

       


        /**
        * s9api transformation pipeline on XsltTransformer API
        */
        [Test]
        public void TestPipelineOnXsltTransformer() {
            try {
                Processor proc = new Processor(true);
                XsltCompiler comp = proc.NewXsltCompiler();

                String xsl = "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                             "<xsl:template match='/'><a><xsl:copy-of select='.'/></a></xsl:template>" +
                             "</xsl:stylesheet>";
                String xml = "<z/>";
                DocumentBuilder builder = proc.NewDocumentBuilder();
                builder.BaseUri = new Uri("http://example.com");
                XdmNode doc = builder.Build(new StringReader(xml));
                XsltTransformer t1 = comp.Compile(new StringReader(xsl)).Load();
                XsltTransformer t2 = comp.Compile(new StringReader(xsl)).Load();
                XsltTransformer t3 = comp.Compile(new StringReader(xsl)).Load();

                XdmDestination d1 = new XdmDestination();
                var dest = new XdmDestination();

                t1.InitialContextNode = doc;

                t2.Destination = t3;

                t3.Destination = dest;

                t1.Run(t2);

                XdmNode node = dest.XdmNode;
                StringWriter sw = new StringWriter();
                Serializer serializer = proc.NewSerializer(sw);
                serializer.SetOutputProperty(Serializer.SAXON_INDENT_SPACES, "0");
                serializer.SerializeXdmNode(node);
                Assert.True(sw.ToString().Contains("<a><a><a><z/></a></a></a>"), "output");
            }
            catch (Exception err) {
                Console.Error.WriteLine(err.StackTrace);
                Assert.Fail(err.Message);
            }
        }
        
        [Test]
        public void TestPipeline2() {
            try {
                Processor proc =  new();
                XsltCompiler comp = proc.NewXsltCompiler();

                const string xsl = "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                                   "<xsl:template match='/'><a><xsl:copy-of select='.'/></a></xsl:template>" +
                                   "</xsl:stylesheet>";
                const string xml = "<z/>";

                Xslt30Transformer stage1 = comp.Compile(new StringReader(xsl)).Load30();
                Xslt30Transformer stage2 = comp.Compile(new StringReader(xsl)).Load30();
                Xslt30Transformer stage3 = comp.Compile(new StringReader(xsl)).Load30();
                Xslt30Transformer stage4 = comp.Compile(new StringReader(xsl)).Load30();
                Xslt30Transformer stage5 = comp.Compile(new StringReader(xsl)).Load30();
                StringWriter sw = new StringWriter();
                Serializer outi = proc.NewSerializer();
                XdmDestination d1 = new XdmDestination();
                outi.OutputWriter = sw;
                stage1.ApplyTemplates(new MemoryStream(Encoding.ASCII.GetBytes(xml)),
                    stage2.AsDocumentDestination(
                        stage3.AsDocumentDestination(
                            stage4.AsDocumentDestination(
                                stage5.AsDocumentDestination(outi)))));
                Console.WriteLine(sw.ToString());
                Assert.True(sw.ToString().Contains("<a><a><a><a><a><z/></a></a></a></a></a>"), "output");
            }
            catch (Exception err) {
                Console.Error.WriteLine(err.StackTrace);
                Assert.Fail(err.Message);
            }
        }


        /**
        * s9api transformation pipeline
        */
        [Test]
        public void TestPipelineShort() {
            try {
                Processor proc = new Processor(true);
                XsltCompiler comp = proc.NewXsltCompiler();

                const string xsl = "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                                   "<xsl:template match='/'><a><xsl:copy-of select='.'/></a></xsl:template>" +
                                   "</xsl:stylesheet>";
                const string xml = "<z/>";
                Xslt30Transformer stage1 = comp.Compile(new StringReader(xsl)).Load30();
                Xslt30Transformer stage2 = comp.Compile(new StringReader(xsl)).Load30();
                StringWriter sw = new();
                Serializer outi = proc.NewSerializer(sw);
                outi.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");
                stage1.ApplyTemplates(new MemoryStream(Encoding.ASCII.GetBytes(xml)),
                    stage2.AsDocumentDestination(outi));
                Console.WriteLine(sw);
                Assert.True(sw.ToString().Contains("<a><a><z/></a></a>"), "output");
            }
            catch (Exception err) {
                Console.Error.WriteLine(err.StackTrace);
                Assert.Fail(err.Message);
            }
        }

        /**
         * Test location information for run-time errors when stylesheet built as an XdmNode
         */
        public void TestErrorNotificationFromSaxonParse() {
            List<Error> errors = new ();

            try {
                // Norm Walsh reported saxon:parse errors not notified to the ErrorListener. Can't reproduce.

                Processor proc = new Processor(true);


                XsltCompiler xqcomp = proc.NewXsltCompiler();
                xqcomp.ErrorReporter = errors.Add;

                StringReader sr = new StringReader("<xsl:stylesheet version='2.0' " +
                                                   "xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:saxon='http://saxon.sf.net/'>\n" +
                                                   "<xsl:template name='main'>\n" +
                                                   "<xsl:copy-of select=\"saxon:parse('&lt;doc>&lt;e>text&lt;/f>&lt;/doc>')\"/>\n" +
                                                   "</xsl:template>\n" +
                                                   "</xsl:stylesheet>");


                Xslt30Transformer trans = xqcomp.Compile(sr).Load30();

                Serializer serializer = proc.NewSerializer();
                MemoryStream os = new MemoryStream();
                serializer.OutputStream = os;

                trans.CallTemplate(new QName("main"), serializer);

                Assert.Fail("Transformation should fail");
            }
            catch (Exception e) {
                //Assert.True(warnings.isEmpty(), "warnings");
                Assert.True(errors.Capacity == 0, "errors");
                //Assert.True(fatals.size() == 1, "fatals");
                //e.printStackTrace();
            }
        }

        [Test]
        public void TestSchemaAware11() {
            // Create a Processor instance.
            try {
                Processor proc = new Processor(true);
                proc.SetProperty("http://saxon.sf.net/feature/timing", "true");
                proc.SetProperty("http://saxon.sf.net/feature/xsd-version", "1.1");
                XsltCompiler comp = proc.NewXsltCompiler();
                comp.SchemaAware = true;
                StringReader sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:import-schema><xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>" +
                    "<xs:element name='e'><xs:complexType><xs:sequence><xs:element name='p'/></xs:sequence><xs:assert test='exists(p)'/></xs:complexType></xs:element>" +
                    "</xs:schema></xsl:import-schema>" +
                    "<xsl:variable name='v'><e><p/></e></xsl:variable>" +
                    "<xsl:template name='main'><xsl:copy-of select='$v' validation='strict'/></xsl:template>" +
                    "</xsl:stylesheet>");
                XsltExecutable exec = comp.Compile(sr);
                Xslt30Transformer transformer = exec.Load30();

                StringWriter sw = new StringWriter();
                Serializer serializer = proc.NewSerializer(sw);
                serializer.SetOutputProperty(Serializer.INDENT, "no");
                transformer.CallTemplate(new QName("main"), serializer);
                Assert.True(sw.ToString().Contains("<e>"));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }
        
        [Test]
        public void TestSchemaAwareStreaming() {
            try {
                Processor proc = new Processor(true);
                proc.SetProperty("http://saxon.sf.net/feature/timing", "true");
                proc.SetProperty("http://saxon.sf.net/feature/xsd-version", "1.1");
                XsltCompiler comp = proc.NewXsltCompiler();
                comp.SchemaAware = true;
                StringReader src = new("<e><p/></e>");
                StringReader sr = new (
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:import-schema><xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>" +
                    "<xs:element name='e'><xs:complexType><xs:sequence><xs:element name='p'/></xs:sequence><xs:assert test='exists(p)'/></xs:complexType></xs:element>" +
                    "</xs:schema></xsl:import-schema>" +
                    "<xsl:variable name='v'><e><p/></e></xsl:variable>" +
                    "<xsl:template match='schema-element(e)'><ok/></xsl:template>" +
                    "<xsl:mode streamable='yes'/>" +
                    "</xsl:stylesheet>");
                XsltExecutable exec = comp.Compile(sr);
                Xslt30Transformer transformer = exec.Load30();
                transformer.SchemaValidationMode = SchemaValidationMode.Strict;
                StringWriter sw = new StringWriter();
                Serializer serializer = proc.NewSerializer(sw);
                serializer.SetOutputProperty(Serializer.INDENT, "no");
                transformer.ApplyTemplates(src, serializer);
                Assert.True(sw.ToString().Contains("<ok/>"));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }
        
        [Test]
        public void TestSchemaAwareStreamingFromXmlReader() {
            try {
                Processor proc = new Processor(true);
                proc.SetProperty("http://saxon.sf.net/feature/timing", "true");
                proc.SetProperty("http://saxon.sf.net/feature/xsd-version", "1.1");
                XsltCompiler comp = proc.NewXsltCompiler();
                comp.SchemaAware = true;
                StringReader src = new("<e><p/></e>");
                StringReader sr = new (
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:import-schema><xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>" +
                    "<xs:element name='e'><xs:complexType><xs:sequence><xs:element name='p'/></xs:sequence><xs:assert test='exists(p)'/></xs:complexType></xs:element>" +
                    "</xs:schema></xsl:import-schema>" +
                    "<xsl:variable name='v'><e><p/></e></xsl:variable>" +
                    "<xsl:template match='schema-element(e)'><ok/></xsl:template>" +
                    "<xsl:mode streamable='yes'/>" +
                    "</xsl:stylesheet>");
                XsltExecutable exec = comp.Compile(sr);
                Xslt30Transformer transformer = exec.Load30();
                XmlReader reader = XmlReader.Create(src);
                transformer.SchemaValidationMode = SchemaValidationMode.Strict;
                StringWriter sw = new StringWriter();
                Serializer serializer = proc.NewSerializer(sw);
                serializer.SetOutputProperty(Serializer.INDENT, "no");
                transformer.ApplyTemplates(reader, serializer);
                Assert.True(sw.ToString().Contains("<ok/>"));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }
        
        [Test]
        public void TestStreamedFragment() {
            try {
                Processor proc = new Processor(true);
                proc.SetProperty("http://saxon.sf.net/feature/timing", "true");
                proc.SetProperty("http://saxon.sf.net/feature/xsd-version", "1.1");
                XsltCompiler comp = proc.NewXsltCompiler();
                comp.SchemaAware = false;
                StringReader src = new("<e/><e/><e/>");
                StringReader sr = new (
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template match='/'><out><xsl:apply-templates/></out></xsl:template>" +
                    "<xsl:template match='e'><ok/></xsl:template>" +
                    "<xsl:mode streamable='yes'/>" +
                    "</xsl:stylesheet>");
                XsltExecutable exec = comp.Compile(sr);
                Xslt30Transformer transformer = exec.Load30();
                XmlReader reader = XmlReader.Create(src, new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Fragment });
                transformer.SchemaValidationMode = SchemaValidationMode.None;
                StringWriter sw = new ();
                Serializer serializer = proc.NewSerializer(sw);
                serializer.SetOutputProperty(Serializer.INDENT, "no");
                transformer.ApplyTemplates(reader, serializer);
                Assert.True(sw.ToString().Contains("<out><ok/><ok/><ok/></out>"));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }
        
        [Test]
        public void TestStreamedInputWithStripping() {
            try {
                Processor proc = new Processor(true);
                proc.SetProperty("http://saxon.sf.net/feature/timing", "true");
                proc.SetProperty("http://saxon.sf.net/feature/xsd-version", "1.1");
                XsltCompiler comp = proc.NewXsltCompiler();
                comp.SchemaAware = false;
                StringReader src = new("<a><x> </x><z> </z></a>");
                StringReader sr = new (
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template match='/'><out><xsl:apply-templates/></out></xsl:template>" +
                    "<xsl:mode streamable='yes' on-no-match='shallow-copy'/>" +
                    "<xsl:strip-space elements='z'/>" +
                    "</xsl:stylesheet>");
                XsltExecutable exec = comp.Compile(sr);
                Xslt30Transformer transformer = exec.Load30();
                XmlReader reader = XmlReader.Create(src);
                transformer.SchemaValidationMode = SchemaValidationMode.None;
                StringWriter sw = new ();
                Serializer serializer = proc.NewSerializer(sw);
                serializer.SetOutputProperty(Serializer.INDENT, "no");
                XdmValue result = transformer.ApplyTemplates(reader);
                serializer.SerializeXdmNode((XdmNode)result[0]);
                Assert.True(sw.ToString().Contains("<out><a><x> </x><z/></a></out>"));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }
        
        [Test]
        public void TestNonDocumentResult() {
            try {
                Processor proc = new Processor(true);
                proc.SetProperty("http://saxon.sf.net/feature/timing", "true");
                proc.SetProperty("http://saxon.sf.net/feature/xsd-version", "1.1");
                XsltCompiler comp = proc.NewXsltCompiler();
                comp.SchemaAware = false;
                StringReader src = new("<a><x> </x><z> </z></a>");
                StringReader sr = new (
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template match='/'><xsl:map><xsl:apply-templates/></xsl:map></xsl:template>" +
                    "<xsl:template match='*'><xsl:map-entry key='name(.)'>1</xsl:map-entry><xsl:apply-templates select='*'/></xsl:template>" +
                    "<xsl:mode streamable='yes' on-no-match='shallow-copy'/>" +
                    "<xsl:strip-space elements='*'/>" +
                    "</xsl:stylesheet>");
                XsltExecutable exec = comp.Compile(sr);
                Xslt30Transformer transformer = exec.Load30();
                XmlReader reader = XmlReader.Create(src);
                transformer.SchemaValidationMode = SchemaValidationMode.None;
                XdmValue result = transformer.ApplyTemplates(reader);
                Assert.True(result[0] is XdmMap);
                XdmMap map = (XdmMap)result[0];
                Assert.True(map.ContainsKey(new XdmAtomicValue("x")));
                Assert.True(map.Get(new XdmAtomicValue("x")).ToString() == "1");
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestValidationWarnings() {
            try {
                Processor processor = new Processor(true);
                processor.SetProperty("http://saxon.sf.net/feature/validation-warnings", "true");
                processor.SchemaManager.Compile(new Uri(ConfigTest.DATA_DIR + "books.xsd"));

                Serializer serializer = processor.NewSerializer();
                serializer.OutputFile = ConfigTest.SANDPIT_DIR + "validationWarnings.xml";

                XsltCompiler compiler = processor.NewXsltCompiler();
                compiler.SchemaAware = true;

                const string xsl = "<xsl:stylesheet version=\"2.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                                   "\n" +
                                   "    <xsl:output indent=\"yes\" encoding=\"UTF-8\" />\n" +
                                   "\n" +
                                   "    <xsl:template match=\"/\">\n" +
                                   "        <xsl:document validation='strict'>\n" +
                                   "            <xsl:next-match />\n" +
                                   "        </xsl:document>\n" +
                                   "    </xsl:template>\n" +
                                   "\n" +
                                   "    <xsl:template match=\"*\">\n" +
                                   "        <xsl:element name=\"{name()}\" namespace=\"{namespace-uri()}\">\n" +
                                   "            <invalid-element/>\n" +
                                   "        </xsl:element>\n" +
                                   "    </xsl:template>\n" +
                                   "</xsl:stylesheet> ";
                Xslt30Transformer transformer = compiler.Compile(new StringReader(xsl)).Load30();

                transformer.ApplyTemplates(new MemoryStream(Encoding.ASCII.GetBytes(xsl)), serializer);

                Assert.True(true);
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }


        /**
         * Test that the base URI of the result document is the base output URI
         */
        [Test]
        public void TestOutputBaseUri() {
            try {
                Processor processor =  new();

                DocumentBuilder builder = processor.NewDocumentBuilder();
                builder.BaseUri = new Uri("http://www.example.com");
                //builder.DtdValidation =  false;
                builder.LineNumbering = true;

                XdmNode document = builder.Build(new FileStream(ConfigTest.DATA_DIR + "books.xml", FileMode.Open));
                XdmNode stylesheet = builder.Build(new FileStream(ConfigTest.DATA_DIR + "books.xsl", FileMode.Open));

                XsltCompiler compiler = processor.NewXsltCompiler();
                compiler.SchemaAware = false;
                XsltExecutable exec = compiler.Compile(stylesheet);
                Xslt30Transformer transformer = exec.Load30();

                XdmDestination result = new XdmDestination();
                transformer.BaseOutputURI = "file://tmp/out.xml";

                transformer.SchemaValidationMode = SchemaValidationMode.Unspecified;
                transformer.GlobalContextItem = document;
                transformer.ApplyTemplates(document, result);

                XdmNode xformed = result.XdmNode;
                Assert.AreEqual("file://tmp/out.xml", xformed.BaseUri.ToString(), "output base uri");
            }
            catch (Exception e) {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(e.Message);
                Assert.Fail();
            }
        }

        /**
     * test a call on XsltExecutable.getGlobalParameters
     * @throws net.sf.saxon.s9api.SaxonApiException
     */
        [Test]
        public void TestGlobalParameters() {
            Processor processor = new Processor(true);
            XsltCompiler xsltCompiler = processor.NewXsltCompiler();

            String source = "<?xml version='1.0'?>" +
                            "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                            "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                            "                version='2.0'>" +
                            "                <xsl:param name='surname' as='xs:string' />" +
                            "                <xsl:param name='age' as='xs:integer' />" +
                            "                <xsl:template match='/'><xsl:element name='root' /></xsl:template>" +
                            "                </xsl:stylesheet>";

            XsltExecutable xsltExecutable = xsltCompiler.Compile(new MemoryStream(Encoding.ASCII.GetBytes(source)));
            Dictionary<QName, ParameterDetails> params1 = xsltExecutable.GetGlobalParameters();

            Console.WriteLine();
            int i = 0;
            foreach (QName key in params1.Keys) {
                Console.WriteLine("Parameter n." + (++i) + ": " + key);
            }

            Console.WriteLine();
            Assert.AreEqual(2, 2);
        }

        /**
    * Get declared types of global parameters
    * @throws SaxonApiException
    * @throws UnsupportedEncodingException
    */
        [Test]
        public void TestGlobalParameterType() {
            Processor processor =  new();
            XsltCompiler xsltCompiler = processor.NewXsltCompiler();

            String source = "<?xml version='1.0'?>" +
                            "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                            "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                            "                version='2.0'>" +
                            "                <xsl:param name='surname' as='xs:string' />" +
                            "                <xsl:param name='age' as='xs:integer?' />" +
                            "                <xsl:param name='doc' as='document-node()' />" +
                            "                <xsl:param name='ele' as='element(pre:a, xs:date)+' xmlns:pre='http://pre/'/>" +
                            "                <xsl:template match='/'><xsl:element name='root' /></xsl:template>" +
                            "                </xsl:stylesheet>";

            XsltExecutable xsltExecutable = xsltCompiler.Compile(new StringReader(source));
            Dictionary<QName, ParameterDetails> paramsi = xsltExecutable.GetGlobalParameters();
            XdmItemType t1 = paramsi[(new QName("surname"))].DeclaredType.ItemType;
            Assert.AreEqual(t1.ToString(), "xs:string", "t1");
            char c1 = paramsi[new QName("surname")].DeclaredType.OccurrenceIndicator;
            Assert.AreEqual(c1, XdmSequenceType.ONE, "c1");
            XdmItemType t2 = paramsi[new QName("age")].DeclaredType.ItemType;
            Assert.AreEqual(t2.ToString(),"xs:integer", "t2");
            char c2 = paramsi[new QName("age")].DeclaredType.OccurrenceIndicator;
            Assert.AreEqual(c2, XdmSequenceType.ZERO_OR_ONE, "c2");
            XdmItemType t3 = paramsi[(new QName("doc"))].DeclaredType.ItemType;
            Assert.AreEqual(t3.ToString(),"document-node()", "t3");
            char c3 = paramsi[(new QName("doc"))].DeclaredType.OccurrenceIndicator;
            Assert.AreEqual(c3, XdmSequenceType.ONE, "c3");
            XdmItemType t4 = paramsi[(new QName("ele"))].DeclaredType.ItemType;
            Assert.AreEqual("element(Q{http://pre/}a, Q{" + NamespaceConstant.SCHEMA + "}date)", t4.ToString(), "t4");
            char c4 = paramsi[(new QName("ele"))].DeclaredType.OccurrenceIndicator;
            Assert.AreEqual(c4, XdmSequenceType.ONE_OR_MORE, "c4");
        }

        /**
     * Test an external  call on a stylesheet function
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */
        [Test]
        public void TestCallFunction() {
            try {
                //Configuration configuration = Configuration.newConfiguration();
                Processor processor = new Processor(true);

                XsltCompiler xsltCompiler = processor.NewXsltCompiler();

                string source = "<?xml version='1.0'?>" +
                                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                                "                xmlns:f='http://localhost/'" +
                                "                version='3.0'>" +
                                "                <xsl:function name='f:add' visibility='public'>" +
                                "                  <xsl:param name='a'/><xsl:param name='b'/>" +
                                "                  <xsl:sequence select='$a + $b'/></xsl:function>" +
                                "                </xsl:stylesheet>";

                xsltCompiler.BaseUri = new Uri("file:///dummy/");
                XsltExecutable xsltExecutable = xsltCompiler.Compile(new MemoryStream(Encoding.ASCII.GetBytes(source)));
                Xslt30Transformer t = xsltExecutable.Load30();
                XdmValue v = t.CallFunction(new QName("http://localhost/", "f:add"),
                    new XdmValue[] {new XdmAtomicValue(2), new XdmAtomicValue(3)});
                Assert.AreEqual(((XdmAtomicValue) v).AsLong(), 5);
            }
            catch (Exception e) {
                //e.printStackTrace();
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestNullInitialContext() {
            try {
                Processor proc =  new();
                String sr = "<input/>";

                DocumentBuilder builder = proc.NewDocumentBuilder();
                builder.BaseUri = new Uri("file:///dummy/");
                XdmNode node = builder.Build(new MemoryStream(Encoding.ASCII.GetBytes(sr)));
                XsltCompiler xqcomp = proc.NewXsltCompiler();
                xqcomp.BaseUri = new Uri("file:///dummy/");
                sr = "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                     "  <xsl:output omit-xml-declaration=\"yes\"/>\n" +
                     "  <xsl:template match=\"/\"><output1/><output2/></xsl:template>\n" +
                     "</xsl:stylesheet>";

                XsltTransformer trans = xqcomp.Compile(new MemoryStream(Encoding.ASCII.GetBytes(sr))).Load();

                StringWriter sw = new StringWriter();
                Serializer s = proc.NewSerializer();
                s.BaseUri = new Uri("file:///dummy/");
                s.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");
                s.OutputWriter = sw;
                trans.InitialContextNode = node;

                trans.Run(s);

                String result = sw.ToString();
                Assert.True(result.Equals("<output1/><output2/>"), "output");
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }


        /**
     * Test an external  call on a stylesheet function with argument conversion
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */
        [Test]
        public void TestCallFunctionArgConversion() {
            try {
                //Configuration configuration = Configuration.newConfiguration();
                Processor processor = new Processor(true);
                XsltCompiler xsltCompiler = processor.NewXsltCompiler();

                String source = "<?xml version='1.0'?>" +
                                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                                "                xmlns:f='http://localhost/'" +
                                "                version='3.0'>" +
                                "                <xsl:function name='f:add' visibility='public'>" +
                                "                   <xsl:param name='a' as='xs:double'/>" +
                                "                   <xsl:param name='b' as='xs:double'/>" +
                                "                   <xsl:sequence select='$a + $b'/>" +
                                "                </xsl:function>" +
                                "                </xsl:stylesheet>";

                //MemorySource ss = new MemorySource(new ByteArrayInputStream(source.getBytes("UTF-8")), "file:///dummy/");
                xsltCompiler.BaseUri = new Uri("file:///dummy/");
                XsltExecutable xsltExecutable = xsltCompiler.Compile(new MemoryStream(Encoding.ASCII.GetBytes(source)));
                Xslt30Transformer t = xsltExecutable.Load30();
                XdmValue v = t.CallFunction(new QName("http://localhost/", "f:add"),
                    new XdmValue[] {new XdmAtomicValue(2), new XdmAtomicValue(3)});
                Assert.AreEqual(((XdmAtomicValue) v).AsDouble(), 5.0e0);
                Assert.AreEqual((((XdmAtomicValue) v)).TypeName.LocalName, "double");
                Assert.AreEqual((((XdmAtomicValue) v)).PrimitiveTypeName.LocalName, "double");
            }
            catch (Exception e) {
                //e.printStackTrace();
                Assert.Fail(e.Message);
            }
        }

        /**
     * Test an external  call on a stylesheet function serializing the results
     * Also makes access to a stylesheet parameter which in turn accesses the global context item
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */
        [Test]
        public void TestCallFunctionWrapResults() {
            try {
                Processor processor = new Processor(true);
                processor.SetProperty("http://saxon.sf.net/feature/generateByteCode", "false");
                XsltCompiler xsltCompiler = processor.NewXsltCompiler();

                String source = "<?xml version='1.0'?>" +
                                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                                "                xmlns:f='http://localhost/'" +
                                "                version='2.0'>" +
                                "                <xsl:param name='x' as='xs:integer'/>" +
                                "                <xsl:param name='y' select='.+2'/>" +
                                "                <xsl:function name='f:add' visibility='public'>" +
                                "                   <xsl:param name='a' as='xs:double'/>" +
                                "                   <xsl:param name='b' as='xs:double'/>" +
                                "                   <xsl:sequence select='$a + $b + $x + $y'/>" +
                                "                </xsl:function>" +
                                "                </xsl:stylesheet>";

                xsltCompiler.BaseUri = new Uri("file:///dummy/");
                XsltExecutable xsltExecutable = xsltCompiler.Compile(new MemoryStream(Encoding.ASCII.GetBytes(source)));
                Xslt30Transformer t = xsltExecutable.Load30();
                t.GlobalContextItem = new XdmAtomicValue(20);
                Dictionary<QName, XdmValue> globals = new Dictionary<QName, XdmValue>();
                globals.Add(new QName("x"), new XdmAtomicValue(30));
                t.SetStylesheetParameters(globals);
                StringWriter sw = new StringWriter();
                Serializer s = processor.NewSerializer();
                s.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");
                s.OutputWriter = sw;
                t.CallFunction(new QName("http://localhost/", "f:add"),
                    new XdmValue[] {new XdmAtomicValue(2), new XdmAtomicValue(3)}, s);
                Assert.AreEqual(sw.ToString(), "57");
            }
            catch (Exception e) {
                //e.printStackTrace();
                Assert.Fail(e.Message);
            }
        }


        /**
     * Test an external  call on a stylesheet function with invalid argument conversion
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */
        [Test]
        public void TestCallFunctionArgInvalid() {
            try {
                //Configuration configuration = Configuration.newConfiguration();
                Processor processor = new Processor(true);

                XsltCompiler xsltCompiler = processor.NewXsltCompiler();

                String source = "<?xml version='1.0'?>" +
                                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                                "                xmlns:f='http://localhost/'" +
                                "                version='2.0'>" +
                                "                <xsl:function name='f:add' visibility='public'>" +
                                "                   <xsl:param name='a' as='xs:double'/>" +
                                "                   <xsl:param name='b' as='xs:double'/>" +
                                "                   <xsl:sequence select='$a + $b'/>" +
                                "                </xsl:function>" +
                                "                </xsl:stylesheet>";

                xsltCompiler.BaseUri = new Uri("file:///dummy/");
                XsltExecutable xsltExecutable = xsltCompiler.Compile(new MemoryStream(Encoding.ASCII.GetBytes(source)));
                Xslt30Transformer t = xsltExecutable.Load30();
                XdmValue v = t.CallFunction(new QName("http://localhost/", "f:add"),
                    new XdmValue[] {new XdmAtomicValue(2), new XdmAtomicValue("3")});
                Assert.Fail("Failed to throw error");
            }
            catch (SaxonApiException e) {
                Console.WriteLine(e.Message);
                // OK
            }
        }

     /**
     * Test calling a stylesheet with an initial template with parameters
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */
        [Test]
        public void TestCallNamedTemplateWithParams() {
            try {
                //Configuration configuration = Configuration.newConfiguration();
                //configuration.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
                Processor processor = new Processor(true);
                XsltCompiler xsltCompiler = processor.NewXsltCompiler();

                String source = "<?xml version='1.0'?>" +
                                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                                "                version='2.0'>" +
                                "                <xsl:template name='t'>" +
                                "                   <xsl:param name='a' as='xs:double'/>" +
                                "                   <xsl:param name='b' as='xs:float'/>" +
                                "                   <xsl:sequence select='$a + $b'/>" +
                                "                </xsl:template>" +
                                "                </xsl:stylesheet>";


                xsltCompiler.BaseUri = new Uri("file:///dummy/");
                XsltExecutable xsltExecutable = xsltCompiler.Compile(new StringReader(source));
                Xslt30Transformer t = xsltExecutable.Load30();
                Dictionary<QName, XdmValue> params1 = new Dictionary<QName, XdmValue>();
                params1.Add(new QName("a"), new XdmAtomicValue(12));
                params1.Add(new QName("b"), new XdmAtomicValue(5));
                t.SetInitialTemplateParameters(params1, false);
                StringWriter sw = new StringWriter();
                Serializer se = processor.NewSerializer(sw);
                se.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");
                t.CallTemplate(new QName("t"), se);
                Assert.AreEqual(sw.ToString(), "17");
            }
            catch (Exception e) {
                //e.printStackTrace();
                Assert.Fail(e.Message);
            }
        }

        /**
     * Test calling a stylesheet with an initial template omitting a required parameters
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */
        [Test]
        public void TestCallNamedTemplateWithParamsRequired() {
            try {
                //Configuration configuration = Configuration.newConfiguration();
                //configuration.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
                Processor processor = new Processor(true);
                XsltCompiler xsltCompiler = processor.NewXsltCompiler();

                String source = "<?xml version='1.0'?>" +
                                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                                "                version='2.0'>" +
                                "                <xsl:template name='t'>" +
                                "                   <xsl:param name='a' as='xs:double'/>" +
                                "                   <xsl:param name='b' as='xs:float' required='yes'/>" +
                                "                   <xsl:sequence select='$a + $b'/>" +
                                "                </xsl:template>" +
                                "                </xsl:stylesheet>";


                XsltExecutable xsltExecutable = xsltCompiler.Compile(new StringReader(source));
                Xslt30Transformer t = xsltExecutable.Load30();
                Dictionary<QName, XdmValue> params1 = new Dictionary<QName, XdmValue>();
                params1.Add(new QName("a"), new XdmAtomicValue(12));
                t.SetInitialTemplateParameters(params1, false);
                StringWriter sw = new StringWriter();
                Serializer se = processor.NewSerializer(sw);
                se.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");
                t.CallTemplate(new QName("t"), se);
                Assert.Fail("failed to detect error");
            }
            catch (SaxonApiException e) {
                //Console.WriteLine(e.printStackTrace());
                Assert.True(true);
            }
        }

        /**
     * Test calling a stylesheet with an initial template with tunnel parameters
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */
        [Test]
        public void TestCallNamedTemplateWithTunnelParams() {
            try {
                //Configuration configuration = Configuration.newConfiguration();
                //configuration.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
                Processor processor = new Processor(true);
                XsltCompiler xsltCompiler = processor.NewXsltCompiler();

                String source = "<?xml version='1.0'?>" +
                                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                                "                version='2.0'>" +
                                "                <xsl:template name='t'>" +
                                "                   <xsl:call-template name='u'/>" +
                                "                </xsl:template>" +
                                "                <xsl:template name='u'>" +
                                "                   <xsl:param name='a' as='xs:double' tunnel='yes'/>" +
                                "                   <xsl:param name='b' as='xs:float' tunnel='yes'/>" +
                                "                   <xsl:sequence select='$a + $b'/>" +
                                "                </xsl:template>" +
                                "                </xsl:stylesheet>";


                XsltExecutable xsltExecutable = xsltCompiler.Compile(new StringReader(source));
                Xslt30Transformer t = xsltExecutable.Load30();
                Dictionary<QName, XdmValue> params1 = new Dictionary<QName, XdmValue>();
                params1.Add(new QName("a"), new XdmAtomicValue(12));
                params1.Add(new QName("b"), new XdmAtomicValue(5));
                t.SetInitialTemplateParameters(params1, true);
                StringWriter sw = new StringWriter();
                Serializer se = processor.NewSerializer(sw);
                se.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");
                t.CallTemplate(new QName("t"), se);
                Assert.AreEqual(sw.ToString(), "17");
            }
            catch (Exception e) {
                //Console.WriteLine(e.printStackTrace());
                Assert.Fail();
            }
        }


        /**
     * Test calling a stylesheet named template getting raw results back
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */
        [Test]
        public void TestCallNamedTemplateWithParamsRaw() {
            try {
                //configuration.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
                Processor processor = new Processor(true);
                processor.SetProperty(FeatureKeys.GENERATE_BYTE_CODE, "false");
                XsltCompiler xsltCompiler = processor.NewXsltCompiler();

                String source = "<?xml version='1.0'?>" +
                                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                                "                version='3.0'>" +
                                "                <xsl:template name='t'>" +
                                "                   <xsl:param name='a' as='xs:double'/>" +
                                "                   <xsl:param name='b' as='xs:float'/>" +
                                "                   <xsl:sequence select='$a+1, $b+1'/>" +
                                "                </xsl:template>" +
                                "                </xsl:stylesheet>";


                XsltExecutable xsltExecutable = xsltCompiler.Compile(new StringReader(source));
                Xslt30Transformer t = xsltExecutable.Load30();
                Dictionary<QName, XdmValue> params1 = new Dictionary<QName, XdmValue>();
                params1.Add(new QName("a"), new XdmAtomicValue(12));
                params1.Add(new QName("b"), new XdmAtomicValue(5));
                t.SetInitialTemplateParameters(params1, false);
                XdmValue val = t.CallTemplate(new QName("t"));
                Assert.AreEqual(2, val.Count);
                Assert.AreEqual(13, ((XdmAtomicValue) val[0]).AsLong());
                Assert.AreEqual(6, ((XdmAtomicValue) val[1]).AsLong());
            }
            catch (Exception e) {
                //Console.WriteLine(e.printStackTrace());
                Assert.Fail();
            }
        }


        /**
     * Test calling a stylesheet named template getting raw results back as a Destination
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */
        [Test]
        public void TestCallNamedTemplateWithParamsRawDestination() {
            try {
                //Configuration configuration = Configuration.newConfiguration();
                //configuration.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
                Processor processor = new Processor();
                XsltCompiler xsltCompiler = processor.NewXsltCompiler();

                String source = "<?xml version='1.0'?>" +
                                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                                "                version='3.0'>" +
                                "                <xsl:template name='t'>" +
                                "                   <xsl:param name='a' as='xs:double'/>" +
                                "                   <xsl:param name='b' as='xs:float'/>" +
                                "                   <xsl:sequence select='$a+1, $b+1'/>" +
                                "                </xsl:template>" +
                                "                </xsl:stylesheet>";

                //StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes("UTF-8")), "file:///dummy/");
                XsltExecutable xsltExecutable = xsltCompiler.Compile(new StringReader(source));
                Xslt30Transformer t = xsltExecutable.Load30();
                Dictionary<QName, XdmValue> paramsi = new Dictionary<QName, XdmValue>();
                paramsi.Add(new QName("a"), new XdmAtomicValue(12));
                paramsi.Add(new QName("b"), new XdmAtomicValue(5));
                t.SetInitialTemplateParameters(paramsi, false);
                RawDestination outi = new RawDestination();
                t.CallTemplate(new QName("t"), outi);
                XdmValue val = outi.XdmValue;
                Assert.AreEqual(2, val.Count);
                Assert.AreEqual(13, ((XdmAtomicValue) val.ItemAt(0)).AsLong());
                Assert.AreEqual(6, ((XdmAtomicValue) val.ItemAt(1)).AsLong());
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }


        /**
* Test calling a stylesheet with an initial template rule with parameters
* @throws net.sf.saxon.s9api.SaxonApiException if the call fails
*/
        [Test]
        public void TestCallTemplateRuleWithParams() {
            try {
                //configuration.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
                Processor processor = new Processor(true);
                XsltCompiler xsltCompiler = processor.NewXsltCompiler();

                String source = "<?xml version='1.0'?>" +
                                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                                "                version='2.0'>" +
                                "                <xsl:template match='*'>" +
                                "                   <xsl:param name='a' as='xs:double'/>" +
                                "                   <xsl:param name='b' as='xs:float'/>" +
                                "                   <xsl:sequence select='name(.), $a + $b'/>" +
                                "                </xsl:template>" +
                                "                </xsl:stylesheet>";

                xsltCompiler.BaseUri = new Uri("file:///dummy/");

                XsltExecutable xsltExecutable = xsltCompiler.Compile(new StringReader(source));
                Xslt30Transformer t = xsltExecutable.Load30();
                Dictionary<QName, XdmValue> params1 = new Dictionary<QName, XdmValue>();
                params1.Add(new QName("a"), new XdmAtomicValue(12));
                params1.Add(new QName("b"), new XdmAtomicValue(5));
                t.SetInitialTemplateParameters(params1, false);
                StringWriter sw = new StringWriter();
                Serializer se = processor.NewSerializer(sw);
                se.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");
                t.ApplyTemplates(new MemoryStream(Encoding.ASCII.GetBytes("<e/>")), se); //TODO remove uri argument
                Assert.AreEqual(sw.ToString(), "e 17");
            }
            catch (Exception ex) {
                //Console.WriteLine(e.printStackTrace());
                Assert.Fail(ex.Message);
            }
        }


        /**
     * Test calling a stylesheet with an initial template rule getting the raw result
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */
        [Test]
        public void TestApplyTemplatesRaw() {
            Processor processor = new Processor(true);
            XsltCompiler xsltCompiler = processor.NewXsltCompiler();

            String source = "<?xml version='1.0'?>" +
                            "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                            "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                            "                version='3.0'>" +
                            "                <xsl:template match='*'>" +
                            "                   <xsl:param name='a' as='xs:double'/>" +
                            "                   <xsl:param name='b' as='xs:float'/>" +
                            "                   <xsl:sequence select='., $a + $b'/>" +
                            "                </xsl:template>" +
                            "                </xsl:stylesheet>";


            XsltExecutable xsltExecutable = xsltCompiler.Compile(new StringReader(source));
            Xslt30Transformer t = xsltExecutable.Load30();
            Dictionary<QName, XdmValue> params1 = new Dictionary<QName, XdmValue>();

            params1.Add(new QName("a"), new XdmAtomicValue(12));
            params1.Add(new QName("b"), new XdmAtomicValue(5));
            t.SetInitialTemplateParameters(params1, false);
            StringWriter sw = new StringWriter();
            Serializer se = processor.NewSerializer(sw);
            se.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");
            XdmValue result = t.ApplyTemplates(new MemoryStream(Encoding.UTF8.GetBytes("<e/>")),
                new Uri("http://dummpy.com"));
            Assert.AreEqual(2, result.Count);
            XdmItem first = result.ItemAt(0);
            Assert.True(first is XdmNode, "t1");
            Assert.AreEqual(((XdmNode) first).NodeName.LocalName, "e");
            XdmItem second = result.ItemAt(1);
            Assert.True(second is XdmAtomicValue, "t2");
            Assert.AreEqual(((XdmAtomicValue) second).AsDouble(), 17e0);
        }

        

        /**
         * Test calling a stylesheet with an initial template rule getting the raw result
         * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
         */
        [Test]
        public void TestApplyTemplatesToXdm() {
            try {
                Processor processor = new Processor();
                XsltCompiler xsltCompiler = processor.NewXsltCompiler();

                string source = "<?xml version='1.0'?>" +
                                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                                "                version='3.0'>" +
                                "                <xsl:template match='*'>" +
                                "                   <xsl:param name='a' as='xs:double'/>" +
                                "                   <xsl:param name='b' as='xs:float'/>" +
                                "                   <xsl:sequence select='., $a + $b'/>" +
                                "                </xsl:template>" +
                                "                </xsl:stylesheet>";


                XsltExecutable xsltExecutable = xsltCompiler.Compile(new StringReader(source));
                Xslt30Transformer t = xsltExecutable.Load30();
                Dictionary<QName, XdmValue> paramsi = new Dictionary<QName, XdmValue>();
                paramsi.Add(new QName("a"), new XdmAtomicValue(12));
                paramsi.Add(new QName("b"), new XdmAtomicValue(5));
                t.SetInitialTemplateParameters(paramsi, false);
                StringWriter sw = new StringWriter();
                Serializer se = processor.NewSerializer(sw);
                se.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");

                XdmValue result = t.ApplyTemplates(new MemoryStream(Encoding.ASCII.GetBytes("<e/>")),
                    new Uri("file:///dummy/"));
                Assert.AreEqual(2, result.Count);
                XdmItem first = result.ItemAt(0);
                Assert.True(first is XdmNode, "t1");
                Assert.True(((XdmNode) first).NodeName.LocalName.Equals("e"));
                XdmItem second = result.ItemAt(1);
                Assert.True(second is XdmAtomicValue, "t2");
                Assert.True(((XdmAtomicValue) second).AsDouble() == 17e0, "v2");
            }
            catch (Exception err) {
                Console.Error.WriteLine(err.StackTrace);
                Assert.Fail(err.Message);
            }
        }


        /**
        * Test calling a stylesheet with an initial template rule getting the serialized
        * result with item separators
        *
        * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
        */
        [Test]
        public void TestApplyTemplatesToSerializer() {
            try {
                //Configuration configuration = Configuration.newConfiguration();
                Processor processor = new Processor();
                XsltCompiler xsltCompiler = processor.NewXsltCompiler();

                string source = "<?xml version='1.0'?>" +
                                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                                "                version='3.0'>" +
                                "                <xsl:output method='text' item-separator='~~'/>" +
                                "                <xsl:template match='.'>" +
                                "                   <xsl:param name='a' as='xs:double'/>" +
                                "                   <xsl:param name='b' as='xs:float'/>" +
                                "                   <xsl:sequence select='., $a + $b'/>" +
                                "                </xsl:template>" +
                                "                </xsl:stylesheet>";

                Stream ss = new MemoryStream(Encoding.UTF8.GetBytes(source));
                XsltExecutable xsltExecutable = xsltCompiler.Compile(ss);
                Xslt30Transformer t = xsltExecutable.Load30();
                Dictionary<QName, XdmValue> paramsi = new Dictionary<QName, XdmValue>();
                paramsi.Add(new QName("a"), new XdmAtomicValue(12));
                paramsi.Add(new QName("b"), new XdmAtomicValue(5));
                t.SetInitialTemplateParameters(paramsi, false);
                StringWriter sw = new StringWriter();
                Serializer se = processor.NewSerializer(sw);
                se.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");
                t.ApplyTemplates(new XdmAtomicValue(16), se);
                Assert.True("16~~17".Equals(sw.ToString()));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }


        [Test]
        public void TestTraceListener() {
            try {
                // Create a Processor instance.
                Processor processor = new Processor();

                ////////////////////////////// ADDING THIS CAUSES AN ISSUE IN THE CONTEXT ///////////////////////////
                // processor.Implementation.setTraceListener(new MyTraceListener());
                // Load the source document
                XdmNode input = processor.NewDocumentBuilder()
                    .Build(new Uri(new Uri(ConfigTest.DATA_DIR), "books.xml"));

                // Create a transformer for the stylesheet.
                XsltTransformer transformer = processor.NewXsltCompiler()
                    .Compile(new Uri(new Uri(ConfigTest.DATA_DIR), "books.xsl")).Load();

                // Set the root node of the source document to be the initial context node
                transformer.InitialContextNode = input;

                // Create a serializer, with output to the standard output stream
                Serializer serializer = processor.NewSerializer();
                serializer.OutputWriter = Console.Out;

                // Transform the source XML and serialize the result document
                transformer.Run(serializer);
                Assert.True(true);
            }
            catch (Exception ex) {
                Assert.Fail(ex.Message);
            }
        }


        /**
         * Send output to an XdmDestination
         */
        [Test]
        public void TestItemSeparatorToSerializer() {
            try {
                Processor proc =  new();
                XsltCompiler comp = proc.NewXsltCompiler();
                StringReader sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "  <xsl:template name='go'>"
                    + "<xsl:comment>start</xsl:comment><a><b/><c/></a><xsl:comment>end</xsl:comment>"
                    + "</xsl:template>" +
                    "</xsl:stylesheet>");
                XsltExecutable exec = comp.Compile(sr);
                Xslt30Transformer xsl = exec.Load30();
                StringWriter sw = new StringWriter();
                Serializer outi = proc.NewSerializer(sw);
                outi.SetOutputProperty(Serializer.METHOD, "xml");
                outi.SetOutputProperty(Serializer.INDENT, "no");
                outi.SetOutputProperty(Serializer.ITEM_SEPARATOR, "+++");
                xsl.CallTemplate(new QName("go"), outi);
                Console.WriteLine(sw.ToString());
                Assert.True(sw.ToString().Contains("<!--start-->+++"));
                Assert.True(sw.ToString().Contains("+++<!--end-->"));
                Assert.True(sw.ToString().Contains("<a><b/><c/></a>"));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }


        [Test]
        public void TestTransformReturningFunction() {
            try {
                Processor proc = new Processor(true);
                XsltCompiler comp = proc.NewXsltCompiler();
                comp.SchemaAware = true;
                StringReader sr = new StringReader(
                    "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='f' xmlns:xs='http://www.w3.org/2001/XMLSchema'>" +
                    "<xsl:template name='main'><xsl:sequence select='f:f#1'/></xsl:template> \n" +
                    "<xsl:function name='f:f' as='xs:integer'><xsl:param name='p'/><xsl:sequence select='$p+42'/></xsl:function> \n" +
                    "</xsl:stylesheet>");
                XsltExecutable exec = comp.Compile(sr);
                Xslt30Transformer transformer = exec.Load30();
                XdmValue value = transformer.CallTemplate(new QName("main"));
                XdmValue[] values = new XdmValue[1];
                values[0] = new XdmAtomicValue(17);
                XdmValue result = ((XdmFunctionItem) value).Invoke(values, proc);
                Assert.True(result is XdmAtomicValue);
                Assert.AreEqual(59, ((XdmAtomicValue) result).AsLong());
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }


        [Test]
        public void TestStripTypeAnnotations() {
            try {
                Processor proc = new Processor(true);
                XsltCompiler comp = proc.NewXsltCompiler();
                StringReader sr = new StringReader("<xslt:transform xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                                                   "                xmlns:my=\"http://ns.example.com/strip-type\"\n" +
                                                   "                xmlns:xslt=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                                                   "                exclude-result-prefixes=\"xs my\"\n" +
                                                   "                input-type-annotations=\"strip\"\n" +
                                                   "                version=\"2.0\">\n" +
                                                   "<!-- Purpose: Show that when @input-type-annotations=\"strip\", is-nilled property of element  nodes is set to false.-->\n" +
                                                   "\n" +
                                                   "   <xslt:template match=\"/my:userNode\">\n" +
                                                   "      <out>\n" +
                                                   "         <xslt:value-of select=\"nilled(.)\"/>\n" +
                                                   "      </out>\n" +
                                                   "   </xslt:template>\n" +
                                                   "</xslt:transform>");

                SchemaManager manager = proc.SchemaManager;
                comp.SchemaAware = true;

                manager.Compile(new FileStream(ConfigTest.DATA_DIR + "varietyOfTypes.xsd", FileMode.Open),
                    new Uri(ConfigTest.DATA_DIR + "varietyOfTypes.xsd"));

                XsltExecutable exec = comp.Compile(sr);
                Xslt30Transformer xsl = exec.Load30();
                String inn = "<?xml version='1.0'?> \n" +
                             "<my:userNode xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n" +
                             "\t\t  xsi:schemaLocation=\"http://ns.example.com/strip-type varietyOfTypes.xsd\"\n" +
                             "\t\t  xmlns:my=\"http://ns.example.com/strip-type\"\n" +
                             "\t \t  specialPart=\"123-AB\" listParts=\"000-SS 000-RR\" xsi:nil=\"true\"/>";

                SchemaValidator validator = manager.NewSchemaValidator();
                validator.Lax = false;
                DocumentBuilder builder = proc.NewDocumentBuilder();
                builder.SchemaValidator = validator;
                Stream sDoc = new MemoryStream(Encoding.UTF8.GetBytes(inn));
                builder.BaseUri = new Uri("file:///dummy/");
                XdmNode doc = builder.Build(sDoc);

                StringWriter writer = new StringWriter();
                Serializer outi = proc.NewSerializer(writer);
                xsl.ApplyTemplates(doc, outi);
                Assert.True(writer.ToString().Contains("false"), "strip-type-annotations not working");
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }


        [Test]
        public void TestXmlWriterDestination() {
            Processor proc = new (true);
            XsltCompiler comp = proc.NewXsltCompiler();
            StringReader sr = new (
                "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>\n" +
                "  <xsl:template match='/'>\n" +
                "    <xsl:variable name='node'>\n" +
                "      <xsl:sequence select='.'/>\n" +
                "    </xsl:variable>\n" +
                "    <xsl:sequence select='$node/node()'/>\n" +
                "  </xsl:template>\n" +
                "</xsl:stylesheet>");
            XsltExecutable exec = comp.Compile(sr);
            Xslt30Transformer xsl = exec.Load30();
            StringReader input = new ("<input size='3'><foo/></input>");
            StringWriter sw = new();
            XmlWriterSettings settings = new () {
                Encoding = new UTF8Encoding(),
                OmitXmlDeclaration = true
            };
            XmlWriterDestination dest = new (XmlWriter.Create(sw, settings));
            xsl.ApplyTemplates(input, dest);
            //Console.Error.WriteLine(sw.ToString());
            Assert.AreEqual("<input size=\"3\"><foo /></input>", sw.ToString());
        }
        
        [Test]
        public void TestXmlWriterDestinationWithNamespaces() {
            Processor proc = new (true);
            XsltCompiler comp = proc.NewXsltCompiler();
            StringReader sr = new (
                "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>\n" +
                "  <xsl:template match='/'>\n" +
                "    <xsl:variable name='node'>\n" +
                "      <xsl:sequence select='.'/>\n" +
                "    </xsl:variable>\n" +
                "    <xsl:sequence select='$node/node()'/>\n" +
                "  </xsl:template>\n" +
                "</xsl:stylesheet>");
            XsltExecutable exec = comp.Compile(sr);
            Xslt30Transformer xsl = exec.Load30();
            StringReader input = new ("<my:input xmlns:my='http://my.com/' size='3'><foo/></my:input>");
            StringWriter sw = new();
            XmlWriterSettings settings = new () {
                Encoding = new UTF8Encoding(),
                OmitXmlDeclaration = true
            };
            XmlWriterDestination dest = new (XmlWriter.Create(sw, settings));
            xsl.ApplyTemplates(input, dest);
            //Console.Error.WriteLine(sw.ToString());
            Assert.AreEqual("<my:input xmlns:my=\"http://my.com/\" size=\"3\"><foo /></my:input>", sw.ToString());
        }



        // 
        // Test Strip-space with ApplyTemplates
        // 
        [Test]
        public void TestStripSpace() {
            try {
                Processor proc = new Processor(true);
                XsltCompiler comp = proc.NewXsltCompiler();
                StringReader sr = new StringReader("<xslt:transform xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                                                   "                xmlns:my=\"http://ns.example.com/strip-type\"\n" +
                                                   "                xmlns:xslt=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                                                   "                exclude-result-prefixes=\"xs my\"\n" +
                                                   "                version=\"3.0\">\n" +
                                                   "   <xslt:strip-space elements='foo my:foo'/>\n" +
                                                   "   <xslt:mode on-no-match='deep-copy'/>\n" +
                                                   "</xslt:transform>");

                XsltExecutable exec = comp.Compile(sr);
                Xslt30Transformer xsl = exec.Load30();
                String inn = "<?xml version='1.0'?> \n" +
                             "<my:userNode xmlns:my='http://ns.example.com/strip-type'><foo> </foo><bar> </bar><my:foo> </my:foo><my:bar> </my:bar></my:userNode>";

                DocumentBuilder builder = proc.NewDocumentBuilder();
                builder.BaseUri = new Uri("http://www.example.com");
                MemoryStream sDoc = new MemoryStream(Encoding.UTF8.GetBytes(inn));
                XdmNode doc = builder.Build(sDoc);

                StringWriter writer = new StringWriter();
                Serializer outi = proc.NewSerializer(writer);
                xsl.ApplyTemplates(doc, outi);
                Assert.True(writer.ToString().Contains("<foo/>"), "strip-space not working");
                Assert.True(writer.ToString().Contains("<my:foo/>"), "strip-space not working");
                Assert.False(writer.ToString().Contains("<bar/>"), "strip-space not working");
                Assert.False(writer.ToString().Contains("<my:bar/>"), "strip-space not working");
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestBug5341() {
            string xslt = @"
                <xsl:stylesheet 
	                xmlns:xsl='http://www.w3.org/1999/XSL/Transform' 
                    version='3.0'>

	                <xsl:template match = '/'>
                        <xsl:sequence select = ""substring-after(/eventItem/contentMeta/scheduled/start, 'T')""/>
	                </xsl:template>
                </xsl:stylesheet>";

            Processor processor = new Processor(true);
            XsltCompiler xsltCompiler = processor.NewXsltCompiler();
            xsltCompiler.XsltLanguageVersion = "3.0";
            var rdr = new StringReader(xslt);
            XsltExecutable exec = xsltCompiler.Compile(rdr);
            Xslt30Transformer transf = exec.Load30();
            StringWriter sw = new();
            Serializer ser = processor.NewSerializer(sw);
            var stream = File.OpenRead(ConfigTest.DATA_DIR + "cirkev.xml");

            transf.ApplyTemplates(stream, ser);
            Assert.AreEqual("<?xml version=\"1.0\" encoding=\"UTF-8\"?>10:00", sw.ToString());
        }
        
        
    }
}