using System;
using System.IO;
using System.Net.Http;
using System.Xml;
using javax.xml.transform.stream;
using Saxon.Impl.Overrides;

namespace Saxon.Callbacks {
    public class StandardXmlResolver : XmlUrlResolver {
        // Maps a URI to an Object containing the actual resource.
        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn) {
            if (absoluteUri.Scheme == "http" || absoluteUri.Scheme == "https") {
                try {
                    HttpResponseMessage response = ResourceLoader.readHttp(absoluteUri.ToString());
                    return response.Content.ReadAsStream();
                }
                catch (System.Net.Http.HttpRequestException e) {
                    throw new net.sf.saxon.trans.XPathException("Failed to retrieve HTTP resource " + absoluteUri, e);
                }
            }
            return base.GetEntity(absoluteUri, role, ofObjectToReturn);
        }
        
        
    }
}