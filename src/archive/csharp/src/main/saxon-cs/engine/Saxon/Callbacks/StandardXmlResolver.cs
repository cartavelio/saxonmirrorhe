using System;
using System.IO;
using System.Net.Http;
using System.Xml;
using Saxon.Ejavax.xml.transform.stream;
using Saxon.Hej;
using Saxon.Hej.lib;
using Saxon.Impl.Overrides;

namespace Saxon.Callbacks {
    public class StandardXmlResolver : XmlUrlResolver {
        private Configuration config;

        public StandardXmlResolver(Configuration config) {
            this.config = config;
        }
        public override Uri ResolveUri(Uri baseUri, string relativeUri) {
            if (relativeUri != null && Saxon.Hej.lib.KnownResourceFinder.isKnownPublicId(relativeUri)) {
                // Microsoft designed this without really understanding public IDs...
                return new Uri("urn:public-id:" + relativeUri);
            }
            return base.ResolveUri(baseUri, relativeUri);
        }

        // Maps a URI to an Object containing the actual resource.
        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn) {
            if (absoluteUri.ToString().StartsWith("urn:public-id:")) {
                string publicId = absoluteUri.ToString().Substring(14);
                Saxon.Hej.lib.KnownResourceFinder.NamedInputStream nis = KnownResourceFinder.fetchPublicId(publicId, config);
                if (nis == null) {
                    return null;
                }
                else {
                    return nis.stream;
                }
            }
            if (absoluteUri.Scheme == "http" || absoluteUri.Scheme == "https") {
                try {
                    HttpResponseMessage response = ResourceLoader.readHttp(absoluteUri.ToString());
                    return response.Content.ReadAsStream();
                }
                catch (System.Net.Http.HttpRequestException e) {
                    throw new Saxon.Hej.trans.XPathException("Failed to retrieve HTTP resource " + absoluteUri, e);
                }
            }
            return base.GetEntity(absoluteUri, role, ofObjectToReturn);
        }
        
        
    }
}

// Copyright (c) 2021 Saxonica Limited