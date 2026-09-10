using Sys = System;
namespace Saxon.Impl.Helpers {
    public class UriUtils {
        public static bool IsOpaque(System.Uri uri) {
            // Javadoc: An opaque URI is an absolute URI whose scheme-specific part does not begin with a slash character ('/'). 
            return !uri.LocalPath.StartsWith("/");
        }
        public static System.Uri Normalize(System.Uri uri) {
            return uri;
        }

        public static string GetQuery(System.Uri uri) {
            string q = uri.Query;
            if (q != null && q.StartsWith("?")) {
                return q.Substring(1);
            }
            else {
                return q;
            }
        }

        public static System.Uri Build(string scheme,
                string userInfo, string host, int port,
                string path, string query, string fragment) {
            Sys.UriBuilder builder = new Sys.UriBuilder(scheme, host, port, path);
            builder.UserName = userInfo;
            builder.Query = query;
            builder.Fragment = fragment;
            return builder.Uri;
        }
        
        public static java.net.URL ToURL(System.Uri uri) {
            throw new(uri.ToString());
        }

        public static bool passesAdditionalChecks(Sys.Uri uri) {
            if (uri.IsWellFormedOriginalString()) {
                return true;
            }
            
            string original = uri.OriginalString;
            if (original.Contains("%")) {
                // If the original contained "%" but wasn't well-formed, don't give it the benefit of the doubt
                return false;
            }
            
            string escaped = System.Uri.EscapeUriString(original);
            if (uri.IsAbsoluteUri) {
                if (new System.Uri(escaped).IsWellFormedOriginalString()) {
                    return true;
                }
            }
            else {
                // The first path segment of a relative URI reference cannot contain a ":"
                int slash = original.IndexOf('/');
                if (slash < 0) {
                    slash = original.Length;
                }

                int colon = original.IndexOf(':');
                if (colon >= 0 && colon < slash) {
                    return false;
                }

                if (new System.Uri(new System.Uri("http://www.w3.org/"), escaped).IsWellFormedOriginalString()) {
                    return true;
                }
            }

            return false;
        }

    }
}