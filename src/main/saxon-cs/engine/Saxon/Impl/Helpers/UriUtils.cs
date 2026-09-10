using Sys = System;
namespace Saxon.Impl.Helpers {
    public class UriUtils {
        public static bool IsOpaque(System.Uri uri) {
            // Javadoc: An opaque URI is an absolute URI whose scheme-specific part does not begin with a slash character ('/'). 
            return !uri.LocalPath.StartsWith("/");
        }
        public static System.Uri Normalize(System.Uri uri) {
            // Javadoc: An opaque URI is an absolute URI whose scheme-specific part does not begin with a slash character ('/'). 
            return uri;
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

    }
}