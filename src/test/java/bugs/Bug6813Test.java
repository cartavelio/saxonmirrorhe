package bugs;

import net.sf.saxon.lib.CatalogResourceResolver;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.ResourceResolver;
import net.sf.saxon.s9api.*;
import net.sf.saxon.value.Base64BinaryValue;
import org.junit.jupiter.api.Assertions;
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
                String uri = "file://" + System.getProperty("user.dir") + "/BUILD.md";
                XPathExecutable exec = compiler.compile("saxon:read-binary-resource('" + uri + "')");
                XPathSelector selector = exec.load();
                XdmValue value = selector.evaluate();
                Assertions.assertTrue(value.getUnderlyingValue() instanceof Base64BinaryValue);
            }
        } catch (Exception ex) {
            fail();
        }
    }

    @Test
    public void testFileBad() {
        try {
            Processor processor = new Processor(true);
            if (!"HE".equals(processor.getSaxonEdition())) {
                ResourceResolver resolver = processor.getUnderlyingConfiguration().getResourceResolver();
                if (resolver instanceof CatalogResourceResolver) {
                    ((CatalogResourceResolver) resolver).setAllowedProtocols("http,https");
                } else {
                    processor.setConfigurationProperty(Feature.ALLOWED_PROTOCOLS, "https,http");
                }

                XPathCompiler compiler = processor.newXPathCompiler();
                compiler.setBaseURI(new URI("file:///tmp/"));
                compiler.declareNamespace("saxon", "http://saxon.sf.net/");
                String uri = "file://" + System.getProperty("user.dir") + "/BUILD.md";
                XPathExecutable exec = compiler.compile("saxon:read-binary-resource('" + uri + "')");
                XPathSelector selector = exec.load();
                selector.evaluate();
                fail();
            }
        } catch (Exception ex) {
            Assertions.assertTrue(ex.getMessage().indexOf("protocol access denied") > 0);
        }
    }
}



