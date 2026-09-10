using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class ProtocolRestricter {
        public static System.Predicate<System.Uri> make(string value) {
            Saxon.Ejava.util.Objects.requireNonNull(value);
            value = value.Trim();
            if (value.Equals("all")) {
                return uri => true;
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
                return uri => {
                    foreach (System.Predicate<System.Uri> pred in permitted) {
                        if (pred(uri)) {
                            return true;
                        }
                    }
                    return false;
                } ;
            }
        }
        private static string scheme(System.Uri uri) {
            return uri.Scheme;
        }
        private static string schemeSpecificPart(System.Uri uri) {
            return uri.GetComponents(System.UriComponents.PathAndQuery, System.UriFormat.Unescaped);
        }
        public static void main(string[] args) {
            System.Uri uri = new System.Uri(args[0], System.UriKind.RelativeOrAbsolute);
            System.Console.Error.WriteLine(((uri.Scheme+" + ")+uri.GetComponents(System.UriComponents.PathAndQuery, System.UriFormat.Unescaped)));
        }
    
    }
}