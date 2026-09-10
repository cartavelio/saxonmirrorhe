package resolvers;

import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.*;
import net.sf.saxon.om.TreeInfo;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import org.junit.Test;
import org.xml.sax.InputSource;
import org.xml.sax.SAXException;
import org.xmlresolver.ResolverFeature;

import javax.xml.transform.Source;
import javax.xml.transform.TransformerException;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.IOException;
import java.net.URI;
import java.net.URISyntaxException;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertTrue;
import static org.junit.Assert.fail;

public class ResolverConfigurationTest {
    private final ConfigTest configTest = new ConfigTest();
    
    @Test
    public void defaultCatalogResourceResolver() {
        Configuration config = new Configuration();
        assertNotNull(config.getResourceResolver());
        assertEquals("net.sf.saxon.lib.CatalogResourceResolver", config.getResourceResolver().getClass().getName());
    }

    @Test
    public void explicitCatalogResourceResolverClass() {
        Configuration config = new Configuration();
        config.setConfigurationProperty(Feature.RESOURCE_RESOLVER_CLASS, "net.sf.saxon.lib.CatalogResourceResolver");
        assertNotNull(config.getResourceResolver());
        assertEquals("net.sf.saxon.lib.CatalogResourceResolver", config.getResourceResolver().getClass().getName());
    }

    @Test
    public void explicitCatalogResourceResolver() {
        Configuration config = new Configuration();

        CatalogResourceResolver underlyingResolver = new CatalogResourceResolver();

        MyResolver resolver = new MyResolver(underlyingResolver);

        config.setConfigurationProperty(Feature.RESOURCE_RESOLVER, resolver);
        assertNotNull(config.getResourceResolver());
        assertEquals("resolvers.ResolverConfigurationTest$MyResolver", config.getResourceResolver().getClass().getName());
    }

    @Test
    public void explicitErroneousCatalogResourceResolver() {
        Configuration config = new Configuration();

        try {
            config.setConfigurationProperty(Feature.RESOURCE_RESOLVER, "Hello");
            fail();
        } catch (IllegalArgumentException ex) {
            assertTrue(ex.getMessage().contains("RESOURCE_RESOLVER value must be an instance of"));
        } catch (Exception ex) {
            fail();
        }
    }

    @Test
    public void wrappedUriResolver() {
        try {
            Configuration config = new Configuration();
            config.setResourceResolver(new ResourceResolverWrappingURIResolver(new StandardURIResolver(config)));
            TreeInfo info = config.buildDocumentTree(new StreamSource(new File(configTest.getDataDir() + "books.xml")));
            assertTrue(info.getRootNode().getBaseURI().endsWith("books.xml"));
        } catch (XPathException e) {
            fail();
        }
    }

    @Test
    public void wrappedUriResolver2() {
        try {
            Configuration config = new Configuration();
            Processor proc = new Processor(config);
            config.setResourceResolver(
                    new ResourceResolverWrappingURIResolver(
                            new StandardURIResolver(config) {
                                @Override
                                public Source resolve(String href, String base)
                                        throws XPathException {
                                    if ((href != null) && (base != null) && base.startsWith("classpath:")) {
                                        try {
                                            URI uri = new URI(href);

                                            if (!uri.isAbsolute()) {
                                                href = "classpath:" +
                                                        new URI(base.substring("classpath:".length())).
                                                                resolve(uri).
                                                                toString();
                                            }
                                        } catch (URISyntaxException e) {
                                            fail();
                                        }
                                    }

                                    return super.resolve(href, base);
                                }
                            }));

            //config.setResourceResolver(new ResourceResolverWrappingURIResolver(new StandardURIResolver(config)));
            TreeInfo info = config.buildDocumentTree(new StreamSource(new File(configTest.getDataDir() + "books.xml")));
            XQueryCompiler xqc = proc.newXQueryCompiler();
            xqc.setBaseURI(new File(configTest.getDataDir()).toURI());
            XQueryExecutable exec = xqc.compile("doc('books.xml')");
            exec.load().run(new NullDestination());
            assertTrue(info.getRootNode().getBaseURI().endsWith("books.xml"));
        } catch (XPathException | SaxonApiException e) {
            fail();
        }
    }

    private class MyResolver extends CatalogResourceResolver {
        private final CatalogResourceResolver resolver;

        public MyResolver(CatalogResourceResolver resolver) {
            this.resolver = resolver;
        }

        //@Override
        public CatalogResourceResolver newInstance() {
            return this;
        }

        @Override
        public <T> T getFeature(ResolverFeature<T> feature) {
            return resolver.getFeature(feature);
        }

        @Override
        public <T> void setFeature(ResolverFeature<T> feature, T value)  {
            resolver.setFeature(feature, value);
        }

        @Override
        public Source resolve(ResourceRequest rr) throws XPathException {
            return resolver.resolve(rr);
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
