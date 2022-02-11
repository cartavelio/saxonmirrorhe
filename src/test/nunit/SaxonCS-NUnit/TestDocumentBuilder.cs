using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;
using Saxon.Api;
using NUnit.Framework;
using System.Collections;



namespace SaxonCS_NUnit
{

    /**
        * NUnit tests for the s9api DocumentBuilder
    */
    class TestDocumentBuilder
    {

        [Test]
        public void TestSaxSourceDTDValidation()
        {
            String validXML = "<!DOCTYPE a [<!ELEMENT a EMPTY>]><a/>";
            String invalidXML = "<!DOCTYPE a [<!ELEMENT a (b+)>]><a/>";
            DocumentBuilder builder = new Processor(false).NewDocumentBuilder();
            builder.BaseUri = new Uri("http://www.example.com");
            builder.DtdValidation = true;

            StringReader sr = new StringReader(validXML);
           
            try
            {
                XdmNode d = builder.Build(sr);
                Assert.NotNull(d);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }

            sr = new StringReader(invalidXML);
         
            try
            {
                XdmNode d = builder.Build(sr);
                Assert.Fail("invalid document parsed without error");
            }
            catch (Exception e)
            {
                // expected failure;
            }

            builder.DtdValidation = false;
            sr = new StringReader(invalidXML);
            try
            {
                XdmNode d = builder.Build(sr);
                Assert.NotNull(d);
            }
            catch (Exception e)
            {
                Assert.Fail("Parsing failed but validation was not requested: " + e.Message);
            }

        }

        [Test]
        public void TestQNameValue() {
            try {
                var path = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, System.AppDomain.CurrentDomain.RelativeSearchPath ?? "");

                // Create a Processor instance.
                Processor processor = new Saxon.Api.Processor(new StreamReader(ConfigTest.NUNIT_DATA_DIR + @"\saxon.config").BaseStream);
                Assert.NotNull(processor);
            }
            catch (Exception ex) {
                Assert.Fail(ex.Message);
            }
            }
        [Test]
        public void TesStringReaderSource()
        {
            try
            {
                StringReader sr = new StringReader("<doc/>");
                DocumentBuilder builder = new Processor(false).NewDocumentBuilder();
                builder.BaseUri = new Uri("http://www.example.com");
                XdmNode d = builder.Build(sr);
                Assert.NotNull(d);
            }
            catch (javax.xml.transform.TransformerException e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TesXmlReaderSource()
        {
            try
            {
                XmlReader xml = XmlReader.Create(new StringReader("<doc/>"));
                DocumentBuilder builder = new Processor(false).NewDocumentBuilder();
                builder.BaseUri = new Uri("http://www.example.com");
                XdmNode d = builder.Build(xml);
                Assert.NotNull(d);
            }
            catch (javax.xml.transform.TransformerException e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestNamespaceNodes()
        {
            try
            {
                StringReader sr = new StringReader("<doc xmlns='abc.uri'><e xmlns:a='one.uri' xmlns:b='two.uri'/></doc>");
                XmlReader xml = XmlReader.Create(sr);
                Processor processor = new Processor(false);
                DocumentBuilder builder = processor.NewDocumentBuilder();
                XdmNode d = builder.Build(xml);
                Assert.NotNull(d);
                XPathSelector xp = processor.NewXPathCompiler().Compile("/*/*").Load();
                xp.ContextItem = d;
                XdmNode e = (XdmNode)xp.EvaluateSingle();
                Assert.NotNull(e);
                IEnumerator nsIter = e.EnumerateAxis(XdmAxis.Namespace);
                Dictionary<String, String> nsBindings = new Dictionary<String, String>();
                while (nsIter.MoveNext())
                {
                    XdmNode ns = (XdmNode)nsIter.Current;
                    Console.WriteLine(ns.ToString());
                    nsBindings.Add((ns.NodeName == null ? "" : ns.NodeName.LocalName), ns.StringValue);
                }
                Assert.True(nsBindings.Count() == 4);


            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestDOMElementNode()
        {
            try
            {

                String sr = "<?xml version='1.0'?> \n" +
                        "<Document> <e> <f/><f/> </e> </Document>";

                // Read an XML file to a DOM
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(sr);

                Processor proc = new Processor(false);
                XsltCompiler comp = proc.NewXsltCompiler();
                XsltExecutable exec = comp.Compile(new Uri(ConfigTest.NUNIT_DATA_DIR + "identity.xsl"));
                XsltTransformer xsl = exec.Load();
                DocumentBuilder builder = proc.NewDocumentBuilder();
                XdmNode node = builder.Build(xml);
                xsl.InitialContextNode = node;
                StringWriter writer = new StringWriter();
                Serializer outi = proc.NewSerializer(writer);

                xsl.Run(outi);
                Assert.True(writer.ToString().Contains("<f/>"), "");
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }


        /**
         * Test that when a document is loaded from multiple external entities, the base URIs of
         * elements are preserved
        */
        [Test]
        public void TestBaseUriFromEntities()
        {
            try
            {

                Processor processor = new Processor(false);
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.DtdProcessing = DtdProcessing.Parse;
                var source = File.OpenRead(ConfigTest.RESOURCES_DIR + "book-wrapper.xml");

                DocumentBuilder builder = processor.NewDocumentBuilder();

                builder.BaseUri = new Uri(ConfigTest.RESOURCES_DIR + "book-wrapper.xml");
                XdmNode d = builder.Build(source);
                XPathCompiler xpc = processor.NewXPathCompiler();
                XPathSelector s = xpc.Compile("distinct-values(//*/base-uri())").Load();
                s.ContextItem = d;
                XdmValue v = s.Evaluate();
                Assert.True(v.Count == 3, "3 distinct base URIs");
            }
            catch (javax.xml.transform.TransformerException e)
            {
                Assert.Fail(e.Message);
            }
        }

        //loads input xml file and returns XDM Node
        static XdmNode GetXdmNode(Uri inputFile)
        {
            Processor processor = new Processor();
            Uri storeInputFileForComparison = inputFile;

            //regular node creation with passed uri - throws MalformedURIException as already reported
            XdmNode inputXdmNode = processor.NewDocumentBuilder().Build(inputFile);

            inputFile = new Uri("file:///" + ConfigTest.NUNIT_DATA_DIR + "\\test test\\books.xml");

            //new node creation with overwritten uri object with the same value - throws also MalformedURIException
            XdmNode nodeWithOverwrittenInputFile = processor.NewDocumentBuilder().Build(inputFile);

            //compare new created uri with passed one:
            bool equal1 = storeInputFileForComparison == inputFile;         //returns true
            bool equal2 = storeInputFileForComparison.Equals(inputFile);    //returns also true

            Uri newInputUriEscaped = new Uri("file:///" + ConfigTest.NUNIT_DATA_DIR + "\\test test\\books.xml", true);

            bool equal3 = storeInputFileForComparison.Equals(newInputUriEscaped);    //returns true

            //new node creation with new uri object with the same value - throws also MalformedURIException
            XdmNode nodeWithUnescapedNewInputUri = processor.NewDocumentBuilder().Build(newInputUriEscaped);

            Uri newInputUriUnescaped = new Uri("file:///" + ConfigTest.NUNIT_DATA_DIR + "\\test test\\books.xml", false);

            bool equal4 = storeInputFileForComparison.Equals(newInputUriUnescaped);    //returns true

            //new node creation with new uri object with the same value - throws also MalformedURIException
            XdmNode nodeWithNewInputUri = processor.NewDocumentBuilder().Build(newInputUriUnescaped);

            Uri twoSlashesInsteadOfAtSign = new Uri("file:///" + ConfigTest.NUNIT_DATA_DIR + "\\test test\\books.xml");

            bool equal5 = storeInputFileForComparison.Equals(twoSlashesInsteadOfAtSign);    //returns true

            //throws MalformedURIException
            XdmNode nodeWithTwoSlashesInsteadOfAtSign = processor.NewDocumentBuilder().Build(twoSlashesInsteadOfAtSign);

            Uri usingFileScheme = new Uri("file:///" + ConfigTest.NUNIT_DATA_DIR + "\\test test\\books.xml");

            //throws MalformedURIException
            XdmNode nodeWithFileScheme = processor.NewDocumentBuilder().Build(usingFileScheme);

            Uri differentPath = new Uri("file:///" + ConfigTest.NUNIT_DATA_DIR + "\\test test\\Test Test.xml");

            bool equal6 = storeInputFileForComparison.Equals(differentPath);    //returns false due to different path

            //throws MalformedURIException
            XdmNode nodeWithDifferentPath = processor.NewDocumentBuilder().Build(differentPath);

            Uri letsCheckIfUriIsValidForDotNet;
            if (Uri.TryCreate("file:///" + ConfigTest.NUNIT_DATA_DIR + "test test\\books.xml", UriKind.Absolute, out letsCheckIfUriIsValidForDotNet)) //returns true
            {
                // the url is valid
            }

            Uri newUri = new Uri("file:///" + ConfigTest.NUNIT_DATA_DIR + "\\Test.xml");   //HostType is also "IPv6HostType | UncHostType" 

            //throws MalformedURIException
            return processor.NewDocumentBuilder().Build(inputFile);
        }

        [Test]
        public void TestUriWithSpace2()
        {
             var uriBuilder = new UriBuilder(Uri.EscapeUriString("file:///" + ConfigTest.DATA_DIR2 + "test test//test test.xml"));

            Uri uri = uriBuilder.Uri;// new Uri(uriBuilder);
            GetXdmNode(uri);
        }

        [Test]
        public void TestUriWithSpace()
        {
            try
            {

                Processor processor = new Processor();
                var uriBuilder = new UriBuilder("file:///"+ConfigTest.DATA_DIR + "book-wrapper.xml");

                Uri uri = uriBuilder.Uri;// new Uri("file:///C:/test test/books.xml");

                XdmNode node = processor.NewDocumentBuilder().Build(uri);


                XPathCompiler xpc = processor.NewXPathCompiler();
                XPathSelector s = xpc.Compile("distinct-values(//*/base-uri())").Load();
                s.ContextItem = node;
                XdmValue v = s.Evaluate();
                Assert.True(v.Count == 3, "3 distinct base URIs");
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }


        /**
    * Test that when a document with multiple base URIs is copied, the base URIs of
    * elements are preserved
         * This is for Saxon 9.6. at present there is not a Build method that can handle a Saxon.Api.XdmNode
    */
        // [Test]
        public void TestBaseUriFromCopy()
        {

            try {
                Processor processor = new Processor(false);
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.DtdProcessing = DtdProcessing.Parse;
                var source = File.OpenRead(ConfigTest.RESOURCES_DIR + "book-wrapper.xml");
                DocumentBuilder builder = processor.NewDocumentBuilder();
                builder.TreeModel = TreeModel.TinyTree;
                XdmNode d = builder.Build(source);
                builder.TreeModel = TreeModel.LinkedTree;
                XPathCompiler xpc = processor.NewXPathCompiler();
                XPathSelector s = xpc.Compile("distinct-values(//*/base-uri())").Load();
                s.ContextItem = d;
                XdmValue v = s.Evaluate();
                Assert.True(v.Count == 3, "3 distinct base URIs");
            } catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
 }


        /**
            * Test that when a document is loaded from multiple external entities, the base URIs of
            * elements are preserved
            */
        [Test]
        public void TestBaseUriOfAttribute()
        {
            try
            {
                Processor processor = new Processor(false);
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.DtdProcessing = DtdProcessing.Parse;
                var source = File.OpenRead(ConfigTest.RESOURCES_DIR + "book-wrapper.xml");
                DocumentBuilder builder = processor.NewDocumentBuilder();
                builder.BaseUri = new Uri(ConfigTest.RESOURCES_DIR + "book-wrapper.xml");
                XdmNode d = builder.Build(source);
                XPathCompiler xpc = processor.NewXPathCompiler();
                XPathSelector s = xpc.Compile("//@CODE").Load();
                s.ContextItem = d;
                foreach (XdmItem i in s.Evaluate())
                {
                    Assert.True(((XdmNode)i).BaseUri.ToString().EndsWith("categories.xml"), "baseURI");
                }
            }
            catch (javax.xml.transform.TransformerException e)
            {
                Assert.Fail(e.Message);
            }
        }

      


        /**
        * Test document builder with document projection
         * This test case will be for Saxon 9.6
        */
        [Test]
        public void TestProjection001()
        {
            try
            {

                Processor processor = new Processor(true);
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.DtdProcessing = DtdProcessing.Parse;
                var source = File.OpenRead(ConfigTest.RESOURCES_DIR + "book-wrapper.xml");

                XQueryCompiler xpc = processor.NewXQueryCompiler();
                XQueryExecutable exp = xpc.Compile("<out>{//CATEGORY}</out>");

                DocumentBuilder builder = processor.NewDocumentBuilder();
                builder.BaseUri = new Uri(ConfigTest.RESOURCES_DIR + "book-wrapper.xml");
                builder.DocumentProjectionQuery = exp;
                builder.TreeModel = TreeModel.TinyTree;
                XdmNode doc = builder.Build(source);


                // Test that the projected doc contains the CATEGORY elements

                StringWriter sw = new StringWriter();
                Serializer destination = processor.NewSerializer();
                destination.SetOutputWriter(sw);
                XQueryEvaluator eval = exp.Load();
                eval.ContextItem = doc;
                eval.Run(destination);

                Assert.True(sw.ToString().Contains("CODE=\"P\" DESC=\"Paperback\""), "contains CAT");

                // Test that the projected doc omits the ITEM elements

                exp = xpc.Compile("count(//ITEM)");
                StringWriter sw2 = new StringWriter();
                Serializer destination2 = processor.NewSerializer();
                destination2.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");
                destination2.SetOutputWriter(sw2);
                XQueryEvaluator eval2 = exp.Load();
                eval2.ContextItem = doc;
                eval2.Run(destination2);
                Assert.True(sw2.ToString().Equals("0"), "omits ITEM");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                Assert.Fail(ex.Message);
            }
        }


        [Test]
        public void TestSvg11Dtd()
        {
            // Test that the SVG 1.1 DTD can be retrieved from the local cache
            try
            {
                Processor proc = new Processor(false);
                proc.SetProperty("http://saxon.sf.net/feature/timing", "true");
                DocumentBuilder b = proc.NewDocumentBuilder();
                b.DtdValidation = (true);
                StringReader sr = new StringReader(
                        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                            "<!-- Generated by graphviz version 2.28.0 (20110901.0847)\n" +
                            " --><!-- Title: g Pages: 1 --><!DOCTYPE svg\n" +
                            "  PUBLIC \"-//W3C//DTD SVG 1.1//EN\" \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\">\n" +
                            "<svg xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\"\n" +
                            "     width=\"206pt\"\n" +
                            "     height=\"98pt\"\n" +
                            "     viewBox=\"0.00pt 0.00pt 206.00pt 98.00pt\"\n" +
                            "     preserveAspectRatio=\"xMidYMid meet\"\n" +
                            "     zoomAndPan=\"magnify\"\n" +
                            "     version=\"1.1\"\n" +
                            "     contentScriptType=\"application/ecmascript\"\n" +
                            "     contentStyleType=\"text/css\">\n" +
                            "   <g id=\"graph1\" class=\"graph\" transform=\"scale(1 1) rotate(0) translate(4 94)\">\n" +
                            "      <title>g</title>\n" +
                            "      <polygon fill=\"white\" stroke=\"white\" points=\"-4,5 -4,-94 203,-94 203,5 -4,5\"/>\n" +
                            "      <!-- d7e7880 --><g id=\"node2\" class=\"node\">\n" +
                            "         <title>d7e7880</title>\n" +
                            "         <polygon fill=\"none\" stroke=\"black\" points=\"54,-63 0,-63 0,-27 54,-27 54,-63\"/>\n" +
                            "         <text text-anchor=\"middle\" x=\"27\" y=\"-40.4\" font-family=\"Times,serif\"\n" +
                            "               font-size=\"14.00\">A</text>\n" +
                            "      </g>\n" +
                            "   </g>\n" +
                            "</svg>");
                //StreamSource ss = new StreamSource(sr, "http://www.saxonica.com/notthere");
                var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                writer.Write(sr);
                writer.Flush();
                stream.Position = 0;
                b.BaseUri = new Uri("http://www.saxonica.com/notthere");
                b.Build(stream);
                Assert.True(true, "svg11Dtd successful");
            }
            catch (Exception sae)
            {
                Assert.Fail(sae.Message);
            }
        }

        [Test]
        public void TestSvg10Dtd()
        {
            // Test that the SVG 1.0 DTD can be retrieved from the local cache
            try
            {
                Processor proc = new Processor(false);
                proc.SetProperty("http://saxon.sf.net/feature/timing", "true");
                DocumentBuilder b = proc.NewDocumentBuilder();
                b.DtdValidation = true;
                StringReader sr = new StringReader(
                        "<?xml version='1.0' encoding='UTF-8' ?>"+
"<!DOCTYPE svg PUBLIC '-//W3C//DTD SVG 1.0//EN' 'http://www.w3.org/TR/2001/REC-SVG-20010904/DTD/svg10.dtd'>" + Environment.NewLine +
"<svg xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink' width='206pt'" + Environment.NewLine +
 "   height ='98pt' viewBox='0.00pt 0.00pt 206.00pt 98.00pt' preserveAspectRatio='xMidYMid meet'" + Environment.NewLine +
  "  zoomAndPan ='magnify' version='1.0' contentScriptType='application/ecmascript'" + Environment.NewLine +
  "  contentStyleType ='text/css'>" + Environment.NewLine +
" <g id='graph1' class='graph' transform='scale(1 1) rotate(0) translate(4 94)'>" + Environment.NewLine +
 "       <title>g</title>" + Environment.NewLine +
 "       <polygon fill='white' stroke='white' points='-4,5 -4,-94 203,-94 203,5 -4,5'/>" + Environment.NewLine +
 "       <!-- d7e7880 -->" + Environment.NewLine +
 "       <g id='node2' class='node'>" + Environment.NewLine +
 "           <title>d7e7880</title>" + Environment.NewLine +
 "           <polygon fill='none' stroke='black' points='54,-63 0,-63 0,-27 54,-27 54,-63'/>" + Environment.NewLine +
 "           <text text-anchor='middle' x='27' y='-40.4' font-family='Times,serif' font-size='14.00'" + Environment.NewLine +
 "               >A</text>" + Environment.NewLine +
 "       </g>" + Environment.NewLine +
 "   </g>" + Environment.NewLine +
"</svg>");
                //StreamSource ss = new StreamSource(sr, "http://www.saxonica.com/notthere");
                var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                writer.Write(sr);
                writer.Flush();
                stream.Position = 0;
                b.BaseUri = new Uri("http://www.saxonica.com/notthere");
                b.Build(stream);
                Assert.True(true, "svg10Dtd successful");

            }
            catch (Exception sae)
            {
                Assert.Fail(sae.Message);
            }
        }


        [Test]
        public void TestWhitespacePolicyUntyped()
        {
            try
            {
                Processor proc = new Processor(false);
                StringReader sr = new StringReader("<doc> </doc>");
                DocumentBuilder builder = proc.NewDocumentBuilder();
                builder.BaseUri = new Uri("http://www.example.com");
                //SAXSource source = new SAXSource(new InputSource(sr));
                builder.WhitespacePolicy = WhitespacePolicy.StripAll;
                String s = builder.Build(sr).ToString();
                Assert.True(s.Contains("<doc/>"));

                sr = new StringReader("<doc> </doc>");
                //source = new SAXSource(new InputSource(sr));
                builder.WhitespacePolicy = WhitespacePolicy.PreserveAll;
                s = builder.Build(sr).ToString();
                Assert.True(s.Contains("<doc> </doc"));

                sr = new StringReader("<doc> </doc>");
                //source = new SAXSource(new InputSource(sr));
                builder.WhitespacePolicy = WhitespacePolicy.StripIgnorable;
                s = builder.Build(sr).ToString();
                Assert.True(s.Contains("<doc> </doc"));

            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestWhitespacePolicySchemaTyped() {
            String s;
            s = buildWithWhitespacePolicyTyped(WhitespacePolicy.StripIgnorable);
            Assert.True(s.Contains("<BOOKS><ITEM"));
            s = buildWithWhitespacePolicyTyped(WhitespacePolicy.PreserveAll);
            Assert.True(s.StartsWith("rejected"));
            s = buildWithWhitespacePolicyTyped(WhitespacePolicy.StripAll);
            Assert.True(s.StartsWith("rejected"));

        }

        private String buildWithWhitespacePolicyTyped(WhitespacePolicy policy)
        {
            try
            {
                Processor proc = new Processor(true);

                SchemaManager mgr = proc.SchemaManager;
                mgr.Compile(new XmlTextReader(ConfigTest.DATA_DIR + "books.xsd"));
                SchemaValidator v = mgr.NewSchemaValidator();

                Serializer serializer = proc.NewSerializer();
                serializer.SetOutputProperty(Serializer.METHOD, "xml");
                serializer.SetOutputProperty(Serializer.INDENT, "no");
                StringWriter sw = new StringWriter();

                DocumentBuilder builder = proc.NewDocumentBuilder();
                builder.SchemaValidator = v;
                    builder.WhitespacePolicy = policy;
                XdmNode doc = builder.Build(new XmlTextReader(ConfigTest.DATA_DIR + "books.xml"));
                serializer.SetOutputWriter(sw);
                serializer.SerializeXdmValue(doc);
                return sw.ToString();

            }
            catch (Exception e)
            {
                return "rejected: " + e.Message;
            }
        }

        [Test]
        public void TestWhitespacePolicyWithDTDValidation()
        {
            String s;
            s = useDTDIgnorableWhitespace(WhitespacePolicy.StripAll);
            Assert.True(s.StartsWith("rejected"));
            s = useDTDIgnorableWhitespace(WhitespacePolicy.StripIgnorable);
            Assert.True(s.Contains("<a><b/><c> </c></a>"));
            s = useDTDIgnorableWhitespace(WhitespacePolicy.PreserveAll);
            Assert.True(s.StartsWith("rejected"));
        }

        private String useDTDIgnorableWhitespace(WhitespacePolicy policy)
        {
            String validXML = "<!DOCTYPE a [<!ELEMENT a ((b|c)*)><!ELEMENT b EMPTY><!ELEMENT c (#PCDATA)>]><a> <b/> <c> </c> </a>";
            Processor proc = new Processor(false);
            DocumentBuilder builder = new Processor(false).NewDocumentBuilder();
            builder.BaseUri = new Uri("http://example.com");
            builder.WhitespacePolicy = policy;
            builder.DtdValidation = true;

            StringReader sr = new StringReader(validXML);
      
            try
            {
                XdmNode d = builder.Build(sr);

                Serializer serializer = proc.NewSerializer();
                serializer.SetOutputProperty(Serializer.METHOD, "xml");
                serializer.SetOutputProperty(Serializer.INDENT, "no");
                StringWriter sw = new StringWriter();
                serializer.SetOutputWriter(sw);
                serializer.SerializeXdmValue(d);
                return sw.ToString();
            }
            catch (Exception e)
            {
                return "rejected: " + e.Message;
            }


        }

        internal static bool LocalNameEquals(QName value) {

            return value.LocalName.Equals("doc") || value.LocalName.Equals("i"); ;
        }

        [Test]
        public void TestCustomWhitespacePolicy()
        {
            try
            {
                Processor proc = new Processor(false);
                DocumentBuilder builder = proc.NewDocumentBuilder();
                builder.BaseUri = new Uri("http://example.com");
                builder.WhitespacePolicy = WhitespacePolicy.StripAll;
                StringReader xml = new StringReader("<doc><b> </b> <i> </i> </doc>");
                builder.WhitespacePolicy = WhitespacePolicy.MakeCustomPolicy(LocalNameEquals);


                XdmNode node = builder.Build(xml);



                Serializer ss = proc.NewSerializer();
                ss.SetOutputProperty(Serializer.METHOD, "xml");
                ss.SetOutputProperty(Serializer.INDENT, "no");
                StringWriter writer = new StringWriter();
                ss.SetOutputWriter(writer);
                ss.SerializeXdmValue(node);
                Assert.True(writer.ToString().Contains("<doc><b> </b><i/></doc>"));

            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public void TestProcInstructions()
        {
            try
            {
                var doc = new XmlDocument();
                var pi = doc.CreateProcessingInstruction("foo", "bar");
                var builder = new Saxon.Api.Processor().NewDocumentBuilder();
                var xdmNode = builder.Build(pi);
                Console.WriteLine(xdmNode.NodeKind);
                IEnumerator enumer = xdmNode.EnumerateAxis(XdmAxis.Descendant);
                enumer.MoveNext();
                while (enumer.Current != null)
                {
                    XdmNode child = (XdmNode)enumer.Current;
                    enumer.MoveNext();

                    if (child.NodeKind == XmlNodeType.ProcessingInstruction)
                    {
                        Assert.True(true, "Expect processingInstruction");
                        return;
                    }
                }
                // Assert.True(child.NodeKind == XmlNodeType.ProcessingInstruction, "Expect processingInstruction");
                Assert.Fail("Failed");
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public void TestAttribute()
        {
            try
            {
                var doc = new XmlDocument();
                var elem = doc.CreateElement("element1");
                var attr = doc.CreateAttribute("foo");
                attr.Value = "bar";
                
                elem.SetAttributeNode(attr);
                var builder = new Processor().NewDocumentBuilder();

                XdmNode xdmNode = builder.Build(elem);
                Console.WriteLine(xdmNode.NodeKind);
                IEnumerator enumer = xdmNode.EnumerateAxis(XdmAxis.Descendant);
                enumer.MoveNext();
                while (enumer.Current != null)
                {
                    XdmNode child = (XdmNode)enumer.Current;
                    enumer.MoveNext();

                    if (child.NodeKind == XmlNodeType.Element)
                    {
                        if(child.GetAttributeValue(new QName("foo")).Equals("bar"))
                        Assert.True(true, "Expect Attribute");
                        return;
                    }
                }
                // Assert.True(child.NodeKind == XmlNodeType.ProcessingInstruction, "Expect processingInstruction");
                Assert.Fail("Failed");
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }


        public XsltExecutable GetExecutable(string stylesheetPath)
        {
            using (FileStream s = new FileStream(stylesheetPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var processor = new Processor();
                var compiler = processor.NewXsltCompiler();
                compiler.BaseUri = new Uri(stylesheetPath);
                var executable = compiler.Compile(s);
                return executable;
            }
        }

        public class MyStream : MemoryStream {

            public override void Close() {
                base.Close();
            }
        }


        [Test]
        public void GetXslt2FO()
        {
            try {
                XmlDocument xml = new XmlDocument();
                XmlNode rootNode = xml.CreateElement("users");
                xml.AppendChild(rootNode);

                XmlNode userNode = xml.CreateElement("user");
                XmlAttribute attribute = xml.CreateAttribute("age");
                attribute.Value = "42";
                userNode.Attributes.Append(attribute);
                userNode.InnerText = "John Doe";
                rootNode.AppendChild(userNode);

               
                Dictionary<string, string> args = new Dictionary<string, string>();

                string xsltFilePath = ConfigTest.DATA_DIR + "main3.xsl";
                // var xsltStep1 = c.Compile(new Uri(ConfigTest.DATA_DIR + "echo_given_sourcefile.xsl"));
                // var xsltStep2 = c.Compile(new Uri(ConfigTest.DATA_DIR + "print_elements.xsl"));

                // Compile stylesheet
                var executable = GetExecutable(xsltFilePath);
                var declaredArgs = executable.GetGlobalParameters();

                // Do transformation to a destination
                var destination = new DomDestination();
                using (var inputStream = new MyStream())
                {
                    xml.Save(inputStream);
                    inputStream.Position = 0;
                    var transformer = executable.Load();
                    FileInfo stylesheetFileInfo = new FileInfo(xsltFilePath);
                    transformer.SetInputStream(inputStream, new Uri(stylesheetFileInfo.DirectoryName));
                    foreach (var arg in args)
                    {
                        var matchingArgDeclaration = declaredArgs.FirstOrDefault(a => a.Key.LocalName.ToLower() == arg.Key.ToLower());
                        if (matchingArgDeclaration.Key == null)
                        {
                            transformer.SetParameter(new QName(arg.Key), new XdmAtomicValue(arg.Value));
                        }
                        else
                        {
                            // try to load up the specific parameter type
                            // list is based on http://www.saxonica.com/documentation/#!dotnetdoc/Saxon.Api/XdmAtomicValue
                            XdmItemType argType = matchingArgDeclaration.Value.GetDeclaredItemType();
                            var argTypeName = "";
                            if (argType != null &&
                                argType is XdmAtomicType)
                                argTypeName = ((XdmAtomicType)argType).Name.LocalName;
                            switch (argTypeName.ToLower())
                            {
                                case "boolean":
                                    transformer.SetParameter(new QName(arg.Key), new XdmAtomicValue(Convert.ToBoolean(arg.Value)));
                                    break;

                                case "integer":
                                    transformer.SetParameter(new QName(arg.Key), new XdmAtomicValue(Convert.ToInt32(arg.Value)));
                                    break;

                                case "decimal":
                                    transformer.SetParameter(new QName(arg.Key), new XdmAtomicValue(Convert.ToDecimal(arg.Value)));
                                    break;

                                case "float":
                                    transformer.SetParameter(new QName(arg.Key), new XdmAtomicValue(Convert.ToSingle(arg.Value)));
                                    break;

                                case "double":
                                    transformer.SetParameter(new QName(arg.Key), new XdmAtomicValue(Convert.ToDouble(arg.Value)));
                                    break;

                                case "anyuri":
                                    transformer.SetParameter(new QName(arg.Key), new XdmAtomicValue(new Uri(arg.Value)));
                                    break;

                                case "qname":
                                    transformer.SetParameter(new QName(arg.Key), new XdmAtomicValue(new QName(arg.Value)));
                                    break;

                                default:
                                    transformer.SetParameter(new QName(arg.Key), new XdmAtomicValue(((arg.Value))));
                                    break;
                            }
                        }
                    }

                    transformer.Run(destination);
                }
                Console.WriteLine(destination.XmlDocument.OuterXml);
                Assert.True(destination.XmlDocument.OuterXml is string);

            } catch (Exception ex) {
                Console.WriteLine(ex.StackTrace);
                Assert.Fail(ex.Message);
            }
        }
    }
}


