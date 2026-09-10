package jaxptest;

import net.sf.saxon.TransformerFactoryImpl;
import org.junit.jupiter.api.Test;

import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerException;
import javax.xml.transform.sax.SAXResult;
import javax.xml.transform.sax.SAXTransformerFactory;
import javax.xml.transform.sax.TransformerHandler;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.io.StringWriter;

import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 *
 */
public class TransformerHandlerTest {

//    public void testStreamingViaJaxp() {
//
//        try {
//            String xsl =
//                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
//                            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
//                            "    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
//                            "    exclude-result-prefixes=\"xs\"\n" +
//                            "    version=\"3.0\">\n"
//                            + "<xsl:mode streamable=\"true\"/>\n" +
//                            "    <xsl:template match=\"*:person\"><xsl:value-of select=\"@id\"/></xsl:template>\n" +
//                            "</xsl:stylesheet>";
//            String xml = "<person id='test'/>";
//
//            TransformerFactory factory = new StreamingTransformerFactory();
//            Transformer transformer = factory.newTransformer(new StreamSource(new StringReader(xsl)));
//            transformer.setErrorReporter(new StandardErrorListener() {
//                @Override
//                public void warning(TransformerException exception) {
//                    System.err.println(exception.getMessage());
//                    fail();
//                }
//            });
//
//            InputSource inputSource = new InputSource(new StringReader(xml));
//            SAXSource xmlSource = new SAXSource(inputSource);
//            StringWriter writer = new StringWriter();
//            transformer.transform(xmlSource, new StreamResult(writer));
//
//            System.out.println(writer.toString());
//        } catch (TransformerException e) {
//            e.printStackTrace();
//            fail();
//        }
//    }
//
//    public void testStreamingViaJaxpNoGlobalContext() {
//
//        try {
//            String xsl =
//                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
//                            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
//                            "    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
//                            "    exclude-result-prefixes=\"xs\"\n" +
//                            "    version=\"3.0\">\n" +
//                            " <xsl:variable name='g' select='.'/>\n" +
//                            " <xsl:mode streamable=\"true\"/>\n" +
//                            "    <xsl:template match=\"*:person\"><xsl:value-of select=\"@id||$g\"/></xsl:template>\n" +
//                            "</xsl:stylesheet>";
//            String xml = "<person id='test'/>";
//
//            TransformerFactory factory = new StreamingTransformerFactory();
//            Transformer transformer = factory.newTransformer(new StreamSource(new StringReader(xsl)));
//            transformer.setErrorReporter(new StandardErrorListener() {
//                @Override
//                public void warning(TransformerException exception) {
//                    System.err.println(exception.getMessage());
//                    fail();
//                }
//            });
//
//            InputSource inputSource = new InputSource(new StringReader(xml));
//            SAXSource xmlSource = new SAXSource(inputSource);
//            StringWriter writer = new StringWriter();
//            transformer.transform(xmlSource, new StreamResult(writer));
//            fail("Unexpected success");
//        } catch (XPathException e) {
//            assert (e.getErrorCodeLocalPart().equals("XPDY0002"));
//        } catch (TransformerException e) {
//            e.printStackTrace();
//            fail();
//        }
//    }
//
//    public void testStripSpace() {
//
//        String xml = "<doc>"
//                + "  <section id='1'> <a>1</a> <b>2</b> </section> "
//                + "  <section id='2' xml:space='preserve'> <a>1</a> <b>2</b> </section> "
//                + " </doc>";
//
//        String xsl = "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>"
//                + "  <xsl:template match='/'><xsl:copy-of select='.'/></xsl:template>"
//                + "  <xsl:strip-space elements='section doc'/>"
//                + " </xsl:stylesheet>";
//        try {
//
//            TransformerFactory tfactory = new TransformerFactoryImpl();
//            StreamSource xslSource = new StreamSource(new StringReader(xsl));
//            Transformer transformer = tfactory.newTransformer(xslSource);
//            StreamSource xmlSource = new StreamSource(new StringReader(xml));
//            StringWriter sw = new StringWriter();
//            transformer.transform(xmlSource, new StreamResult(sw));
//            assertEquals("<?xml version=\"1.0\" encoding=\"UTF-8\"?><doc><section id=\"1\"><a>1</a><b>2</b></section><section id=\"2\" xml:space=\"preserve\"> <a>1</a> <b>2</b> </section></doc>", sw.toString());
//        } catch (Exception ex) {
//            fail();
//        }
//
//
//    }

    @Test
    public void testTransformerHandlerWithUnparsedEntities() {
        // See http://stackoverflow.com/questions/43387074/accessing-unparsed-entities-in-xslt-with-a-saxtransformerfactory-and-transformer
        // See Saxon bug 3201
        try {
            String xsl =
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"
                            + "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"2.0\">\n"
                            + "\n"
                            + "    <xsl:template match=\"cover\">\n"
                            + "      <xsl:copy>\n"
                            + "        <xsl:value-of select=\"unparsed-entity-uri(@imgref)\"/>\n"
                            + "      </xsl:copy>\n"
                            + "    </xsl:template>\n"
                            + "\n"
                            + "    <xsl:template match=\"@*|node()\">\n"
                            + "        <xsl:copy>\n"
                            + "            <xsl:apply-templates select=\"@*|node()\"/>\n"
                            + "        </xsl:copy>\n"
                            + "    </xsl:template>\n"
                            + "\n"
                            + "</xsl:stylesheet>";

            String xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"
                    + "<!DOCTYPE book\n"
                    + "[<!ENTITY cover_hadrien SYSTEM \"images/covers/cover_hadrien.jpg\" NDATA jpeg>]>\n"
                    + "<book>\n"
                    + "  <title>Les mémoires d'Hadrien</title>\n"
                    + "  <author>Marguerite Yourcenar</author>\n"
                    + "  <cover imgref=\"cover_hadrien\" />\n"
                    + "</book>";

            SAXTransformerFactory factory = new TransformerFactoryImpl();
            TransformerHandler th = factory.newTransformerHandler(new StreamSource(new StringReader(xsl)));

            StringWriter writer = new StringWriter();
            th.setResult(new StreamResult(writer));
            Transformer t = factory.newTransformer();

            t.transform(new StreamSource(new StringReader(xml)),
                        new SAXResult(th));

            assertTrue(writer.toString().contains("images/covers/cover_hadrien.jpg</cover>"));
            //System.out.println(writer.toString());
        } catch (TransformerException e) {
            e.printStackTrace();
            fail();
        }
    }

}


// Contributor(s): none.
//



