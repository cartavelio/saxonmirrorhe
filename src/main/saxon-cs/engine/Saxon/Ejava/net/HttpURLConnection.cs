using System;
using System.IO;
using System.IO.Compression;

namespace Saxon.Ejava.net {
    public class HttpURLConnection : URLConnection {

        private readonly System.Net.Http.HttpResponseMessage httpResponse;

        public HttpURLConnection(URL url, System.Net.Http.HttpResponseMessage httpResponse) :
                base (url) {
            this.httpResponse = httpResponse;
            Stream raw = httpResponse.Content.ReadAsStream();
            if (httpResponse.Content.Headers.ContentEncoding.Contains("gzip")) {
                this.stream = new GZipStream(raw, CompressionMode.Decompress); 
            } else {
                this.stream = raw;
            }

        }

        public override string getContentType() {
            return httpResponse.Content.Headers.ContentType?.MediaType;
        }

        public override long getLastModified() {
            DateTimeOffset? offset = httpResponse.Content.Headers.LastModified;
            if (offset.HasValue) {
                return offset.Value.ToUnixTimeMilliseconds();
            }
            return -1L;
        }


    }
}