package sorttests;

import junit.framework.TestCase;

import java.text.ParseException;
import java.text.RuleBasedCollator;
import net.sf.saxon.s9api.*;

public class CzechCollation extends TestCase {

    public void testStartsWith() {
        try {
            Processor processor = new Processor(false);
            XPathCompiler compiler = processor.newXPathCompiler();
            QName p = new QName("p");
            compiler.declareVariable(p);
            XPathSelector selector = compiler.compile(
                    "starts-with($p, '\u0161', 'http://saxon.sf.net/collation?class=sorttests.CzechCollation$CzechCollationRules;decomposition=full')").load();
            selector.setVariable(p, new XdmAtomicValue("smith"));
            XdmAtomicValue result = (XdmAtomicValue)selector.evaluateSingle();
            assertTrue("result1", result.getBooleanValue());
            selector.setVariable(p, new XdmAtomicValue("\u0161mith"));
            result = (XdmAtomicValue)selector.evaluateSingle();
            assertTrue("result2", result.getBooleanValue());
        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail();
        }
    }




    public static class CzechCollationRules extends RuleBasedCollator {

        public CzechCollationRules() throws ParseException {
            super(traditionalCzechRules);
        }

        private static String upperAcuteA = "\u00c1";

        private static String lowerAcuteA = "\u00e1";

        private static String upperUmlatA = "\u00c4";

        private static String upperHacekZ = "\u017d";

        private static String lowerHacekZ = "\u017e";

        private static String traditionalCzechRules =

                ("< A=a=" + upperAcuteA + "=" + lowerAcuteA + "=" + upperUmlatA +
                        "< Z=z <" + upperHacekZ + "=" + lowerHacekZ);

    }

}




