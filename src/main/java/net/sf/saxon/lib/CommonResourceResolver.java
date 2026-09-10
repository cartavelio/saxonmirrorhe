package net.sf.saxon.lib;

import net.sf.saxon.trans.XPathException;
import org.xml.sax.EntityResolver;
import org.xml.sax.ext.EntityResolver2;
import org.xmlresolver.NamespaceResolver;
import org.xmlresolver.ResolverFeature;

import javax.xml.transform.URIResolver;

/**
 * The CommonResourceResolver is the centralized handler for resolving URIs to resources.
 *
 * XSLT and XQuery read data from URIs in a number of different places:
 *
 * 1. XML documents are loaded from URIs with <code>doc()</code> and related functions
 * 2. External parsed entities are resolved within those documents.
 * 3. Unparsed (text) documents are loaded from URIs with <code>unparsed-text()</code> and related functions
 * 4. Stylesheet and query modules are imported from URIs
 * 5. Schemas are imported by URI and their imports are loaded
 *
 * Saxonica provides different resolver classes in each of these places, allowing application writers
 * to have fine-grained control of the resources returned.
 *
 * Starting in Saxon 11, the default implementation of all these different resolvers is centralized
 * in the CommonResourceResolver. The standard implementation of this abstract class is provided by
 * CatalogResourceResolver which uses the XML Resolver, an XML Catalog based resolver to map
 * URIs to resources.
 *
 * <blockquote><p>Note: There is an unfortunate but unavoidable terminology issue here to bear in mind. Within the context
 * of a resource resolver, the notion of "NamespaceResolver" is about mapping from a namespace URI,
 * for example a module namespace, to the resource that contains the code for the module. This is
 * distinct from, and completely different to, the notion of resolving a namespace prefix to a URI,
 * which is a common operation within the transformation of an XML document.</p></blockquote>
 *
 * <ul>
 *     <li><code>EntityResolver</code> (and <code>EntityResolver2</code>) are used for XML documents and
 *     external parsed entities</li>
 *     <li><code>NamespaceResolver</code> is used for query modules.</li>
 *     <li><code>URIResolver</code> is used for other URIs.</li>
 * </ul>
 *
 * Each <code>Configuration</code> has its own resource resolver and they are independent of each other.
 * It is possible to specify a new, common resolver for a <code>Configuration</code> by providing a concrete extension of the
 * <code>CommonResourceResolver</code> class to {@link net.sf.saxon.Configuration#setCommonResourceResolver}.
 * This does not replace the existing APIs which allow
 * an application to specify individual resolvers in various places. Those resolvers will take precedence
 * over the common resolver.
 *
 * It is a common feature of existing resolver APIs that they chain together. For example,
 * it is often the case that when a new <code>EntityResolver</code> is added to an <code>XMLReader</code>,
 * it is configured so that the existing (possibly underlying, implementation defined) resolver will be called
 * if the newly specified resolver fails to resolve the URI.
 *
 * The <code>chain</code> methods allow for this kind of chaining. A <code>CommonResourceResolver</code>
 * can be instructed to chain failed entity resolution through a previous or different resolver.
 *
 * The chain methods returns "this" so that calls to the chain methods can be strung together in a fluent style.
 *
 * The resolver methods in this class are expected to return <code>null</code> if the requested resource
 * could not be found. It's common for the caller to then simply attempt to resolve the resource by
 * dereferencing the URI, but the resolver APIs don't do this. (If they did, it would be impossible for
 * the caller to determine if the resolver was able to locate the resource or not, sometimes useful information.)
 *
 * @since 11
 */

public abstract class CommonResourceResolver implements EntityResolver, EntityResolver2, URIResolver, NamespaceResolver {
    protected EntityResolver nextEntityResolver = null;
    protected EntityResolver2 nextEntityResolver2 = null;
    protected URIResolver nextURIResolver = null;
    protected NamespaceResolver nextNamespaceResolver = null;

    /**
     * Return a new instance of the resolver. This method allows a new resolver to be constructed
     * that has the same underlying (implementation specific) configuration as an existing resolver.
     * @return a new resolver.
     */
    public abstract CommonResourceResolver newInstance();

    /**
     * Returns a feature setting from the resolver. As the standard version of this resolver is
     * built on top of the XML Resolver, we simply reuse those feature keys. An alternate implementation
     * is not required to support all the keys and may choose alternate implementations for them.
     * @param feature The feature setting
     * @param <T> The type of the feature
     * @return The value of the feature
     */
    public abstract <T> T getFeature(ResolverFeature<T> feature);

    /**
     * Sets a feature in the resolver. As the standard version of this resolver is
     * built on top of the XML Resolver, we simply reuse those feature keys. An alternate implementation
     * is not required to support all the keys and may choose alternate implementations for them.
     * @param feature The feature setting
     * @param value The desired value for that feature
     * @param <T> The type of the feature
     * @throws XPathException if the feature is unsupported or the value is not usable
     * @throws NullPointerException if the feature is null or if the value is null and null is not an acceptable value for the feature.
     */
    public abstract <T> void setFeature(ResolverFeature<T> feature, T value) throws XPathException;

    protected void copyFrom(CommonResourceResolver resolver) {
        nextEntityResolver = resolver.nextEntityResolver;
        nextEntityResolver2 = resolver.nextEntityResolver2;
        nextURIResolver = resolver.nextURIResolver;
        nextNamespaceResolver = resolver.nextNamespaceResolver;
    }

    /**
     * Chains an <code>EntityResolver</code> to this resolver.
     *
     * <p>Returns a new CommonResourceResolver that shares the underlying configuration of the current
     * resolver. The new resolver will chain requests to resolve an entity through the specified
     * EntityResolver if resolution otherwise failed.</p>
     *
     * @param resolver the fallback resolver. If the fallback resolver is an instance of <code>EntityResolver2</code>,
     *                 it will be used as the fallback for that interface as well.
     * @return this
     */
    public CommonResourceResolver chain(EntityResolver resolver) {
        CommonResourceResolver copy = newInstance();
        copy.copyFrom(this);
        copy.nextEntityResolver = resolver;
        if (resolver instanceof EntityResolver2) {
            copy.nextEntityResolver2 = (EntityResolver2) resolver;
        }
        return copy;
    }

    /**
     * Chains an <code>EntityResolver2</code> to this resolver.
     *
     * <p>Returns a new CommonResourceResolver that shares the underlying configuration of the current
     * resolver. The new resolver will chain requests to resolve an entity through the specified
     * EntityResolver2 if resolution otherwise failed.</p>
     *
     * <p>An EntityResolver2 is an extension of the EntityResolver class; this method sets
     * both resolvers to use the EntityResolver2 passed in.</p>
     *
     * @param resolver the fallback resolver.
     * @return this
     */
    public CommonResourceResolver chain(EntityResolver2 resolver) {
        CommonResourceResolver copy = newInstance();
        copy.copyFrom(this);
        copy.nextEntityResolver = resolver;
        copy.nextEntityResolver2 = resolver;
        return copy;
    };

    /**
     * Chains a <code>URIResolver</code> to this resolver.
     *
     * <p>Returns a new CommonResourceResolver that shares the underlying configuration of the current
     * resolver. The new resolver will chain requests to resolve URIs through the specified
     * URIResolver if resolution otherwise failed.</p>
     *
     * @param resolver the fallback resolver.
     * @return this
     */
    public CommonResourceResolver chain(URIResolver resolver) {
        CommonResourceResolver copy = newInstance();
        copy.copyFrom(this);
        copy.nextURIResolver = resolver;
        return copy;
    }

    /**
     * Chains a <code>NamespaceResolver</code> to this resolver.
     *
     * <p>Returns a new CommonResourceResolver that shares the underlying configuration of the current
     * resolver. The new resolver will chain requests to resolve namespaces through the specified
     * NamespaceResolver if resolution otherwise failed.</p>
     *
     * @param resolver the fallback resolver.
     * @return this
     */
    public CommonResourceResolver chain(NamespaceResolver resolver) {
        CommonResourceResolver copy = newInstance();
        copy.copyFrom(this);
        copy.nextNamespaceResolver = resolver;
        return copy;
    }
}
