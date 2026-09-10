package s9apitest;

import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * JUnit tests for the s9api XQueryCompiler
 */
public class TestLocalization {

    @Test
    public void testCollationLangUsingIcu() {
        collationLang(true);
    }

    @Test
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
            assertEquals(1, ((XdmAtomicValue) val).getLongValue());
            exec = c.compile(
                    "compare('z', 'ä', 'http://www.w3.org/2013/collation/UCA?lang=sv')");
            val = exec.load().evaluate();
            assertEquals(((XdmAtomicValue) val).getLongValue(), -1);
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testCollationStrengthUsingIcu() {
        collationStrength(true);
    }

    @Test
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
            assertEquals(0, ((XdmAtomicValue) val).getLongValue());
            exec = c.compile(
                    "compare('z', 'Z', 'http://www.w3.org/2013/collation/UCA?lang=sv;strenth=identical')");
            val = exec.load().evaluate();
            assertEquals(((XdmAtomicValue) val).getLongValue(), -1);
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testCollationDecompositionUsingIcu() {
        collationDecomposition(true);
    }

    @Test
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

    @Test
    public void testHyphenationICU(){
        assertEquals("Twenty Two", s22(true, "en-GB"), "a");
        assertEquals("Twenty Two", s22(true, "en-US"), "b");
        assertEquals("Twenty-Two", s22(true, "en-x-hyphen"), "c");
        assertEquals("Twenty-Two", s22(true, "en-GB-x-hyphen"), "d");
        assertEquals("Twenty-Two", s22(true, "en-US-x-hyphen"), "e");
    }

    @Test
    public void testHyphenationSansICU(){
        assertEquals("Twenty Two", s22(false, "en-GB"), "p");
        assertEquals("Twenty Two", s22(false, "en-US"), "q");
        assertEquals("Twenty-Two", s22(false, "en-x-hyphen"), "r");
        assertEquals("Twenty-Two", s22(false, "en-GB-x-hyphen"), "s");
        assertEquals("Twenty-Two", s22(false, "en-US-x-hyphen"), "t");
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

    @Test
    public void testHyphenationOrdinalICU() {
        assertEquals("Twenty Third", s22o(true, "en-GB"), "a");
        assertEquals("Twenty Third", s22o(true, "en-US"), "b");
        assertEquals("Twenty-Third", s22o(true, "en-x-hyphen"), "c");
        assertEquals("Twenty-Third", s22o(true, "en-GB-x-hyphen"), "d");
        assertEquals("Twenty-Third", s22o(true, "en-US-x-hyphen"), "e");
        assertEquals("Twenty Third", s22o(true, "en-x-nohyphen"), "f");
    }

    @Test
    public void testHyphenationOrdinalSansICU() {
        // inconsistent for backwards compatibility
        assertEquals("Twenty-Third", s22o(false, "en-GB"), "p");
        // inconsistent for backwards compatibility
        assertEquals("Twenty-Third", s22o(false, "en-US"), "q");
        assertEquals("Twenty-Third", s22o(false, "en-x-hyphen"), "r");
        assertEquals("Twenty-Third", s22o(false, "en-GB-x-hyphen"), "s");
        assertEquals("Twenty-Third", s22o(false, "en-US-x-hyphen"), "t");
        assertEquals("Twenty Third", s22o(false, "en-x-nohyphen"), "u");
    }


}
