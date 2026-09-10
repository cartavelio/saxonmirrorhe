package s9apitest;

import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * JUnit tests for the s9api XPathCompiler running in 4.0 mode.
 */
public class TestXPathCompiler40 {

    Processor p = new Processor(true);
    XPathCompiler compiler = p.newXPathCompiler();
    XdmNode document;


    public TestXPathCompiler40() {
        try {
            compiler.setLanguageVersion("4.0");
            DocumentBuilder b = p.newDocumentBuilder();
            document = b.build(new StreamSource(new StringReader("<foo x='1'><bar/><baz/></foo>")));
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    private String good(String expr) {
        try {
            XPathExecutable exec = compiler.compile(expr);
            XPathSelector selector = exec.load();
            selector.setContextItem(document);
            return selector.evaluate().toString();
        } catch (SaxonApiException e1) {
            e1.printStackTrace();
            fail(e1.getMessage());
            return null;
        }
    }

    private void bad(String expr) {
        try {
            XPathExecutable exec = compiler.compile(expr);
            XPathSelector selector = exec.load();
            selector.setContextItem(document);
            fail("unexpected success");
        } catch (SaxonApiException e1) {
            // Failure was expected
        }
    }


    @Test
    public void testIf() {
        String result = good("if (//*) {3}");
        assertEquals("3", result);
    }

    @Test
    public void testFor() {
        String result = good("string-join(for member $a in [10,20,30] for $x in 0 to 2 return $x+$a, '-')");
        assertEquals("10-11-12-20-21-22-30-31-32", result);
    }

    @Test
    public void testOtherwise() {
        String result = good("current-date()[. lt xs:date('1900-01-01')] otherwise 'christmas'");
        assertEquals("christmas", result);
    }

    @Test
    public void testArrayFilter() {
        String result = good("[1,2,3,4]?[. ge 2]?* => string-join('-')");
        assertEquals("2-3-4", result);
    }


    @Test
    public void testLineNumberOfExpression() {
        try {
            XPathExecutable exec = compiler.compile("/dmAddress/\ndmIdent/\ndmCode/\n@modelIdentCode[");
            XPathSelector selector = exec.load();
            selector.setContextItem(document);
            fail("unexpected success");
        } catch (SaxonApiException e1) {
            assertEquals(4, e1.getLineNumber());
        }
    }

    @Test
    public void testLineNumberOfPattern() {
        try {
            XPathExecutable exec = compiler.compilePattern("/dmAddress/\ndmIdent/\ndmCode/\n@modelIdentCode[");
            XPathSelector selector = exec.load();
            selector.setContextItem(document);
            fail("unexpected success");
        } catch (SaxonApiException e1) {
            assertEquals(4, e1.getLineNumber());
        }
    }


}
