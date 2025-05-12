package internaltests;

import com.saxonica.config.JavaLocalizerFactory;
import com.saxonica.config.ProfessionalConfiguration;
import com.saxonica.config.pe.ICUNumbererPE;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.Numberer;
import net.sf.saxon.str.BMPString;

import java.util.Locale;

import static org.junit.jupiter.api.Assertions.assertEquals;

/**
 * JUnit tests to check that Saxon's local numbering is consistent with ICU numbering
 */
public class NumberingTests {

    private void compareCardinal(long number, ICUNumbererPE icu, Numberer he) {
        String heNumber = he.format(number, BMPString.of("w"), 0, "_", "a", "", "").replace('-', ' ').replace(",", "");
        String icuNumber = icu.format(number, BMPString.of("w"), 0, "_", "a", "", "")
                .replace('-', ' ').replace(",", "").replace("\u00AD", "");
        assertEquals(icuNumber, heNumber, number+"");
    }

    private void compareOrdinal(long number, ICUNumbererPE icu, Numberer he, String scheme) {
        String heNumber = he.format(number, BMPString.of("w"), 0, "_", "a", "", scheme).replace('-', ' ').replace(",", "");
        String icuNumber = icu.format(number, BMPString.of("w"), 0, "_", "a", "", scheme).replace('-', ' ').replace(",", "");
        assertEquals(icuNumber, heNumber, number + "");
    }

    private void testCardinal(String lang, String country) {
        Locale locale = new Locale(lang, country);
        Configuration icuConfig = new ProfessionalConfiguration();
        ICUNumbererPE icu = ICUNumbererPE.getNumberer(locale, "xxx", icuConfig);
        Configuration heConfig = new Configuration();
        JavaLocalizerFactory factory = new JavaLocalizerFactory(heConfig);
        factory.setBuiltInLocalizationClasses();
        heConfig.setLocalizerFactory(factory);
        Numberer he = heConfig.makeNumberer(lang + "-" + country, null);
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

    private void testOrdinal(String lang, String country, String scheme) {
        Locale locale = new Locale(lang, country);
        Configuration icuConfig = new ProfessionalConfiguration();
        ICUNumbererPE icu = ICUNumbererPE.getNumberer(locale, "xxx", icuConfig);
        Configuration heConfig = new Configuration();
        JavaLocalizerFactory factory = new JavaLocalizerFactory(heConfig);
        factory.setBuiltInLocalizationClasses();
        heConfig.setLocalizerFactory(factory);
        Numberer he = heConfig.makeNumberer(lang + "-" + country, null);
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


    //@Test
    public void testBritishEnglish() {
        testCardinal("en", "GB");
        testOrdinal("en", "GB", "%spellout-ordinal-verbose");
    }

    //@Test
    public void testAmericanEnglish() {
        testCardinal("en", "US");
        testOrdinal("en", "US", "%spellout-ordinal-verbose");
    }

    //@Test
    public void testGerman() {
        testCardinal("de","DE");
        testOrdinal("de","DE", "%spellout-ordinal");
    }

    //@Test
    public void testMetropolitanFrench() {
        testCardinal("fr","FR");
        testOrdinal("fr","FR", "%spellout-ordinal");
    }

    //@Test
    public void testBelgianFrench() {
        testCardinal("fr","BE");
        testOrdinal("fr","BE", "%spellout-ordinal");
    }

    //@Test
    public void testBelgianFlemish() {
        testCardinal("nl","BE");
        testOrdinal("nl","BE", "%spellout-ordinal");
    }

    //@Test
    public void testDutch() {
        testCardinal("nl","NL");
        testOrdinal("nl","NL", "%spellout-ordinal");
    }

    //@Test
    public void testDanish() {
        testCardinal("de","DE");
        testOrdinal("da", "DE", "%spellout-ordinal");
    }

    //@Test
    public void testItalian() {
        testCardinal("it", "IT");
        testOrdinal("it", "IT", "%spellout-ordinal");
    }

    //@Test
    public void testHebrew() {
        testCardinal("he", "IS");
        testOrdinal("he", "IS", "%spellout-ordinal");
    }

    //@Test
    public void testTurkish() {
        testCardinal("tr", "TR");
        testOrdinal("tr","TR", "%spellout-ordinal");
    }


}
