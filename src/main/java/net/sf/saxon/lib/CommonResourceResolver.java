package net.sf.saxon.lib;

import net.sf.saxon.trans.XPathException;
import org.xml.sax.EntityResolver;
import org.xml.sax.ext.EntityResolver2;
import org.xmlresolver.NamespaceResolver;
import org.xmlresolver.ResolverFeature;

import javax.xml.transform.URIResolver;

public abstract class CommonResourceResolver implements EntityResolver, EntityResolver2, URIResolver, NamespaceResolver {
    protected EntityResolver nextEntityResolver = null;
    protected EntityResolver2 nextEntityResolver2 = null;
    protected URIResolver nextURIResolver = null;
    protected NamespaceResolver nextNamespaceResolver = null;

    public abstract CommonResourceResolver newInstance();
    public abstract <T> T getFeature(ResolverFeature<T> feature);
    public abstract <T> void setFeature(ResolverFeature<T> feature, T value) throws XPathException;

    protected void copyFrom(CommonResourceResolver resolver) {
        nextEntityResolver = resolver.nextEntityResolver;
        nextEntityResolver2 = resolver.nextEntityResolver2;
        nextURIResolver = resolver.nextURIResolver;
        nextNamespaceResolver = resolver.nextNamespaceResolver;
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
    };

    public CommonResourceResolver chain(URIResolver resolver) {
        nextURIResolver = resolver;
        return this;
    }

    public CommonResourceResolver chain(NamespaceResolver resolver) {
        nextNamespaceResolver = resolver;
        return this;
    }
}
