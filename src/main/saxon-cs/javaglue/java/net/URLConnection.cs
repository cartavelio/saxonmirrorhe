using System;
using System.IO;

namespace java.net {
    public class URLConnection {

        private readonly URL url;
        protected Stream stream;
        
        public URLConnection(URL url) {
            this.url = url;
        }

        public URLConnection(URL url, Stream stream) {
            this.url = url;
            this.stream = stream;
        }

        public System.IO.Stream getInputStream() {
            return stream;
        }

        public long getContentLengthLong() {
            return stream.Length;
        }

        public URL getURL() {
            return url;
        }

        public virtual string getContentType() {
            throw new NotImplementedException();
        }

        public virtual string getContentEncoding() {
            throw new NotImplementedException();
        }

        public static string guessContentTypeFromName(string uri) {
            return "application/xml";
        }

        public static string guessContentTypeFromStream(System.IO.Stream stream) {
            return "application/xml";
        }

        public virtual long getLastModified() {
            throw new NotImplementedException();
        }

    }
}