package resolvers;

import net.sf.saxon.Configuration;
import net.sf.saxon.lib.CatalogResourceResolver;
import net.sf.saxon.lib.CommonResourceResolver;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.trans.XPathException;
import org.junit.Test;
import org.xml.sax.InputSource;
import org.xml.sax.SAXException;
import org.xmlresolver.ResolverFeature;

import javax.xml.transform.Source;
import javax.xml.transform.TransformerException;
import java.io.IOException;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertTrue;
import static org.junit.Assert.fail;

public class ResolverConfigurationTest {
    @Test
    public void defaultCatalogResourceResolver() {
        Configuration config = new Configuration();
        assertNotNull(config.getCommonResourceResolver());
        assertEquals("net.sf.saxon.lib.CatalogResourceResolver", config.getCommonResourceResolver().getClass().getName());
    }

    @Test
    public void explicitCatalogResourceResolverClass() {
        Configuration config = new Configuration();
        config.setConfigurationProperty(Feature.SAXON_RESOURCE_RESOLVER_CLASS, "net.sf.saxon.lib.CatalogResourceResolver");
        assertNotNull(config.getCommonResourceResolver());
        assertEquals("net.sf.saxon.lib.CatalogResourceResolver", config.getCommonResourceResolver().getClass().getName());
    }

    @Test
    public void explicitCatalogResourceResolver() {
        Configuration config = new Configuration();

        CommonResourceResolver underlyingResolver = new CatalogResourceResolver();

        MyResolver resolver = new MyResolver(underlyingResolver);

        config.setConfigurationProperty(Feature.SAXON_RESOURCE_RESOLVER, resolver);
        assertNotNull(config.getCommonResourceResolver());
        assertEquals("resolvers.ResolverConfigurationTest$MyResolver", config.getCommonResourceResolver().getClass().getName());
    }

    @Test
    public void explicitErroneousCatalogResourceResolver() {
        Configuration config = new Configuration();

        try {
            config.setConfigurationProperty(Feature.SAXON_RESOURCE_RESOLVER, "Hello");
            fail();
        } catch (IllegalArgumentException ex) {
            assertTrue(ex.getMessage().contains("SAXON_RESOURCE_RESOLVER value must be an instance of"));
        } catch (Exception ex) {
            fail();
        }
    }

    private class MyResolver extends CommonResourceResolver {
        private final CommonResourceResolver resolver;

        public MyResolver(CommonResourceResolver resolver) {
            this.resolver = resolver;
        }

        @Override
        public CommonResourceResolver newInstance() {
            return this;
        }

        @Override
        public <T> T getFeature(ResolverFeature<T> feature) {
            return resolver.getFeature(feature);
        }

        @Override
        public <T> void setFeature(ResolverFeature<T> feature, T value) throws XPathException {
            resolver.setFeature(feature, value);
        }

        @Override
        public Source resolve(String href, String base) throws TransformerException {
            return resolver.resolve(href, base);
        }

        @Override
        public InputSource getExternalSubset(String name, String baseURI) throws SAXException, IOException {
            return resolver.getExternalSubset(name, baseURI);
        }

        @Override
        public InputSource resolveEntity(String name, String publicId, String baseURI, String systemId) throws SAXException, IOException {
            return resolver.resolveEntity(name, publicId, baseURI, systemId);
        }

        @Override
        public InputSource resolveEntity(String publicId, String systemId) throws SAXException, IOException {
            return resolver.resolveEntity(publicId, systemId);
        }

        @Override
        public Source resolveNamespace(String uri, String nature, String purpose) throws TransformerException {
            return resolver.resolveNamespace(uri, nature, purpose);
        }
    }

}
