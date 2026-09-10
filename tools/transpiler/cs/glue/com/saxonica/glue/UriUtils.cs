namespace com.saxonica.glue {
    public class UriUtils {
        public static bool IsOpaque(System.Uri uri) {
            // Javadoc: An opaque URI is an absolute URI whose scheme-specific part does not begin with a slash character ('/'). 
            return !uri.LocalPath.StartsWith("/");
        }
        public static System.Uri Normalize(System.Uri uri) {
            // Javadoc: An opaque URI is an absolute URI whose scheme-specific part does not begin with a slash character ('/'). 
            return uri;
        }
    }
}