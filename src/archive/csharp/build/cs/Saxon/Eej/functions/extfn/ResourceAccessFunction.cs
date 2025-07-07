using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    abstract public class ResourceAccessFunction : Saxon.Hej.functions.SystemFunction {
        protected internal System.IO.Stream resolveResourceAccess(Saxon.Hej.expr.XPathContext context, string uri) {
            Saxon.Hej.lib.JJ_ResourceResolver resolver = context.getConfiguration().getResourceResolver();
            Saxon.Callbacks.DirectResourceResolver direct = new Saxon.Callbacks.DirectResourceResolver(context.getConfiguration());
            Saxon.Hej.lib.JJ_ResourceRequest request = new Saxon.Hej.lib.JJ_ResourceRequest();
            request.uri = uri;
            request.baseUri = getRetainedStaticContext().getStaticBaseUriString();
            request.nature = Saxon.Hej.lib.JJ_ResourceRequest.BINARY_NATURE;
            Saxon.Ejavax.xml.transform.Source source = request.resolve(resolver, direct);
            if ((source is Saxon.Hej.lib.AugmentedSource)) {
                source = ((Saxon.Hej.lib.AugmentedSource)(source)).getContainedSource();
            }
            if ((source is Saxon.Ejavax.xml.transform.stream.StreamSource)) {
                Saxon.Ejavax.xml.transform.stream.StreamSource ssource = (Saxon.Ejavax.xml.transform.stream.StreamSource)(source);
                if (((ssource.getInputStream() == null) && (ssource.getSystemId() != null))) {
                    Saxon.Ejava.net.URL url;
                    if ((request.baseUri != null)) {
                        url = Saxon.Impl.Helpers.UriUtils.ToURL(new System.Uri(new System.Uri(request.baseUri, System.UriKind.RelativeOrAbsolute), ssource.getSystemId()));
                    } else {
                        url = Saxon.Impl.Helpers.UriUtils.ToURL(new System.Uri(ssource.getSystemId(), System.UriKind.RelativeOrAbsolute));
                    }
                    System.IO.Stream stream = Saxon.Impl.Overrides.ResourceLoader.urlStream(url);
                    ssource.setInputStream(stream);
                }
                return ((Saxon.Ejavax.xml.transform.stream.StreamSource)(source)).getInputStream();
            }
            return null;
        }
    
    }
}