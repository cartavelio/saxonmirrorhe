using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using NUnit.Framework;
using Saxon.Api;
using Saxon.Ejavax.xml.transform;
using Saxon.Hej.om;
using Saxon.Hej.tree.util;
using Saxon.Hej.tree.wrapper;
using DocumentBuilder = Saxon.Api.DocumentBuilder;
using Exception = System.Exception;
using Processor = Saxon.Api.Processor;
using SchemaManager = Saxon.Api.SchemaManager;
using Serializer = Saxon.Api.Serializer;
using XPathCompiler = Saxon.Api.XPathCompiler;
using XPathSelector = Saxon.Api.XPathSelector;
using XQueryCompiler = Saxon.Api.XQueryCompiler;
using XQueryEvaluator = Saxon.Api.XQueryEvaluator;
using Xslt30Transformer = Saxon.Api.Xslt30Transformer;
using XsltExecutable = Saxon.Api.XsltExecutable;

namespace SaxonCS_NUnit {
    public class TestDom {

        private void CopyDomNodeToDestination(Processor proc, XmlDocument node, IDestination destination) {
            XQueryEvaluator copy = proc.NewXQueryCompiler().Compile(".").Load();
            copy.ContextItem = proc.NewDocumentBuilder().Wrap(node);
            copy.Run(destination);
        }

        private Xslt30Transformer IdentityTransformer(Processor proc) {
            string identityXslt =
                "<transform xmlns='http://www.w3.org/1999/XSL/Transform' version='3.0'><mode on-no-match='shallow-copy'/></transform>";
            return proc.NewXsltCompiler().Compile(new StringReader(identityXslt)).Load30();
        }

        /**
     * Test that namespaces are copied OK by an identity transform
     */

        [Test]
        public void testCopyNamespaces() {
            try {
                string input = "<?xml version='1.0'?> \n" +
                               "<XmlDocument xmlns='http://lost.namespace.com/' \n" +
                               "xmlns:xsi='http://www.namespacexsi.com' " +
                               "xsi:value='some-value'/>";

                // Read an XML file to a DOM
                Processor proc = new();
                DocumentBuilder builder = proc.NewDocumentBuilder();
                XmlDocument doc = new();
                doc.Load(new StringReader(input));
                XdmNode xdmNode = builder.Wrap(doc);

                // Output a DOM to an XML file
                StringWriter sw = new();
                CopyDomNodeToDestination(proc, doc, proc.NewSerializer(sw));
                //System.err.println(sw.ToString());
                Assert.That(sw.ToString().Contains("http://lost.namespace.com/"), "Namespace is present in output");
                Assert.That(sw.ToString().Contains("xmlns:xsi="), "Namespace is present in output");
                Assert.That(sw.ToString().Contains("xsi:value"), "Attribute is present in output");
            }
            catch (Exception e) {
                Console.Error.WriteLine(e.StackTrace);
                Assert.Fail(e.Message);
            }
        }


        [Test]
        public void testDescendantAxis() {
            // Bug 4683
            try {
                
                string xml = "<root>\n"
                             + "  <item/>\n"
                             + "  <item>\n"
                             + "    <foo id=\"foo1\"/>\n"
                             + "  </item>\n"
                             + "  <item></item>\n"
                             + "  <item>\n"
                             + "    <foo id=\"foo2\"/>\n"
                             + "  </item>\n"
                             + "</root>";

                // Read an XML file to a DOM
                Processor proc = new();
                DocumentBuilder builder = proc.NewDocumentBuilder();
                XmlDocument doc = new();
                doc.Load(new StringReader(xml));
                XdmNode xdmNode = builder.Wrap(doc);

                string stylesheet =
                    "<xsl:transform version=\"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n"
                    + "  xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" exclude-result-prefixes=\"#all\">\n"
                    + "\n"
                    + "  <xsl:param name=\"DEBUG\" static=\"yes\" as=\"xs:boolean\" select=\"true()\"/>\n"
                    + "\n"
                    + "  <xsl:output method=\"text\" item-separator=\"&#10;\"/>\n"
                    + "\n"
                    + "  <xsl:template match=\"/\">\n"
                    + "    <xsl:for-each select=\"//item\">\n"
                    + "      <xsl:assert test=\"not(*) eq not(descendant::foo)\" use-when=\"$DEBUG\">No child elements but descendants</xsl:assert>\n"
                    + "      <xsl:value-of select=\"position(), not(*), descendant::foo/@id\"/>\n"
                    + "    </xsl:for-each>\n"
                    + "  </xsl:template>\n"
                    + "\n"
                    + "</xsl:transform>";

                XsltExecutable templates = proc.NewXsltCompiler().Compile(new StringReader(stylesheet));
                Xslt30Transformer transformer = templates.Load30();
                StringWriter sw = new StringWriter();
                transformer.ApplyTemplates(proc.NewDocumentBuilder().Wrap(doc), proc.NewSerializer(sw));
                //System.err.println(sw.ToString());
                Assert.That(sw.ToString().Contains("1 true\n"
                                                   + "2 false foo1\n"
                                                   + "3 true\n"
                                                   + "4 false foo2"));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        /**
         * Get the string value of a document node
         */

        [Test]
        public void testStringValueOfDocHE() {
            testStringValueOfDoc();
        }


        private void testStringValueOfDoc() {
            try {
                string xml = "<!DOCTYPE test [\n" +
                             " <!ENTITY omega 'theta'>\n" +
                             "]>\n" +
                             "<test>[&omega;]</test>";

                // Read an XML file to a DOM
                Processor proc = new();
                DocumentBuilder builder = proc.NewDocumentBuilder();
                XmlDocument doc = new();
                doc.Load(new StringReader(xml));
                XdmNode xdmNode = builder.Wrap(doc);

                string stylesheet =
                    "<?xml version=\"1.0\"?> \n" +
                    "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                    "<xsl:template match='/'>\n" +
                    "<out><xsl:value-of select='.'/></out>\n" +
                    "</xsl:template>\n" +
                    "</xsl:stylesheet>";

                XsltExecutable templates = proc.NewXsltCompiler().Compile(new StringReader(stylesheet));
                Xslt30Transformer transformer = templates.Load30();
                StringWriter sw = new ();
                transformer.ApplyTemplates(xdmNode, proc.NewSerializer(sw));
                //System.err.println(sw.ToString());
                Assert.That(sw.ToString().Contains("<out>[theta]</out>"));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        /**
         * Test names of wrapped DOM nodes (and ability to select them...)
         */

        [Test]
        public void testNodeNames() {
            string xml = "<root xmlns='http://default.ns/'>"
                         + "  <?pi data?>"
                         + "  <!-- comment -->"
                         + "  <item/>"
                         + "  <item>"
                         + "    <x:foo xmlns:x='http://x.ns/' name='foo1' xml:id=\"foo1\"/>\n"
                         + "  </item>"
                         + "  <item></item>"
                         + "  <item>"
                         + "    <foo id=\"foo2\"/>"
                         + "  </item>"
                         + "  <textItem>text</textItem>"
                         + "  <cdataItem><![CDATA[cdata]]></cdataItem>"
                         + "</root>";

            void Check(XdmNode node, string prefix, string uri, string local) {
                Assert.AreEqual( prefix,node.NodeName == null ? "" : node.NodeName.Prefix);
                Assert.AreEqual(local, node.NodeName == null ? "" : node.NodeName.LocalName);
                Assert.AreEqual(uri, node.NodeName == null ? "" : node.NodeName.Uri);
            }
            
            Processor proc = new();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XmlDocument doc = new();
            doc.Load(new StringReader(xml));
            XdmNode docNode = builder.Wrap(doc);
            
            Check(docNode, "", "", "");
            XdmNode root = docNode.Select(Steps.Child("*")).First();
            Check(root, "", "http://default.ns/", "root");
            XdmNode comment = root.Select(Steps.Child(Predicates.IsComment())).First();
            Check(comment, "", "", "");
            XdmNode pi = root.Select(Steps.Child(Predicates.IsProcessingInstruction())).First();
            Check(pi, "", "", "pi");
            XdmNode item = root.Select(Steps.Child("item")).First();
            Check(item, "", "http://default.ns/", "item");
            XdmNode foo = root.Select(Steps.Descendant("foo")).First();
            Check(foo, "x", "http://x.ns/", "foo");
            XdmNode name = foo.Select(Steps.Attribute("name")).First();
            Check(name, "", "", "name");
            XdmNode xmlId = foo.Select(Steps.Attribute("id")).First();
            Check(xmlId, "xml", NamespaceConstant.XML, "id");
            XdmNode textItemElement = root.Select(Steps.Descendant("textItem")).First();
            XdmNode textItem = textItemElement.Select(Steps.Child(Predicates.IsText())).First();
            Check(textItem, "", "", "");
            XdmNode cdataItem = root.Select(Steps.Descendant("cdataItem")).First()
                .Select(Steps.Child(Predicates.IsText())).First();
            Check(cdataItem, "", "", "");
        }

        /**
         * Test calling an extension function when the stylesheet has stripped whitespace (virtually)
         */

        [Test]
        public void testStripSpaceHE() {
            testStripSpace();
        }


        private void testStripSpace() {
            try {
                string xml = "<?xml version='1.0'?> \n" +
                             "<XmlDocument> <e> <f/><f/> </e> </XmlDocument>";

                // Read an XML file to a DOM
                Processor proc = new();
                DocumentBuilder builder = proc.NewDocumentBuilder();
                XmlDocument doc = new();
                doc.Load(new StringReader(xml));
                XdmNode xdmNode = builder.Wrap(doc);

                XdmItemType showDomType = proc.ParseItemType("function(node()*) as xs:anyAtomicType*");
                proc.RegisterExtensionFunction(new QName("java:jaxptest.DOMTest", "showDomNodes"), showDomType, args => {
                    System.Collections.Generic.List<XdmAtomicValue> result = new();
                    foreach (XdmItem item in args[0]) {
                        XmlElement domElement = (XmlElement) ((XdmNode) item).GetUnderlyingXmlNode();
                        result.Add(new XdmAtomicValue("[" + domElement.Name + "]"));
                    }

                    return new XdmValue(result);
                });

                // Create stylesheet with call to extension function

                string stylesheet =
                    "<?xml version=\"1.0\"?> \n" +
                    "<xsl:stylesheet version=\"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                    "<xsl:strip-space elements='*'/>" +
                    "<xsl:template match='/'><out>\n" +
                    "<xsl:value-of select='ext:showDomNodes(//f)' xmlns:ext='java:jaxptest.DOMTest'/>\n" +
                    "<xsl:value-of select='ext:showDomNodes((//e)[1])' xmlns:ext='java:jaxptest.DOMTest'/>\n" +
                    "<xsl:apply-templates/>\n" +
                    "</out></xsl:template>\n" +
                    "<xsl:template match=\"@*|node()\">\n" +
                    "<xsl:copy>\n" +
                    "  <xsl:apply-templates select=\"@*|node()\" /> \n" +
                    "</xsl:copy>\n" +
                    "</xsl:template>\n" +
                    "</xsl:stylesheet>";

                XsltExecutable templates = proc.NewXsltCompiler().Compile(new StringReader(stylesheet));
                Xslt30Transformer transformer = templates.Load30();
                StringWriter sw = new ();
                transformer.ApplyTemplates(xdmNode, proc.NewSerializer(sw));
                Console.Error.WriteLine(sw.ToString());
                Assert.That(sw.ToString().Contains("[f] [f][e]"), "Extension function not called");
            }
            catch (Exception e) {
                Console.Error.WriteLine(e.StackTrace);
                Assert.Fail(e.Message);
            }
        }

        /*
         * Method to be called from previous test
         */

        // public static string showDomNodes(NodeList nodes) {
        //     StringBuilder sb = new StringBuilder(20);
        //     for (int i = 0; i < nodes.getLength(); i++) {
        //         sb.append('[').append(nodes.item(i).getNodeName()).append(']');
        //     }
        //     return sb.ToString();
        // }

        /*
         * Method to be called from previous test
         */

        // public static string showDomNode(Node node) {
        //     return "{" + node.getNodeName() + "}";
        // }

        /**
         * Test Configuration.buildDocument given a DOMSource that wraps an XmlElement node
         */

        [Test]
        public void testBuildFromDomElement() {
            try {
                string xml = "<?xml version='1.0'?> \n" +
                             "<XmlDocument> <e> <f/><f/> </e> </XmlDocument>";

                // Read an XML file to a DOM
                Processor proc = new();
                DocumentBuilder builder = proc.NewDocumentBuilder();
                XmlDocument doc = new();
                doc.Load(new StringReader(xml));
                XdmNode xdmNode = builder.Wrap(doc);

                XmlElement ele = doc.DocumentElement;
                NodeInfo nodeInfo = xdmNode.Implementation;
                Assert.That(nodeInfo is VirtualNode, "not-copied");
            }
            catch (Exception e) {
                //e.printStackTrace();
                Assert.Fail(e.Message);
            }
        }

        /**
         * Test that Saxon treats a text node with "null" content as if it had zero-length content
         */

        [Test]
        public void testNullTextNodes() {
            try {
                Processor proc = new();
                XmlDocument doc = new();
                XmlElement top = doc.CreateElement("top");
                doc.AppendChild(top);
                XmlText text = doc.CreateTextNode("data");
                /////////////////////////
                text.Data = null;
                /////////////////////////
                top.AppendChild(text);
                XdmNode wrapper = proc.NewDocumentBuilder().Wrap(doc);
                StringWriter sw = new StringWriter();
                Serializer serializer = proc.NewSerializer(sw);
                serializer.SerializeXdmNode(wrapper);
                Assert.AreEqual("<?xml version=\"1.0\" encoding=\"UTF-8\"?><top/>", sw.ToString(),
                    "wrong output " + sw);
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }


        // [Test]
        // public void testNullTextNodesDomino() {
        //     try {
        //         Configuration config = new EnterpriseConfiguration();
        //         XmlDocument doc = makeDocWithNullTextNode();
        //         NodeInfo wrapper = DominoTree.makeTree(doc, config, "http://base.uri/").getRootNode();
        //         try (StringWriter sw = new StringWriter()) {
        //             Receiver output = config.getSerializerFactory().getReceiver(new StreamResult(sw));
        //             wrapper.copy(output, CopyOptions.ALL_NAMESPACES | CopyOptions.TYPE_ANNOTATIONS, Loc.NONE);
        //             Assert.AreEqual("<?xml version=\"1.0\" encoding=\"UTF-8\"?><top/>", sw.ToString(), "wrong output " + sw);
        //         }
        //     } catch (ParserConfigurationException | XPathException e) {
        //         Assert.Fail(e.getMessage());
        //     } catch (Exception err) {
        //         err.printStackTrace();
        //         Assert.Fail(err.getMessage());
        //     }
        // }



        // [Test]
        // public void testAdjacentTextNodesWithDomino() {
        //     try {
        //         Configuration config = new EnterpriseConfiguration();
        //         XmlDocument doc = makeDocWithAdjacentTextNodes(false);
        //         NodeInfo wrapper = DominoTree.makeTree(doc, config, "http://base.uri/").getRootNode();
        //         XdmNode docNode = new XdmNode(wrapper);
        //         navigateOverAdjacentTextNodes(docNode);
        //
        //     } catch (ParserConfigurationException | XPathException e) {
        //         Assert.Faile.getMessage());
        //     } catch (Exception err) {
        //         err.printStackTrace();
        //         Assert.Failerr.getMessage());
        //     }
        // }

        [Test]
        public void testAdjacentTextNodesWithPlainDomWrapper() {
            try {
                Processor proc = new();
                XmlDocument doc = makeDocWithAdjacentTextNodes(false);
                XdmNode docNode = proc.NewDocumentBuilder().Wrap(doc);
                NavigateOverAdjacentTextNodes(docNode);

            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        private void NavigateOverAdjacentTextNodes(XdmNode docNode) {
            XdmNode top = docNode.Select(Steps.Child()).First();

            long count = top.Select(Steps.Child()).Count();
            Assert.AreEqual(1, count, "t0");
            XdmNode text = top.Select(Steps.Child()).First();
            Assert.AreEqual("text1text2", text.StringValue, "t1");

            count = docNode.Select(Steps.Descendant()).Count();
            Assert.AreEqual(2, count, "t2");
            text = docNode.Select(Steps.Descendant(Predicates.IsText())).First();
            Assert.AreEqual("text1text2", text.StringValue, "t3");

            count = top.Select(Steps.Descendant()).Count();
            Assert.AreEqual(1, count, "t4");
            text = top.Select(Steps.Descendant()).First();
            Assert.AreEqual("text1text2", text.StringValue, "t5");

        }

        private void navigateOverAdjacentTextNodesMixed(XdmNode docNode) {
            XdmNode top = docNode.Select(Steps.Child()).First();

            long count = top.Select(Steps.Child()).Count();
            Assert.AreEqual(3, count);
            XdmNode text = top.Select(Steps.Child(Predicates.IsText())).First();
            Assert.AreEqual("text1text2", text.StringValue, "t1");

            count = docNode.Select(Steps.Descendant()).Count();
            Assert.AreEqual(4, count);
            text = docNode.Select(Steps.Descendant(Predicates.IsText())).First();
            Assert.AreEqual("text1text2", text.StringValue, "t2");

            count = top.Select(Steps.Descendant()).Count();
            Assert.AreEqual(3, count);
            text = top.Select(Steps.Descendant(Predicates.IsText())).First();
            Assert.AreEqual("text1text2", text.StringValue, "t3");

            XdmNode alpha = text.Select(Steps.PrecedingSibling()).First();
            Assert.AreEqual("alpha", alpha.NodeName.LocalName, "t4");

            XdmNode omega = text.Select(Steps.FollowingSibling()).First();
            Assert.AreEqual("omega", omega.NodeName.LocalName, "t5");

            XdmNode preOmega = omega.Select(Steps.PrecedingSibling()).First();
            Assert.AreEqual("text1text2", preOmega.StringValue, "t6");

            XdmNode prePreOmega = preOmega.Select(Steps.PrecedingSibling()).First();
            Assert.AreEqual("alpha", prePreOmega.NodeName.LocalName, "t7");

            NodeInfo omegaInfo = omega.Implementation;
            if (omegaInfo is SteppingNode omegaNode) {
                NodeInfo preOmegaInfo = omegaNode.getPreviousSibling();
                Assert.AreEqual("text1text2", preOmegaInfo.getStringValue(), "t8");
                NodeInfo prePreOmegaInfo = ((SteppingNode) preOmegaInfo).getPreviousSibling();
                Assert.AreEqual("alpha", prePreOmegaInfo.getLocalPart(), "t9");
            }

        }

        // [Test]
        // public void testAdjacentTextNodesWithDominoMixed() {
        //     try {
        //         Configuration config = new EnterpriseConfiguration();
        //         XmlDocument doc = makeDocWithAdjacentTextNodes(true);
        //         NodeInfo wrapper = DominoTree.makeTree(doc, config, "http://base.uri/").getRootNode();
        //         XdmNode docNode = new XdmNode(wrapper);
        //         navigateOverAdjacentTextNodesMixed(docNode);
        //
        //     } catch (ParserConfigurationException | XPathException e) {
        //         Assert.Faile.getMessage());
        //     } catch (Exception err) {
        //         err.printStackTrace();
        //         Assert.Failerr.getMessage());
        //     }
        // }

        [Test]
        public void testAdjacentTextNodesWithPlainDomWrapperMixed() {
            try {
                Processor proc = new();
                XmlDocument doc = makeDocWithAdjacentTextNodes(true);
                XdmNode docNode = proc.NewDocumentBuilder().Wrap(doc);
                navigateOverAdjacentTextNodesMixed(docNode);

            }
            catch (Exception e) {
                Console.Error.WriteLine(e.StackTrace);
                Assert.Fail(e.Message);
            }
        }


        // [Test]
        // public void testAdjacentTextNodesDomino() {
        //     try {
        //         Configuration config = new EnterpriseConfiguration();
        //         XmlDocument doc = makeDocWithAdjacentTextNodes(false);
        //         NodeInfo wrapper = DominoTree.makeTree(doc, config, "http://base.uri/").getRootNode();
        //         StringWriter sw = new StringWriter();
        //         Receiver out = config.getSerializerFactory().getReceiver(new StreamResult(sw));
        //         wrapper.copy(out, CopyOptions.ALL_NAMESPACES | CopyOptions.TYPE_ANNOTATIONS, Loc.NONE);
        //         Assert.AreEqual("<?xml version=\"1.0\" encoding=\"UTF-8\"?><top>text1text2</top>", sw.ToString(), "wrong output " + sw);
        //         XPathFactoryImpl xf = new net.sf.saxon.xpath.XPathFactoryImpl();
        //         xf.setConfiguration(config);
        //         XPath xp = xf.newXPath();
        //         string result = xp.evaluate("count(.//text())", wrapper);
        //         Assert.AreEqual("1", result, "number of text nodes ");
        //     } catch (ParserConfigurationException | XPathException e) {
        //         Assert.Faile.getMessage());
        //     } catch (Exception err) {
        //         err.printStackTrace();
        //         Assert.Failerr.getMessage());
        //     }
        // }

        private XmlDocument makeDocWithAdjacentTextNodes(bool mixed) {
            XmlDocument doc = new();
            XmlElement top = doc.CreateElement("top");
            doc.AppendChild(top);
            if (mixed) {
                XmlElement alpha = doc.CreateElement("alpha");
                top.AppendChild(alpha);
            }

            XmlText text1 = doc.CreateTextNode("text1");
            top.AppendChild(text1);
            XmlText text2 = doc.CreateTextNode("text2");
            top.AppendChild(text2);
            if (mixed) {
                XmlElement omega = doc.CreateElement("omega");
                top.AppendChild(omega);
            }

            return doc;
        }


        /**
         * Test JAXP XPath API delivering a DOM NodeList given a DOM Node as input
         */

        [Test]
        public void testXPathNodeList() {
            try {
                Processor proc = new();
                string xml = "<?xml version='1.0'?> \n" +
                             "<XmlDocument><e/><f/><g/></XmlDocument>";
                XmlDocument doc = new();
                doc.Load(new MemoryStream(new UTF8Encoding().GetBytes(xml)));
                XPathSelector xpe = proc.NewXPathCompiler().Compile("/*/*").Load();
                xpe.ContextItem = proc.NewDocumentBuilder().Wrap(doc);
                XdmValue isList = xpe.Evaluate();
                Assert.AreEqual(3, isList.Count(), "length");
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        /**
     * Test JAXP XPath API delivering a DOM NodeList given a DOMSource as input
     */

        // [Test]
        // public void testXPathNodeListFromDOMSource() {
        //     try {
        //         string in = "<?xml version='1.0'?> \n" +
        //                 "<XmlDocument><e/><f/><g/></XmlDocument>";
        //         DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
        //         factory.setNamespaceAware(true);
        //         DocumentBuilder db = factory.newDocumentBuilder();
        //         InputSource is = new InputSource(new StringReader(in));
        //         XmlDocument doc = db.parse(is);
        //         DOMSource source = proc.NewDocumentBuilder().Wrap(doc);
        //         XPathEvaluator xpe = new XPathEvaluator();
        //         NodeList isList = (NodeList) xpe.evaluate("/*/*", source, XPathConstants.NODESET);
        //         Assert.AreEqual(3, isList.getLength(), "length");
        //     } catch (ParserConfigurationException | SAXException | IOException | XPathExpressionException e) {
        //         Assert.Faile.getMessage());
        //     }
        // }


        [Test]
        public void testNodeListFromXsltExtension() {
            string stylesheet =
                "<?xml version=\"1.0\"?> \n" +
                "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                "<xsl:strip-space elements='*'/>" +
                "<xsl:template match='/'>\n" +
                "<a><xsl:value-of select=\"count(ext:makeElementList('r', 'e', 5))\" xmlns:ext='java:jaxptest.DOMTest'/></a>\n" +
                "<b><xsl:value-of select=\"count(ext:makeElementList('r', 'e', 5)/*)\" xmlns:ext='java:jaxptest.DOMTest'/></b>\n" +
                "<c><xsl:value-of select=\"count(ext:makeElementList('r', 'e', 5)/node())\" xmlns:ext='java:jaxptest.DOMTest'/></c>\n" +
                "</xsl:template>\n" +
                "</xsl:stylesheet>";

            try {
                Processor proc = new();
                DocumentBuilder builder = proc.NewDocumentBuilder();
                XmlDocument doc = new();
                XdmNode wrappedDoc = builder.Wrap(doc);
                XdmItemType makeElementListType =
                    proc.ParseItemType("function(xs:string, xs:string, xs:integer) as node()*");
                proc.RegisterExtensionFunction(new QName("java:jaxptest.DOMTest", "makeElementList"), 
                        makeElementListType, args => {
                    long repeat = ((XdmAtomicValue) args[2]).AsLong();
                    XdmValue result = XdmEmptySequence.Instance;
                    for (int i = 0; i < repeat; i++) {
                        XmlElement e = doc.CreateElement(((XdmAtomicValue) args[0]).StringValue);
                        result = result.Append(builder.Wrap(wrappedDoc, e));
                    }

                    return result;
                });
                XsltExecutable templates = proc.NewXsltCompiler().Compile(new StringReader(stylesheet));
                Xslt30Transformer transformer = templates.Load30();
                XdmNode source = proc.NewDocumentBuilder().Build(new StringReader("<doc/>"));
                StringWriter sw = new StringWriter();
                transformer.ApplyTemplates(source, proc.NewSerializer(sw));
                Assert.That(sw.ToString().Contains("<a>5</a><b>0</b><c>0</c>"), sw.ToString());
            }
            catch (TransformerException e) {
                Assert.Fail(e.Message);
                //e.printStackTrace();
            }

        }


        /**
         * Test showing a DOM NodeList returned by an extension function
         */

        [Test]
        public void testXPathNodeListFromExtensionFunction() {
            try {
                Processor proc = new();
                XdmItemType sortType = proc.ParseItemType("function(item()*) as item()*");
                proc.RegisterExtensionFunction(
                        new QName("java:jaxptest.DOMTest","sortArrayToNodeList"),
                        sortType, 
                        args => {
                            XdmItem[] array = args[0].ToArray();
                            Array.Sort(array, Comparer<XdmItem>.Create((x,y) => 
                                 StringComparer.InvariantCulture.Compare(x.StringValue, y.StringValue)));
                            return new XdmValue(array);
                        });
                const string xml = "<?xml version='1.0'?> \n" +
                                   "<books><book>fred</book><book>jane</book><book>anne</book><book>sue</book></books>";
                XmlDocument doc = new();
                doc.Load(new StringReader(xml));
                string exp = "ext:sortArrayToNodeList(//book)";
                XPathCompiler xpc = proc.NewXPathCompiler();
                xpc.DeclareNamespace("ext", "java:jaxptest.DOMTest");
                XPathSelector xpe = xpc.Compile(exp).Load();
                xpe.ContextItem = proc.NewDocumentBuilder().Wrap(doc);
                XdmValue isList = xpe.Evaluate();
                Assert.AreEqual(4, isList.Count, "length");
                Assert.AreEqual("anne", isList[0].StringValue);
                Assert.AreEqual("fred", isList[1].StringValue);
                Assert.AreEqual("jane", isList[2].StringValue);
                Assert.AreEqual("sue", isList[3].StringValue);
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
                //e.printStackTrace();
            }
        }

        // /**
        //  * Variant of above test using an extension function that returns a NodeList
        //  * implemented using the JAXP FunctionResolver
        //  */
        //
        // [Test]
        // public void testXPathNodeListFromJaxpExtensionFunction() {
        //     try {
        //         string in = "<?xml version='1.0'?> \n" +
        //                 "<books><book>fred</book><book>jane</book><book>anne</book><book>sue</book></books>";
        //         DocumentBuilderFactory factory =
        //                 DocumentBuilderFactory.newInstance();
        //         factory.setNamespaceAware(true);
        //         DocumentBuilder db = factory.newDocumentBuilder();
        //         InputSource is = new InputSource(new StringReader(in));
        //         XmlDocument doc = db.parse(is);
        //         XPath xpath = getXPath();
        //         NodeList isList = (NodeList) xpath.evaluate(
        //                 "ext:sortArrayToSequence(//book)", doc, XPathConstants.NODESET);
        //         Assert.AreEqual(4, isList.getLength(), "length");
        //
        //         isList = (NodeList) xpath.evaluate("ext:sortArrayToNodeList(//book)", doc, XPathConstants.NODESET);
        //
        //         Assert.AreEqual(4, isList.getLength(), "length");
        //
        //     } catch (ParserConfigurationException | SAXException | IOException e) {
        //         Assert.Faile.getMessage());
        //     } catch (XPathExpressionException e) {
        //         Assert.Faile.getMessage());
        //         e.printStackTrace();
        //     }
        // }

        // public XPath getXPath() {
        //     XPath xpath = new XPathEvaluator();
        //     xpath.setNamespaceContext(new NamespaceContext() {
        //         @Override
        //         public string getNamespaceURI(string prefix) {
        //             return prefix.equals("ext") ? "http://extString" : null;
        //         }
        //
        //         @Override
        //         public string getPrefix(string namespaceURI) {
        //             return null;
        //         }
        //
        //         @Override
        //         public Iterator getPrefixes(string namespaceURI) {
        //             return null;
        //         }
        //     });
        //
        //     xpath.setXPathFunctionResolver((fname, arity) -> {
        //         if (fname == null) {
        //             throw new NullPointerException("The function name cannot be null.");
        //         }
        //         try {
        //             Class cls = Class.forName("jaxptest.DOMTest");
        //             return new XPathFunctionImpl(cls, fname.getLocalPart());
        //         } catch (Exception e) {
        //             Assert.Faile.getMessage());
        //         }
        //         return null;
        //     });
        //     return xpath;
        // }




        [Test]
        public void testTopLevelWhitespaceText() {
            // effect of adding top-level whitespace text node
            string stylesheet =
                "<?xml version=\"1.0\"?> \n" +
                "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                "<xsl:template match='/'>\n" +
                "<a/>\n" +
                "<xsl:text>   </xsl:text>" +
                "</xsl:template>\n" +
                "</xsl:stylesheet>";

            try {
                Processor proc = new Processor();
                XsltExecutable templates = proc.NewXsltCompiler().Compile(new StringReader(stylesheet));
                Xslt30Transformer transformer = templates.Load30();
                StringReader source = new StringReader("<doc/>");
                XmlDocument document = new();
                transformer.ApplyTemplates(source, new DomDestination(document));
                Assert.AreEqual("a", document.DocumentElement.LocalName, "result");
            }
            catch (TransformerException e) {
                Assert.Fail(e.getMessage());
                //e.printStackTrace();
            }

        }

        [Test]
        public void testTopLevelNonWhitespaceText() {
            // effect of adding top-level whitespace text node
            string stylesheet =
                "<?xml version=\"1.0\"?> \n" +
                "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                "<xsl:template match='/'>\n" +
                "<a/>\n" +
                "<xsl:text>foo</xsl:text>" +
                "</xsl:template>\n" +
                "</xsl:stylesheet>";

            try {
                Processor proc = new Processor();
                XsltExecutable templates = proc.NewXsltCompiler().Compile(new StringReader(stylesheet));
                Xslt30Transformer transformer = templates.Load30();
                XmlDocument document = new();
                transformer.ApplyTemplates(new StringReader("<doc/>"), new DomDestination(document));
                Assert.Fail("error not detected");
            }
            catch (SaxonApiException e) {
                Assert.That(e.Message.Contains("the specified node is the wrong type"));
            }
            catch (Exception e) {
                Console.Error.WriteLine(e.StackTrace);
                Assert.Fail(e.Message);
            }

        }

        [Test]
        public void testIdentityTransformOfDOMTextNode() {
            XmlDocument document = new();
            XmlElement spanElement = document.CreateElement("span");
            document.AppendChild(spanElement);
            XmlText textNode = document.CreateTextNode("some text");
            spanElement.AppendChild(textNode);
            XmlComment commentNode = document.CreateComment("a comment");
            spanElement.AppendChild(commentNode);
            XmlProcessingInstruction piNode = document.CreateProcessingInstruction("tgt", "some data");
            spanElement.AppendChild(piNode);

            Processor proc = new();
            Xslt30Transformer transformer = IdentityTransformer(proc);
            StringWriter sw = new StringWriter();
            Serializer serializer = proc.NewSerializer(sw);
            serializer.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");
            serializer.SetOutputProperty(Serializer.METHOD, "xml");

            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode wrappedDoc = builder.Wrap(document);
            transformer.ApplyTemplates(builder.Wrap(wrappedDoc, textNode), serializer);
            Assert.AreEqual("some text", sw.ToString(), "text node");

            sw = new StringWriter();
            serializer.OutputWriter = sw;
            transformer.ApplyTemplates(builder.Wrap(wrappedDoc, commentNode), serializer);
            Assert.AreEqual("<!--a comment-->", sw.ToString(), "comment");

            sw = new StringWriter();
            serializer.OutputWriter = sw;
            transformer.ApplyTemplates(builder.Wrap(wrappedDoc, piNode), serializer);
            Assert.AreEqual("<?tgt some data?>", sw.ToString(), "processing instruction");
        }

        [Test]
        public void testIdentityTransformOfEmptyDOMSource() {
            XmlDocument document = new();

            Processor proc = new();

            Xslt30Transformer transformer = IdentityTransformer(proc);
            StringWriter sw = new StringWriter();
            Serializer serializer = proc.NewSerializer(sw);
            serializer.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");
            serializer.SetOutputProperty(Serializer.METHOD, "xml");

            transformer.ApplyTemplates(proc.NewDocumentBuilder().Wrap(document), serializer);
            Assert.AreEqual("", sw.ToString());
        }

        /**
     * Test case(s) from T-Mobile, 8 April 2010: validate an element in a DOM that is not the root node
     */

        // [Test]
        // public void testElementValidationYN() {
        //     tryElementValidation(true, false);
        // }

        [Test]
        public void testElementValidationYY() {
            tryElementValidation(true, true);
        }

        // [Test]
        // public void testElementValidationNN() {
        //     tryElementValidation(false, false);
        // }

        [Test]
        public void testElementValidationNY() {
            tryElementValidation(false, true);
        }

        private void tryElementValidation(bool withPrefix, bool namespaceAware) {

            try {
                string prefixXml = "<n0:bar xmlns:n0=\"http://bar\">\n" +
                                   "    <foo xmlns=\"http://foo\"></foo>\n" +
                                   "</n0:bar>";

                string noPrefixXml = "<bar xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n" +
                                     " xsi:schemaLocation=\"http://bar bar.xsd\"\n" +
                                     " xmlns=\"http://bar\">\n" +
                                     "    <foo xmlns=\"http://foo\"></foo>\n" +
                                     "</bar>";

                string barXsd = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                                "<schema \n" +
                                "    xmlns:foo=\"http://foo\"\n" +
                                "    xmlns:bar=\"http://bar\"\n" +
                                "    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                                "    xmlns=\"http://www.w3.org/2001/XMLSchema\"\n" +
                                "    attributeFormDefault=\"unqualified\"\n" +
                                "    blockDefault=\"\"\n" +
                                "    elementFormDefault=\"qualified\"\n" +
                                "    finalDefault=\"\"\n" +
                                "    targetNamespace=\"http://bar\"\n" +
                                "    version=\"0102\">\n" +
                                "\n" +
                                "    <xs:import namespace=\"http://foo\"/>\n" +
                                "\n" +
                                "\n" +
                                "    <xs:complexType abstract=\"false\" mixed=\"false\" name=\"Bar\">\n" +
                                "        <xs:sequence maxOccurs=\"1\" minOccurs=\"1\">\n" +
                                "            <xs:element maxOccurs=\"1\" minOccurs=\"0\" ref=\"foo:foo\">\n" +
                                "            </xs:element>\n" +
                                "        </xs:sequence>\n" +
                                "    </xs:complexType>\n" +
                                "    \n" +
                                "    <xs:element abstract=\"false\" name=\"bar\" type=\"bar:Bar\"/>\n" +
                                "    \n" +
                                "</schema>        ";

                string fooXsd = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                                "<schema \n" +
                                "    xmlns:foo=\"http://foo\"\n" +
                                "    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                                "    xmlns=\"http://www.w3.org/2001/XMLSchema\"\n" +
                                "    attributeFormDefault=\"unqualified\"\n" +
                                "    blockDefault=\"\"\n" +
                                "    elementFormDefault=\"qualified\"\n" +
                                "    finalDefault=\"\"\n" +
                                "    targetNamespace=\"http://foo\"\n" +
                                "    version=\"0102\">\n" +
                                "\n" +
                                "    <xs:complexType abstract=\"false\" mixed=\"false\" name=\"Foo\">\n" +
                                "    </xs:complexType>\n" +
                                "    \n" +
                                "    <xs:element abstract=\"false\" name=\"foo\" type=\"foo:Foo\"/>\n" +
                                "    \n" +
                                "</schema>        ";


                Processor proc = new();
                SchemaManager schema = proc.SchemaManager;
                DocumentBuilder builder = proc.NewDocumentBuilder();

                schema.Compile(new StringReader(fooXsd));
                schema.Compile(new StringReader(barXsd));

                XQueryCompiler ctx = proc.NewXQueryCompiler();
                XQueryEvaluator xq = ctx.Compile("validate{.}").Load();




                // Process XML source that uses namespace prefixes
                XmlDocument doc = new XmlDocument();
                doc.Load(new StringReader(withPrefix ? prefixXml : noPrefixXml));

                XmlNode node = doc.DocumentElement.GetElementsByTagName("foo")[0];
                if (namespaceAware) {
                    Assert.AreEqual("http://foo", node.NamespaceURI);
                }
                else {
                    Assert.Null(node.NamespaceURI);
                }

                xq.ContextItem = builder.Wrap(builder.Wrap(doc), node);

                XdmValue result = xq.Evaluate();
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual("http://foo", ((XdmNode) result[0]).NodeName.Uri);


            }
            catch (Exception e) {
                //e.printStackTrace();
                Assert.Fail();
            }
        }


        [Test]
        public void testElementCopyYY() {
            tryElementCopy(true, true);
        }

        // [Test]
        // //@Tag("expectedtofail")
        // public void testElementCopyNN() {
        //     // MHK 2020-03-13: this is failing on 10.0. The call on getNamespaceURI() on the 'foo' element returns
        //     // "", not the actual namespace http://foo. I think this is OK: we don't actually support a non-namespace-aware
        //     // DOM, and we can't detect that the DOM is non-namespace-aware.
        //     tryElementCopy(false, false);
        // }

        [Test]
        public void testElementCopyNY() {
            tryElementCopy(false, true);
        }

        private void tryElementCopy(bool withPrefix, bool namespaceAware) {

            try {
                Processor proc = new();
                string prefixXml = "<n0:bar xmlns:n0=\"http://bar\">\n" +
                                   "    <foo xmlns=\"http://foo\" a='1' n0:b='2'><zing/><n0:zang/></foo>\n" +
                                   "</n0:bar>";

                string noPrefixXml = "<bar xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n" +
                                     " xsi:schemaLocation=\"http://bar bar.xsd\"\n" +
                                     " xmlns=\"http://bar\">\n" +
                                     "    <foo xmlns=\"http://foo\" a='1' xsi:b='2'><zing/><xml:zang/></foo>\n" +
                                     "</bar>";



                // Process XML source
                XmlDocument doc = new XmlDocument();
                doc.Load(new StringReader(withPrefix ? prefixXml : noPrefixXml));

                XmlNode node = doc.DocumentElement.GetElementsByTagName("foo")[0];
                if (namespaceAware) {
                    Assert.AreEqual("http://foo", node.NamespaceURI);
                }
                else {
                    Assert.Null(node.NamespaceURI);
                }

                Xslt30Transformer t = IdentityTransformer(proc);
                DomDestination result = new DomDestination();
                DocumentBuilder builder = proc.NewDocumentBuilder();
                XdmNode source = builder.Wrap(builder.Wrap(doc), node);
                t.ApplyTemplates(source, result);

                XdmNode wrappedResultDoc = builder.Wrap(result.XmlDocument);
                XdmNode wrappedDocumentElement = builder.Wrap(wrappedResultDoc, result.XmlDocument.DocumentElement);

                XdmFunctionItem deepEqual = proc.GetSystemFunction(
                    new QName("http://www.w3.org/2005/xpath-functions", "deep-equal"), 2);
                XdmValue comparison = deepEqual.Invoke(new XdmValue[] {source, wrappedDocumentElement}, proc);
                Assert.That(((XdmAtomicValue) comparison).AsBoolean(), "Copy is not deep-equal");

            }
            catch (Exception e) {
                Console.Error.WriteLine(e.Message + "\n" + e.StackTrace);
                Assert.Fail();
            }
        }


        // [Test]
        // public void testTypedDom() {
        //     try {
        //         DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
        //         factory.setNamespaceAware(true);
        //         DocumentBuilder domBuilder = factory.newDocumentBuilder();
        //
        //         // Process XML source that uses namespace prefixes
        //         XmlDocument doc = domBuilder.parse(new InputSource(new FileInputStream(ConfigTest.DATA_DIR + "books.xml")));
        //
        //         SchemaFactory sf = null; //SchemaFactory.newInstance(XMLConstants.W3C_XML_SCHEMA_NS_URI);
        //         sf = new SchemaFactoryImpl();
        //         Schema s = sf.newSchema(new File(ConfigTest.DATA_DIR + "books.xsd"));
        //         Validator v = s.newValidator();
        //         DomDestination r = new DomDestination();
        //         v.validate(proc.NewDocumentBuilder().Wrap(doc), r);
        //         XmlDocument d2 = (XmlDocument) r.getNode();
        //         XmlElement e = d2.DocumentElement;
        //         TypeInfo ti = e.getSchemaTypeInfo();
        //         Assert.Null(ti.getTypeName(), "TypeInfo null");
        //         //Console.WriteLine(ti.getTypeName());
        //         // This doesn't actually work. Saxon has no way of setting the type info in the dom, because there
        //         // is no setTypeInfo() method.
        //
        //     } catch (ParserConfigurationException | IOException | SAXException e1) {
        //         Assert.Faile1.getMessage());
        //         e1.printStackTrace();
        //     }
        // }

        // [Test]
        // public void testTypedDom11() {
        //     try {
        //         DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
        //         factory.setNamespaceAware(true);
        //         DocumentBuilder domBuilder = factory.newDocumentBuilder();
        //
        //         // Process XML source that uses namespace prefixes
        //         XmlDocument doc = new XmlDocument();
        //         doc.Load(new WebClient().OpenRead(new Uri(ConfigTest.DataDirUri, "books.xml")));
        //         domBuilder.parse(new InputSource(new FileInputStream(ConfigTest.DATA_DIR + "books.xml")));
        //
        //         SchemaFactory sf = null; //SchemaFactory.newInstance(XMLConstants.W3C_XML_SCHEMA_NS_URI);
        //         sf = new SchemaFactoryImpl();
        //         sf.setProperty(FeatureKeys.XSD_VERSION, "1.1");
        //         Schema s = sf.newSchema(new File(ConfigTest.DATA_DIR + "books11.xsd"));
        //         Validator v = s.newValidator();
        //         DomDestination r = new DomDestination();
        //         v.validate(proc.NewDocumentBuilder().Wrap(doc), r);
        //         XmlDocument d2 = (XmlDocument) r.getNode();
        //         XmlElement e = d2.DocumentElement;
        //         TypeInfo ti = e.getSchemaTypeInfo();
        //         Assert.Null(ti.getTypeName(), "TypeInfo null");
        //         // This doesn't actually work. Saxon has no way of setting the type info in the dom, because there
        //         // is no setTypeInfo() method.
        //
        //     } catch (ParserConfigurationException | IOException | SAXException e1) {
        //         Assert.Faile1.getMessage());
        //         e1.printStackTrace();
        //     }
        // }

        // [Test]
        // public void testReusableValidator() {
        //     try {
        //         DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
        //         factory.setNamespaceAware(true);
        //         DocumentBuilder domBuilder = factory.newDocumentBuilder();
        //
        //
        //         XmlDocument doc = domBuilder.parse(new InputSource(new FileInputStream(ConfigTest.DATA_DIR + "books.xml")));
        //
        //         SchemaFactory sf = null; //SchemaFactory.newInstance(XMLConstants.W3C_XML_SCHEMA_NS_URI);
        //         sf = new SchemaFactoryImpl();
        //         Schema s = sf.newSchema(new File(ConfigTest.DATA_DIR + "books.xsd"));
        //         Validator v = s.newValidator();
        //         DomDestination r = new DomDestination();
        //         v.validate(proc.NewDocumentBuilder().Wrap(doc), r);
        //         XmlDocument d2 = (XmlDocument) r.getNode();
        //         XmlElement e = d2.DocumentElement;
        //
        //         DomDestination r2 = new DomDestination();
        //         v.validate(proc.NewDocumentBuilder().Wrap(e), r2);
        //
        //     } catch (ParserConfigurationException | IOException | SAXException e1) {
        //         Assert.Faile1.getMessage());
        //         e1.printStackTrace();
        //     }
        // }


        [Test]
        public void testNullNamespace() {
            try {
                Processor proc = new Processor();
                string xml =

                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                    "<root xmlns=\"abc\">" +
                    "<name xmlns=\"\">" +
                    "<first>Joe</first>" +
                    "<last>Blow</last>" +
                    "</name>" +
                    "</root>";

                string path =
                    "/*[local-name(.)=\"root\" and namespace-uri(.)=\"abc\"]" +
                    "/*[local-name(.)=\"name\" and namespace-uri(.)=\"\"]" +
                    "/*[local-name(.)=\"first\" and namespace-uri(.)=\"\"]/text()";


                DocumentBuilder builder = proc.NewDocumentBuilder();
                XmlDocument doc = new XmlDocument();
                doc.Load(new StringReader(xml));
                XdmNode xdmNode = builder.Wrap(doc);

                XPathCompiler xPath = proc.NewXPathCompiler();
                XPathSelector xp = xPath.Compile(path).Load();
                xp.ContextItem = xdmNode;
                XdmValue result = xp.Evaluate();

                Console.WriteLine(result.Count);
                Assert.AreEqual(1, result.Count);
                for (int i = 0; i < result.Count; i++) {
                    XdmNode n = (XdmNode) result[i];
                    Console.WriteLine(n);
                }


            }
            catch (Exception err) {
                Console.Error.WriteLine(err.StackTrace);
                Assert.Fail(err.Message);
            }
        }

        [Test]
        public void testNullNamespaceNonNamespaceAware() {
            try {
                Processor proc = new();
                string xml =

                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                    "<root xmlns=\"abc\">" +
                    "<name xmlns=\"\">" +
                    "<first>Joe</first>" +
                    "<last>Blow</last>" +
                    "</name>" +
                    "</root>";

                string path =
                    "/*[local-name(.)=\"root\" and namespace-uri(.)=\"abc\"]" +
                    "/*[local-name(.)=\"name\" and namespace-uri(.)=\"\"]" +
                    "/*[local-name(.)=\"first\" and namespace-uri(.)=\"\"]/text()";


                DocumentBuilder builder = proc.NewDocumentBuilder();
                XmlDocument doc = new XmlDocument();
                doc.Load(new StringReader(xml));
                XdmNode xdmNode = builder.Wrap(doc);

                XPathCompiler xPath = proc.NewXPathCompiler();
                XPathSelector xp = xPath.Compile(path).Load();
                xp.ContextItem = xdmNode;
                XdmValue result = xp.Evaluate();

                Console.WriteLine(result.Count);
                Assert.AreEqual(1, result.Count);
                for (int i = 0; i < result.Count; i++) {
                    XdmNode n = (XdmNode) result[i];
                    Console.WriteLine(n);
                }


            }
            catch (Exception err) {
                Console.Error.WriteLine(err.StackTrace);
                Assert.Fail(err.Message);
            }
        }


        // [Test]
        // public void testBug1781() {
        //     try {
        //         Processor proc = new Processor(); 
        //         DomDestination domWriter = new DomDestination();
        //         domWriter.setPipelineConfiguration(proc.makePipelineConfiguration());
        //         domWriter.open();
        //         domWriter.startDocument(ReceiverOption.NONE);
        //         AttributeMap atts = EmptyAttributeMap.getInstance();
        //         atts = atts.put(new AttributeInfo(new NoNamespaceName("id"), BuiltInAtomicType.UNTYPED_ATOMIC, "iii", Loc.NONE, ReceiverOption.IS_ID));
        //         domWriter.startElement(new NoNamespaceName("root"), Untyped.getInstance(),
        //                                atts, NamespaceMap.emptyMap(),
        //                                Loc.NONE, ReceiverOption.NONE);
        //         domWriter.endElement();
        //         domWriter.endDocument();
        //         domWriter.close();
        //         Assert.AreEqual("", domWriter.getCurrentRoot().getStringValue());
        //     } catch (Exception err) {
        //         err.printStackTrace();
        //         Assert.Fail(();
        //     }
        // }

        [Test]
        public void testEmptyDom() {
            // Bug 2102
            try {
                XmlDocument sourceDoc = new();
                StringWriter sw = new StringWriter();
                Processor proc = new();
                string ss =
                    "<z xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xsl:version='2.0' count='{count(//*)}'/>";
                Xslt30Transformer transformer = proc.NewXsltCompiler().Compile(new StringReader(ss)).Load30();
                transformer.ApplyTemplates(proc.NewDocumentBuilder().Wrap(sourceDoc), proc.NewSerializer(sw));
                Assert.AreEqual("<?xml version=\"1.0\" encoding=\"UTF-8\"?><z count=\"0\"/>", sw.ToString());
            }
            catch (Exception e1) {
                Assert.Fail(e1.Message);
                //e1.printStackTrace();
            }
        }

        [Test]
        public void testEmptyDomResult() {
            // Bug 2164
            try {
                Processor proc = new();
                string ss =
                    "<z xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xsl:version='2.0' count='{count(//*)}'/>";
                Xslt30Transformer transformer = proc.NewXsltCompiler().Compile(new StringReader(ss)).Load30();
                DomDestination result = new DomDestination();
                transformer.ApplyTemplates(new StringReader(ss), result);
                Assert.AreEqual("1", (result.XmlDocument).DocumentElement.GetAttribute("count"));
            }
            catch (TransformerException e1) {
                Assert.Fail(e1.Message);
                //e1.printStackTrace();
            }
        }

        [Test]
        public void testSpaceStrippedDomWithExtensionFunction() {
            // Bug 3142
            try {
                string xml =

                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                    "<root xmlns=\"abc\">" +
                    "<name xmlns=\"\">" +
                    "<first>Joe</first>" +
                    "<last>Blow</last>" +
                    "</name>" +
                    "</root>";

                Processor proc = new();
                XdmItemType countType = proc.ParseItemType("function(item()*) as xs:integer");
                proc.RegisterExtensionFunction(new QName("java:jaxptest.DOMTest", "count"),
                    countType,
                    args => new XdmAtomicValue(args[0].Count));
                XmlDocument doc = new XmlDocument();
                doc.Load(new StringReader(xml));
                XdmNode xdmNode = proc.NewDocumentBuilder().Wrap(doc);


                string ss = "<xsl:transform xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='2.0'> "
                            + "<xsl:strip-space elements='*'/>"
                            + "<xsl:template match='/'><z count='{ext:count(//*)}' xmlns:ext='java:jaxptest.DOMTest'/></xsl:template>"
                            + "</xsl:transform>";
                Xslt30Transformer transformer = proc.NewXsltCompiler().Compile(new StringReader(ss)).Load30();
                DomDestination result = new DomDestination();
                transformer.ApplyTemplates(xdmNode, result);
                Assert.AreEqual("4", result.XmlDocument.DocumentElement.GetAttribute("count"));
            }
            catch (Exception e1) {
                Assert.Fail(e1.Message);
                //e1.printStackTrace();
            }
        }


        [Test]
        public void testBug3164() {
            try {
                string xml = "<test width='5'></test>";
                string stylesheet =
                    "<?xml version=\"1.0\"?> \n" +
                    "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                    "<xsl:template match='/'>\n" +
                    "<out><xsl:value-of select='ext:foo(/)' xmlns:ext='java:jaxptest.DOMTest'/></out>\n" +
                    "</xsl:template>\n" +
                    "</xsl:stylesheet>";
                Processor proc = new();
                XdmItemType fooType = proc.ParseItemType("function(node()) as xs:string");
                proc.RegisterExtensionFunction(new QName("java:jaxptest.DOMTest", "foo"), fooType,
                    args => new XdmAtomicValue(((XdmNode) args[0])
                        .Select(Steps.Child("test").Then(Steps.Attribute("width"))).First().StringValue));
            
                DocumentBuilder builder = proc.NewDocumentBuilder();
                XmlDocument document = new();
                document.Load(new StringReader(xml));
                XdmNode wrapper = builder.Wrap(document);
                XsltExecutable templates = proc.NewXsltCompiler().Compile(new StringReader(stylesheet));
                Xslt30Transformer transformer = templates.Load30();
                StringWriter sw = new ();
                transformer.ApplyTemplates(wrapper, proc.NewSerializer(sw));
                //Console.Error.WriteLine(sw.ToString());
                Assert.That(sw.ToString().Contains("<out>5</out>"));
            }
            catch (Exception e) {
                Console.WriteLine(e.StackTrace);
                Assert.Fail(e.Message);
            }
        }

        public static XmlNode foo(XmlDocument commandNode) {
            XmlElement command = commandNode.DocumentElement;
            Console.WriteLine("xxx get start");
            XmlAttribute attributeNode = command.GetAttributeNode("width");
            Console.WriteLine("xxx get end");
            return attributeNode;
        }

        [Test]
        public void testBug4121() {
            Xslt30Transformer transformer;
            Xslt30Transformer identityTransformer;

            string xslt =
                "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>\n" +
                "  <xsl:template match='/'>\n" +
                "    <xsl:variable name='node'>\n" +
                "      <xsl:sequence select='.'/>\n" +
                "    </xsl:variable>\n" +
                "    <xsl:sequence select='$node/node()'/>\n" +
                "  </xsl:template>\n" +
                "</xsl:stylesheet>";

            try {
                Processor proc = new();
                MemoryStream baos = new();
                Serializer serializer = proc.NewSerializer(baos);
                transformer = proc.NewXsltCompiler().Compile(new StringReader(xslt)).Load30();
                serializer.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");
                identityTransformer = IdentityTransformer(proc);


                string input = "<input/>";
                DocumentBuilder builder = proc.NewDocumentBuilder();
                XmlDocument document = new();
                transformer.ApplyTemplates(new StringReader(input), new DomDestination(document));

                identityTransformer.ApplyTemplates(builder.Wrap(document), serializer);
                string output = new UTF8Encoding().GetString(baos.GetBuffer(), 0, (int)baos.Length);
                Console.Error.WriteLine(output);
                Assert.AreEqual(output, input);
            }
            catch (Exception e) {
                //e.printStackTrace();
                Assert.Fail();
            }

        }

        [Test]
        public void testBug4509() {
            try {
                XmlDocument doc = new();
                doc.Load(new StringReader("<root xmlns='namespace'><a/><b/></root>"));
                XmlElement root = doc.DocumentElement;
                Assert.That(root.HasAttribute("xmlns"));
                XmlElement a = (XmlElement) root.ChildNodes[0];
                Assert.False(a.HasAttribute("xmlns"));
                Assert.AreEqual(0, a.Attributes.Count);
                XmlElement b = (XmlElement) root.ChildNodes[1];
                Assert.False(b.HasAttribute("xmlns"));
                Assert.AreEqual(0, b.Attributes.Count);
            }
            catch (SaxonApiException err) {
                Assert.Fail(err.Message);
            }
        }

        // [Test]
        // public void testBug4509a() {
        //     try {
        //         Processor proc = new Processor(false);
        //         net.sf.saxon.s9api.DocumentBuilder builder = proc.newDocumentBuilder();
        //         XdmNode tree = builder.build(new StreamSource(new StringReader("<root xmlns='namespace'><a/><b/></root>")));
        //         XmlDocument doc = (DocumentOverNodeInfo) DocumentOverNodeInfo.wrap(tree.getUnderlyingNode());
        //         XmlElement root = doc.DocumentElement;
        //         Assert.That(root.HasAttribute("xmlns"));
        //         XmlElement a = (XmlElement) root.ChildNodes[0];
        //         Assert.False(a.HasAttribute("xmlns"));
        //         Assert.AreEqual(0, a.Attributes.Count);
        //         XmlElement b = (XmlElement) root.ChildNodes[1];
        //         Assert.False(b.HasAttribute("xmlns"));
        //         Assert.AreEqual(0, b.Attributes.Count);
        //     } catch (SaxonApiException err) {
        //         Assert.Fail(err.getMessage());
        //     }
        // }


        // [Test]
        // public void testBug4509b() {
        //     try {
        //         Processor processor = new Processor(false);
        //         Saxon.Api.DocumentBuilder documentBuilder = processor.NewDocumentBuilder();
        //         string xml = "<root xmlns='namespace' abc='100'><element0 def='200'/><element1 ghi='300'/></root>";
        //         XdmNode node = documentBuilder.build(new StringReader(xml));
        //         XmlDocument document = (DocumentOverNodeInfo) DocumentOverNodeInfo.wrap(node.getUnderlyingNode());
        //         XmlElement root = document.DocumentElement;
        //         XmlElement element0 = (XmlElement) root.ChildNodes().item(0);
        //         Console.WriteLine(string.format("%s", element0.getNodeName()));
        //         for (int i = 0; i < element0.Attributes.getLength(); i++) {
        //             Console.WriteLine(string.format("element0 attribute[%d] %s=%s", i, element0.Attributes.item(i).getNodeName(), element0.Attributes.item(i).getNodeValue()));
        //         }
        //         Console.WriteLine();
        //         XmlElement element1 = (XmlElement) root.ChildNodes[1];
        //         Console.WriteLine(string.format("%s", element1.getNodeName()));
        //         for (int i = 0; i < element1.Attributes.Count; i++) {
        //             Console.WriteLine(string.format("element1 attribute[%d] %s=%s", i, element1.Attributes.item(i).getNodeName(), element1.Attributes.item(i).getNodeValue()));
        //         }
        //         Console.WriteLine();
        //         Console.WriteLine(string.format("treeModel=%s", documentBuilder.getTreeModel().getClass().getName()));
        //         Console.WriteLine("java -cp 'SaxonHE9-9-17J\\saxon9he.jar';. TEST");
        //         Console.WriteLine("ACTUAL OUTPUT ON MY PC");
        //         Console.WriteLine("element0");
        //         Console.WriteLine("element0 attribute[0] xmlns:xml=http://www.w3.org/XML/1998/namespace");
        //         Console.WriteLine("element0 attribute[1] def=200");
        //         Console.WriteLine("element1");
        //         Console.WriteLine("element1 attribute[0] xmlns:xml=http://www.w3.org/XML/1998/namespace");
        //         Console.WriteLine("element1 attribute[1] ghi=300");
        //         Console.WriteLine("treeModel=net.sf.saxon.om.TreeModel$TinyTree");
        //         Console.WriteLine();
        //         Console.WriteLine("java -cp 'SaxonHE10-0J\\saxon-he-10.0.jar';. TEST");
        //         Console.WriteLine("ACTUAL OUTPUT ON MY PC");
        //         Console.WriteLine("element0");
        //         Console.WriteLine("element0 attribute[0] xmlns=namespace");
        //         Console.WriteLine("element0 attribute[1] def=200");
        //         Console.WriteLine("element1");
        //         Console.WriteLine("element1 attribute[0] xmlns=namespace");
        //         Console.WriteLine("element1 attribute[1] ghi=300");
        //         Console.WriteLine("treeModel=net.sf.saxon.om.TreeModel$TinyTree");
        //         Console.WriteLine();
        //     } catch (Exception e) {
        //     }
        // }


        /**
     * Test that namespaces are copied OK by an identity transform
     */

        [Test]
        public void testNamespaceSerialization4858() {
            // bug 4858
            try {
                // Stylesheet
                string xslt =
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'"
                    + " xmlns:my='java:jaxptest.DOMTest' xmlns:xhtml='http://www.w3.org/1999/xhtml'>\n" +
                    "  <xsl:template match='/'>\n" +
                    "    <xsl:variable name='body' as='element(xhtml:body)' select='my:makeBodyElement()'/>\n" +
                    "    <xsl:sequence select='$body'/>\n" +
                    "  </xsl:template>\n" +
                    "</xsl:stylesheet>";

                // Run a transformation that invokes an extension function returning this DOM
                Processor proc = new();
                XdmItemType makeBodyElementType = proc.ParseItemType("function() as node()");
                proc.RegisterExtensionFunction(new QName("java:jaxptest.DOMTest", "makeBodyElement"), 
                        makeBodyElementType, args => {
                    XmlDocument doc = new();
                    XmlElement body = doc.CreateElement("xhtml:body", NamespaceConstant.XHTML);
                    XmlElement para = doc.CreateElement("xhtml:p", NamespaceConstant.XHTML);
                    body.AppendChild(para);
                    para.AppendChild(doc.CreateTextNode(("Lorem Ipsum")));
                    DocumentBuilder builder = proc.NewDocumentBuilder();
                    return builder.Wrap(builder.Wrap(doc), body);
                });
                XsltExecutable templates = proc.NewXsltCompiler().Compile(new StringReader(xslt));
                Xslt30Transformer transformer = templates.Load30();
                StringWriter sw = new StringWriter();

                transformer.ApplyTemplates(new StringReader("<a/>"), proc.NewSerializer(sw));
                Console.Error.WriteLine(sw.ToString());
                Assert.That(sw.ToString().Contains("<xhtml:body><xhtml:p>Lorem Ipsum</xhtml:p></xhtml:body>"));
            }
            catch (Exception e) {
                //e.printStackTrace();
                Assert.Fail(e.Message);
            }
        }


        [Test]
        public void testNamespaceSerializationBug4937() {
            try {
                string xml = "<a xmlns='someNS' name='test'><b xmlns=''/></a>";
                XmlDocument doc = new();
                doc.Load(new StringReader(xml));

                Processor proc = new();
                Xslt30Transformer tf = IdentityTransformer(proc);

                StringWriter writer = new StringWriter();

                Serializer ser = proc.NewSerializer(writer);
                ser.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");
                ser.SetOutputProperty(Serializer.INDENT, "yes");
                DocumentBuilder builder = proc.NewDocumentBuilder();
                tf.ApplyTemplates(builder.Wrap(builder.Wrap(doc), doc.DocumentElement), ser);

                string transformed = writer.ToString();
                Console.WriteLine(writer.ToString());

                XmlDocument doc2 = new();
                doc2.Load(new StringReader(transformed));
                Assert.AreEqual("someNS", doc2.DocumentElement.NamespaceURI);
                Assert.AreEqual("", doc2.GetElementsByTagName("b")[0].NamespaceURI);
            }
            catch (Exception err) {
                Assert.Fail(err.Message);
            }
        }




    }
}


