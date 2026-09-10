package jaxptest;

import org.junit.jupiter.api.Test;
import org.w3c.dom.Document;

import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerConfigurationException;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.dom.DOMResult;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.io.StringWriter;

import static org.junit.jupiter.api.Assertions.assertEquals;


public class DOMPositionTest {

  private static final String XSL_1 =
    "<xsl:stylesheet version='1.0' " +
    "  xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
    "  <xsl:output method='xml' omit-xml-declaration='yes'/>" +

    "  <xsl:template match='/'>" +
    "    <tab>" +
    "      <xsl:for-each select='//xmlItem'>" +
    "        <row/>" +
    "      </xsl:for-each>" +
    "    </tab>" +
    "  </xsl:template>" +

    "</xsl:stylesheet>";

  private static final String XSL_2 =
    "<xsl:stylesheet version='2.0' " +
    "  xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
    "  <xsl:output method='xml' omit-xml-declaration='yes'/>" +

    "  <xsl:template match='tab'>" +
    "    <table>" +
    "      <xsl:apply-templates/>" +
    "    </table>" +
    "  </xsl:template>" +

    "  <xsl:template match='row'>" +
    "    <tr>" +
    "      <xsl:attribute name=\"p\"><xsl:value-of select=\"position()\"/></xsl:attribute>" +
    "    </tr>" +
    "  </xsl:template>" +

    "</xsl:stylesheet>";

  private Document transformToDOM() throws Exception {
    Transformer transformer = createTransformer(XSL_1);
    StreamSource xmlSource = createSource();

    Document doc = DocumentBuilderFactory.newInstance().newDocumentBuilder().newDocument();
    DOMResult result = new DOMResult(doc);

    transformer.transform(xmlSource, result);
    return doc;
  }

  private String secondTransform(Document dom) throws Exception {
    Transformer transformer = createTransformer(XSL_2);

    StringWriter out = new StringWriter();
    StreamResult result = new StreamResult(out);

    transformer.transform(new DOMSource(dom), result);

    return out.toString();
  }


  private Transformer createTransformer(String xsl) throws TransformerConfigurationException {
    StreamSource xslSource = new StreamSource(new StringReader(xsl));

    System.setProperty("javax.xml.transform.TransformerFactory", "net.sf.saxon.TransformerFactoryImpl");
    TransformerFactory transformerFactory = TransformerFactory.newInstance();

    return transformerFactory.newTransformer(xslSource);
  }

  private StreamSource createSource() {
    return new StreamSource(new StringReader("<root>" +
                                             "  <xmlItem/>" +
                                             "  <xmlItem/>" +
                                             "  <xmlItem/>" +
                                             "</root>"));
  }

    @Test
  public void testTransformation() throws Exception {
    Document doc = transformToDOM();
    String result = secondTransform(doc);

    assertEquals("<table><tr p=\"1\"/><tr p=\"2\"/><tr p=\"3\"/></table>", result);
  }
}
