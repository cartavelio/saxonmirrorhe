package bugs;

import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.ResourceResolver;
import net.sf.saxon.s9api.*;
import net.sf.saxon.value.Base64BinaryValue;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Disabled;
import org.junit.jupiter.api.Test;

import java.net.URI;

import static org.junit.jupiter.api.Assertions.fail;

public class Bug6813Test {
    @Test
    public void testFileOk() {
        try {
            Processor processor = new Processor(true);
            if (!"HE".equals(processor.getSaxonEdition())) {
                XPathCompiler compiler = processor.newXPathCompiler();
                compiler.declareNamespace("saxon", "http://saxon.sf.net/");
                String uri = "file://" + System.getProperty("user.dir") + "/../BUILD.md";
                XPathExecutable exec = compiler.compile("saxon:read-binary-resource('" + uri + "')");
                XPathSelector selector = exec.load();
                XdmValue value = selector.evaluate();
                Assertions.assertInstanceOf(Base64BinaryValue.class, value.getUnderlyingValue());
            }
        } catch (Exception ex) {
            fail();
        }
    }

    @Disabled
    public void testFileNotAllowed() {
        try {
            Processor processor = new Processor(true);
            if (!"HE".equals(processor.getSaxonEdition())) {
                processor.getUnderlyingConfiguration().setBooleanProperty(Feature.ALLOW_EXTERNAL_FUNCTIONS, false);
                XPathCompiler compiler = processor.newXPathCompiler();
                compiler.setBaseURI(new URI("file:///tmp/"));
                compiler.declareNamespace("saxon", "http://saxon.sf.net/");
                String uri = "file://" + System.getProperty("user.dir") + "/../BUILD.md";
                XPathExecutable exec = compiler.compile("saxon:read-binary-resource('" + uri + "')");
                XPathSelector selector = exec.load();
                selector.evaluate();
                fail();
            }
        } catch (Exception ex) {
            Assertions.assertTrue(ex.getMessage().indexOf("protocol access denied") > 0);
        }
    }

    @Test
    public void testFileProtocolNotAllowed() {
        try {
            Processor processor = new Processor(true);
            if (!"HE".equals(processor.getSaxonEdition())) {
                processor.setConfigurationProperty(Feature.ALLOWED_PROTOCOLS, "https,http");
                XPathCompiler compiler = processor.newXPathCompiler();
                compiler.setBaseURI(new URI("file:///tmp/"));
                compiler.declareNamespace("saxon", "http://saxon.sf.net/");
                String uri = "file://" + System.getProperty("user.dir") + "/../BUILD.md";
                XPathExecutable exec = compiler.compile("saxon:read-binary-resource('" + uri + "')");
                XPathSelector selector = exec.load();
                selector.evaluate();
                fail();
            }
        } catch (Exception ex) {
            Assertions.assertTrue(ex.getMessage().indexOf("protocol access denied") > 0
                    || ex.getMessage().indexOf("has been prohibited") > 0);
        }
    }

    @Test
    public void testEXPathFileProtocolNotAllowed() {
        try {
            Processor processor = new Processor(true);
            if (!"HE".equals(processor.getSaxonEdition())) {
                ResourceResolver resolver = processor.getUnderlyingConfiguration().getResourceResolver();
                processor.setConfigurationProperty(Feature.ALLOWED_PROTOCOLS, "https,http");

                XPathCompiler compiler = processor.newXPathCompiler();
                compiler.setBaseURI(new URI("file:///tmp/"));
                compiler.declareNamespace("exf", "http://expath.org/ns/file");
                String uri = "file://" + System.getProperty("user.dir") + "/../BUILD.md";
                XPathExecutable exec = compiler.compile("exf:name('" + uri + "')");
                XPathSelector selector = exec.load();
                selector.evaluate();
                fail();
            }
        } catch (Exception ex) {
            Assertions.assertTrue(ex.getMessage().indexOf("protocol access denied") > 0);
        }
    }

}



