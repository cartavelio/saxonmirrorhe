package net.sf.saxon.lib;

import org.xml.sax.EntityResolver;
import org.xml.sax.InputSource;
import org.xml.sax.SAXException;
import org.xml.sax.ext.EntityResolver2;
import org.xmlresolver.Resolver;
import org.xmlresolver.ResolverFeature;
import org.xmlresolver.XMLResolverConfiguration;

import javax.xml.transform.Source;
import javax.xml.transform.TransformerException;
import javax.xml.transform.URIResolver;
import java.io.IOException;

public class CatalogResourceResolver extends CommonResourceResolver {
    private final Resolver resolver;

    public CatalogResourceResolver() {
        resolver = new Resolver(new XMLResolverConfiguration());
    }

    public CatalogResourceResolver(Resolver resolver) {
        this.resolver = resolver;
    }

    public CommonResourceResolver newInstance() {
        CommonResourceResolver newResolver = new CatalogResourceResolver(resolver);
        newResolver.copyFrom(this);
        return newResolver;
    }

    public CommonResourceResolver chain(EntityResolver resolver) {
        nextEntityResolver = resolver;
        if (resolver instanceof EntityResolver2) {
            nextEntityResolver2 = (EntityResolver2) resolver;
        }
        return this;
    }

    public CommonResourceResolver chain(EntityResolver2 resolver) {
        nextEntityResolver = resolver;
        nextEntityResolver2 = resolver;
        return this;
    }

    public CommonResourceResolver chain(URIResolver resolver) {
        nextURIResolver = resolver;
        return this;
    }

    public <T> T getFeature(ResolverFeature<T> feature) {
        if (resolver == null) {
            return null;
        }
        return resolver.getConfiguration().getFeature(feature);
    }

    public <T> void setFeature(ResolverFeature<T> feature, T value) {
        if (resolver == null) {
            throw new NullPointerException();
        }
        resolver.getConfiguration().setFeature(feature, value);
    }

    @Override
    public Source resolve(String href, String base) throws TransformerException {
        Source result = null;
        if (resolver != null) {
            result = resolver.resolve(href, base);
        }
        if (result == null && nextURIResolver != null) {
            result = nextURIResolver.resolve(href, base);
        }
        return result;
    }

    @Override
    public InputSource getExternalSubset(String name, String baseURI) throws SAXException, IOException {
        InputSource result = null;
        if (resolver != null) {
            result = resolver.getExternalSubset(name, baseURI);
        }
        if (result == null && nextEntityResolver2 != null) {
            result = nextEntityResolver2.getExternalSubset(name, baseURI);
        }
        return result;
    }

    @Override
    public InputSource resolveEntity(String name, String publicId, String baseURI, String systemId) throws SAXException, IOException {
        InputSource result = null;
        if (resolver != null) {
            result = resolver.resolveEntity(name, publicId, baseURI, systemId);
        }
        if (result == null && nextEntityResolver2 != null) {
            result = nextEntityResolver2.resolveEntity(name, publicId, baseURI, systemId);
        }
        if (result == null && nextEntityResolver != null) {
            result = nextEntityResolver.resolveEntity(publicId, systemId);
        }
        return result;
    }

    @Override
    public InputSource resolveEntity(String publicId, String systemId) throws SAXException, IOException {
        InputSource result = null;
        if (resolver != null) {
            result = resolver.resolveEntity(publicId, systemId);
        }
        if (result == null && nextEntityResolver2 != null) {
            result = nextEntityResolver2.resolveEntity(publicId, systemId);
        }
        if (result == null && nextEntityResolver != null && nextEntityResolver != nextEntityResolver2) {
            result = nextEntityResolver.resolveEntity(publicId, systemId);
        }
        return result;
    }

    @Override
    public Source resolveNamespace(String uri, String nature, String purpose) throws TransformerException {
        Source result = null;
        if (resolver != null) {
            result = resolver.resolveNamespace(uri, nature, purpose);
        } if (resolver == null && nextNamespaceResolver != null) {
            result = nextNamespaceResolver.resolveNamespace(uri, nature, purpose);
        }
        return result;
    }
}
