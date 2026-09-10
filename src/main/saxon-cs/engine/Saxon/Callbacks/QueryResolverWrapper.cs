using System;
using System.IO;
using System.Xml;
using javax.xml.transform.stream;
using Saxon.Api;

namespace Saxon.Callbacks {
    public class QueryResolverWrapper : net.sf.saxon.lib.ModuleURIResolver {
        private readonly IQueryResolver queryResolver;
        private readonly XmlResolver xmlResolver;

        public QueryResolverWrapper(IQueryResolver queryResolver, XmlResolver xmlResolver) {
            this.queryResolver = queryResolver;
            this.xmlResolver = xmlResolver;
        }

        public javax.xml.transform.stream.StreamSource[] resolve(string moduleURI, string baseURI, string[] locations) {
            Uri[] uris = queryResolver.GetModules(moduleURI, new Uri(baseURI), locations);
            StreamSource[] sss = new StreamSource[uris.Length];
            for (int i = 0; i < uris.Length; i++) {
                object o = queryResolver.GetEntity(uris[i])
                           ?? xmlResolver.GetEntity(uris[i], "query", typeof(Stream));
                sss[i] = o switch {
                    Stream stream =>
                        new StreamSource(stream, uris[i].ToString()),
                    string s =>
                        new StreamSource(new StringReader(s), uris[i].ToString()),
                    _ =>
                        new StreamSource(uris[i].ToString())
                };
            }

            return sss;
        }
    }

}