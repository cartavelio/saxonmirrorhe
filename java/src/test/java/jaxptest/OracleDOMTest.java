package jaxptest;


import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;

/**
 *  Note: requires Oracle XDK v10
 */
public class OracleDOMTest {

    @Test
    public void testDummy() {
        assertEquals(2, 2);
    }

//    /**
//     * Test that namespaces are copied OK by an identity transform
//     */
//
//    public void testCopyNamespaces() {
//        try {
//            String in = "<?xml version='1.0'?> \n" +
//                    "<Document xmlns='http://lost.namespace.com/' \n" +
//                    "xmlns:xsi='http://www.namespacexsi.com' " +
//                    "xsi:value='some-value'/>";
//
//            // Read an XML file to a DOM
//            System.setProperty("javax.xml.parsers.DocumentBuilderFactory", "oracle.xml.jaxp.JXDocumentBuilderFactory");
//            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
//            factory.setNamespaceAware(true);
//            DocumentBuilder db = factory.newDocumentBuilder();
//            InputSource is = new InputSource(new StringReader(in));
//            Document doc = db.parse(is);
//
//            // Output a DOM to an XML file
//            TransformerFactory tFactory = new TransformerFactoryImpl();
//            Transformer transformer = tFactory.newTransformer();
//            DOMSource source = new DOMSource(doc);
//            StringWriter sw = new StringWriter();
//            transformer.transform(source, new StreamResult(sw));
//            assertTrue("Namespace is present in output", sw.toString().contains("http://lost.namespace.com/"));
//            assertTrue("Namespace is present in output", sw.toString().contains("xmlns:xsi="));
//            assertTrue("Attribute is present in output", sw.toString().contains("xsi:value"));
//        }
//        catch (Exception e) {
//            fail(e.getMessage());
//        }
//    }
//
//    /**
//     * Test calling an extension function when the stylesheet has stripped whitespace (virtually)
//     */
//
//    public void testStripSpace() {
//        try {
//            String in = "<?xml version='1.0'?> \n" +
//                    "<Document> <e> <f/><f/> </e> </Document>";
//
//            // Read an XML file to a DOM
//            System.setProperty("javax.xml.parsers.DocumentBuilderFactory", "oracle.xml.jaxp.JXDocumentBuilderFactory");
//            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
//            factory.setNamespaceAware(true);
//            DocumentBuilder db = factory.newDocumentBuilder();
//            InputSource is = new InputSource(new StringReader(in));
//            Document doc = db.parse(is);
//
//            // Create stylesheet with call to extension function
//
//            String stylesheet =
//                    "<?xml version=\"1.0\"?> \n" +
//                            "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
//                            "<xsl:strip-space elements='*'/>" +
//                            "<xsl:template match='/'>\n" +
//                            "<xsl:value-of select='ext:showDomNodes(//f)' xmlns:ext='java:jaxptest.DOMTest'/>\n" +
//                            "<xsl:value-of select='ext:showDomNode((//e)[1])' xmlns:ext='java:jaxptest.DOMTest'/>\n" +
//                            "<xsl:apply-templates/>\n" +
//                            "</xsl:template>\n" +
//                            "<xsl:template match=\"@*|node()\">\n" +
//                            "<xsl:copy>\n" +
//                            "  <xsl:apply-templates select=\"@*|node()\" /> \n" +
//                            "</xsl:copy>\n" +
//                            "</xsl:template>\n" +
//                            "</xsl:stylesheet>";
//
//            TransformerFactory tFactory = new ProfessionalTransformerFactory();
//            Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(stylesheet)));
//            Transformer transformer = templates.newTransformer();
//            DOMSource source = new DOMSource(doc);
//            StringWriter sw = new StringWriter();
//            transformer.transform(source, new StreamResult(sw));
//            assertTrue("Extension function not called", sw.toString().contains("[f][f]{e}"));
//        }
//        catch (Exception e) {
//            fail(e.getMessage());
//        }
//    }
//
//    /**
//     * Method to be called from previous test
//     */
//
//    public static String showDomNodes(NodeList nodes) {
//        StringBuffer sb = new StringBuffer(20);
//        for (int i = 0; i < nodes.getLength(); i++) {
//            sb.append('[').append(nodes.item(i).getNodeName()).append(']');
//        }
//        return sb.toString();
//    }
//
//    /**
//     * Method to be called from previous test
//     */
//
//    public static String showDomNode(Node node) {
//        return "{" + node.getNodeName() + "}";
//    }
//
//    /**
//     * Test Configuration.buildDocument given a DOMSource that wraps an Element node
//     */
//
//    public void testBuildFromDomElement() {
//        // Note, this works rather inelegantly. Rather than creating a DOMSender, it wraps the Element
//        // node is a Saxon XOMNodeWrapper and uses the generic Navigator.copy() method on the XOMNodeWrapper
//        try {
//            String in = "<?xml version='1.0'?> \n" +
//                    "<Document> <e> <f/><f/> </e> </Document>";
//
//            // Read an XML file to a DOM
//            System.setProperty("javax.xml.parsers.DocumentBuilderFactory", "oracle.xml.jaxp.JXDocumentBuilderFactory");
//            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
//            factory.setNamespaceAware(true);
//            DocumentBuilder db = factory.newDocumentBuilder();
//            InputSource is = new InputSource(new StringReader(in));
//            Document doc = db.parse(is);
//            Element ele = doc.getDocumentElement();
//            NodeInfo nodeInfo = DOMObjectModel.getInstance().wrap(ele, new Configuration());
//            assertTrue("not-copied", nodeInfo instanceof VirtualNode);
//            NodeInfo elemInfo = DOMObjectModel.getInstance().copy(ele, TreeModel.TINY_TREE, new Configuration());
//            assertFalse("copied", elemInfo instanceof VirtualNode);
//            assertEquals("is-element", elemInfo.getNodeKind(), Type.ELEMENT);
//        } catch (Exception e) {
//            e.printStackTrace();
//            fail(e.getMessage());
//        }
//    }
//
//    /**
//     * Test that Saxon treats a text node with "null" content as if it had zero-length content
//     */
//
//    public void testNullTextNodes() {
//        try {
//            Configuration config = new Configuration();
//            System.setProperty("javax.xml.parsers.DocumentBuilderFactory", "oracle.xml.jaxp.JXDocumentBuilderFactory");
//            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
//            factory.setNamespaceAware(true);
//            DocumentBuilder db = factory.newDocumentBuilder();
//            Document doc = db.newDocument();
//            Element top = doc.createElement("top");
//            doc.appendChild(top);
//            Text text = doc.createTextNode("data");
//            /////////////////////////
//            text.setData(null);
//            /////////////////////////
//            top.appendChild(text);
//            DocumentWrapper wrapper = new DocumentWrapper(doc, "http://base.uri/", config);
//            StringWriter sw = new StringWriter();
//            Receiver out = config.getSerializerFactory().getReceiver(
//                    new StreamResult(sw), config.makePipelineConfiguration(), new Properties());
//            wrapper.copy(out, (CopyOptions.ALL_NAMESPACES | CopyOptions.TYPE_ANNOTATIONS), 0);
//            assertTrue("wrong output " + sw.toString(),
//                    sw.toString().equals("<?xml version=\"1.0\" encoding=\"UTF-8\"?><top/>"));
//        } catch (ParserConfigurationException e) {
//            fail(e.getMessage());
//        } catch (net.sf.saxon.trans.XPathException e) {
//            fail(e.getMessage());
//        } catch (Exception err) {
//            err.printStackTrace();
//            fail(err.getMessage());
//        }
//    }
//
//    /**
//     * Test JAXP XPath API delivering a DOM NodeList given a DOM Node as input
//     */
//
//    public void testXPathNodeList() {
//        try {
//            String in = "<?xml version='1.0'?> \n" +
//                    "<Document><e/><f/><g/></Document>";
//            System.setProperty("javax.xml.parsers.DocumentBuilderFactory", "oracle.xml.jaxp.JXDocumentBuilderFactory");
//            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
//            factory.setNamespaceAware(true);
//            DocumentBuilder db = factory.newDocumentBuilder();
//            InputSource is = new InputSource(new StringReader(in));
//            Document doc = db.parse(is);
//            XPathEvaluator xpe = new XPathEvaluator();
//            NodeList isList = (NodeList)xpe.evaluate("/*/*", doc, XPathConstants.NODESET);
//            assertTrue("length", isList.getLength() == 3);
//        } catch (ParserConfigurationException e) {
//            fail(e.getMessage());
//        } catch (SAXException e) {
//            fail(e.getMessage());
//        } catch (IOException e) {
//            fail(e.getMessage());
//        } catch (XPathExpressionException e) {
//            fail(e.getMessage());
//        }
//    }
//
//    /**
//     * Test JAXP XPath API delivering a DOM NodeList given a DOMSource as input
//     */
//
//    public void testXPathNodeListFromDOMSource() {
//        try {
//            String in = "<?xml version='1.0'?> \n" +
//                    "<Document><e/><f/><g/></Document>";
//            System.setProperty("javax.xml.parsers.DocumentBuilderFactory", "oracle.xml.jaxp.JXDocumentBuilderFactory");
//            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
//            factory.setNamespaceAware(true);
//            DocumentBuilder db = factory.newDocumentBuilder();
//            InputSource is = new InputSource(new StringReader(in));
//            Document doc = db.parse(is);
//            DOMSource source = new DOMSource(doc);
//            XPathEvaluator xpe = new XPathEvaluator();
//            NodeList isList = (NodeList)xpe.evaluate("/*/*", source, XPathConstants.NODESET);
//            assertTrue("length", isList.getLength() == 3);
//        } catch (ParserConfigurationException e) {
//            fail(e.getMessage());
//        } catch (SAXException e) {
//            fail(e.getMessage());
//        } catch (IOException e) {
//            fail(e.getMessage());
//        } catch (XPathExpressionException e) {
//            fail(e.getMessage());
//        }
//    }
//
//    /**
//     * Test JAXP XPath API delivering a DOM NodeList given a Saxon XOMNodeWrapper
//     */
//
//    public void testXPathNodeListFromWrapper() {
//        try {
//            String in = "<?xml version='1.0'?> \n" +
//                    "<Document><e/><f/><g/></Document>";
//            System.setProperty("javax.xml.parsers.DocumentBuilderFactory", "oracle.xml.jaxp.JXDocumentBuilderFactory");
//            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
//            factory.setNamespaceAware(true);
//            DocumentBuilder db = factory.newDocumentBuilder();
//            InputSource is = new InputSource(new StringReader(in));
//            Document doc = db.parse(is);
//            XPathEvaluator xpe = new XPathEvaluator();
//            DocumentWrapper wrapper = new DocumentWrapper(doc, "http://example.com/", xpe.getConfiguration());
//            NodeList isList = (NodeList)xpe.evaluate("/*/*", wrapper, XPathConstants.NODESET);
//            assertTrue("length", isList.getLength() == 3);
//        } catch (ParserConfigurationException e) {
//            fail(e.getMessage());
//        } catch (SAXException e) {
//            fail(e.getMessage());
//        } catch (IOException e) {
//            fail(e.getMessage());
//        } catch (XPathExpressionException e) {
//            fail(e.getMessage());
//        }
//    }
//
//    public void testNodeListFromXsltExtension() {
//        String stylesheet =
//                    "<?xml version=\"1.0\"?> \n" +
//                            "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
//                            "<xsl:strip-space elements='*'/>" +
//                            "<xsl:template match='/'>\n" +
//                            "<a><xsl:value-of select=\"count(ext:makeElementList('r', 'e', 5))\" xmlns:ext='java:jaxptest.DOMTest'/></a>\n" +
//                            "<b><xsl:value-of select=\"count(ext:makeElementList('r', 'e', 5)/*)\" xmlns:ext='java:jaxptest.DOMTest'/></b>\n" +
//                            "<c><xsl:value-of select=\"count(ext:makeElementList('r', 'e', 5)/node())\" xmlns:ext='java:jaxptest.DOMTest'/></c>\n" +
//                            "</xsl:template>\n" +
//                            "</xsl:stylesheet>";
//
//        try {
//            TransformerFactory tFactory = new EnterpriseTransformerFactory();
//            Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(stylesheet)));
//            Transformer transformer = templates.newTransformer();
//            StreamSource source = new StreamSource(new StringReader("<doc/>"));
//            StringWriter sw = new StringWriter();
//            transformer.transform(source, new StreamResult(sw));
//            assertTrue(sw.toString(), sw.toString().contains("<a>5</a><b>0</b><c>0</c>"));
//        } catch (TransformerException e) {
//            fail(e.getMessage());
//            e.printStackTrace();
//        }
//
//    }
//
//    public static NodeList makeElementList(String rootName, String elementName, double counter) {
//        System.setProperty("javax.xml.parsers.DocumentBuilderFactory", "oracle.xml.jaxp.JXDocumentBuilderFactory");
//        DocumentBuilderFactory dbfactory = DocumentBuilderFactory.newInstance();
//        dbfactory.setNamespaceAware(true);
//        try {
//            DocumentBuilder domparser = dbfactory.newDocumentBuilder();
//            Document document = domparser.newDocument();
//            Element element = document.createElement(rootName);
//            document.appendChild(element);
//            for (int i = 0; i < counter; i++)
//                element.appendChild(document.createElement(elementName));
//            return element.getChildNodes();
//        } catch (Throwable e) {
//            throw new RuntimeException(e);
//        }
//    }
//
//
//    /**
//     * Test showing a DOM NodeList returned by an extension function
//     */
//
//    public void testXPathNodeListFromExtensionFunction() {
//        try {
//            String in = "<?xml version='1.0'?> \n" +
//                    "<books><book>fred</book><book>jane</book><book>anne</book><book>sue</book></books>";
//            System.setProperty("javax.xml.parsers.DocumentBuilderFactory", "oracle.xml.jaxp.JXDocumentBuilderFactory");
//            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
//            factory.setNamespaceAware(true);
//            DocumentBuilder db = factory.newDocumentBuilder();
//            InputSource is = new InputSource(new StringReader(in));
//            Document doc = db.parse(is);
//            XPathEvaluator xpe = new XPathEvaluator();
//            String exp = "ext:sortArrayToNodeList(//book)";
//            xpe.setNamespaceContext(new NamespaceContext() {
//                public String getNamespaceURI(String prefix) {
//                    return (prefix.equals("ext") ? "java:jaxptest.DOMTest" : null);
//                }
//                public String getPrefix(String namespaceURI) {
//                    return null;
//                }
//                public Iterator getPrefixes(String namespaceURI) {
//                    return null;
//                }
//            });
//            NodeList isList = (NodeList)xpe.evaluate(exp, doc, XPathConstants.NODESET);
//            assertTrue("length", isList.getLength() == 4);
//        } catch (ParserConfigurationException e) {
//            fail(e.getMessage());
//            e.printStackTrace();
//        } catch (SAXException e) {
//            fail(e.getMessage());
//            e.printStackTrace();
//        } catch (IOException e) {
//            fail(e.getMessage());
//            e.printStackTrace();
//        } catch (XPathExpressionException e) {
//            fail(e.getMessage());
//            e.printStackTrace();
//        }
//    }
//
//    /**
//     * Extension function used from the test above
//     * @param source
//     * @return
//     * @throws Exception
//     */
//
////    public static NodeList sortArrayToNodeList(net.sf.saxon.value.Value source) throws Exception {
////        System.setProperty("javax.xml.parsers.DocumentBuilderFactory", "oracle.xml.jaxp.JXDocumentBuilderFactory");
////        DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
////        factory.setNamespaceAware(true);
////        DocumentBuilder db = factory.newDocumentBuilder();
////        Document doc = db.newDocument();
////        String[] items = new String[source.getLength()];
////        for (int i = 0; i < source.getLength(); i++) {
////            items[i] = source.itemAt(i).getStringValue();
////        }
////        Arrays.sort(items);
////        NodeSet resultSet = new NodeSet();
////        synchronized (doc) {
////            for (int i = 0; i < items.length; i++) {
////                resultSet.addNode(doc.createTextNode(items[i]));
////            }
////        }
////        return resultSet;
////    }
//
//    /**
//     * Variant of above test using an extension function that returns a NodeList
//     * implemented using the JAXP FunctionResolver
//     */
//
//    public void testXPathNodeListFromJaxpExtensionFunction() {
//         try {
//             String in = "<?xml version='1.0'?> \n" +
//                "<books><book>fred</book><book>jane</book><book>anne</book><book>sue</book></books>";
//             System.setProperty("javax.xml.parsers.DocumentBuilderFactory", "oracle.xml.jaxp.JXDocumentBuilderFactory");
//             DocumentBuilderFactory factory =
//                     DocumentBuilderFactory.newInstance();
//             factory.setNamespaceAware(true);
//             DocumentBuilder db = factory.newDocumentBuilder();
//             InputSource is = new InputSource(new StringReader(in));
//             Document doc = db.parse(is);
//             XPath xpath = getXPath();
//             NodeList isList = (NodeList)xpath.evaluate(
//                     "ext:sortArrayToSequence(//book)", doc, XPathConstants.NODESET);
//             assertTrue("length", isList.getLength() == 4);
//
//             isList =  (NodeList)xpath.evaluate("ext:sortArrayToNodeList(//book)",doc,XPathConstants.NODESET);
//
//             assertTrue("length", isList.getLength() == 4);
//
//         } catch (ParserConfigurationException e) {
//             fail(e.getMessage());
//         } catch (SAXException e) {
//             fail(e.getMessage());
//         } catch (IOException e) {
//             fail(e.getMessage());
//         } catch (XPathExpressionException e) {
//             fail(e.getMessage());
//             e.printStackTrace();
//         }
//     }
//
//     public XPath getXPath() {
//           XPath xpath = new XPathEvaluator();
//           xpath.setNamespaceContext(new NamespaceContext() {
//               public String getNamespaceURI(String prefix) {
//                   return (prefix.equals("ext") ? "http://extString" : null);
//               }
//
//               public String getPrefix(String namespaceURI) {
//                   return null;
//               }
//
//               public Iterator getPrefixes(String namespaceURI) {
//                   return null;
//               }
//           });
//
//           xpath.setXPathFunctionResolver(new XPathFunctionResolver() {
//               public XPathFunction resolveFunction(QName fname, int arity) {
//                   if (fname == null)
//                       throw new NullPointerException("The function name cannot be null.");
//                   try {
//                       Class cls = Class.forName("jaxptest.DOMTest");
//                       return new XPathFunctionImpl(cls, fname.getLocalPart());
//                   } catch (Exception e) {
//                       fail(e.getMessage());
//                   }
//                   return null;
//               }
//           });
//           return xpath;
//       }
//
//
////     public static NodeList sortArrayToNodeList(ArrayList source) throws Exception {
////         System.setProperty("javax.xml.parsers.DocumentBuilderFactory", "oracle.xml.jaxp.JXDocumentBuilderFactory");
////         DocumentBuilderFactory factory =
////                 DocumentBuilderFactory.newInstance();
////         factory.setNamespaceAware(true);
////         DocumentBuilder db = factory.newDocumentBuilder();
////         Document doc = db.newDocument();
////         String[] items = new String[source.size()];
////         for (int i = 0; i < source.size(); i++) {
////             items[i] = source.get(i).toString();
////         }
////         Arrays.sort(items);
////         NodeSet resultSet = new NodeSet();
////         synchronized (doc) {
////             for (int i = 0; i < items.length; i++) {
////                 resultSet.addNode(doc.createTextNode(items[i]));
////             }
////         }
////         return resultSet;
////     }
//
//      public static Object sortArrayToSequence(ArrayList source) throws Exception {
//          try {
//              Node[] items = new Node[source.size()];
//              for (int i = 0; i < source.size(); i++) {
//                  items[i] = (Node)source.get(i);
//              }
//              Arrays.sort(items,new Comparator(){
//                  public int compare(Object src, Object des){
//                    return ((Node)src).getTextContent().compareTo(((Node)des).getTextContent());
//                  }
//              });
//
//              return items;
//          } catch (Exception e) {
//              e.printStackTrace();
//              throw e;
//          }
//      }
//
//
//     private class XPathFunctionImpl implements XPathFunction {
//
//         private Class m_handler;
//         private String m_funcName;
//
//         /**
//          * Construct an instance of XPathFunctionImpl from the
//          * ExtensionHandler and function name.
//          *
//          * @param handler
//          * @param funcName
//          */
//         private XPathFunctionImpl(Class handler, String funcName) {
//             m_handler = handler;
//             m_funcName = funcName;
//         }
//
//         public Object evaluate(List args) throws XPathFunctionException {
//             Class[] clsArr = new Class[args.size()];
//             Object[] objArr = new Object[args.size()];
//             for (int i = 0; i < args.size(); i++) {
//                 clsArr[i] = args.get(i).getClass();
//                 objArr[i] = args.get(i);
//             }
//             try {
//                 Method callMethod = m_handler.getMethod(m_funcName, clsArr);
//                 return callMethod.invoke(m_handler, objArr);
//             } catch (NoSuchMethodException e) {
//                 fail("External method " + m_funcName + " not found: " + e.getMessage());
//                 return null;
//             } catch (Exception e) {
//                 throw new XPathFunctionException(e);
//             }
//         }
//     }
//
//	public void testIdentityTransformOfDOMTextNode() throws Exception {
//        System.setProperty("javax.xml.parsers.DocumentBuilderFactory", "oracle.xml.jaxp.JXDocumentBuilderFactory");
//        DocumentBuilderFactory documentBuilderFactory = DocumentBuilderFactory.newInstance();
//		Document document = documentBuilderFactory.newDocumentBuilder().newDocument();
//		Element spanElement = document.createElement("span");
//		document.appendChild(spanElement);
//		Text textNode = document.createTextNode("some text");
//		spanElement.appendChild(textNode);
//        Comment commentNode = document.createComment("a comment");
//        spanElement.appendChild(commentNode);
//        ProcessingInstruction piNode = document.createProcessingInstruction("tgt", "some data");
//        spanElement.appendChild(piNode);
//
//        TransformerFactory transformerFactory = new TransformerFactoryImpl();
//        Transformer transformer = transformerFactory.newTransformer();
//        transformer.setOutputProperty(OutputKeys.OMIT_XML_DECLARATION, "yes");
//        transformer.setOutputProperty(OutputKeys.METHOD, "xml");
//
//        StringWriter sw = new StringWriter();
//        transformer.transform(new DOMSource(textNode), new StreamResult(sw));
//        assertEquals("text node", sw.toString(), "some text");
//
//        sw = new StringWriter();
//        transformer.transform(new DOMSource(commentNode), new StreamResult(sw));
//        assertEquals("text node", sw.toString(), "<!--a comment-->");
//
//        sw = new StringWriter();
//        transformer.transform(new DOMSource(piNode), new StreamResult(sw));
//        assertEquals("text node", sw.toString(), "<?tgt some data?>");
//    }
//
//    /**
//     * Test case from T-Mobile, 8 April 2010
//     * @throws Exception
//     */
//
//    public void testElementValidation() throws Exception {
//        System.setProperty("javax.xml.parsers.DocumentBuilderFactory", "oracle.xml.jaxp.JXDocumentBuilderFactory");
//        String prefixXml = "<n0:bar xmlns:n0=\"http://bar\">\n" +
//                "    <foo xmlns=\"http://foo\"></foo>\n" +
//                "</n0:bar>";
//
//        String noPrefixXml = "<bar xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n" +
//                " xsi:schemaLocation=\"http://bar bar.xsd\"\n" +
//                " xmlns=\"http://bar\">\n" +
//                "    <foo xmlns=\"http://foo\"></foo>\n" +
//                "</bar>";
//
//        String barXsd = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
//                "<schema \n" +
//                "    xmlns:foo=\"http://foo\"\n" +
//                "    xmlns:bar=\"http://bar\"\n" +
//                "    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
//                "    xmlns=\"http://www.w3.org/2001/XMLSchema\"\n" +
//                "    attributeFormDefault=\"unqualified\"\n" +
//                "    blockDefault=\"\"\n" +
//                "    elementFormDefault=\"qualified\"\n" +
//                "    finalDefault=\"\"\n" +
//                "    targetNamespace=\"http://bar\"\n" +
//                "    version=\"0102\">\n" +
//                "\n" +
//                "    <xs:import namespace=\"http://foo\"/>\n" +
//                "\n" +
//                "\n" +
//                "    <xs:complexType abstract=\"false\" mixed=\"false\" name=\"Bar\">\n" +
//                "        <xs:sequence maxOccurs=\"1\" minOccurs=\"1\">\n" +
//                "            <xs:element maxOccurs=\"1\" minOccurs=\"0\" ref=\"foo:foo\">\n" +
//                "            </xs:element>\n" +
//                "        </xs:sequence>\n" +
//                "    </xs:complexType>\n" +
//                "    \n" +
//                "    <xs:element abstract=\"false\" name=\"bar\" type=\"bar:Bar\"/>\n" +
//                "    \n" +
//                "</schema>        ";
//
//        String fooXsd = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
//                "<schema \n" +
//                "    xmlns:foo=\"http://foo\"\n" +
//                "    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
//                "    xmlns=\"http://www.w3.org/2001/XMLSchema\"\n" +
//                "    attributeFormDefault=\"unqualified\"\n" +
//                "    blockDefault=\"\"\n" +
//                "    elementFormDefault=\"qualified\"\n" +
//                "    finalDefault=\"\"\n" +
//                "    targetNamespace=\"http://foo\"\n" +
//                "    version=\"0102\">\n" +
//                "\n" +
//                "    <xs:complexType abstract=\"false\" mixed=\"false\" name=\"Foo\">\n" +
//                "    </xs:complexType>\n" +
//                "    \n" +
//                "    <xs:element abstract=\"false\" name=\"foo\" type=\"foo:Foo\"/>\n" +
//                "    \n" +
//                "</schema>        ";
//
//
//        Configuration config = new EnterpriseConfiguration();
//
//		config.addSchemaSource( new StreamSource(new StringReader(fooXsd)));
//		config.addSchemaSource( new StreamSource(new StringReader(barXsd)));
//
//		StaticQueryContext ctx = config.newStaticQueryContext();
//		XQueryExpression xq = ctx.compileQuery("validate{.}");
//		final DynamicQueryContext dynamicContext = new DynamicQueryContext(config);
//
//        for (int i=0; i<2; i++) {
//
//            // First try with a namespaceAware factory, then with non-namespaceAware
//            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
//            factory.setNamespaceAware(i==0);
//
//            System.out.println(factory.getClass().getName());
//
//            DocumentBuilder domBuilder = factory.newDocumentBuilder();
//
//            Document doc = domBuilder.parse(new InputSource(new StringReader(prefixXml)));
//
//            Node node  = doc.getDocumentElement().getElementsByTagName("foo").item(0);
//            if (i==1) {
//                // Following assertion succeeds with Xerces DOM, fails with Oracle DOM
//                //assertNull(node.getNamespaceURI());
//            } else {
//                assertEquals(node.getNamespaceURI(), "http://foo");
//            }
//
//            dynamicContext.setContextItem( build( new DOMSource(node),config) );
//
//            List result = xq.evaluate(dynamicContext);
//            assertEquals(result.size(), 1);
//            assertEquals(((NodeInfo)result.get(0)).getURI(), "http://foo");
//
//
//            doc = domBuilder.parse(new InputSource(new StringReader(noPrefixXml)));
//
//            node  = doc.getDocumentElement().getElementsByTagName("foo").item(0);
//            if (i==1) {
//                // Following assertion succeeds with Xerces DOM, fails with Oracle DOM
//                //assertNull(node.getNamespaceURI());
//            } else {
//                assertEquals(node.getNamespaceURI(), "http://foo");
//            }
//
//            dynamicContext.setContextItem( build( new DOMSource(node),config) );
//
//            result = xq.evaluate(dynamicContext);
//            assertEquals(result.size(), 1);
//            assertEquals(((NodeInfo)result.get(0)).getURI(), "http://foo");
//        }
//    }
//
//	public static NodeInfo build(Source source, Configuration config) throws Exception {
//        // Create a pipeline configuration
//        PipelineConfiguration pipe = config.makePipelineConfiguration();
//
//        // Create an appropriate Builder
//        Builder b = new LinkedTreeBuilder(pipe);
//
//        // Set builder properties
//        b.setPipelineConfiguration(pipe);
//
//        Sender.send(source, b, null);
//
//        // Get the constructed document
//        NodeInfo newdoc = b.getCurrentRoot();
//
//        // Reset the builder, detaching it from the constructed document
//        b.reset();
//
//        // If requested, close the input stream
//        if (source instanceof AugmentedSource && ((AugmentedSource)source).isPleaseCloseAfterUse()) {
//            ((AugmentedSource)source).close();
//        }
//        return newdoc;
//	}
//
//    public void testNullPrefix() throws Exception {
//		Configuration config = new EnterpriseConfiguration();
//        String dir = ConfigTest.DATA_DIR + "k5/";
//		config.addSchemaSource( new StreamSource( new File(dir + "bar.xsd")));
//		config.addSchemaSource( new StreamSource( new File(dir + "foo.xsd")));
//        // 2011-09-14: failing because bar imports foo and components from foo are apparently loaded twice?
//
//		StaticQueryContext ctx = config.newStaticQueryContext();
//		XQueryExpression xq = ctx.compileQuery(new FileReader(new File(dir + "gen.xq")));
//		final DynamicQueryContext dynamicContext = new DynamicQueryContext(
//				config);
//
//        System.setProperty("javax.xml.parsers.DocumentBuilderFactory", "oracle.xml.jaxp.JXDocumentBuilderFactory");
//		DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
//
//		System.out.println(factory.getClass().getName());
//
//		Document doc = DocumentBuilderFactory.newInstance().newDocumentBuilder().parse(new File(dir + "prefix.xml"));
//
//		Node barNode = doc.getElementsByTagName("bar").item(0);
//		//oracle bpel does it too-(
//		barNode.setPrefix("");
//
//
//		Node node  = doc.getDocumentElement();
//		print(doc.getDocumentElement());
//
//		AugmentedSource source = AugmentedSource.makeAugmentedSource(new DOMSource(doc));
//		source.setWrapDocument(false);
//
//
//		NodeInfo docInfo =  config.buildDocument(source);
//
//
//		dynamicContext.setContextItem( docInfo );
//
//		DOMResult domRes = new DOMResult();
//		xq.run(dynamicContext,domRes,null);
//
//		print(((Document)domRes.getNode()).getDocumentElement());
//    }
//
//    private static void print(Node node) {
//		try {
//			String out = node2String(node, "-");
//			System.out.println(out);
//		} catch (Throwable t) {
//			t.printStackTrace();
//		}
//	}
//
//	private static String node2String(Node node, String indent) {
//		String out = indent + "Prefix: " + node.getPrefix() + " | NamespaceURI: " + node.getNamespaceURI()
//				+ " | Name: " + node.getNodeName() + " | Value: " + node.getNodeValue() + "\n";
//
//		NamedNodeMap map = node.getAttributes();
//		if (map != null) {
//			for (int i = 0; i < map.getLength(); ++i) {
//				out += node2String(map.item(i), indent + "-");
//			}
//		}
//
//		NodeList nodes = node.getChildNodes();
//		for (int i = 0; i < nodes.getLength(); ++i) {
//			out += node2String(nodes.item(i), indent + "-");
//		}
//
//		return out;
//	}

}
