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
                        permitted.Add(uri => scheme(uri).Equals(token));
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
        private static string scheme(System.Uri uri) {
            return uri.Scheme;
        }
        private static string schemeSpecificPart(System.Uri uri) {
            return uri.GetComponents(System.UriComponents.PathAndQuery, System.UriFormat.Unescaped);
        }
    
    }
}