package s9apitest;

import junit.framework.TestCase;
import net.sf.saxon.s9api.*;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;

/**
 * JUnit tests for the s9api XPathCompiler running in 3.0 mode.
 *
 * @author Michael Kay
 * @date Dec2017
 */
public class TestXPathCompiler30 extends TestCase {

    Processor p = new Processor(true);
    XPathCompiler compiler = p.newXPathCompiler();
    XdmNode document;


    public TestXPathCompiler30() {
        try {
            compiler.setLanguageVersion("3.0");
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

    public void testArith() {
        String result = good("2+2");
        assertEquals("4", result);
    }

    public void testIf() {
        String result = good("if (//*) then 3 else 4");
        assertEquals("3", result);
    }

    public void testFor() {
        String result = good("string-join(for $i in 1 to 4 return $i, '-')");
        assertEquals("1-2-3-4", result);
    }

    public void testExists() {
        String result = good("exists(//*)");
        assertEquals("true", result);
    }

    public void testBang() {
        String result = good("string-join((1 to 3)!(. + 1), '-')");
        assertEquals("2-3-4", result);
    }

    public void testConcat() {
        String result = good("'abc' || 'def'");
        assertEquals("abcdef", result);
    }

    public void testLet() {
        String result = good("let $x := 3 return $x + 1");
        assertEquals("4", result);
    }

    public void testInlineFunctions() {
        String result = good("function($x){$x + 1}(3)");
        assertEquals("4", result);
    }

    public void testNoAnnotations() {
        bad("%private function($x){$x + 1}(3)");
    }

    public void testDynamicFunctionCall() {
        String result = good("exists#1(3)");
        assertEquals("true", result);
    }

    public void testEQNames() {
        String result = good("Q{http://www.w3.org/2005/xpath-functions}exists(3)");
        assertEquals("true", result);
    }

    public void testNoConstructors() {
        bad("<a/>");
    }

    public void testNoMaps() {
        bad("map{1:'a'}");
    }

    public void testNoArrays() {
        bad("[1,2,3,4]");
    }

    public void testNoMapType() {
        bad("23 instance of map(*)");
    }

    public void testNoArrayType() {
        bad("23 instance of array(*)");
    }
    
    public void testNoWhere() {
        bad("for $i in 1 to 10 where $i gt 3 return $i");
    }

    public void testNoOrderBy() {
        bad("for $i in 1 to 10 order by -$i return $i");
    }

    public void testNoCurrent() {
        bad("current()");
    }

    public void testRound2() {
        String result = good("round(12.341, 2)");
        assertEquals("12.34", result);
    }

    public void testGenerateId() {
        String result = good("generate-id(/)");
        assertTrue(result.length() > 0);
    }

    public void testQFlag() {
        String result = good("matches('quid', 'quod', 'q')");
        assertEquals("false", result);
    }

    public void testNoNonCapturingGroups() {
        String result = good("replace('abcdef', '(?:abc)(def)', '$1')");
        assertEquals("def", result);
    }

    public void testMapFunctions() {
        // allowed in 3.0 mode as extension functions
        String result = good("Q{http://www.w3.org/2005/xpath-functions/map}get(Q{http://www.w3.org/2005/xpath-functions/map}entry('a', 'b'), 'a')");
        assertEquals("b", result);
    }

    public void testNoLookup() {
        bad("Q{http://www.w3.org/2005/xpath-functions/map}entry('a', 'b')?a");
    }

    public void testArrayFunctions() {
        // allowed in 3.0 mode as extension functions
        String result = good("Q{http://www.w3.org/2005/xpath-functions/array}flatten(Q{http://www.w3.org/2005/xpath-functions/array}append(Q{http://www.w3.org/2005/xpath-functions/array}join(()), '12'))");
        assertEquals("12", result);
    }

    public void testNoDefaultLanguage() {
        bad("default-language()");
    }

}

