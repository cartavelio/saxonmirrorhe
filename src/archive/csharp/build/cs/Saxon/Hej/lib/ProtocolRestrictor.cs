using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class ProtocolRestrictor {
        private readonly System.Predicate<System.Uri> predicate;
        private readonly string originalRule;
        public ProtocolRestrictor(string value)  {
            Saxon.Ejava.util.Objects.requireNonNull(value);
            this.originalRule = value;
            value = value.Trim();
            if (value.Equals("all")) {
                predicate = uri => true;
            } else {
                System.Collections.Generic.IList<System.Predicate<System.Uri>> permitted = new System.Collections.Generic.List<System.Predicate<System.Uri>>();
                string[] tokens = System.Text.RegularExpressions.Regex.Split(value, ",\\s*");
                foreach (string token in tokens) {
                    if ((token.StartsWith("jar:") && (token.Length>4))) {
                        string subScheme = token.Substring(4).ToLower();
                        permitted.Add(uri => (scheme(uri).Equals("jar") && schemeSpecificPart(uri).ToLower().StartsWith(subScheme)));
                    } else {
                        permitted.Add(uri => token.Equals(scheme(uri)));
                    }
                }
                predicate = uri => {
                    foreach (System.Predicate<System.Uri> pred in permitted) {
                        if (pred(uri)) {
                            return true;
                        }
                    }
                    return false;
                } ;
            }
        }
        public bool test(System.Uri uri) {
            return predicate(uri);
        }
        public override string ToString() {
            return originalRule;
        }
        public Saxon.Hej.lib.JJ_ResourceResolver asResourceResolver(Saxon.Hej.lib.JJ_ResourceResolver existing) {
            return new Saxon.Hej.lib.ProtocolRestrictor.RestrictedResourceResolver(this, existing);
        }
        private static string scheme(System.Uri uri) {
            return uri.Scheme;
        }
        private static string schemeSpecificPart(System.Uri uri) {
            return uri.GetComponents(System.UriComponents.PathAndQuery, System.UriFormat.Unescaped);
        }
        public class RestrictedResourceResolver : Saxon.Hej.lib.JJ_ResourceResolver {
            private readonly Saxon.Hej.lib.ProtocolRestrictor protocolRestrictor;
            private readonly Saxon.Hej.lib.JJ_ResourceResolver nextResolver;
            public RestrictedResourceResolver(Saxon.Hej.lib.ProtocolRestrictor pr, Saxon.Hej.lib.JJ_ResourceResolver rr)  {
                this.protocolRestrictor = pr;
                this.nextResolver = rr;
            }
            public void setAllowedProtocols(string protocols) {
                if ((nextResolver is Saxon.Callbacks.CatalogResourceResolver)) {
                    Saxon.Callbacks.CatalogResourceResolver catres = (Saxon.Callbacks.CatalogResourceResolver)(nextResolver);
                    catres.setAllowedProtocols(protocols);
                }
            }
            public Saxon.Ejavax.xml.transform.Source resolve(Saxon.Hej.lib.JJ_ResourceRequest request) {
                if (protocolRestrictor.test(new System.Uri(request.uri, System.UriKind.RelativeOrAbsolute))) {
                    return nextResolver.resolve(request);
                } else {
                    throw new Saxon.Hej.trans.XPathException((("Access to URI "+request.uri)+" has been prohibited"));
                }
            }
        
        }
    
    }
}