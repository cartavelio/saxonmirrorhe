package nativetests;

import com.saxonica.config.JavaLocalizerFactory;
import com.saxonica.config.ProfessionalConfiguration;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.number.Numberer_en;
import net.sf.saxon.lib.LocalizerFactory;
import net.sf.saxon.lib.Numberer;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XPathCompiler;
import org.junit.jupiter.api.Test;

import java.util.Properties;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Test various aspects of setting the Saxon Configuration from Java code
 */
public class ConfigurationTest {

    /**
     * Demonstrate use of a user-written LocalizerFactory to customize numbering
     */

    @Test
    public void testCustomizedNumbering() {
        try {
            Configuration config = new ProfessionalConfiguration();
            LocalizerFactory lf = new JavaLocalizerFactory(config) {
                @Override
                public Numberer getNumberer(String language, String country) {
                    Numberer n = super.getNumberer(language, country);
                    if (n instanceof Numberer_en) {
                        // request "twenty-six" rather than "twenty six"
                        ((Numberer_en) n).setTensUnitsSeparatorCardinal("-");
                    }
                    return n;
                }
            };
            config.setLocalizerFactory(lf);
            Processor p = new Processor(config);
            XPathCompiler xp = p.newXPathCompiler();
            xp.setLanguageVersion("3.0");
            String result = xp.compile("format-integer(26, 'w')").load().evaluateSingle().toString();
            assertEquals("twenty six", result.replace('-', ' '), "result");
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testLicenseProperties() {
        Configuration config = new ProfessionalConfiguration();
        Properties props = config.getLicenseFeatures();
        assertEquals("Saxonica", props.getProperty("Licensor"));
        assertNotNull(props.getProperty("Licensee"));
        assertNotNull(props.getProperty("Email"));
        assertNotNull(props.getProperty("Issued"));
        assertNotNull(props.getProperty("Signature"));
    }


}



