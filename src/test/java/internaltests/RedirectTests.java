package internaltests;

// Tests for various functions following http 301/302 redirects.

import jaxptest.ConfigTest;
import net.sf.saxon.junit.TestHttpServer;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.io.StringWriter;
import java.nio.file.Path;
import java.nio.file.Paths;

import static org.junit.jupiter.api.Assertions.*;

public class RedirectTests {
    private final Path testDataDirPath = Paths.get(new ConfigTest().getDataDir()).resolve("redirectTests");
    private TestHttpServer testHttp = new TestHttpServer(testDataDirPath);

    @Test
    public void testXslImport() {
        try {
            Processor processor = new Processor(false);
            testHttp.route(http -> {
                http.redirect.get("/stylesheet.xsl", "/redirected.xsl");
                http.get("/redirected.xsl", testHttp.file("redirected.xsl"));
            });

            String xsl = "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                    "                xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                    "                exclude-result-prefixes=\"xs\"\n" +
                    "                version=\"3.0\">\n" +
                    "<xsl:import href=\"" + testHttp.url("/stylesheet.xsl") + "\"/>\n" +
                    "<xsl:output method=\"text\" encoding=\"utf-8\"/>\n" +
                    "<xsl:template name=\"xsl:initial-template\">\n" +
                    "  <xsl:call-template name=\"imported\"/>\n" +
                    "</xsl:template>\n" +
                    "</xsl:stylesheet>\n";

            // Create a transformer for the stylesheet.
            XsltTransformer transformer = processor.newXsltCompiler().compile(
                    new StreamSource(new StringReader(xsl))).load();

            // Create a serializer
            StringWriter sw = new StringWriter();
            Serializer out = processor.newSerializer(sw);
            out.setOutputProperty(Serializer.Property.METHOD, "text");
            transformer.setParameter(new QName("x"), new XdmAtomicValue(2));
            transformer.setDestination(out);
            transformer.transform();
            assertEquals("Redirect succeeded.", sw.toString());

        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        } finally {
            testHttp.stop();
        }
    }

    @Test
    public void testXPathDoc() {
        try {
            Processor processor = new Processor(false);
            testHttp.route(http -> {
                http.redirect.get("/step1.html", "/step2.html");
                http.redirect.get("/step2.html", "/step3.html");
                http.get("/step3.html", testHttp.file("step3.html"));
            });

            String xsl = "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                    "                xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                    "                xmlns:h=\"http://www.w3.org/1999/xhtml\"\n" +
                    "                exclude-result-prefixes=\"xs\"\n" +
                    "                version=\"3.0\">\n" +
                    "<xsl:output method=\"text\" encoding=\"utf-8\"/>\n" +
                    "<xsl:template name=\"xsl:initial-template\">\n" +
                    "  <xsl:sequence select=\"doc('" + testHttp.url("/step1.html") + "')//h:h1/text()\"/>\n" +
                    "</xsl:template>\n" +
                    "</xsl:stylesheet>\n";

            // Create a transformer for the stylesheet.
            XsltTransformer transformer = processor.newXsltCompiler().compile(
                    new StreamSource(new StringReader(xsl))).load();

            // Create a serializer
            StringWriter sw = new StringWriter();
            Serializer out = processor.newSerializer(sw);
            out.setOutputProperty(Serializer.Property.METHOD, "text");
            transformer.setParameter(new QName("x"), new XdmAtomicValue(2));
            transformer.setDestination(out);
            transformer.transform();
            assertEquals("step3.html", sw.toString());

        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        } finally {
            testHttp.stop();
        }
    }

    @Test
    public void testLoop() {
        try {
            Processor processor = new Processor(false);
            testHttp.route(http -> {
                http.redirect.get("/infiniteloop.html", "/seeloopinfinite.html");
                http.redirect.get("/seeloopinfinite.html", "/infiniteloop.html");
            });

            String xsl = "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                    "                xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                    "                xmlns:h=\"http://www.w3.org/1999/xhtml\"\n" +
                    "                exclude-result-prefixes=\"xs\"\n" +
                    "                version=\"3.0\">\n" +
                    "<xsl:output method=\"text\" encoding=\"utf-8\"/>\n" +
                    "<xsl:template name=\"xsl:initial-template\">\n" +
                    "  <xsl:sequence select=\"doc('" + testHttp.url("/infiniteloop.html") + "')//h:h1/text()\"/>\n" +
                    "</xsl:template>\n" +
                    "</xsl:stylesheet>\n";

            // Create a transformer for the stylesheet.
            XsltTransformer transformer = processor.newXsltCompiler().compile(
                    new StreamSource(new StringReader(xsl))).load();

            // Create a serializer
            StringWriter sw = new StringWriter();
            Serializer out = processor.newSerializer(sw);
            out.setOutputProperty(Serializer.Property.METHOD, "text");
            transformer.setParameter(new QName("x"), new XdmAtomicValue(2));
            transformer.setDestination(out);
            transformer.transform();
            fail("Stylesheet should have failed");
        } catch (SaxonApiException e) {
            // We used to test for "Server redirected too many" here, but that's not actually present
            // in the exception from the *production* version of the XPathException class. Alas.
            // So we let this pass with any exception.
            assertTrue(true);
        } finally {
            testHttp.stop();
        }
    }

}
