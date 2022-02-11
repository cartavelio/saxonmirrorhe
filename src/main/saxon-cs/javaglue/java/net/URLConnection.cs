using System;

namespace java.net {
    public class URLConnection {

        public System.IO.Stream getInputStream() {
            throw new NotImplementedException();
        }

        public int getContentLength() {
            return 0;
        }

        public URL getURL() {
            return null;
        }

        public string getContentType() {
            throw new NotImplementedException();
        }

        public static string guessContentTypeFromName(string uri) {
            return "application/xml";
        }

        public static string guessContentTypeFromStream(System.IO.Stream stream) {
            return "application/xml";
        }

        public long getLastModified() {
            throw new NotImplementedException();
        }

    }
}