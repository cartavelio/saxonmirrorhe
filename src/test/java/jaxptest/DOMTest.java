package jaxptest;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.config.EnterpriseTransformerFactory;
import com.saxonica.config.ProfessionalTransformerFactory;
import com.saxonica.ee.domino.DominoTree;
import com.saxonica.ee.jaxp.SchemaFactoryImpl;
import junit.framework.TestCase;
import net.sf.saxon.Configuration;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.dom.DOMObjectModel;
import net.sf.saxon.dom.DOMWriter;
import net.sf.saxon.dom.DocumentWrapper;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.ExplicitLocation;
import net.sf.saxon.expr.sort.CodepointCollatingComparer;
import net.sf.saxon.jaxp.SaxonTransformerFactory;
import net.sf.saxon.lib.AugmentedSource;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.om.CopyOptions;
import net.sf.saxon.om.NoNamespaceName;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.TreeModel;
import net.sf.saxon.query.DynamicQueryContext;
import net.sf.saxon.query.StaticQueryContext;
import net.sf.saxon.query.XQueryExpression;
import net.sf.saxon.s9api.XdmNode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.linked.LinkedTreeBuilder;
import net.sf.saxon.tree.util.SteppingNode;
import net.sf.saxon.tree.wrapper.VirtualNode;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.Type;
import net.sf.saxon.type.Untyped;
import net.sf.saxon.xpath.XPathEvaluator;
import net.sf.saxon.xpath.XPathFactoryImpl;
import org.w3c.dom.*;
import org.xml.sax.InputSource;
import org.xml.sax.SAXException;

import javax.xml.namespace.NamespaceContext;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import javax.xml.transform.*;
import javax.xml.transform.dom.DOMResult;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import javax.xml.validation.Schema;
import javax.xml.validation.SchemaFactory;
import javax.xml.validation.Validator;
import javax.xml.xpath.*;
import java.io.*;
import java.lang.reflect.Method;
import java.nio.charset.StandardCharsets;
import java.util.*;

import static net.sf.saxon.s9api.streams.Predicates.isText;
import static net.sf.saxon.s9api.streams.Steps.*;

//import org.apache.xpath.NodeSet;

/**
 *
 */
public class DOMTest extends TestCase {

    /**
     * Test that namespaces are copied OK by an identity transform
     */

    public void testCopyNamespaces() {
        try {
            String in = "<?xml version='1.0'?> \n" +
                "<Document xmlns='http://lost.namespace.com/' \n" +
                "xmlns:xsi='http://www.namespacexsi.com' " +
                "xsi:value='some-value'/>";

            // Read an XML file to a DOM
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            DocumentBuilder db = factory.newDocumentBuilder();
            InputSource is = new InputSource(new StringReader(in));
            Document doc = db.parse(is);

            // Output a DOM to an XML file
            TransformerFactory tFactory = new TransformerFactoryImpl();
            Transformer transformer = tFactory.newTransformer();
            DOMSource source = new DOMSource(doc);
            StringWriter sw = new StringWriter();
            transformer.transform(source, new StreamResult(sw));
            //System.err.println(sw.toString());
            assertTrue("Namespace is present in output", sw.toString().contains("http://lost.namespace.com/"));
            assertTrue("Namespace is present in output", sw.toString().contains("xmlns:xsi="));
            assertTrue("Attribute is present in output", sw.toString().contains("xsi:value"));
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    /**
     * Get the string value of a document node
     */

    public void testStringValueOfDocHE() {
        testStringValueOfDoc(new TransformerFactoryImpl());
    }

    public void testStringValueOfDocPE() {
        testStringValueOfDoc(new ProfessionalTransformerFactory());
    }

    public void testStringValueOfDocEE() {
        testStringValueOfDoc(new EnterpriseTransformerFactory());
    }


    public void testStringValueOfDoc(TransformerFactory tFactory) {
        try {
            String xml = "<!DOCTYPE test [\n" +
                " <!ENTITY omega 'theta'>\n" +
                "]>\n" +
                "<test>[&omega;]</test>";

            // Read an XML file to a DOM
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            DocumentBuilder db = factory.newDocumentBuilder();
            InputSource is = new InputSource(new StringReader(xml));
            Document doc = db.parse(is);

            String stylesheet =
                "<?xml version=\"1.0\"?> \n" +
                    "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                    "<xsl:template match='/'>\n" +
                    "<out><xsl:value-of select='.'/></out>\n" +
                    "</xsl:template>\n" +
                    "</xsl:stylesheet>";

            Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(stylesheet)));
            Transformer transformer = templates.newTransformer();
            StringWriter sw = new StringWriter();
            transformer.transform(new DOMSource(doc), new StreamResult(sw));
            //System.err.println(sw.toString());
            assertTrue(sw.toString().contains("<out>[theta]</out>"));
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    public void testDescendantAxis() {
        // Bug 4683
        try {
            TransformerFactory tFactory = new TransformerFactoryImpl();
            String xml = "<root>\n"
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
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            DocumentBuilder db = factory.newDocumentBuilder();
            InputSource is = new InputSource(new StringReader(xml));
            Document doc = db.parse(is);

            String stylesheet =
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

            Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(stylesheet)));
            Transformer transformer = templates.newTransformer();
            StringWriter sw = new StringWriter();
            transformer.transform(new DOMSource(doc), new StreamResult(sw));
            //System.err.println(sw.toString());
            assertTrue(sw.toString().contains("1 true\n"
                                                      + "2 false foo1\n"
                                                      + "3 true\n"
                                                      + "4 false foo2"));
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    /**
     * Test calling an extension function when the stylesheet has stripped whitespace (virtually)
     */

    public void testStripSpaceHE() {
        testStripSpace(new TransformerFactoryImpl());
    }

    public void testStripSpacePE() {
        testStripSpace(new ProfessionalTransformerFactory());
    }

    public void testStripSpaceEE() {
        testStripSpace(new EnterpriseTransformerFactory());
    }

    public void testStripSpace(TransformerFactory tFactory) {
        try {
            String in = "<?xml version='1.0'?> \n" +
                "<Document> <e> <f/><f/> </e> </Document>";

            // Read an XML file to a DOM
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            DocumentBuilder db = factory.newDocumentBuilder();
            InputSource is = new InputSource(new StringReader(in));
            Document doc = db.parse(is);

            // Create stylesheet with call to extension function

            String stylesheet =
                "<?xml version=\"1.0\"?> \n" +
                    "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                    "<xsl:strip-space elements='*'/>" +
                    "<xsl:template match='/'>\n" +
                    "<xsl:value-of select='ext:showDomNodes(//f)' xmlns:ext='java:jaxptest.DOMTest'/>\n" +
                    "<xsl:value-of select='ext:showDomNode((//e)[1])' xmlns:ext='java:jaxptest.DOMTest'/>\n" +
                    "<xsl:apply-templates/>\n" +
                    "</xsl:template>\n" +
                    "<xsl:template match=\"@*|node()\">\n" +
                    "<xsl:copy>\n" +
                    "  <xsl:apply-templates select=\"@*|node()\" /> \n" +
                    "</xsl:copy>\n" +
                    "</xsl:template>\n" +
                    "</xsl:stylesheet>";

            Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(stylesheet)));
            Transformer transformer = templates.newTransformer();
            DOMSource source = new DOMSource(doc);
            StringWriter sw = new StringWriter();
            transformer.transform(source, new StreamResult(sw));
            //System.err.println(sw.toString());
            assertTrue("Extension function not called", sw.toString().contains("[f][f]{e}"));
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    /**
     * Method to be called from previous test
     */

    public static String showDomNodes(NodeList nodes) {
        StringBuilder sb = new StringBuilder(20);
        for (int i = 0; i < nodes.getLength(); i++) {
            sb.append('[').append(nodes.item(i).getNodeName()).append(']');
        }
        return sb.toString();
    }

    /**
     * Method to be called from previous test
     */

    public static String showDomNode(Node node) {
        return "{" + node.getNodeName() + "}";
    }

    /**
     * Test Configuration.buildDocument given a DOMSource that wraps an Element node
     */

    public void testBuildFromDomElement() {
        // Note, this works rather inelegantly. Rather than creating a DOMSender, it wraps the Element
        // node is a Saxon XOMNodeWrapper and uses the generic Navigator.copy() method on the XOMNodeWrapper
        try {
            String in = "<?xml version='1.0'?> \n" +
                "<Document> <e> <f/><f/> </e> </Document>";

            // Read an XML file to a DOM
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            DocumentBuilder db = factory.newDocumentBuilder();
            InputSource is = new InputSource(new StringReader(in));
            Document doc = db.parse(is);
            Element ele = doc.getDocumentElement();
            NodeInfo nodeInfo = DOMObjectModel.getInstance().wrap(ele, new Configuration());
            assertTrue("not-copied", nodeInfo instanceof VirtualNode);
            NodeInfo elemInfo = DOMObjectModel.getInstance().copy(ele, TreeModel.TINY_TREE, new Configuration());
            assertFalse("copied", elemInfo instanceof VirtualNode);
            assertEquals("is-element", elemInfo.getNodeKind(), Type.ELEMENT);
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    /**
     * Test that Saxon treats a text node with "null" content as if it had zero-length content
     */

    public void testNullTextNodes() {
        try {
            Configuration config = new Configuration();
            Document doc = makeDocWithNullTextNode();
            DocumentWrapper wrapper = new DocumentWrapper(doc, "http://base.uri/", config);
            StringWriter sw = new StringWriter();
            Receiver out = config.getSerializerFactory().getReceiver(new StreamResult(sw));
            wrapper.getRootNode().copy(out, CopyOptions.ALL_NAMESPACES | CopyOptions.TYPE_ANNOTATIONS, ExplicitLocation.UNKNOWN_LOCATION);
            assertEquals("wrong output " + sw, "<?xml version=\"1.0\" encoding=\"UTF-8\"?><top/>", sw.toString());
        } catch (ParserConfigurationException | XPathException e) {
            fail(e.getMessage());
        } catch (Exception err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }

    public void testNullTextNodesDomino() {
        try {
            Configuration config = new EnterpriseConfiguration();
            Document doc = makeDocWithNullTextNode();
            NodeInfo wrapper = DominoTree.makeTree(doc, config, "http://base.uri/").getRootNode();
            try (StringWriter sw = new StringWriter()) {
                Receiver out = config.getSerializerFactory().getReceiver(new StreamResult(sw));
                wrapper.copy(out, CopyOptions.ALL_NAMESPACES | CopyOptions.TYPE_ANNOTATIONS, ExplicitLocation.UNKNOWN_LOCATION);
                assertEquals("wrong output " + sw, "<?xml version=\"1.0\" encoding=\"UTF-8\"?><top/>", sw.toString());
            }
        } catch (ParserConfigurationException | XPathException e) {
            fail(e.getMessage());
        } catch (Exception err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }

    private Document makeDocWithNullTextNode() throws ParserConfigurationException {
        DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
        factory.setNamespaceAware(true);
        DocumentBuilder db = factory.newDocumentBuilder();
        Document doc = db.newDocument();
        Element top = doc.createElement("top");
        doc.appendChild(top);
        Text text = doc.createTextNode("data");
        /////////////////////////
        text.setData(null);
        /////////////////////////
        top.appendChild(text);
        return doc;
    }

    public void testAdjacentTextNodesWithDomino() {
        try {
            Configuration config = new EnterpriseConfiguration();
            Document doc = makeDocWithAdjacentTextNodes(false);
            NodeInfo wrapper = DominoTree.makeTree(doc, config, "http://base.uri/").getRootNode();
            XdmNode docNode = new XdmNode(wrapper);
            navigateOverAdjacentTextNodes(docNode);

        } catch (ParserConfigurationException | XPathException e) {
            fail(e.getMessage());
        } catch (Exception err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }

    public void testAdjacentTextNodesWithPlainDomWrapper() {
        try {
            Configuration config = new EnterpriseConfiguration();
            Document doc = makeDocWithAdjacentTextNodes(false);
            NodeInfo wrapper = new DocumentWrapper(doc, "http://base.uri/", config).getRootNode();
            XdmNode docNode = new XdmNode(wrapper);
            navigateOverAdjacentTextNodes(docNode);

        } catch (ParserConfigurationException e) {
            fail(e.getMessage());
        } catch (Exception err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }

    private void navigateOverAdjacentTextNodes(XdmNode docNode) {
        XdmNode top = docNode.select(child()).asNode();

        long count = top.select(child()).count();
        assertEquals(1, count);
        XdmNode text = top.select(child()).asNode();
        assertEquals("t1", "text1text2", text.getStringValue());

        count = docNode.select(descendant()).count();
        assertEquals("t2", 2, count);
        text = docNode.select(descendant(isText())).asNode();
        assertEquals("t3", "text1text2", text.getStringValue());

        count = top.select(descendant()).count();
        assertEquals("t4", 1, count);
        text = top.select(descendant()).asNode();
        assertEquals("t5", "text1text2", text.getStringValue());

    }

    private void navigateOverAdjacentTextNodesMixed(XdmNode docNode) {
        XdmNode top = docNode.select(child()).asNode();

        long count = top.select(child()).count();
        assertEquals(3, count);
        XdmNode text = top.select(child(isText())).asNode();
        assertEquals("t1", "text1text2", text.getStringValue());

        count = docNode.select(descendant()).count();
        assertEquals(4, count);
        text = docNode.select(descendant(isText())).asNode();
        assertEquals("t2", "text1text2", text.getStringValue());

        count = top.select(descendant()).count();
        assertEquals(3, count);
        text = top.select(descendant(isText())).asNode();
        assertEquals("t3", "text1text2", text.getStringValue());

        XdmNode alpha  = text.select(precedingSibling()).asNode();
        assertEquals("t4", "alpha", alpha.getNodeName().getLocalName());

        XdmNode omega = text.select(followingSibling()).asNode();
        assertEquals("t5", "omega", omega.getNodeName().getLocalName());

        XdmNode preOmega = omega.select(precedingSibling()).first().asNode();
        assertEquals("t6", "text1text2", preOmega.getStringValue());

        XdmNode prePreOmega = preOmega.select(precedingSibling()).first().asNode();
        assertEquals("t7", "alpha", prePreOmega.getNodeName().getLocalName());

        NodeInfo omegaInfo = omega.getUnderlyingNode();
        if (omegaInfo instanceof SteppingNode) {
            NodeInfo preOmegaInfo = ((SteppingNode) omegaInfo).getPreviousSibling();
            assertEquals("t8", "text1text2", preOmegaInfo.getStringValue());
            NodeInfo prePreOmegaInfo = ((SteppingNode) preOmegaInfo).getPreviousSibling();
            assertEquals("t9", "alpha", prePreOmegaInfo.getLocalPart());
        }

    }

    public void testAdjacentTextNodesWithDominoMixed() {
        try {
            Configuration config = new EnterpriseConfiguration();
            Document doc = makeDocWithAdjacentTextNodes(true);
            NodeInfo wrapper = DominoTree.makeTree(doc, config, "http://base.uri/").getRootNode();
            XdmNode docNode = new XdmNode(wrapper);
            navigateOverAdjacentTextNodesMixed(docNode);

        } catch (ParserConfigurationException | XPathException e) {
            fail(e.getMessage());
        } catch (Exception err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }

    public void testAdjacentTextNodesWithPlainDomWrapperMixed() {
        try {
            Configuration config = new EnterpriseConfiguration();
            Document doc = makeDocWithAdjacentTextNodes(true);
            NodeInfo wrapper = new DocumentWrapper(doc, "http://base.uri/", config).getRootNode();
            XdmNode docNode = new XdmNode(wrapper);
            navigateOverAdjacentTextNodesMixed(docNode);

        } catch (ParserConfigurationException e) {
            fail(e.getMessage());
        } catch (Exception err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }


    public void testAdjacentTextNodesDomino() {
        try {
            Configuration config = new EnterpriseConfiguration();
            Document doc = makeDocWithAdjacentTextNodes(false);
            NodeInfo wrapper = DominoTree.makeTree(doc, config, "http://base.uri/").getRootNode();
            StringWriter sw = new StringWriter();
            Receiver out = config.getSerializerFactory().getReceiver(new StreamResult(sw));
            wrapper.copy(out, CopyOptions.ALL_NAMESPACES | CopyOptions.TYPE_ANNOTATIONS, ExplicitLocation.UNKNOWN_LOCATION);
            assertEquals("wrong output " + sw, "<?xml version=\"1.0\" encoding=\"UTF-8\"?><top>text1text2</top>", sw.toString());
            XPathFactoryImpl xf =  new net.sf.saxon.xpath.XPathFactoryImpl();
            xf.setConfiguration(config);
            XPath xp = xf.newXPath();
            String result = xp.evaluate("count(.//text())", wrapper);
            assertEquals("number of text nodes ", "1", result);
        } catch (ParserConfigurationException | XPathException e) {
            fail(e.getMessage());
        } catch (Exception err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }

    private Document makeDocWithAdjacentTextNodes(boolean mixed) throws ParserConfigurationException {
        DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
        factory.setNamespaceAware(true);
        DocumentBuilder db = factory.newDocumentBuilder();
        Document doc = db.newDocument();
        Element top = doc.createElement("top");
        doc.appendChild(top);
        if (mixed) {
            Element alpha = doc.createElement("alpha");
            top.appendChild(alpha);
        }
        Text text1 = doc.createTextNode("text1");
        top.appendChild(text1);
        Text text2 = doc.createTextNode("text2");
        top.appendChild(text2);
        if (mixed) {
            Element omega = doc.createElement("omega");
            top.appendChild(omega);
        }
        return doc;
    }


    /**
     * Test JAXP XPath API delivering a DOM NodeList given a DOM Node as input
     */

    public void testXPathNodeList() {
        try {
            String in = "<?xml version='1.0'?> \n" +
                "<Document><e/><f/><g/></Document>";
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            DocumentBuilder db = factory.newDocumentBuilder();
            InputSource is = new InputSource(new StringReader(in));
            Document doc = db.parse(is);
            XPathEvaluator xpe = new XPathEvaluator();
            NodeList isList = (NodeList) xpe.evaluate("/*/*", doc, XPathConstants.NODESET);
            assertEquals("length", 3, isList.getLength());
        } catch (ParserConfigurationException | XPathExpressionException | IOException | SAXException e) {
            fail(e.getMessage());
        }
    }

    /**
     * Test JAXP XPath API delivering a DOM NodeList given a DOMSource as input
     */

    public void testXPathNodeListFromDOMSource() {
        try {
            String in = "<?xml version='1.0'?> \n" +
                "<Document><e/><f/><g/></Document>";
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            DocumentBuilder db = factory.newDocumentBuilder();
            InputSource is = new InputSource(new StringReader(in));
            Document doc = db.parse(is);
            DOMSource source = new DOMSource(doc);
            XPathEvaluator xpe = new XPathEvaluator();
            NodeList isList = (NodeList) xpe.evaluate("/*/*", source, XPathConstants.NODESET);
            assertEquals("length", 3, isList.getLength());
        } catch (ParserConfigurationException | SAXException | IOException | XPathExpressionException e) {
            fail(e.getMessage());
        }
    }

    /**
     * Test JAXP XPath API delivering a DOM NodeList given a Saxon DOM NodeWrapper
     */

    public void testXPathNodeListFromWrapper() {
        try {
            String in = "<?xml version='1.0'?> \n" +
                "<Document><e/><f/><g/></Document>";
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            DocumentBuilder db = factory.newDocumentBuilder();
            InputSource is = new InputSource(new StringReader(in));
            Document doc = db.parse(is);
            XPathEvaluator xpe = new XPathEvaluator();
            DocumentWrapper wrapper = new DocumentWrapper(doc, "http://example.com/", xpe.getConfiguration());
            NodeList isList = (NodeList) xpe.evaluate("/*/*", wrapper, XPathConstants.NODESET);
            assertEquals("length", 3, isList.getLength());
        } catch (ParserConfigurationException | SAXException | IOException | XPathExpressionException e) {
            fail(e.getMessage());
        }
    }

    public void testNodeListFromXsltExtension() {
        String stylesheet =
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
            TransformerFactory tFactory = new EnterpriseTransformerFactory();
            Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(stylesheet)));
            Transformer transformer = templates.newTransformer();
            StreamSource source = new StreamSource(new StringReader("<doc/>"));
            StringWriter sw = new StringWriter();
            transformer.transform(source, new StreamResult(sw));
            assertTrue(sw.toString(), sw.toString().contains("<a>5</a><b>0</b><c>0</c>"));
        } catch (TransformerException e) {
            fail(e.getMessage());
            e.printStackTrace();
        }

    }

    public static NodeList makeElementList(String rootName, String elementName, double counter) {
        DocumentBuilderFactory dbfactory = DocumentBuilderFactory.newInstance();
        dbfactory.setNamespaceAware(true);
        try {
            DocumentBuilder domparser = dbfactory.newDocumentBuilder();
            Document document = domparser.newDocument();
            Element element = document.createElement(rootName);
            document.appendChild(element);
            for (int i = 0; i < counter; i++) {
                element.appendChild(document.createElement(elementName));
            }
            return element.getChildNodes();
        } catch (Throwable e) {
            throw new RuntimeException(e);
        }
    }


    /**
     * Test showing a DOM NodeList returned by an extension function
     */

    public void testXPathNodeListFromExtensionFunction() {
        try {
            String in = "<?xml version='1.0'?> \n" +
                "<books><book>fred</book><book>jane</book><book>anne</book><book>sue</book></books>";
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            DocumentBuilder db = factory.newDocumentBuilder();
            InputSource is = new InputSource(new StringReader(in));
            Document doc = db.parse(is);
            XPathEvaluator xpe = new XPathEvaluator();
            String exp = "ext:sortArrayToNodeList(//book)";
            xpe.setNamespaceContext(new NamespaceContext() {
                public String getNamespaceURI(String prefix) {
                    return prefix.equals("ext") ? "java:jaxptest.DOMTest" : null;
                }

                public String getPrefix(String namespaceURI) {
                    return null;
                }

                public Iterator getPrefixes(String namespaceURI) {
                    return null;
                }
            });
            NodeList isList = (NodeList) xpe.evaluate(exp, doc, XPathConstants.NODESET);
            assertEquals("length", 4, isList.getLength());
        } catch (ParserConfigurationException | SAXException | IOException | XPathExpressionException e) {
            fail(e.getMessage());
            e.printStackTrace();
        }
    }

    /**
     * Variant of above test using an extension function that returns a NodeList
     * implemented using the JAXP FunctionResolver
     */

    public void testXPathNodeListFromJaxpExtensionFunction() {
        try {
            String in = "<?xml version='1.0'?> \n" +
                "<books><book>fred</book><book>jane</book><book>anne</book><book>sue</book></books>";
            DocumentBuilderFactory factory =
                DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            DocumentBuilder db = factory.newDocumentBuilder();
            InputSource is = new InputSource(new StringReader(in));
            Document doc = db.parse(is);
            XPath xpath = getXPath();
            NodeList isList = (NodeList) xpath.evaluate(
                "ext:sortArrayToSequence(//book)", doc, XPathConstants.NODESET);
            assertEquals("length", 4, isList.getLength());

            isList = (NodeList) xpath.evaluate("ext:sortArrayToNodeList(//book)", doc, XPathConstants.NODESET);

            assertEquals("length", 4, isList.getLength());

        } catch (ParserConfigurationException | SAXException | IOException e) {
            fail(e.getMessage());
        } catch (XPathExpressionException e) {
            fail(e.getMessage());
            e.printStackTrace();
        }
    }

    public XPath getXPath() {
        XPath xpath = new XPathEvaluator();
        xpath.setNamespaceContext(new NamespaceContext() {
            public String getNamespaceURI(String prefix) {
                return prefix.equals("ext") ? "http://extString" : null;
            }

            public String getPrefix(String namespaceURI) {
                return null;
            }

            public Iterator getPrefixes(String namespaceURI) {
                return null;
            }
        });

        xpath.setXPathFunctionResolver((fname, arity) -> {
            if (fname == null) {
                throw new NullPointerException("The function name cannot be null.");
            }
            try {
                Class cls = Class.forName("jaxptest.DOMTest");
                return new XPathFunctionImpl(cls, fname.getLocalPart());
            } catch (Exception e) {
                fail(e.getMessage());
            }
            return null;
        });
        return xpath;
    }


    public static NodeList sortArrayToNodeList(ArrayList source) throws Exception {
        DocumentBuilderFactory factory =
            DocumentBuilderFactory.newInstance();
        factory.setNamespaceAware(true);
        DocumentBuilder db = factory.newDocumentBuilder();
        Document doc = db.newDocument();
        String[] items = new String[source.size()];
        for (int i = 0; i < source.size(); i++) {
            items[i] = source.get(i).toString();
        }
        Arrays.sort(items);

        final ArrayList<Node> textNodes = new ArrayList<>(items.length);

        for (String item : items) {
            textNodes.add(doc.createTextNode(item));
        }

        return new NodeList() {

            public Node item(int index) {
                return textNodes.get(index);
            }

            public int getLength() {
                return textNodes.size();
            }
        };
    }

    public static Object sortArrayToSequence(ArrayList source) {
        try {
            Node[] items = new Node[source.size()];
            for (int i = 0; i < source.size(); i++) {
                items[i] = (Node) source.get(i);
            }
            Arrays.sort(items, Comparator.comparing(Node::getTextContent));

            return items;
        } catch (Exception e) {
            e.printStackTrace();
            throw e;
        }
    }


    private class XPathFunctionImpl implements XPathFunction {

        private final Class<?> m_handler;
        private final String m_funcName;

        /**
         * Construct an instance of XPathFunctionImpl from the
         * ExtensionHandler and function name.
         *
         * @param handler  Class that implements the function
         * @param funcName  Name of the function
         */
        private XPathFunctionImpl(Class<?> handler, String funcName) {
            m_handler = handler;
            m_funcName = funcName;
        }

        public Object evaluate(List args) throws XPathFunctionException {
            Class[] clsArr = new Class[args.size()];
            Object[] objArr = new Object[args.size()];
            for (int i = 0; i < args.size(); i++) {
                clsArr[i] = args.get(i).getClass();
                objArr[i] = args.get(i);
            }
            try {
                Method callMethod = m_handler.getMethod(m_funcName, clsArr);
                return callMethod.invoke(m_handler, objArr);
            } catch (NoSuchMethodException e) {
                fail("External method " + m_funcName + " not found: " + e.getMessage());
                return null;
            } catch (Exception e) {
                throw new XPathFunctionException(e);
            }
        }
    }

    public void testTopLevelWhitespaceText() throws Exception {
        // effect of adding top-level whitespace text node
        String stylesheet =
            "<?xml version=\"1.0\"?> \n" +
                "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                "<xsl:template match='/'>\n" +
                "<a/>\n" +
                "<xsl:text>   </xsl:text>" +
                "</xsl:template>\n" +
                "</xsl:stylesheet>";

        try {
            TransformerFactory tFactory = new EnterpriseTransformerFactory();
            Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(stylesheet)));
            Transformer transformer = templates.newTransformer();
            StreamSource source = new StreamSource(new StringReader("<doc/>"));
            DocumentBuilderFactory documentBuilderFactory = DocumentBuilderFactory.newInstance();
            Document document = documentBuilderFactory.newDocumentBuilder().newDocument();
            transformer.transform(source, new DOMResult(document));
            assertEquals("result", "a", document.getDocumentElement().getNodeName());
        } catch (TransformerException e) {
            fail(e.getMessage());
            e.printStackTrace();
        }

    }

    public void testTopLevelNonWhitespaceText() throws Exception {
        // effect of adding top-level whitespace text node
        String stylesheet =
            "<?xml version=\"1.0\"?> \n" +
                "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                "<xsl:template match='/'>\n" +
                "<a/>\n" +
                "<xsl:text>foo</xsl:text>" +
                "</xsl:template>\n" +
                "</xsl:stylesheet>";

        try {
            TransformerFactory tFactory = new EnterpriseTransformerFactory();
            Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(stylesheet)));
            Transformer transformer = templates.newTransformer();
            StreamSource source = new StreamSource(new StringReader("<doc/>"));
            DocumentBuilderFactory documentBuilderFactory = DocumentBuilderFactory.newInstance();
            Document document = documentBuilderFactory.newDocumentBuilder().newDocument();
            transformer.transform(source, new DOMResult(document));
            fail("error not detected");
        } catch (TransformerException e) {
            // OK, no action;
        }

    }

    public void testIdentityTransformOfDOMTextNode() throws Exception {
        DocumentBuilderFactory documentBuilderFactory = DocumentBuilderFactory.newInstance();
        Document document = documentBuilderFactory.newDocumentBuilder().newDocument();
        Element spanElement = document.createElement("span");
        document.appendChild(spanElement);
        Text textNode = document.createTextNode("some text");
        spanElement.appendChild(textNode);
        Comment commentNode = document.createComment("a comment");
        spanElement.appendChild(commentNode);
        ProcessingInstruction piNode = document.createProcessingInstruction("tgt", "some data");
        spanElement.appendChild(piNode);

        TransformerFactory transformerFactory = new TransformerFactoryImpl();
        Transformer transformer = transformerFactory.newTransformer();
        transformer.setOutputProperty(OutputKeys.OMIT_XML_DECLARATION, "yes");
        transformer.setOutputProperty(OutputKeys.METHOD, "xml");

        StringWriter sw = new StringWriter();
        transformer.transform(new DOMSource(textNode), new StreamResult(sw));
        assertEquals("text node", sw.toString(), "some text");

        sw = new StringWriter();
        transformer.transform(new DOMSource(commentNode), new StreamResult(sw));
        assertEquals("text node", sw.toString(), "<!--a comment-->");

        sw = new StringWriter();
        transformer.transform(new DOMSource(piNode), new StreamResult(sw));
        assertEquals("text node", sw.toString(), "<?tgt some data?>");
    }

    public void testIdentityTransformOfEmptyDOMSource() throws Exception {
        DocumentBuilderFactory documentBuilderFactory = DocumentBuilderFactory.newInstance();
        Document document = documentBuilderFactory.newDocumentBuilder().newDocument();

        TransformerFactory transformerFactory = new TransformerFactoryImpl();

        Transformer transformer = transformerFactory.newTransformer();
        transformer.setOutputProperty(OutputKeys.OMIT_XML_DECLARATION, "yes");
        transformer.setOutputProperty(OutputKeys.METHOD, "xml");
        for (Source source : new Source[]{
            new DOMSource(document),
            new DOMSource(),
            //new StreamSource(),
        }) {
            System.err.println("with DOMSource(" + ((DOMSource)source).getNode() + ")");
            StringWriter sw = new StringWriter();
            transformer.transform(source, new StreamResult(sw));
            assertTrue(sw.toString().isEmpty());
        }
    }

    /**
     * Test case(s) from T-Mobile, 8 April 2010: validate an element in a DOM that is not the root node
     */

    public void testElementValidationYN() {
        tryElementValidation(true, false);
    }

    public void testElementValidationYY() {
        tryElementValidation(true, true);
    }

    public void testElementValidationNN() {
        tryElementValidation(false, false);
    }

    public void testElementValidationNY() {
        tryElementValidation(false, true);
    }

    private void tryElementValidation(boolean withPrefix, boolean namespaceAware) {

        try {
            String prefixXml = "<n0:bar xmlns:n0=\"http://bar\">\n" +
                "    <foo xmlns=\"http://foo\"></foo>\n" +
                "</n0:bar>";

            String noPrefixXml = "<bar xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n" +
                " xsi:schemaLocation=\"http://bar bar.xsd\"\n" +
                " xmlns=\"http://bar\">\n" +
                "    <foo xmlns=\"http://foo\"></foo>\n" +
                "</bar>";

            String barXsd = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
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

            String fooXsd = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
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


            Configuration config = new EnterpriseConfiguration();

            config.addSchemaSource(new StreamSource(new StringReader(fooXsd)));
            config.addSchemaSource(new StreamSource(new StringReader(barXsd)));

            StaticQueryContext ctx = config.newStaticQueryContext();
            XQueryExpression xq = ctx.compileQuery("validate{.}");
            final DynamicQueryContext dynamicContext = new DynamicQueryContext(config);


            // First try with a namespaceAware factory, then with non-namespaceAware
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(namespaceAware);

            // Assume we're running with Xerces, but if not, it doesn't really affect the test
            assertEquals("xerces?", "org.apache.xerces.jaxp.DocumentBuilderFactoryImpl", factory.getClass().getName());

            DocumentBuilder domBuilder = factory.newDocumentBuilder();

            // Process XML source that uses namespace prefixes
            Document doc = domBuilder.parse(new InputSource(new StringReader(withPrefix ? prefixXml : noPrefixXml)));

            Node node = doc.getDocumentElement().getElementsByTagName("foo").item(0);
            if (namespaceAware) {
                assertEquals(node.getNamespaceURI(), "http://foo");
            } else {
                assertNull(node.getNamespaceURI());
            }

            dynamicContext.setContextItem(build(new DOMSource(node), config));

            List result = xq.evaluate(dynamicContext);
            assertEquals(result.size(), 1);
            assertEquals(((NodeInfo) result.get(0)).getURI(), "http://foo");


        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    public static NodeInfo build(Source source, Configuration config) throws Exception {
        // Create a pipeline configuration
        PipelineConfiguration pipe = config.makePipelineConfiguration();

        // Create an appropriate Builder
        Builder b = new LinkedTreeBuilder(pipe);

        Sender.send(source, b, null);

        // Get the constructed document
        NodeInfo newdoc = b.getCurrentRoot();

        // Reset the builder, detaching it from the constructed document
        b.reset();

        // If requested, close the input stream
        if (source instanceof AugmentedSource && ((AugmentedSource) source).isPleaseCloseAfterUse()) {
            ((AugmentedSource) source).close();
        }
        return newdoc;
    }

//    public void testSaxonDocumentBuilder() {
//        try {
//            DocumentBuilderFactoryImpl domFactory = new net.sf.saxon.dom.DocumentBuilderFactoryImpl();
//            Configuration config = (Configuration)domFactory.getAttribute(FeatureKeys.CONFIGURATION);
//            domFactory.setNamespaceAware(true);
//            XPathFactory xpathFactory = new net.sf.saxon.xpath.XPathFactoryImpl(config);
//
//            String in = "<?xml version='1.0'?> \n" +
//                        "<files> <file name='a' md5='true'/> <file name='b' md5='false'/> </files>";
//
//            // Read an XML file to a DOM
//            InputSource is = new InputSource(new StringReader(in));
//            Document dom = domFactory.newDocumentBuilder().parse(is);
//            XPath xp = xpathFactory.newXPath();
//            NodeList result = (NodeList)xp.evaluate("/files/file", dom, XPathConstants.NODESET);
//            Properties ret = new Properties();
//            for (int i=0; i<result.getLength(); i++) {
//                Element file = (Element)result.item(i);
//                ret.setProperty(file.getAttribute("name"), file.getAttribute("md5"));
//            }
//            assertTrue("result", ret.getProperty("a").equals("true"));
//        } catch (SAXException e) {
//            e.printStackTrace();
//            fail(e.getMessage());
//        } catch (IOException e) {
//            e.printStackTrace();
//            fail(e.getMessage());
//        } catch (ParserConfigurationException e) {
//            e.printStackTrace();
//            fail(e.getMessage());
//        } catch (XPathExpressionException e) {
//            e.printStackTrace();
//            fail(e.getMessage());
//        }
//
//    }

    /**
     * DOM-to-DOM identity transform: Copy an element in a DOM that is not the root node
     */

    public void testElementCopyYN() {
        tryElementCopy(true, false);
    }

    public void testElementCopyYY() {
        tryElementCopy(true, true);
    }

    public void testElementCopyNN() {
        tryElementCopy(false, false);
    }

    public void testElementCopyNY() {
        tryElementCopy(false, true);
    }

    private void tryElementCopy(boolean withPrefix, boolean namespaceAware) {

        try {
            String prefixXml = "<n0:bar xmlns:n0=\"http://bar\">\n" +
                "    <foo xmlns=\"http://foo\" a='1' n0:b='2'><zing/><n0:zang/></foo>\n" +
                "</n0:bar>";

            String noPrefixXml = "<bar xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n" +
                " xsi:schemaLocation=\"http://bar bar.xsd\"\n" +
                " xmlns=\"http://bar\">\n" +
                "    <foo xmlns=\"http://foo\" a='1' xsi:b='2'><zing/><xml:zang/></foo>\n" +
                "</bar>";


            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(namespaceAware);

            // Assume we're running with Xerces, but if not, it doesn't really affect the test
            assertEquals("xerces?", "org.apache.xerces.jaxp.DocumentBuilderFactoryImpl", factory.getClass().getName());

            DocumentBuilder domBuilder = factory.newDocumentBuilder();

            // Process XML source
            Document doc = domBuilder.parse(new InputSource(new StringReader(withPrefix ? prefixXml : noPrefixXml)));

            Node node = doc.getDocumentElement().getElementsByTagName("foo").item(0);
            if (namespaceAware) {
                assertEquals(node.getNamespaceURI(), "http://foo");
            } else {
                assertNull(node.getNamespaceURI());
            }

            Transformer t = new TransformerFactoryImpl().newTransformer();
            DOMResult result = new DOMResult();
            t.transform(new DOMSource(node), result);

            Configuration config = new Configuration();
            NodeInfo one = DOMObjectModel.getInstance().wrap(node, config);
            NodeInfo two = DOMObjectModel.getInstance().wrap(((Document)result.getNode()).getDocumentElement(), config);
            XPathContext context = config.getConversionContext();
            boolean comparison = net.sf.saxon.functions.DeepEqual.deepEqual(
                one.iterate(), two.iterate(), CodepointCollatingComparer.getInstance(), context, 0);
            assertTrue("Copy is not deep-equal", comparison);

        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }


    public void testTypedDom() {
        try {
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            DocumentBuilder domBuilder = factory.newDocumentBuilder();

            // Process XML source that uses namespace prefixes
            Document doc = domBuilder.parse(new InputSource(new FileInputStream(ConfigTest.DATA_DIR + "books.xml")));

            SchemaFactory sf = null; //SchemaFactory.newInstance(XMLConstants.W3C_XML_SCHEMA_NS_URI);
            sf = new SchemaFactoryImpl();
            Schema s = sf.newSchema(new File(ConfigTest.DATA_DIR + "books.xsd"));
            Validator v = s.newValidator();
            DOMResult r = new DOMResult();
            v.validate(new DOMSource(doc), r);
            Document d2 = (Document) r.getNode();
            Element e = d2.getDocumentElement();
            TypeInfo ti = e.getSchemaTypeInfo();
            assertNull("TypeInfo null", ti.getTypeName());
            //System.out.println(ti.getTypeName());
            // This doesn't actually work. Saxon has no way of setting the type info in the dom, because there
            // is no setTypeInfo() method.

        } catch (ParserConfigurationException | IOException | SAXException e1) {
            fail(e1.getMessage());
            e1.printStackTrace();
        }
    }

    public void testTypedDom11() {
        try {
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            DocumentBuilder domBuilder = factory.newDocumentBuilder();

            // Process XML source that uses namespace prefixes
            Document doc = domBuilder.parse(new InputSource(new FileInputStream(ConfigTest.DATA_DIR + "books.xml")));

            SchemaFactory sf = null; //SchemaFactory.newInstance(XMLConstants.W3C_XML_SCHEMA_NS_URI);
            sf = new SchemaFactoryImpl();
            sf.setProperty(FeatureKeys.XSD_VERSION, "1.1");
            Schema s = sf.newSchema(new File(ConfigTest.DATA_DIR + "books11.xsd"));
            Validator v = s.newValidator();
            DOMResult r = new DOMResult();
            v.validate(new DOMSource(doc), r);
            Document d2 = (Document) r.getNode();
            Element e = d2.getDocumentElement();
            TypeInfo ti = e.getSchemaTypeInfo();
            assertNull("TypeInfo null", ti.getTypeName());
            // This doesn't actually work. Saxon has no way of setting the type info in the dom, because there
            // is no setTypeInfo() method.

        } catch (ParserConfigurationException | IOException | SAXException e1) {
            fail(e1.getMessage());
            e1.printStackTrace();
        }
    }

    public void testReusableValidator() {
        try {
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            DocumentBuilder domBuilder = factory.newDocumentBuilder();


            Document doc = domBuilder.parse(new InputSource(new FileInputStream(ConfigTest.DATA_DIR + "books.xml")));

            SchemaFactory sf = null; //SchemaFactory.newInstance(XMLConstants.W3C_XML_SCHEMA_NS_URI);
            sf = new SchemaFactoryImpl();
            Schema s = sf.newSchema(new File(ConfigTest.DATA_DIR + "books.xsd"));
            Validator v = s.newValidator();
            DOMResult r = new DOMResult();
            v.validate(new DOMSource(doc), r);
            Document d2 = (Document) r.getNode();
            Element e = d2.getDocumentElement();

            DOMResult r2 = new DOMResult();
            v.validate(new DOMSource(e), r2);

        } catch (ParserConfigurationException | IOException | SAXException e1) {
            fail(e1.getMessage());
            e1.printStackTrace();
        }
    }


    public void testNullNamespace() {
        try {
            String xml =

                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                    "<root xmlns=\"abc\">" +
                    "<name xmlns=\"\">" +
                    "<first>Joe</first>" +
                    "<last>Blow</last>" +
                    "</name>" +
                    "</root>";

            String path =
                "/*[local-name(.)=\"root\" and namespace-uri(.)=\"abc\"]" +
                    "/*[local-name(.)=\"name\" and namespace-uri(.)=\"\"]" +
                    "/*[local-name(.)=\"first\" and namespace-uri(.)=\"\"]/text()";

            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            DocumentBuilder builder = factory.newDocumentBuilder();
            Document doc = builder.parse(new InputSource(new StringReader(xml)));

            XPathFactory xPathFactory = //XPathFactory.newInstance();
                new XPathFactoryImpl();
            if (!"net.sf.saxon.xpath.XPathFactoryImpl".equals(xPathFactory.getClass().getName())) {
                throw new RuntimeException();
            }

            XPath xpath = xPathFactory.newXPath();
            NodeList list = (NodeList) xpath.evaluate(path, doc, XPathConstants.NODESET);

            System.out.println(Integer.toString(list.getLength()));
            assertEquals(1, list.getLength());
            for (int i = 0; i < list.getLength(); i++) {
                Node n = list.item(i);
                System.out.println(n);
            }


        } catch (Exception err) {
            fail();
        }
    }

    public void testNullNamespaceNonNamespaceAware() {
        try {
            String xml =

                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                    "<root xmlns=\"abc\">" +
                    "<name xmlns=\"\">" +
                    "<first>Joe</first>" +
                    "<last>Blow</last>" +
                    "</name>" +
                    "</root>";

            String path =
                "/*[local-name(.)=\"root\" and namespace-uri(.)=\"abc\"]" +
                    "/*[local-name(.)=\"name\" and namespace-uri(.)=\"\"]" +
                    "/*[local-name(.)=\"first\" and namespace-uri(.)=\"\"]/text()";

            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(false);
            DocumentBuilder builder = factory.newDocumentBuilder();
            Document doc = builder.parse(new InputSource(new StringReader(xml)));

            XPathFactory xPathFactory = //XPathFactory.newInstance();
                new XPathFactoryImpl();
            if (!"net.sf.saxon.xpath.XPathFactoryImpl".equals(xPathFactory.getClass().getName())) {
                throw new RuntimeException();
            }

            XPath xpath = xPathFactory.newXPath();
            NodeList list = (NodeList) xpath.evaluate(path, doc, XPathConstants.NODESET);

            System.out.println(Integer.toString(list.getLength()));
            assertEquals(1, list.getLength());
            for (int i = 0; i < list.getLength(); i++) {
                Node n = list.item(i);
                System.out.println(n);
            }


        } catch (Exception err) {
            fail();
        }
    }


    public void testBug1781() {
        try {
            Configuration config = new Configuration();
            DOMWriter domWriter = new DOMWriter();
            domWriter.setPipelineConfiguration(config.makePipelineConfiguration());
            domWriter.open();
            domWriter.startDocument(0);
            domWriter.startElement(new NoNamespaceName("root"), Untyped.getInstance(), ExplicitLocation.UNKNOWN_LOCATION, 0);
            domWriter.attribute(new NoNamespaceName("id"), BuiltInAtomicType.UNTYPED_ATOMIC, "iii", ExplicitLocation.UNKNOWN_LOCATION, ReceiverOptions.IS_ID);
            domWriter.endElement();
            domWriter.endDocument();
            domWriter.close();
            assertEquals(domWriter.getCurrentRoot().getStringValue(), "");
        } catch (Exception err) {
            err.printStackTrace();
            fail();
        }
    }

    public void testEmptyDom() {
        // Bug 2102
        try {
            DOMSource source = new DOMSource();
            StringWriter sw = new StringWriter();
            TransformerFactory factory = new TransformerFactoryImpl();
            String ss = "<z xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xsl:version='2.0' count='{count(//*)}'/>";
            Transformer transformer = factory.newTransformer(new StreamSource(new StringReader(ss)));
            transformer.transform(source, new StreamResult(sw));
            assertEquals(sw.toString(), "<?xml version=\"1.0\" encoding=\"UTF-8\"?><z count=\"0\"/>");
        } catch (TransformerException e1) {
            fail(e1.getMessage());
            e1.printStackTrace();
        }
    }

    public void testEmptyDomResult() {
        // Bug 2164
        try {
            TransformerFactory factory = new TransformerFactoryImpl();
            String ss = "<z xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xsl:version='2.0' count='{count(//*)}'/>";
            Transformer transformer = factory.newTransformer(new StreamSource(new StringReader(ss)));
            DOMResult result = new DOMResult();
            transformer.transform(new StreamSource(new StringReader(ss)), result);
            assertEquals("1", ((Document) result.getNode()).getDocumentElement().getAttribute("count"));
        } catch (TransformerException e1) {
            fail(e1.getMessage());
            e1.printStackTrace();
        }
    }

    public void testSpaceStrippedDomWithExtensionFunction() {
        // Bug 3142
        try {
            String xml =

                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                            "<root xmlns=\"abc\">" +
                            "<name xmlns=\"\">" +
                            "<first>Joe</first>" +
                            "<last>Blow</last>" +
                            "</name>" +
                            "</root>";

            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(false);
            DocumentBuilder builder = factory.newDocumentBuilder();
            Document doc = builder.parse(new InputSource(new StringReader(xml)));

            TransformerFactory tfactory = new TransformerFactoryImpl();
            tfactory.setAttribute(FeatureKeys.GENERATE_BYTE_CODE, false);
            String ss = "<xsl:transform xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='2.0'> "
                    + "<xsl:strip-space elements='*'/>"
                    + "<xsl:template match='/'><z count='{ext:count(//*)}' xmlns:ext='java:jaxptest.DOMTest'/></xsl:template>"
                    + "</xsl:transform>";
            Transformer transformer = tfactory.newTransformer(new StreamSource(new StringReader(ss)));
            DOMResult result = new DOMResult();
            transformer.transform(new DOMSource(doc), result);
            assertEquals("4", ((Document) result.getNode()).getDocumentElement().getAttribute("count"));
        } catch (Exception e1) {
            fail(e1.getMessage());
            e1.printStackTrace();
        }
    }

    public static int count(NodeList nodes) {
        return nodes.getLength();
    }

    /**
     * Test subclass of DOMSource (bug 3241)
     */

    public void testDOMSourceSubclass() {
        try {
            String xml = "<test>[theta]</test>";

            // Read an XML file to a DOM
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            DocumentBuilder db = factory.newDocumentBuilder();
            InputSource is = new InputSource(new StringReader(xml));
            Document doc = db.parse(is);

            String stylesheet =
                    "<?xml version=\"1.0\"?> \n" +
                            "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                            "<xsl:template match='/'>\n" +
                            "<out><xsl:value-of select='.'/></out>\n" +
                            "</xsl:template>\n" +
                            "</xsl:stylesheet>";

            TransformerFactory tFactory = new ProfessionalTransformerFactory();
            Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(stylesheet)));
            Transformer transformer = templates.newTransformer();
            StringWriter sw = new StringWriter();
            transformer.transform(new SubSource(doc), new StreamResult(sw));
            //System.err.println(sw.toString());
            assertTrue(sw.toString().contains("<out>[theta]</out>"));
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    private static class SubSource extends DOMSource {
        public SubSource(Node node) {
            super(node);
        }
    }

    public void testBug3164() {
        try {
            String xml = "<test width='5'></test>";
            String stylesheet =
                    "<?xml version=\"1.0\"?> \n" +
                            "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                            "<xsl:template match='/'>\n" +
                            "<out><xsl:value-of select='ext:foo(/)' xmlns:ext='java:jaxptest.DOMTest'/></out>\n" +
                            "</xsl:template>\n" +
                            "</xsl:stylesheet>";
            TransformerFactory tFactory = new ProfessionalTransformerFactory();
            //tFactory.setAttribute(FeatureKeys.TRACE_EXTERNAL_FUNCTIONS, true);
            Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(stylesheet)));
            Transformer transformer = templates.newTransformer();
            StringWriter sw = new StringWriter();
            transformer.transform(new StreamSource(new StringReader(xml)), new StreamResult(sw));
            //System.err.println(sw.toString());
            assertTrue(sw.toString().contains("5"));
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    public static Node foo(Document commandNode) {
        Element command = commandNode.getDocumentElement();
        System.out.println("xxx get start");
        Attr attributeNode = command.getAttributeNode("width");
        System.out.println("xxx get end");
        return attributeNode;
    }

    public void testBug4121() {
        Transformer transformer;
        Transformer identityTransformer;

        String xslt =
                "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>\n" +
                        "  <xsl:template match='/'>\n" +
                        "    <xsl:variable name='node'>\n" +
                        "      <xsl:sequence select='.'/>\n" +
                        "    </xsl:variable>\n" +
                        "    <xsl:sequence select='$node/node()'/>\n" +
                        "  </xsl:template>\n" +
                        "</xsl:stylesheet>";

        try {
            TransformerFactory transformerFactory = new SaxonTransformerFactory();
            transformer = transformerFactory.newTransformer(new StreamSource(new StringReader(xslt)));
            transformer.setOutputProperty("omit-xml-declaration", "yes");
            identityTransformer = transformerFactory.newTransformer();
            identityTransformer.setOutputProperty("omit-xml-declaration", "yes");

            String input = "<input/>";
            DocumentBuilder documentBuilder = DocumentBuilderFactory.newInstance().newDocumentBuilder();
            Document document = documentBuilder.newDocument();
            transformer.transform(new StreamSource(new StringReader(input)), new DOMResult(document));
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            Result result = new StreamResult(baos);
            identityTransformer.transform(new DOMSource(document), result);
            String output = new String(baos.toByteArray(), StandardCharsets.UTF_8);
            assertEquals(output, input);
        } catch (ParserConfigurationException e) {
            e.printStackTrace();
            fail();
        } catch (TransformerException e) {
            e.printStackTrace();
            fail();
        }

    }


}

//
// The contents of this file are subject to the Mozilla Public License Version
// 1.0 (the "License");
// you may not use this file except in compliance with the License. You may
// obtain a copy of the
// License at http://www.mozilla.org/MPL/
//
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either express or implied.
// See the License for the specific language governing rights and limitations
// under the License.
//
// The Original Code is: all this file.
//
// The Initial Developer of the Original Code is Michael Kay,
//
// Portions created by (your name) are Copyright (C) (your legal entity). All
// Rights Reserved.
//
// Contributor(s): none.
//



