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
 * It is possible to specify a new, common resolver by providing a concrete extension of the
 * <code>CommonResourceResolver</code> class. This does not replace the existing APIs which allow
 * an application to specify individual resolvers in various places. Those resolvers will take precedence
 * over the common resolver.
 *
 * @since 11
 */

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
