package jaxptest;

import com.saxonica.config.EnterpriseTransformerFactory;
import com.saxonica.config.ProfessionalTransformerFactory;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.dom.DocumentBuilderImpl;
import net.sf.saxon.type.Type;
import net.sf.saxon.xpath.XPathEvaluator;
import net.sf.saxon.xpath.XPathFactoryImpl;
import org.junit.jupiter.api.Test;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;
import org.xml.sax.InputSource;
import org.xml.sax.SAXException;

import javax.xml.transform.Templates;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import javax.xml.xpath.XPath;
import javax.xml.xpath.XPathConstants;
import javax.xml.xpath.XPathExpressionException;
import javax.xml.xpath.XPathFactory;
import java.io.StringReader;
import java.io.StringWriter;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Test the DocumentBuilderImpl class, which builds a read-only DOM view of a TinyTree
 */
public class DocumentBuilderImplTest {

    /**
     * Test that namespaces are copied OK by an identity transform
     */

    @Test
    public void testBasicNavigation() {
        try {
            String in = "<?xml version='1.0'?> \n" +
                    "<document><section id='abc'>123</section><!--comment--><?pi data?></document>";

            // Read an XML file to a DOM
            DocumentBuilderImpl db = new DocumentBuilderImpl();
            InputSource is = new InputSource(new StringReader(in));
            Document doc = db.parse(is);
            Element element = doc.getDocumentElement();
            assertEquals("document", element.getTagName(), "name");
            assertTrue(element.getNamespaceURI() == null, "namespace");
            NodeList children = element.getChildNodes();
            assertEquals(3, children.getLength(), "count");
            Element section = (Element)children.item(0);
            assertEquals("section", section.getTagName(), "name");
            assertEquals("abc", section.getAttribute("id"), "att");
            assertEquals("abc", section.getAttributeNS(null, "id"), "attNS");
            assertEquals(Type.COMMENT, children.item(1).getNodeType(), "isComment");
            assertEquals(Type.PROCESSING_INSTRUCTION, children.item(2).getNodeType(), "isPI");
            assertEquals("comment", children.item(1).getNodeValue(), "commentVal");
            assertEquals("data", children.item(2).getNodeValue(), "piVal");

        } catch (Exception e) {
            fail(e.getMessage());
        }
    }


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
            DocumentBuilderImpl db = new DocumentBuilderImpl();
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
            fail(e.getMessage());
        }
    }

    /**
     * Get the string value of a document node
     */

    @Test
    public void testStringValueOfDocHE() {
        doTestStringValue(new TransformerFactoryImpl());
    }

    @Test
    public void testStringValueOfDocPE() {
        doTestStringValue(new ProfessionalTransformerFactory());
    }

    @Test
    public void testStringValueOfDocEE() {
        doTestStringValue(new EnterpriseTransformerFactory());
    }


    public void doTestStringValue(TransformerFactory tFactory) {
        try {
            String xml = "<!DOCTYPE test [\n" +
                " <!ENTITY omega 'theta'>\n" +
                "]>\n" +
                "<test>[&omega;]</test>";

            // Read an XML file to a DOM
            DocumentBuilderImpl db = new DocumentBuilderImpl();
            db.setConfiguration(((TransformerFactoryImpl)tFactory).getConfiguration());
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
     * Test JAXP XPath API delivering a DOM NodeList given a DOM Node as input
     */

    @Test
    public void testXPathNodeList() {
        try {
            String in = "<?xml version='1.0'?> \n" +
                "<Document><e/><f/><g/></Document>";
            DocumentBuilderImpl db = new DocumentBuilderImpl();
            InputSource is = new InputSource(new StringReader(in));
            Document doc = db.parse(is);
            XPathEvaluator xpe = new XPathEvaluator(db.getConfiguration());
            NodeList isList = (NodeList) xpe.evaluate("/*/*", doc, XPathConstants.NODESET);
            assertEquals(3, isList.getLength(), "length");
        } catch (SAXException | XPathExpressionException e) {
            fail(e.getMessage());
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

            DocumentBuilderImpl builder = new DocumentBuilderImpl();
            Document doc = builder.parse(new InputSource(new StringReader(xml)));

            XPathFactory xPathFactory = //XPathFactory.newInstance();
                new XPathFactoryImpl(builder.getConfiguration());
            if (!"net.sf.saxon.xpath.XPathFactoryImpl".equals(xPathFactory.getClass().getName())) {
                throw new RuntimeException();
            }

            XPath xpath = xPathFactory.newXPath();
            NodeList list = (NodeList) xpath.evaluate(path, doc, XPathConstants.NODESET);

            System.out.println(list.getLength());
            assertEquals(1, list.getLength(), "length");
            for (int i = 0; i < list.getLength(); i++) {
                Node n = list.item(i);
                System.out.println(n);
            }


        } catch (Exception err) {
            err.printStackTrace();
            fail();
        }
    }


}



