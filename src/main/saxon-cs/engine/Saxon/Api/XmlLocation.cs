using System;

namespace Saxon.Api {
    /// <summary>
    /// Xml location. An implementation of <c>IXmlLocation</c>.
    /// </summary>
    internal class XmlLocation : IXmlLocation {
        public Uri BaseUri { get; set; }
        public int LineNumber { get; set; }
    }
}