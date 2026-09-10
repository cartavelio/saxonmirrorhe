using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Org.XmlResolver;
using Org.XmlResolver.Features;
using Org.XmlResolver.Utils;
using Saxon.Api;
using Saxon.Ejavax.xml.transform;
using Saxon.Ejavax.xml.transform.stream;
using Saxon.Hej.resource;

namespace Saxon.Callbacks {
    /// <summary>
    /// This implementation of <c>CommonResourceResolver</c> attempts direct resolution of requested
    /// URIs using standard .NET mechanisms. It uses the services of the <c>UriUtils</c> class in the
    /// <c>Org.XmlResolver</c> package, which should be able to handle standard file URIs, http and https
    /// URIs, as well as more specialized schemes such as <c>data</c> and <c>pack</c>. 
    /// </summary>
    /// <remarks>
    /// <para>This resolver will normally be used after other mechanisms have been exhausted; it will often
    /// appear at the end of a chain of resolvers.
    /// </para>
    /// </remarks>
    public class DirectResourceResolver : Saxon.Hej.lib.JJ_ResourceResolver {

        public DirectResourceResolver(Saxon.Hej.Configuration config) {

        }


        /// <summary>
        /// Get a stream, and associated base URI, for a resource identified by a URI, supplied typically
        /// as the combination of a relative URI amd base URI
        /// </summary>
        /// <param name="request">Details of the required resource, including its URI</param>
        /// <returns>An <c>IResource</c> containing the content of the resource as a <c>Stream</c> or
        /// <c>TextReader</c>, together with a <c>SystemId</c> that identifies the base URI of the
        /// resource. Returns null if the request cannot be resolved.</returns>
        public Source resolve(Saxon.Hej.lib.JJ_ResourceRequest request) {
            if (request.uriIsNamespace) {
                return null;
            }
            try {
                string href = request.relativeUri ?? request.uri;
                string baseUri = request.baseUri;
                if (string.IsNullOrEmpty(baseUri)) {
                    baseUri = Saxon.Ejava.io.File.CurrentDirectoryUri().AbsoluteUri;
                }
                Uri abs = new Uri(new Uri(baseUri), href);
                if (abs.Scheme == "http" || abs.Scheme == "https") {
                    using WebClient webClient = new WebClient();
                    byte[] octets = webClient.DownloadData(abs);

                    // Get response header.
                    string contentType = webClient.ResponseHeaders["Content-Type"];
                    TypedStreamSource tss = new();
                    tss.setContentType(contentType);
                    tss.setInputStream(new MemoryStream(octets));
                    tss.setSystemId(abs.ToString());
                    return tss;
                }
                Stream stream = UriUtils.GetStream(abs);
                return new StreamSource(stream, abs.ToString());
            }

            catch (WebException) {
                // Shouldn't be necessary, but the 0.4.0 resolver code is throwing this for a non-existent URI
                return new StreamSource(request.uri);
            }
            catch (System.ArgumentException) {
                // Thrown for an unknown URI scheme
                return new StreamSource(request.uri);
            }
            catch (System.IO.FileNotFoundException) {
                // Shouldn't be necessary, but the 0.4.0 resolver code is throwing this for a non-existent file
                return new StreamSource(request.uri);
            }
            catch (System.IO.DirectoryNotFoundException) {
                // Shouldn't be necessary, but the 0.4.0 resolver code is throwing this for a non-existent file
                return new StreamSource(request.uri);
            }

            //return null;
        }

    }
}