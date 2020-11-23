package s9apitest;

import junit.framework.TestCase;
import net.sf.saxon.s9api.*;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;

/**
 * test that xsl:assert assertions can be enabled and disabled at run-time and compile-time
 */
public class TestAssert extends TestCase {

    String xsl =
        "<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema'>" +
            "<xsl:template name='main'>" +
            "<xsl:for-each select='1 to 5'>" +
            "<xsl:assert test='. instance of xs:string'>Not a string</xsl:assert>" +
            "</xsl:for-each>" +
            "</xsl:template>" +
            "</xsl:transform>";

    public void testDisabledByDefault() {
        try {
            // Run code with a failing assertion, which is no problem because assertions are disabled by default

            Processor p = new Processor(true);
            XsltCompiler c = p.newXsltCompiler();
            XsltExecutable e = c.compile(new StreamSource(new StringReader(xsl)));
            XsltTransformer t = e.load();
            t.setInitialTemplate(new QName("main"));
            t.setDestination(p.newSerializer(System.out));
            t.transform();
        } catch (SaxonApiException e1) {
            e1.printStackTrace();
            fail();
        }
    }

    public void testEnabledAtCompileTime() {
        try {
            // Run code with a failing assertion, which causes failure because assertions are enabled
            Processor p = new Processor(true);
            XsltCompiler c = p.newXsltCompiler();
            c.setAssertionsEnabled(true);
            XsltExecutable e = c.compile(new StreamSource(new StringReader(xsl)));
            XsltTransformer t = e.load();
            t.setInitialTemplate(new QName("main"));
            t.setDestination(p.newSerializer(System.out));
            t.transform();
            fail();
        } catch (SaxonApiException e1) {
            assertTrue(e1.getMessage().contains("assert"));
        }
    }

    public void testDisabledAtRunTime() {
        try {
            // Run code with a failing assertion, which causes failure because assertions are enabled
            Processor p = new Processor(true);
            XsltCompiler c = p.newXsltCompiler();
            c.setAssertionsEnabled(true);
            XsltExecutable e = c.compile(new StreamSource(new StringReader(xsl)));
            Xslt30Transformer t = e.load30();
            t.setAssertionsEnabled(false);
            t.callTemplate(new QName("main"), p.newSerializer(System.out));
        } catch (SaxonApiException e1) {
            fail();
        }
    }
}

