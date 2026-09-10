package internaltests;

import com.saxonica.config.ProfessionalConfiguration;
import com.saxonica.config.pe.ICUNumbererPE;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.Numberer;
import net.sf.saxon.str.BMPString;
import org.junit.jupiter.api.Test;

import java.util.Locale;

import static org.junit.jupiter.api.Assertions.assertEquals;

/**
 * JUnit tests to check that Saxon's local numbering is consistent with ICU numbering
 */
public class TestNumbering {

    private void compareCardinal(long number, ICUNumbererPE icu, Numberer he) {
        String heNumber = he.format(number, BMPString.of("w"), 0, "_", "a", "").replace('-', ' ').replace(",", "");
        String icuNumber = icu.format(number, BMPString.of("w"), 0, "_", "a", "").replace('-', ' ').replace(",", "");
        assertEquals(icuNumber, heNumber, number+"");
    }

    private void compareOrdinal(long number, ICUNumbererPE icu, Numberer he, String scheme) {
        String heNumber = he.format(number, BMPString.of("w"), 0, "_", "a", scheme).replace('-', ' ').replace(",", "");
        String icuNumber = icu.format(number, BMPString.of("w"), 0, "_", "a", scheme).replace('-', ' ').replace(",", "");
        assertEquals(icuNumber, heNumber, number + "");
    }

    private void testCardinal(String lang) {
        Locale locale = new Locale(lang);
        Configuration icuConfig = new ProfessionalConfiguration();
        ICUNumbererPE icu = ICUNumbererPE.getNumberer(locale, "xxx", icuConfig);
        Configuration heConfig = new Configuration();
        Numberer he = heConfig.makeNumberer(lang, null);
        for (long i=0; i<=100; i++) {
             compareCardinal(i, icu, he);
        }
        for (long i=1; i<=20; i++) {
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
        testCardinal("de");
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
        testOrdinal("en-GB", "%spellout-ordinal");
    }


}
