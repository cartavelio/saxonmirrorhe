using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.resource {
    public class UnknownResource : Saxon.Hej.lib.Resource {
        private readonly Saxon.Hej.expr.XPathContext context;
        private readonly Saxon.Hej.resource.AbstractResourceCollection.InputDetails details;
        public static readonly Saxon.Hej.lib.ResourceFactory FACTORY = /* ctor#2 */ (a, b) => new Saxon.Hej.resource.UnknownResource(a, b);
        public UnknownResource(Saxon.Hej.expr.XPathContext context, Saxon.Hej.resource.AbstractResourceCollection.InputDetails details)  {
            this.context = context;
            this.details = details;
        }
        public string getResourceURI() {
            return details.resourceUri;
        }
        public Saxon.Hej.om.Item getItem() {
            System.IO.Stream stream;
            if ((details.binaryContent != null)) {
                stream = new System.IO.MemoryStream(details.binaryContent);
            } else {
                try {
                    stream = details.getInputStream();
                } catch (System.IO.IOException e) {
                    if ((details.onError == Saxon.Hej.functions.URIQueryParameters.ON_ERROR_FAIL)) {
                        throw new Saxon.Hej.trans.XPathException(e);
                    } else {
                        return null;
                    }
                }
            }
            if ((stream == null)) {
                throw new Saxon.Hej.trans.XPathException(("Unable to dereference resource URI "+details.resourceUri));
            }
            string mediaType;
            try {
                stream = Saxon.Hej.query.InputStreamMarker.ensureMarkSupported(stream);
                mediaType = Saxon.Ejava.net.URLConnection.guessContentTypeFromStream(stream);
            } catch (System.IO.IOException e) {
                mediaType = null;
            }
            if ((mediaType == null)) {
                mediaType = context.getConfiguration().getMediaTypeForFileExtension("");
            }
            if (((mediaType == null) || mediaType.Equals("application/unknown"))) {
                mediaType = "application/binary";
            }
            details.contentType = mediaType;
            details.binaryContent = Saxon.Hej.resource.BinaryResource.readBinaryFromStream(stream, details.resourceUri);
            Saxon.Hej.lib.ResourceFactory delegee = context.getConfiguration().getResourceFactoryForMediaType(mediaType);
            Saxon.Hej.lib.Resource actual = /*Delegate.makeResource()*/delegee(context, details);
            return actual.getItem();
        }
        public string getContentType() {
            return "application/xml";
        }
    
    }
}