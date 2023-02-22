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
import java.util.Collections;

/**
 * The standard implementation of the CommonResourceResolver used by Saxon.
 *
 * This class extends the {@Link CommonResourceResolver} providing concrete implementations of
 * all of the resolver interfaces. This class delegates actual resolution to an underlying
 * {@link org.xmlresolver.Resolver} that it wraps.
 *
 * @since 11
 */

public class CatalogResourceResolver extends CommonResourceResolver {
    private final Resolver resolver;

    /**
     * Creates a new CatalogResourceResolver resolver with a default {@link org.xmlresolver.XMLResolverConfiguration}.
     * This default configuration will read a configuration file from the class path
     * and interrogate system properties to determine its initial configuration.
     *
     * The configuration can be updated by calling {@link #setFeature}. Alternatively,
     * the constructor can be passed an existing resolver.
     */
    public CatalogResourceResolver() {
        // We explicitly pass in an empty list so that in the absence of any other configuration,
        // the resolver doesn't use ./catalog.xml as a default. Historically, Saxon didn't use a
        // resolver unless a catalog was specified, so this default could have unexpected consequences.
        resolver = new Resolver(new XMLResolverConfiguration(null, Collections.emptyList()));
    }

    /**
     * Creates a new CatlaogResourceResolver using the provided resolver as its underlying resolver.
     * @param resolver The resolver to wrap.
     */
    public CatalogResourceResolver(Resolver resolver) {
        this.resolver = resolver;
    }

    /**
     * Return a new instance of the resolver. This method creates a new CommonResourceResolver
     * that uses the same underlying {@link org.xmlresolver.Resolver}.
     * @return A new CatalogResourceResolver.
     */
    public CommonResourceResolver newInstance() {
        CommonResourceResolver newResolver = new CatalogResourceResolver(resolver);
        newResolver.copyFrom(this);
        return newResolver;
    }

    /**
     * Chains an entity resolver.
     * @param resolver the fallback resolver. If the fallback resolver is an instance of <code>EntityResolver2</code>,
     *                 it will be used as the fallback for that interface as well.
     * @return this
     */
    public CommonResourceResolver chain(EntityResolver resolver) {
        nextEntityResolver = resolver;
        if (resolver instanceof EntityResolver2) {
            nextEntityResolver2 = (EntityResolver2) resolver;
        }
        return this;
    }

    /**
     * Chains an entity resolver.
     * @param resolver the fallback resolver.
     * @return this
     */
    public CommonResourceResolver chain(EntityResolver2 resolver) {
        nextEntityResolver = resolver;
        nextEntityResolver2 = resolver;
        return this;
    }

    /**
     * Chains a URI resolver.
     * @param resolver the fallback resolver.
     * @return this.
     */
    public CommonResourceResolver chain(URIResolver resolver) {
        nextURIResolver = resolver;
        return this;
    }

    /**
     * Calls {@link org.xmlresolver.XMLResolverConfiguration#getFeature} on the underlying
     * resolver configuration.
     * @param feature The feature setting
     * @param <T> The feature type
     * @return The value for the specified feature.
     */
    public <T> T getFeature(ResolverFeature<T> feature) {
        if (resolver == null) {
            return null;
        }
        return resolver.getConfiguration().getFeature(feature);
    }

    /**
     * Calls {@link org.xmlresolver.XMLResolverConfiguration#setFeature} on the underlying
     * resolver configuration.
     * @param feature The feature setting
     * @param value The desired value for that feature
     * @param <T> The feature type
     * @throws NullPointerException if the underlying resolver is null. Some features will
     * also throw this exception if the value provided is null and that's not a meaningful
     * feature value.
     */
    public <T> void setFeature(ResolverFeature<T> feature, T value) {
        if (resolver == null) {
            throw new NullPointerException();
        }
        resolver.getConfiguration().setFeature(feature, value);
    }

    /**
     * Resolve the URI. If the specified <code>href</code> cannot be resolved, it will be
     * made absolute against the <code>base</code> (if the base has been provided). If the
     * resulting absolute URI is different from <code>href</code>, an attempt will be
     * made to resolve the absolute URI.
     *
     * If catalog resolution fails and a fallback URI resolver has been chained, the
     * fallback resolver will be attempted.
     *
     * @param href The URI
     * @param base The base URI
     * @return The resolved resource, or null if it could not be resolved.
     * @throws TransformerException if an error occurs during the attempt to resolve the URI.
     */
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

    /**
     * Resolves an external subset. This method is part of the {@link org.xml.sax.ext.EntityResolver2} interface.
     * The resolver will attempt to find the external subset through the catalog resolver.
     *
     * If catalog resolution fails and a fallback EntityResolver2 resolver has been chained, the
     * fallback resolver will be attempted.
     *
     * @param name The doctype name.
     * @param baseURI The base URI.
     * @return The external subset, or null if it could not be found.
     * @throws SAXException If an error occurs during the attempt to resolve the external subset.
     * @throws IOException If it isn't possible to create the input source or if the base URI is invalid.
     */
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

    /**
     * Resolves an entity. This method attempts to resolve the entity with the catalog resolver.
     *
     * If catalog resolution fails and a fallback EntityResolver2 resolver has been chained, the
     * fallback resolver will be attempted. If that fails and a further EntityResolver has been chained,
     * that fallback will also be attempted.
     *
     * Depending on whether various aspects of the entity are provided (public and system identifiers,
     * the name and baseURI, etc.), different aspects of the catalog will be queried. Not all parsers
     * provide all of these parameters. It's common for the name and baseURI to be null, for example.
     * If the parser doesn't provide them, then the catalog resolver will not be able to resolve with them.
     *
     * @param name The name of the entity, often null
     * @param publicId The public identifier of the entity, often null
     * @param baseURI The base URI of the entity, often null
     * @param systemId The system identifier of the entity
     * @return The entity, or null if it could not be found.
     * @throws SAXException If an error occurs during the attempt to resolve the external subset.
     * @throws IOException If it isn't possible to create the input source or if the base URI is invalid.
     */
    @Override
    public InputSource resolveEntity(String name, String publicId, String baseURI, String systemId) throws SAXException, IOException {
        InputSource result = null;
        if (resolver != null) {
            result = resolver.resolveEntity(name, publicId, baseURI, systemId);
        }
        if (result == null && nextEntityResolver2 != null) {
            result = nextEntityResolver2.resolveEntity(name, publicId, baseURI, systemId);
        }
        if (result == null && nextEntityResolver != null && nextEntityResolver != nextEntityResolver2) {
            result = nextEntityResolver.resolveEntity(publicId, systemId);
        }
        return result;
    }

    /**
     * Resolves an entity. This method attempts to resolve the entity with the catalog resolver.
     *
     * If catalog resolution fails and a fallback EntityResolver2 resolver has been chained, the
     * fallback resolver will be attempted. If that fails and a further EntityResolver has been chained,
     * that fallback will also be attempted.
     *
     * @param publicId The public identifier of the entity, often null
     * @param systemId The system identifier of the entity
     * @return The entity, or null if it could not be found.
     * @throws SAXException If an error occurs during the attempt to resolve the external subset.
     * @throws IOException If it isn't possible to create the input source or if the base URI is invalid.
     */
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

    /**
     * Resolves a URI that is known to be a namespace URI.
     *
     * This intereface allows a resolver to request a particular kind of resource (one with a particular nature,
     * possibly for a particular purpose) for a URI. The URI is usually the namespace URI. Namespace URIs are
     * often not usefully resolvable on the web, but a catalog resolver can still offer resolution.
     *
     * If neither a nature or a purpose are provided, or if using them produces no results, this method
     * simply attempts to lookup the URI in the catalog. If that also fails, and if a fallback namespace
     * resolver has been chained, resolution will be attempted with the fallback resolver.
     *
     * @param uri The namespace URI
     * @param nature The nature of the resource requested, for example, the URI of the media type
     * @param purpose The purpose of the request, for example "validation"
     * @return The resource or null if it could not be found.
     * @throws TransformerException if an error occurs during the attempt to resolve the URI.
     */
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
