using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class StandardURIChecker : Saxon.Hej.lib.URIChecker {
        private static readonly Saxon.Hej.lib.StandardURIChecker THE_INSTANCE = new Saxon.Hej.lib.StandardURIChecker();
        public static Saxon.Hej.lib.StandardURIChecker getInstance() {
            return THE_INSTANCE;
        }
        private static readonly System.Threading.ThreadLocal<Saxon.Hej.expr.sort.LFUCache<string,System.Uri>> caches = new System.Threading.ThreadLocal<Saxon.Hej.expr.sort.LFUCache<string, System.Uri>>();
        protected internal StandardURIChecker()  {
        }
        public bool isValidURI(string value) {
            Saxon.Hej.expr.sort.LFUCache<string,System.Uri> cache = caches.Value;
            if ((cache == null)) {
                cache = new Saxon.Hej.expr.sort.LFUCache<string,System.Uri>(50);
                caches.Value = cache;
            }
            if (cache.containsKey(value)) {
                return true;
            }
            string sv = Saxon.Hej.value.Whitespace.trim(value);
            if (sv.Length==0) {
                return true;
            }
            try {
                System.Uri uri = new System.Uri(sv, System.UriKind.RelativeOrAbsolute);
                if (passesAdditionalChecks(uri)) {
                    cache.put(value, uri);
                    return true;
                }
            } catch (System.UriFormatException e) {
            }
            sv = Saxon.Hej.functions.IriToUri.iriToUri(Saxon.Hej.str.StringView.tidy(sv)).ToString();
            try {
                System.Uri uri = new System.Uri(sv, System.UriKind.RelativeOrAbsolute);
                if (passesAdditionalChecks(uri)) {
                    cache.put(value, uri);
                    return true;
                }
            } catch (System.UriFormatException e) {
                return false;
            }
            return false;
        }
        public bool passesAdditionalChecks(System.Uri uri) {
            return Saxon.Impl.Helpers.UriUtils.passesAdditionalChecks(uri);
        }
        public void checkThoroughly(System.Uri uri) {
            if (!(passesAdditionalChecks(uri))) {
                throw new System.UriFormatException(uri.ToString() + " : " + "Fails detailed checking");
            }
        }
        public static void main(string[] args) {
            System.Console.Error.WriteLine(((args[0]+" is valid? - ")+getInstance().isValidURI(args[0])));
        }
    
    }
}