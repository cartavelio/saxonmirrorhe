using System;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;

namespace Saxon.Ejava.net {
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

        public static string guessContentTypeFromName(string uriStr) {
            Uri uri = new Uri(uriStr, UriKind.RelativeOrAbsolute);
            string path = uri.PathAndQuery;
            int qMark = path.IndexOf('?');
            if (qMark >= 0) {
                path = path.Substring(0, qMark);
            }

            // code from https://dotnetcoretutorials.com/2018/08/14/getting-a-mime-type-from-a-file-name-in-net-core/
            var provider = new FileExtensionContentTypeProvider();
            string contentType;
            if(!provider.TryGetContentType(path, out contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }

        public static string guessContentTypeFromStream(System.IO.Stream stream) {
            return "application/xml";
        }

        public virtual long getLastModified() {
            throw new NotImplementedException();
        }

    }
}