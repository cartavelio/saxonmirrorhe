package resolvers;

import net.sf.saxon.lib.CatalogResourceResolver;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.ResourceResolver;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XPathCompiler;
import org.junit.Test;
import org.xmlresolver.ResolverFeature;

import static junit.framework.TestCase.fail;

public class Bug5334 {
    @Test
    public void testBug5334() {
        String[] fileTests = { "unparsed-text('not-existing.txt')",
                "unparsed-text('foo.txt')",
                "json-doc('not-existing.json')",
                "json-doc('foo.json')",
                "doc('not-existing.xml')",
                "doc('foo.xml')" };

        Processor processor = new Processor(false);

        //runXPathTests(processor, fileTests);

        ResourceResolver defaultResolver = (ResourceResolver)processor.getConfigurationProperty(Feature.RESOURCE_RESOLVER);

        processor.getUnderlyingConfiguration().setConfigurationProperty(Feature.ALLOWED_PROTOCOLS, "http");

        /*
        CatalogResourceResolver catalogResourceResolver = (CatalogResourceResolver) defaultResolver;
        catalogResourceResolver.setFeature(ResolverFeature.ACCESS_EXTERNAL_DOCUMENT, "http");
        catalogResourceResolver.setFeature(ResolverFeature.THROW_URI_EXCEPTIONS, true);
        catalogResourceResolver.setFeature(ResolverFeature.ACCESS_EXTERNAL_ENTITY, "http");
        catalogResourceResolver.setFeature(ResolverFeature.MERGE_HTTPS, true);
         */

        runXPathTests(processor, fileTests);
    }

    private static void runXPathTests(Processor processor, String[] fileTests) {
        XPathCompiler xpathCompiler = processor.newXPathCompiler();

        for (String fileTest : fileTests) {
            try {
                System.out.println(xpathCompiler.evaluateSingle(fileTest, null));
                fail();
            }
            catch (SaxonApiException ex) {
                System.out.println(ex.getMessage());
            }
        }
    }
}
