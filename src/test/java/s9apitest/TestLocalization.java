package s9apitest;

import junit.framework.TestCase;
import net.sf.saxon.s9api.*;

/**
 * JUnit tests for the s9api XQueryCompiler
 */
public class TestLocalization extends TestCase {

    public void testCollationLangUsingIcu() {
        collationLang(true);
    }

    public void testCollationLangUsingJava() {
        collationLang(false);
    }

    public void collationLang(boolean useICU) {
        try {
            Processor p = new Processor(useICU);
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryExecutable exec = c.compile(
                    "compare('z', 'ä', 'http://www.w3.org/2013/collation/UCA?lang=de')");
            XdmValue val = exec.load().evaluate();
            assertEquals(((XdmAtomicValue) val).getLongValue(), 1);
            exec = c.compile(
                    "compare('z', 'ä', 'http://www.w3.org/2013/collation/UCA?lang=sv')");
            val = exec.load().evaluate();
            assertEquals(((XdmAtomicValue) val).getLongValue(), -1);
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    public void testCollationStrengthUsingIcu() {
        collationStrength(true);
    }

    public void testCollationStrengthUsingJava() {
        collationStrength(false);
    }

    private void collationStrength(boolean useICU) {
        try {
            Processor p = new Processor(useICU);
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryExecutable exec = c.compile(
                    "compare('z', 'Z', 'http://www.w3.org/2013/collation/UCA?lang=en;strength=primary')");
            XdmValue val = exec.load().evaluate();
            assertEquals(((XdmAtomicValue) val).getLongValue(), 0);
            exec = c.compile(
                    "compare('z', 'Z', 'http://www.w3.org/2013/collation/UCA?lang=sv;strenth=identical')");
            val = exec.load().evaluate();
            assertEquals(((XdmAtomicValue) val).getLongValue(), -1);
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    public void testCollationDecompositionUsingIcu() {
        collationDecomposition(true);
    }

    public void testCollationDecompositionUsingJava() {
        collationDecomposition(false);
    }

    public void collationDecomposition(boolean useICU) {
        try {
            Processor p = new Processor(useICU);
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryExecutable exec = c.compile(
                    "compare('c\u0327', '\u00e7', 'http://www.w3.org/2013/collation/UCA?lang=es;normalization=yes;strength=identical')");
            XdmValue val = exec.load().evaluate();
            assertEquals(0, ((XdmAtomicValue) val).getLongValue());
            exec = c.compile(
                    "compare('c\u0327', '\u00e7', 'http://www.w3.org/2013/collation/UCA?lang=es;normalization=no;strength=identical')");
            val = exec.load().evaluate();
            assertEquals(useICU ? 0 : -1, ((XdmAtomicValue) val).getLongValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    private String s22(boolean useICU, String lang) {
        try {
            Processor p = new Processor(useICU);
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryExecutable exec = c.compile("declare variable $lang external; format-integer(22, 'Ww', $lang)");
            XQueryEvaluator runner = exec.load();
            runner.setExternalVariable(new QName("lang"), new XdmAtomicValue(lang));
            return runner.evaluateSingle().getStringValue();
        } catch (SaxonApiException e) {
            fail(e.getMessage());
            return "";
        }
    }

    public void testHyphenationICU() {
        assertEquals("a", "Twenty Two", s22(true, "en-GB"));
        assertEquals("b", "Twenty Two", s22(true, "en-US"));
        assertEquals("c", "Twenty-Two", s22(true, "en-x-hyphen"));
        assertEquals("d", "Twenty-Two", s22(true, "en-GB-x-hyphen"));
        assertEquals("e", "Twenty-Two", s22(true, "en-US-x-hyphen"));
    }

    public void testHyphenationSansICU() {
        assertEquals("p", "Twenty Two", s22(false, "en-GB"));
        assertEquals("q", "Twenty Two", s22(false, "en-US"));
        assertEquals("r", "Twenty-Two", s22(false, "en-x-hyphen"));
        assertEquals("s", "Twenty-Two", s22(false, "en-GB-x-hyphen"));
        assertEquals("t", "Twenty-Two", s22(false, "en-US-x-hyphen"));
    }

    private String s22o(boolean useICU, String lang) {
        try {
            Processor p = new Processor(useICU);
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryExecutable exec = c.compile("declare variable $lang external; format-integer(23, 'Ww;o', $lang)");
            XQueryEvaluator runner = exec.load();
            runner.setExternalVariable(new QName("lang"), new XdmAtomicValue(lang));
            return runner.evaluateSingle().getStringValue();
        } catch (SaxonApiException e) {
            fail(e.getMessage());
            return "";
        }
    }

    public void testHyphenationOrdinalICU() {
        assertEquals("a", "Twenty Third", s22o(true, "en-GB"));
        assertEquals("b", "Twenty Third", s22o(true, "en-US"));
        assertEquals("c", "Twenty-Third", s22o(true, "en-x-hyphen"));
        assertEquals("d", "Twenty-Third", s22o(true, "en-GB-x-hyphen"));
        assertEquals("e", "Twenty-Third", s22o(true, "en-US-x-hyphen"));
        assertEquals("f", "Twenty Third", s22o(true, "en-x-nohyphen"));
    }

    public void testHyphenationOrdinalSansICU() {
        // inconsistent for backwards compatibility
        assertEquals("p", "Twenty-Third", s22o(false, "en-GB"));
        // inconsistent for backwards compatibility
        assertEquals("q", "Twenty-Third", s22o(false, "en-US"));
        assertEquals("r", "Twenty-Third", s22o(false, "en-x-hyphen"));
        assertEquals("s", "Twenty-Third", s22o(false, "en-GB-x-hyphen"));
        assertEquals("t", "Twenty-Third", s22o(false, "en-US-x-hyphen"));
        assertEquals("u", "Twenty Third", s22o(false, "en-x-nohyphen"));
    }


}