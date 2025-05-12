package s9apitest;

import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * JUnit tests for the s9api XPathCompiler running in 2.0 mode.
 */
public class TestXPathCompiler20 {

    Processor p = new Processor(false);
    XPathCompiler compiler = p.newXPathCompiler();
    XdmNode document;


    public TestXPathCompiler20() {
        try {
            compiler.setLanguageVersion("2.0");
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
    public void testArith() {
        String result = good("2+2");
        assertEquals("4", result);
    }

    @Test
    public void testIf() {
        String result = good("if (//*) then 3 else 4");
        assertEquals("3", result);
    }

    @Test
    public void testFor() {
        String result = good("string-join(for $i in 1 to 4 return $i, '-')");
        assertEquals("1-2-3-4", result);
    }

    @Test
    public void testExists() {
        String result = good("exists(//*)");
        assertEquals("true", result);
    }

    @Test
    public void testNoBang() {
        bad("(1 to 3)!(. + 1)");
    }

    @Test
    public void testNoConcat() {
        bad("'abc' || 'def'");
    }

    @Test
    public void testNoLet() {
        bad("let $x := 3 return $x + 1");
    }

    @Test
    public void testNoInlineFunctions() {
        bad("function($x){$x + 1}(3)");
    }

    @Test
    public void testNoDynamicFunctionCall() {
        bad("exists#1(3)");
    }

    @Test
    public void testNoEQNames() {
        bad("Q{http://www.w3.org/2005/xpath-functions}exists(3)");
    }

    @Test
    public void testNoConstructors() {
        bad("<a/>");
    }

    @Test
    public void testNoMaps() {
        bad("map{1:'a'}");
    }

    @Test
    public void testNoArrays() {
        bad("[1,2,3,4]");
    }

    @Test
    public void testNoWhere() {
        bad("for $i in 1 to 10 where $i gt 3 return $i");
    }

    @Test
    public void testNoOrderBy() {
        bad("for $i in 1 to 10 order by -$i return $i");
    }

    @Test
    public void testNoCurrent() {
        bad("current()");
    }

    @Test
    public void testNoRound2() {
        bad("round(12345, 2)");
    }

    @Test
    public void testNoGenerateId() {
        bad("generate-id(/)");
    }

    @Test
    public void testNoQFlag() {
        bad("matches('quid', 'quod', 'q'");
    }

    @Test
    public void testNoNonCapturingGroups() {
        bad("replace('(?:abc:)(def)', '$1', 'ghi'");
    }


}

