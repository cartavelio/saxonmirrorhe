package sorttests;


import java.text.ParseException;
import java.text.RuleBasedCollator;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

public class CzechCollation {

    @Test
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
            assertTrue(result.getBooleanValue(), "result1");
            selector.setVariable(p, new XdmAtomicValue("\u0161mith"));
            result = (XdmAtomicValue)selector.evaluateSingle();
            assertTrue(result.getBooleanValue(), "result2");
        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail();
        }
    }




    public static class CzechCollationRules extends RuleBasedCollator {

        public CzechCollationRules() throws ParseException {
            super(traditionalCzechRules);
        }

        private static final String upperAcuteA = "\u00c1";

        private static final String lowerAcuteA = "\u00e1";

        private static final String upperUmlatA = "\u00c4";

        private static final String upperHacekZ = "\u017d";

        private static final String lowerHacekZ = "\u017e";

        private static final String traditionalCzechRules =

                ("< A=a=" + upperAcuteA + "=" + lowerAcuteA + "=" + upperUmlatA +
                        "< Z=z <" + upperHacekZ + "=" + lowerHacekZ);

    }

}




