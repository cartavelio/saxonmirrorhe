package net.sf.saxon.lib;

import net.sf.saxon.trans.XPathException;
import org.xml.sax.EntityResolver;
import org.xml.sax.ext.EntityResolver2;
import org.xmlresolver.NamespaceResolver;
import org.xmlresolver.ResolverFeature;

import javax.xml.transform.URIResolver;

public abstract class SaxonResourceResolver implements EntityResolver, EntityResolver2, URIResolver, NamespaceResolver {
    protected EntityResolver nextEntityResolver = null;
    protected EntityResolver2 nextEntityResolver2 = null;
    protected URIResolver nextURIResolver = null;
    protected NamespaceResolver nextNamespaceResolver = null;

    public abstract SaxonResourceResolver newInstance();
    public abstract <T> T getFeature(ResolverFeature<T> feature);
    public abstract <T> void setFeature(ResolverFeature<T> feature, T value) throws XPathException;

    protected void copyFrom(SaxonResourceResolver resolver) {
        nextEntityResolver = resolver.nextEntityResolver;
        nextEntityResolver2 = resolver.nextEntityResolver2;
        nextURIResolver = resolver.nextURIResolver;
        nextNamespaceResolver = resolver.nextNamespaceResolver;
    }

    public SaxonResourceResolver chain(EntityResolver resolver) {
        nextEntityResolver = resolver;
        if (resolver instanceof EntityResolver2) {
            nextEntityResolver2 = (EntityResolver2) resolver;
        }
        return this;
    }

    public SaxonResourceResolver chain(EntityResolver2 resolver) {
        nextEntityResolver = resolver;
        nextEntityResolver2 = resolver;
        return this;
    };

    public SaxonResourceResolver chain(URIResolver resolver) {
        nextURIResolver = resolver;
        return this;
    }

    public SaxonResourceResolver chain(NamespaceResolver resolver) {
        nextNamespaceResolver = resolver;
        return this;
    }
}
