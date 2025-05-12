package nativetests;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.NamespaceUri;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.fail;

/**
 *   Test the "born-binary" schema for the FN namespace (the result of fn:analyze-string)
 */
public class FnSchemaTest {

    @Test
    public void testSchema() {
        try {
            EnterpriseConfiguration config = new EnterpriseConfiguration();
            config.addSchemaForBuiltInNamespace(NamespaceUri.FN);
            config.setSchemaValidationMode(Validation.STRICT);
            String instance = "<fn:analyze-string-result xmlns:fn=\"http://www.w3.org/2005/xpath-functions\">" +
                    "<fn:non-match>(</fn:non-match><fn:match>(banana)</fn:match><fn:non-match>)</fn:non-match>" +
                    "</fn:analyze-string-result>";
            config.buildDocumentTree(new StreamSource(new StringReader(instance)));
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testSchema2() {
        try {
            EnterpriseConfiguration config = new EnterpriseConfiguration();
            config.addSchemaForBuiltInNamespace(NamespaceUri.FN);
            config.setSchemaValidationMode(Validation.STRICT);
            String instance = "<fn:analyze-string-result xmlns:fn=\"http://www.w3.org/2005/xpath-functions\">" +
                    "<fn:match><fn:group nr=\"1\">b<fn:group nr=\"2\"/></fn:group></fn:match>" +
                    "<fn:non-match>anana</fn:non-match></fn:analyze-string-result>";
            config.buildDocumentTree(new StreamSource(new StringReader(instance)));
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testSchemaInvalid() {
        // Note, this test produces error messages on system.err, but succeeds anyway.
        // 2016-11-03: the instance actually validates, see bug 29976. Changed to an instance that doesn't validate.
        try {
            EnterpriseConfiguration config = new EnterpriseConfiguration();
            config.addSchemaForBuiltInNamespace(NamespaceUri.FN);
            config.setSchemaValidationMode(Validation.STRICT);
            String instance = "<fn:analyze-string-result xmlns:fn=\"http://www.w3.org/2005/xpath-functions\">" +
                    "<fn:match><fn:group nr=\"1\">b<fn:group nr=\"2\"/></fn:group></fn:match>" +
                    "<fn:non-match>anan<fn:group nr=\"3\">a</fn:group></fn:non-match></fn:analyze-string-result>";
            config.buildDocumentTree(new StreamSource(new StringReader(instance)));
            fail("Invalid content not detected");
        } catch (Exception e) {
            // success
        }
    }

}



