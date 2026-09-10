package jaxptest;

import com.saxonica.config.EnterpriseTransformerFactory;
import com.saxonica.config.StreamingTransformerFactory;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.lib.StandardErrorListener;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.CompilerInfo;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.StringValue;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;
import org.xml.sax.SAXException;
import org.xml.sax.ext.Attributes2Impl;

import javax.xml.transform.Templates;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerException;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.sax.SAXTransformerFactory;
import javax.xml.transform.sax.TransformerHandler;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.*;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 *
 */
public class TransformTest  {
    private final ConfigTest configTest = new ConfigTest();

    @Test
    public void testStreamingViaJaxp() {

        try {
            String xsl =
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                            "    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                            "    exclude-result-prefixes=\"xs\"\n" +
                            "    version=\"3.0\">\n"
                            + "<xsl:mode streamable=\"true\"/>\n" +
                            "    <xsl:template match=\"*:person\"><xsl:value-of select=\"@id\"/></xsl:template>\n" +
                            "</xsl:stylesheet>";
            String xml = "<person id='test'/>";

            TransformerFactory factory = new StreamingTransformerFactory();
            Transformer transformer = factory.newTransformer(new StreamSource(new StringReader(xsl)));
            transformer.setErrorListener(new StandardErrorListener() {
                @Override
                public void warning(TransformerException exception) {
                    System.err.println(exception.getMessage());
                    fail();
                }
            });

            InputSource inputSource = new InputSource(new StringReader(xml));
            SAXSource xmlSource = new SAXSource(inputSource);
            StringWriter writer = new StringWriter();
            transformer.transform(xmlSource, new StreamResult(writer));

            System.out.println(writer);
        } catch (TransformerException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testStreamingViaDefaultFactory() {
        // This runs successfully, but doesn't use streaming.
        try {
            String xsl =
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                            "    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                            "    exclude-result-prefixes=\"xs\"\n" +
                            "    version=\"3.0\">\n"
                            + "<xsl:mode streamable=\"true\"/>\n" +
                            "    <xsl:template match=\"*:person\"><xsl:value-of select=\"@id\"/></xsl:template>\n" +
                            "</xsl:stylesheet>";
            String xml = "<person id='test'/>";

            TransformerFactory factory = new EnterpriseTransformerFactory();
            Transformer transformer = factory.newTransformer(new StreamSource(new StringReader(xsl)));
            transformer.setErrorListener(new StandardErrorListener() {
                @Override
                public void warning(TransformerException exception) {
                    System.err.println(exception.getMessage());
                    fail();
                }
            });

            InputSource inputSource = new InputSource(new StringReader(xml));
            SAXSource xmlSource = new SAXSource(inputSource);
            StringWriter writer = new StringWriter();
            transformer.transform(xmlSource, new StreamResult(writer));

            System.out.println(writer);
        } catch (TransformerException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testStreamingTransformerFactory() {
        try {
            String xsl = "<xsl:transform xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>"
                    + "<xsl:mode streamable='yes'/>\n"
                    + "<xsl:param name='input-uri'/>\n"
                    + "<xsl:template match=\"/\">\n"
                    + "  <html>\n"
                    + "  <body>\n"
                    + "    <table border=\"1\">\n"
                    + "      <xsl:source-document streamable=\"yes\" href=\"{$input-uri}\">\n"
                    + "      <xsl:for-each select=\"osm/node\">\n"
                    + "      <tr>\n"
                    + "        <td><xsl:value-of select=\"@lat\" /></td>\n"
                    + "        <td><xsl:value-of select=\"@lon\" /></td>\n"
                    + "      </tr>\n"
                    + "      </xsl:for-each>\n"
                    + "      </xsl:source-document>\n"
                    + "    </table>\n"
                    + "  </body>\n"
                    + "  </html>\n"
                    + "</xsl:template>"
                    + "</xsl:transform>";
            StreamSource src = new StreamSource(new StringReader(xsl));
            StreamingTransformerFactory strf = new StreamingTransformerFactory();
            strf.setAttribute(FeatureKeys.TIMING, true);
            Transformer _transformer = strf.newTransformer(src);
            _transformer.setParameter("input-uri", new File(configTest.getDataDir() + "books.xml").toURI().toString());
            InputStream is = new FileInputStream(new File(configTest.getDataDir() + "books.xml"));
            Reader reader = new InputStreamReader(is, "ISO-8859-1");
            OutputStream out = System.out;
            _transformer.transform(new StreamSource(reader), new StreamResult(System.out));
        } catch (FileNotFoundException | TransformerException | UnsupportedEncodingException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testStreamingTransformerHandler() {
        try {
            String xsl =
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                            "    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                            "    exclude-result-prefixes=\"xs\"\n" +
                            "    version=\"3.0\">\n"
                            + "<xsl:mode streamable=\"true\"/>\n" +
                            "    <xsl:template match=\"doc\"><s><xsl:apply-templates/></s></xsl:template>\n" +
                            "    <xsl:template match=\"row\"><t id=\"{.}\"/></xsl:template>\n" +
                            "</xsl:stylesheet>";
            String xml = "<person id='test'/>";

            TransformerFactory factory = new StreamingTransformerFactory();
            final TransformerHandler handler = ((SAXTransformerFactory)factory).newTransformerHandler(
                    new StreamSource(new StringReader(xsl)));

            StringWriter sw = new StringWriter();
            handler.setResult(new StreamResult(sw));
            try {
                handler.startDocument();
                handler.startElement("", "doc", "doc", new Attributes2Impl());
                for (int i = 0; i < 1000; i++) {
                    handler.startElement("", "row", "row", new Attributes2Impl());
                    char[] chars = ("text" + i).toCharArray();
                    handler.characters(chars, 0, chars.length);
                    handler.endElement("", "row", "row");
                }
                handler.endElement("", "doc", "doc");
                handler.endDocument();
            } catch (SAXException e) {
                e.printStackTrace();
                fail();
            }
            System.out.println(sw);
        } catch (TransformerException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testStreamingViaJaxpNoGlobalContext() {

        try {
            String xsl =
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                            "    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                            "    exclude-result-prefixes=\"xs\"\n" +
                            "    version=\"3.0\">\n" +
                            " <xsl:variable name='g' select='.'/>\n" +
                            " <xsl:mode streamable=\"true\"/>\n" +
                            "    <xsl:template match=\"*:person\"><xsl:value-of select=\"@id||$g\"/></xsl:template>\n" +
                            "</xsl:stylesheet>";
            String xml = "<person id='test'/>";

            TransformerFactory factory = new StreamingTransformerFactory();
            Transformer transformer = factory.newTransformer(new StreamSource(new StringReader(xsl)));
            transformer.setErrorListener(new StandardErrorListener() {
                @Override
                public void warning(TransformerException exception) {
                    System.err.println(exception.getMessage());
                    fail();
                }
            });

            InputSource inputSource = new InputSource(new StringReader(xml));
            SAXSource xmlSource = new SAXSource(inputSource);
            StringWriter writer = new StringWriter();
            transformer.transform(xmlSource, new StreamResult(writer));
            fail("Unexpected success");
        } catch (XPathException e) {
            assertEquals("XPDY0002", e.getErrorCodeLocalPart());
        } catch (TransformerException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testStripSpace() {

        String xml = "<doc>"
                + "  <section id='1'> <a>1</a> <b>2</b> </section> "
                + "  <section id='2' xml:space='preserve'> <a>1</a> <b>2</b> </section> "
                + " </doc>";

        String xsl = "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>"
                + "  <xsl:template match='/'><xsl:copy-of select='.'/></xsl:template>"
                + "  <xsl:strip-space elements='section doc'/>"
                + " </xsl:stylesheet>";
        try {

            TransformerFactory tfactory = new TransformerFactoryImpl();
            StreamSource xslSource = new StreamSource(new StringReader(xsl));
            Transformer transformer = tfactory.newTransformer(xslSource);
            StreamSource xmlSource = new StreamSource(new StringReader(xml));
            StringWriter sw = new StringWriter();
            transformer.transform(xmlSource, new StreamResult(sw));
            assertEquals("<?xml version=\"1.0\" encoding=\"UTF-8\"?><doc><section id=\"1\"><a>1</a><b>2</b></section><section id=\"2\" xml:space=\"preserve\"> <a>1</a> <b>2</b> </section></doc>", sw.toString());
        } catch (Exception ex) {
            fail();
        }


    }

    @Test
    public void testJsonSerialization() {
        // See Saxon bug 3225
        try {
            String xsl =
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                            "    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                            "    exclude-result-prefixes=\"xs\"\n" +
                            "    version=\"3.0\">\n" +
                            "    <xsl:output method='json'/>\n" +
                            "    <xsl:template match=\"*:person\"><xsl:sequence select=\"map{'x':.}\"/></xsl:template>\n" +
                            "</xsl:stylesheet>";
            String xml = "<person id='test'/>";

            TransformerFactory factory = new TransformerFactoryImpl();
            Transformer transformer = factory.newTransformer(new StreamSource(new StringReader(xsl)));
            transformer.setErrorListener(new StandardErrorListener() {
                @Override
                public void warning(TransformerException exception) {
                    System.err.println(exception.getMessage());
                    fail();
                }
            });

            InputSource inputSource = new InputSource(new StringReader(xml));
            SAXSource xmlSource = new SAXSource(inputSource);
            StringWriter writer = new StringWriter();
            transformer.transform(xmlSource, new StreamResult(writer));
            assertTrue(writer.toString().startsWith("{\"x\":\"<person id"));
            //System.out.println(writer.toString());
        } catch (TransformerException e) {
            e.printStackTrace();
            fail();
        }
    }


    public void testJsonSerialization2() {
        // See Saxon bug 3225
        try {
            String xsl =
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                            "    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                            "    exclude-result-prefixes=\"xs\"\n" +
                            "    version=\"3.0\">\n" +
                            "    <xsl:template match=\"*:person\"><xsl:sequence select=\"map{'x':.}\"/></xsl:template>\n" +
                            "</xsl:stylesheet>";
            String xml = "<person id='test'/>";

            TransformerFactory factory = new TransformerFactoryImpl();
            Transformer transformer = factory.newTransformer(new StreamSource(new StringReader(xsl)));
            transformer.setOutputProperty("method", "json");
            transformer.setErrorListener(new StandardErrorListener() {
                @Override
                public void warning(TransformerException exception) {
                    System.err.println(exception.getMessage());
                    fail();
                }
            });

            InputSource inputSource = new InputSource(new StringReader(xml));
            SAXSource xmlSource = new SAXSource(inputSource);
            StringWriter writer = new StringWriter();
            transformer.transform(xmlSource, new StreamResult(writer));
            //System.out.println(writer.toString());
            assertTrue(writer.toString().startsWith("{\"x\":\"<person id"));
        } catch (TransformerException e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testAdaptiveSerialization() {
        // See Saxon bug 3225
        try {
            String xsl =
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                            "    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                            "    exclude-result-prefixes=\"xs\"\n" +
                            "    version=\"3.0\">\n" +
                            "    <xsl:output method='adaptive'/>\n" +
                            "    <xsl:template match=\"*:person\"><xsl:sequence select=\"[1,2,3]\"/></xsl:template>\n" +
                            "</xsl:stylesheet>";
            String xml = "<person id='test'/>";

            TransformerFactory factory = new TransformerFactoryImpl();
            Transformer transformer = factory.newTransformer(new StreamSource(new StringReader(xsl)));
            transformer.setErrorListener(new StandardErrorListener() {
                @Override
                public void warning(TransformerException exception) {
                    System.err.println(exception.getMessage());
                    fail();
                }
            });

            InputSource inputSource = new InputSource(new StringReader(xml));
            SAXSource xmlSource = new SAXSource(inputSource);
            StringWriter writer = new StringWriter();
            transformer.transform(xmlSource, new StreamResult(writer));
            assertEquals("[1,2,3]", writer.toString());
            //System.out.println(writer.toString());
        } catch (TransformerException e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testStaticParameters() {
        // See Saxon bug 3225
        try {
            String xsl =
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                            "    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                            "    exclude-result-prefixes=\"xs\"\n" +
                            "    version=\"3.0\">\n" +
                            "    <xsl:param name='x' static='true' required='true'/>" +
                            "    <xsl:template match='/'><xsl:sequence _select='{$x}'/></xsl:template>\n" +
                            "</xsl:stylesheet>";
            String xml = "<person id='test'/>";

            TransformerFactoryImpl factory = new TransformerFactoryImpl();
            StreamSource xslS = new StreamSource(new StringReader(xsl));
            CompilerInfo info = new CompilerInfo(factory.getConfiguration());
            info.setParameter(new StructuredQName("", "", "x"), StringValue.bmp("2+2"));
            Templates templates = factory.newTemplates(xslS, info);
            Transformer transformer = templates.newTransformer();
            transformer.setOutputProperty("method", "text");

            InputSource inputSource = new InputSource(new StringReader(xml));
            SAXSource xmlSource = new SAXSource(inputSource);
            StringWriter writer = new StringWriter();
            transformer.transform(xmlSource, new StreamResult(writer));

            System.out.println(writer);
            assertEquals("4", writer.toString());
        } catch (TransformerException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testJitErrorHandling() {
        // See Saxon bug 3702. There's a static error in the template, which gets found
        // during execution (because of JIT): so what happens during a second execution
        // of the stylesheet?
        try {
            String xsl =
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                            "    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                            "    exclude-result-prefixes=\"xs\"\n" +
                            "    version=\"3.0\">\n" +
                            "    <xsl:template match='/'><xsl:apply-templates/></xsl:template>\n" +
                            "    <xsl:template match='doc'><xsl:apply-templates/></xsl:template>\n" +
                            "    <xsl:template match='para'><xsl:call-template name='f:nonexistent'/></xsl:template>\n" +
                            "</xsl:stylesheet>";
            String xml = "<doc><para/><para/></doc>";

            TransformerFactoryImpl factory = new EnterpriseTransformerFactory();
            StreamSource xslS = new StreamSource(new StringReader(xsl));
            Templates templates = factory.newTemplates(xslS);
            Transformer transformer = templates.newTransformer();
            transformer.setOutputProperty("method", "text");

            StringWriter writer = null;
            for (int i=0; i<2; i++) {
                writer = null;
                try {
                    InputSource inputSource = new InputSource(new StringReader(xml));
                    SAXSource xmlSource = new SAXSource(inputSource);
                    writer = new StringWriter();
                    transformer.transform(xmlSource, new StreamResult(writer));
                    fail("unexpected success");
                } catch (TransformerException e) {
                    // Yes, we expected that to fail
                }
            }

        } catch (TransformerException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testTransformerReuse() {
        // See Saxon bug 4019. Check that global variables are reset when transformer is reused
        try {
            String xsl =
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                            "    version=\"3.0\">\n" +
                            "    <xsl:variable name='g' select='/'/>" +
                            "    <xsl:template match='/'><out val='{$g}'/></xsl:template>\n" +
                            "</xsl:stylesheet>";
            String xmlA = "<doc>A</doc>";
            String xmlB = "<doc>B</doc>";

            TransformerFactoryImpl factory = new EnterpriseTransformerFactory();
            StreamSource xslS = new StreamSource(new StringReader(xsl));
            Templates templates = factory.newTemplates(xslS);
            Transformer transformer = templates.newTransformer();
            transformer.setOutputProperty("omit-xml-declaration", "yes");
            String[] inputs = new String[]{xmlA, xmlB};
            StringWriter writer = null;
            for (int i = 0; i < 2; i++) {
                writer = null;
                try {
                    InputSource inputSource = new InputSource(new StringReader(inputs[i]));
                    SAXSource xmlSource = new SAXSource(inputSource);
                    writer = new StringWriter();
                    transformer.transform(xmlSource, new StreamResult(writer));
                    if (i==0) {
                        assertEquals("<out val=\"A\"/>", writer.toString(), "test" + i);
                    } else {
                        assertEquals("<out val=\"B\"/>", writer.toString(), "test" + i);
                    }
                } catch (TransformerException e) {
                    fail();
                }
            }

        } catch (TransformerException e) {
            e.printStackTrace();
            fail();
        }
    }


}


// Contributor(s): none.
//



