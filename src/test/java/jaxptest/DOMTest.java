package jaxptest;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.config.EnterpriseTransformerFactory;
import com.saxonica.config.ProfessionalTransformerFactory;
import com.saxonica.ee.domino.DominoTree;
import com.saxonica.ee.jaxp.SchemaFactoryImpl;
import net.sf.saxon.Configuration;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.dom.*;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.expr.sort.CodepointCollatingComparer;
import net.sf.saxon.jaxp.SaxonTransformerFactory;
import net.sf.saxon.lib.AugmentedSource;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.om.*;
import net.sf.saxon.query.DynamicQueryContext;
import net.sf.saxon.query.StaticQueryContext;
import net.sf.saxon.query.XQueryExpression;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
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
import org.junit.jupiter.api.Test;
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

import static java.lang.System.err;
import static net.sf.saxon.s9api.streams.Predicates.isText;
import static net.sf.saxon.s9api.streams.Steps.child;
import static net.sf.saxon.s9api.streams.Steps.descendant;
import static net.sf.saxon.s9api.streams.Steps.followingSibling;
import static net.sf.saxon.s9api.streams.Steps.precedingSibling;
import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.assertNull;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

//import org.apache.xpath.NodeSet;

/**
 *  Tests of transformations with DOM input or output
 */
public class DOMTest {
    private final ConfigTest configTest = new ConfigTest();

    /**
     * Test that namespaces are copied OK by an identity transform
     */

    @Test
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
            assertTrue(sw.toString().contains("http://lost.namespace.com/"), "Namespace is present in output");
            assertTrue(sw.toString().contains("xmlns:xsi="), "Namespace is present in output");
            assertTrue(sw.toString().contains("xsi:value"), "Attribute is present in output");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }


    @Test
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
     * Get the string value of a document node
     */

    @Test
    public void testStringValueOfDocHE() {
        testStringValueOfDoc(new TransformerFactoryImpl());
    }

    @Test
    public void testStringValueOfDocPE() {
        testStringValueOfDoc(new ProfessionalTransformerFactory());
    }

    @Test
    public void testStringValueOfDocEE() {
        testStringValueOfDoc(new EnterpriseTransformerFactory());
    }

    private void testStringValueOfDoc(TransformerFactory tFactory) {
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

    /**
     * Test calling an extension function when the stylesheet has stripped whitespace (virtually)
     */

    @Test
    public void testStripSpaceHE() {
        testStripSpace(new TransformerFactoryImpl());
    }

    @Test
    public void testStripSpacePE() {
        testStripSpace(new ProfessionalTransformerFactory());
    }

    @Test
    public void testStripSpaceEE() {
        testStripSpace(new EnterpriseTransformerFactory());
    }

    private void testStripSpace(TransformerFactory tFactory) {
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
            assertTrue(sw.toString().contains("[f][f]{e}"), "Extension function not called");
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

    @Test
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
            assertTrue(nodeInfo instanceof VirtualNode, "not-copied");
            NodeInfo elemInfo = DOMObjectModel.getInstance().copy(ele, TreeModel.TINY_TREE, new Configuration());
            assertFalse(elemInfo instanceof VirtualNode, "copied");
            assertEquals(Type.ELEMENT, elemInfo.getNodeKind(), "is-element");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    /**
     * Test that Saxon treats a text node with "null" content as if it had zero-length content
     */

    @Test
    public void testNullTextNodes() {
        try {
            Configuration config = new Configuration();
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
            DocumentWrapper wrapper = new DocumentWrapper(doc, "http://base.uri/", config);
            StringWriter sw = new StringWriter();
            Receiver out = config.getSerializerFactory().getReceiver(new StreamResult(sw));
            wrapper.getRootNode().copy(out, CopyOptions.ALL_NAMESPACES | CopyOptions.TYPE_ANNOTATIONS, Loc.NONE);
            assertEquals("<?xml version=\"1.0\" encoding=\"UTF-8\"?><top/>", sw.toString(), "wrong output " + sw);
        } catch (ParserConfigurationException | XPathException e) {
            fail(e.getMessage());
        } catch (Exception err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }


    @Test
    public void testNullTextNodesDomino() {
        try {
            Configuration config = new EnterpriseConfiguration();
            Document doc = makeDocWithNullTextNode();
            NodeInfo wrapper = DominoTree.makeTree(doc, config, "http://base.uri/").getRootNode();
            try (StringWriter sw = new StringWriter()) {
                Receiver out = config.getSerializerFactory().getReceiver(new StreamResult(sw));
                wrapper.copy(out, CopyOptions.ALL_NAMESPACES | CopyOptions.TYPE_ANNOTATIONS, Loc.NONE);
                assertEquals("<?xml version=\"1.0\" encoding=\"UTF-8\"?><top/>", sw.toString(), "wrong output " + sw);
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

    @Test
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

    @Test
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
        assertEquals("text1text2", text.getStringValue(), "t1");

        count = docNode.select(descendant()).count();
        assertEquals(2, count, "t2");
        text = docNode.select(descendant(isText())).asNode();
        assertEquals("text1text2", text.getStringValue(), "t3");

        count = top.select(descendant()).count();
        assertEquals(1, count, "t4");
        text = top.select(descendant()).asNode();
        assertEquals("text1text2", text.getStringValue(), "t5");

    }

    private void navigateOverAdjacentTextNodesMixed(XdmNode docNode) {
        XdmNode top = docNode.select(child()).asNode();

        long count = top.select(child()).count();
        assertEquals(3, count);
        XdmNode text = top.select(child(isText())).asNode();
        assertEquals("text1text2", text.getStringValue(), "t1");

        count = docNode.select(descendant()).count();
        assertEquals(4, count);
        text = docNode.select(descendant(isText())).asNode();
        assertEquals("text1text2", text.getStringValue(), "t2");

        count = top.select(descendant()).count();
        assertEquals(3, count);
        text = top.select(descendant(isText())).asNode();
        assertEquals("text1text2", text.getStringValue(), "t3");

        XdmNode alpha = text.select(precedingSibling()).asNode();
        assertEquals("alpha", alpha.getNodeName().getLocalName(), "t4");

        XdmNode omega = text.select(followingSibling()).asNode();
        assertEquals("omega", omega.getNodeName().getLocalName(), "t5");

        XdmNode preOmega = omega.select(precedingSibling()).first().asNode();
        assertEquals("text1text2", preOmega.getStringValue(), "t6");

        XdmNode prePreOmega = preOmega.select(precedingSibling()).first().asNode();
        assertEquals("alpha", prePreOmega.getNodeName().getLocalName(), "t7");

        NodeInfo omegaInfo = omega.getUnderlyingNode();
        if (omegaInfo instanceof SteppingNode) {
            NodeInfo preOmegaInfo = ((SteppingNode) omegaInfo).getPreviousSibling();
            assertEquals("text1text2", preOmegaInfo.getStringValue(), "t8");
            NodeInfo prePreOmegaInfo = ((SteppingNode) preOmegaInfo).getPreviousSibling();
            assertEquals("alpha", prePreOmegaInfo.getLocalPart(), "t9");
        }

    }

    @Test
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

    @Test
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


    @Test
    public void testAdjacentTextNodesDomino() {
        try {
            Configuration config = new EnterpriseConfiguration();
            Document doc = makeDocWithAdjacentTextNodes(false);
            NodeInfo wrapper = DominoTree.makeTree(doc, config, "http://base.uri/").getRootNode();
            StringWriter sw = new StringWriter();
            Receiver out = config.getSerializerFactory().getReceiver(new StreamResult(sw));
            wrapper.copy(out, CopyOptions.ALL_NAMESPACES | CopyOptions.TYPE_ANNOTATIONS, Loc.NONE);
            assertEquals("<?xml version=\"1.0\" encoding=\"UTF-8\"?><top>text1text2</top>", sw.toString(), "wrong output " + sw);
            XPathFactoryImpl xf = new net.sf.saxon.xpath.XPathFactoryImpl();
            xf.setConfiguration(config);
            XPath xp = xf.newXPath();
            String result = xp.evaluate("count(.//text())", wrapper);
            assertEquals("1", result, "number of text nodes ");
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

    @Test
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
            assertEquals(3, isList.getLength(), "length");
        } catch (ParserConfigurationException | XPathExpressionException | IOException | SAXException e) {
            fail(e.getMessage());
        }
    }

    /**
     * Test JAXP XPath API delivering a DOM NodeList given a DOMSource as input
     */

    @Test
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
            assertEquals(3, isList.getLength(), "length");
        } catch (ParserConfigurationException | SAXException | IOException | XPathExpressionException e) {
            fail(e.getMessage());
        }
    }

    /**
     * Test JAXP XPath API delivering a DOM NodeList given a Saxon DOM NodeWrapper
     */

    @Test
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
            assertEquals(3, isList.getLength(), "length");
        } catch (ParserConfigurationException | SAXException | IOException | XPathExpressionException e) {
            fail(e.getMessage());
        }
    }

    @Test
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
            assertTrue(sw.toString().contains("<a>5</a><b>0</b><c>0</c>"), sw.toString());
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

    @Test
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
                @Override
                public String getNamespaceURI(String prefix) {
                    return prefix.equals("ext") ? "java:jaxptest.DOMTest" : null;
                }

                @Override
                public String getPrefix(String namespaceURI) {
                    return null;
                }

                @Override
                public Iterator getPrefixes(String namespaceURI) {
                    return null;
                }
            });
            NodeList isList = (NodeList) xpe.evaluate(exp, doc, XPathConstants.NODESET);
            assertEquals(4, isList.getLength(), "length");
        } catch (ParserConfigurationException | SAXException | IOException | XPathExpressionException e) {
            fail(e.getMessage());
            e.printStackTrace();
        }
    }

    /**
     * Variant of above test using an extension function that returns a NodeList
     * implemented using the JAXP FunctionResolver
     */

    @Test
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
            assertEquals(4, isList.getLength(), "length");

            isList = (NodeList) xpath.evaluate("ext:sortArrayToNodeList(//book)", doc, XPathConstants.NODESET);

            assertEquals(4, isList.getLength(), "length");

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
            @Override
            public String getNamespaceURI(String prefix) {
                return prefix.equals("ext") ? "http://extString" : null;
            }

            @Override
            public String getPrefix(String namespaceURI) {
                return null;
            }

            @Override
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

            @Override
            public Node item(int index) {
                return textNodes.get(index);
            }

            @Override
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
         * @param funcName Name of the function
         */
        private XPathFunctionImpl(Class<?> handler, String funcName) {
            m_handler = handler;
            m_funcName = funcName;
        }

        @Override
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

    @Test
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
            assertEquals("a", document.getDocumentElement().getNodeName(), "result");
        } catch (TransformerException e) {
            fail(e.getMessage());
            e.printStackTrace();
        }

    }

    @Test
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

    @Test
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
        assertEquals("some text", sw.toString(), "text node");

        sw = new StringWriter();
        transformer.transform(new DOMSource(commentNode), new StreamResult(sw));
        assertEquals("<!--a comment-->", sw.toString(), "comment");

        sw = new StringWriter();
        transformer.transform(new DOMSource(piNode), new StreamResult(sw));
        assertEquals("<?tgt some data?>", sw.toString(), "processing instruction");
    }

    @Test
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
            err.println("with DOMSource(" + ((DOMSource) source).getNode() + ")");
            StringWriter sw = new StringWriter();
            transformer.transform(source, new StreamResult(sw));
            assertTrue(sw.toString().isEmpty());
        }
    }

    /**
     * Test case(s) from T-Mobile, 8 April 2010: validate an element in a DOM that is not the root node
     */

    @Test
    public void testElementValidationYN() {
        tryElementValidation(true, false);
    }

    @Test
    public void testElementValidationYY() {
        tryElementValidation(true, true);
    }

    @Test
    public void testElementValidationNN() {
        tryElementValidation(false, false);
    }

    @Test
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
            assertEquals("org.apache.xerces.jaxp.DocumentBuilderFactoryImpl", factory.getClass().getName(), "xerces?");

            DocumentBuilder domBuilder = factory.newDocumentBuilder();

            // Process XML source that uses namespace prefixes
            Document doc = domBuilder.parse(new InputSource(new StringReader(withPrefix ? prefixXml : noPrefixXml)));

            Node node = doc.getDocumentElement().getElementsByTagName("foo").item(0);
            if (namespaceAware) {
                assertEquals("http://foo", node.getNamespaceURI());
            } else {
                assertNull(node.getNamespaceURI());
            }

            dynamicContext.setContextItem(build(new DOMSource(node), config));

            List result = xq.evaluate(dynamicContext);
            assertEquals(1, result.size());
            assertEquals(NamespaceUri.of("http://foo"), ((NodeInfo) result.get(0)).getNamespaceUri());


        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    public static NodeInfo build(Source source, Configuration config) throws Exception {
        // Create a pipeline configuration
        PipelineConfiguration pipe = config.makePipelineConfiguration();

        // Create an appropriate Builder
        Builder b = new LinkedTreeBuilder(pipe, Durability.LASTING);

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

    @Test
    public void testElementCopyYY() {
        tryElementCopy(true, true);
    }

//    @Test
//    @Tag("expectedtofail")
//    public void testElementCopyNN() {
//        // MHK 2020-03-13: this is failing on 10.0. The call on getNamespaceURI() on the 'foo' element returns
//        // "", not the actual namespace http://foo. I think this is OK: we don't actually support a non-namespace-aware
//        // DOM, and we can't detect that the DOM is non-namespace-aware.
//        tryElementCopy(false, false);
//    }

    @Test
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
            assertEquals("org.apache.xerces.jaxp.DocumentBuilderFactoryImpl", factory.getClass().getName(), "xerces?");

            DocumentBuilder domBuilder = factory.newDocumentBuilder();

            // Process XML source
            Document doc = domBuilder.parse(new InputSource(new StringReader(withPrefix ? prefixXml : noPrefixXml)));

            Node node = doc.getDocumentElement().getElementsByTagName("foo").item(0);
            if (namespaceAware) {
                assertEquals("http://foo", node.getNamespaceURI());
            } else {
                assertNull(node.getNamespaceURI());
            }

            Transformer t = new TransformerFactoryImpl().newTransformer();
            DOMResult result = new DOMResult();
            t.transform(new DOMSource(node), result);

            Configuration config = new Configuration();
            NodeInfo one = DOMObjectModel.getInstance().wrap(node, config);
            NodeInfo two = DOMObjectModel.getInstance().wrap(((Document) result.getNode()).getDocumentElement(), config);
            XPathContext context = config.getConversionContext();
            boolean comparison = net.sf.saxon.functions.DeepEqual.deepEqual(
                    one.iterate(), two.iterate(), CodepointCollatingComparer.getInstance(), context, 0);
            assertTrue(comparison, "Copy is not deep-equal");

        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }


    @Test
    public void testTypedDom() {
        try {
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            DocumentBuilder domBuilder = factory.newDocumentBuilder();

            // Process XML source that uses namespace prefixes
            Document doc = domBuilder.parse(new InputSource(new FileInputStream(configTest.getDataDir() + "books.xml")));

            SchemaFactory sf = null; //SchemaFactory.newInstance(XMLConstants.W3C_XML_SCHEMA_NS_URI);
            sf = new SchemaFactoryImpl();
            Schema s = sf.newSchema(new File(configTest.getDataDir() + "books.xsd"));
            Validator v = s.newValidator();
            DOMResult r = new DOMResult();
            v.validate(new DOMSource(doc), r);
            Document d2 = (Document) r.getNode();
            Element e = d2.getDocumentElement();
            TypeInfo ti = e.getSchemaTypeInfo();
            assertNull(ti.getTypeName(), "TypeInfo null");
            //System.out.println(ti.getTypeName());
            // This doesn't actually work. Saxon has no way of setting the type info in the dom, because there
            // is no setTypeInfo() method.

        } catch (ParserConfigurationException | IOException | SAXException e1) {
            fail(e1.getMessage());
            e1.printStackTrace();
        }
    }

    @Test
    public void testTypedDom11() {
        try {
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            DocumentBuilder domBuilder = factory.newDocumentBuilder();

            // Process XML source that uses namespace prefixes
            Document doc = domBuilder.parse(new InputSource(new FileInputStream(configTest.getDataDir() + "books.xml")));

            SchemaFactory sf = null; //SchemaFactory.newInstance(XMLConstants.W3C_XML_SCHEMA_NS_URI);
            sf = new SchemaFactoryImpl();
            sf.setProperty(FeatureKeys.XSD_VERSION, "1.1");
            Schema s = sf.newSchema(new File(configTest.getDataDir() + "books11.xsd"));
            Validator v = s.newValidator();
            DOMResult r = new DOMResult();
            v.validate(new DOMSource(doc), r);
            Document d2 = (Document) r.getNode();
            Element e = d2.getDocumentElement();
            TypeInfo ti = e.getSchemaTypeInfo();
            assertNull(ti.getTypeName(), "TypeInfo null");
            // This doesn't actually work. Saxon has no way of setting the type info in the dom, because there
            // is no setTypeInfo() method.

        } catch (ParserConfigurationException | IOException | SAXException e1) {
            fail(e1.getMessage());
            e1.printStackTrace();
        }
    }

    @Test
    public void testReusableValidator() {
        try {
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            DocumentBuilder domBuilder = factory.newDocumentBuilder();


            Document doc = domBuilder.parse(new InputSource(new FileInputStream(configTest.getDataDir() + "books.xml")));

            SchemaFactory sf = null; //SchemaFactory.newInstance(XMLConstants.W3C_XML_SCHEMA_NS_URI);
            sf = new SchemaFactoryImpl();
            Schema s = sf.newSchema(new File(configTest.getDataDir() + "books.xsd"));
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


    @Test
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

            System.out.println(list.getLength());
            assertEquals(1, list.getLength());
            for (int i = 0; i < list.getLength(); i++) {
                Node n = list.item(i);
                System.out.println(n);
            }


        } catch (Exception err) {
            fail();
        }
    }

    @Test
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

            System.out.println(list.getLength());
            assertEquals(1, list.getLength());
            for (int i = 0; i < list.getLength(); i++) {
                Node n = list.item(i);
                System.out.println(n);
            }


        } catch (Exception err) {
            fail();
        }
    }


    @Test
    public void testBug1781() {
        try {
            Configuration config = new Configuration();
            DOMWriter domWriter = new DOMWriter();
            domWriter.setPipelineConfiguration(config.makePipelineConfiguration());
            domWriter.open();
            domWriter.startDocument(ReceiverOption.NONE);
            AttributeMap atts = EmptyAttributeMap.getInstance();
            atts = atts.put(new AttributeInfo(new NoNamespaceName("id"), BuiltInAtomicType.UNTYPED_ATOMIC, "iii", Loc.NONE, ReceiverOption.IS_ID));
            domWriter.startElement(new NoNamespaceName("root"), Untyped.getInstance(),
                                   atts, NamespaceMap.emptyMap(),
                                   Loc.NONE, ReceiverOption.NONE);
            domWriter.endElement();
            domWriter.endDocument();
            domWriter.close();
            assertEquals("", domWriter.getCurrentRoot().getStringValue());
        } catch (Exception err) {
            err.printStackTrace();
            fail();
        }
    }

    @Test
    public void testEmptyDom() {
        // Bug 2102
        try {
            DOMSource source = new DOMSource();
            StringWriter sw = new StringWriter();
            TransformerFactory factory = new TransformerFactoryImpl();
            String ss = "<z xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xsl:version='2.0' count='{count(//*)}'/>";
            Transformer transformer = factory.newTransformer(new StreamSource(new StringReader(ss)));
            transformer.transform(source, new StreamResult(sw));
            assertEquals("<?xml version=\"1.0\" encoding=\"UTF-8\"?><z count=\"0\"/>", sw.toString());
        } catch (TransformerException e1) {
            fail(e1.getMessage());
            e1.printStackTrace();
        }
    }

    @Test
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

    @Test
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

    @Test
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

    @Test
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

    @Test
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

    @Test
    public void testBug4509() {
        try {
            Processor proc = new Processor(false);
            net.sf.saxon.s9api.DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setTreeModel(DOMObjectModel.getInstance());
            XdmNode tree = builder.build(new StreamSource(new StringReader("<root xmlns='namespace'><a/><b/></root>")));
            Document doc = (Document) ((DOMNodeWrapper) tree.getUnderlyingNode()).getUnderlyingNode();
            Element root = doc.getDocumentElement();
            assertTrue(root.hasAttribute("xmlns"));
            Element a = (Element) root.getChildNodes().item(0);
            assertFalse(a.hasAttribute("xmlns"));
            assertEquals(0, a.getAttributes().getLength());
            Element b = (Element) root.getChildNodes().item(1);
            assertFalse(b.hasAttribute("xmlns"));
            assertEquals(0, b.getAttributes().getLength());
        } catch (SaxonApiException err) {
            fail(err.getMessage());
        }
    }

    @Test
    public void testBug4509a() {
        try {
            Processor proc = new Processor(false);
            net.sf.saxon.s9api.DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode tree = builder.build(new StreamSource(new StringReader("<root xmlns='namespace'><a/><b/></root>")));
            Document doc = (DocumentOverNodeInfo) DocumentOverNodeInfo.wrap(tree.getUnderlyingNode());
            Element root = doc.getDocumentElement();
            assertTrue(root.hasAttribute("xmlns"));
            Element a = (Element) root.getChildNodes().item(0);
            assertFalse(a.hasAttribute("xmlns"));
            assertEquals(0, a.getAttributes().getLength());
            Element b = (Element) root.getChildNodes().item(1);
            assertFalse(b.hasAttribute("xmlns"));
            assertEquals(0, b.getAttributes().getLength());
        } catch (SaxonApiException err) {
            fail(err.getMessage());
        }
    }




    /**
     * Method to be called from next test
     */

    public static NodeInfo makeBodyElement(XPathContext context) throws Exception {
        String in = "<?xml version='1.0'?> \n" +
                "<body xmlns=\"http://www.w3.org/1999/xhtml\">\n"
                + "        <p>Dit type is in IMKAD gemodelleerd,......</p>\n"
                + "</body>";

        // Read an XML file to a DOM
        DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
        factory.setNamespaceAware(true);
        DocumentBuilder db = factory.newDocumentBuilder();
        InputSource is = new InputSource(new StringReader(in));
        Document doc = db.parse(is);
        DocumentWrapper wrapper = new DocumentWrapper(doc, "http://dummy/", context.getConfiguration());
        return wrapper.wrap(doc.getDocumentElement());
    }

    /**
     * Test that namespaces are copied OK by an identity transform
     */

    @Test
    public void testNamespaceSerialization4858() {
        // bug 4858
        try {
            // Stylesheet
            String xslt =
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'"
                            + " xmlns:my='java:jaxptest.DOMTest' xmlns:xhtml='http://www.w3.org/1999/xhtml'>\n" +
                            "  <xsl:template match='/'>\n" +
                            "    <xsl:variable name='body' as='element(xhtml:body)' select='my:makeBodyElement()'/>\n" +
                            "    <xsl:sequence select='$body'/>\n" +
                            "  </xsl:template>\n" +
                            "</xsl:stylesheet>";

            // Run a transformation that invokes an extension function returning this DOM
            TransformerFactory tFactory = new TransformerFactoryImpl();
            Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(xslt)));
            Transformer transformer = templates.newTransformer();
            StringWriter sw = new StringWriter();
            transformer.transform(new StreamSource(new StringReader("<a/>")), new StreamResult(sw));
            err.println(sw.toString());
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }


    @Test
    public void testNamespaceSerializationBug4937 () {
        try {
            String xml = "<a xmlns='someNS' name='test'><b xmlns=''/></a>";
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);

            DocumentBuilder builder = factory.newDocumentBuilder();
            Document doc = builder.parse(new InputSource(new StringReader(xml)));

            Transformer tf = new SaxonTransformerFactory().newTransformer();

            StringWriter writer = new StringWriter(512);

            tf.setOutputProperty(OutputKeys.OMIT_XML_DECLARATION, "yes"); //$NON-NLS-1$ //$NON-NLS-2$
            tf.setOutputProperty(OutputKeys.INDENT, "yes"); //$NON-NLS-1$ //$NON-NLS-2$
            tf.transform(new DOMSource(doc.getDocumentElement()), new StreamResult(writer));

            String transformed = writer.toString();
            System.out.println(writer.toString());

            Document doc2 = builder.parse(new InputSource(new StringReader(transformed)));

        } catch (Exception err) {
            fail();
        }
    }

    @Test
    void testIdentityTransformToDomNamespaces() throws Exception {
        TransformerFactory transformerFactory = new net.sf.saxon.TransformerFactoryImpl();
        DocumentBuilderFactory documentFactory = DocumentBuilderFactory.newInstance();
        documentFactory.setNamespaceAware(true);

        Document document = documentFactory.newDocumentBuilder().newDocument();
        DOMResult result = new DOMResult(document);

        String xml =
                "<X xmlns=\"FOO\" xmlns:b=\"BAR\">"
                        + "<Y xmlns=\"\"/>"
                        + "</X>";

        transformerFactory.newTransformer().transform(new StreamSource(
                new StringReader(xml)), result);

        assertNotNull(document.getDocumentElement().getChildNodes().item(0)
                              .getAttributes().getNamedItem("xmlns"));
    }

    @Test
    void testUndeclareExistingNamespace() throws Exception {
        TransformerFactory transformerFactory = new net.sf.saxon.TransformerFactoryImpl();
        DocumentBuilderFactory documentFactory = DocumentBuilderFactory.newInstance();
        documentFactory.setNamespaceAware(true);

        InputSource stub = new InputSource(new StringReader("<a xmlns='http://ns.one/' xmlns:b='http://ns.two/' xmlns:c='http://ns.three/'/>"));
        Document document = documentFactory.newDocumentBuilder().parse(stub);
        DOMResult result = new DOMResult(document.getDocumentElement());

        String xml = "<X xmlns:b='http://ns.two.b/' xmlns:c='http://ns.three/'/>";

        transformerFactory.newTransformer().transform(new StreamSource(
                new StringReader(xml)), result);

        StringWriter sw = new StringWriter();
        transformerFactory.newTransformer().transform(new DOMSource(document), new StreamResult(sw));

        Node outer = document.getDocumentElement();
        assertEquals("a", outer.getLocalName());
        assertEquals("http://ns.one/", outer.getNamespaceURI());
        assertEquals("http://ns.one/", outer.getAttributes().getNamedItem("xmlns").getNodeValue());
        assertEquals("http://ns.two/", outer.getAttributes().getNamedItemNS(NamespaceConstant.XMLNS, "b").getNodeValue());
        assertEquals("http://ns.three/", outer.getAttributes().getNamedItemNS(NamespaceConstant.XMLNS, "c").getNodeValue());
        Node inner = outer.getFirstChild();
        assertEquals("X", inner.getLocalName());
        assertNull(inner.getNamespaceURI());
        assertEquals("", inner.getAttributes().getNamedItem("xmlns").getNodeValue());
        assertEquals("http://ns.two.b/", inner.getAttributes().getNamedItemNS(NamespaceConstant.XMLNS, "b").getNodeValue());
        assertNull(inner.getAttributes().getNamedItemNS(NamespaceConstant.XMLNS, "c"));
    }

    @Test
    public void testBug5375() {
        try {
            TransformerFactory transformerFactory = new TransformerFactoryImpl();
            DocumentBuilderFactory documentFactory = DocumentBuilderFactory.newInstance();
            documentFactory.setNamespaceAware(true);
            InputSource stub = new InputSource(new StringReader("<a xmlns:abc=\"a\" />"));
            Document document = documentFactory.newDocumentBuilder().parse(stub);
            DOMResult result = new DOMResult(document.getDocumentElement());
            String xml = "<X xmlns:ab=\"b\" xmlns:ac=\"c\"/>";
            transformerFactory.newTransformer().transform(new StreamSource(
                    new StringReader(xml)), result);
            StringWriter sw = new StringWriter();
            transformerFactory.newTransformer().transform(new DOMSource(document), new StreamResult(sw));
        } catch (SAXException | TransformerException | ParserConfigurationException | IOException e) {
            e.printStackTrace();
            fail();
        }
    }


}



