package internaltests;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.PreparedSchema;
import com.saxonica.ee.schema.sdoc.FnNamespaceSchema;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.Sender;
import net.sf.saxon.event.Sink;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.fail;


/**
 *  Tests of the schema for the FN namespace
 */
public class FnNamespaceSchemaTest {

    private void validate(String content, boolean valid) {
        EnterpriseConfiguration config = new EnterpriseConfiguration();
        PreparedSchema schema = new FnNamespaceSchema(config).buildSchema();
        ParseOptions validationOptions = new ParseOptions().withSchemaValidationMode(Validation.STRICT);
        Receiver validator = schema.getDocumentValidator(
                new Sink(config.makePipelineConfiguration()), "test.xml", validationOptions, Loc.NONE);
        StreamSource ss = new StreamSource(new StringReader(content));
        try {
            Sender.send(ss, validator, new ParseOptions());
            if (!valid) {
                fail("Validation should have failed");
            }
        } catch (XPathException e) {
            if (valid) {
                fail("Validation failed");
            }
        }
    }

    @Test
    public void test001() {
        String s = "<analyze-string-result xmlns='http://www.w3.org/2005/xpath-functions'/>";
        validate(s, true);
    }

    @Test
    public void test002() {
        String s = "<analyze-string-result xmlns='http://www.w3.org/2005/xpath-functions'>"
                + "<match>xx</match>"
                + "<non-match>yy</non-match>"
                + "<match><group nr='1'>zz</group>z</match>"
                + "</analyze-string-result>";
        validate(s, true);
    }

    @Test
    public void test003() {
        String s = "<analyze-string-result xmlns='http://www.w3.org/2005/xpath-functions'>"
                + "<match>xx</match>"
                + "<non-match>yy</non-match>"
                + "<match><group nr='1'>zz</group>z</match>"
                + "<splurge/>"
                + "</analyze-string-result>";
        validate(s, false);
    }

    @Test
    public void test004() {
        String s = "<analyze-string-result xmlns='http://www.w3.org/2005/xpath-functions'>"
                + "<match>xx</match>"
                + "<non-match>yy</non-match>"
                + "<match><group nr='1'>zz</group><group nr='2'>zz</group>z<lookahead-group nr='3' value='qqq' position='10'/></match>"
                + "</analyze-string-result>";
        validate(s, true);
    }
}
