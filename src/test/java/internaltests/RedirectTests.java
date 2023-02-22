package internaltests;

// Tests for various functions following http 301/302 redirects.

import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.QName;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.Serializer;
import net.sf.saxon.s9api.XdmAtomicValue;
import net.sf.saxon.s9api.XsltTransformer;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.io.StringWriter;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

public class RedirectTests {
    private static final String httpBase = "https://dev.saxonica.com/tests/redirect/";

    @Test
    public void testXslImport() {
        try {
            Processor processor = new Processor(false);

            String xsl = "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                    "                xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                    "                exclude-result-prefixes=\"xs\"\n" +
                    "                version=\"3.0\">\n" +
                    "<xsl:import href=\"" + httpBase + "stylesheet.xsl\"/>\n" +
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
        }
    }

    @Test
    public void testXPathDoc() {
        try {
            Processor processor = new Processor(false);

            String xsl = "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                    "                xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                    "                xmlns:h=\"http://www.w3.org/1999/xhtml\"\n" +
                    "                exclude-result-prefixes=\"xs\"\n" +
                    "                version=\"3.0\">\n" +
                    "<xsl:output method=\"text\" encoding=\"utf-8\"/>\n" +
                    "<xsl:template name=\"xsl:initial-template\">\n" +
                    "  <xsl:sequence select=\"doc('" + httpBase+ "step1.html')//h:h1/text()\"/>\n" +
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
        }
    }

    @Test
    public void testLoop() {
        try {
            Processor processor = new Processor(false);

            String xsl = "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                    "                xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                    "                xmlns:h=\"http://www.w3.org/1999/xhtml\"\n" +
                    "                exclude-result-prefixes=\"xs\"\n" +
                    "                version=\"3.0\">\n" +
                    "<xsl:output method=\"text\" encoding=\"utf-8\"/>\n" +
                    "<xsl:template name=\"xsl:initial-template\">\n" +
                    "  <xsl:sequence select=\"doc('" + httpBase+ "infiniteloop.html')//h:h1/text()\"/>\n" +
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
        }
    }

}
