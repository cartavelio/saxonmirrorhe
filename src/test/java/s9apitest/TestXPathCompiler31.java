package s9apitest;

import junit.framework.TestCase;
import net.sf.saxon.s9api.*;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;

/**
 * JUnit tests for the s9api XPathCompiler running in 3.0 mode.
 */
public class TestXPathCompiler31 extends TestCase {

    Processor p = new Processor(true);
    XPathCompiler compiler = p.newXPathCompiler();
    XdmNode document;


    public TestXPathCompiler31() {
        try {
            compiler.setLanguageVersion("3.1");
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

    public void testMaps() {
        String result = good("map{1:'a'}?1");
        assertEquals("a", result);
    }

    public void testMapType() {
        String result = good("map{1:'a'} instance of map(*)");
        assertEquals("true", result);
    }

    public void testArrays() {
        String result = good("['1a','2a','3a','4a']?2");
        assertEquals("2a", result);
    }

    public void testArrayType() {
        String result = good("['1a','2a','3a','4a'] instance of array(*)");
        assertEquals("true", result);
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
        String result = good("Q{http://www.w3.org/2005/xpath-functions/map}get(Q{http://www.w3.org/2005/xpath-functions/map}entry('a', 'b'), 'a')");
        assertEquals("b", result);
    }

    public void testArrayFunctions() {
        String result = good("Q{http://www.w3.org/2005/xpath-functions/array}flatten(Q{http://www.w3.org/2005/xpath-functions/array}append([], '12'))");
        assertEquals("12", result);
    }

    public void testDefaultLanguage() {
        String result = good("default-language()");
        assertEquals("en", result);
    }

    public void testBug4081() {
        String xml="<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
                + "<Employees>\n"
                + "    <Employee id=\"1\">\n"
                + "        <age:ag xmlns:age=\"http://www.w3.org/2003/01/geo/wgs84_pos#\">29</age:ag>\n"
                + "        <name>Pankaj</name>\n"
                + "        <gender>Male</gender>\n"
                + "        <role>Java Developer</role>\n"
                + "    </Employee>\n"
                + "    <Employee id=\"2\">\n"
                + "        <age:ag xmlns:age=\"http://www.w3.org/2003/01/geo/wgs84_pos#\">35</age:ag>\n"
                + "        <name>Lisa</name>\n"
                + "        <gender>Female</gender>\n"
                + "        <role>CEO</role>\n"
                + "    </Employee>\n"
                + "    <Employee id=\"3\">\n"
                + "        <age:ag xmlns:age=\"http://www.w3.org/2003/01/geo/wgs84_pos#\">45</age:ag>\n"
                + "        <name>Tom</name>\n"
                + "        <gender>Male</gender>\n"
                + "        <role>Manager</role>\n"
                + "    </Employee>\n"
                + "    <Employee id=\"4\">\n"
                + "        <age:ag xmlns:age=\"http://www.w3.org/2003/01/geo/wgs84_pos#\">55</age:ag>\n"
                + "        <name>Meghan</name>\n"
                + "        <gender>Female</gender>\n"
                + "        <role>Manager</role>\n"
                + "    </Employee>\n"
                + "</Employees>";

        String xpath = "//Employees/Employee[1]/age:ag";
        try {
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            c.declareNamespace("age", "http://www.w3.org/2003/01/geo/wgs84_pos#");
            XPathExecutable e = c.compile(xpath);
            XPathSelector s = e.load();
            s.setContextItem(p.newDocumentBuilder().build(new StreamSource(new StringReader(xml))));
            XdmItem val = s.evaluateSingle();
            assertEquals("result", "29", val.getStringValue());
            assertEquals("serialized", "<age:ag xmlns:age=\"http://www.w3.org/2003/01/geo/wgs84_pos#\">29</age:ag>", val.toString());
        } catch (SaxonApiException e1) {
            fail();
        }
    }

}

