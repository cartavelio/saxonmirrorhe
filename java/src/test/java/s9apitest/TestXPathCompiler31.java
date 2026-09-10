package s9apitest;

import net.sf.saxon.lib.Logger;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * JUnit tests for the s9api XPathCompiler running in 3.0 mode.
 */
public class TestXPathCompiler31 {

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
    public void testBang() {
        String result = good("string-join((1 to 3)!(. + 1), '-')");
        assertEquals("2-3-4", result);
    }

    @Test
    public void testConcat() {
        String result = good("'abc' || 'def'");
        assertEquals("abcdef", result);
    }

    @Test
    public void testLet() {
        String result = good("let $x := 3 return $x + 1");
        assertEquals("4", result);
    }

    @Test
    public void testInlineFunctions() {
        String result = good("function($x){$x + 1}(3)");
        assertEquals("4", result);
    }

    @Test
    public void testNoAnnotations() {
        bad("%private function($x){$x + 1}(3)");
    }

    @Test
    public void testDynamicFunctionCall() {
        String result = good("exists#1(3)");
        assertEquals("true", result);
    }

    @Test
    public void testEQNames() {
        String result = good("Q{http://www.w3.org/2005/xpath-functions}exists(3)");
        assertEquals("true", result);
    }

    @Test
    public void testNoConstructors() {
        bad("<a/>");
    }

    @Test
    public void testMaps() {
        String result = good("map{1:'a'}?1");
        assertEquals("a", result);
    }

    @Test
    public void testMapType() {
        String result = good("map{1:'a'} instance of map(*)");
        assertEquals("true", result);
    }

    @Test
    public void testArrays() {
        String result = good("['1a','2a','3a','4a']?2");
        assertEquals("2a", result);
    }

    @Test
    public void testArrayType() {
        String result = good("['1a','2a','3a','4a'] instance of array(*)");
        assertEquals("true", result);
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
    public void testRound2() {
        String result = good("round(12.341, 2)");
        assertEquals("12.34", result);
    }

    @Test
    public void testGenerateId() {
        String result = good("generate-id(/)");
        assertNotNull(result);
        assertTrue(result.length() > 0);
    }

    @Test
    public void testQFlag() {
        String result = good("matches('quid', 'quod', 'q')");
        assertEquals("false", result);
    }

    @Test
    public void testNoNonCapturingGroups() {
        String result = good("replace('abcdef', '(?:abc)(def)', '$1')");
        assertEquals("def", result);
    }

    @Test
    public void testMapFunctions() {
        String result = good("Q{http://www.w3.org/2005/xpath-functions/map}get(Q{http://www.w3.org/2005/xpath-functions/map}entry('a', 'b'), 'a')");
        assertEquals("b", result);
    }

    @Test
    public void testArrayFunctions() {
        String result = good("Q{http://www.w3.org/2005/xpath-functions/array}flatten(Q{http://www.w3.org/2005/xpath-functions/array}append([], '12'))");
        assertEquals("12", result);
    }

    @Test
    public void testDefaultLanguage() {
        String result = good("default-language()");
        assertEquals("en", result);
    }

    @Test
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
            assertEquals("29", val.getStringValue(), "result");
            assertEquals("<age:ag xmlns:age=\"http://www.w3.org/2003/01/geo/wgs84_pos#\">29</age:ag>", val.toString(), "serialized");
        } catch (SaxonApiException e1) {
            fail();
        }
    }

    @Test
    public void testUnprefixedElementPolicy() {
        try {
            Processor p = new Processor(false);
            DocumentBuilder db = p.newDocumentBuilder();
            XdmNode doc =
                    db.build(new StreamSource(new StringReader(
                            "<doc><p>one</p><p xmlns='n2'>two</p><n3:p xmlns:n3='n3'>three</n3:p></doc>")));
            XPathCompiler c = p.newXPathCompiler();
            c.setLanguageVersion ("3.1");
            XdmItem result = c.evaluateSingle("string-join(//p)", doc);
            assertEquals("one", result.getStringValue());
            c.setUnprefixedElementMatchingPolicy(UnprefixedElementMatchingPolicy.ANY_NAMESPACE);
            result = c.evaluateSingle("string-join(//p)", doc);
            assertEquals("onetwothree", result.getStringValue());
            c.setUnprefixedElementMatchingPolicy(UnprefixedElementMatchingPolicy.DEFAULT_NAMESPACE);
            c.declareNamespace("", "n2");
            result = c.evaluateSingle("string-join(//p)", doc);
            assertEquals("two", result.getStringValue());
            c.setUnprefixedElementMatchingPolicy(UnprefixedElementMatchingPolicy.DEFAULT_NAMESPACE_OR_NONE);
            result = c.evaluateSingle("string-join(//p)", doc);
            assertEquals("onetwo", result.getStringValue());
        } catch (Exception e) {
            fail();
        }
    }

    @Test
    public void testUndeclaredVariables() {
        try {
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            c.setLanguageVersion("3.1");
            c.setAllowUndeclaredVariables(true);
            assertTrue(c.isAllowUndeclaredVariables());
            XPathExecutable exec = c.compile("$x + 1");
            XPathSelector selector = exec.load();
            selector.setVariable(new QName("x"), new XdmAtomicValue(23));
            XdmItem result = selector.evaluateSingle();
            assertEquals("24", result.getStringValue());
        } catch (Exception e) {
            fail();
        }
    }

    @Test
    public void testTraceFunction() {
        try {
            HashMap<String, Integer> map = new HashMap<>();
            map.put("x", 42);
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            c.setLanguageVersion("3.1");
            XPathExecutable exec = c.compile("23 + trace(?x)");
            XPathSelector selector = exec.load();
            List<String> messages = new ArrayList<>();
            selector.setTraceFunctionDestination(new Logger() {
                public void println(String message, int severity) {
                    messages.add(message);
                }
            });
            selector.setContextItem(XdmMap.makeMap(map));
            XdmItem result = selector.evaluateSingle();
            assertEquals("65", result.getStringValue());
            assertEquals(1, messages.size());
            assertTrue(messages.get(0).contains("42"));
        } catch (Exception e) {
            fail();
        }
    }


    @Test
    public void TestDefaultDecimalFormat() {
        try {
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            c.setLanguageVersion("3.1");
            c.setDecimalFormatProperty("decimal-separator", "~");
            c.setDecimalFormatProperty("grouping-separator", "|");
            c.setDecimalFormatProperty("exponent-separator", "E");
            XPathExecutable e = c.compile("format-number(123.4, '0|000~0E0')");
            XPathSelector s = e.load();
            String result = s.evaluateSingle().getStringValue();
            assertEquals("1|234~0E-1", result);
        } catch (Exception e) {
            System.err.println(e.getMessage());
            fail(e);
        }
    }

}

