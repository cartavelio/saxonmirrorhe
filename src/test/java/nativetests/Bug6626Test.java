package nativetests;

import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.io.StringWriter;

import static org.junit.jupiter.api.Assertions.fail;

/**
 * Test various aspects of setting the Saxon Configuration from Java code
 */
public class Bug6626Test {

    @Test
    public void testBug6626() {
        String xml = "<root>\n"
                + "    <foo>foo 1</foo>\n"
                + "    <bar>bar 1</bar>\n"
                + "</root>";

        String xsl1 = "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n"
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

        String xsl2 = "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n"
                + "                xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n"
                + "                xmlns:saxon=\"http://saxon.sf.net/\"\n"
                + "                exclude-result-prefixes=\"#all\"\n"
                + "                version=\"3.0\"\n"
                + "                expand-text=\"yes\">\n"
                + "\n"
                + "    <xsl:accumulator name=\"latest-foo-value\" streamable=\"yes\" initial-value=\"()\" as=\"xs:string?\">\n"
                + "        <xsl:accumulator-rule match=\"foo/text()\" select=\"string()\"/>\n"
                + "    </xsl:accumulator>\n"
                + "\n"
                + "    <xsl:mode on-no-match=\"shallow-copy\" streamable=\"yes\" use-accumulators=\"latest-foo-value\"/>\n"
                + "\n"
                + "    <xsl:mode name=\"grounded\" on-no-match=\"shallow-copy\"/>\n"
                + "\n"
                + "    <xsl:template match=\"bar\">\n"
                + "        <xsl:comment>node {node-name()}: accumulator-before('latest-foo-value'): {accumulator-before('latest-foo-value')}</xsl:comment>\n"
                + "        <xsl:apply-templates select=\"copy-of()\" mode=\"grounded\"/>\n"
                + "    </xsl:template>\n"
                + "\n"
                + "    <xsl:template mode=\"grounded\" match=\"bar\">\n"
                + "        <xsl:comment>node {node-name()}: accumulator-before('latest-foo-value'): {accumulator-before('latest-foo-value')}</xsl:comment>\n"
                + "        <xsl:next-match/>\n"
                + "    </xsl:template>\n"
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

            XsltExecutable xsltExecutable1 = xsltCompiler.compile(new StreamSource(new StringReader(xsl1)));

            Xslt30Transformer xslt30Transformer1 = xsltExecutable1.load30();

            XsltExecutable xsltExecutable2 = xsltCompiler.compile(new StreamSource(new StringReader(xsl2)));

            Xslt30Transformer xslt30Transformer2 = xsltExecutable2.load30();

//            // Run transformation 2 on its own
//            StringWriter sw0 = new StringWriter();
//            Xslt30Transformer xslt30Transformer0 = xsltExecutable2.load30();
//            xslt30Transformer0.applyTemplates(new StreamSource(new StringReader(xml)), xslt30Transformer0.newSerializer(sw0));
//            System.err.println("===Standalone:===");
//            System.err.println(sw0);

            // Run transformation 1 followed by 2
            StringWriter sw2 = new StringWriter();
            xslt30Transformer1.applyTemplates(
                    new StreamSource(new StringReader(xml)),
                    xslt30Transformer2.asDocumentDestination(xslt30Transformer2.newSerializer(sw2)));

            System.err.println("===Combined:===");
            System.err.println(sw2);
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e);
        }
    }

}



