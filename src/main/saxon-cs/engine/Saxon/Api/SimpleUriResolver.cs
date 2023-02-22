using System;
using System.IO;
using System.Xml;

namespace Saxon.Api {
    
    /// <summary>
    /// A simple implementation of <c>XmlResolver</c> that supplies a delegate function accepting
    /// an absolute URI as its argument and returning a Stream.
    /// </summary>
    /// <remarks>
    /// Although this class implements <c>XmlResolver</c>, there is nothing specific to XML in its
    /// function; it can equally be used for resolving URIs for other media types.
    /// </remarks>
    /// <example>
    /// <code>
	/// XmlResolver resolver = new SimpleUriResolver(uri => new WebClient().OpenRead(uri))
	/// </code>
    /// </example>
    public class SimpleUriResolver : XmlResolver {

        private readonly System.Func<Uri, Stream> fetcher;
        public SimpleUriResolver(System.Func<Uri, Stream> fetcher) {
            this.fetcher = fetcher;
        } 
        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn) {
            return fetcher(absoluteUri);
        }
    }
}