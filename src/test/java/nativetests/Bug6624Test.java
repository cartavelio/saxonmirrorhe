package nativetests;

import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.ByteArrayOutputStream;
import java.io.StringReader;
import java.io.UnsupportedEncodingException;

import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Regression test for bug 6626
 */
public class Bug6624Test {

    @Test
    public void testBug6624() {
        String xml = "<root>\n"
                + "    <foo>foo 1</foo>\n"
                + "    <bar>bar 1</bar>\n"
                + "</root>";

        String xsl = "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n"
                + "                xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n"
                + "                xmlns:saxon=\"http://saxon.sf.net/\"\n"
                + "                exclude-result-prefixes=\"#all\"\n"
                + "                version=\"3.0\"\n"
                + "                expand-text=\"yes\">\n"
                + "\n"
                + "    <xsl:mode on-no-match=\"shallow-copy\" streamable=\"yes\"/>\n"
                + "\n"
                + "    <xsl:template match=\"/\">\n"
                + "        <xsl:copy>\n"
                + "            <xsl:apply-templates/>\n"
                + "            <xsl:comment>Run with {static-base-uri()} at {current-dateTime()}</xsl:comment>\n"
                + "        </xsl:copy>\n"
                + "    </xsl:template>\n"
                + "\n"
                + "</xsl:stylesheet>";

 
        try {
            Processor processor = new Processor(true);

            XsltCompiler xsltCompiler = processor.newXsltCompiler();

            XsltExecutable xsltExecutable1 = xsltCompiler.compile(
                    new StreamSource(new StringReader(xsl)));

            Xslt30Transformer xslt30Transformer1 = xsltExecutable1.load30();

            XsltExecutable xsltExecutable2 = xsltCompiler.compile(
                    new StreamSource(new StringReader(xsl)));

            Xslt30Transformer xslt30Transformer2 = xsltExecutable2.load30();

            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            xslt30Transformer1.applyTemplates(
                    new StreamSource(new StringReader(xml)),
                    xslt30Transformer2.asDocumentDestination(xslt30Transformer2.newSerializer(baos)));

            assertTrue(baos.toString("utf-8").contains("<foo>foo 1</foo>"));

        } catch (SaxonApiException | UnsupportedEncodingException e) {
            e.printStackTrace();
            fail(e);
        }
    }

}



