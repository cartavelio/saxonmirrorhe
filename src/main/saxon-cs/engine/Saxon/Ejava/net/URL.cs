using System.Net;
using System.Text;
using System.Web;
using SysUri = System.Uri;

namespace Saxon.Ejava.net {
    /// <summary>
    /// A partial emulation of the java.net.URL class, implemented as a wrapper around a .NET System.Uri
    /// </summary>
    public class URL {
        private readonly SysUri uri;
        private const int FollowRedirectLimit = 64;

        public URL(string str) {
            uri = new SysUri(str);
        }

        public URL(SysUri uri) {
            this.uri = uri;
        }

        public URL(URL baseURL, string href) {
            uri = new SysUri(baseURL.uri, href);
        }

        public string getPath() {
            return uri.AbsolutePath;
        }

        public string getProtocol() {
            return uri.Scheme;
        }

        public System.Uri toURI() {
            return uri;
        }

        public System.IO.Stream openStream() {
            // This is heavily based on code borrowed from xmlresolvercs/XmlResolver/ResourceAccess.cs 
            switch (uri.Scheme) {
                case "data":
                    return getDataResource(uri);
                case "file":
                    return getFileResource(uri);
                case "http":
                case "https":
                    return getHttpResource(uri);
                default:
                    throw new IOException("Unsupported URI scheme in: " + uri);
            }
        }

        private static Stream getDataResource(Uri resourceUri) {
            string uri = resourceUri.ToString();

            // Can't use URI accessors because they percent decode the string incorrectly.
            var path = uri[5..];
            var pos = path.IndexOf(",", StringComparison.Ordinal);

            if (pos < 0) {
                throw new UriFormatException(uri + ": comma separator missing");
            }

            var mediaType = path[..pos];
            String data = path[(pos + 1)..];
            if (mediaType.EndsWith(";base64")) {
                // Base64 decode it
                var base64Bytes = Convert.FromBase64String(data);
                return new MemoryStream(base64Bytes);
            }
            else {
                // URL decode it
                string charset = "UTF-8";
                pos = mediaType.IndexOf(";charset=", StringComparison.Ordinal);
                if (pos > 0) {
                    charset = mediaType[(pos + 9)..];
                    pos = charset.IndexOf(";", StringComparison.Ordinal);
                    if (pos >= 0) {
                        charset = charset[..pos];
                    }
                }

                data = HttpUtility.UrlDecode(data, Encoding.GetEncoding(charset));
                return new MemoryStream(Encoding.UTF8.GetBytes(data));
            }
        }

        private static Stream getFileResource(Uri resourceUri) {
            string filename = new Saxon.Ejava.io.File(resourceUri).getPath();
            return File.OpenRead(filename);
        }

        private static HttpClient _httpClient = new HttpClient();

        private static Stream getHttpResource(Uri resourceUri) {
            var seen = new HashSet<Uri>();
            var count = FollowRedirectLimit;
            var resolvedUri = resourceUri;
            HttpStatusCode status = HttpStatusCode.OK;
            bool done = false;

            while (!done) {
                count--;
                done = count <= 0;

                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, resolvedUri);
                HttpResponseMessage resp = _httpClient.Send(req);

                status = resp.StatusCode;
                if (seen.Contains(resolvedUri)) {
                    throw new HttpRequestException("Redirect loop", null, status);
                }

                seen.Add(resolvedUri);

                if (resp.StatusCode == HttpStatusCode.OK) {
                    // var headers = new Dictionary<string, List<string>>();
                    // foreach (var header in resp.Headers) {
                    //     List<string> value = new List<string>();
                    //     foreach (var hvalue in header.Value) {
                    //         value.Add(hvalue);
                    //     }
                    //
                    //     headers.Add(header.Key, value);
                    // }
                    //

                   return resp.Content.ReadAsStream();
                   
                }

                if (resp.StatusCode is HttpStatusCode.Moved or HttpStatusCode.Redirect) {
                    resolvedUri = resp.Content.Headers.ContentLocation ??
                                  throw new HttpRequestException("Redirect without location", null, status);
                }
                else {
                    done = true;
                }
            }

            if (count <= 0) {
                throw new HttpRequestException("Too many redirects", null, status);
            }

            throw new HttpRequestException("Failed to read resource", null, status);
        }
    }
}