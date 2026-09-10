package s9apitest;

import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.io.StringWriter;
import java.util.HashMap;
import java.util.Map;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;


/**
 * JUnit tests on s9api operations on UseWhenFilter class where we are using
 * the static=yes attribute on xsl:param and xsl:variable
 */
public class TestStaticAttr {

    /**
     * Test external supply of param value
     */

    @Test
    public void testStaticAtt1() {
        try {
            Processor proc = new Processor(true);

            XsltCompiler compiler = proc.newXsltCompiler();
            String stylesheet = "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                    "    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                    "    exclude-result-prefixes=\"xs\"\n" +
                    "    version=\"3.0\">\n" +
                    "    <xsl:variable name=\"xx\" select=\"'2'\" static=\"yes\"/>\n" +
                    "    <xsl:param name=\"abc\" static=\"yes\" required=\"yes\" as=\"xs:integer\"/>\n" +
                    "    <xsl:template match=\"/\" use-when=\"$abc=2\"><out><xsl:value-of select=\"$xx\" /></out></xsl:template>\n" +
                    "</xsl:stylesheet>";
            Source ss = new StreamSource(new StringReader(stylesheet));
            compiler.setParameter(new QName("abc"), new XdmAtomicValue(2));
            XsltTransformer trans = compiler.compile(ss).load();
            ss = new StreamSource(new StringReader(stylesheet));
            trans.setSource(ss);
            StringWriter sw = new StringWriter();
            Serializer serializer = proc.newSerializer(sw);
            trans.setDestination(serializer);
            trans.transform();
            assertEquals("<?xml version=\"1.0\" encoding=\"UTF-8\"?><out>2</out>", sw.toString());
        } catch (SaxonApiException e) {
            fail(e.toString());
        }
    }


    /**
     * Test external supply of param value. The required type supplied in this test case is incorrect.
     * Therefore an XPathException is expected
     */

    @Test
    public void testStaticAtt2() {
        try {
            Processor proc = new Processor(true);

            XsltCompiler compiler = proc.newXsltCompiler();
            String stylesheet = "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                    "    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                    "    exclude-result-prefixes=\"xs\"\n" +
                    "    version=\"3.0\">\n" +
                    "    <xsl:variable name=\"xx\" select=\"'2'\" static=\"yes\"/>\n" +
                    "    <xsl:param name=\"abc\" static=\"yes\" required=\"yes\" as=\"xs:string\"/>\n" +
                    "    <xsl:template match=\"/\" use-when=\"$abc12=2\"><out><xsl:value-of select=\"$xx\" /></out></xsl:template>\n" +
                    "</xsl:stylesheet>";
            Source ss = new StreamSource(new StringReader(stylesheet));
            compiler.setParameter(new QName("abc"), new XdmAtomicValue(2));
            XsltTransformer trans = compiler.compile(ss).load();
            ss = new StreamSource(new StringReader(stylesheet));
            trans.setSource(ss);
            Serializer serializer = proc.newSerializer(System.out);
            trans.setDestination(serializer);
            trans.transform();
            fail("Unexpected success");
        } catch (SaxonApiException e) {
            assertTrue(
                    e.getMessage().contains("Failed to compile stylesheet") ||
                    e.getMessage().contains("The required item type of the value of variable $abc is xs:string") ||
                    e.getMessage().contains("Failed to convert the supplied value for variable $abc: Cannot convert 2 to xs:string"));
        }
    }

    /**
     * Test external supply of param value. For the xsl:param the value has not be supplied externally.
     * Therefore an XPathException is expected
     */

    @Test
    public void testStaticAtt3() {
        try {
            Processor proc = new Processor(true);

            XsltCompiler compiler = proc.newXsltCompiler();
            String stylesheet = "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                    "    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                    "    exclude-result-prefixes=\"xs\"\n" +
                    "    version=\"3.0\">\n" +
                    "    <xsl:variable name=\"xx\" select=\"2\" static=\"yes\"/>\n" +
                    "    <xsl:param name=\"abc\" static=\"yes\" required=\"yes\" as=\"xs:string\"/>\n" +
                    "    <xsl:template match=\"/\" use-when=\"$xx=2\"><out><xsl:value-of select=\"$xx\" /></out></xsl:template>\n" +
                    "</xsl:stylesheet>";
            Source ss = new StreamSource(new StringReader(stylesheet));
            XsltTransformer trans = compiler.compile(ss).load();
            ss = new StreamSource(new StringReader(stylesheet));
            trans.setSource(ss);
            Serializer serializer = proc.newSerializer(System.out);
            trans.setDestination(serializer);
            trans.transform();
            fail("Unexpected success");
        } catch (SaxonApiException e) {
            assertTrue(e.getMessage().contains("No value was supplied for the required static parameter $abc"));
        }
    }

    /**
     * Test external supply of param value. The xsl:variable is defined as not static.
     * Therefore references to it should not be allowed in a use-when attribute. XPathException is expected
     */

    @Test
    public void testStaticAtt4() {
        try {
            Processor proc = new Processor(true);

            XsltCompiler compiler = proc.newXsltCompiler();
            String stylesheet = "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                    "    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                    "    exclude-result-prefixes=\"xs\"\n" +
                    "    version=\"3.0\">\n" +
                    "    <xsl:variable name=\"xx\" select=\"'2'\" static=\"no\"/>\n" +
                    "    <xsl:param name=\"abc\" static=\"yes\" required=\"yes\" as=\"xs:integer\"/>\n" +
                    "    <xsl:template match=\"/\" use-when=\"$xx=2\"><out><xsl:value-of select=\"$xx\" /></out></xsl:template>\n" +
                    "</xsl:stylesheet>";
            Source ss = new StreamSource(new StringReader(stylesheet));
            compiler.setParameter(new QName("abc"), new XdmAtomicValue(2));
            XsltTransformer trans = compiler.compile(ss).load();
            ss = new StreamSource(new StringReader(stylesheet));
            trans.setSource(ss);
            Serializer serializer = proc.newSerializer(System.out);
            trans.setDestination(serializer);
            trans.transform();
            fail("Unexpected success");
        } catch (SaxonApiException e) {
            assertEquals("XPST0008", e.getErrorCode().getLocalName(), "error-code");
        }
    }


    /**
     * Test use of static variable in use-when (no params). Success expected
     */

    @Test
    public void testStaticAtt5() {
        try {
            Processor proc = new Processor(true);

            XsltCompiler compiler = proc.newXsltCompiler();
            String stylesheet = "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                    "    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                    "    exclude-result-prefixes=\"xs\"\n" +
                    "    version=\"3.0\">\n" +
                    "    <xsl:variable name=\"xx\" select=\"'2'\" static=\"yes\"/>\n" +
                    "    <xsl:template match=\"/\" use-when=\"$xx='2'\"><out><xsl:value-of select=\"$xx\" /></out></xsl:template>\n" +
                    "</xsl:stylesheet>";
            Source ss = new StreamSource(new StringReader(stylesheet));
            XsltTransformer trans = compiler.compile(ss).load();
            ss = new StreamSource(new StringReader(stylesheet));
            trans.setSource(ss);
            StringWriter sw = new StringWriter();
            Serializer serializer = proc.newSerializer(sw);
            trans.setDestination(serializer);
            trans.transform();
            assertTrue(sw.toString().contains("<out>2</out>"));
        } catch (SaxonApiException e) {
            fail(e.toString());
        }
    }

    /**
     * Supplying a dynamic value for a static parameter should fail
     */

    @Test
    public void testStaticAtt6() {
        try {
            Processor proc = new Processor(false);

            XsltCompiler compiler = proc.newXsltCompiler();
            String stylesheet = "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                    "    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                    "    exclude-result-prefixes=\"xs\"\n" +
                    "    version=\"3.0\">\n" +
                    "    <xsl:param name=\"xx\" select=\"'2'\" static=\"yes\"/>\n" +
                    "    <xsl:param name=\"abc\" static=\"yes\" required=\"yes\" as=\"xs:integer\"/>\n" +
                    "    <xsl:template match=\"/\" use-when=\"$abc=2\"><out><xsl:value-of select=\"$xx\" /></out></xsl:template>\n" +
                    "</xsl:stylesheet>";
            Source ss = new StreamSource(new StringReader(stylesheet));
            compiler.setParameter(new QName("abc"), new XdmAtomicValue(2));
            XsltTransformer trans = compiler.compile(ss).load();
            trans.setParameter(new QName("xx"), new XdmAtomicValue(3));
            ss = new StreamSource(new StringReader(stylesheet));
            trans.setSource(ss);
            Serializer serializer = proc.newSerializer(System.out);
            trans.setDestination(serializer);
            trans.transform();
            fail("Unexpected success");
        } catch (SaxonApiException e) {
            assertTrue(e.getMessage().contains("$xx"));
        }
    }

    /**
     * Supplying a dynamic value for a parameter already supplied dynamically should fail
     */

    @Test
    public void testStaticAtt6a() {
        try {
            Processor proc = new Processor(false);

            XsltCompiler compiler = proc.newXsltCompiler();
            String stylesheet = "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                    "    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                    "    exclude-result-prefixes=\"xs\"\n" +
                    "    version=\"3.0\">\n" +
                    "    <xsl:param name=\"xx\" select=\"'2'\" static=\"no\"/>\n" +
                    "    <xsl:param name=\"abc\" static=\"yes\" required=\"yes\" as=\"xs:integer\"/>\n" +
                    "    <xsl:template match=\"/\" use-when=\"$abc=2\"><out><xsl:value-of select=\"$xx\" /></out></xsl:template>\n" +
                    "</xsl:stylesheet>";
            Source ss = new StreamSource(new StringReader(stylesheet));
            compiler.setParameter(new QName("abc"), new XdmAtomicValue(2));
            compiler.setParameter(new QName("xx"), new XdmAtomicValue(29));
            XsltTransformer trans = compiler.compile(ss).load();
            trans.setParameter(new QName("xx"), new XdmAtomicValue(34));
            ss = new StreamSource(new StringReader(stylesheet));
            trans.setSource(ss);
            Serializer serializer = proc.newSerializer(System.out);
            trans.setDestination(serializer);
            trans.transform();
            fail("Unexpected success");
        } catch (SaxonApiException e) {
            assertTrue(e.getMessage().contains("$xx"));
        }
    }

    /**
     * Supplying a dynamic value for a static parameter should fail
     */

    @Test
    public void testStaticAtt7() {
        try {
            Processor proc = new Processor(false);

            XsltCompiler compiler = proc.newXsltCompiler();
            String stylesheet = "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                    "    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                    "    exclude-result-prefixes=\"xs\"\n" +
                    "    version=\"3.0\">\n" +
                    "    <xsl:param name=\"xx\" select=\"'2'\" static=\"yes\"/>\n" +
                    "    <xsl:param name=\"abc\" static=\"yes\" required=\"yes\" as=\"xs:integer\"/>\n" +
                    "    <xsl:template match=\"/\" use-when=\"$abc=2\"><out><xsl:value-of select=\"$xx\" /></out></xsl:template>\n" +
                    "</xsl:stylesheet>";
            Source ss = new StreamSource(new StringReader(stylesheet));
            compiler.setParameter(new QName("abc"), new XdmAtomicValue(2));
            Xslt30Transformer trans = compiler.compile(ss).load30();
            Map<QName, XdmValue> params = new HashMap<>();
            params.put(new QName("xx"), new XdmAtomicValue(3));
            trans.setStylesheetParameters(params);
            ss = new StreamSource(new StringReader(stylesheet));
            Serializer serializer = proc.newSerializer(System.out);
            trans.transform(ss, serializer);
            fail("Unexpected success");
        } catch (SaxonApiException e) {
            assertTrue(e.getMessage().contains("$xx"));
        }
    }


}

