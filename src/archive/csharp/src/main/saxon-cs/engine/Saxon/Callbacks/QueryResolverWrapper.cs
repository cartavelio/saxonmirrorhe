using System;
using System.IO;
using System.Xml;
using Saxon.Ejavax.xml.transform.stream;
using Saxon.Api;

namespace Saxon.Callbacks {
    public class QueryResolverWrapper : Saxon.Hej.lib.ModuleURIResolver {
        private readonly XQueryResolver _xQueryResolver;
        private readonly XmlResolver xmlResolver;

        public QueryResolverWrapper(XQueryResolver xQueryResolver, XmlResolver xmlResolver) {
            this._xQueryResolver = xQueryResolver;
            this.xmlResolver = xmlResolver;
        }

        public Saxon.Ejavax.xml.transform.stream.StreamSource[] resolve(string moduleURI, string baseURI, string[] locations) {
            Uri[] locationUris = new Uri[locations.Length];
            for (int i=0; i<locations.Length; i++) {
                locationUris[i] = new Uri(locations[i], UriKind.RelativeOrAbsolute);
            }
            XQueryModuleSource[] modules = _xQueryResolver(moduleURI, baseURI==null ? null : new Uri(baseURI), locationUris);
            if (modules == null) {
                return null;
            }
            StreamSource[] sss = new StreamSource[modules.Length];
            for (int i = 0; i < modules.Length; i++) {
                Uri returnedBaseUri = modules[i].BaseUri;
                sss[i] = new StreamSource(new StringReader(modules[i].Content), returnedBaseUri == null ? null : returnedBaseUri.ToString());
            }

            return sss;
        }
    }

}

// Copyright (c) 2021 Saxonica Limited