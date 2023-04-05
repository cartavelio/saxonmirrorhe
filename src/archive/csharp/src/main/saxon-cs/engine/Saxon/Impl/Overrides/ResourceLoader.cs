using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Xml.XPath;
using Saxon.Ejava.io;
using Saxon.Ejava.net;
using Saxon.Hej.resource;
using IOException = System.IO.IOException;

namespace Saxon.Impl.Overrides {
    public class ResourceLoader {
        /**
         * The maximum number of redirects to follow before throwing an IOException.
         * If you allow the underlying Java URL class to follow redirects, it gives
         * up after 20 hops.
         */
        public static int MAX_REDIRECTS = 20;

        /**
         * Open a URLConnection to the resource identified by the URI. For HTTP URIs, this
         * method will follow up to MAX_REDIRECTS redirects or until it detects a loop;
         * the connection returned in this case is to the first resource that did not
         * return a 301 or 302 response code.
         *
         * @param url The URL to retrieve.
         * @return An InputStream for the resource content.
         * @throws IOException If more than MAX_REDIRECTS are occur or if a loop is detected.
         */
        public static URLConnection urlConnection(URL url) {
            if ("http" == url.getProtocol() || "https" == url.getProtocol()) {
                // TODO: handle redirects
                HttpResponseMessage response = readHttp(url.toURI().ToString());
                return new HttpURLConnection(url, response);
            }
            else if ("file" == url.getProtocol()) {
                Stream input = System.IO.File.OpenRead(url.getPath());
                return new URLConnection(url, input);
            }
            else {
                throw new System.IO.IOException("Cannot handle protocol " + url.getProtocol());
            }
        }

        private static readonly HttpClient _client = new HttpClient();

        public static HttpResponseMessage readHttp(string uri) {
            try {
                // See https://stackoverflow.com/questions/67696375/in-net-failure-to-retrieve-http-resource-from-w3c-web-site
                // Failure to set user-agent caused problems with XSLT test catalog-005
                var req = new HttpRequestMessage(HttpMethod.Get, uri);
                req.Headers.Add("user-agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 10_3 like Mac OS X)");
                var resp = _client.Send(req);
                resp.EnsureSuccessStatusCode();
                return resp;
            }
            catch (System.Net.Http.HttpRequestException e) {
                throw new System.IO.IOException("Failed to retrieve HTTP resource " + uri, e);
            }
        }

        /**
         * Open a stream to retrieve the content identified by the URI. For HTTP URIs, this
         * method will follow up to MAX_REDIRECTS redirects or until it detects a loop.
         * This method automatically accepts and decompresses gzip encoded responses.
         *
         * @param url The URL to retrieve.
         * @return An InputStream for the resource content.
         * @throws IOException If more than MAX_REDIRECTS are occur or if a loop is detected.
         */
        public static Stream urlStream(URL url) {
            try {
                URLConnection conn = urlConnection(url);
                return conn.getInputStream();
            }
            catch (Exception e) {
                throw new System.IO.IOException("Failed to open connection to " + url + ": " + e.Message);
            }
        }

        /**
         * Open a reader to retrieve the content identified by the URI. This handles HTTP
         * redirects in the same way as {@link #urlStream(URL)}, but then it also wraps
         * the stream in a Reader, using the logic prescribed for the {@code fn:unparsed-text}
         * function.
         *
         * @param url The URL to retrieve.
         * @param requestedEncoding The requested encoding. This is used only as a fallback, following
         *                 the rules of the {@code fn:unparsed-text} specification
         * @return A Reader for the resource content.
         * @throws IOException If more than MAX_REDIRECTS are occur or if a loop is detected.
         */
        public static TextReader urlReader(URL url, string requestedEncoding) {
            URLConnection conn = ResourceLoader.urlConnection(url);
            Stream inputStream = conn.getInputStream();
            string contentEncoding = conn.getContentEncoding();
            string resourceEncoding = null;
            if ("gzip" == contentEncoding) {
                inputStream = new GZipStream(inputStream, CompressionMode.Decompress);
            }

            // Get any external (HTTP) requestedEncoding label.
            bool isXmlMediaType = false;

            // The file:// URL scheme gives no useful information...
            if ("file" != url.getProtocol()) {
                // Use the contentType from the HTTP header if available, and parse it
                string contentType = conn.getContentType();
                if (contentType != null) {
                    ParsedContentType parsedContentType = new ParsedContentType(contentType);

                    isXmlMediaType = parsedContentType.isXmlMediaType;
                    resourceEncoding = parsedContentType.encoding;
                }
            }


            try {
                if (requestedEncoding == null) {
                    requestedEncoding = "UTF-8";
                }

                if (resourceEncoding == null || isXmlMediaType) {
                    resourceEncoding = EncodingDetector.inferStreamEncoding(inputStream, requestedEncoding, null);
                }
            }
            catch (IOException e) {
                resourceEncoding = "UTF-8";
            }

            return getReaderFromStream(inputStream, resourceEncoding);
        }
        
        public static TextReader getReaderFromStream(Stream input, string encoding) {
            return new StreamReader(input, System.Text.Encoding.GetEncoding(encoding));
        }
    }
    
    
}

// Copyright (c) 2021 - 2023 Saxonica Limited