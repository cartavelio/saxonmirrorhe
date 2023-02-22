package net.sf.saxon.lib;

import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;

import javax.xml.transform.Source;
import java.util.function.Function;

/**
 * Convenience class to allow a ResourceResolver to be implemented as a lambda expression.
 * Needed for C# transpilation, so that ResourceResolvers can be supplied both as classes
 * implementing an interface, and as lambda functions.
 */

public class ResourceResolverDelegate implements ResourceResolver {

    private final Function<ResourceRequest, Source> lambda;

    public ResourceResolverDelegate(Function<ResourceRequest, Source> lambda) {
        this.lambda = lambda;
    }

    @Override
    public Source resolve(ResourceRequest request) throws XPathException {
        try {
            return lambda.apply(request);
        } catch (UncheckedXPathException err) {
            throw err.getXPathException();
        }
    }
}


