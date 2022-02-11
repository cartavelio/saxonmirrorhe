package nativetests;

import com.saxonica.config.JavaLocalizerFactory;
import com.saxonica.config.ProfessionalConfiguration;
import com.saxonica.config.ProfessionalTransformerFactory;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.number.Numberer_en;
import net.sf.saxon.lib.LocalizerFactory;
import net.sf.saxon.lib.Numberer;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XPathCompiler;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Test;

import javax.xml.transform.ErrorListener;
import javax.xml.transform.TransformerConfigurationException;
import javax.xml.transform.TransformerException;
import javax.xml.transform.stream.StreamSource;
import java.io.File;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Test various aspects of setting the Saxon Configuration from Java code
 */
public class ConfigurationTest {

    /**
     * Demonstrate use of a user-written LocalizerFactory to customize numbering
     */

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
    public void testBug4650() {
        try {
            Configuration configuration = Configuration.readConfiguration(new StreamSource(new File("/Users/mike/bugs/2020/4650-Nadolu/SaxonTest-2/resources/saxonPEConfig.xml")));
            ProfessionalTransformerFactory factory = new ProfessionalTransformerFactory((ProfessionalConfiguration) configuration);
            factory.setErrorListener(new ErrorListener() {
                @Override
                public void warning(TransformerException exception) throws TransformerException {
                    exception.printStackTrace(System.out);
                }

                @Override
                public void fatalError(TransformerException exception) throws TransformerException {
                    exception.printStackTrace(System.out);
                }

                @Override
                public void error(TransformerException exception) throws TransformerException {
                    exception.printStackTrace(System.out);
                }
            });
            factory.newTransformer(new StreamSource(new File("resources/test-xsl-validation.xsl")));
        } catch (TransformerConfigurationException e) {
            e.printStackTrace(System.out);
        } catch (XPathException e) {
            e.printStackTrace(System.out);
        }
    }
}



