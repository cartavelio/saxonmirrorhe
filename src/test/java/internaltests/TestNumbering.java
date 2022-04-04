package internaltests;

import com.saxonica.config.ProfessionalConfiguration;
import com.saxonica.config.pe.ICUNumbererPE;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.Numberer;
import net.sf.saxon.str.BMPString;
import org.junit.jupiter.api.Test;

import java.util.Locale;

/**
 * JUnit tests to check that Saxon's local numbering is consistent with ICU numbering
 */
public class TestNumbering {

    // For the moment, these tests are not failing if there's a discrepancy, they're merely reporting it.

    private void compareCardinal(long number, ICUNumbererPE icu, Numberer he) {
        String heNumber = he.format(number, BMPString.of("w"), 0, "_", "a", "").replace('-', ' ').replace(",", "").replace(" and ", " ");
        String icuNumber = icu.format(number, BMPString.of("w"), 0, "_", "a", "").replace('-', ' ').replace(",", "").replace(" and ", " ");;
        //assertEquals(icuNumber, heNumber, number + "");
        if (!heNumber.equals(icuNumber)) {
            System.err.println("Mismatch " + number + " (ICU " + icuNumber + " / HE " + heNumber + ")");
        }
    }

    private void compareOrdinal(long number, ICUNumbererPE icu, Numberer he, String scheme) {
        String heNumber = he.format(number, BMPString.of("w"), 0, "_", "a", scheme).replace('-', ' ').replace(",", "").replace(" and ", " ");;
        String icuNumber = icu.format(number, BMPString.of("w"), 0, "_", "a", scheme).replace('-', ' ').replace(",", "").replace(" and ", " ");;
        //assertEquals(icuNumber, heNumber, number + "");
        if (!heNumber.equals(icuNumber)) {
            System.err.println("Mismatch " + number + "(ICU " + icuNumber + " / HE " + heNumber + ")");
        }
    }

    private void testCardinal(String lang) {
        System.err.println("Cardinal - " + lang);
        Locale locale = new Locale(lang);
        Configuration icuConfig = new ProfessionalConfiguration();
        ICUNumbererPE icu = ICUNumbererPE.getNumberer(locale, "xxx", icuConfig);
        Configuration heConfig = new Configuration();
        Numberer he = heConfig.makeNumberer(lang, null);
        for (long i = 0; i <= 100; i++) {
            compareCardinal(i, icu, he);
        }
        for (long i = 1; i <= 20; i++) {
            compareCardinal(i * 100, icu, he);
            compareCardinal(i * 100 + 1, icu, he);
            compareCardinal(i * 100 + 9, icu, he);
            compareCardinal(i * 100 + 10, icu, he);
            compareCardinal(i * 100 + 25, icu, he);
            compareCardinal(i * 100 + 40, icu, he);
        }
        for (long i = 1; i <= 20; i++) {
            compareCardinal(10000 + i * 100, icu, he);
            compareCardinal(23000 + i * 100 + 1, icu, he);
            compareCardinal(140000 + i * 100 + 9, icu, he);
            compareCardinal(300000 + i * 100 + 10, icu, he);
            compareCardinal(1000000 + i * 100 + 25, icu, he);
            compareCardinal(13000000 + i * 100 + 40, icu, he);
        }
    }

    private void testOrdinal(String lang, String scheme) {
        System.err.println("Ordinal - " + lang + " - " + scheme);
        Locale locale = new Locale(lang);
        Configuration icuConfig = new ProfessionalConfiguration();
        ICUNumbererPE icu = ICUNumbererPE.getNumberer(locale, "xxx", icuConfig);
        Configuration heConfig = new Configuration();
        Numberer he = heConfig.makeNumberer(lang, null);
        for (long i = 0; i <= 100; i++) {
            compareOrdinal(i, icu, he, scheme);
        }
        for (long i = 1; i <= 20; i++) {
            compareOrdinal(i * 100, icu, he, scheme);
            compareOrdinal(i * 100 + 1, icu, he, scheme);
            compareOrdinal(i * 100 + 9, icu, he, scheme);
            compareOrdinal(i * 100 + 10, icu, he, scheme);
            compareOrdinal(i * 100 + 25, icu, he, scheme);
            compareOrdinal(i * 100 + 40, icu, he, scheme);
        }
        for (long i = 1; i <= 20; i++) {
            compareOrdinal(10000 + i * 100, icu, he, scheme);
            compareOrdinal(23000 + i * 100 + 1, icu, he, scheme);
            compareOrdinal(140000 + i * 100 + 9, icu, he, scheme);
            compareOrdinal(300000 + i * 100 + 10, icu, he, scheme);
            compareOrdinal(1000000 + i * 100 + 25, icu, he, scheme);
            compareOrdinal(13000000 + i * 100 + 40, icu, he, scheme);
        }
    }


    @Test
    public void testEnglish() {
        testCardinal("en-GB");
        testOrdinal("en-GB", "%spellout-ordinal");
    }

    @Test
    public void testAmerican() {
        testCardinal("en-US");
        testOrdinal("en-US", "%spellout-ordinal");
    }

    @Test
    public void testGerman() {
        testCardinal("de");
        testOrdinal("de", "%spellout-ordinal");
    }

    @Test
    public void testMetropolitanFrench() {
        testCardinal("fr-FR");
        testOrdinal("fr-FR", "%spellout-ordinal");
    }

    @Test
    public void testBelgianFrench() {
        testCardinal("fr-BE");
        testOrdinal("fr-BE", "%spellout-ordinal");
    }

    @Test
    public void testBelgianFlemish() {
        testCardinal("nl-BE");
        testOrdinal("nl-BE", "%spellout-ordinal");
    }

    @Test
    public void testDutch() {
        testCardinal("nl");
        testOrdinal("nl", "%spellout-ordinal");
    }

    @Test
    public void testDanish() {
        testCardinal("da");
        testOrdinal("da", "%spellout-ordinal");
    }

    @Test
    public void testItalian() {
        testCardinal("it");
        testOrdinal("it", "%spellout-ordinal");
    }

    @Test
    public void testHebrew() {
        testCardinal("he");
        testOrdinal("he", "%spellout-ordinal");
    }

    @Test
    public void testTurkish() {
        testCardinal("tr");
        testOrdinal("tr", "%spellout-ordinal");
    }


}
